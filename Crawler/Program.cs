using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Crawler
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] inputStrList = new string[] { "", "" };
            for (int i = 0; i < Math.Min(args.Length, inputStrList.Length); i++)
            {
                inputStrList[i] = args[i];
            }
            Configuration.Initialize("./Config.xml");
            DateTime dt = DateTime.Now;
            string dtStr = dt.ToString("yyyy_MM_dd HH_mm_ss");
            Logger.LogFilePath = Path.Combine(Configuration.LogDir, dtStr + @".log");
            Type tp = typeof(Crawler.Program);
            object obj = Activator.CreateInstance(tp);
            MethodInfo method = tp.GetMethod(Configuration.MainMethodName);
            method.Invoke(obj, inputStrList);
        }

        public static void CheckZipFile(string inputStrFirst, string InputStrSecond)
        {
            HashSet<string> fileSetLocal = FileUtils.TraverseFile(Configuration.DownloadZipDir);
            HashSet<string> dirSetLocal = FileUtils.TraverseFolder(Configuration.ZipExtractDir);
            Dictionary<string, DBGitDataModel> existDownLoadURLData;
            DBUtils.GetExistZipData(out existDownLoadURLData);
            List<string> fileListDBExtra = new List<string>();
            List<string> dirListDBExtra = new List<string>();
            foreach (string downLoadURLData in existDownLoadURLData.Keys)
            {
                DBGitDataModel model = existDownLoadURLData[downLoadURLData];
                if (fileSetLocal.Contains(model.fileName))
                {
                    fileSetLocal.Remove(model.fileName);
                }
                else
                {
                    fileListDBExtra.Add(model.fileName);
                }
                if (dirSetLocal.Contains(model.dirName))
                {
                    dirSetLocal.Remove(model.dirName);
                }
                else
                {
                    dirListDBExtra.Add(model.dirName);
                }
            }
            Console.WriteLine("Extra local file: ");
            foreach (string fileName in fileSetLocal)
            {
                Console.WriteLine(fileName);
            }
            Console.WriteLine("Extra local directory: ");
            foreach (string dirName in dirSetLocal)
            {
                Console.WriteLine(dirName);
            }
            Console.WriteLine("Extra DB file: ");
            foreach (string fileName in fileListDBExtra)
            {
                Console.WriteLine(fileName);
            }
            Console.WriteLine("Extra DB directory: ");
            foreach (string dirName in dirListDBExtra)
            {
                Console.WriteLine(dirName);
            }

        }
        public static void CheckEmptyReadmeFile(string inputStrFirst, string InputStrSecond)
        {
            DateTime startTime = DateTime.Now;
            HashSet<string> fileEmpty = FileUtils.GetEmptyFile(Configuration.ReadMeFileDir);
            DateTime now = DateTime.Now;
            int timeCost = (int)(now - startTime).TotalSeconds;
            Console.WriteLine("Get Empty Readme File Data Time cost: " + timeCost + "s");
            Console.WriteLine("Empty File Size: " + fileEmpty.Count);
            startTime = now;
            FileUtils.DeleteReadmeFile(fileEmpty);
            now = DateTime.Now;
            timeCost = (int)(now - startTime).TotalSeconds;
            Console.WriteLine("Delete Empty Readme File Data Time cost: " + timeCost + "s");
        }
        public static void CheckReadmeFile(string inputStrFirst, string InputStrSecond)
        {
            HashSet<string> fileSetLocal = FileUtils.TraverseFile(Configuration.ReadMeFileDir);
            HashSet<string> existDataDB = DBUtils.GetExistReadmeFileNameData();
            List<string> fileDBExtra = new List<string>();
            foreach (string dataDB in existDataDB)
            {
                if (fileSetLocal.Contains(dataDB))
                {
                    fileSetLocal.Remove(dataDB);
                }
                else
                {
                    fileDBExtra.Add(dataDB);
                }
            }
            Console.WriteLine("Extra local file: " + fileSetLocal.Count);
            FileUtils.DeleteReadmeFile(fileSetLocal);
            //foreach (string fileName in fileSetLocal)
            //{
            //    Console.WriteLine(fileName);
            //}
            Console.WriteLine("Extra DB file: " + fileDBExtra.Count);
            DBUtils.SetEmptyDataFromDBCrawlerGitDetailDataByReadmeFileName(fileDBExtra);
            //foreach (string fileName in fileDBExtra)
            //{
            //    Console.WriteLine(fileName);
            //}
        }
        public static void CrawlAndStoreGitData(string inputStrFirst, string InputStrSecond)
        {
            List<string> inputList = FileUtils.ReadFileLine(Configuration.URLFile);
            List<DBGitDataModel> insertData = new List<DBGitDataModel>();
            Dictionary<string, DBGitDataModel> existDownLoadURLData;
            HashSet<string> existPKData = DBUtils.GetExistZipData(out existDownLoadURLData);
            for (int i = 0; i < inputList.Count; i++)
            {
                string[] eles = inputList[i].Split(new char[] { '\t' });
                string repositoryPath = eles[0];
                Console.WriteLine(repositoryPath);
                if (existPKData.Contains(repositoryPath))
                {
                    continue;
                }
                int impressionCount = Convert.ToInt32(eles[1]);
                int clickCount = Convert.ToInt32(eles[2]);
                HttpStatusCode statusCode;
                Dictionary<string, string> header;
                string htmlContent;
                try
                {
                    htmlContent = CrawlerClass.Crawl(repositoryPath, out statusCode, out header);
                }
                catch (Exception e)
                {
                    Logger.WriteLog("error: crawl \"" + repositoryPath + "\" " + e.Message);
                    continue;
                }
                string downloadRelativeURL = HtmlResolve.GetGitDownloadURL(htmlContent);
                if (downloadRelativeURL == null || downloadRelativeURL == "")
                {
                    continue;
                }
                string downloadURL = Configuration.RootURL + downloadRelativeURL;
                string fileName;
                string dirName;
                if (existDownLoadURLData.ContainsKey(downloadURL))
                {
                    DBGitDataModel modelTemp = existDownLoadURLData[downloadURL];
                    fileName = modelTemp.fileName;
                    dirName = modelTemp.dirName;
                }
                else
                {
                    fileName = CrawlerClass.HttpDownloadFile(downloadURL, Configuration.DownloadZipDir);
                    dirName = FileUtils.ZipExtractToDirectory(Path.Combine(Configuration.DownloadZipDir, fileName), Configuration.ZipExtractDir);
                    if (dirName == "")
                    {
                        continue;
                    }
                }
                DBGitDataModel model = new DBGitDataModel(repositoryPath, downloadURL, impressionCount, clickCount, fileName, dirName);
                insertData.Add(model);
                if (insertData.Count == Configuration.DBInsertCountEveryTime)
                {
                    DBUtils.StoreDataToDBGitDataPart(insertData);
                    insertData = new List<DBGitDataModel>();
                    Console.WriteLine("Store Data To DBGitData Part End!!!");
                }
            }
            if (insertData.Count > 0)
            {
                DBUtils.StoreDataToDBGitDataPart(insertData);
            }
        }
        public static void CrawlAndStoreCrawlerGitDetailData(string inputStrFirst, string InputStrSecond)
        {
            int[] indexData = GetIndex(inputStrFirst, InputStrSecond);
            int startIndex = indexData[0], endIndex = indexData[1];
            if (startIndex < 0)
            {
                return;
            }
            DateTime startTime;
            bool start = startIndex > 0;
            string existLastData = DBUtils.getExistLastData();
            if (existLastData == "")
            {
                Console.WriteLine("get the exist last data in DB error!!!");
                Logger.WriteLog("get the exist last data in DB error!!!");
                return;
            }
            List<string> inputList = FileUtils.ReadFileLine(Configuration.URLFile);
            List<DBCrawlerGitDetailDataModel> insertData = new List<DBCrawlerGitDetailDataModel>();
            Console.WriteLine("Lines Count: " + inputList.Count);
            endIndex = endIndex > 0 ? endIndex : inputList.Count;
            Console.WriteLine("Crawler GitDetailData from line " + startIndex + " to line " + endIndex + " in file \"" + Configuration.URLFile + "\".");
            //HashSet<string> existPKData = DBUtils.GetExistData();
            startTime = DateTime.Now;
            for (int i = startIndex; i < endIndex; i++)
            {
                string[] eles = inputList[i].Split(new char[] { '\t' });
                string repositoryPath = eles[0];
                if (!start)
                {
                    if (repositoryPath == existLastData)
                    {
                        start = true;
                        Console.WriteLine("Crawl and store data to CrawlerGitDetailData start!!!");
                        Logger.WriteLog("Crawl and store data to CrawlerGitDetailData start!!!");
                    }
                    continue;
                }
                //Console.WriteLine(repositoryPath);
                //if (existPKData.Contains(repositoryPath))
                //{
                //    continue;
                //}
                string readmePrefixName = repositoryPath.Substring(Configuration.RootURL.Length + 1, repositoryPath.Length - Configuration.RootURL.Length - 1).Replace("/", "_");
                int impressionCount = Convert.ToInt32(eles[1]);
                int clickCount = Convert.ToInt32(eles[2]);
                HttpStatusCode statusCode;
                Dictionary<string, string> header;
                string htmlContent;
                try
                {
                    htmlContent = CrawlerClass.Crawl(repositoryPath, out statusCode, out header);
                }
                catch (Exception e)
                {
                    Logger.WriteLog("error: crawl \"" + repositoryPath + "\" " + e.Message);
                    continue;
                }
                string downloadRelativeURL = HtmlResolve.GetGitDownloadURL(htmlContent);
                string downloadURL = "";
                if (downloadRelativeURL != null && downloadRelativeURL != "")
                {
                    downloadURL = Configuration.RootURL + downloadRelativeURL;
                }
                string readmeSuffixName;
                string repositoryContent;
                List<string> topicsList;
                string readmeFileContent;
                try
                {
                    repositoryContent = HtmlResolve.getRepositoryContent(htmlContent);
                    topicsList = HtmlResolve.getTopicsList(htmlContent);
                    readmeFileContent = HtmlResolve.getReadmeContent(htmlContent, out readmeSuffixName);
                }
                catch (Exception e)
                {
                    Logger.WriteLog("getReadmeContent error: " + e.Message);
                    Logger.WriteLog("the repositoryPath is \"" + repositoryPath + "\".");
                    continue;
                }
                string readmeFileName = FileUtils.SaveReadmeFile(readmePrefixName, readmeSuffixName, readmeFileContent);
                if (readmeFileName == "")
                {
                    Logger.WriteLog("SaveReadmeFile error: readmePrefixName is \"" + readmePrefixName + "\" and readmeSuffixName is \"" + readmeSuffixName + "\".");
                }
                DBCrawlerGitDetailDataModel model = new DBCrawlerGitDetailDataModel(repositoryPath, downloadURL, impressionCount, clickCount, repositoryContent, topicsList, readmeFileName);

                insertData.Add(model);
                if (insertData.Count == Configuration.DBInsertCountEveryTime)
                {
                    DBUtils.StoreDataToDBCrawlerGitDetailDataPart(ref insertData);
                    insertData = new List<DBCrawlerGitDetailDataModel>();
                    Console.WriteLine("Store Data To DBCrawlerGitDetailData Part End!!!");
                    Console.WriteLine("Line index: " + i);
                    DateTime now = DateTime.Now;
                    int timeCost = (int)(now - startTime).TotalSeconds;
                    Console.WriteLine("Time cost: " + timeCost + "s");
                    startTime = now;
                }
            }
            if (insertData.Count > 0)
            {
                DBUtils.StoreDataToDBCrawlerGitDetailDataPart(ref insertData);
            }
        }
        public static void SaveNotStoredDetailData(string inputStrFirst, string InputStrSecond)
        {
            List<string> inputList = FileUtils.ReadFileLine(Configuration.URLFile);
            List<string> insertData = new List<string>();
            DateTime startTime = DateTime.Now;
            HashSet<string> existPKData = DBUtils.GetExistDetailData();
            DateTime now = DateTime.Now;
            int timeCost = (int)(now - startTime).TotalSeconds;
            startTime = now;
            Console.WriteLine("Get Exist Data Time cost: " + timeCost + "s");
            for (int i = 0; i < inputList.Count; i++)
            {
                if (i % 10000 == 0)
                {
                    Console.WriteLine(i);
                }
                string[] eles = inputList[i].Split(new char[] { '\t' });
                string repositoryPath = eles[0];
                if (existPKData.Contains(repositoryPath))
                {
                    continue;
                }
                insertData.Add(inputList[i]);
            }
            Console.WriteLine("Not Stored Line: " + insertData.Count);
            now = DateTime.Now;
            timeCost = (int)(now - startTime).TotalSeconds;
            startTime = now;
            Console.WriteLine("Get Not Stored Data Time cost: " + timeCost + "s");
            FileUtils.SaveFile(Configuration.RemainDataFile, insertData);
            now = DateTime.Now;
            timeCost = (int)(now - startTime).TotalSeconds;
            Console.WriteLine("Save File Time cost: " + timeCost + "s");
        }

        public static void UpdateDBReadmeName(string inputStrFirst, string InputStrSecond)
        {
            int[] indexData = GetIndex(inputStrFirst, InputStrSecond);
            int startIndex = indexData[0], endIndex = indexData[1];
            if (startIndex < 0)
            {
                return;
            }
            DateTime startTime = DateTime.Now;
            List<string> emptyReadmeDataList = DBUtils.GetEmptyReadmeData();
            DateTime now = DateTime.Now;
            int timeCost = (int)(now - startTime).TotalSeconds;
            startTime = now;
            Console.WriteLine("Get Empty Readme Data Time cost: " + timeCost + "s");
            Console.WriteLine("Empty Readme Data Line Count: " + emptyReadmeDataList.Count);
            endIndex = endIndex > 0 ? endIndex : emptyReadmeDataList.Count;
            Console.WriteLine("Crawler GitDetailData from line " + startIndex + " to line " + endIndex + " in file \"" + Configuration.URLFile + "\".");
            //HashSet<string> existPKData = DBUtils.GetExistData();
            int updateCount = 0;
            for (int i = startIndex; i < endIndex; i++)
            {
                string repositoryPath = emptyReadmeDataList[i];
                string readmePrefixName = repositoryPath.Substring(Configuration.RootURL.Length + 1, repositoryPath.Length - Configuration.RootURL.Length - 1).Replace("/", "_");
                HttpStatusCode statusCode;
                Dictionary<string, string> header;
                string htmlContent;
                if (i % Configuration.DBInsertCountEveryTime == 0)
                {
                    Console.WriteLine("Line index: " + i + "\t" + "updateCount: " + updateCount);
                    updateCount = 0;
                }
                try
                {
                    htmlContent = CrawlerClass.Crawl(repositoryPath, out statusCode, out header);
                }
                catch (Exception e)
                {
                    Logger.WriteLog("error: crawl \"" + repositoryPath + "\" " + e.Message);
                    continue;
                }
                string readmeSuffixName;
                string readmeFileContent = HtmlResolve.getReadmeContent(htmlContent, out readmeSuffixName);
                if (readmeFileContent == null || readmeFileContent.Length < 1)
                {
                    Logger.WriteLog("\"" + repositoryPath + "\" crawler readmeFileContent is empty!!!");
                    continue;
                }
                string readmeFileName = FileUtils.SaveReadmeFile(readmePrefixName, readmeSuffixName, readmeFileContent);
                if (readmeFileName == "")
                {
                    Logger.WriteLog("SaveReadmeFile error: readmePrefixName is \"" + readmePrefixName + "\" and readmeSuffixName is \"" + readmeSuffixName + "\".");
                }
                else
                {
                    DBUtils.UpdateEmptyReadmeData(repositoryPath, readmeFileName);
                    updateCount++;
                }
            }
        }
        public static void StoreReadmeContentDataToDB(string inputStrFirst, string InputStrSecond)
        {
            int[] indexData = GetIndex(inputStrFirst, InputStrSecond);
            int startIndex = indexData[0], endIndex = indexData[1];
            if (startIndex < 0)
            {
                return;
            }
            DateTime startTime = DateTime.Now;
            List<string> repositoryPathList;
            List<string> readmeFileNameList = DBUtils.GetExistReadmeData(out repositoryPathList);
            DateTime now = DateTime.Now;
            int timeCost = (int)(now - startTime).TotalSeconds;
            startTime = now;
            Console.WriteLine("Get readme File Name List Time cost: " + timeCost + "s");
            if (repositoryPathList.Count != readmeFileNameList.Count)
            {
                Console.WriteLine("repositoryPathList.Count: \"" + repositoryPathList.Count + "\" is not equal to readmeFileNameList.Count: \"" + readmeFileNameList.Count + "\", return!!!");
                return;
            }
            Console.WriteLine("Readme Data Line Count: " + repositoryPathList.Count);
            endIndex = endIndex > 0 ? endIndex : repositoryPathList.Count;
            Console.WriteLine("Store Readme Content Data To CrawlerGitReadmeContent from line " + startIndex + " to line " + endIndex + " in DB \"" + Configuration.TableName + "\".");
            List<string> repositoryPathInputList = new List<string>();
            List<string> readmeFileContentInputList = new List<string>();
            HashSet<string> existReadmeContentSet = DBUtils.GetExistReadmeContentData();
            Console.WriteLine("Exist Readme Content Data Line Count: " + existReadmeContentSet.Count);
            now = DateTime.Now;
            timeCost = (int)(now - startTime).TotalSeconds;
            Console.WriteLine("Time cost: " + timeCost + "s");
            startTime = now;
            for (int i = startIndex; i < endIndex; i++)
            {
                string repositoryPath = repositoryPathList[i];
                if (existReadmeContentSet.Contains(repositoryPath))
                {
                    continue;
                }
                string readmeFileName = readmeFileNameList[i];
                string readmeFileContent = FileUtils.getReadmeFileContent(readmeFileName);
                repositoryPathInputList.Add(repositoryPath);
                readmeFileContentInputList.Add(readmeFileContent);
                if (repositoryPathInputList.Count == Configuration.DBInsertCountEveryTime)
                {
                    DBUtils.StoreDataToDBCrawlerGitReadmeContentPart(ref repositoryPathInputList, ref readmeFileContentInputList);
                    repositoryPathInputList = new List<string>();
                    readmeFileContentInputList = new List<string>();
                    Console.WriteLine("Store Readme Content Data To CrawlerGitReadmeContent Part End!!!");
                    Console.WriteLine("Line index: " + i);
                    now = DateTime.Now;
                    timeCost = (int)(now - startTime).TotalSeconds;
                    Console.WriteLine("Time cost: " + timeCost + "s");
                    startTime = now;
                }
            }
            if (repositoryPathInputList.Count > 0)
            {
                DBUtils.StoreDataToDBCrawlerGitReadmeContentPart(ref repositoryPathInputList, ref readmeFileContentInputList);
            }
        }
        public static void StoreReadmeContentDataToOneTable(string inputStrFirst, string InputStrSecond)
        {
            int[] indexData = GetIndex(inputStrFirst, InputStrSecond);
            int startIndex = indexData[0], endIndex = indexData[1];
            if (startIndex < 0)
            {
                return;
            }
            DateTime startTime = DateTime.Now;
            List<string> repositoryPathList;
            DBUtils.GetExistReadmeData(out repositoryPathList);
            DateTime now = DateTime.Now;
            int timeCost = (int)(now - startTime).TotalSeconds;
            startTime = now;
            Console.WriteLine("Get readme File Name List Time cost: " + timeCost + "s");
            Console.WriteLine("Readme Data Line Count: " + repositoryPathList.Count);
            endIndex = endIndex > 0 ? endIndex : repositoryPathList.Count;
            Console.WriteLine("Store Readme Content Data To CrawlerGitReadmeContent from line " + startIndex + " to line " + endIndex + " in DB \"" + Configuration.TableName + "\".");
            for (int i = startIndex; i < endIndex; i++)
            {
                string repositoryPath = repositoryPathList[i];
                string readmeFileContent = DBUtils.GetReadmeContent(repositoryPath);
                DBUtils.UpdateReadmeContentDataInTableName(repositoryPath, readmeFileContent);
                if (i % Configuration.DBInsertCountEveryTime == 0)
                {
                    Console.WriteLine("Store Readme Content Data To CrawlerGitReadmeContent Part End!!!");
                    Console.WriteLine("Line index: " + i);
                    now = DateTime.Now;
                    timeCost = (int)(now - startTime).TotalSeconds;
                    Console.WriteLine("Time cost: " + timeCost + "s");
                    startTime = now;
                }
            }
            Console.WriteLine("Store Readme Content Data To CrawlerGitReadmeContent All End*****************************");
        }

        public static void UpdateRepositoryName(string inputStrFirst, string InputStrSecond)
        {
            int[] indexData = GetIndex(inputStrFirst, InputStrSecond);
            int startIndex = indexData[0], endIndex = indexData[1];
            if (startIndex < 0)
            {
                return;
            }
            DateTime startTime = DateTime.Now;
            List<string> repositoryNameList;
            List<string> repositoryPathList = DBUtils.GetExistDetailDataRepositoryName(out repositoryNameList);
            DateTime now = DateTime.Now;
            int timeCost = (int)(now - startTime).TotalSeconds;
            startTime = now;
            Console.WriteLine("Get readme File Name List Time cost: " + timeCost + "s");
            if (repositoryPathList.Count != repositoryNameList.Count)
            {
                Console.WriteLine("repositoryPathList.Count: \"" + repositoryPathList.Count + "\" is not equal to repositoryNameList.Count: \"" + repositoryNameList.Count + "\", return!!!");
                return;
            }
            Console.WriteLine("Readme Data Line Count: " + repositoryPathList.Count);
            endIndex = endIndex > 0 ? endIndex : repositoryPathList.Count;
            Console.WriteLine("Store Readme Content Data To CrawlerGitReadmeContent from line " + startIndex + " to line " + endIndex + " in DB \"" + Configuration.TableName + "\".");
            for (int i = startIndex; i < endIndex; i++)
            {
                string repositoryPath = repositoryPathList[i];
                string repositoryName = repositoryNameList[i];
                if (repositoryName == "")
                {
                    repositoryName = repositoryPath.Substring(Configuration.RootURL.Length + 1, repositoryPath.Length - Configuration.RootURL.Length - 1);
                    DBUtils.UpdateRepositoryName(repositoryPath, repositoryName);
                }


                if (i % Configuration.DBInsertCountEveryTime == 0)
                {
                    Console.WriteLine("Store Readme Content Data To CrawlerGitReadmeContent Part End!!!");
                    Console.WriteLine("Line index: " + i);
                    now = DateTime.Now;
                    timeCost = (int)(now - startTime).TotalSeconds;
                    Console.WriteLine("Time cost: " + timeCost + "s");
                    startTime = now;
                }
            }
            Console.WriteLine("Store Readme Content Data To CrawlerGitReadmeContent All End*****************************");
        }
        public static void DownloadGitCodeZip(string inputStrFirst, string InputStrSecond)
        {
            int[] indexData = GetIndex(inputStrFirst, InputStrSecond);
            int startIndex = indexData[0], endIndex = indexData[1];
            if (startIndex < 0)
            {
                return;
            }
            DateTime startTime = DateTime.Now;
            List<string> repositoryNameList;
            List<string> downloadURLList = DBUtils.GetDownloadURLList(out repositoryNameList);
            DateTime now = DateTime.Now;
            int timeCost = (int)(now - startTime).TotalSeconds;
            startTime = now;
            Console.WriteLine("Get Name List Time cost: " + timeCost + "s");
            if (downloadURLList.Count != repositoryNameList.Count)
            {
                Console.WriteLine("downloadURLList.Count: \"" + downloadURLList.Count + "\" is not equal to repositoryNameList.Count: \"" + repositoryNameList.Count + "\", return!!!");
                return;
            }
            Console.WriteLine("Data Line Count: " + downloadURLList.Count);
            endIndex = endIndex > 0 ? endIndex : downloadURLList.Count;
            //ThreadPool.SetMaxThreads(1, 1);
            for (int i = startIndex; i < endIndex; i++)
            {
                //ThreadPool.QueueUserWorkItem(new WaitCallback(DownloadFileTaskMethod), new GitCodeZipInfoClass(downloadURLList[i], repositoryNameList[i].Replace("/", "_").Replace("\\", "_") + ".zip"));
                string fileName = CrawlerClass.HttpDownloadFile(downloadURLList[i], Configuration.DownloadZipDir, false, false, repositoryNameList[i].Replace("/", "_").Replace("\\", "_") + ".zip");
                Console.WriteLine("Run " + i + " line fileName: " + fileName + " will be downloaded");
                //Thread.Sleep(100);
            }
            Console.WriteLine("Download Git Code Zip All End*****************************");
        }
        private static void DownloadFileTaskMethod(Object gitCodeZipInfoObj)
        {
            CrawlerClass.HttpDownloadFile(((GitCodeZipInfoClass)gitCodeZipInfoObj).downloadURL, Configuration.DownloadZipDir, false, true, ((GitCodeZipInfoClass)gitCodeZipInfoObj).repositoryName);
            Console.WriteLine(((GitCodeZipInfoClass)gitCodeZipInfoObj).repositoryName + " will be downloaded");
        }
        private class GitCodeZipInfoClass
        {
            public string downloadURL { get; set; }
            public string repositoryName { get; set; }
            public GitCodeZipInfoClass(string downloadURL, string repositoryName)
            {
                this.downloadURL = downloadURL;
                this.repositoryName = repositoryName;
            }
        }

        public static void CheckNotDownloadGitCodeZip(string inputStrFirst, string InputStrSecond)
        {
            int[] indexData = GetIndex(inputStrFirst, InputStrSecond);
            int startIndex = indexData[0], endIndex = indexData[1];
            if (startIndex < 0)
            {
                return;
            }
            DateTime startTime = DateTime.Now;
            List<string> downloadURLList = DBUtils.GetDownloadURLList(out List<string> repositoryNameList);
            List<string> notDowmLoadRepositoryNameList = new List<string>();
            List<string> notDownloadURLList = new List<string>();
            if (downloadURLList.Count != repositoryNameList.Count)
            {
                Console.WriteLine("downloadURLList.Count: \"" + downloadURLList.Count + "\" is not equal to repositoryNameList.Count: \"" + repositoryNameList.Count + "\", return!!!");
                return;
            }
            Console.WriteLine("Data Line Count: " + downloadURLList.Count);
            endIndex = endIndex > 0 ? endIndex : downloadURLList.Count;
            for (int i = startIndex; i < endIndex; i++)
            {
                string filePath = Path.Combine(Configuration.DownloadZipDir, repositoryNameList[i].Replace("/", "_").Replace("\\", "_") + ".zip");
                if (!File.Exists(filePath))
                {
                    notDowmLoadRepositoryNameList.Add(repositoryNameList[i]);
                    notDownloadURLList.Add(downloadURLList[i]);
                }
            }
            Console.WriteLine("Not Download Git Code Zip File Count: " + notDowmLoadRepositoryNameList.Count + " " + notDownloadURLList.Count);
            for (int i = 0; i < Math.Min(notDowmLoadRepositoryNameList.Count, notDownloadURLList.Count); i++)
            {
                Console.WriteLine("notDowmLoadRepositoryName: " + notDowmLoadRepositoryNameList[i]);
                Console.WriteLine("notDownloadURLList: " + notDownloadURLList[i]);
            }
            Console.WriteLine();
        }

        public static void GitCodeZipExtractToDirectory(string inputStrFirst, string InputStrSecond)
        {
            int[] indexData = GetIndex(inputStrFirst, InputStrSecond);
            int startIndex = indexData[0], endIndex = indexData[1];
            if (startIndex < 0)
            {
                return;
            }
            string[] gitCodeZipFullPathList = Directory.GetFiles(Configuration.DownloadZipDir);
            Console.WriteLine("Data Line Count: " + gitCodeZipFullPathList.Length);
            endIndex = endIndex > 0 ? endIndex : gitCodeZipFullPathList.Length;
            for (int i = startIndex; i < endIndex; i++)
            {
                string fileName = Path.GetFileName(gitCodeZipFullPathList[i]);
                string dirName = FileUtils.ZipExtractToDirectory(Path.Combine(Configuration.DownloadZipDir, fileName), Configuration.ZipExtractDir);
                FileUtils.DfsChangeFileExtensionName(Path.Combine(Configuration.ZipExtractDir, dirName), ".exe", "_exe.txt");
                Console.WriteLine("Run " + i + " line fileName: " + fileName + " will be extracted to directory");
            }
            Console.WriteLine(gitCodeZipFullPathList.Length);


        }


        private static int[] GetIndex(string inputStrFirst, string InputStrSecond)
        {
            int startIndex = 0, endIndex = 0;
            if (inputStrFirst != null && inputStrFirst.Length > 0)
            {
                try
                {
                    startIndex = Convert.ToInt32(inputStrFirst);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Crawl And Store Crawler GitDetailData First Input Error: " + e.Message);
                    return new int[] { -1, -1 };
                }
            }
            if (InputStrSecond != null && InputStrSecond.Length > 0)
            {
                try
                {
                    endIndex = Convert.ToInt32(InputStrSecond);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Crawl And Store Crawler GitDetailData Second Input Error: " + e.Message);
                    return new int[] { -1, -1 };
                }
            }
            return new int[] { startIndex, endIndex };
        }
    }
}
