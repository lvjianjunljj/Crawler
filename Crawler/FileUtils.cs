using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.IO;

namespace Crawler
{
    class FileUtils
    {
        public static List<string> ReadFileLine(string filePath)
        {
            List<string> res = new List<string>();
            StreamReader sr = new StreamReader(filePath, Encoding.Default);
            String line;
            while ((line = sr.ReadLine()) != null)
            {
                res.Add(line.ToString());
            }
            return res;
        }
        public static string SaveReadmeFile(string readmePrefixName, string readmeSuffixName, string fileContent)
        {
            if (readmeSuffixName.Contains("\"") || readmeSuffixName.Contains(">"))
            {
                int endIndexFirst = readmeSuffixName.IndexOf("\"");
                int endIndexSecond = readmeSuffixName.IndexOf(">");
                if (endIndexFirst < 0)
                {
                    readmeSuffixName = readmeSuffixName.Remove(endIndexSecond, readmeSuffixName.Length - endIndexSecond);
                }
                else if (endIndexSecond < 0)
                {
                    readmeSuffixName = readmeSuffixName.Remove(endIndexFirst, readmeSuffixName.Length - endIndexFirst);
                }
                else
                {
                    int endIndex = Math.Min(endIndexFirst, endIndexSecond);
                    readmeSuffixName = readmeSuffixName.Remove(endIndex, readmeSuffixName.Length - endIndex);
                }
            }
            string readmeFileName = readmePrefixName + readmeSuffixName;
            int index = 1;
            while (File.Exists(readmeFileName))
            {
                readmeFileName = readmePrefixName + (++index) + readmeSuffixName;
            }
            try
            {
                SaveFile(Path.Combine(Configuration.ReadMeFileDir, readmeFileName), fileContent);
                return readmeFileName;
            }
            catch (Exception e)
            {
                Logger.WriteLog("SaveReadmeFile error: " + e.Message);
                return "";
            }
        }

        public static string ZipExtractToDirectory(string zipFilePath, string toDirPath)
        {
            //ZipFile.ExtractToDirectory("D://yt-master.zip", "D://");
            //ZipArchive archive = ZipFile.OpenRead(zipFilePath);
            string zipFileName = Path.GetFileName(zipFilePath);
            string dirName = zipFileName.Substring(0, zipFileName.Length - 4) ;
            //try
            //{
            //    // Get file list
            //    var files = archive.Entries;
            //    dirName = files[0].FullName.Remove(files[0].FullName.Length - 1, 1);
            //}
            //catch (Exception e)
            //{
            //    Logger.WriteLog("Extract to directory error:" + e.Message);
            //    int index = zipFilePath.LastIndexOf("\\");
            //    dirName = zipFilePath.Substring(index + 1, zipFilePath.Length - 4 - index - 1);
            //}
            string dirPath = Path.Combine(toDirPath, dirName);
            if (!Directory.Exists(dirPath))
            {
                try
                {
                    ZipFile.ExtractToDirectory(zipFilePath, dirPath);
                }
                catch (Exception e)
                {
                    Logger.WriteLog("Extract To Directory Error: " + e.Message);
                    Logger.WriteLog("zipFilePath: " + zipFilePath);
                    File.Delete(zipFilePath);
                }
            }
            else
            {
                Logger.WriteLog("Extract To Directory Warning: \"" + dirPath + "\" has exist!!!");
                //Directory.Move(dirPath, dirPath + "_temp");
                //try
                //{
                //    ZipFile.ExtractToDirectory(zipFilePath, toDirPath);
                //}
                //catch (Exception e)
                //{
                //    Logger.WriteLog("Extract To Directory Error: " + e.Message);
                //    Logger.WriteLog("zipFilePath: " + zipFilePath);
                //    //File.Delete(zipFilePath);
                //}
                //int index = 2;
                //string dirNameTemp = dirName + (index++);
                //string dirPathTemp = Path.Combine(toDirPath, dirNameTemp);
                //while (Directory.Exists(dirPathTemp))
                //{
                //    dirNameTemp = dirName + (index++);
                //    dirPathTemp = Path.Combine(toDirPath, dirNameTemp);
                //}
                //Directory.Move(dirPath, dirPathTemp);
                //Directory.Move(dirPath + "_temp", dirPath);
                //dirName = dirNameTemp;
            }
            return dirName;
        }
        public static HashSet<string> TraverseFile(string folderFullName)
        {
            HashSet<string> fileSet = new HashSet<string>();
            DirectoryInfo TheFolder = new DirectoryInfo(folderFullName);
            //TraverseFolder File
            foreach (FileInfo NextFile in TheFolder.GetFiles())
            {
                fileSet.Add(NextFile.Name);
            }
            return fileSet;
        }
        public static HashSet<string> GetEmptyFile(string folderFullName)
        {
            HashSet<string> fileSet = new HashSet<string>();
            DirectoryInfo TheFolder = new DirectoryInfo(folderFullName);
            //TraverseFolder File
            foreach (FileInfo NextFile in TheFolder.GetFiles())
            {
                if (NextFile.Length == 0)
                {
                    fileSet.Add(NextFile.Name);
                }
            }
            return fileSet;
        }
        public static HashSet<string> TraverseFolder(string folderFullName)
        {
            HashSet<string> folderSet = new HashSet<string>();
            DirectoryInfo TheFolder = new DirectoryInfo(folderFullName);
            //TraverseFolder Folder
            foreach (DirectoryInfo NextFolder in TheFolder.GetDirectories())
            {
                folderSet.Add(NextFolder.Name);
            }
            return folderSet;
        }
        public static HashSet<string> Traverse(string folderFullName, out HashSet<string> fileSet)
        {
            fileSet = new HashSet<string>();
            HashSet<string> folderSet = new HashSet<string>();
            DirectoryInfo TheFolder = new DirectoryInfo(folderFullName);
            //TraverseFolder Folder
            foreach (DirectoryInfo NextFolder in TheFolder.GetDirectories())
            {
                folderSet.Add(NextFolder.Name);
            }
            //TraverseFolder File
            foreach (FileInfo NextFile in TheFolder.GetFiles())
            {
                fileSet.Add(NextFile.Name);
            }
            return folderSet;
        }
        public static void DeleteReadmeFile(List<string> readmeFileNameList)
        {
            foreach (string readmeFileName in readmeFileNameList)
            {
                File.Delete(Path.Combine(Configuration.ReadMeFileDir, readmeFileName));
            }
        }
        public static void DeleteReadmeFile(HashSet<string> readmeFileNameList)
        {
            foreach (string readmeFileName in readmeFileNameList)
            {
                File.Delete(Path.Combine(Configuration.ReadMeFileDir, readmeFileName));
            }
        }
        public static void SaveFile(string filePath, List<string> contentLinesList)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            StreamWriter writer = File.AppendText(filePath);//文件中添加文件流
            for (int i = 0; i < contentLinesList.Count; i++)
            {
                writer.WriteLine(contentLinesList[i]);
            }
            writer.Flush();
            writer.Close();
        }
        public static string getReadmeFileContent(string readmeFileName)
        {
            string readmeFilePath = Path.Combine(Configuration.ReadMeFileDir, readmeFileName);
            if (!File.Exists(readmeFilePath))
            {
                return "";
            }
            string readmeFileContent = "";
            try
            {
                StreamReader sr = new StreamReader(readmeFilePath, Encoding.Default);
                readmeFileContent = sr.ReadToEnd();
            }
            catch (Exception e)
            {
                Logger.WriteLog("get readme file content error: " + e.Message);
                readmeFileContent = "";
            }
            return readmeFileContent;
        }
        private static void SaveFile(string filePath, string fileContent)
        {
            FileStream fs = new FileStream(filePath, FileMode.Create);
            byte[] data = System.Text.Encoding.Default.GetBytes(fileContent);
            fs.Write(data, 0, data.Length);
            //清空缓冲区、关闭流
            fs.Flush();
            fs.Close();
        }

        public static void DfsChangeFileExtensionName(string path, string oldExtensionName, string newExtensionName)
        {
            if (Directory.Exists(path))
            {
                string[] childrenDirPath = Directory.GetDirectories(path);
                string[] childrenFilPath = Directory.GetFiles(path);
                foreach (string childDirPath in childrenDirPath)
                {
                    DfsChangeFileExtensionName(childDirPath, oldExtensionName, newExtensionName);
                }
                foreach (string childFilPath in childrenFilPath)
                {
                    ChangeFileExtensionName(childFilPath, oldExtensionName, newExtensionName);
                }
            }
            else
            {
                ChangeFileExtensionName(path, oldExtensionName, newExtensionName);
            }
        }
        private static void ChangeFileExtensionName(string filePath, string oldExtensionName, string newExtensionName)
        {
            if (File.Exists(filePath) && Path.GetExtension(filePath) == oldExtensionName)
            {
                try
                {
                    File.Move(filePath, filePath.Substring(0, filePath.Length - oldExtensionName.Length) + newExtensionName);
                    Logger.WriteLog("[Info] Extract File has exe file... Move Successfully...The file path is: " + filePath);
                } catch (Exception e)
                {
                Logger.WriteLog("[Warning] Extract File has exe file and fail to move ... The file path is: " + filePath);

                }
            }
        }
    }
}
