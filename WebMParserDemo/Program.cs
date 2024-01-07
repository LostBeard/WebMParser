using SpawnDev.WebMParser;

namespace WebMParserDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var inputFile = "";
            var verbose = true;
            var fixDuration = true;
#if DEBUG
            var videoFolder = @"C:\Users\TJ\.SpawnDev.AccountsServer\messages";
            inputFile = Path.Combine(videoFolder, "test.webm");
            inputFile = Path.Combine(videoFolder, "56135218-d984-4b18-96a8-f81e830da98f.webm");
            //inputFile = Path.Combine(videoFolder, "Big_Buck_Bunny_4K.webm.480p.vp9.webm");
#endif
            //
            var inputFileBaseName = Path.GetFileName(inputFile);
            Console.WriteLine($"Input: {inputFileBaseName}");
            //
            using var inputStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            var webm = new WebMStreamParser(inputStream);
            // 
            if (verbose)
            {
                // Display verbose element output
                var elements = webm.Descendants;
                foreach (var element in elements)
                {
                    var indent = new string('-', element.IdChain.Length - 1);
                    Console.WriteLine($"{indent}{element}");
                }
            }
            //

            // 
            if (fixDuration)
            {
                // Fix duration if not present
                var modified = webm.FixDuration();
                // webm.Modified will also be true if the WebM is modified
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
