namespace SpawnDev.WebMParser
{
    public class ContainerElement : WebMElement<List<WebMElement>>
    {
        public override string ToString() => $"[ {Id} ] - IdChain: [ {string.Join(" ", IdChain.ToArray())} ] Type: {this.GetType().Name} Length: {Length} bytes Entries: {Data.Count}";
        public ContainerElement(ElementId id) : base(id) { }
        public ContainerElement? GetContainer(params ElementId[] ids)
        {
            return GetElements<ContainerElement>(ids).FirstOrDefault();
        }
        public List<ContainerElement> GetContainers(params ElementId[] ids)
        {
            return GetElements<ContainerElement>(ids);
        }
        public WebMElement? GetElement(params ElementId[] ids)
        {
            return GetElements<WebMElement>(ids).FirstOrDefault();
        }
        public List<WebMElement> GetElements(params ElementId[] ids)
        {
            return GetElements<WebMElement>(ids);
        }
        public T? GetElement<T>(params ElementId[] ids) where T : WebMElement
        {
            return GetElements<T>(ids).FirstOrDefault();
        }
        public List<T> GetElements<T>(params ElementId[] ids) where T : WebMElement
        {
            ContainerElement? containerElement = this;
            for (var i = 0; i < ids.Length - 1; i++)
            {
                var id = ids[i];
                containerElement = (ContainerElement?)containerElement.Data.FirstOrDefault(o => o.Id == id);
                if (containerElement == null) return new List<T>();
            }
            var lastId = ids.Last();
            return containerElement.Data.Where(o => o.Id == lastId).Select(o => (T)o).ToList();
        }
        long CalculatedLength = 0;
        public override long Length => Stream != null ? Stream.Length : CalculatedLength;
        long CalculateLength()
        {
            long ret = 0;
            foreach (var element in Data)
            {
                var len = element.Length;
                ret += len;
                var idSize = GetElementIdUintSize(element.Id);
                var lenSize = GetContainerUintSize((uint)len);
                ret += idSize;
                ret += lenSize;
            }
            return ret;
        }
        public override long CopyTo(Stream stream, int? bufferSize = null)
        {
            if (!DataChanged && Stream != null)
            {
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
            else
            {
                foreach (var element in Data)
                {
                    var len = element.Length;
                    var idBytes = GetElementIdUintBytes(element.Id);
                    var lenBytes = GetContainerUintBytes((uint)len);
                    stream.Write(idBytes);
                    stream.Write(lenBytes);
                    element.CopyTo(stream, bufferSize);
                }
                return Length;
            }
        }
        public override void UpdateByData()
        {
            Stream = null;
            CalculatedLength = CalculateLength();
            DataChangedInvoke();
        }
        public override void UpdateBySource()
        {
            if (Data != null)
            {
                foreach (var el in Data)
                {
                    el.OnDataChanged -= Element_DataChanged;
                }
            }
            var elements = new List<WebMElement>();
            if (Stream == null)
            {
                return;
            }
            while (Stream.Position < Stream.Length)
            {
                var id = ReadElementId(Stream);
                var len = ReadContainerUint(Stream);
                var sectionBodyPos = Stream.Position;
                var elementType = GetElementType(id);
                if (len == UnknownElementSize)
                {
                    if (id == ElementId.Segment)
                    {
                        len = FindSegmentLength(Stream);
                        var gg = true;
                    }
                    else if (id == ElementId.Cluster)
                    {
                        len = FindClusterLength(Stream);
                        var gg = true;
                    }
                    else
                    {
                        throw new Exception("Invalid data");
                    }
                }
                var bytesLeft = Stream.Length - Stream.Position;
                if (len > bytesLeft || len == 0 || len == uint.MaxValue)
                {
                    // invalid section length
                    break;
                }
                var slice = Stream.Slice(len);
                var element = (WebMElement)Activator.CreateInstance(elementType, id)!;
                elements.Add(element);
                element.UpdateIdChain(IdChain);
                element.SetSource(slice);
                element.OnDataChanged += Element_DataChanged;
            }
            Data = elements;
        }
        public List<WebMElement> Descendants
        {
            get
            {
                var ret = new List<WebMElement>();
                foreach (var el in Data)
                {
                    ret.Add(el);
                    if (el is ContainerElement container)
                    {
                        ret.AddRange(container.Descendants);
                    }
                }
                return ret;
            }
        }
        public bool Add(ElementId id, double value) => Add(new FloatElement(id, value));
        public bool Add(ElementId id, float value) => Add(new FloatElement(id, value));
        public bool Add(ElementId id, string value) => Add(new StringElement(id, value));
        public bool Add(ElementId id, ulong value) => Add(new UintElement(id, value));
        public bool Add(ElementId id, long value) => Add(new IntElement(id, value));
        public bool Add(WebMElement element)
        {
            if (Data.Contains(element)) return false;
            Data.Add(element);
            element.UpdateIdChain(IdChain);
            element.OnDataChanged += Element_DataChanged;
            UpdateByData();
            return true;
        }
        private void Element_DataChanged(WebMElement obj)
        {
            UpdateByData();
        }
        public bool Remove(WebMElement element)
        {
            var succ = Data.Remove(element);
            if (!succ) return false;
            element.OnDataChanged -= Element_DataChanged;
            UpdateByData();
            return succ;
        }
        static uint FindSegmentLength(Stream stream)
        {
            long startOffset = stream.Position;
            long pos = stream.Position;
            while (pos < stream.Length)
            {
                pos = stream.Position;
                try
                {
                    var id = ReadElementId(stream);
                    var len = ReadContainerUint(stream);
                    if (len == uint.MaxValue && id == ElementId.Cluster)
                    {
                        len = FindClusterLength(stream);
                    }
                    if (!SegmentChildTypes.Contains(id))
                    {
                        break;
                    }
                    stream.Seek(len, SeekOrigin.Current);
                }
                catch (Exception ex)
                {
                    break;
                }
            }
            stream.Position = startOffset;
            return (uint)(pos - startOffset);
        }
        static uint FindClusterLength(Stream stream)
        {
            long startOffset = stream.Position;
            long pos = stream.Position;
            while (pos < stream.Length)
            {
                pos = stream.Position;
                try
                {
                    var id = ReadElementId(stream);
                    var len = ReadContainerUint(stream);
                    var sectionInfo = GetElementType(id);
                    if (!ClusterChildTypes.Contains(id))
                    {
                        break;
                    }
                    stream.Seek(len, SeekOrigin.Current);
                }
                catch (Exception ex)
                {
                    break;
                }
            }
            stream.Position = startOffset;
            return (uint)(pos - startOffset);
        }
    }
}
