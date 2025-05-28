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
    public abstract class RPFConverterBase
    {
        public string RPFBasePath { get; set; }
        public string ArchivePath { get; set; }

        public Dictionary<string, byte[]> MVFS = new Dictionary<string, byte[]>(); //""vfs"" to simulate the folder structure inside memory, ik lol
        public Dictionary<string, byte[]> SVFS = new Dictionary<string, byte[]>(); //""vfs"" to simulate the folder structure inside memory, ik lol


        public bool IsFinished = false;
        public abstract bool Process((string name, string rpfname, byte[] data) data);

        public RPFConverterBase() { }

    }
}