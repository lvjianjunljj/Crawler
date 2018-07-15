using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    class DBGitDataModel
    {
        public string repositoryPath { get; set; }
        public string downloadURL { get; set; }
        public int impressionCount { get; set; }
        public int clickCount { get; set; }
        public string fileName { get; set; }
        public string dirName { get; set; }
        public DBGitDataModel(string repositoryPath, string downloadURL, int impressionCount, int clickCount, string fileName, string dirName)
        {
            this.repositoryPath = CheckDBStringData(repositoryPath);
            this.downloadURL = CheckDBStringData(downloadURL);
            this.impressionCount = impressionCount;
            this.clickCount = clickCount;
            this.fileName = CheckDBStringData(fileName);
            this.dirName = CheckDBStringData(dirName);
        }
        public static string CheckDBStringData(string inputStr)
        {
            return inputStr.Replace("'", "''");
        }
    }
    class DBCrawlerGitDetailDataModel
    {
        public string repositoryPath { get; set; }
        public string downloadURL { get; set; }
        public int impressionCount { get; set; }
        public int clickCount { get; set; }
        public string repositoryContent { get; set; }
        public List<string> topicsList { get; set; }

        public DBCrawlerGitDetailDataModel(
            string repositoryPath,
            string downloadURL,
            int impressionCount,
            int clickCount,
            string repositoryContent,
            List<string> topicsList,
            string readmeFileName)
        {
            this.repositoryPath = DBGitDataModel.CheckDBStringData(repositoryPath);
            this.downloadURL = DBGitDataModel.CheckDBStringData(downloadURL);
            this.impressionCount = impressionCount;
            this.clickCount = clickCount;
            this.repositoryContent = DBGitDataModel.CheckDBStringData(repositoryContent);
            this.topicsList = topicsList;
            if (this.topicsList != null)
            {
                for (int i = 0; i < this.topicsList.Count; i++)
                {
                    this.topicsList[i] = DBGitDataModel.CheckDBStringData(this.topicsList[i]);
                }
            }

        }
    }
}
