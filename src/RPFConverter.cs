using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using CodeWalker.GameFiles;
using CodeWalker.Utils;
using SharpCompress.Archives;
using SharpCompress.Common.Rar;

namespace rpf2fivem.src
{
    public class RPFConverter<T> where T : RPFConverterBase, new()
    {
        Encoding utf8WithoutBom = new UTF8Encoding(false);

        public Type ExtractorType = typeof(T);

        private List<T> Extractors = new List<T>();

        //Should ideally also support the cancelation source since its likely called after convert, makes me think we should probably set it at class level somewhere 
        public void SaveToDisk(string finalPath)
        {
            var streamRoot = Path.Combine(finalPath, "stream");
            var metaRoot = Path.Combine(finalPath, "data"); //needs to be data lool

            if (Directory.Exists(finalPath))
                Directory.Delete(finalPath, true);

            Directory.CreateDirectory(streamRoot);
            Directory.CreateDirectory(metaRoot);

            if (!AllConvertersFinished())
            {
                Console.WriteLine("Converters are still running!");
                return;
            }

            Parallel.ForEach(Extractors, extractor =>
            {
                string uniqueID = Path.GetRandomFileName().Replace(".", "").Substring(0, 8);
                string extractorStreamPath = Path.Combine(streamRoot, uniqueID);
                string extractorMetaPath = Path.Combine(metaRoot, uniqueID);

                Directory.CreateDirectory(extractorStreamPath);
                Directory.CreateDirectory(extractorMetaPath);

                var streamFileNames = new ConcurrentDictionary<string, int>();
                var metaFileNames = new ConcurrentDictionary<string, int>();

                Parallel.ForEach(extractor.SVFS, streamFile =>
                {
                    string uniqueName = GetUniqueFileName(streamFile.Key, streamFileNames);
                    WriteFileFast(Path.Combine(extractorStreamPath, uniqueName), streamFile.Value);
                });

                Parallel.ForEach(extractor.MVFS, metaFile =>
                {
                    string uniqueName = GetUniqueFileName(metaFile.Key, metaFileNames);
                    WriteFileFast(Path.Combine(extractorMetaPath, uniqueName), metaFile.Value);
                });

                Console.WriteLine($"Saved files for extractor {extractor.RPFBasePath} in folder {uniqueID}");
            });
            
            var fxmanifest = Properties.Resources.fxmanifest_true;
            File.WriteAllText(Path.Combine(finalPath,"fxmanifest.lua"), fxmanifest, utf8WithoutBom);
        }

        public void WriteFileFast(string path, byte[] data)
        {
            const int BufferSize = 1048576; // 1 MB buffer for copying data chunks

            var fs = new FileStream(
                path,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                BufferSize,
                FileOptions.SequentialScan); // or FileOptions.WriteThrough for disk-level flush


            fs.Write(data, 0, data.Length);
            fs.Dispose();
        }

        private string GetUniqueFileName(string fileName, ConcurrentDictionary<string, int> nameTracker)
        {
            string baseName = Path.GetFileNameWithoutExtension(fileName);
            string ext = Path.GetExtension(fileName);
            int count = nameTracker.AddOrUpdate(fileName, 0, (_, old) => old + 1);

            return count == 0 ? fileName : $"{baseName}_{count}{ext}";
        }

        public bool AllConvertersFinished()
        {
            return Extractors.All(e => e.IsFinished);
        }

        public async Task<(int successCount, List<bool> results)> ConvertAsync(List<(string name,string rpfname, byte[] data)> data, CancellationToken cancellationToken = default)
        {
            Extractors.Clear();
            var zipCount = data.Count;
            var tasks = new List<Task<bool>>(zipCount);

            for (int i = 0; i < zipCount; i++)
            {
                var index = i; // Prevent closure issue, should be gone now i hate csharp

                var extractor = new T();
                extractor.RPFBasePath = data[index].rpfname;
                extractor.ArchivePath = data[index].name;
                Extractors.Add(extractor);
                tasks.Add(Task.Run(() =>
                {
                    cancellationToken.ThrowIfCancellationRequested();


                    var status = extractor.Process(data[index]);
                    extractor.IsFinished = status; //maybe we need more then bool in the future but for now its fine we can allways upgrade it to a struct for example.
                    return status;
                }, cancellationToken));
            }

            try
            {
                var results = await Task.WhenAll(tasks);

                int successCount = 0;
                foreach (var result in results)
                    if (result) successCount++;

                return (successCount, new List<bool>(results));
            }
            catch (OperationCanceledException)
            {
                //Console.WriteLine("Extraction was canceled.");
                return (0, new List<bool>());
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"An error occurred: {ex}");
                throw; // or return a failed result structure
            }
        }
    }




}
