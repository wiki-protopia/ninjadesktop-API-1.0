using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using NinjaTrader.Client;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace TestConnect
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Initialize NinjaTrader client
            Client ninjaTraderClient = new Client();

            // Connect to NinjaTrader
            int connect = ninjaTraderClient.Connected(1);
            Console.WriteLine(string.Format("{0} | connect: {1}", DateTime.Now, connect.ToString()));

            string url = "http://localhost:8080/webhook/";
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine("Listening for webhook notifications on " + url);

            // Handle incoming webhook notifications
            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;

                if (request.HttpMethod == "POST")
                {
                    using (Stream body = request.InputStream)
                    {
                        using (StreamReader reader = new StreamReader(body, request.ContentEncoding))
                        {
                            string requestBody = reader.ReadToEnd();
                            Console.WriteLine("Received webhook notification:");
                            Console.WriteLine(requestBody);

                            // Parse JSON data
                            JObject json = JObject.Parse(requestBody);
                            // Now you can access data from the JSON object
                            // For example:
                            Console.WriteLine("Event Type: " + json.ToString());

                            // Process the webhook payload here
                            int qty = (int)json["qty"];

                            int placeresult = ninjaTraderClient.Command("PLACE", "SIM101", "My Externel", "BUY", 1, "MARKET", 100, 100, "DAY", null, null, null, null);

                            string propertyName = null, propertyValue = null;

                            for (int i = 1; i <= qty; i++)
                            {
                                propertyName = $"take_profit_{i}_price";
                                Console.WriteLine(propertyName);
                                propertyValue = (string)json[propertyName];

                                Dictionary<string, object> userData = new Dictionary<string, object>
                                {
                                    { "alert", (string)json["alert"] },
                                    { "account", (string)json["account"] },
                                    { "ticker", (string)json["ticker"] },
                                    { "qty", 1 },
                                    { propertyName, propertyValue },
                                    { "stop_price", (string)json["stop_price"] },
                                    { "tif", (string)json["tif"] },
                                    { "oco_id", (string)json["oco_id"] }
                                };
                                //string jsonData = JsonSerializer.Serialize(userData);
                                Console.WriteLine(userData);
                            }

                            var breakData = new Dictionary<string, object>
                            {
                                { "alert", "Adjusted OCO Short" },
                                { "account", (string)json["account"] },
                                { "ticker", (string)json["ticker"] },
                                { "qty", (string)json["qty"] },
                                { propertyName, propertyValue },
                                { "stop_price", (string)json["stop_price"] },
                                { "tif", (string)json["tif"] },
                                { "oco_id", "Same trade num for TP2 OCO_ID" }
                            };

                            // string breakjsonData = JsonSerializer.Serialize(breakData);
                            Console.WriteLine(breakData);

                        }
                    }
                }

                HttpListenerResponse response = context.Response;
                string responseString = "<html><body>Webhook received successfully!</body></html>";
                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }

            /*if (ninjaTraderClient.Connect(1))
            {
                Console.WriteLine("Connected to NinjaTrader!");

                // Do whatever you need to do after connecting
                // For example, you can subscribe to events, request data, etc.
            }
            else
            {
                Console.WriteLine("Failed to connect to NinjaTrader!");
            }*/
        }
    }
}
