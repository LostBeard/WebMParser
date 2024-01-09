namespace SpawnDev.WebMParser
{
    public static class StreamExtensions
    {
        /// <summary>
        /// Returns the maximum number of bytes that can be read starting from the given offset<br />
        /// If the offset &gt;= length or offset &lt; 0 or !CanRead, 0 is returned
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static long MaxReadableCount(this Stream _this, long offset)
        {
            if (!_this.CanRead || offset < 0 || offset >= _this.Length || _this.Length == 0) return 0;
            return _this.Length - offset;
        }
        public static long MaxReadableCount(this Stream _this) => _this.MaxReadableCount(_this.Position);

        public static long GetReadableCount(this Stream _this, long maxCount)
        {
            return _this.GetReadableCount(_this.Position, maxCount);
        }
        public static long GetReadableCount(this Stream _this, long offset, long maxCount)
        {
            if (maxCount <= 0) return 0;
            var bytesLeft = _this.MaxReadableCount(offset);
            return Math.Max(bytesLeft, maxCount);
        }
        public static byte[] ReadBytes(this Stream _this)
        {
            var readCount = _this.MaxReadableCount();
            var bytes = new byte[readCount];
            _this.Read(bytes, 0, bytes.Length);
            return bytes;
        }
        public static byte[] ReadBytes(this Stream _this, long count, bool requireCountExact = false)
        {
            var readCount = _this.GetReadableCount(count);
            if (readCount != count && requireCountExact) throw new Exception("Not available");
            var bytes = new byte[readCount];
            if (readCount == 0) return bytes;
            _this.Read(bytes, 0, bytes.Length);
            return bytes;
        }
        public static byte[] ReadBytes(this Stream _this, long offset, long count, bool requireCountExact = false)
        {
            var origPosition = _this.Position;
            _this.Position = offset;
            try
            {
                var readCount = _this.GetReadableCount(offset, count);
                if (readCount != count && requireCountExact) throw new Exception("Not available");
                var bytes = new byte[readCount];
                if (readCount == 0) return bytes;
                _this.Read(bytes, 0, bytes.Length);
                return bytes;
            }
            finally
            {
                _this.Position = origPosition;
            }
        }
        public static int ReadByte(this Stream _this, long offset)
        {
            var origPosition = _this.Position;
            _this.Position = offset;
            try
            {
                var ret = _this.ReadByte();
                return ret;
            }
            finally
            {
                _this.Position = origPosition;
            }
        }
        public static int ReadByteOrThrow(this Stream _this, long offset)
        {
            var origPosition = _this.Position;
            _this.Position = offset;
            try
            {
                var ret = _this.ReadByte();
                if (ret == -1) throw new EndOfStreamException();
                return ret;
            }
            finally
            {
                _this.Position = origPosition;
            }
        }
        public static int ReadByteOrThrow(this Stream _this)
        {
            var ret = _this.ReadByte();
            if (ret == -1) throw new EndOfStreamException();
            return ret;
        }
    }
}
