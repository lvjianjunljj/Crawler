using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Crawler
{
    static class Logger
    {
        public static string LogFilePath;
        public static void WriteLog(string logContent)
        {
            StreamWriter writer = File.AppendText(LogFilePath);//文件中添加文件流

            writer.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + logContent);
            writer.Flush();
            writer.Close();
        }
    }
}