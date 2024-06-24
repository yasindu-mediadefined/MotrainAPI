using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetExamMotrainIntergration
{
    public class AwardCoins
    {
        //API endpoint URL
        private static string motrainAPIEndPoint = ConfigurationSettings.AppSettings["MotrainAPIEndPoint"];


        ////
        ///Award netexam points as motrain coins to the motrain players/netexam users <summary>
        /// Award netexam points as motrain coins to the motrain players/netexam users
        /// </summary>
        /// <param name="motrainUserID"></param>
        /// <param name="motrainCoins"></param>
        /// <returns>awardCoins</returns>
        public string AwardCoinstoMotrainPlayer( string motrainUserID, int motrainCoins, string courseName)
        {

            try
            {
                // Create the data object matching the JSON structure

                string jsonRequestBody = "{\"coins\":" + motrainCoins + "," +
                                            "\"reason\":{"+
                                                         "\"string\":\"transaction:credit.coursexcompleted\"," +
                                                         "\"args\":{\"name\":\""+ courseName +"\"}}}";

                //string jsonPayload = JsonConvert.SerializeObject(jsonRequestBody);

                string requestAwardCoinsAPIUrl = $"{motrainAPIEndPoint}/users/{motrainUserID}/balance";
                var httpCoinsWebRequest = (HttpWebRequest)WebRequest.Create(requestAwardCoinsAPIUrl);
                httpCoinsWebRequest.ContentType = "application/json";
                httpCoinsWebRequest.Method = "POST";
                httpCoinsWebRequest.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + NetExamMortrainIntegration.motrainAPIKey);
                var awardCoins = "";
                
                using (var streamWriter = new StreamWriter(httpCoinsWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(jsonRequestBody);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                try
                {
                    using (var awardCoinsResponse = (HttpWebResponse)httpCoinsWebRequest.GetResponse())
                    {
                        if (httpCoinsWebRequest.HaveResponse && awardCoinsResponse != null)
                        {
                            using (var readerData = new StreamReader(awardCoinsResponse.GetResponseStream()))
                            {
                                //API response
                                awardCoins = readerData.ReadToEnd();
                                
                                return awardCoins;
                            }
                        }

                    }
                }
                catch (WebException ex)
                {
                    using (HttpWebResponse response = (HttpWebResponse)ex.Response)
                    {
                        using (var streamReader = new StreamReader(response.GetResponseStream()))
                        {
                            string errorResponse = streamReader.ReadToEnd();
                            // Log and handle the error response
                            NetExamMotrainFileGeneration.Logger.Error("AwardCoinstoMotrainPlayer" + errorResponse);
                            throw;
                        }
                    }
                }
                return awardCoins;
            }
            catch (Exception ex)
            {
                string error = ex.ToString();
                NetExamMotrainFileGeneration.Logger.Error("AwardCoinstoMotrainPlayer" + error);
                throw;
            }

        }
    }
}
