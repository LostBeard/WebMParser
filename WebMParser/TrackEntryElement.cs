using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawnDev.WebMParser
{
    public enum TrackType : byte
    {
        Video = 1,
        Audio = 2,
        Complex = 3,
        Logo = 0x10,
        Subtitle = 0x11,
        Buttons = 0x12,
        Control = 0x20,
    }
    public class TrackEntryElement : ContainerElement
    {
        public TrackEntryElement(ElementId id) : base(id)
        {
        }
        public byte TrackNumber
        {
            get => (byte)(ulong)GetElement<UintElement>(ElementId.TrackNumber);
        }
        public byte TrackUID
        {
            get => (byte)(ulong)GetElement<UintElement>(ElementId.TrackUID);
        }
        public TrackType TrackType
        {
            get => (TrackType)(byte)(ulong)GetElement<UintElement>(ElementId.TrackType);
        }
        public string CodecID
        {
            get => (string)GetElement<StringElement>(ElementId.CodecID)!;
        }
        public string Language
        {
            get => (string)GetElement<StringElement>(ElementId.Language)!;
        }
        public ulong DefaultDuration
        {
            get => (ulong)GetElement<UintElement>(ElementId.DefaultDuration);
        }
    }
}
