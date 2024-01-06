namespace SpawnDev.WebMParser
{
    public class SimpleBlockElement : BinaryElement
    {
        public SimpleBlockElement(ElementId id) : base(id) { }
        public byte TrackId => (byte)(Stream!.ReadByte(0) & ~0x80);
        public uint Timecode => BitConverter.ToUInt16(Stream!.ReadBytes(1, 2).Reverse().ToArray());
        public override string ToString() => $"{Id} - IdChain: [ {string.Join(" ", IdChain.ToArray())} ] Type: {this.GetType().Name} Length: {Length} bytes TrackId: {TrackId} Timecode: {Timecode}";
    }
}
