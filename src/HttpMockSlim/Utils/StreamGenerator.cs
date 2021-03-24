using System;

namespace HttpMockSlim.Utils
{
    /// <summary>
    /// Generates a one-time readable byte-stream mock of specified size
    /// </summary>
    public class StreamGenerator : UnbufferedReadOnlyStreamBase
    {
        private readonly long _size;
        private readonly byte _fillValue;

        private long _position = 0;

        /// <summary>
        /// Create stream
        /// </summary>
        /// <param name="size">Size of data in bytes that stream will allow to read before reporting end of stream</param>
        /// <param name="fillValue">value to fill</param>
        public StreamGenerator(long size, byte fillValue)
        {
            _size = size;
            _fillValue = fillValue;
        }

        /// <summary>
        /// Create stream
        /// </summary>
        /// <param name="size">Length of stream</param>
        /// <param name="fillValue">value to fill</param>
        public StreamGenerator(long size, char fillValue) : this(size, Convert.ToByte(fillValue)) { }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_position >= _size)
                return 0;
            if ((int)_position + count > _size)
                count = (int)(_size - _position);
            _position += count;

            System.Runtime.CompilerServices.Unsafe.InitBlock(ref buffer[offset], _fillValue, Convert.ToUInt32(count));

            return count;
        }
    }
}