using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

namespace SpawnDev.WebMParser
{
    public class ContainerElement : WebMElement<ReadOnlyCollection<WebMElement>>
    {
        private List<WebMElement> __Data = new List<WebMElement>();
        public override ReadOnlyCollection<WebMElement> Data
        {
            get => __Data.AsReadOnly();
            set => throw new NotImplementedException();
        }
        
        public override string ToString() => $"{Index} [ {Id} ] - IdChain: [ {string.Join(" ", IdChain.ToArray())} ] Type: {this.GetType().Name} Length: {Length} bytes Entries: {Data.Count}";
        public ContainerElement(ElementId id) : base(id) { }
        public ContainerElement? GetContainer(params ElementId[] ids) => GetElement<ContainerElement>(ids);
        public List<ContainerElement> GetContainers(params ElementId[] ids) => GetElements<ContainerElement>(ids);
        public bool ElementExists(params ElementId[] idChain) => GetElement<WebMElement>(idChain) != null;
        public WebMElement? GetElement(params ElementId[] idChain) => GetElement<WebMElement>(idChain);
        public List<WebMElement> GetElements(params ElementId[] idChain) => GetElements<WebMElement>(idChain);
        public T? GetElement<T>(params ElementId[] idChain) where T : WebMElement => (T?)Descendants.FirstOrDefault(o => o.IdChain.SequenceEndsWith(idChain));
        /// <summary>
        /// Returns all elements with an IdChain that ends with the provided idChain<br />
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="idChain">idChain is an array ending in the desired ElementId types to be returned, with optional preceding parent ElementIds</param>
        /// <returns></returns>
        public List<T> GetElements<T>(params ElementId[] idChain) where T : WebMElement => Descendants.Where(o => o.IdChain.SequenceEndsWith(idChain)).Select(o => (T)o).ToList();
        long CalculatedLength = 0;
        /// <summary>
        /// Returns the byte length of this container
        /// </summary>
        public override long Length => Stream != null ? Stream.Length : CalculatedLength;
        private long CalculateLength()
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
        /// <summary>
        /// Copies the container to the specified stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
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
            foreach (var el in __Data)
            {
                el.OnDataChanged -= Element_DataChanged;
                el.SetParent();
            }
            __Data.Clear();
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
                    }
                    else if (id == ElementId.Cluster)
                    {
                        len = FindClusterLength(Stream);
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
                element.SetParent(this);
                element.SetSource(slice);
                element.OnDataChanged += Element_DataChanged;
                Stream.Position = sectionBodyPos + len;
            }
            __Data = elements;
        }
        /// <summary>
        /// A flat list of all the elements contained by this element and their children
        /// </summary>
        public ReadOnlyCollection<WebMElement> Descendants
        {
            get
            {
                var ret = new List<WebMElement>();
                foreach (var el in __Data)
                {
                    ret.Add(el);
                    if (el is ContainerElement container)
                    {
                        ret.AddRange(container.Descendants);
                    }
                }
                return ret.AsReadOnly();
            }
        }
        /// <summary>
        /// Adds a FloatElement to this container with the given value
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Add(ElementId id, double value) => Add(new FloatElement(id, value));
        /// <summary>
        /// Adds a FloatElement to this container with the given value
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Add(ElementId id, float value) => Add(new FloatElement(id, value));
        /// <summary>
        /// Adds a StringElement to this container with the given value
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Add(ElementId id, string value) => Add(new StringElement(id, value));
        /// <summary>
        /// Adds a UintElement to this container with the given value
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Add(ElementId id, ulong value) => Add(new UintElement(id, value));
        /// <summary>
        /// Adds an IntElement to this container with the given value
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Add(ElementId id, long value) => Add(new IntElement(id, value));
        /// <summary>
        /// Adds a WebMElement to this container with the given value
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public bool Add(WebMElement element)
        {
            if (__Data.Contains(element)) return false;
            __Data.Add(element);
            element.SetParent(this);
            element.OnDataChanged += Element_DataChanged;
            UpdateByData();
            return true;
        }
        private void Element_DataChanged(WebMElement obj)
        {
            UpdateByData();
        }
        /// <summary>
        /// Removes a WebElement from this container
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public bool Remove(WebMElement element)
        {
            var succ = __Data.Remove(element);
            if (!succ) return false;
            element.SetParent();
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
                    if (!SegmentChildIds.Contains(id))
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
                    if (!ClusterChildIds.Contains(id))
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
