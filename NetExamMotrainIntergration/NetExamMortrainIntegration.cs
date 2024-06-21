using System;
using System.IO;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Net;
using System.Text.Json;
using Newtonsoft.Json;
using System.Web.Script.Serialization;
using Nest;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using System.Linq;

namespace NetExamMotrainIntergration
{
    static class NetExamMortrainIntegration
    {
        public static string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
        private static string currentDate = DateTime.Now.ToString("yyyyMMdd.HHmmss");
        private static string LogFilePath = ConfigurationSettings.AppSettings["LogFileFolder"];
        public static string LOGFILENAME = "";
        private static StreamWriter sw;
        //  static bool errorEncountered = false;
        //Motrain API Key
        public static string motrainAPIKey ="Vq4mhejWyqwcPZ0d5hnEbSze7UOFoYvo7bUPcVRN4aRgMDrpFF8MomiFhurqB6PZ";
        //Motrain account ID
        private static string motrainAPIAccountID = "580b06e3-fe64-435a-a207-3cc58fa6791c";
        // The base address of the API endpoint
        private static string motrianRequestTeamAPIUrl = $"https://api.motrainapp.com/v2/accounts/{motrainAPIAccountID}/teams";
        private static string teamID =string.Empty;
        private static string accountID = string.Empty;
        private static string teamName = string.Empty;
        private static string tickets = string.Empty;

        private static string jsonString = string.Empty;
        private static CheckPlayer checkPlayer = new CheckPlayer();
        private static CreatePlayer createPlayer = new CreatePlayer();

        [STAThread]
        static void Main(string[] args)
        {
            
            
            log4net.Config.XmlConfigurator.Configure();
            //int iCompany = 11299;
            //Get the users and courses who completed the courses
            GetMotrainCourses();
        }

        /// <summary>
        /// Return users and courses who completed courses
        /// </summary>
        /// <param name="iCompany"></param>
        private static void GetMotrainCourses()
        {
            try
            {
                string UserID = string.Empty;
                int iCSID;
                int MotrainStatus;
                int coins;
                string firstName = string.Empty, lastName = string.Empty, email =string.Empty;
                string address1 = string.Empty, address2 = string.Empty, city = string.Empty, state = string.Empty, country=string.Empty;
                using (SqlConnection conn = new SqlConnection(connectionString)) 
                {
                    NetExamMotrainFileGeneration.Logger.Debug("GetMotrainCourses --");

                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("APIGetMotrainCourses", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        //cmd.Parameters.Add("@CompanyID", SqlDbType.Int).Value = iCompany;

                        SqlDataReader dreader = cmd.ExecuteReader();

                        if (dreader.Read())
                        {
                            while (dreader.Read())
                            {
                                UserID = dreader["UserID"].ToString();
                                iCSID = int.Parse(dreader["iCSID"].ToString());
                                MotrainStatus = int.Parse(dreader["MotrainStatus"].ToString());
                                coins = int.Parse(dreader["points"].ToString());
                                email = dreader["email"].ToString();
                                firstName = dreader["fname"].ToString();
                                lastName = dreader["lname"].ToString();
                                address1 = dreader["address1"].ToString();
                                address2 = dreader["address2"].ToString();
                                city = dreader["city"].ToString();
                                state = dreader["state"].ToString();
                                country = dreader["country"].ToString();

                                if (MotrainStatus == 0)
                                {
                                    ProcessMotrainAPI(UserID,iCSID,MotrainStatus,coins,email,firstName,lastName
                                        , address1, address2, city,state,country);
                                }
                            }

                        }
                        dreader.Close();
                    }


                }

            }
            catch (Exception ex)
            {
                string error = ex.ToString();
                NetExamMotrainFileGeneration.Logger.Error(error);
                throw;
            }
        }

        private static void ProcessMotrainAPI(string userID, int iCSID, int motrainStatus, int coins, string email, string firstName, string lastName
                                        , string adderss1, string adderss2, string city, string state, string country)
        {
            
            try
            {
                var httpTeamWebRequest = (HttpWebRequest)WebRequest.Create(motrianRequestTeamAPIUrl);
                httpTeamWebRequest.ContentType = "application/json";
                httpTeamWebRequest.Method = "GET";
                var motrainTeamAPIresult = string.Empty;
                var existingPlayer = string.Empty;
                var createdPlayerDtails =string.Empty;
                httpTeamWebRequest.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + motrainAPIKey);
                try
                {
                    
                    using (var motrainTeamResponse = httpTeamWebRequest.GetResponse() as HttpWebResponse)
                    {
                        if (httpTeamWebRequest.HaveResponse && motrainTeamResponse != null)
                        {
                            using (var readerData = new StreamReader(motrainTeamResponse.GetResponseStream()))
                            {
                                //API response
                                motrainTeamAPIresult = readerData.ReadToEnd();
                                // Check and print the type of each JSON response

                                //JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                                //var dict = (IDictionary<string, object>)jsonSerializer.DeserializeObject(motrainAPIresult);

                                // JSON array string into a JArray object.
                                JArray teamArray = JArray.Parse(motrainTeamAPIresult);
                                //creates an empty JSON object
                                JObject jsonObject = new JObject();

                                foreach (JObject item in teamArray)
                                {
                                    teamID = item["id"].ToString();
                                    //calling seperate web method to get exisiting users
                                    //CheckPlayer.cs
                                    existingPlayer = checkPlayer.CheckExistingPlayer(teamID,email);
                                    // Deserialize using Newtonsoft.Json
                                    List<Player> existingPlayerList = JsonConvert.DeserializeObject<List<Player>>(existingPlayer);
                                    if (existingPlayerList.Count == 0)
                                    {
                                        //calling seperate web method to post new users
                                        createdPlayerDtails = createPlayer.CreateMotrainPlayer(teamID, userID, iCSID, motrainStatus, coins, email, firstName, lastName
                                            , adderss1, adderss2, city, state, country);
                                    }
                                    else
                                    {
                                        Console.WriteLine("The JSON array is not empty (Newtonsoft.Json).");
                                        
                                    }
                                    jsonObject[teamID] = item;
                                }

                                //string resultJson = jsonObject.ToString();
                               
                                NetExamMotrainFileGeneration.Logger.Debug("Json Response:" + teamArray);
                                

                            }
                        }
                    }
                }
                catch (WebException e)
                {
                    if (e.Response != null)
                    {
                        using (var errorResponse = (HttpWebResponse)e.Response)
                        {
                            using (var readererorrDate = new StreamReader(errorResponse.GetResponseStream()))
                            {
                                string error = readererorrDate.ReadToEnd();
                                motrainTeamAPIresult = error;
                                NetExamMotrainFileGeneration.Logger.Debug("Json Response Error:" + motrainTeamAPIresult);
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                string error = ex.ToString();
                NetExamMotrainFileGeneration.Logger.Error(error);
                throw;
            }

        }


        private static void Log(string logString)
        {
            LOGFILENAME = LogFilePath + "log.txt";
            if (!File.Exists(LOGFILENAME))
            {
                sw = File.CreateText(LOGFILENAME);
            }
            else
            {
                sw = File.AppendText(LOGFILENAME);
            }
            sw.WriteLine(logString);
            sw.Close();
        }




    }
}

