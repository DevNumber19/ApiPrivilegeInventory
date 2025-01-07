using System;
using System.Configuration;
using System.IO;
using System.Net;

namespace TriggerAlertRemainingDeal
{
    class Program
    {
        static void Main(string[] args)
        {
            string URL = ConfigurationManager.AppSettings["url"].ToLower();
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(URL);
            httpWebRequest.Method = "GET";

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            string response;

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                response = streamReader.ReadToEnd();
            }
        }
    }
}
