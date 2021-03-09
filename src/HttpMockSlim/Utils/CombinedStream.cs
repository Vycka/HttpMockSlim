using System;
using System.Collections.Generic;
using System.IO;

namespace HttpMockSlim.Utils
{
    
    // Took idea from https://stackoverflow.com/questions/3879152/how-do-i-concatenate-two-system-io-stream-instances-into-one
    // Made my version of it.
    public class CombinedStream : Stream, IDisposablesBag
    {
        private readonly IList<IDisposable> _disposables;

        private readonly bool _disposeStreams;
        private readonly IEnumerator<Stream> _enumerator;
        private bool _moveNextResult;

        /// <summary>
        /// Create new instance of CombinedStream
        /// </summary>
        /// <param name="streams">Streams to be read from. Enumerable most not contain any nulls. (Enumerable will be iterated only as needed)</param>
        /// <param name="disposeStreams">Dispose passed streams once CombinedStream receives dispose call</param>
        public CombinedStream(IEnumerable<Stream> streams, bool disposeStreams)
        {
            _disposeStreams = disposeStreams;
            _enumerator = streams?.GetEnumerator() ?? throw new ArgumentNullException(nameof(streams));

            _moveNextResult = _enumerator.MoveNext();
            _disposables = new List<IDisposable>();
        }


        /// <summary>
        /// Create new instance of CombinedStream
        /// </summary>
        /// <param name="streams">Streams to be read from. Array most not contain any null values. All streams in will be disposed once CombinedStream receives Dispose call</param>
        public CombinedStream(params Stream[] streams)
            : this(streams, true)
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = 0;

            if (_moveNextResult)
            {
                do
                {
                    // ReSharper disable once PossibleNullReferenceException
                    // * We expect here a clean enumerable to be passed in c-tor
                    bytesRead = _enumerator.Current.Read(buffer, offset, count);

                    if (bytesRead == 0)
                    {
                        _enumerator.Current.Close();

                        if (_disposeStreams)
                        {
                            AddDisposable(_enumerator.Current);
                        }

                        _moveNextResult = _enumerator.MoveNext();
                    }
                } while (bytesRead == 0 && _moveNextResult);
            }

            return bytesRead;
        }


        // Called from Stream's base Dispose
        protected override void Dispose(bool disposing)
        {
            if (_moveNextResult)
            {
                do
                {
                    _enumerator.Current?.Close();

                    if (_disposeStreams)
                    {
                        AddDisposable(_enumerator.Current);
                    }
                } while (_enumerator.MoveNext());
            }

            _enumerator.Dispose();

            foreach (IDisposable disposable in _disposables)
            {
                disposable.Dispose();
            }
        }

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }


        public void AddDisposable(IDisposable disposable)
        {
            _disposables.Add(disposable);
        }
    }
}