namespace SpawnDev.WebMParser
{
    public class FloatElement : WebMElement<double>
    { 
        public static implicit operator double(FloatElement element) => element.Data;
        int DataSize = 4;
        public FloatElement(ElementId id) : base(id) { }
        public FloatElement(ElementId id, float value) : base(id)
        {
            DataSize = 4;
            Data = value;
        }
        public FloatElement(ElementId id, double value) : base(id)
        {
            DataSize = 8;
            Data = value;
        }
        public override void UpdateBySource()
        {
            DataSize = (int)Stream!.Length;
            var source = Stream!.ReadBytes().Reverse().ToArray();
            if (DataSize == 4)
            {
                Data = BitConverter.ToSingle(source);
            }
            else if (DataSize == 8)
            {
                Data = BitConverter.ToDouble(source);
            }
        }
        public override void UpdateByData()
        {
            if (DataSize == 4)
            {
                var bytes = BitConverter.GetBytes((float)Data).Reverse().ToArray();
                Stream = new ByteSegment(bytes);
            }
            else if (DataSize == 8)
            {
                var bytes = BitConverter.GetBytes(Data).Reverse().ToArray();
                Stream = new ByteSegment(bytes);
            }
        }
    }
}
