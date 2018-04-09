using BitHub.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitHub.Helpers.Repository
{
    public static class FileHelper
    {
        public static Tuple<uint, string> GetFileAllTextAndLineCount(this IFileManager manager, string path)
        {
            StringBuilder sb = new StringBuilder();
            uint lineCount = 0;
            using (StreamReader reader = new StreamReader(manager.Open(path, FileMode.Open), Encoding.UTF8))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    ++lineCount;
                    sb.AppendLine(line);
                }
            }

            return new Tuple<uint, string>(lineCount, sb.ToString());
        }
    }
}
