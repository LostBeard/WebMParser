namespace SpawnDev.WebMParser
{
    public class UintElement : WebMElement<ulong>
    {
        public static explicit operator ulong(UintElement? element) => element == null ? 0 : element.Data;
        public static explicit operator ulong?(UintElement? element) => element == null ? default : element.Data;
        public UintElement(ElementId id) : base(id) { }
        public UintElement(ElementId id, ulong value) : base(id)
        {
            Data = value;
        }
        public override void UpdateBySource()
        {
            // switch endianness and pad to 64bit
            var bytes = Stream!.ReadBytes().Reverse().ToList();
            while(bytes.Count < 8) bytes.Add(0);
            Data = BitConverter.ToUInt64(bytes.ToArray());
        }
        public override void UpdateByData()
        {
            // switch endianness and remove preceding 0 bytes
            var bytes = BitConverter.GetBytes(Data).Reverse().ToList();
            while(bytes.Count > 1 && bytes[0] == 0) bytes.RemoveAt(0);
            Stream = new ByteSegment(bytes.ToArray());
        }
    }
}
