using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Crawler
{
    static class HtmlResolve
    {
        static string indexContent = "Download ZIP";
        static string firstContent = "<a href=\"";
        static string lastContent = ".zip\"";
        public static string GetGitDownloadURL(string htmlContent)
        {
            try
            {
                int index = htmlContent.IndexOf(indexContent);
                string content = htmlContent.Substring(0, index);
                int indexStart = content.LastIndexOf(firstContent);
                int indexEnd = content.LastIndexOf(lastContent);
                string url = content.Substring(indexStart + firstContent.Length, indexEnd + lastContent.Length - indexStart - firstContent.Length - 1);
                return url;
            }
            catch (Exception e)
            {
                Logger.WriteLog("Get git downloadURL error: " + e.Message);
                return null;
            }
        }

        public static string getRepositoryContent(string htmlContent)
        {
            string indexStartContent = "<div class=\"repository-meta-content col-11 mb-1\">";
            string indexEndContent = "</div>";
            string cubeStartContent = ">";
            string cubeEndContent = "<";
            try
            {
                int indexStart = htmlContent.IndexOf(indexStartContent);
                if (indexStart < 0)
                {
                    return "";
                }
                int indexEnd = htmlContent.IndexOf(indexEndContent, indexStart);
                string subContent = htmlContent.Substring(indexStart + indexStartContent.Length, indexEnd - indexStart - indexStartContent.Length).Replace("\n", "").Replace("\t", "");
                int cubeStart = subContent.IndexOf(cubeStartContent);
                int cubeEnd = subContent.IndexOf(cubeEndContent, cubeStart);
                string extractContent = "";
                while (cubeEnd > 0)
                {
                    extractContent += subContent.Substring(cubeStart + 1, cubeEnd - cubeStart - 1).Trim() + " ";
                    cubeStart = subContent.IndexOf(cubeStartContent, cubeEnd);
                    cubeEnd = subContent.IndexOf(cubeEndContent, cubeStart);
                }
                return extractContent.Trim();
            }
            catch (Exception e)
            {
                Console.WriteLine("get Repository Content error: " + e.Message);
                return "";
            }
        }
        public static List<string> getTopicsList(string htmlContent)
        {
            List<string> topicsList = new List<string>();
            string indexStartContent = "<div class=\"list-topics-container f6 mt-1\">";
            string indexEndContent = "</div>";
            string cubeStartContent = ">";
            string cubeEndContent = "<";
            try
            {
                int indexStart = htmlContent.IndexOf(indexStartContent);
                if (indexStart < 0)
                {
                    return topicsList;
                }
                int indexEnd = htmlContent.IndexOf(indexEndContent, indexStart);
                string subContent = htmlContent.Substring(indexStart + indexStartContent.Length, indexEnd - indexStart - indexStartContent.Length).Replace("\n", "").Replace("\t", "");
                int cubeStart = subContent.IndexOf(cubeStartContent);
                int cubeEnd = subContent.IndexOf(cubeEndContent, cubeStart);
                while (cubeEnd > 0)
                {
                    string topic = subContent.Substring(cubeStart + 1, cubeEnd - cubeStart - 1).Trim();
                    if (topic != null && topic != "")
                    {
                        topicsList.Add(topic);
                    }
                    cubeStart = subContent.IndexOf(cubeStartContent, cubeEnd);
                    cubeEnd = subContent.IndexOf(cubeEndContent, cubeStart);
                }
                return topicsList;
            }
            catch (Exception e)
            {
                Console.WriteLine("get Topics List error: " + e.Message);
                return topicsList;
            }
        }

        public static string getReadmeContent(string htmlContent, out string readmeSuffixName)
        {
            string indexStartContentFirst = "<div id=\"readme";
            string indexReadmeSuffixNameStartContent = "README.";
            string indexReadmeSuffixNameEndContent = "</";
            string indexStartContentSecond = "<article class=\"";
            string indexEndContent = "</article>";
            try
            {
                int indexStart = htmlContent.IndexOf(indexStartContentFirst);
                int indexReadmeSuffixNameStart = htmlContent.IndexOf(indexReadmeSuffixNameStartContent, indexStart);
                if (indexReadmeSuffixNameStart < 0)
                {
                    readmeSuffixName = "";
                }
                else
                {
                    int indexReadmeSuffixNameEnd = htmlContent.IndexOf(indexReadmeSuffixNameEndContent, indexReadmeSuffixNameStart);
                    readmeSuffixName = "." + htmlContent.Substring(indexReadmeSuffixNameStart + indexReadmeSuffixNameStartContent.Length,
                        indexReadmeSuffixNameEnd - indexReadmeSuffixNameStart - indexReadmeSuffixNameStartContent.Length).Replace("\n", "").Replace(" ", "");
                }
                indexStart = htmlContent.IndexOf(indexStartContentSecond, indexStart);
                int indexEnd = htmlContent.IndexOf(indexEndContent, indexStart);
                return ExtractText(htmlContent.Substring(indexStart, indexEnd - indexStart));
            }
            catch (Exception e)
            {
                //Console.WriteLine("get Readme Content error: " + e.Message);
                Logger.WriteLog("get Readme Content error: " + e.Message);
                readmeSuffixName = "";
                return "";
            }
        }



        private static string ExtractText(string inStrHtml)
        {
            string result = inStrHtml;
            result = RemoveComment(result);
            result = RemoveScript(result);
            result = RemoveStyle(result);
            result = RemoveTags(result);
            return result.Trim();
        }


        private static string RemoveComment(string input)
        {
            string result = input;
            //remove comment  
            result = Regex.Replace(result, @"<!--[^-]*-->", string.Empty, RegexOptions.IgnoreCase);
            return result;
        }
        private static string RemoveStyle(string input)
        {
            string result = input;
            //remove all styles  
            result = Regex.Replace(result, @"<style[^>]*?>.*?</style>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            return result;
        }
        private static string RemoveScript(string input)
        {
            string result = input;
            result = Regex.Replace(result, @"<script[^>]*?>.*?</script>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            result = Regex.Replace(result, @"<noscript[^>]*?>.*?</noscript>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            return result;
        }
        private static string RemoveTags(string input)
        {
            string result = input;
            result = result.Replace(" ", " ");
            result = result.Replace("'", "\"");
            result = result.Replace("<", "<");
            result = result.Replace(">", ">");
            result = result.Replace("&", "&");
            result = result.Replace("<br>", "\r\n");
            result = Regex.Replace(result, @"<[\s\S]*?>", string.Empty, RegexOptions.IgnoreCase);
            return result;
        }
    }
}
