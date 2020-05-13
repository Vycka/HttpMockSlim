using System;
using System.IO;

namespace HttpMockSlim.Utils
{
    /// <summary>
    /// Generates a one-time readible byte-stream mock of specified size
    /// </summary>
    public class StreamGenerator : Stream
    {
        private readonly long _maxSize;
        private readonly byte _fillValue;

        /// <summary>
        /// Create stream
        /// </summary>
        /// <param name="maxSize">Length of stream</param>
        /// <param name="fillValue">value to fill</param>
        public StreamGenerator(long maxSize, byte fillValue)
        {
            _maxSize = maxSize;
            _fillValue = fillValue;
        }

        /// <summary>
        /// Create stream
        /// </summary>
        /// <param name="maxSize">Length of stream</param>
        /// <param name="fillValue">value to fill</param>
        public StreamGenerator(long maxSize, char fillValue) : this(maxSize, (byte)fillValue) { }
        public override void Flush()
        {
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (Position >= _maxSize)
                return 0;
            if ((int)Position + count > _maxSize)
                count = (int)(_maxSize - Position);
            Position += count;

            System.Runtime.CompilerServices.Unsafe.InitBlock(ref buffer[offset], _fillValue, Convert.ToUInt32(count));

            return count;
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;

        public override long Length
        {
            get { throw new NotSupportedException("Length explicitly not given for streaming"); }
        }

        public override long Position { get; set; }
    }
}