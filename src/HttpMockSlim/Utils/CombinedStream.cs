using System;
using System.Collections.Generic;
using System.IO;

namespace HttpMockSlim.Utils
{
    // Took idea from https://stackoverflow.com/questions/3879152/how-do-i-concatenate-two-system-io-stream-instances-into-one
    // Made my version of it.
    public class CombinedStream : Stream
    {
        private readonly IEnumerator<Stream> _enumerator;

        private bool _moveNextResult;


        /// <summary>
        /// Create new instance of CombinedStream
        /// </summary>
        /// <param name="streams">Streams to be read from. Enumerable most not contain any nulls. (Enumerable will be iterated only as needed, all passed streams (even if not read) will be closed upon dispose)</param>
        public CombinedStream(IEnumerable<Stream> streams)
        {
            //this._streams = new Queue<Stream>(streams);
            _enumerator = streams.GetEnumerator();
            _moveNextResult = _enumerator.MoveNext();
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
                        _moveNextResult = _enumerator.MoveNext();
                    }
                } while (bytesRead == 0 && _moveNextResult);
            }

            return bytesRead;
        }


        protected override void Dispose(bool disposing)
        {
            if (_moveNextResult)
            {
                _enumerator.Current?.Close();

                while (_enumerator.MoveNext())
                    _enumerator.Current?.Close();
            }

            _enumerator.Dispose();
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
    }
}