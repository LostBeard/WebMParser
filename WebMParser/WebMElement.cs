using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SpawnDev.WebMParser
{
    public abstract class WebMElement
    {
        public bool DataChanged { get; protected set; }
        public ElementId Id { get; init; }
        public ElementId[] IdChain { get; protected set; }
        public SegmentSource? Stream { get; protected set; }
        public virtual long Length => Stream != null ? Stream.Length : 0;
        public WebMElement(ElementId id)
        {
            Id = id;
            IdChain = id == ElementId.File ? [] : [id];
        }

        public void UpdateIdChain(ElementId[] parentIdChain)
        {
            var idChain = new List<ElementId>(parentIdChain);
            if (Id != ElementId.File) idChain.Add(Id);
            IdChain = idChain.ToArray();
        }

        public static ElementId ReadElementId(Stream data) => (ElementId)ReadContainerUint(data);
        public static uint ReadContainerUint(Stream data)
        {
            var firstByte = (byte)data.ReadByte();
            var bytes = 8 - Convert.ToString(firstByte, 2).Length;
            long value = firstByte - (1 << (7 - bytes));
            for (var i = 0; i < bytes; i++)
            {
                value *= 256;
                value += (byte)data.ReadByte();
            }
            return (uint)value;
        }
        public static int GetElementIdUintSize(ElementId x) => GetContainerUintSize((uint)x);
        public static int GetContainerUintSize(uint x)
        {
            int bytes;
            int flag;
            for (bytes = 1, flag = 0x80; x >= flag && bytes < 8; bytes++, flag *= 0x80) { }
            return bytes;
        }
        public static byte[] GetElementIdUintBytes(ElementId x) => GetContainerUintBytes((uint)x);
        public static byte[] GetContainerUintBytes(uint x)
        {
            int bytes;
            int flag;
            for (bytes = 1, flag = 0x80; x >= flag && bytes < 8; bytes++, flag *= 0x80) { }
            var ret = new byte[bytes];
            var value = flag + x;
            for (var i = bytes - 1; i >= 0; i--)
            {
                var c = value % 256;
                ret[i] = (byte)c;
                value = (value - c) / 256;
            }
            return ret;
        }

        public virtual long CopyTo(Stream stream, int? bufferSize = null)
        {
            if (Stream == null) return 0;
            Stream.Position = 0;
            if (bufferSize != null)
            {
                Stream.CopyTo(stream, bufferSize.Value);
            }
            else
            {
                Stream.CopyTo(stream);
            }
            return Length;
        }

        /// <summary>
        /// Should be overridden and internally update WebMBase.Data
        /// </summary>
        public virtual void UpdateBySource()
        {
            Stream?.Seek(0, SeekOrigin.End);
        }
        protected bool UpdatingSource { get; private set; } = false;
        public void SetSource(SegmentSource stream)
        {
            Stream = stream;
            UpdatingSource = true;
            UpdateBySource();
            UpdatingSource = false;
        }
        public static List<ElementId> ClusterChildTypes = new List<ElementId>
        {
            ElementId.Timecode,
            ElementId.Position,
            ElementId.PrevSize,
            ElementId.SimpleBlock,
            ElementId.BlockGroup,
        };
        public static List<ElementId> SegmentChildTypes = new List<ElementId>
        {
            ElementId.SeekHead,
            ElementId.Info,
            ElementId.Tracks,
            ElementId.Chapters,
            ElementId.Cluster,
            ElementId.Cues,
            ElementId.Attachments,
            ElementId.Tags,
        };
        public static Type GetElementType(ElementId id) => ElementTypeMap.TryGetValue(id, out var ret) ? ret : typeof(UnknownElement);
        public static Dictionary<ElementId, Type> ElementTypeMap = new Dictionary<ElementId, Type>
        {
            { ElementId.EBML, typeof(ContainerElement) },
            { ElementId.EBMLVersion, typeof(UintElement) },
            { ElementId.EBMLReadVersion, typeof(UintElement) },
            { ElementId.EBMLMaxIDLength, typeof(UintElement) },
            { ElementId.EBMLMaxSizeLength, typeof(UintElement) },
            { ElementId.DocType, typeof(StringElement) },
            { ElementId.DocTypeVersion, typeof(UintElement) },
            { ElementId.DocTypeReadVersion, typeof(UintElement) },
            { ElementId.Void, typeof(BinaryElement) },
            { ElementId.CRC32, typeof(BinaryElement) },
            { ElementId.SignatureSlot, typeof(ContainerElement) },
            { ElementId.SignatureAlgo, typeof(UintElement) },
            { ElementId.SignatureHash, typeof(UintElement) },
            { ElementId.SignaturePublicKey, typeof(BinaryElement) },
            { ElementId.Signature, typeof(BinaryElement) },
            { ElementId.SignatureElements, typeof(ContainerElement) },
            { ElementId.SignatureElementList, typeof(ContainerElement) },
            { ElementId.SignedElement, typeof(BinaryElement) },
            { ElementId.Segment, typeof(ContainerElement) },
            { ElementId.SeekHead, typeof(ContainerElement) },
            { ElementId.Seek, typeof(ContainerElement) },
            { ElementId.SeekID, typeof(BinaryElement) },
            { ElementId.SeekPosition, typeof(UintElement) },
            { ElementId.Info, typeof(ContainerElement) },
            { ElementId.SegmentUID, typeof(BinaryElement) },
            { ElementId.SegmentFilename, typeof(StringElement) },
            { ElementId.PrevUID, typeof(BinaryElement) },
            { ElementId.PrevFilename, typeof(StringElement) },
            { ElementId.NextUID, typeof(BinaryElement) },
            { ElementId.NextFilename, typeof(StringElement) },
            { ElementId.SegmentFamily, typeof(BinaryElement) },
            { ElementId.ChapterTranslate, typeof(ContainerElement) },
            { ElementId.ChapterTranslateEditionUID, typeof(UintElement) },
            { ElementId.ChapterTranslateCodec, typeof(UintElement) },
            { ElementId.ChapterTranslateID, typeof(BinaryElement) },
            { ElementId.TimecodeScale, typeof(UintElement) },
            { ElementId.Duration, typeof(FloatElement) },
            { ElementId.DateUTC, typeof(DateElement) },
            { ElementId.Title, typeof(StringElement) },
            { ElementId.MuxingApp, typeof(StringElement) },
            { ElementId.WritingApp, typeof(StringElement) },
            { ElementId.Cluster, typeof(ContainerElement) },
            { ElementId.Timecode, typeof(UintElement) },
            { ElementId.SilentTracks, typeof(ContainerElement) },
            { ElementId.SilentTrackNumber, typeof(UintElement) },
            { ElementId.Position, typeof(UintElement) },
            { ElementId.PrevSize, typeof(UintElement) },
            { ElementId.SimpleBlock, typeof(SimpleBlockElement) },
            { ElementId.BlockGroup, typeof(ContainerElement) },
            { ElementId.Block, typeof(BinaryElement) },
            { ElementId.BlockVirtual, typeof(BinaryElement) },
            { ElementId.BlockAdditions, typeof(ContainerElement) },
            { ElementId.BlockMore, typeof(ContainerElement) },
            { ElementId.BlockAddID, typeof(UintElement) },
            { ElementId.BlockAdditional, typeof(BinaryElement) },
            { ElementId.BlockDuration, typeof(UintElement) },
            { ElementId.ReferencePriority, typeof(UintElement) },
            { ElementId.ReferenceBlock, typeof(IntElement) },
            { ElementId.ReferenceVirtual, typeof(IntElement) },
            { ElementId.CodecState, typeof(BinaryElement) },
            { ElementId.DiscardPadding, typeof(IntElement) },
            { ElementId.Slices, typeof(ContainerElement) },
            { ElementId.TimeSlice, typeof(ContainerElement) },
            { ElementId.LaceNumber, typeof(UintElement) },
            { ElementId.FrameNumber, typeof(UintElement) },
            { ElementId.BlockAdditionID, typeof(UintElement) },
            { ElementId.Delay, typeof(UintElement) },
            { ElementId.SliceDuration, typeof(UintElement) },
            { ElementId.ReferenceFrame, typeof(ContainerElement) },
            { ElementId.ReferenceOffset, typeof(UintElement) },
            { ElementId.ReferenceTimeCode, typeof(UintElement) },
            { ElementId.EncryptedBlock, typeof(BinaryElement) },
            { ElementId.Tracks, typeof(ContainerElement) },
            { ElementId.TrackEntry, typeof(ContainerElement) },
            { ElementId.TrackNumber, typeof(UintElement) },
            { ElementId.TrackUID, typeof(UintElement) },
            { ElementId.TrackType, typeof(UintElement) },
            { ElementId.FlagEnabled, typeof(UintElement) },
            { ElementId.FlagDefault, typeof(UintElement) },
            { ElementId.FlagForced, typeof(UintElement) },
            { ElementId.FlagLacing, typeof(UintElement) },
            { ElementId.MinCache, typeof(UintElement) },
            { ElementId.MaxCache, typeof(UintElement) },
            { ElementId.DefaultDuration, typeof(UintElement) },
            { ElementId.DefaultDecodedFieldDuration, typeof(UintElement) },
            { ElementId.TrackTimecodeScale, typeof(FloatElement) },
            { ElementId.TrackOffset, typeof(IntElement) },
            { ElementId.MaxBlockAdditionID, typeof(UintElement) },
            { ElementId.Name, typeof(StringElement) },
            { ElementId.Language, typeof(StringElement) },
            { ElementId.CodecID, typeof(StringElement) },
            { ElementId.CodecPrivate, typeof(BinaryElement) },
            { ElementId.CodecName, typeof(StringElement) },
            { ElementId.AttachmentLink, typeof(UintElement) },
            { ElementId.CodecSettings, typeof(StringElement) },
            { ElementId.CodecInfoURL, typeof(StringElement) },
            { ElementId.CodecDownloadURL, typeof(StringElement) },
            { ElementId.CodecDecodeAll, typeof(UintElement) },
            { ElementId.TrackOverlay, typeof(UintElement) },
            { ElementId.CodecDelay, typeof(UintElement) },
            { ElementId.SeekPreRoll, typeof(UintElement) },
            { ElementId.TrackTranslate, typeof(ContainerElement) },
            { ElementId.TrackTranslateEditionUID, typeof(UintElement) },
            { ElementId.TrackTranslateCodec, typeof(UintElement) },
            { ElementId.TrackTranslateTrackID, typeof(BinaryElement) },
            { ElementId.Video, typeof(ContainerElement) },
            { ElementId.FlagInterlaced, typeof(UintElement) },
            { ElementId.StereoMode, typeof(UintElement) },
            { ElementId.AlphaMode, typeof(UintElement) },
            { ElementId.OldStereoMode, typeof(UintElement) },
            { ElementId.PixelWidth, typeof(UintElement) },
            { ElementId.PixelHeight, typeof(UintElement) },
            { ElementId.PixelCropBottom, typeof(UintElement) },
            { ElementId.PixelCropTop, typeof(UintElement) },
            { ElementId.PixelCropLeft, typeof(UintElement) },
            { ElementId.PixelCropRight, typeof(UintElement) },
            { ElementId.DisplayWidth, typeof(UintElement) },
            { ElementId.DisplayHeight, typeof(UintElement) },
            { ElementId.DisplayUnit, typeof(UintElement) },
            { ElementId.AspectRatioType, typeof(UintElement) },
            { ElementId.ColourSpace, typeof(BinaryElement) },
            { ElementId.GammaValue, typeof(FloatElement) },
            { ElementId.FrameRate, typeof(FloatElement) },
            { ElementId.Audio, typeof(ContainerElement) },
            { ElementId.SamplingFrequency, typeof(FloatElement) },
            { ElementId.OutputSamplingFrequency, typeof(FloatElement) },
            { ElementId.Channels, typeof(UintElement) },
            { ElementId.ChannelPositions, typeof(BinaryElement) },
            { ElementId.BitDepth, typeof(UintElement) },
            { ElementId.TrackOperation, typeof(ContainerElement) },
            { ElementId.TrackCombinePlanes, typeof(ContainerElement) },
            { ElementId.TrackPlane, typeof(ContainerElement) },
            { ElementId.TrackPlaneUID, typeof(UintElement) },
            { ElementId.TrackPlaneType, typeof(UintElement) },
            { ElementId.TrackJoinBlocks, typeof(ContainerElement) },
            { ElementId.TrackJoinUID, typeof(UintElement) },
            { ElementId.TrickTrackUID, typeof(UintElement) },
            { ElementId.TrickTrackSegmentUID, typeof(BinaryElement) },
            { ElementId.TrickTrackFlag, typeof(UintElement) },
            { ElementId.TrickMasterTrackUID, typeof(UintElement) },
            { ElementId.TrickMasterTrackSegmentUID, typeof(BinaryElement) },
            { ElementId.ContentEncodings, typeof(ContainerElement) },
            { ElementId.ContentEncoding, typeof(ContainerElement) },
            { ElementId.ContentEncodingOrder, typeof(UintElement) },
            { ElementId.ContentEncodingScope, typeof(UintElement) },
            { ElementId.ContentEncodingType, typeof(UintElement) },
            { ElementId.ContentCompression, typeof(ContainerElement) },
            { ElementId.ContentCompAlgo, typeof(UintElement) },
            { ElementId.ContentCompSettings, typeof(BinaryElement) },
            { ElementId.ContentEncryption, typeof(ContainerElement) },
            { ElementId.ContentEncAlgo, typeof(UintElement) },
            { ElementId.ContentEncKeyID, typeof(BinaryElement) },
            { ElementId.ContentSignature, typeof(BinaryElement) },
            { ElementId.ContentSigKeyID, typeof(BinaryElement) },
            { ElementId.ContentSigAlgo, typeof(UintElement) },
            { ElementId.ContentSigHashAlgo, typeof(UintElement) },
            { ElementId.Cues, typeof(ContainerElement) },
            { ElementId.CuePoint, typeof(ContainerElement) },
            { ElementId.CueTime, typeof(UintElement) },
            { ElementId.CueTrackPositions, typeof(ContainerElement) },
            { ElementId.CueTrack, typeof(UintElement) },
            { ElementId.CueClusterPosition, typeof(UintElement) },
            { ElementId.CueRelativePosition, typeof(UintElement) },
            { ElementId.CueDuration, typeof(UintElement) },
            { ElementId.CueBlockNumber, typeof(UintElement) },
            { ElementId.CueCodecState, typeof(UintElement) },
            { ElementId.CueReference, typeof(ContainerElement) },
            { ElementId.CueRefTime, typeof(UintElement) },
            { ElementId.CueRefCluster, typeof(UintElement) },
            { ElementId.CueRefNumber, typeof(UintElement) },
            { ElementId.CueRefCodecState, typeof(UintElement) },
            { ElementId.Attachments, typeof(ContainerElement) },
            { ElementId.AttachedFile, typeof(ContainerElement) },
            { ElementId.FileDescription, typeof(StringElement) },
            { ElementId.FileName, typeof(StringElement) },
            { ElementId.FileMimeType, typeof(StringElement) },
            { ElementId.FileData, typeof(BinaryElement) },
            { ElementId.FileUID, typeof(UintElement) },
            { ElementId.FileReferral, typeof(BinaryElement) },
            { ElementId.FileUsedStartTime, typeof(UintElement) },
            { ElementId.FileUsedEndTime, typeof(UintElement) },
            { ElementId.Chapters, typeof(ContainerElement) },
            { ElementId.EditionEntry, typeof(ContainerElement) },
            { ElementId.EditionUID, typeof(UintElement) },
            { ElementId.EditionFlagHidden, typeof(UintElement) },
            { ElementId.EditionFlagDefault, typeof(UintElement) },
            { ElementId.EditionFlagOrdered, typeof(UintElement) },
            { ElementId.ChapterAtom, typeof(ContainerElement) },
            { ElementId.ChapterUID, typeof(UintElement) },
            { ElementId.ChapterStringUID, typeof(StringElement) },
            { ElementId.ChapterTimeStart, typeof(UintElement) },
            { ElementId.ChapterTimeEnd, typeof(UintElement) },
            { ElementId.ChapterFlagHidden, typeof(UintElement) },
            { ElementId.ChapterFlagEnabled, typeof(UintElement) },
            { ElementId.ChapterSegmentUID, typeof(BinaryElement) },
            { ElementId.ChapterSegmentEditionUID, typeof(UintElement) },
            { ElementId.ChapterPhysicalEquiv, typeof(UintElement) },
            { ElementId.ChapterTrack, typeof(ContainerElement) },
            { ElementId.ChapterTrackNumber, typeof(UintElement) },
            { ElementId.ChapterDisplay, typeof(ContainerElement) },
            { ElementId.ChapString, typeof(StringElement) },
            { ElementId.ChapLanguage, typeof(StringElement) },
            { ElementId.ChapCountry, typeof(StringElement) },
            { ElementId.ChapProcess, typeof(ContainerElement) },
            { ElementId.ChapProcessCodecID, typeof(UintElement) },
            { ElementId.ChapProcessPrivate, typeof(BinaryElement) },
            { ElementId.ChapProcessCommand, typeof(ContainerElement) },
            { ElementId.ChapProcessTime, typeof(UintElement) },
            { ElementId.ChapProcessData, typeof(BinaryElement) },
            { ElementId.Tags, typeof(ContainerElement) },
            { ElementId.Tag, typeof(ContainerElement) },
            { ElementId.Targets, typeof(ContainerElement) },
            { ElementId.TargetTypeValue, typeof(UintElement) },
            { ElementId.TargetType, typeof(StringElement) },
            { ElementId.TagTrackUID, typeof(UintElement) },
            { ElementId.TagEditionUID, typeof(UintElement) },
            { ElementId.TagChapterUID, typeof(UintElement) },
            { ElementId.TagAttachmentUID, typeof(UintElement) },
            { ElementId.SimpleTag, typeof(ContainerElement) },
            { ElementId.TagName, typeof(StringElement) },
            { ElementId.TagLanguage, typeof(StringElement) },
            { ElementId.TagDefault, typeof(UintElement) },
            { ElementId.TagString, typeof(StringElement) },
            { ElementId.TagBinary, typeof(BinaryElement) },
        };
        public static uint UnknownElementSize = uint.MaxValue;
        public override string ToString() => $"{Id} - IdChain: [ {string.Join(" ", IdChain.ToArray())} ] Type: {this.GetType().Name} Length: {Length} bytes";
        public event Action<WebMElement> OnDataChanged;
        protected void DataChangedInvoke()
        {
            DataChanged = true;
            OnDataChanged?.Invoke(this);
        }
    }
    public abstract class WebMElement<T> : WebMElement
    {
        private T _Data = default(T);
        public virtual T Data 
        { 
            get => _Data; 
            set 
            {
                //var isEqual = EqualityComparer<T>.Default.Equals(_Data, value);
                //if (isEqual) return;
                _Data = value;
                if (!UpdatingSource)
                {
                    UpdateByData();
                    DataChangedInvoke();
                }
            } 
        }
        public WebMElement(ElementId id) : base(id){ }
        /// <summary>
        /// Should be overridden and internally update WebMBase.Source
        /// </summary>
        public virtual void UpdateByData()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Should be overridden and internally update WebMBase.Data
        /// </summary>
        public override void UpdateBySource()
        {
            throw new NotImplementedException();
        }
        public override string ToString() => $"{Id} - IdChain: [ {string.Join(" ", IdChain.ToArray())} ] Type: {this.GetType().Name} Length: {Length} bytes Value: {Data}";
    }
}
