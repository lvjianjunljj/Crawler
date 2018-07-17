using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    static class DBUtils
    {
        static SqlConnection Connection;
        public static void StoreDataToDBAll(List<DBGitDataModel> inputList)
        {
            Connection = new SqlConnection("Server=" + Configuration.DBserver + ";DataBase=" + Configuration.DBName + ";uid=" + Configuration.DBUID + ";pwd=" + Configuration.DBPWD);
            Connection.Open();
            string dbInsertSql = "INSERT INTO [" + Configuration.DBName + "].[dbo].[" + Configuration.TableName + "]([repositoryPath],[downloadURL],[impressionCount],[clickCount],[fileName],[dirName])VALUES";
            int lineCount = 0;
            for (int i = 0; i < inputList.Count; i++)
            {
                DBGitDataModel model = inputList[i];
                dbInsertSql += "('" + model.repositoryPath + "','" + model.downloadURL + "','" + model.impressionCount + "','" + model.clickCount + "','" + model.fileName + "','" + model.dirName + "'),";
                lineCount++;
                if (lineCount == Configuration.DBExecuteCountEveryTime)
                {
                    ExecuteByText(dbInsertSql.Remove(dbInsertSql.Length - 1, 1));
                    dbInsertSql = "INSERT INTO [" + Configuration.DBName + "].[dbo].[" + Configuration.TableName + "]([repositoryPath],[downloadURL],[impressionCount],[clickCount],[fileName],[dirName])VALUES";
                    lineCount = 0;
                }
            }
            if (lineCount > 0)
            {
                ExecuteByText(dbInsertSql.Remove(dbInsertSql.Length - 1, 1));
            }
            Connection.Close();
        }
        public static void StoreDataToDBGitDataPart(List<DBGitDataModel> inputList)
        {
            Connection = new SqlConnection("Server=" + Configuration.DBserver + ";DataBase=" + Configuration.DBName + ";uid=" + Configuration.DBUID + ";pwd=" + Configuration.DBPWD);
            Connection.Open();
            StoreDataToDBGitDataPart(inputList, 0, inputList.Count);
            Connection.Close();
        }
        private static void StoreDataToDBGitDataPart(List<DBGitDataModel> inputList, int startIndex, int endIndex)
        {
            if (startIndex >= endIndex)
            {
                return;
            }
            string dbInsertSql = "INSERT INTO [" + Configuration.DBName + "].[dbo].[" + Configuration.TableName + "]([repositoryPath],[downloadURL],[impressionCount],[clickCount],[fileName],[dirName])VALUES";
            for (int i = 0; i < inputList.Count; i++)
            {
                DBGitDataModel model = inputList[i];
                dbInsertSql += "('" + model.repositoryPath + "','" + model.downloadURL + "','" + model.impressionCount + "','" + model.clickCount + "','" + model.fileName + "','" + model.dirName + "'),";
            }
            bool success = ExecuteByText(dbInsertSql.Remove(dbInsertSql.Length - 1, 1));
            if (!success)
            {
                if (startIndex + 1 < endIndex)
                {
                    int midIndex = (startIndex + endIndex) / 2;
                    StoreDataToDBGitDataPart(inputList, startIndex, midIndex);
                    StoreDataToDBGitDataPart(inputList, midIndex, endIndex);
                }
            }
        }
        public static void StoreDataToDBCrawlerGitDetailDataPart(ref List<DBCrawlerGitDetailDataModel> inputList)
        {
            Connection = new SqlConnection("Server=" + Configuration.DBserver + ";DataBase=" + Configuration.DBName + ";uid=" + Configuration.DBUID + ";pwd=" + Configuration.DBPWD);
            Connection.Open();
            StoreDataToDBCrawlerGitDetailDataPart(ref inputList, 0, inputList.Count);
            Connection.Close();
        }
        private static void StoreDataToDBCrawlerGitDetailDataPart(ref List<DBCrawlerGitDetailDataModel> inputList, int startIndex, int endIndex)
        {
            if (startIndex >= endIndex)
            {
                return;
            }
            string dbInsertSql = "INSERT INTO [" + Configuration.DBName + "].[dbo].[" + Configuration.TableName + "]([repositoryPath],[downloadURL],[impressionCount],[clickCount],[repositoryContent],[topicsList],[readmeFileName])VALUES";
            for (int i = startIndex; i < endIndex; i++)
            {
                DBCrawlerGitDetailDataModel model = inputList[i];
                string topicsListStr = "";
                for (int j = 0; j < model.topicsList.Count; j++)
                {
                    if (j > 0)
                    {
                        topicsListStr += ";";
                    }
                    topicsListStr += model.topicsList[j];
                }
                dbInsertSql += "('" + model.repositoryPath + "','" + model.downloadURL + "','" + model.impressionCount + "','" + model.clickCount + "','" + model.repositoryContent + "','" + topicsListStr + "','" + model.readmeFileName + "'),";
            }
            bool success = ExecuteByText(dbInsertSql.Remove(dbInsertSql.Length - 1, 1));
            if (!success)
            {
                if (startIndex + 1 < endIndex)
                {
                    int midIndex = (startIndex + endIndex) / 2;
                    StoreDataToDBCrawlerGitDetailDataPart(ref inputList, startIndex, midIndex);
                    StoreDataToDBCrawlerGitDetailDataPart(ref inputList, midIndex, endIndex);
                }
            }
        }
        public static void StoreDataToDBCrawlerGitReadmeContentPart(ref List<string> repositoryPathList, ref List<string> readmeFileContentList)
        {
            Connection = new SqlConnection("Server=" + Configuration.DBserver + ";DataBase=" + Configuration.DBName + ";uid=" + Configuration.DBUID + ";pwd=" + Configuration.DBPWD);
            Connection.Open();
            StoreDataToDBCrawlerGitReadmeContentPart(ref repositoryPathList, ref readmeFileContentList, 0, repositoryPathList.Count);
            Connection.Close();
        }
        private static void StoreDataToDBCrawlerGitReadmeContentPart(ref List<string> repositoryPathList, ref List<string> readmeFileContentList, int startIndex, int endIndex)
        {
            if (startIndex >= endIndex)
            {
                return;
            }
            string dbInsertSql = "INSERT INTO [" + Configuration.DBName + "].[dbo].[" + Configuration.ReadmeContentTableName + "]([repositoryPath],[readmeContent])VALUES";
            for (int i = startIndex; i < endIndex; i++)
            {
                dbInsertSql += "('" + repositoryPathList[i].Replace("'", "''") + "','" + readmeFileContentList[i].Replace("'", "''") + "'),";
            }
            bool success = ExecuteByText(dbInsertSql.Remove(dbInsertSql.Length - 1, 1));
            if (!success)
            {
                if (startIndex + 1 < endIndex)
                {
                    int midIndex = (startIndex + endIndex) / 2;
                    StoreDataToDBCrawlerGitReadmeContentPart(ref repositoryPathList, ref readmeFileContentList, startIndex, midIndex);
                    StoreDataToDBCrawlerGitReadmeContentPart(ref repositoryPathList, ref readmeFileContentList, midIndex, endIndex);
                }
            }
        }
        public static void DeleteDataFromDBCrawlerGitDetailDataByReadmeFileName(List<string> deleteReadmeFileNameList)
        {
            if (deleteReadmeFileNameList == null || deleteReadmeFileNameList.Count == 0)
            {
                return;
            }
            Connection = new SqlConnection("Server=" + Configuration.DBserver + ";DataBase=" + Configuration.DBName + ";uid=" + Configuration.DBUID + ";pwd=" + Configuration.DBPWD);
            Connection.Open();
            string dbDeleteSql = "DELETE FROM [" + Configuration.DBName + "].[dbo].[" + Configuration.TableName + "] WHERE ";
            int lineCount = 0;
            bool success = true;
            for (int i = 0; i < deleteReadmeFileNameList.Count; i++)
            {
                dbDeleteSql += "readmeFileName = '" + deleteReadmeFileNameList[i].Replace("'", "''") + "' or ";
                lineCount++;
                if (lineCount == Configuration.DBExecuteCountEveryTime)
                {
                    success = success && ExecuteByText(dbDeleteSql.Remove(dbDeleteSql.Length - 4, 4));
                    dbDeleteSql = "DELETE FROM [" + Configuration.DBName + "].[dbo].[" + Configuration.TableName + "] WHERE ";
                    lineCount = 0;
                }
            }
            if (lineCount > 0)
            {
                success = success && ExecuteByText(dbDeleteSql.Remove(dbDeleteSql.Length - 4, 4));
            }
            if (!success)
            {
                Logger.WriteLog("Delete Data From DB CrawlerGitDetailData By Readme File Name fail!!!");
            }
            Connection.Close();
        }
        public static void SetEmptyDataFromDBCrawlerGitDetailDataByReadmeFileName(List<string> setEmptyReadmeFileNameList)
        {
            if (setEmptyReadmeFileNameList == null || setEmptyReadmeFileNameList.Count == 0)
            {
                return;
            }
            Connection = new SqlConnection("Server=" + Configuration.DBserver + ";DataBase=" + Configuration.DBName + ";uid=" + Configuration.DBUID + ";pwd=" + Configuration.DBPWD);
            Connection.Open();
            string dbSetEmptyeSql = "UPDATE [" + Configuration.DBName + "].[dbo].[" + Configuration.TableName + "] SET readmeFileName = '' WHERE ";
            int lineCount = 0;
            bool success = true;
            for (int i = 0; i < setEmptyReadmeFileNameList.Count; i++)
            {
                dbSetEmptyeSql += "readmeFileName = '" + setEmptyReadmeFileNameList[i].Replace("'", "''") + "' or ";
                lineCount++;
                if (lineCount == Configuration.DBExecuteCountEveryTime)
                {
                    success = success && ExecuteByText(dbSetEmptyeSql.Remove(dbSetEmptyeSql.Length - 4, 4));
                    dbSetEmptyeSql = "UPDATE [" + Configuration.DBName + "].[dbo].[" + Configuration.TableName + "] SET readmeFileName = '' WHERE ";
                    lineCount = 0;
                }
            }
            if (lineCount > 0)
            {
                success = success && ExecuteByText(dbSetEmptyeSql.Remove(dbSetEmptyeSql.Length - 4, 4));
            }
            if (!success)
            {
                Logger.WriteLog("Delete Data From DB CrawlerGitDetailData By Readme File Name fail!!!");
            }
            Connection.Close();
        }
        public static HashSet<string> GetExistZipData(out Dictionary<string, DBGitDataModel> existDownLoadURLData)
        {
            existDownLoadURLData = new Dictionary<string, DBGitDataModel>();
            HashSet<string> pkNameSet = new HashSet<string>();
            Connection = new SqlConnection("Server=" + Configuration.DBserver + ";DataBase=" + Configuration.DBName + ";uid=" + Configuration.DBUID + ";pwd=" + Configuration.DBPWD);
            Connection.Open();
            string dbQuerySql = "SELECT * FROM [" + Configuration.DBName + "].[dbo].[" + Configuration.TableName + "] order by id";
            try
            {
                SqlCommand cmd = new SqlCommand(dbQuerySql, Connection);
                cmd.CommandType = CommandType.Text;

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string repositoryPath = reader["repositoryPath"].ToString();
                    string downloadURL = reader["downloadURL"].ToString();
                    int impressionCount = Convert.ToInt32(reader["impressionCount"].ToString());
                    int clickCount = Convert.ToInt32(reader["clickCount"].ToString());
                    string fileName = reader["fileName"].ToString();
                    string dirName = reader["dirName"].ToString();
                    pkNameSet.Add(repositoryPath);
                    DBGitDataModel model = new DBGitDataModel(repositoryPath, downloadURL, impressionCount, clickCount, fileName, dirName);
                    if (!existDownLoadURLData.ContainsKey(downloadURL))
                    {
                        existDownLoadURLData.Add(downloadURL, model);
                    }
                }
                Connection.Close();
                return pkNameSet;
            }
            catch (Exception ex)
            {
                Connection.Close();
                Console.WriteLine(ex.Message);
                return pkNameSet;
            }
        }
        public static HashSet<string> GetExistReadmeFileNameData()
        {
            HashSet<string> readmeDBSet = new HashSet<string>();
            Connection = new SqlConnection("Server=" + Configuration.DBserver + ";DataBase=" + Configuration.DBName + ";uid=" + Configuration.DBUID + ";pwd=" + Configuration.DBPWD);
            Connection.Open();
            string dbQuerySql = "SELECT * FROM [" + Configuration.DBName + "].[dbo].[" + Configuration.TableName + "] order by id";
            try
            {
                SqlCommand cmd = new SqlCommand(dbQuerySql, Connection);
                cmd.CommandType = CommandType.Text;

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string readmeFileName = reader["readmeFileName"].ToString();
                    if (readmeFileName != "")
                    {
                        readmeDBSet.Add(readmeFileName);
                    }
                }
                Connection.Close();
                return readmeDBSet;
            }
            catch (Exception ex)
            {
                Connection.Close();
                Console.WriteLine(ex.Message);
                return readmeDBSet;
            }
        }
        public static List<string> GetExistReadmeData(out List<string> repositoryPathList)
        {
            repositoryPathList = new List<string>();
            List<string> readmeFileNameList = new List<string>();
            Connection = new SqlConnection("Server=" + Configuration.DBserver + ";DataBase=" + Configuration.DBName + ";uid=" + Configuration.DBUID + ";pwd=" + Configuration.DBPWD);
            Connection.Open();
            string dbQuerySql = "SELECT * FROM [" + Configuration.DBName + "].[dbo].[" + Configuration.TableName + "] order by id";
            try
            {
                SqlCommand cmd = new SqlCommand(dbQuerySql, Connection);
                cmd.CommandType = CommandType.Text;

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    repositoryPathList.Add(reader["repositoryPath"].ToString());
                    readmeFileNameList.Add(reader["readmeFileName"].ToString());
                }
                Connection.Close();
                return readmeFileNameList;
            }
            catch (Exception ex)
            {
                Connection.Close();
                Console.WriteLine(ex.Message);
                return readmeFileNameList;
            }
        }
        public static List<string> GetDownloadURLList(out List<string> repositoryNameList)
        {
            repositoryNameList = new List<string>();
            List<string> downloadURLList = new List<string>();
            Connection = new SqlConnection("Server=" + Configuration.DBserver + ";DataBase=" + Configuration.DBName + ";uid=" + Configuration.DBUID + ";pwd=" + Configuration.DBPWD);
            Connection.Open();
            string dbQuerySql = "SELECT * FROM [" + Configuration.DBName + "].[dbo].[" + Configuration.TableName + "] order by id";
            try
            {
                SqlCommand cmd = new SqlCommand(dbQuerySql, Connection);
                cmd.CommandType = CommandType.Text;

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    downloadURLList.Add(reader["downloadURL"].ToString());
                    repositoryNameList.Add(reader["repositoryName"].ToString());
                }
                Connection.Close();
                return downloadURLList;
            }
            catch (Exception ex)
            {
                Connection.Close();
                Console.WriteLine(ex.Message);
                return downloadURLList;
            }
        }

        public static string GetReadmeFileName(string repositoryPath)
        {
            string readmeFileName = "";
            Connection = new SqlConnection("Server=" + Configuration.DBserver + ";DataBase=" + Configuration.DBName + ";uid=" + Configuration.DBUID + ";pwd=" + Configuration.DBPWD);
            Connection.Open();
            string dbQuerySql = "SELECT * FROM [" + Configuration.DBName + "].[dbo].[" + Configuration.TableName + "]  WHERE repositoryPath = '" + repositoryPath + "'";
            try
            {
                SqlCommand cmd = new SqlCommand(dbQuerySql, Connection);
                cmd.CommandType = CommandType.Text;

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string readmeFileNameTemp = reader["readmeFileName"].ToString();
                    if (readmeFileNameTemp != "")
                    {
                        readmeFileName = readmeFileNameTemp;
                    }
                }
                Connection.Close();
                return readmeFileName;
            }
            catch (Exception ex)
            {
                Connection.Close();
                Console.WriteLine(ex.Message);
                return readmeFileName;
            }
        }
        public static string GetReadmeContent(string repositoryPath)
        {
            string readmeContent = "";
            Connection = new SqlConnection("Server=" + Configuration.DBserver + ";DataBase=" + Configuration.DBName + ";uid=" + Configuration.DBUID + ";pwd=" + Configuration.DBPWD);
            Connection.Open();
            string dbQuerySql = "SELECT * FROM [" + Configuration.DBName + "].[dbo].[" + Configuration.ReadmeContentTableName + "]  WHERE repositoryPath = '" + repositoryPath.Replace("'", "''") + "'";
            try
            {
                SqlCommand cmd = new SqlCommand(dbQuerySql, Connection);
                cmd.CommandType = CommandType.Text;

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string readmeContentTemp = reader["readmeContent"].ToString();
                    if (readmeContentTemp != "")
                    {
                        readmeContent = readmeContentTemp;
                    }
                }
                Connection.Close();
                return readmeContent;
            }
            catch (Exception ex)
            {
                Connection.Close();
                Console.WriteLine(ex.Message);
                return readmeContent;
            }
        }
        public static void UpdateEmptyReadmeData(string repositoryPath, string readmeFileName)
        {
            List<string> readmeDBSet = new List<string>();
            Connection = new SqlConnection("Server=" + Configuration.DBserver + ";DataBase=" + Configuration.DBName + ";uid=" + Configuration.DBUID + ";pwd=" + Configuration.DBPWD);
            Connection.Open();
            try
            {
                string dbSetEmptyeSql = "UPDATE [" + Configuration.DBName + "].[dbo].[" + Configuration.TableName + "] SET readmeFileName = '" + readmeFileName.Replace("'", "''") + "' WHERE repositoryPath = '" + repositoryPath.Replace("'", "''") + "'";
                bool success = ExecuteByText(dbSetEmptyeSql);
                Connection.Close();
            }
            catch (Exception ex)
            {
                Connection.Close();
                Console.WriteLine(ex.Message);
            }
        }
        public static void UpdateReadmeContentDataInTableName(string repositoryPath, string readmeContent)
        {
            Connection = new SqlConnection("Server=" + Configuration.DBserver + ";DataBase=" + Configuration.DBName + ";uid=" + Configuration.DBUID + ";pwd=" + Configuration.DBPWD);
            Connection.Open();
            try
            {
                string dbSetEmptyeSql = "UPDATE [" + Configuration.DBName + "].[dbo].[" + Configuration.TableName + "] SET readmeContent = '" + readmeContent.Replace("'", "''") + "' WHERE repositoryPath = '" + repositoryPath.Replace("'", "''") + "'";
                ExecuteByText(dbSetEmptyeSql);
                Connection.Close();
            }
            catch (Exception ex)
            {
                Connection.Close();
                Console.WriteLine(ex.Message);
            }
        }

        public static void UpdateRepositoryName(string repositoryPath, string repositoryName)
        {
            Connection = new SqlConnection("Server=" + Configuration.DBserver + ";DataBase=" + Configuration.DBName + ";uid=" + Configuration.DBUID + ";pwd=" + Configuration.DBPWD);
            Connection.Open();
            try
            {
                string dbSetEmptyeSql = "UPDATE [" + Configuration.DBName + "].[dbo].[" + Configuration.TableName + "] SET repositoryName = '" + repositoryName.Replace("'", "''") + "' WHERE repositoryPath = '" + repositoryPath.Replace("'", "''") + "'";
                ExecuteByText(dbSetEmptyeSql);
                Connection.Close();
            }
            catch (Exception ex)
            {
                Connection.Close();
                Console.WriteLine(ex.Message);
            }
        }
        public static List<string> GetEmptyReadmeData()
        {
            List<string> readmeDBSet = new List<string>();
            Connection = new SqlConnection("Server=" + Configuration.DBserver + ";DataBase=" + Configuration.DBName + ";uid=" + Configuration.DBUID + ";pwd=" + Configuration.DBPWD);
            Connection.Open();
            string dbQuerySql = "SELECT * FROM [" + Configuration.DBName + "].[dbo].[" + Configuration.TableName + "] WHERE readmeFileName = ''";
            try
            {
                SqlCommand cmd = new SqlCommand(dbQuerySql, Connection);
                cmd.CommandType = CommandType.Text;

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string readmeFileName = reader["readmeFileName"].ToString();
                    string repositoryPath = reader["repositoryPath"].ToString();
                    if (readmeFileName == "")
                    {
                        readmeDBSet.Add(repositoryPath);
                    }
                }
                Connection.Close();
                return readmeDBSet;
            }
            catch (Exception ex)
            {
                Connection.Close();
                Console.WriteLine(ex.Message);
                return readmeDBSet;
            }
        }
        public static HashSet<string> GetExistDetailData()
        {
            HashSet<string> pkNameSet = new HashSet<string>();
            Connection = new SqlConnection("Server=" + Configuration.DBserver + ";DataBase=" + Configuration.DBName + ";uid=" + Configuration.DBUID + ";pwd=" + Configuration.DBPWD);
            Connection.Open();
            string dbQuerySql = "SELECT " + Configuration.DBPrimaryKey + " FROM [" + Configuration.DBName + "].[dbo].[" + Configuration.TableName + "]";
            try
            {
                SqlCommand cmd = new SqlCommand(dbQuerySql, Connection);
                cmd.CommandType = CommandType.Text;

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    pkNameSet.Add(reader[Configuration.DBPrimaryKey].ToString());
                }
                Connection.Close();
                return pkNameSet;
            }
            catch (Exception ex)
            {
                Connection.Close();
                Console.WriteLine(ex.Message);
                return pkNameSet;
            }
        }

        public static List<string> GetExistDetailDataRepositoryName(out List<string> repositoryNameList)
        {
            List<string> pkNameList = new List<string>();
            repositoryNameList = new List<string>();
            Connection = new SqlConnection("Server=" + Configuration.DBserver + ";DataBase=" + Configuration.DBName + ";uid=" + Configuration.DBUID + ";pwd=" + Configuration.DBPWD);
            Connection.Open();
            string dbQuerySql = "SELECT " + Configuration.DBPrimaryKey + ", repositoryName FROM [" + Configuration.DBName + "].[dbo].[" + Configuration.TableName + "]";
            try
            {
                SqlCommand cmd = new SqlCommand(dbQuerySql, Connection);
                cmd.CommandType = CommandType.Text;

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    pkNameList.Add(reader[Configuration.DBPrimaryKey].ToString());
                    repositoryNameList.Add(reader["repositoryName"].ToString());
                }
                Connection.Close();
                return pkNameList;
            }
            catch (Exception ex)
            {
                Connection.Close();
                Console.WriteLine(ex.Message);
                return pkNameList;
            }
        }
        public static HashSet<string> GetExistReadmeContentData()
        {
            HashSet<string> pkNameSet = new HashSet<string>();
            Connection = new SqlConnection("Server=" + Configuration.DBserver + ";DataBase=" + Configuration.DBName + ";uid=" + Configuration.DBUID + ";pwd=" + Configuration.DBPWD);
            Connection.Open();
            string dbQuerySql = "SELECT " + Configuration.DBPrimaryKey + " FROM [" + Configuration.DBName + "].[dbo].[" + Configuration.ReadmeContentTableName + "]";
            try
            {
                SqlCommand cmd = new SqlCommand(dbQuerySql, Connection);
                cmd.CommandType = CommandType.Text;

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    pkNameSet.Add(reader[Configuration.DBPrimaryKey].ToString());
                }
                Connection.Close();
                return pkNameSet;
            }
            catch (Exception ex)
            {
                Connection.Close();
                Console.WriteLine(ex.Message);
                return pkNameSet;
            }
        }
        public static string getExistLastData()
        {
            string existLastData = "";
            Connection = new SqlConnection("Server=" + Configuration.DBserver + ";DataBase=" + Configuration.DBName + ";uid=" + Configuration.DBUID + ";pwd=" + Configuration.DBPWD);
            Connection.Open();
            string dbQuerySql = "SELECT * FROM [" +
                Configuration.DBName +
                "].[dbo].[" +
                Configuration.TableName +
                "] WHERE id IN (SELECT MAX(id) FROM [" +
                Configuration.DBName + "].[dbo].[" +
                Configuration.TableName +
                "])";
            try
            {
                SqlCommand cmd = new SqlCommand(dbQuerySql, Connection);
                cmd.CommandType = CommandType.Text;

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    existLastData = reader[Configuration.DBPrimaryKey].ToString();
                }
                Connection.Close();
            }
            catch (Exception ex)
            {
                Connection.Close();
                Console.WriteLine(ex.Message);
                existLastData = "";
            }
            return existLastData;
        }
        private static bool ExecuteByText(string dbInsertSql)
        {
            try
            {
                SqlCommand cmd = new SqlCommand(dbInsertSql, Connection);
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Execute DB By Text error:" + ex.Message);
                Logger.WriteLog("Execute DB By Text error:" + ex.Message);
                return false;
            }
        }
        private static void ExecuteByStoredProcedure()
        {
            try
            {
                string server = "STCVM-H03";
                string database = "Crawler";
                SqlConnection conn = null;
                conn = new SqlConnection("Server=" + server + ";DataBase=" + database + ";uid=" + Configuration.DBUID + ";pwd=" + Configuration.DBPWD);
                SqlCommand cmd = new SqlCommand("SaveRunLog", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@province", "Beijing"));
                cmd.Parameters.Add(new SqlParameter("@city", "Beijing"));
                cmd.Parameters.Add(new SqlParameter("@name", "Jianjun Lv"));
                cmd.Parameters.Add(new SqlParameter("@gender", "male"));
                cmd.Parameters.Add(new SqlParameter("@count", 1234));

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
