namespace SpawnDev.WebMParser
{
    public static class StreamExtensions
    {
        public static byte[] ReadBytes(this Stream _this)
        {
            var bytesLeft = Math.Max(_this.Length - _this.Position, 0);
            var bytes = new byte[bytesLeft];
            _this.Read(bytes, 0, bytes.Length);
            return bytes;
        }
        public static byte[] ReadBytes(this Stream _this, long count)
        {
            long bytesLeft = Math.Max(_this.Length - _this.Position, 0);
            if (count < 0) count = 0;
            count = Math.Min(count, bytesLeft);
            var bytes = new byte[count];
            if (count == 0) return bytes;
            _this.Read(bytes, 0, bytes.Length);
            return bytes;
        }
        public static byte[] ReadBytes(this Stream _this, long offset, long count)
        {
            _this.Position = offset;
            long bytesLeft = Math.Max(_this.Length - _this.Position, 0);
            if (count < 0) count = 0;
            count = Math.Min(count, bytesLeft);
            var bytes = new byte[count];
            if (count == 0) return bytes;
            _this.Read(bytes, 0, bytes.Length);
            return bytes;
        }
        public static byte ReadByte(this Stream _this, long offset)
        {
            _this.Position = offset;
            return (byte)_this.ReadByte();
        }
    }
}
