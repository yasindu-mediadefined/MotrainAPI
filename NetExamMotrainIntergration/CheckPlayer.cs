using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetExamMotrainIntergration
{
    public class CheckPlayer
    {
        public string CheckExistingPlayer(string teamID, string email)
        {
            string requestChkeckingPlayerAPIUrl = $"https://api.motrainapp.com/v2/teams/{teamID}/users?email={email}";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(requestChkeckingPlayerAPIUrl);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";
            httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + NetExamMortrainIntegration.motrainAPIKey);
            var existingUserMail = string.Empty;
            try
            {

                using (var exstingPlayerResponse = httpWebRequest.GetResponse() as HttpWebResponse)
                {
                    if (httpWebRequest.HaveResponse && exstingPlayerResponse != null)
                    {
                        if (httpWebRequest.HaveResponse && exstingPlayerResponse != null)
                        {
                            using (var readerData = new StreamReader(exstingPlayerResponse.GetResponseStream()))
                            {
                                //API response
                                existingUserMail = readerData.ReadToEnd();
                                return existingUserMail;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string error = ex.ToString();
                NetExamMotrainFileGeneration.Logger.Error("CheckExisitingUser" + error);
                throw;
            }
            return existingUserMail;
        }
    }
}
