using System.Text;

namespace WebMParser
{
    public class StringElement : WebMElement<string>
    {
        public static implicit operator string(StringElement element) => element.Data;
        public StringElement(ElementId id) : base(id) { }
        public Encoding Encoding { get; set; } = Encoding.UTF8;
        public StringElement(ElementId id, string value) : base(id)
        {
            Data = value;
        }
        public override void UpdateBySource()
        {
            Data = Encoding.GetString(Stream!.ReadBytes());
        }
        public override void UpdateByData()
        {
            Stream = new ByteSegment(Encoding.GetBytes(Data));
        }
    }
}
