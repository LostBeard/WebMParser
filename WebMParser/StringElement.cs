﻿using System.Text;

namespace SpawnDev.WebMParser
{
    public class StringElement : WebMElement<string>
    {
        public static explicit operator string?(StringElement? element) => element == null ? null : element.Data;
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
