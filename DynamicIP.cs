using System;
using System.Linq;
using System.Net;
using System.Xml;

namespace DynamicIP
{
    public class DynamicIP
    {
        static void DisplayHelp()
        {
            int padding = 25;
            Console.WriteLine("\neNom Dynamic DNS Updater Tool\n");

            Console.Write("/h [HostName]".PadRight(padding));
            Console.WriteLine("Specify the host name to update.");
            Console.Write("".PadRight(padding));
            Console.WriteLine("Use @ for the root domain.");

            Console.Write("/z [Zone]".PadRight(padding));
            Console.WriteLine("Specify the SLD.TLD to update, such as enom.com.");

            Console.Write("/p [Domain Password]".PadRight(padding));
            Console.WriteLine("Specify the access password that has been");
            Console.Write("".PadRight(padding));
            Console.WriteLine("assigned to the domain.");
            Console.Write("".PadRight(padding));
            Console.WriteLine("Must first set under General Settings for the domain.");

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
            bool errors = false, verbose = false;
            string baseURL, hostName = "", zone = "", domainPassword = "", ipAddress = "", requestString;
            baseURL = "http://dynamic.name-services.com/interface.asp?Command=SetDNSHost&responseType=XML";

            //Uncomment the lines below to hard code the values into the program.
            //This will allow you to run without command line parameters.		

            //hostName = "example";
            //zone = "enom.com";
            //domainPassword = "domainPassword";

            for (int i = 0; i < args.Count(); i++)
            {
                if (args[i] == "/?")
                {
                    DisplayHelp();
                    return;
                }
                else if (i + 1 < args.Count())
                {
                    switch (args[i])
                    {
                        case "/h":
                            hostName = args[++i];
                            break;

                        case "/z":
                            zone = args[++i];
                            break;

                        case "/p":
                            domainPassword = args[++i];
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

            if (string.IsNullOrEmpty(hostName) || string.IsNullOrEmpty(zone) || string.IsNullOrEmpty(domainPassword))
            {
                DisplayHelp();
                return;
            }

            requestString = baseURL + "&HostName=" + hostName + "&zone=" + zone + "&DomainPassword=" + domainPassword;

            if (!string.IsNullOrEmpty(ipAddress))
            {
                requestString += "&Address=" + ipAddress;
            }

            if (verbose)
            {
                Console.WriteLine("Updating " + hostName + "." + zone);
                Console.WriteLine("Using password " + domainPassword);
            }

            WebRequest webGetRequest = WebRequest.Create(requestString);
            XmlReader responseReader = XmlReader.Create(webGetRequest.GetResponse().GetResponseStream());

            while (responseReader.Read())
            {
                if (responseReader.NodeType == XmlNodeType.Element)
                {
                    if (responseReader.Name == "IP" && responseReader.Read())
                    {
                        Console.WriteLine("IP address used: {0}", responseReader.Value);
                    }
                    else if (responseReader.Name == "ErrCount" && responseReader.Read())
                    {
                        Console.WriteLine("There were {0} errors", responseReader.Value);
                    }
                    else if (responseReader.Name == "Err1" && responseReader.Read())
                    {
                        Console.WriteLine("The error returned was \"{0}\"", responseReader.Value);
                        errors = true;
                    }
                    else if (!errors && responseReader.Name == "Done" && responseReader.Read())
                    {
                        Console.WriteLine("The request to update {0}.{1} {2} submitted successfully", hostName, zone,
                        responseReader.Value == "true" ? "was" : "was not");
                    }
                }
            }
        }
    }
}