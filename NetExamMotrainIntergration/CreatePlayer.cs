using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NetExamMotrainIntergration
{
    public class CreatePlayer
    {
        public string CreateMotrainPlayer(string teamID, string userID, int iCSID, int motrainStatus, int coins, string email, string firstName, string lastName
                                        , string adderss1, string adderss2, string city, string state, string country)
        {
            var createdPlayerDetails = string.Empty;
            try
            {
                // Create the data object matching the JSON structure
                var requestData = new
                {
                    firstname = firstName,
                    lastname = lastName,
                    email = email,
                    shipping = new
                    {
                        location_name = "",
                        location_id = "",
                        address_line_1 = adderss1,
                        address_line_2 = adderss2,
                        city = city,
                        postcode = "",
                        state = state,
                        country = country
                    }
                };


                // Convert object to JSON string
                string jsonBody = JsonConvert.SerializeObject(requestData);
                string requestcreatePlayerAPIUrl = $"https://api.motrainapp.com/v2/teams/{teamID}/users";
                var httpPlayerWebRequest = (HttpWebRequest)WebRequest.Create(requestcreatePlayerAPIUrl);
                httpPlayerWebRequest.ContentType = "application/json";
                httpPlayerWebRequest.Method = "POST";
                httpPlayerWebRequest.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + NetExamMortrainIntegration.motrainAPIKey);
                

                using (var streamWriter = new StreamWriter(httpPlayerWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(jsonBody);
                    streamWriter.Flush();
                    streamWriter.Close();

                }

                using (var createPlayerResponse = (HttpWebResponse)httpPlayerWebRequest.GetResponse())
                {
                        if (httpPlayerWebRequest.HaveResponse && createPlayerResponse != null)
                        {
                            using (var readerData = new StreamReader(createPlayerResponse.GetResponseStream()))
                            {
                                //API response
                                createdPlayerDetails = readerData.ReadToEnd();
                                return createdPlayerDetails;
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
            return createdPlayerDetails;
        }
    }
}
