using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace DynamicIP
{
    public class DynamicIP
    {
        static void DisplayHelp()
        {
            int padding = 25;
            Console.WriteLine("\nGoogle Dynamic DNS Updater Tool\n");

            Console.Write("/h [HostName]".PadRight(padding));
            Console.WriteLine("Specify the host name to update.");
            Console.Write("".PadRight(padding));
            Console.WriteLine("Use @ for the root domain.");

            Console.Write("/u [Username]".PadRight(padding));
            Console.WriteLine("Specify the Google provided username.");

            Console.Write("/p [Password]".PadRight(padding));
            Console.WriteLine("Specify the access password that has been");
            Console.Write("".PadRight(padding));
            Console.WriteLine("assigned to the domain.");

            Console.Write("/i [IP Address]".PadRight(padding));
            Console.WriteLine("Set the domain to a specific IP address.");
            Console.Write("".PadRight(padding));
            Console.WriteLine("If this option is omitted, the host");
            Console.Write("".PadRight(padding));
            Console.WriteLine("machine's detected external IP address will be used.");

            Console.Write("/v".PadRight(padding));
            Console.WriteLine("Display verbose output. This will display the domain");
            Console.Write("".PadRight(padding));
            Console.WriteLine("name, password, and IP address used.");

            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            bool verbose = false;
            string baseURL, hostName = "", user = "", password = "", ipAddress = "", directory;
            baseURL = "https://domains.google.com";
            directory = "/nic/update";


            //Uncomment the lines below to hard code the values into the program.
            //This will allow you to run without command line parameters.		

            //hostName = "example";
            //zone = "enom.com";
            //domainPassword = "domainPassword";

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "/?")
                {
                    DisplayHelp();
                    return;
                }
                else if (i + 1 < args.Length)
                {
                    switch (args[i])
                    {
                        case "/h":
                            hostName = args[++i];
                            break;

                        case "/u":
                            user = args[++i];
                            break;

                        case "/p":
                            password = args[++i];
                            break;

                        case "/i":
                            ipAddress = args[++i];
                            break;

                        case "/v":
                            verbose = true;
                            break;

                        default:
                            Console.WriteLine("Unrecognized or invalid parameter(s). Use /? for help\n");
                            return;
                    }
                }
                else
                {
                    Console.WriteLine("Unrecognized or invalid parameter(s). Use /? for help\n");
                    return;
                }
            }

            if (string.IsNullOrEmpty(hostName) || 
                string.IsNullOrEmpty(user) || 
                string.IsNullOrEmpty(password))
            {
                DisplayHelp();
                return;
            }

            if (verbose)
            {
                Console.WriteLine("Updating " + hostName);
                Console.WriteLine("Using " + user + ":" + password);
            }

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseURL);
                var request = new HttpRequestMessage(HttpMethod.Post, directory);
                var auth = new UTF8Encoding().GetBytes(string.Format("{0}:{1}", user, password));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(auth));
                client.DefaultRequestHeaders.Add("User-Agent", "GohanDNS/1");

                var data = new Dictionary<string, string>
                {
                    { "hostname", hostName }
                };

                if (!string.IsNullOrEmpty(ipAddress))
                {
                    data.Add("myip", ipAddress);
                }

                request.Content = new FormUrlEncodedContent(data);

                var response = client.SendAsync(request).GetAwaiter().GetResult();
                Console.WriteLine(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            }


            //Old approach for outputting results...
            //
            //while (responseReader.Read())
            //{
            //    if (responseReader.NodeType == XmlNodeType.Element)
            //    {
            //        if (responseReader.Name == "IP" && responseReader.Read())
            //        {
            //            Console.WriteLine("IP address used: {0}", responseReader.Value);
            //        }
            //        else if (responseReader.Name == "ErrCount" && responseReader.Read())
            //        {
            //            Console.WriteLine("There were {0} errors", responseReader.Value);
            //        }
            //        else if (responseReader.Name == "Err1" && responseReader.Read())
            //        {
            //            Console.WriteLine("The error returned was \"{0}\"", responseReader.Value);
            //            errors = true;
            //        }
            //        else if (!errors && responseReader.Name == "Done" && responseReader.Read())
            //        {
            //            Console.WriteLine("The request to update {0}.{1} {2} submitted successfully", hostName, zone,
            //            responseReader.Value == "true" ? "was" : "was not");
            //        }
            //    }
            //}
        }
    }
}