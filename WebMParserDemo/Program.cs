using SpawnDev.WebMParser;

namespace WebMParserDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var inputFile = "";
            var verbose = 1;
            var fixDuration = true;
#if DEBUG
            var videoFolder = @"C:\Users\TJ\.SpawnDev.AccountsServer\messages";
            inputFile = Path.Combine(videoFolder, "test.webm");
            //inputFile = Path.Combine(videoFolder, "56135218-d984-4b18-96a8-f81e830da98f.webm");
            inputFile = Path.Combine(videoFolder, "Big_Buck_Bunny_4K.webm.480p.vp9.webm");
#endif
            //
            var inputFileBaseName = Path.GetFileName(inputFile);
            Console.WriteLine($"Input: {inputFileBaseName}");
            //
            using var inputStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            var webm = new WebMStreamParser(inputStream);
            var tracks = webm.GetElements<TrackEntryElement>(ElementId.TrackEntry);
            // 
            if (verbose >= 1)
            {
                Console.WriteLine($"Duration: {webm.Duration.ToString() ?? "-"}");
                if (webm.HasAudio)
                {
                    Console.WriteLine($"-- Audio --");
                    Console.WriteLine($"AudioCodecID: {webm.AudioCodecID}");
                    Console.WriteLine($"AudioSamplingFrequency: {webm.AudioSamplingFrequency}");
                    Console.WriteLine($"AudioBitDepth: {webm.AudioBitDepth}");
                    Console.WriteLine($"AudioChannels: {webm.AudioChannels}");
                }
                else
                {
                    Console.WriteLine($"-- Audio --");
                }
                if (webm.HasVideo)
                {
                    Console.WriteLine($"-- Video --");
                    Console.WriteLine($"VideoCodecID: {webm.VideoCodecID}");
                    Console.WriteLine($"VideoSize: {webm.VideoPixelWidth}x{webm.VideoPixelHeight}");
                }
                else
                {
                    Console.WriteLine($"-- Video --");
                }
            }
            if (verbose >= 2)
            {
                // Display verbose element output
                var elements = webm.Descendants;
                foreach (var element in elements)
                {
                    var indent = new string('-', element.IdChain.Length - 1);
                    var elementStr = element.ToString();
                    if (elementStr.Contains("SimpleBlock")) continue;
                    Console.WriteLine($"{indent}{elementStr}");
                }
            }
            // 
            if (fixDuration)
            {
                // Fix duration if not present
                var modified = webm.FixDuration();
                // webm.DataChanged will also be true if the WebM is modified
                if (modified)
                {
                    var outFile = Path.Combine(Path.GetDirectoryName(inputFile)!, Path.GetFileNameWithoutExtension(inputFile) + ".fixed" + Path.GetExtension(inputFile));
                    using var outputStream = new FileStream(outFile, FileMode.Create, FileAccess.Write, FileShare.None);
                    webm.CopyTo(outputStream);
                }
            }
#if DEBUG
            Console.ReadLine();
#endif
        }
    }
}
