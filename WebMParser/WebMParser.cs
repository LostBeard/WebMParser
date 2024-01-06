namespace SpawnDev.WebMParser
{
    public class WebMStreamParser : ContainerElement
    {
        public WebMStreamParser(Stream? stream = null) : base(ElementId.File)
        {
            if (stream != null)
            {
                if (typeof(SegmentSource).IsAssignableFrom(stream.GetType()))
                {
                    SetSource((SegmentSource)stream);
                }
                else
                {
                    SetSource(new StreamSegment(stream));
                }
            }
        }

        public uint? TimecodeScale
        {
            get
            {
                var timecodeScale = GetElement<UintElement>(ElementId.Segment, ElementId.Info, ElementId.TimecodeScale);
                return timecodeScale != null ? (uint)timecodeScale.Data : null;
            }
            set
            {
                var timecodeScale = GetElement<UintElement>(ElementId.Segment, ElementId.Info, ElementId.TimecodeScale);
                if (timecodeScale == null)
                {
                    if (value != null)
                    {
                        var info = GetContainer(ElementId.Segment, ElementId.Info);
                        info!.Add(ElementId.TimecodeScale, value.Value);
                    }
                }
                else
                {
                    if (value == null)
                    {
                        var info = GetContainer(ElementId.Segment, ElementId.Info);
                        info!.Remove(timecodeScale);
                    }
                    else
                    {
                        timecodeScale.Data = value.Value;
                    }
                }
            }
        }
        public double? Duration
        {
            get
            {
                var duration = GetElement<FloatElement>(ElementId.Segment, ElementId.Info, ElementId.Duration);
                return duration != null ? duration.Data : null;
            }
            set
            {
                var duration = GetElement<FloatElement>(ElementId.Segment, ElementId.Info, ElementId.Duration);
                if (duration == null)
                {
                    if (value != null)
                    {
                        var info = GetContainer(ElementId.Segment, ElementId.Info);
                        info!.Add(ElementId.Duration, value.Value);
                    }
                }
                else
                {
                    if (value == null)
                    {
                        var info = GetContainer(ElementId.Segment, ElementId.Info);
                        info!.Remove(duration);
                    }
                    else
                    {
                        duration.Data = value.Value;
                    }
                }
            }
        }

        public bool FixDuration()
        {
            if (Duration == null)
            {
                var durationEstimate = GetDurationEstimate();
                Duration = durationEstimate;
                return true;
            }
            return false;
        }

        public double GetDurationEstimate()
        {
            double duration = 0;
            var segments = GetContainers(ElementId.Segment);
            foreach (var segment in segments)
            {
                var clusters = segment.GetContainers(ElementId.Cluster);
                foreach (var cluster in clusters)
                {
                    var timecode = cluster.GetElement<UintElement>(ElementId.Timecode);
                    if (timecode != null)
                    {
                        duration = timecode.Data;
                    };
                    var simpleBlocks = cluster.GetElements<SimpleBlockElement>(ElementId.SimpleBlock);
                    var simpleBlockLast = simpleBlocks.LastOrDefault();
                    if (simpleBlockLast != null)
                    {
                        duration += simpleBlockLast.Timecode;
                    }
                }
            }
            return duration;
        }
    }
}
