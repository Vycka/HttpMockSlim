﻿using System;
using System.Collections.Generic;
using System.IO;

namespace HttpMockSlim.Utils
{
    // Took idea from https://stackoverflow.com/questions/3879152/how-do-i-concatenate-two-system-io-stream-instances-into-one
    // Made my version of it.
    public class CombinedStream : UnbufferedReadOnlyStreamBase, IDisposablesBag
    {
        private readonly Stack<IDisposable> _disposables;

        private readonly bool _disposeStreams;
        private readonly IEnumerator<Stream> _enumerator;
        private bool _moveNextResult;

        /// <summary>
        /// Create new instance of CombinedStream
        /// </summary>
        /// <param name="streams">Streams to be read from. Enumerable most not contain any nulls. (Enumerable will be iterated only as needed)</param>
        /// <param name="disposeStreams">Dispose passed streams</param>
        public CombinedStream(IEnumerable<Stream> streams, bool disposeStreams = true)
        {
            _disposeStreams = disposeStreams;
            _enumerator = streams?.GetEnumerator() ?? throw new ArgumentNullException(nameof(streams));

            _moveNextResult = _enumerator.MoveNext();
            _disposables = new Stack<IDisposable>();
        }

        /// <summary>
        /// Create new instance of CombinedStream
        /// </summary>
        /// <param name="streams">Streams to be read from. Array most not contain any null values. All streams in will be disposed.</param>
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
                            _enumerator.Current.Dispose();
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
                        _enumerator.Current?.Dispose();
                    }
                } while (_enumerator.MoveNext());
            }

            _enumerator.Dispose();

            while (_disposables.Count != 0)
            {
                _disposables.Pop()?.Dispose();
            }
        }

        public void AddDisposable(IDisposable disposable)
        {
            _disposables.Push(disposable);
        }
    }
}