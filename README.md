# SpawnDev.WebMParser

| Package | Description |
|---------|-------------|
|**[SpawnDev.WebMParser](#webmparser)** <br /> [![NuGet version](https://badge.fury.io/nu/SpawnDev.WebMParser.svg)](https://www.nuget.org/packages/SpawnDev.WebMParser)| .Net WebM parser and editor | 

WebMParser is a .Net WebM parser written in C#. 

The initial goal of this library is to allow adding a duration to WebM video files recorded using [MediaRecorder](https://developer.mozilla.org/en-US/docs/Web/API/MediaRecorder) in a web browser. I am using this library in a browser based Blazor WebAssembly video messaging application.  

The demo project included with the library is a .Net core 8 console app that currently just allows testing the library.  

To fix the duration in a WebM file, WebM parser reads the Timecode information from Clusters and SimpleBlocks and adds a Segment > Info > Duration element with the new duration.


Example of how to add Duration info if not found in a webm stream.
```cs

using var inputStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read);

var webm = new WebMStreamParser(inputStream);

// FixDuration returns true if the WebM was modified
var modified = webm.FixDuration();
// webm.Modified will also be true if the WebM is modified
if (modified)
{
    var outFile = Path.Combine(Path.GetDirectoryName(inputFile)!, Path.GetFileNameWithoutExtension(inputFile) + ".fixed" + Path.GetExtension(inputFile));
    using var outputStream = new FileStream(outFile, FileMode.Create, FileAccess.Write, FileShare.None);
    webm.CopyTo(outputStream);
}
```

Example that prints out basic info for every element found in the WebM stream
```cs
var elements = webm.Descendants;
foreach (var element in elements)
{
    var indent = new string('-', element.IdChain.Length - 1);
    Console.WriteLine($"{indent}{element}");
}
```

Example of how to get an element
```cs
var durationElement = webm.GetElement<FloatElement>(ElementId.Segment, ElementId.Info, ElementId.Duration);
var duration = durationElement?.Data ?? 0;
```

Example of how to get all elements of a type
```cs
var segments = webm.GetElements<ContainerElement>(ElementId.Segment);
```

Example of how to use ElementIds to walk the data tree and access information
```cs
var segments = webm.GetContainers(ElementId.Segment);
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
```

Example of how to add an element  
All parent containers are automatically marked Modified if any children are added, removed, or changed.
```cs
var info = GetContainer(ElementId.Segment, ElementId.Info);
info!.Add(ElementId.Duration, 100000);
```