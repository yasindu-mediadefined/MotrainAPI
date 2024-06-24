using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetExamMotrainIntergration
{
    public class CreatePlayer
    {
        //API endpoint URL
        private static string motrainAPIEndPoint = ConfigurationSettings.AppSettings["MotrainAPIEndPoint"];
        private static string requestBody;
        //private static JObject createdPlayerDetails = new JObject();
        
        public string CreateMotrainPlayer(string teamID, string userID, int iCSID, int motrainStatus, int coins, string email, string firstName, string lastName
                                        , string adderss1, string adderss2, string city, string state, string country)
        {
            
            try
            {
                // Create the data object matching the JSON structure
                country = country.Substring(0, 2);
                var requestBody = new
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
               
                //string jsonRequestBody = "{\"firstname\":\"" + firstName + "\"," +
                //                        "\"lastname\":\"" + lastName + "\"," +
                //                        "\"email\":\"" + email + "\"," +
                //                        "\"shipping\":{\"location_name\":\"\"," +
                //                                     "\"location_id\":\"\"," +
                //                                     "\"address_line_1\":\"" + adderss1 + "\"," +
                //                                     "\"address_line_2\":\"" + adderss2 + "\"," +
                //                                     "\"city\":\"" + city + "\"," +
                //                                     "\"postcode\":\"\"," +
                //                                     "\"state\":\"" + state + "\"," +
                //                                     "\"country\":\"" + country + "\"}" +
                //                                     "}";



                string jsonPayload = JsonConvert.SerializeObject(requestBody);

                string requestcreatePlayerAPIUrl = $"{motrainAPIEndPoint}/teams/{teamID}/users";
                var httpPlayerWebRequest = (HttpWebRequest)WebRequest.Create(requestcreatePlayerAPIUrl);
                httpPlayerWebRequest.ContentType = "application/json";
                httpPlayerWebRequest.Method = "POST";
                httpPlayerWebRequest.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + NetExamMortrainIntegration.motrainAPIKey);
                var createdPlayerDetails = "";
                //httpPlayerWebRequest.ContentLength = data.Length;

                // Write the request body
                //using (var stream = httpPlayerWebRequest.GetRequestStream())
                //{
                //    stream.Write(data, 0, data.Length);
                //}

                // Read and log the request body before sending it

                //using (var stream = new MemoryStream())
                //{
                //    stream.Write(data, 0, data.Length);
                //    stream.Position = 0;
                //    using (var reader = new StreamReader(stream))
                //    {
                //        requestBody = reader.ReadToEnd();
                //    }
                //}
                using (var streamWriter = new StreamWriter(httpPlayerWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(jsonPayload);
                    streamWriter.Flush();
                    streamWriter.Close();

                }

                try
                {
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
                catch (WebException ex)
                {
                    using (HttpWebResponse response = (HttpWebResponse)ex.Response)
                    {
                        using (var streamReader = new StreamReader(response.GetResponseStream()))
                        {
                            string errorResponse = streamReader.ReadToEnd();
                            // Log and handle the error response
                            NetExamMotrainFileGeneration.Logger.Error("CheckExisitingUser" + errorResponse);
                            throw;
                        }
                    }
                }
                return createdPlayerDetails;
            }
            catch (Exception ex)
            {
                string error = ex.ToString();
                NetExamMotrainFileGeneration.Logger.Error("CreateMotrainUser" + error);
                throw;
            }
           
        }
    }
}
