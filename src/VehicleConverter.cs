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
    public class VehicleConverter : RPFConverterBase
    {


        static Dictionary<string, string[]> extensions = new Dictionary<string, string[]>()
        {
            { "meta",  new string[]{ ".meta", "clip_sets.xml" } },
            { "stream", new string[]{".ytd", ".yft", ".ydr" } }
        };

        public override bool Process((string name,string rpfname, byte[] data) rpfdata)
        {
            ////Console.WriteLine(rpfdata.data.Length.ToString());

            Main.LogAppend($"[Thread]:Converting:{Path.GetFileNameWithoutExtension(rpfdata.name)}");
            RpfFile rpf = new RpfFile(rpfdata.data, rpfdata.name, rpfdata.name);
            ExtractRPFData(rpf);
            Main.LogAppend($"[Thread]:Finished Converting:{Path.GetFileNameWithoutExtension(rpfdata.name)}");
            return true;
        }

        private void ExtractRPFData(RpfFile rpf)
        {
            using (var memoryStream = new MemoryStream(rpf.fData))
            using (var reader = new BinaryReader(memoryStream))
            {
                if (rpf.ScanStructure(null, null))
                {
                    ////Console.WriteLine(rpfdata.name + " passed the scanner.. ready for stage 2");
                    var entries = rpf.AllEntries;

                    foreach (var entry in entries)
                    {
                        if (entry.NameLower.EndsWith(".rpf"))
                        {
                            //Console.WriteLine("Found a rpf: " + entry.NameLower);
                            using (var memoryStream2 = new MemoryStream(rpf.fData))
                            using (var reader2 = new BinaryReader(memoryStream2))
                            {
                                if (entry is RpfBinaryFileEntry)
                                {
                                    RpfBinaryFileEntry binentry = entry as RpfBinaryFileEntry;
                                    byte[] data = rpf.ExtractFileBinary(binentry, reader2);
                                    var subRpf = new RpfFile(data, entry.NameLower, rpf.Path);
                                    ExtractRPFData(subRpf);
                                }
                            }
                        }
                        else if (entry is RpfBinaryFileEntry)
                        {
                            RpfBinaryFileEntry binentry = entry as RpfBinaryFileEntry;
                            byte[] data = rpf.ExtractFileBinary(binentry, reader);

                            if (extensions["meta"].Any(ext => entry.NameLower.EndsWith(ext)))
                            {
                                MVFS[entry.NameLower] = data;
                            }
                            else if (extensions["stream"].Any(ext => entry.NameLower.EndsWith(ext)))
                            {
                                SVFS[entry.NameLower] = data;
                            }
                        }

                        else if (entry is RpfResourceFileEntry reSentry)
                        {
                            byte[] fileData = rpf.ExtractFileResource(reSentry, reader);
                            fileData = ResourceBuilder.Compress(fileData); //not completely ideal to recompress it... for one it will be slow thats for sure we should just swap it at some point
                            fileData = ResourceBuilder.AddResourceHeader(reSentry, fileData);

                            if (extensions["meta"].Any(ext => entry.NameLower.EndsWith(ext)))
                            {
                                MVFS[entry.NameLower] = fileData;
                            }
                            else if (extensions["stream"].Any(ext => entry.NameLower.EndsWith(ext)))
                            {
                                SVFS[entry.NameLower] = fileData;
                                if (entry.NameLower.EndsWith(".ytd"))
                                {


                                    if (!entry.NameLower.EndsWith("+hi", StringComparison.OrdinalIgnoreCase))
                                    {
                                        string baseName = entry.NameLower.Remove(entry.NameLower.Length - 4); // Remove .ytd extension
                                        string yftPath = baseName + ".yft";
                                        bool hasMatchingYft = SVFS.ContainsKey(yftPath);

                                        if (hasMatchingYft)
                                        {
                                            //LogAppend("[CodeWalker] Located streaming hash name with matching .yft file: " + baseName);
                                            //Main.LogAppend($"[Thread]:Found Model name for:{ArchivePath} : {baseName}");
                                            HelperScriptRegistry.SetVehicleBaseName(ArchivePath, baseName); //TODO: i have a bad feeling right now, this will be prone to race conditions i feel like
                                        }
                                    }
                                }
                            }
                        }

                        /*
                        if (!entry.NameLower.EndsWith(".rpf"))
                        {
                            if (entry is RpfBinaryFileEntry)
                            {
                                HandleRPFBinaryFileEntry(rpf, reader, entry, Path.GetFullPath(entry.Path));
                            }
                            else if (entry is RpfResourceFileEntry)
                            {
                                RpfResourceFileEntry reSentry = entry as RpfResourceFileEntry;
                                byte[] data = rpf.ExtractFileResource(reSentry, reader);
                                data = ResourceBuilder.Compress(data); //not completely ideal to recompress it... for one it will be slow thats for sure we should just swap it at some point
                                data = ResourceBuilder.AddResourceHeader(reSentry, data);

                                if (data == null)
                                {
                                    if (reSentry.FileSize == 0)
                                    {
                                        //LogAppend("[CodeWalker] Resource (" + entry.Path + ") filesize was empty!");
                                    }
                                }
                                else if (data.Length == 0)
                                {
                                    //LogAppend("[CodeWalker] Decompressed output (" + entry.Path + ") was empty!");
                                }
                                else
                                {
                                    foreach (KeyValuePair<string, string[]> extensionMap in extensions)
                                    {
                                        foreach (string extension in extensionMap.Value)
                                        {
                                            if (entry.NameLower.EndsWith(extension))
                                            {

                                                if (extension.Equals(".ytd"))
                                                {
                                                    if (false) //todo add check to mainform if textures should be compressed.
                                                    {
                                                        RpfFileEntry rpfentry = entry as RpfFileEntry;

                                                        byte[] ytddata = rpfentry.File.ExtractFile(rpfentry);

                                                        YtdFile ytd = new YtdFile();
                                                        ytd.Load(ytddata, rpfentry);

                                                        Dictionary<uint, Texture> Dicts = new Dictionary<uint, Texture>();

                                                        bool somethingResized = false;
                                                        foreach (KeyValuePair<uint, Texture> texture in ytd.TextureDict.Dict)
                                                        {
                                                            if (texture.Value.Width > 512) // Only resize if it is greater than 1440p
                                                            {
                                                                somethingResized = ResizeTexture(Path.GetFullPath(rpfdata.name), Dicts, somethingResized, texture); //brought to you by strg+. -> extract method
                                                            }
                                                            else
                                                            {
                                                                Dicts.Add(texture.Key, texture.Value);
                                                            }
                                                        }

                                                        if (!somethingResized)
                                                        {
                                                            //LogAppend("[CodeWalker] No textures in dictionary were resized, all under 512 pixels.");
                                                        }

                                                        TextureDictionary dictionary = new TextureDictionary();
                                                        dictionary.Textures = new ResourcePointerList64<Texture>();
                                                        dictionary.TextureNameHashes = new ResourceSimpleList64_uint();
                                                        dictionary.Textures.data_items = Dicts.Values.ToArray();
                                                        dictionary.TextureNameHashes.data_items = Dicts.Keys.ToArray();

                                                        dictionary.BuildDict();
                                                        ytd.TextureDict = dictionary;

                                                        byte[] resizedYtdData = ytd.Save();
                                                        vfs[Path.GetFullPath(entry.NameLower)] = data;
                                                        //File.WriteAllBytes(Path.GetFullPath(Path.Combine(directoryOffset, entry.NameLower)), resizedYtdData);

                                                        //Console.WriteLine($"Found YTD Data {entry.NameLower}: " + resizedYtdData.Length);

                                                        // LogAppend("[CodeWalker] Resized texture dictionary (ytd) " + entry.NameLower + ".");
                                                        break;
                                                    }
                                                }
                                                //Console.WriteLine($"Found YTD Data {entry.NameLower}: " + data.Length);
                                                vfs[Path.GetFullPath(entry.NameLower)] = data;
                                                //File.WriteAllBytes(Path.GetFullPath(Path.Combine(directoryOffset, entry.NameLower)), data);
                                                break;
                                            }
                                        }
                                    }

                                    if (entry.NameLower.EndsWith(".ytd"))
                                    {
                                        //Console.WriteLine($"Found YTD Data {entry.NameLower}: " + data.Length);

                                        if (!entry.NameLower.EndsWith("+hi", StringComparison.OrdinalIgnoreCase))
                                        {
                                            string baseName = entry.NameLower.Remove(entry.NameLower.Length - 4); // Remove .ytd extension
                                            string yftPath = Path.GetFullPath(entry.NameLower);
                                            bool hasMatchingYft = vfs.ContainsKey(yftPath);

                                            if (hasMatchingYft)
                                            {
                                                //TODO: Register in some central model registry.
                                                //LogAppend("[CodeWalker] Located streaming hash name with matching .yft file: " + baseName);
                                                //Console.WriteLine("Found yft model:" + baseName);
                                                //models.Add((baseName, SingleEnviromentFolder)); //TODO: i have a bad feeling right now, this will be prone to race conditions i feel like
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                RpfBinaryFileEntry binaryentry = entry as RpfBinaryFileEntry;
                                byte[] data = rpf.ExtractFileBinary(binaryentry, reader);

                                if (data != null)
                                {
                                    RpfFile subRPF = new RpfFile(data, entry.NameLower, entry.Path);

                                    if (subRPF.ScanStructure(null, null))
                                    {
                                        ExtractRPFData(rpfdata, subRPF);
                                    }
                                }
                                else
                                {
                                    //Console.WriteLine("Failed to extract: " + rpfdata.name);
                                }

                                //File.Delete(directoryOffset + entry.NameLower);
                            }
                        }
                              */
                    }
                }
            }
        }

        private bool ResizeTexture(string directoryOffset, Dictionary<uint, Texture> Dicts, bool somethingResized, KeyValuePair<uint, Texture> texture)
        {
            byte[] dds = DDSIO.GetDDSFile(texture.Value);
            File.WriteAllBytes("./NConvert/" + texture.Value.Name + ".dds", dds);

            try
            {
                using (Process SizingProcess = new Process())
                {
                    if (!File.Exists(@"./NConvert/nconvert.exe"))
                    {
                        throw new ArgumentException("NConvert binaries are null or non existant.");
                    }
                    if (string.IsNullOrEmpty(texture.Value?.Name))
                    {
                        throw new ArgumentException("Texture name is null or empty.");
                    }

                    SizingProcess.StartInfo.FileName = @"./NConvert/nconvert.exe";
                    SizingProcess.StartInfo.Arguments = $"-out dds -resize 50% 50% -overwrite \"./NConvert/{texture.Value.Name}.dds\"";
                    SizingProcess.StartInfo.UseShellExecute = false;
                    SizingProcess.StartInfo.CreateNoWindow = true;
                    SizingProcess.StartInfo.RedirectStandardOutput = true;
                    SizingProcess.StartInfo.RedirectStandardError = true;

                    SizingProcess.Start();
                    SizingProcess.WaitForExit();

                    if (SizingProcess.ExitCode != 0)
                    {
                        string error = SizingProcess.StandardError.ReadToEnd();
                        throw new InvalidOperationException($"Process exited with code {SizingProcess.ExitCode}. Error: {error}");
                    }
                }
            }
            catch (Exception ex)
            {
                // If the process fails, we can just add the original texture to the dictionary
                Dicts.Add(texture.Key, texture.Value);
                File.Delete(directoryOffset + texture.Value.Name + ".dds");

                // Log the error
                //WarningAppend($"[NConvert] Failed to resize texture ({texture.Value.Name}) to 50%!");
                //WarningAppend($"[NConvert] Binary returned the error: {ex.Message}");
                return false;
            }

            File.Move("./NConvert/" + texture.Value.Name + ".dds", directoryOffset + texture.Value.Name + ".dds");

            byte[] resizedData = File.ReadAllBytes(directoryOffset + texture.Value.Name + ".dds");
            Texture resizedTex = DDSIO.GetTexture(resizedData);
            resizedTex.Name = texture.Value.Name;
            Dicts.Add(texture.Key, resizedTex);

            File.Delete(directoryOffset + texture.Value.Name + ".dds");
            somethingResized = true;
            return somethingResized;
        }

        private void HandleRPFBinaryFileEntry(RpfFile rpf, BinaryReader reader, RpfEntry entry, string dir)
        {
            RpfBinaryFileEntry binentry = entry as RpfBinaryFileEntry;
            byte[] data = rpf.ExtractFileBinary(binentry, reader);
            if (data == null)
            {
                if (binentry.FileSize == 0)
                {
                    //LogAppend("[CodeWalker] Invalid binary filesize!");
                }
                else
                {
                    //LogAppend("[CodeWalker] Binary data is null");
                }
            }
            else if (data.Length == 0)
            {
                //LogAppend("[CodeWalker] Decompressed output " + entry.Path + " was empty!");
            }
            else
            {
                if(entry == null)
                {
                    //Console.WriteLine("A entry wasent a RpfBinaryFileEntry");
                    return;
                }
                ////Console.WriteLine(data.Length.ToString()); //Further processable.
                //File.WriteAllBytes(Path.GetFullPath(Path.Combine(directoryOffset, entry.NameLower)), data);
                //vfs[Path.Combine(dir, entry.Name)] = data; //write to vfs instead
               // //Console.WriteLine("Added to vfs: " + Path.Combine(dir, entry.Name));
            }
        }
    }
}