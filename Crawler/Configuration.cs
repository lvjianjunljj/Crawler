using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Crawler
{
    public static class Configuration
    {
        public static string MainMethodName { get; set; }
        public static string RootURL { get; set; }
        public static int CrawlTimeout { get; set; }
        public static string UserAgent { get; set; }
        public static int MaxRetryWhenCrawl { get; set; }
        public static int MaxRedirectCount { get; set; }
        public static string DBserver { get; set; }
        public static string DBName { get; set; }
        public static string DBUID { get; set; }
        public static string DBPWD { set; get; }
        public static string TableName { get; set; }
        public static string ReadmeContentTableName { get; set; }
        public static string DBPrimaryKey { get; set; }
        public static int DBInsertCountEveryTime { get; set; }
        public static int DBExecuteCountEveryTime { get; set; }
        public static string URLFile { get; set; }
        public static string RemainDataFile { get; set; }
        public static string LogDir { get; set; }
        public static string DownloadZipDir { get; set; }
        public static string ZipExtractDir { get; set; }
        public static string ReadMeFileDir { get; set; }

        //public static Dictionary<string, string> JobList;
        public static void Initialize(string configFilePath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(configFilePath);
            XmlNode xn = xmlDoc.SelectSingleNode("Configuration");


            XmlNodeList xnl1 = xn.ChildNodes;

            foreach (XmlNode xn1 in xnl1)
            {
                XmlElement xe1 = (XmlElement)xn1;

                switch (xe1.Name)
                {
                    case "MainMethodName":
                        MainMethodName = xe1.InnerText;
                        break;
                    case "RootURL":
                        RootURL = xe1.InnerText;
                        break;
                    case "CrawlTimeout":
                        CrawlTimeout =  Convert.ToInt32(xe1.InnerText);
                        break;
                    case "UserAgent":
                        UserAgent = xe1.InnerText;
                        break;
                    case "MaxRetryWhenCrawl":
                        MaxRetryWhenCrawl = Convert.ToInt32(xe1.InnerText);
                        break;
                    case "MaxRedirectCount":
                        MaxRedirectCount = Convert.ToInt32(xe1.InnerText);
                        break;
                    case "DBserver":
                        DBserver = xe1.InnerText;
                        break;
                    case "DBName":
                        DBName = xe1.InnerText;
                        break;
                    case "DBUID":
                        DBUID = xe1.InnerText;
                        break;
                    case "DBPWD":
                        DBPWD = xe1.InnerText;
                        break;
                    case "TableName":
                        TableName = xe1.InnerText;
                        break;
                    case "ReadmeContentTableName":
                        ReadmeContentTableName = xe1.InnerText;
                        break;
                    case "DBPrimaryKey":
                        DBPrimaryKey = xe1.InnerText;
                        break;
                    case "DBInsertCountEveryTime":
                        DBInsertCountEveryTime = Convert.ToInt16(xe1.InnerText);
                        break;
                    case "DBExecuteCountEveryTime":
                        DBExecuteCountEveryTime = Convert.ToInt16(xe1.InnerText);
                        break;
                    case "URLFile":
                        URLFile = xe1.InnerText;
                        break;
                    case "RemainDataFile":
                        RemainDataFile = xe1.InnerText;
                        break;
                    case "LogDir":
                        LogDir = xe1.InnerText;
                        CreateDirectory(LogDir);
                        break;
                    case "DownloadZipDir":
                        DownloadZipDir = xe1.InnerText;
                        CreateDirectory(DownloadZipDir);
                        break;
                    case "ZipExtractDir":
                        ZipExtractDir = xe1.InnerText;
                        CreateDirectory(ZipExtractDir);
                        break;
                    case "ReadMeFileDir":
                        ReadMeFileDir = xe1.InnerText;
                        CreateDirectory(ReadMeFileDir);
                        break;
                    //case "JobList":
                    //    XmlNodeList xnl2 = xe1.ChildNodes;
                    //    foreach (XmlNode xn2 in xnl2)
                    //    {
                    //        XmlElement xe2 = (XmlElement)xn2;
                    //        JobList.Add(xe2.GetAttribute("index"), xe2.InnerText);
                    //    }
                    //    break;
                }
            }
        }
        private static void CreateDirectory(string dirPath)
        {
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }
    }
}