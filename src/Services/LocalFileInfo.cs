using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace BitHub.Services
{
    public class LocalFileInfo : IFileInfoManager
    {

        private FileInfo fileInfo;

        public void SetFilePath(string path)
        {
            fileInfo = new FileInfo(path);
        }

        public long GetLength()
        {
            return fileInfo.Length;
        }
    }
}
