namespace SpawnDev.WebMParser
{
    public class IntElement : WebMElement<long>
    {
        public static implicit operator long(IntElement element) => element.Data;
        int DataSize = 4;
        public IntElement(ElementId id) : base(id) { }
        public IntElement(ElementId id, long value) : base(id)
        {
            Data = value;
        }
        public override void UpdateBySource()
        {
            // switch endianness and pad to 64bit
            var bytes = Stream!.ReadBytes().Reverse().ToList();
            while (bytes.Count < 8) bytes.Add(0);
            Data = BitConverter.ToInt64(bytes.ToArray());
        }
        public override void UpdateByData()
        {
            // switch endianness and remove preceding 0 bytes
            var bytes = BitConverter.GetBytes(Data).Reverse().ToList();
            while (bytes.Count > 1 && bytes[0] == 0) bytes.RemoveAt(0);
            Stream = new ByteSegment(bytes.ToArray());
        }
    }
}
