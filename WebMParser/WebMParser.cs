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

        /// <summary>
        /// Get and Set for TimecodeScale from the first segment block
        /// </summary>
        public virtual uint? TimecodeScale
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

        string? Title
        {
            get
            {
                var title = GetElement<StringElement>(ElementId.Segment, ElementId.Info, ElementId.Title);
                return (string?)title;
            }
            set
            {
                var title = GetElement<StringElement>(ElementId.Segment, ElementId.Info, ElementId.Title);
                if (title == null)
                {
                    if (value != null)
                    {
                        var info = GetContainer(ElementId.Segment, ElementId.Info);
                        info!.Add(ElementId.Title, value);
                    }
                }
                else
                {
                    if (value == null)
                    {
                        title.Remove();
                    }
                    else
                    {
                        title.Data = value;
                    }
                }
            }
        }

        string? MuxingApp
        {
            get
            {
                var docType = GetElement<StringElement>(ElementId.Segment, ElementId.Info, ElementId.MuxingApp);
                return docType != null ? docType.Data : null;
            }
        }

        string? WritingApp
        {
            get
            {
                var docType = GetElement<StringElement>(ElementId.Segment, ElementId.Info, ElementId.WritingApp);
                return docType != null ? docType.Data : null;
            }
        }

        string? EBMLDocType
        {
            get
            {
                var docType = GetElement<StringElement>(ElementId.EBML, ElementId.DocType);
                return docType != null ? docType.Data : null;
            }
        }

        /// <summary>
        /// Returns true if audio tracks exist
        /// </summary>
        public virtual bool HasAudio => GetElements<TrackEntryElement>(ElementId.Segment, ElementId.Tracks, ElementId.TrackEntry).Where(o => o.TrackType == TrackType.Audio).Any();

        public virtual uint? AudioChannels
        {
            get
            {
                var channels = GetElement<UintElement>(ElementId.Segment, ElementId.Tracks, ElementId.TrackEntry, ElementId.Audio, ElementId.Channels);
                return channels != null ? (uint)channels : null;
            }
        }
        public virtual double? AudioSamplingFrequency
        {
            get
            {
                var samplingFrequency = GetElement<FloatElement>(ElementId.Segment, ElementId.Tracks, ElementId.TrackEntry, ElementId.Audio, ElementId.SamplingFrequency);
                return samplingFrequency != null ? (double)samplingFrequency : null;
            }
        }
        public virtual uint? AudioBitDepth
        {
            get
            {
                var bitDepth = GetElement<UintElement>(ElementId.Segment, ElementId.Tracks, ElementId.TrackEntry, ElementId.Audio, ElementId.BitDepth);
                return bitDepth != null ? (uint)bitDepth : null;
            }
        }


        /// <summary>
        /// Returns true if video tracks exist
        /// </summary>
        public virtual bool HasVideo => GetElements<TrackEntryElement>(ElementId.Segment, ElementId.Tracks, ElementId.TrackEntry).Where(o => o.TrackType == TrackType.Video).Any();

        public virtual string VideoCodecID => GetElements<TrackEntryElement>(ElementId.Segment, ElementId.Tracks, ElementId.TrackEntry).Where(o => o.TrackType == TrackType.Video).FirstOrDefault()?.CodecID ?? "";

        public virtual string AudioCodecID => GetElements<TrackEntryElement>(ElementId.Segment, ElementId.Tracks, ElementId.TrackEntry).Where(o => o.TrackType == TrackType.Audio).FirstOrDefault()?.CodecID ?? "";

        public virtual uint? VideoPixelWidth
        {
            get
            {
                var pixelWidth = GetElement<UintElement>(ElementId.Segment, ElementId.Tracks, ElementId.TrackEntry, ElementId.Video, ElementId.PixelWidth);
                return pixelWidth != null ? (uint)pixelWidth : null;
            }
        }

        public virtual uint? VideoPixelHeight
        {
            get
            {
                var pixelHeight = GetElement<UintElement>(ElementId.Segment, ElementId.Tracks, ElementId.TrackEntry, ElementId.Video, ElementId.PixelHeight);
                return pixelHeight != null ? (uint)pixelHeight : null;
            }
        }

        /// <summary>
        /// Get and Set for the first segment block duration
        /// </summary>
        public virtual double? Duration
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

        /// <summary>
        /// If the Duration is not set in the first segment block, the duration will be calculated using Cluster and SimpleBlock data and written to Duration
        /// </summary>
        /// <returns></returns>
        public virtual bool FixDuration()
        {
            if (Duration == null)
            {
                var durationEstimate = GetDurationEstimate();
                Duration = durationEstimate;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Duration calculated using Cluster and SimpleBlock data and written to Duration
        /// </summary>
        /// <returns></returns>
        public virtual double GetDurationEstimate()
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
