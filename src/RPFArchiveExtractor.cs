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
    public static class RPFArchiveExtractor
    {
        public static async Task<List<(string name,string rpfname, byte[] data)>> ExtractRPFsFromArchivesAsync(List<string> archivePaths)
        {
            var results = new ConcurrentBag<(string name,string rpfname, byte[] data)>();

            var tasks = archivePaths.Select(path => Task.Run(() =>
            {
                try
                {
                    var archive = ArchiveFactory.Open(path);

                    // Step 1: Filter entries in parallel (safe)
                    var rpfEntries = archive.Entries
                        .AsParallel()
                        .Where(e => !e.IsDirectory && e.Key.EndsWith(".rpf", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    // Step 2: Sequentially extract each .rpf entry
                    foreach (var entry in rpfEntries)
                    {
                        var ms = new MemoryStream();
                        entry.WriteTo(ms); // Not thread-safe, so keep it sequential
                        results.Add((path,Path.GetFileName(entry.Key), ms.ToArray()));
                        ms.Dispose();
                    }
                    archive.Dispose();
                }
                catch (Exception ex)
                {
                    //Console.WriteLine($"Failed to process archive '{path}': {ex.Message}");
                }
            })).ToArray();

            await Task.WhenAll(tasks);

            return results.ToList();
        }
    }
}