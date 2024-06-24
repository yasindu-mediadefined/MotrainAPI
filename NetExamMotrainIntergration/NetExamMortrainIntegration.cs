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
using Elasticsearch.Net;
using System.Windows.Input;

namespace NetExamMotrainIntergration
{
    static class NetExamMortrainIntegration
    {
        public static string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
        private static string currentDate = DateTime.Now.ToString("yyyyMMdd.HHmmss");
        private static string LogFilePath = ConfigurationSettings.AppSettings["LogFileFolder"];
        public static string LOGFILENAME = "";
        private static StreamWriter sw;

        //API endpoint URL
        private static string motrainAPIEndPoint = ConfigurationSettings.AppSettings["MotrainAPIEndPoint"];
        //Motrain API Key
        public static string motrainAPIKey = ConfigurationSettings.AppSettings["MotrainAPIKey"];
        //Motrain account ID
        private static string motrainAPIAccountID = ConfigurationSettings.AppSettings["MotrainAPIAccountID"];

        private static string motrianRequestTeamAPIUrl = $"{motrainAPIEndPoint}/accounts/{motrainAPIAccountID}/teams";
        private static string teamID =string.Empty;
        private static string accountID = string.Empty;
        private static string teamName = string.Empty;
        private static string tickets = string.Empty;

        private static string jsonString = string.Empty;
        private static CheckPlayer checkPlayer = new CheckPlayer();
        private static CreatePlayer createPlayer = new CreatePlayer();
        private static AwardCoins awardCoins = new AwardCoins();
        private static int motrainPlayerCoins;
        private static string motrainPlayerUserID;


        /// <summary>
        /// The main entry point for the motrain application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            
            
            log4net.Config.XmlConfigurator.Configure();
            //Get the users and courses who completed the courses
            GetMotrainCourses();
        }

        /// <summary>
        /// Return users and courses who completed courses
        /// </summary>
       
        private static void GetMotrainCourses()
        {
            try
            {
                string UserID = string.Empty;
                int iCSID;
                int MotrainStatus;
                int coursePoins;
                string firstName = string.Empty, lastName = string.Empty, email =string.Empty;
                string address1 = string.Empty, address2 = string.Empty, city = string.Empty, state = string.Empty, country=string.Empty, courseName=string.Empty;
                using (SqlConnection conn = new SqlConnection(connectionString)) 
                {
                    NetExamMotrainFileGeneration.Logger.Debug("GetMotrainCourses -- Begin");

                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("APIGetMotrainCourses", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        SqlDataReader dreader = cmd.ExecuteReader();

                        if (dreader.Read())
                        {
                            while (dreader.Read())
                            {
                                UserID = dreader["UserID"].ToString();
                                iCSID = int.Parse(dreader["iCSID"].ToString());
                                MotrainStatus = int.Parse(dreader["MotrainStatus"].ToString());
                                coursePoins = int.Parse(dreader["points"].ToString());
                                email = dreader["email"].ToString();
                                firstName = dreader["fname"].ToString();
                                lastName = dreader["lname"].ToString();
                                address1 = dreader["address1"].ToString();
                                address2 = dreader["address2"].ToString();
                                city = dreader["city"].ToString();
                                state = dreader["state"].ToString();
                                country = dreader["country"].ToString();
                                courseName = dreader["nvName"].ToString();

                                if (MotrainStatus == 0)
                                {
                                    //process the motrain API from here
                                    ProcessMotrainAPI(UserID,iCSID,courseName,MotrainStatus, coursePoins, email,firstName,lastName
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

        /// <summary>
        /// Connecting with Motrain API end points
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="iCSID"></param>
        /// <param name="motrainStatus"></param>
        /// <param name="coursePoints"></param>
        /// <param name="email"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="adderss1"></param>
        /// <param name="adderss2"></param>
        /// <param name="city"></param>
        /// <param name="state"></param>
        /// <param name="country"></param>
        private static void ProcessMotrainAPI(string userID, int iCSID,string courseName, int motrainStatus, int coursePoints, string email, string firstName, string lastName
                                        , string adderss1, string adderss2, string city, string state, string country)
        {
            
            try
            {
                //Check the available Motrain teams
                var httpTeamWebRequest = (HttpWebRequest)WebRequest.Create(motrianRequestTeamAPIUrl);
                httpTeamWebRequest.ContentType = "application/json";
                httpTeamWebRequest.Method = "GET";
                var motrainTeamAPIresult = string.Empty;
                var existingPlayer = "";
                var createdPlayerDtails ="";
                var awardCoinstoMotrainPlayer = "";
                //string createdPlayerDtails = new JObject();
                httpTeamWebRequest.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + motrainAPIKey);
                try
                {
                    
                    using (var motrainTeamResponse = httpTeamWebRequest.GetResponse() as HttpWebResponse)
                    {
                        if (httpTeamWebRequest.HaveResponse && motrainTeamResponse != null)
                        {
                            using (var readerData = new StreamReader(motrainTeamResponse.GetResponseStream()))
                            {
                                //Motrain teams API response
                                motrainTeamAPIresult = readerData.ReadToEnd();
                                // JSON array string into a JArray object
                                JArray teamArray = JArray.Parse(motrainTeamAPIresult);
                                //creates an empty JSON object
                                JObject jsonObject = new JObject();

                                foreach (JObject item in teamArray)
                                {
                                    teamID = item["id"].ToString();
                                    //Check Existing Motrain Player
                                    existingPlayer = checkPlayer.CheckExistingPlayer(teamID,email);
                                    // Parse the JSON string array to json array
                                    JArray jsonExistingPlayerArray = JArray.Parse(existingPlayer);
                                    
                                    if (jsonExistingPlayerArray.Count == 0)
                                    {
                                        //Create New Motrain Player
                                        createdPlayerDtails = createPlayer.CreateMotrainPlayer(teamID, email, firstName, lastName
                                            , adderss1, adderss2, city, state, country);
                                       
                                        // Convert JSON string to JObject
                                        JObject jsonObjectCreatePlayer = JsonConvert.DeserializeObject<JObject>(createdPlayerDtails);
                                        motrainPlayerUserID = jsonObjectCreatePlayer["id"].ToString();
                                        //Awards coins to Motrain Newly Player
                                        awardCoinstoMotrainPlayer = awardCoins.AwardCoinstoMotrainPlayer(motrainPlayerUserID, coursePoints, courseName);
                                        if (awardCoinstoMotrainPlayer.Length > 0)
                                        {
                                            using (SqlConnection conn = new SqlConnection(connectionString))
                                            {
                                                conn.Open();
                                                //Update Motrain table
                                                using (SqlCommand cmd = new SqlCommand("UpdateMotrainStatus", conn))
                                                {
                                                    cmd.CommandType = CommandType.StoredProcedure;
                                                    cmd.Parameters.Add("@motrainStatus", SqlDbType.Int).Value = 1;
                                                    cmd.Parameters.Add("@UserID", SqlDbType.Text).Value = userID;
                                                    cmd.Parameters.Add("@iCSID", SqlDbType.Int).Value = iCSID;
                                                    cmd.Parameters.Add("@motrainPlayerUserID", SqlDbType.Text).Value = motrainPlayerUserID;

                                                    SqlDataReader dreader = cmd.ExecuteReader();

                                                    if (dreader.Read())
                                                    {
                                                        string finalresult = dreader["result"].ToString();

                                                    }
                                                    dreader.Close();
                                                }
                                                conn.Close();
                                            }

                                        }
                                    }
                                    else
                                    {
                                        foreach (JObject existingPlayerObject in jsonExistingPlayerArray)
                                        {

                                            motrainPlayerUserID = existingPlayerObject["id"].ToString();
                                            //Awards coins to Motrain Newly Player
                                            awardCoinstoMotrainPlayer = awardCoins.AwardCoinstoMotrainPlayer(motrainPlayerUserID, coursePoints, courseName);
                                            if (awardCoinstoMotrainPlayer.Length > 0)
                                            {
                                                using (SqlConnection conn = new SqlConnection(connectionString))
                                                {
                                                    conn.Open();
                                                    //Update Motrain table
                                                    using (SqlCommand cmd = new SqlCommand("UpdateMotrainStatus", conn))
                                                    {
                                                        cmd.CommandType = CommandType.StoredProcedure;
                                                        cmd.Parameters.Add("@motrainStatus", SqlDbType.Int).Value = 1;
                                                        cmd.Parameters.Add("@UserID", SqlDbType.Text).Value = userID;
                                                        cmd.Parameters.Add("@iCSID", SqlDbType.Int).Value = iCSID;
                                                        cmd.Parameters.Add("@motrainPlayerUserID", SqlDbType.Text).Value = motrainPlayerUserID;

                                                        SqlDataReader dreader = cmd.ExecuteReader();

                                                        if (dreader.Read())
                                                        {
                                                            string finalresult = dreader["result"].ToString();

                                                        }
                                                        dreader.Close();
                                                    }
                                                    conn.Close();
                                                }

                                            }
                                        }

                                    }
                                   
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

