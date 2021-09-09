using System;
using System.Data;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Http.Headers;
using System.Threading.Tasks;


namespace DVPP_PremisesandAwsconnectivity
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                bool continuepar = false;
                Program p = new Program();
                Console.WriteLine("App conectivity test application.");
                Console.WriteLine("Press 1 for connecting application.");
                Console.WriteLine("Press 2 for database connectivity.");
                Console.WriteLine("Press 3 for network connectivity test.");
                Console.WriteLine("Press 4 to exit.");
                do
                {
                    string input = Console.ReadLine();
                    if (input == "1" || input == "2" || input == "3" || input == "4")
                    {
                        switch (input)
                        {
                            case "1":
                                Console.WriteLine("Your input for case 1 is: {0}", input);
                                p.TestApplicationConectivity().Wait();
                                break;
                            case "2":
                                Console.WriteLine("Your input for case 2 is: {0}", input);
                                p.TestDatabaseConectivity();
                                break;
                            case "3":
                                Console.WriteLine("Your input for case 2 is: {0}", input);
                                p.TestNetworkConectivity();
                                break;
                            case "4":
                                Console.WriteLine("Your input for case 4 is: {0}", input);
                                Environment.Exit(0);
                                break;
                            default:
                                Console.WriteLine("Your input is not correct: {0}", input);
                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please enter 1, 2 or 3.");
                    }

                } while (continuepar == false);
              
            }
            catch (Exception ex)
            {

                string m = ex.Message;
            }

        }

       
        public async Task TestApplicationConectivity()
        {
            string url = ConfigurationManager.AppSettings["WebURL"];
            string userName = ConfigurationManager.AppSettings["WebUserName"];
            string password = ConfigurationManager.AppSettings["WebPassword"];
            string apiFunction = ConfigurationManager.AppSettings["ApiFunction"];
            Console.WriteLine("Url that you are trying to connect." + "  " + url);
            Console.WriteLine("Username=" + userName);
            Console.WriteLine("Password=" + password);
            string callingUrl = url;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(userName+":"+ password));  
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfo);
                #region Consume GET method  
                HttpResponseMessage response = await client.GetAsync(apiFunction);
                if (response.IsSuccessStatusCode)
                {
                    string httpResponseResult = response.Content.ReadAsStringAsync().ContinueWith(task => task.Result).Result;
                    Console.WriteLine("Result: " + httpResponseResult);
                    Console.WriteLine("StatusCode =" + (int)response.StatusCode);
                 }
                else
                {
                    Console.WriteLine("StatusCode =" + (int)response.StatusCode);
                }
            }
                #endregion
             
            }
            public void TestDatabaseConectivity()
             {
            string connectionString = ConfigurationManager.AppSettings["Aws_DataBase"];
            string SqlQuery = ConfigurationManager.AppSettings["SqlQuery"];
            Console.WriteLine("Database that you are trying to connect." + "  " + connectionString);
            Console.WriteLine("Query that will be fired on database=" + SqlQuery);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                if (connection.State == ConnectionState.Open)
                {
                    Console.WriteLine("Connection opened successfully.");
                }
                using (SqlCommand command = new SqlCommand(SqlQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                Console.WriteLine(reader.GetValue(i));
                            }
                            Console.WriteLine();
                        }
                    }
                }
            }
        }
        public void TestNetworkConectivity()
        {
            string Host= ConfigurationManager.AppSettings["NetworkHost"];
             bool status = PingHost(Host);
             Ping ping = new Ping();
            PingReply pingresult = ping.Send(Host);
           if (status)
            {
                Console.WriteLine("Connection opened successfully with IP= "+ Host);
            }
            else
            {
                Console.WriteLine("Unable to open connection with IP= " + Host);
            }
            string inputPath = ConfigurationManager.AppSettings["SourceFolder"];
            Console.WriteLine("Started reading file from  " + inputPath);
            string path = ConfigurationManager.AppSettings["DestinationFolder"];
            string text = System.IO.File.ReadAllText(inputPath);
            using (StreamWriter sw = File.CreateText(path))
            {
                Console.WriteLine("Started writing file to  " + path);
                sw.WriteLine(text);
                
            }
            Console.WriteLine("File writing completed");
           
        }
        public static bool PingHost(string nameOrAddress)
        {
            bool pingable = false;
            Ping pinger = null;

            try
            {
                pinger = new Ping();
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }
            finally
            {
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }

            return pingable;
        }
    }
}
