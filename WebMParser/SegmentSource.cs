﻿namespace SpawnDev.WebMParser
{
    public abstract class SegmentSource : Stream
    {
        /// <summary>
        /// The underlying source of the segment
        /// </summary>
        public virtual object SourceObject { get; protected set; }
        /// <summary>
        /// Segment start position in Source
        /// </summary>
        public virtual long Offset { get; private set; }
        /// <summary>
        /// Whether this SegmentSource owns the underlying source object
        /// </summary>
        public virtual bool OwnsSource { get; private set; }
        /// <summary>
        /// Segment size in bytes.
        /// </summary>
        protected virtual long Size { get; set; }
        protected virtual long SourcePosition { get; set; }
        // Stream
        public override long Length => Size;
        public override bool CanRead => SourceObject != null;
        public override bool CanSeek => SourceObject != null;
        public override bool CanWrite => false;
        public override bool CanTimeout => false;
        public override long Position { get => SourcePosition - Offset; set => SourcePosition = value + Offset; }
        public override void Flush() { }
        public override void SetLength(long value) => throw new NotImplementedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();
        public SegmentSource(long offset, long size, bool ownsSource)
        {
            Offset = offset;
            Size = size;
            OwnsSource = ownsSource;
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.End:
                    Position = Length + offset;
                    break;
                case SeekOrigin.Current:
                    Position = Position + offset;
                    break;
            }
            return Position;
        }
        public virtual SegmentSource Slice(long offset, long size)
        {
            var slice =  (SegmentSource)Activator.CreateInstance(this.GetType(), SourceObject, offset, size, OwnsSource)!;
            return slice;
        }
        public SegmentSource Slice(long size) => Slice(SourcePosition, size);
        //public virtual void WriteTo(Stream targetStream, long bufferSize = 65536)
        //{
        //    byte[] buffer = new byte[bufferSize];
        //    int n;
        //    while ((n = Read(buffer, 0, buffer.Length)) != 0)
        //        targetStream.Write(buffer, 0, n);
        //}
        public override void CopyTo(Stream destination, int bufferSize)
        {
            base.CopyTo(destination, bufferSize);
        }
        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return base.CopyToAsync(destination, bufferSize, cancellationToken);
        }
    }
    public abstract class SegmentSource<T> : SegmentSource
    {
        public T Source { get; private set; }
        public SegmentSource(T source, long offset, long size, bool ownsSource) : base(offset, size, ownsSource)
        {
            Source = source;
            SourceObject = source!;
        }
    }
}