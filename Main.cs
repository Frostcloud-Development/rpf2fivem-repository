using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeWalker.GameFiles;
using CodeWalker.Utils;
using Sentry;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace rpf2fivem
{

    public partial class Main : Form
    {

        // GLOBALS

        int currentQueue = 1;
        Random rnd = new Random();
        int convertFromFolder_resname;
        string combinedFolderString = "";
        //string LatestStreamingName = ""; this is prone for race conditions

        string CurrentBuildName = "helper-scripts@4.3.1-patch5";
        string LatestBuildName = "";
        bool ApplicationSafeShutdown = false;

        bool QbCoreHelperState = false;
        bool QbxCoreHelperState = false;
        bool CombineResourceState = true;


        public struct VehicleData
        { 
            public string InternalReference { get; set; }
            public string Name { get; set; }
            public string Brand { get; set; }
            public string Model { get; set; }
            public int Price { get; set; }
            public string Category { get; set; }
            public string Type { get; set; }
            public string Hash { get; set; }
        }

        public struct StructureFolders
        {
            public string streamFolder;
            public string dataFolder;
        }

        static List<VehicleData> vehicleArray = new List<VehicleData>();
        static Dictionary<string, string> modelNames = new Dictionary<string, string>();
        static Dictionary<string, string[]> extensions = new Dictionary<string, string[]>()
        {
            { "meta",  new string[]{ ".meta", "clip_sets.xml" } },
            { "stream", new string[]{".ytd", ".yft", ".ydr" } }
        };

        public Main()
        {

            //var task = GetLatestReleaseName("OWNER", "REPO");
            //task.Wait(); // since Main can't be async in 7.3

            //LatestBuildName = task.Result;

            InitializeComponent();
        }

        static Task<string> GetLatestReleaseName(string owner, string repo)
        {
            return Task.Run(async () =>
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("CSharpApp", "1.0"));

                string url = $"https://api.github.com/repos/Avenze/rpf2fivem-repository/releases/latest";
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string jsonString = await response.Content.ReadAsStringAsync();

                using (JsonDocument doc = JsonDocument.Parse(jsonString))
                {
                    JsonElement root = doc.RootElement;
                    if (root.TryGetProperty("name", out JsonElement nameProp))
                    {
                        return nameProp.GetString();
                    }
                    return "No name found";
                }
            });
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            // Validate if a log exists, if not, create one!
            if (!Directory.Exists(@"./logs"))
            {
                Directory.CreateDirectory(@"logs");
            }
            if (!File.Exists(@"./logs/latest.log"))
            {
                FileStream fs = File.Create(@"./logs/latest.log");
                fs.Close();
            }

            if (Directory.Exists("cache"))
            {
                Directory.Delete("cache", true);
                Directory.CreateDirectory("cache");
            }

            if (Directory.Exists("combinercache"))
            {
                Directory.Delete("combinercache", true);
                Directory.CreateDirectory("combinercache");
            }



            // Minor setup
            this.ActiveControl = label1; // prevent random textbox focus
            fivemresname_tb.Text = rnd.Next(2147483647).ToString();

            LogAppend("rpf2fivem");
            LogAppend("---------------");
            LogAppend("Developed by avenze, _jupe_ and vscorpio (https://github.com/Avenze, https://github.com/xVice1337 and https://github.com/vscorpio)");
            LogAppend("GitHub repository: https://github.com/Avenze/rpf2fivem-repository");
            LogAppend("---------------");

            LogAppend("GTA5-Mods links must look like this: ");
            LogAppend("https://files.gta5-mods.com/uploads/XXXCARNAMEXXXX/XXXCARNAMEXXXX.zip");
            LogAppend("Links must be DIRECT link else they won't download!");
            LogAppend("");

            // Validate if NConvert exists in the directory
            if (!Directory.Exists("./NConvert"))
            {
                // add warning if the user hasn't installed NConvert properly
                WarningAppend("[NConvert] It seems like you haven't installed NConvert, please follow");
                WarningAppend("[NConvert] the installation instructions on the GitHub Repository or the forum thread.");

                CompressCheck.Checked = false;
                CompressCheck.Enabled = false;
            }
        }

        // Helper Functions

        public void LogAppend(string text)
        {
            if (log.InvokeRequired)
            {
                log.Invoke(new Action(() => {
                    log.AppendText(text + Environment.NewLine);
                }));
            }
            else
            {
                log.AppendText(text + Environment.NewLine);
            }

            StatusHandler(text);
            LogFile("[INFO] " + text);
        }

        public void WarningAppend(string text)
        {
            if (log.InvokeRequired)
            {
                log.Invoke(new Action(() => {
                    log.AppendText(text + Environment.NewLine);
                }));
            }
            else
            {
                log.AppendText(text + Environment.NewLine);
            }

            StatusHandler(text);
            LogFile("[WARNING] " + text);
        }

        public void ErrorAppend(string text)
        {
            if (log.InvokeRequired)
            {
                log.Invoke(new Action(() => {
                    log.AppendText("[Error] An error occurred during execution, stacktrace has been logged to /logs/latest.log, please submit to GitHub Issues page." + Environment.NewLine);
                }));
            }
            else
            {
                log.AppendText("[Error] An error occurred during execution, stacktrace has been logged to /logs/latest.log, please submit to GitHub Issues page." + Environment.NewLine);
            }

            LogFile("[ERROR] " + text);
        }

        public static class ProgramLogger
        {
            public static void LogAppend(string text)
            {
                if (Application.OpenForms["Main"] is Main mainForm)
                {
                    mainForm.Invoke((MethodInvoker)delegate
                    {
                        mainForm.log.AppendText(text + Environment.NewLine);
                        mainForm.StatusHandler(text);
                        mainForm.LogFile("[INFO] " + text);
                    });
                }
            }
            public static void ErrorAppend(string text)
            {
                if (Application.OpenForms["Main"] is Main mainForm)
                {
                    mainForm.Invoke((MethodInvoker)delegate
                    {
                        mainForm.log.AppendText("[ERROR] An error occoured during execution, stacktrace has been logged to /logs/latest.log, please submit to GitHub Issues page.");
                        mainForm.LogFile("[ERROR] " + text);
                    });
                }
            }
        }

        private readonly object logFileLock = new object();

        public void LogFile(string text)
        {
            try
            {
                string currentDate = DateTime.Now.ToString(@"MM\/dd\/yyyy\ hh\:mm\:ss");

                lock (logFileLock)
                {
                    using (TextWriter tw = new StreamWriter(@"./logs/latest.log", append: true))
                    {
                        tw.WriteLine("[" + currentDate + "] " + text + Environment.NewLine);
                        tw.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                // Use Invoke to avoid cross-thread issues in ErrorAppend if it touches UI
                InvokeIfRequired(() => ErrorAppend("[Worker] Failed to write log to file. Stacktrace: " + ex));
            }
        }

        private void StatusHandler(string status)
        {
            InvokeIfRequired(() => { tsStatus.Text = "Status: " + status; });

        }

        private void QueueHandler(int current, int total)
        {
            InvokeIfRequired(() =>
            {
                tsQueue.Text = "Queue: " + current + "/" + total;
            });
        }

        async Task AsyncFileDownload(string url)
        {
            string file = System.IO.Path.GetFileName(url);
            WebClient wb = new WebClient();
            await wb.DownloadFileTaskAsync(new Uri(url), file);
        }

        private void HideShellCmd(string cmd)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C " + cmd;
            process.StartInfo = startInfo;
            process.Start();
        }

      

        // SharpCompress Functions
        private void unZip(string archivePath, string extractionDirectory)
        {
            try
            {
                // Directory.CreateDirectory wurde bereits in UnpackSpecificArchive aufgerufen
                using (var archive = ZipArchive.Open(archivePath))
                {
                    InvokeIfRequired(() => LogAppend($"[SharpCompress] Entpacke ZIP-Archiv: {Path.GetFileName(archivePath)}"));
                    foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
                    {
                        entry.WriteToDirectory(extractionDirectory, new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true // Vorsicht: Überschreibt vorhandene Dateien ohne Nachfrage
                        });
                    }
                    InvokeIfRequired(() => LogAppend($"[SharpCompress] ZIP-Archiv {Path.GetFileName(archivePath)} erfolgreich entpackt."));
                }
            }
            catch (Exception ex)
            {
                InvokeIfRequired(() => ErrorAppend($"[SharpCompress] Fehler beim Entpacken von ZIP-Archiv {Path.GetFileName(archivePath)}: {ex.Message}"));
                // SentrySdk.CaptureException(ex);
            }
        }

        private void unRar(string archivePath, string extractionDirectory)
        {
            try
            {
                // Directory.CreateDirectory wurde bereits in UnpackSpecificArchive aufgerufen
                using (var archive = RarArchive.Open(archivePath))
                {
                    InvokeIfRequired(() => LogAppend($"[SharpCompress] Entpacke RAR-Archiv: {Path.GetFileName(archivePath)}"));
                    foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
                    {
                        entry.WriteToDirectory(extractionDirectory, new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                    InvokeIfRequired(() => LogAppend($"[SharpCompress] RAR-Archiv {Path.GetFileName(archivePath)} erfolgreich entpackt."));
                }
            }
            catch (Exception ex)
            {
                InvokeIfRequired(() => ErrorAppend($"[SharpCompress] Fehler beim Entpacken von RAR-Archiv {Path.GetFileName(archivePath)}: {ex.Message}"));
                // SentrySdk.CaptureException(ex);
            }
        }

        private void unSeven(string archivePath, string extractionDirectory)
        {
            try
            {
                // Directory.CreateDirectory wurde bereits in UnpackSpecificArchive aufgerufen
                using (var archive = SevenZipArchive.Open(archivePath))
                {
                    InvokeIfRequired(() => LogAppend($"[SharpCompress] Entpacke 7z-Archiv: {Path.GetFileName(archivePath)}"));
                    foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
                    {
                        entry.WriteToDirectory(extractionDirectory, new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                    InvokeIfRequired(() => LogAppend($"[SharpCompress] 7z-Archiv {Path.GetFileName(archivePath)} erfolgreich entpackt."));
                }
            }
            catch (Exception ex)
            {
                InvokeIfRequired(() => ErrorAppend($"[SharpCompress] Fehler beim Entpacken von 7z-Archiv {Path.GetFileName(archivePath)}: {ex.Message}"));
                // SentrySdk.CaptureException(ex);
            }
        }


        public void UnpackSpecificArchive(string archiveToUnpackPath, string specificUnpackDirectory)
        {
            if (string.IsNullOrEmpty(archiveToUnpackPath) || !File.Exists(archiveToUnpackPath))
            {
                InvokeIfRequired(() => ErrorAppend($"[SharpCompress] Fehler: Archivdatei nicht gefunden unter '{archiveToUnpackPath}'."));
                return;
            }

            if (string.IsNullOrEmpty(specificUnpackDirectory))
            {
                InvokeIfRequired(() => ErrorAppend($"[SharpCompress] Fehler: Kein Zielverzeichnis für die Extraktion von '{Path.GetFileName(archiveToUnpackPath)}' angegeben."));
                return;
            }

            // Sicherstellen, dass das Zielverzeichnis existiert
            try
            {
                Directory.CreateDirectory(specificUnpackDirectory);
            }
            catch (Exception ex)
            {
                InvokeIfRequired(() => ErrorAppend($"[SharpCompress] Fehler beim Erstellen des Zielverzeichnisses '{specificUnpackDirectory}': {ex.Message}"));
                // SentrySdk.CaptureException(ex);
                return;
            }

            string extension = Path.GetExtension(archiveToUnpackPath).ToLowerInvariant();
            InvokeIfRequired(() => LogAppend($"[SharpCompress] Versuche Archiv '{Path.GetFileName(archiveToUnpackPath)}' ({extension}) nach '{specificUnpackDirectory}' zu entpacken..."));

            switch (extension)
            {
                case ".rar":
                    unRar(archiveToUnpackPath, specificUnpackDirectory);
                    break;
                case ".zip":
                    unZip(archiveToUnpackPath, specificUnpackDirectory);
                    break;
                case ".7z":
                    unSeven(archiveToUnpackPath, specificUnpackDirectory);
                    break;
                default:
                    InvokeIfRequired(() => WarningAppend($"[SharpCompress] Unsupported archive type: '{extension}' for file: '{Path.GetFileName(archiveToUnpackPath)}'."));
                    break;
            }
        }

        // Decompression Functions
        private void universalCacheUnpack(string guid)
        {
            string rarfileExtension = "*.rar";
            var basePath = Path.GetFullPath(Path.Combine("cache", guid));
            string[] rarFiles = Directory.GetFiles(basePath, rarfileExtension, SearchOption.AllDirectories);

            foreach (var item in rarFiles)
            {
                LogAppend("[SharpCompress] Found .RAR archive, decompressing...");
                UnpackSpecificArchive(item, Path.Combine(basePath, "unpack"));
               // unRar(Path.Combine("cache", Path.GetFileName(item)));
            }

            string zipfileExtension = "*.zip";
            string[] zipFiles = Directory.GetFiles(basePath, zipfileExtension, SearchOption.AllDirectories);

            foreach (var item in zipFiles)
            {
                LogAppend("[SharpCompress] Found .ZIP archive, decompressing...");
                UnpackSpecificArchive(item, Path.Combine(basePath, "unpack"));
            }

            string sevenfileExtension = "*.7z";
            string[] sevenFiles = Directory.GetFiles(basePath, sevenfileExtension, SearchOption.AllDirectories);

            foreach (var item in sevenFiles)
            {
                LogAppend("[SharpCompress] Found .7Z archive, decompressing...");
                UnpackSpecificArchive(item, Path.Combine(basePath, "unpack"));
            }

            return;
        }

        List<(string,string)> models = new List<(string,string)>();

        // Unpacking Functions
        private void RpfUnpack(string resname,string guid, string SingleEnvFolder)
        {
            string rpfExtension = "*.rpf";
            var gpath = Path.GetFullPath(Path.Combine("cache", guid, "unpack"));
            //MessageBox.Show(gpath);
            string[] rpfFiles = Directory.GetFiles(gpath, rpfExtension, SearchOption.AllDirectories);
            foreach (var item in rpfFiles)
            {
                RpfFile rpf = new RpfFile(item, item);
                LogAppend("[CodeWalker] Unpacking " + item + "...");

                if (rpf.ScanStructure(null, null))
                {
                    ExtractFilesInRPF(rpf, Path.GetFullPath(Path.Combine("cache", guid, "rpfunpack")), resname, SingleEnvFolder);
                }
            }

            if (rpfFiles.Length == 0)
            {
                WarningAppend("[CodeWalker] Vehicle (" + resname + ") is incompatible, no .rpf file was found.");
            }
        }

        private void ExtractFilesInRPF(RpfFile rpf, string directoryOffset, string resName, string SingleEnviromentFolder)
        {
            using (BinaryReader br = new BinaryReader(File.OpenRead(rpf.GetPhysicalFilePath())))
            {
                foreach (RpfEntry entry in rpf.AllEntries)
                {
                    if (!entry.NameLower.EndsWith(".rpf")) //don't try to extract rpf's, they will be done separately..
                    {
                        if (entry is RpfBinaryFileEntry)
                        {
                            RpfBinaryFileEntry binentry = entry as RpfBinaryFileEntry;
                            byte[] data = rpf.ExtractFileBinary(binentry, br);
                            if (data == null)
                            {
                                if (binentry.FileSize == 0)
                                {
                                    LogAppend("[CodeWalker] Invalid binary filesize!");
                                }
                                else
                                {
                                    LogAppend("[CodeWalker] Binary data is null");
                                }
                            }
                            else if (data.Length == 0)
                            {
                                LogAppend("[CodeWalker] Decompressed output " + entry.Path + " was empty!");
                            }
                            else
                            {
                                File.WriteAllBytes(Path.GetFullPath(Path.Combine(directoryOffset, entry.NameLower)), data);
                            }
                        }
                        else if (entry is RpfResourceFileEntry)
                        {
                            RpfResourceFileEntry reSentry = entry as RpfResourceFileEntry;
                            byte[] data = rpf.ExtractFileResource(reSentry, br);
                            data = ResourceBuilder.Compress(data); //not completely ideal to recompress it... for one it will be slow thats for sure we should just swap it at some point
                            data = ResourceBuilder.AddResourceHeader(reSentry, data);

                            if (data == null)
                            {
                                if (reSentry.FileSize == 0)
                                {
                                    LogAppend("[CodeWalker] Resource (" + entry.Path + ") filesize was empty!");
                                }
                            }
                            else if (data.Length == 0)
                            {
                                LogAppend("[CodeWalker] Decompressed output (" + entry.Path + ") was empty!");
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
                                                if (CompressCheck.Checked == true)
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
                                                            somethingResized = ResizeTexture(directoryOffset, Dicts, somethingResized, texture); //brought to you by strg+. -> extract method
                                                        }
                                                        else
                                                        {
                                                            Dicts.Add(texture.Key, texture.Value);
                                                        }
                                                    }

                                                    if (!somethingResized)
                                                    {
                                                        LogAppend("[CodeWalker] No textures in dictionary were resized, all under 512 pixels.");
                                                    }

                                                    TextureDictionary dictionary = new TextureDictionary();
                                                    dictionary.Textures = new ResourcePointerList64<Texture>();
                                                    dictionary.TextureNameHashes = new ResourceSimpleList64_uint();
                                                    dictionary.Textures.data_items = Dicts.Values.ToArray();
                                                    dictionary.TextureNameHashes.data_items = Dicts.Keys.ToArray();

                                                    dictionary.BuildDict();
                                                    ytd.TextureDict = dictionary;

                                                    byte[] resizedYtdData = ytd.Save();
                                                    File.WriteAllBytes(Path.GetFullPath(Path.Combine(directoryOffset,entry.NameLower)), resizedYtdData);

                                                    LogAppend("[CodeWalker] Resized texture dictionary (ytd) " + entry.NameLower + ".");
                                                    break;
                                                }
                                            }

                                            File.WriteAllBytes(Path.GetFullPath(Path.Combine(directoryOffset,entry.NameLower)), data);
                                            break;
                                        }
                                    }
                                }

                                if (entry.NameLower.EndsWith(".ytd"))
                                {
                                    if (!entry.NameLower.EndsWith("+hi", StringComparison.OrdinalIgnoreCase))
                                    {
                                        string baseName = entry.NameLower.Remove(entry.NameLower.Length - 4); // Remove .ytd extension
                                        string yftPath = Path.GetFullPath(Path.Combine(directoryOffset,baseName + ".yft"));
                                        bool hasMatchingYft = File.Exists(yftPath);

                                        if (hasMatchingYft)
                                        {
                                            LogAppend("[CodeWalker] Located streaming hash name with matching .yft file: " + baseName);
                                            models.Add((baseName, SingleEnviromentFolder)); //TODO: i have a bad feeling right now, this will be prone to race conditions i feel like
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        RpfBinaryFileEntry binaryentry = entry as RpfBinaryFileEntry;
                        byte[] data = rpf.ExtractFileBinary(binaryentry, br);
                        var patthh = Path.GetFullPath(Path.Combine(directoryOffset, entry.NameLower));
                        File.WriteAllBytes(patthh, data);

                        RpfFile subRPF = new RpfFile(patthh, patthh);

                        if (subRPF.ScanStructure(null, null))
                        {
                            ExtractFilesInRPF(subRPF, Path.GetFullPath(directoryOffset), resName, SingleEnviromentFolder);
                        }
                        //File.Delete(directoryOffset + entry.NameLower);
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
                WarningAppend($"[NConvert] Failed to resize texture ({texture.Value.Name}) to 50%!");
                WarningAppend($"[NConvert] Binary returned the error: {ex.Message}");
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

        // Configuration Helper Scripts
        private VehicleData? FindVehicleByInternalReference(string InternalReference)
        {
            return vehicleArray.FirstOrDefault(vehicle => vehicle.InternalReference == InternalReference);
        }

        private void UpdateVehicleData(string InternalReference, string StreamingHash)
        {
            // Find the index of the VehicleData in the vehicleArray  
            int index = vehicleArray.FindIndex(vehicle => vehicle.InternalReference == InternalReference);

            if (index != -1)
            {
                // Update the model and hash of the VehicleData  
                var updatedVehicle = vehicleArray[index];
                updatedVehicle.Model = StreamingHash;
                updatedVehicle.Hash = StreamingHash;

                // Replace the VehicleData in the vehicleArray  
                vehicleArray[index] = updatedVehicle;

                LogAppend($"[HelperScripts] Updated VehicleData for InternalReference: {InternalReference} with StreamingHash: {StreamingHash}");
            }
            else
            {
                WarningAppend($"[HelperScripts] No VehicleData found for InternalReference: {InternalReference}");
            }
        }

        private void InvokeQueueVehicleHelper(string fileName)
        {
            if (QbxCoreHelperState || QbCoreHelperState || AddonCarSpawnerHelperState)
            {
                var VehicleDataObject = InvokeHelperQuestionnaire(fileName);
                vehicleArray.Add(new VehicleData
                {
                    InternalReference = fivemresname_tb.Text,
                    Name = VehicleDataObject.Name,
                    Brand = VehicleDataObject.Brand,
                    Model = VehicleDataObject.Model,
                    Price = VehicleDataObject.Price,
                    Category = VehicleDataObject.Category,
                    Type = VehicleDataObject.Type,
                    Hash = VehicleDataObject.Hash
                });
            }
        }

        bool AddonCarSpawnerHelperState = false;

        void AddVehicleToGroupedLuaFile(string filePath, VehicleData vehicle, string modelName)
        {
            // Read existing lines (or create new list if file doesn't exist)
            List<string> lines = File.Exists(filePath)
                ? File.ReadAllLines(filePath).ToList()
                : new List<string> { "config.cars = {", "}" };

            string brandKey = $"['{vehicle.Brand}']";
            string vehicleEntry = $"        {{ name = '{vehicle.Name}', model = '{modelName}' }},";

            int brandStartIndex = lines.FindIndex(l => l.TrimStart().StartsWith(brandKey));
            if (brandStartIndex != -1)
            {
                // Insert just before the closing brace of this brand
                int insertIndex = brandStartIndex + 1;
                while (insertIndex < lines.Count && !lines[insertIndex].TrimStart().StartsWith("},"))
                    insertIndex++;
                lines.Insert(insertIndex, vehicleEntry);
            }
            else
            {
                // Insert new brand section before the final closing brace
                int closingIndex = lines.FindLastIndex(l => l.Trim() == "}");
                lines.Insert(closingIndex, $"    {brandKey} = {{");
                lines.Insert(closingIndex + 1, vehicleEntry);
                lines.Insert(closingIndex + 2, "    },");
                lines.Insert(closingIndex + 3, "");
            }

            LogAppend("[AddonSpawnerGenerator]:" + vehicle.Model);
            File.WriteAllLines(filePath, lines);
        }

        private void InvokeHelperScripts(string StreamingModelName, string InternalReference)
        {
            if (QbxCoreHelperState || QbCoreHelperState || AddonCarSpawnerHelperState)
            {
                try
                {
                    string qbxCoreFilePath = "qbxcore_vehicles.txt";
                    string qbCoreFilePath = "qbcore_vehicles.txt";
                    string addoncarspawnerconfig = "config.lua";
        
                    var vehicleData = FindVehicleByInternalReference(InternalReference);
                    UpdateVehicleData(InternalReference, StreamingModelName);

                    if (vehicleData.Value.Name == null)
                    {
                        WarningAppend($"[HelperScripts] No vehicle data found for InternalMemoryReference: {InternalReference}");
                        WarningAppend($"[HelperScripts] Did you forget to enable the helpers before you created the queue?");
                        return;
                    }
                    LogAppend("[HelperScripts]:" + vehicleData.Value.Model);
                    if (AddonCarSpawnerHelperState)
                    {
                        AddVehicleToGroupedLuaFile(addoncarspawnerconfig, vehicleData.Value, StreamingModelName);
                        LogAppend("[HelperScripts] config.lua file updated successfully.");
                    }

                    if (QbxCoreHelperState)
                    {
                        using (StreamWriter writer = new StreamWriter(qbxCoreFilePath, append: true))
                        {
                            writer.WriteLine($"{StreamingModelName} = {{");
                            writer.WriteLine($"    name = '{vehicleData.Value.Name}',");
                            writer.WriteLine($"    brand = '{vehicleData.Value.Brand}',");
                            writer.WriteLine($"    model = '{StreamingModelName}',");
                            writer.WriteLine($"    price = {vehicleData.Value.Price},");
                            writer.WriteLine($"    category = '{vehicleData.Value.Category}',");
                            writer.WriteLine($"    type = '{vehicleData.Value.Type}',");
                            writer.WriteLine($"    hash = '{StreamingModelName}',");
                            writer.WriteLine("},");
                        }
                        LogAppend("[HelperScripts] qbxcore_vehicles.txt file updated successfully.");
                    }

                    if (QbCoreHelperState)
                    {
                        using (StreamWriter writer = new StreamWriter(qbCoreFilePath, append: true))
                        {
                            writer.WriteLine($"{{ model = '{StreamingModelName}', name = '{vehicleData.Value.Name}', brand = '{vehicleData.Value.Brand}', price = {vehicleData.Value.Price}, category = '{vehicleData.Value.Category}', type = '{vehicleData.Value.Type}', shop = 'none' }},");
                        }
                        LogAppend("[HelperScripts] qbcore_vehicles.txt file updated successfully.");
                    }
                }
                catch (Exception ex)
                {
                    ErrorAppend($"[HelperScripts] Failed to generate helper files. Error: {ex.Message}");
                }
            }
        }

        public VehicleData InvokeHelperQuestionnaire(string fileName)
        {
            HelperWindow helperForm = new HelperWindow(fileName);
            helperForm.ShowDialog();

            if (helperForm.InputFinishedFlag)
            {
                var data = new VehicleData
                {
                    Name = helperForm.VehicleName,
                    Brand = helperForm.VehicleBrand,
                    Model = "",
                    Price = int.Parse(helperForm.VehiclePrice),
                    Category = helperForm.VehicleCategory,
                    Type = helperForm.VehicleType,
                    Hash = ""
                };

                LogAppend($"[HelperScripts] Finished configuration helper for vehicle: {helperForm.VehicleName}");

                return data;
            }

            return default; // Return default value if InputFinishedFlag is false
        }

        // Cleanup Functions
        private void fixTextureFile(string filePath)
        {
 
            string content = File.ReadAllText(filePath, Encoding.Default);
            char[] array = content.ToCharArray();
            array[3] = '7';
            content = new string(array);
            File.WriteAllText(filePath, content, Encoding.Default);
        }

        private void InflateResourceFolder(string streamFolder, string dataFolder, string type, bool isYtd, bool isYtf, bool combined, string guid)
        {
            //Assume user types .txt into textbox
            string fileExtension = "*." + type;
            string basePath = Path.GetFullPath(Path.Combine("cache", guid, "rpfunpack"));
            string[] txtFiles = Directory.GetFiles(basePath, fileExtension, SearchOption.AllDirectories); // had to add a more specific directory here aswell, can't check the entire cache folder anymore :weary:

            foreach (var item in txtFiles)
            {
                
                if (isYtd)
                {
                    fixTextureFile(item);
                    File.Move(item, Path.Combine(streamFolder, Path.GetFileName(item))); // put into stream folder inside resource name

                }
                else if (isYtf)
                {
                    fixTextureFile(item);
                    File.Move(item, Path.Combine(streamFolder, Path.GetFileName(item))); // put into stream folder inside resource name
                }
                else
                {
                    File.Move(item, Path.Combine(dataFolder, Path.GetFileName(item)));
                }
            }
        }

        private void RemoveUnnessecary(string type, string guid)
        {
            string fileExtension = "*." + type;
            string[] txtFiles = Directory.GetFiles($"cache/{guid}/unpack", fileExtension, SearchOption.AllDirectories); // changed "cache" to "cache/unpack" to only clean the unpack folder, where the magic happens, actually i am thinking maybe doing it in streams is better so we dont have so much "state" going on since i started multithreading stuff but oh well

            //I moved this since logging takes a bunch of time if its a bunch of files.
            LogAppend($"[Worker] Deleting {fileExtension} files...");

            foreach (var item in txtFiles)
            {
           
                File.Delete(item);
            }
        }

        private void cleanUp(string guid)
        {
            try
            {
                //Directory.Delete($"cache/{guid}", true);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }

            if (fivemresname_tb.InvokeRequired)
            {
                fivemresname_tb.Invoke(new Action(() =>
                {
                    fivemresname_tb.Text = rnd.Next(2147483647).ToString();
                }));
            }
            else
            {
                fivemresname_tb.Text = rnd.Next(2147483647).ToString();
            }

            // Assuming StatusHandler updates UI too:
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => StatusHandler("Idle")));
            }
            else
            {
                StatusHandler("Idle");
            }
        }


        // Setup functions
        string fxmanifest_single = Properties.Resources.fxmanifest_false;
        string fxmanifest_combined = Properties.Resources.fxmanifest_true;

        private void SetupBasicEnviroment(string guid)
        {
            LogAppend("[Worker] Setting up basic enviroment...");
            //CreateDirectory will automatically check if it exist, this will arguably shorten jit or compile times lol
            Directory.CreateDirectory("cache");
            var basePath = Path.GetFullPath(Path.Combine("cache", guid));
            Directory.CreateDirectory(basePath); //what i said above for directory creation, i dont know if Path.Combine returns a Full path so ill do it like this

            Directory.CreateDirectory(Path.GetFullPath(Path.Combine(basePath, "unpack")));
            Directory.CreateDirectory(Path.GetFullPath(Path.Combine(basePath, "rpfunpack")));
            Directory.CreateDirectory(Path.GetFullPath(Path.Combine(basePath, "structure")));
            Directory.CreateDirectory(Path.GetFullPath(Path.Combine(basePath, "data")));
            LogAppend($"[Worker] Created /cache directory for guid: {guid}.");

            //I hope this will just work lol
            if (!Directory.Exists("./resources"))
            {
                Directory.CreateDirectory("resources");
                LogAppend("[Worker] Created /resources directory.");
            }

        }

        private void SetupStructureFolders(string folderName, string combinedFolderName, bool combinedEnv, string guid)
        {
            Encoding utf8WithoutBom = new UTF8Encoding(false);
            string directory = "";
            string fxmanifest = "";

            if (combinedEnv == true)
            {
                if (Directory.Exists(@"./combinercache"))
                {
                    LogAppend("[Worker] Combiner Cache folder exists, checking if there is any valid resource in there...");

                    if (Directory.EnumerateFileSystemEntries(@"./combinercache").Any())
                    {
                        LogAppend("[Worker] Found resource in Combiner Cache, moving back to unpacking cache.");
                        Directory.Move(@"./combinercache/" + combinedFolderName, $@"./cache/{guid}/structure/" + combinedFolderName);
                    } 
                    else
                    {
                        LogAppend("[Worker] Combiner Cache was empty, likely first processed resource.");

                        directory = $@"./cache/{guid}/structure/" + combinedFolderName;
                        fxmanifest = fxmanifest_combined;

                        Directory.CreateDirectory(directory);
                        Directory.CreateDirectory(directory + "/stream/");
                        Directory.CreateDirectory(directory + "/data/");
                        File.WriteAllText(directory + @"\fxmanifest.lua", fxmanifest, utf8WithoutBom);
                    }
                }
            }
            else
            {
                directory = $@"./cache/{guid}/structure/" + folderName;
                fxmanifest = fxmanifest_single;

                Directory.CreateDirectory(directory);
                Directory.CreateDirectory(directory + "/stream/");
                Directory.CreateDirectory(directory + "/data/");
                File.WriteAllText(directory + @"\fxmanifest.lua", fxmanifest, utf8WithoutBom);
            }

            LogAppend("[Worker] Created resource folder structure.");
        }

        private StructureFolders CreateDataFolders(string folderName, string combinedFolderName, bool combinedEnv, string guid)
        {
            StructureFolders structure;

            if (combinedEnv == true)
            {
                string directory = $@"./cache/{guid}/structure/" + combinedFolderName;

                Directory.CreateDirectory(directory + "/stream/" + folderName); // refer to the folder structure :/
                Directory.CreateDirectory(directory + "/data/" + folderName);

                structure.streamFolder = directory + @"/stream/" + folderName;
                structure.dataFolder = directory + @"/data/" + folderName;
            }
            else
            {
                string directory = $@"./cache/{guid}/structure/" + folderName;

                structure.streamFolder = directory + "/stream/";
                structure.dataFolder = directory + "/data/";
            }

            return structure;
        }

        // Conversion Functions

        //Updated to work with a Semaphore to control how many will be processed at once, realistically we should have it fit in a set ammount of ram since 2 2gb archives and 2 20mb archives behave diffrently under garbage collector, to achive having control over the diffrent caches i simply added a guid before the other stuff in the cache so its /cache/guid/
        public async Task startConversion(bool folder, string resource, string saferesource)
        {
            Encoding utf8WithoutBom = new UTF8Encoding(false);
            Regex regex = new Regex(@"<(.*?)>");
            InvokeIfRequired(() => LogAppend("[Worker] Start conversion process..."));

            InvokeIfRequired(() => {
                tsBar.Maximum = queueList.Items.Count;
                tsBar.Value = 0;
            });

            combinedFolderString = rnd.Next(2147483647).ToString();
            Directory.CreateDirectory(@"./combinercache");

            var itemList = queueList.Items.Cast<string>().ToList();
            var lockObj = new object();

            // Extract paths from itemList
            var zipFiles = itemList
                .Select(item => item.Split('>')[1].Trim()) // Get the part after ">"
                .Select(path => new FileInfo(path))        // Convert to FileInfo
                .ToList();

            //Make sure we dont unzip everything at once, which will for one make the GC explode and for the other use a unknown ammount of ram.
            //The class it self can also be easily editied to allow to set max ram consumption from the gui later.
            var evaluator = new UnzipSettingsEvaluator();
                                                         
            evaluator.EvaluateSettings(zipFiles);
            LogAppend("[Multithreading] Max concurrent unzips: " + evaluator.MaxConcurrentUnzips);
            var semaphore = new SemaphoreSlim(evaluator.MaxConcurrentUnzips);

            var tasks = itemList.Select(CurrentItem => Task.Run(async () =>
            {
                await semaphore.WaitAsync(); // Wait to enter
                string guid = Guid.NewGuid().ToString();
                var stopwatch = Stopwatch.StartNew();

                try
                {
                    InvokeIfRequired(() => LogAppend($"[Worker] Setting up basic environment for: {guid}"));
                    SetupBasicEnviroment(guid);
                    QueueHandler(currentQueue, itemList.Count);
                    string SingleEnviromentFolder = regex.Match(CurrentItem).Groups[1].Value;
                    string StreamFolder = "";
                    string DataFolder = "";

                    LogAppend("[Worker] Setting up resource folder structure...");
                    SetupStructureFolders(SingleEnviromentFolder, combinedFolderString, CombineResourceState, guid);

                    LogAppend("[Worker] Fetching resource stream and data folders...");
                    var StructureFolders = CreateDataFolders(SingleEnviromentFolder, combinedFolderString, CombineResourceState, guid);
                    StreamFolder = StructureFolders.streamFolder;
                    DataFolder = StructureFolders.dataFolder;

                    var CleanedItemName = CurrentItem.Replace($"<{SingleEnviromentFolder}>", "");
                    if (CurrentItem != "" && CleanedItemName.Contains("https://files.gta5-mods.com/") && !CleanedItemName.Contains("XXXCARNAMEXXXX"))
                    {
                        LogAppend("[Worker] Downloading vehicle archive from GTA5-Mods...");
                        await AsyncFileDownload(CleanedItemName);
                    }
                    else if (CurrentItem != "" && File.Exists(CleanedItemName))
                    {
                        LogAppend("[Worker] Processing locally stored vehicle archive...");
                        string destinationPath = Path.Combine("cache", guid, Path.GetFileName(CleanedItemName));
                        File.Copy(CleanedItemName, destinationPath, overwrite: true);
                    }
                    else
                    {
                        lock (lockObj)
                        {
                            currentQueue++;
                            // InvokeIfRequired(() => cleanUp(guid));
                        }

                        stopwatch.Stop();
                        InvokeIfRequired(() => jobTime.Text = $"| Last job took: {stopwatch.ElapsedMilliseconds} ms");
                        WarningAppend($"[Worker] File {CleanedItemName} does not exist, skipping.");
                        return;
                    }
                    // MessageBox.Show(CleanedItemName);

                    //File.Copy(CleanedItemName, )
                    //InvokeIfRequired(() => LogAppend("[Worker] Moving archives to cache..."));
                    //HideShellCmd($@"move *.rar ./cache/{guid}/");
                    //HideShellCmd($@"move *.zip ./cache/{guid}/");
                    //HideShellCmd($@"move *.7z ./cache/{guid}/");

                     LogAppend("[SharpCompress] Decompressing...");
                    //await Task.Delay(500);
                    universalCacheUnpack(guid);
                    //await Task.Delay(2500);

                     LogAppend("[Worker] Removing leftover files from the archive...");
                    RemoveUnnessecary("yft", guid);
                    RemoveUnnessecary("ytd", guid);
                    RemoveUnnessecary("meta", guid);

                    LogAppend("[CodeWalker] Searching for dlc.rpf...");
                    RpfUnpack(CleanedItemName, guid, SingleEnviromentFolder);

                    LogAppend("[Worker] Moving items from cache to resource folder...");
                    //await Task.Delay(5000);
                    LogAppend("[Worker] Inflating and fixing resources for: " + guid);
                    InflateResourceFolder(StreamFolder, DataFolder, "meta", false, false, false, guid);
                    InflateResourceFolder(StreamFolder, DataFolder, "yft", false, true, false, guid);
                    InflateResourceFolder(StreamFolder, DataFolder, "ytd", true, false, false, guid);

                    IncreaseProgressBar();

                    if (tsBar.Value == itemList.Count)
                    {
                        LogAppend("[Worker] Moving Combiner Cache folder to /resources as all conversions are finished.");

                        string sourcePath = Path.Combine("./combinercache", combinedFolderString);
                        string targetPath = Path.Combine("./resources", combinedFolderString);

                        // Move the folder from combiner cache to resources
                        if (Directory.Exists(targetPath))
                        {
                            // Optionally handle existing target folder (throw, merge, or delete)
                            LogAppend($"[Warning] Target folder already exists at {targetPath}. Deleting it before move.");
                            Directory.Delete(targetPath, true);
                        }

                        Directory.Move(sourcePath, targetPath);

                        // Delete the whole combinercache folder after moving the subfolder
                        if (Directory.Exists("./combinercache"))
                        {
                            //Directory.Delete("./combinercache", true);
                        }
                    }
                    else
                    {
                        LogAppend("[Worker] Copying resource folder to Combiner Cache.");

                        string sourcePath = Path.Combine("./cache", guid, "structure", combinedFolderString);
                        string targetPath = Path.Combine("./combinercache", combinedFolderString);

                        CopyIfNotExists(new DirectoryInfo(sourcePath), new DirectoryInfo(targetPath));
                    }




                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                    ErrorAppend("[CodeWalker] Failed to extract dlc.rpf, stack trace: " + ex);
                }
                finally
                {

                    lock (lockObj)
                    {
                        currentQueue++;
                        //InvokeIfRequired(() => cleanUp(guid));
                    }

                    stopwatch.Stop();
                    InvokeIfRequired(() => jobTime.Text = $"| Last job took: {stopwatch.ElapsedMilliseconds} ms");
                    semaphore.Release(); //Make sure to tell semaphore a thread got freed.
                }

            }));

            await Task.WhenAll(tasks);

        
            foreach(var model in models)
            {
                var modelName = model.Item1;
                var fold = model.Item2;
                if (QbxCoreHelperState || QbCoreHelperState || AddonCarSpawnerHelperState)
                {
                    InvokeHelperScripts(modelName, fold);
                }

                LogAppend($"[Worker] Conversion of vehicle {modelName} has finished");
            }
            
            InvokeIfRequired(() => jobTime.Text = $"| Finished");

        }

        private void IncreaseProgressBar()
        {
            InvokeIfRequired(() =>
            {
                tsBar.Value++;
            });
        }


        // Helper method to copy files/folders only if they don't exist in target
        void CopyIfNotExists(DirectoryInfo source, DirectoryInfo target)
        {
            if (!target.Exists)
            {
                target.Create();
            }

            foreach (FileInfo file in source.GetFiles())
            {
                string targetFilePath = Path.Combine(target.FullName, file.Name);
                if (!File.Exists(targetFilePath))
                {
                    file.CopyTo(targetFilePath);
                }
            }

            foreach (DirectoryInfo subDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(subDir.Name);
                CopyIfNotExists(subDir, nextTargetSubDir);
            }
        }

        private void InvokeIfRequired(Action action)
        {
            if (this.InvokeRequired)
                this.Invoke(action);
            else
                action();
        }

        // Events

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
        }

        private async void button2_Click_1(object sender, EventArgs e)
        {
            Random rnd = new Random();
            convertFromFolder_resname = rnd.Next(555555);

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "7z|*.7z|ZIP|*.zip|RAR|*.rar";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var filePath = openFileDialog.FileName;
                    var safeFileName = openFileDialog.SafeFileName;
                    LogAppend("[Worker] Converting resource at " + filePath);
                    await startConversion(true, filePath, safeFileName);

                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            queueList.Items.Clear();
            vehicleArray.Clear();
            btnStart.Enabled = false;
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            await startConversion(false, "", "");
        }

        private void btnAddQueue_Click(object sender, EventArgs e)
        {
            LogAppend("[InputHandler] Adding specificed job to the queue...");

            // Check for the helper states and if so, invoke the helper questionnaire
            InvokeQueueVehicleHelper(textBox1.Text);

            // Run the standard function
            queueList.Items.Add($"<{fivemresname_tb.Text}> " + textBox1.Text);
            fivemresname_tb.Text = rnd.Next(2147483647).ToString();
            QueueHandler(0, queueList.Items.Count);

            // Reset the input textbox
            textBox1.Clear();
            textBox1.Text = "https://files.gta5-mods.com/uploads/XXXCARNAMEXXXX/XXXCARNAMEXXXX.zip";

            // Return active control and start button
            this.ActiveControl = label1;
            btnStart.Enabled = queueList.Items.Count > 0;
        }

        private void SelectArchive_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "Archive files (*.rar;*.zip;*.7z)|*.rar;*.zip;*.7z";
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Multiselect = true; // Allow multiple file selection

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (var file in openFileDialog.FileNames) {
                        QueueArchive(file);
                    }
             
                }
            }
        }

        public void QueueArchive(string file)
        {
            LogAppend("[InputHandler] Adding specificed job to the queue...");

            // Check for the helper states and if so, invoke the helper questionnaire
            InvokeQueueVehicleHelper(file);

            // Run the standard function
            queueList.Items.Add($"<{fivemresname_tb.Text}> " + file);
            fivemresname_tb.Text = rnd.Next(2147483647).ToString();
            QueueHandler(0, queueList.Items.Count);

            // Reset the input textbox
           // textBox1.Clear();
            //textBox1.Text = "https://files.gta5-mods.com/uploads/XXXCARNAMEXXXX/XXXCARNAMEXXXX.zip";

            // Return active control and start button
            this.ActiveControl = label1;
            btnStart.Enabled = queueList.Items.Count > 0;
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Contains("https://files.gta5-mods.com/") && !textBox1.Text.Contains("XXXCARNAMEXXXX"))
            {
                gta5mods_status.ForeColor = Color.Green;
                gta5mods_status.Text = "OK";
                btnAddQueue.Enabled = true;

            }
            else if (File.Exists(textBox1.Text) && (textBox1.Text.EndsWith(".rar", StringComparison.OrdinalIgnoreCase) || textBox1.Text.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) || textBox1.Text.EndsWith(".7z", StringComparison.OrdinalIgnoreCase)))
            {
                gta5mods_status.ForeColor = Color.Green;
                gta5mods_status.Text = "OK";
                btnAddQueue.Enabled = true;
            }
            else
            {
                gta5mods_status.ForeColor = Color.Red;
                gta5mods_status.Text = "ERROR";
                btnAddQueue.Enabled = false;

            }
        }

        private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
        {
            if (CompressCheck.Checked == true)
            {
                LogAppend("[InputHandler] Enabled texture compression/downsizing.");
            }
            else
            {
                LogAppend("[InputHandler] Disabled texture compression/downsizing.");
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (QbxCoreHelper.Checked == true)
            {
                QbxCoreHelperState = true;
                LogAppend("[HelperScripts] Enabled vehicles list helper for qbx_core");
            }
            else
            {
                QbxCoreHelperState = false;
                LogAppend("[HelperScripts] Disabled vehicles list helper for qbx_core");
            }
        }

        private void QbCoreHelper_CheckedChanged(object sender, EventArgs e)
        {
            if (QbCoreHelper.Checked == true)
            {
                QbCoreHelperState = true;
                LogAppend("[HelperScripts] Enabled vehicles list helper for qb-core");
            }
            else
            {
                QbCoreHelperState = false;
                LogAppend("[HelperScripts] Disabled vehicles list helper for qb-core");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Check and recreate SaveQueueList.dat  
                if (File.Exists("SaveQueueList.dat"))
                {
                    File.Delete("SaveQueueList.dat");
                    LogAppend("[SaveState] Recreating SaveQueueList.dat data file for storage...");
                }
                using (File.Create("SaveQueueList.dat")) { }

                // Check and recreate SaveVehicleArray.dat  
                if (File.Exists("SaveVehicleArray.dat"))
                {
                    File.Delete("SaveVehicleArray.dat");
                    LogAppend("[SaveState] Recreating SaveVehicleArray.dat data file for storage...");
                }
                using (File.Create("SaveVehicleArray.dat")) { }

                LogAppend("[SaveState] Successfully recreated empty state files.");
            }
            catch (Exception ex)
            {
                ErrorAppend($"[SaveState] Failed to recreate state files. Error: {ex.Message}");
            }

            try
            {
                // Save queueList to SaveQueueList.dat
                using (StreamWriter writer = new StreamWriter("SaveQueueList.dat"))
                {
                    foreach (var item in queueList.Items)
                    {
                        writer.WriteLine(item.ToString());
                    }
                }
                LogAppend("[SaveState] Successfully saved queueList to SaveQueueList.dat.");

                // Save vehicleArray to SaveVehicleArray.dat
                using (StreamWriter writer = new StreamWriter("SaveVehicleArray.dat"))
                {
                    foreach (var vehicle in vehicleArray)
                    {
                        if (vehicle.Model == null || vehicle.Model == "")
                        {
                            writer.WriteLine($"{vehicle.InternalReference}|{vehicle.Name}|{vehicle.Brand}|NOMODEL|{vehicle.Price}|{vehicle.Category}|{vehicle.Type}|NOMODEL");
                        }
                        else
                        {
                            writer.WriteLine($"{vehicle.InternalReference}|{vehicle.Name}|{vehicle.Brand}|{vehicle.Model}|{vehicle.Price}|{vehicle.Category}|{vehicle.Type}|{vehicle.Hash}");
                        }
                    }
                }
                LogAppend("[SaveState] Successfully saved vehicleArray to SaveVehicleArray.dat.");
            }
            catch (Exception ex)
            {
                ErrorAppend($"[SaveState] Failed to save current state. Error: {ex.Message}");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // Clear the queueList before loading anything
                queueList.Items.Clear();
                QueueHandler(0, queueList.Items.Count);
                btnStart.Enabled = false;

                // Clear the vehicleArray collection
                vehicleArray.Clear();

                // Load queueList from SaveQueueList.dat
                if (File.Exists("SaveQueueList.dat"))
                {
                    using (StreamReader reader = new StreamReader("SaveQueueList.dat"))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            
                            queueList.Items.Add(line);
                        }
                    }
                    LogAppend("[LoadState] Successfully loaded queueList from SaveQueueList.dat.");
                }
                else
                {
                    LogAppend("[LoadState] SaveQueueList.dat not found. Skipping queueList loading.");
                }

                // Load vehicleArray from SaveVehicleArray.dat
                if (File.Exists("SaveVehicleArray.dat"))
                {
                    using (StreamReader reader = new StreamReader("SaveVehicleArray.dat"))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var parts = line.Split('|');
                            if (parts.Length == 8)
                            {
                                vehicleArray.Add(new VehicleData
                                {
                                    InternalReference = parts[0],
                                    Name = parts[1],
                                    Brand = parts[2],
                                    Model = parts[3],
                                    Price = int.Parse(parts[4]),
                                    Category = parts[5],
                                    Type = parts[6],
                                    Hash = parts[7]
                                });
                            }
                            else
                            {
                                WarningAppend("[LoadState] Malformed array line in SaveVehicleArray.dat, skipping.");
                            }
                        }
                    }

                    LogAppend("[LoadState] Successfully loaded vehicleArray from SaveVehicleArray.dat.");
                }
                else
                {
                    if(MessageBox.Show("Do you want to create the internal Car data table now? (you will need to enter a bunch of data for each car)", "rpf2fivem - Import", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        foreach (var item in queueList.Items)
                        {
                            //TODO Implement creating the list now, seems kind of usefull if the user forget before.
                        }
                    }
                    else
                    {
                        LogAppend("[LoadState] SaveVehicleArray.dat not found and user doesnt want to create them now. Skipping vehicleArray loading.");
                    }
               
                }

                // Update the UI
                btnStart.Enabled = queueList.Items.Count > 0;

            }
            catch (Exception ex)
            {
                ErrorAppend($"[LoadState] Failed to load saved state. Error: {ex.Message}");
            }
        }

        private void LoadEncryptionData_CheckedChanged_2(object sender, EventArgs e)
        {
            if (!LoadEncryptionData.Checked) { return; }

            // Check if the "Keys" directory exists in the current directory
            if (Directory.Exists("Keys"))
            {
                LogAppend("[KeyExtraction] Saved magic data was found, loading into CodeWalker key structure.");
                GTA5Keys.LoadMagicData();
                LoadEncryptionData.Enabled = false;
            }

            // If the directory doesn't exist, prompt the user to select the GTA5.exe directory
            else
            {
                using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
                {
                    folderDialog.Description = "Select Directory with GTA5.exe";
                    folderDialog.ShowNewFolderButton = false;

                    if (folderDialog.ShowDialog() == DialogResult.OK)
                    {
                        string selectedPath = folderDialog.SelectedPath;
                        if (File.Exists(Path.Combine(selectedPath, "GTA5.exe")))
                        {
                            LogAppend($"[KeyExtraction] Valid GTA5.exe directory selected: {selectedPath}");
                            try
                            {
                                GTA5Keys.LoadFromPath(selectedPath);

                                LogAppend("[KeyExtraction] Generating magic data files for future use...");


                                if (!Directory.Exists("Keys"))
                                {
                                    Directory.CreateDirectory("Keys");
                                }

                                GTA5Keys.SaveToPath();

                                LogAppend("[KeyExtraction] Successfully generated and saved magic data files.");

                                LoadEncryptionData.Enabled = false;
                            }
                            catch (Exception ex)
                            {
                                ErrorAppend($"[KeyExtraction] Failed to load encryption data. Error: {ex.Message}");
                            }
                        }
                        else
                        {
                            WarningAppend("[KeyExtraction] The selected directory does not contain GTA5.exe. Please select a valid directory.");
                        }
                    }
                }
            }
        }

        private void checkBox1_CheckedChanged_2(object sender, EventArgs e)
        {
            LogAppend("[Setup]:AddonCarSpawner Config generation: " + checkBox1.Checked);
            AddonCarSpawnerHelperState = checkBox1.Checked;
        }
    }



    public class UnzipSettingsEvaluator
    {
        public int CpuCoreCount { get; private set; }
        public long AvailableMemoryMB { get; private set; }

        public int MaxConcurrentUnzips { get; private set; }

        public UnzipSettingsEvaluator()
        {
            CpuCoreCount = Environment.ProcessorCount;
            AvailableMemoryMB = GetAvailableMemoryMB();
        }

        public static long GetAvailableMemoryMB()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
                if (GlobalMemoryStatusEx(memStatus))
                {
                    return (long)(memStatus.ullAvailPhys / (1024 * 1024));
                }
            }
            // TODO: Add Linux/macOS implementations
            return 2048; // fallback default
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal class MEMORYSTATUSEX
        {
            public uint dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        public void EvaluateSettings(IEnumerable<FileInfo> zipFiles)
        {
            var avgSizeMB = zipFiles.Any() ? zipFiles.Average(f => f.Length) / (1024 * 1024.0) : 150;
            var totalSizeMB = zipFiles.Sum(f => f.Length) / (1024 * 1024.0);

            // Simple heuristic:
            // IO-bound if small avg size (< 200MB), CPU-bound if large
            bool ioBound = avgSizeMB < 200;

            if (ioBound)
                MaxConcurrentUnzips = Math.Min(4 * CpuCoreCount, 16); // allow more IO-bound threads
            else
                MaxConcurrentUnzips = Math.Max(1, CpuCoreCount / 2); // limit CPU-bound tasks

            // RAM safety: ensure each unzip has ~500MB headroom
            var memLimit = (int)(AvailableMemoryMB / 500);
            MaxConcurrentUnzips = Math.Min(MaxConcurrentUnzips, memLimit);
        }
    }

    /*
    public static class SentrySdk
    {
        public static void CaptureException(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
    */
}


