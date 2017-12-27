using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Xml;

namespace PhotonControl.Helper
{
    public class GameServerConfigChanger
    {
        // Fields
        private static int ipLookupRetryCount = 0;
        private static readonly string[] ipLookupUrls = new string[] { "http://licensesp.exitgames.com/echoip", "http://api-sth01.exip.org/?call=ip", "http://api-ams01.exip.org/?call=ip", "http://api-nyc01.exip.org/?call=ip", "http://licensespch.exitgames.com/echoip" };
        private static object lockObject;
        private static Thread publicIpLookupThread;

        // Events
        public static event GetPublicIpCompletedHandler GetPublicIpCompleted;

        // Methods
        public static void DoGetPublicIPAsync()
        {
            try
            {
                if (ipLookupRetryCount <= ipLookupUrls.Length)
                {
                    string uriString = ipLookupUrls[ipLookupRetryCount];
                    WebClient client = new WebClient
                    {
                        Proxy = null
                    };
                    client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:12.0) Gecko/20100101 Firefox/12.0");
                    client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(GameServerConfigChanger.GetPublicIPAsyncCompleted);
                    Uri address = new Uri(uriString);
                    client.DownloadStringAsync(address);
                }
            }
            catch
            {
            }
        }

        public static bool EditConfigFiles(string pathToConfigs, string newPublicIpValue, out string message)
        {
            message = null;
            foreach (string str in pathToConfigs.Split(new char[] { ',', ';' }))
            {
                if (!File.Exists(str))
                {
                    message = string.Format("{0}: {1}", Program.ResourceManager.GetString("exceptionFileNotFound"), str);
                }
                else
                {
                    try
                    {
                        XmlDocument document = new XmlDocument
                        {
                            PreserveWhitespace = true
                        };
                        document.Load(str);
                        XmlNode node = document.SelectSingleNode("//setting[@name='PublicIPAddress']/value");
                        if (node != null)
                        {
                            node.InnerText = newPublicIpValue;
                        }
                        document.Save(str);
                    }
                    catch (Exception exception)
                    {
                        message = exception.Message;
                        return false;
                    }
                }
            }
            return true;
        }

        public static string GetCurrentConfigIp(string pathToConfigs, out string errorMessage)
        {
            errorMessage = null;
            string path = pathToConfigs.Split(new char[] { ',', ';' })[0];
            if (!File.Exists(path))
            {
                errorMessage = string.Format("{0}: {1}", Program.ResourceManager.GetString("exceptionFileNotFound"), path);
                return null;
            }
            XmlTextReader reader = null;
            try
            {
                reader = new XmlTextReader(path);
                XmlDocument document = new XmlDocument();
                document.Load(reader);
                if (document.DocumentElement == null)
                {
                    errorMessage = Program.ResourceManager.GetString("gameServerConfigInvalidMsg");
                    return null;
                }
                foreach (XmlNode node in document.DocumentElement.GetElementsByTagName("setting"))
                {
                    if (((node != null) && (node.Attributes != null)) && ((node.Attributes["name"].Value != null) && node.Attributes["name"].Value.Equals("PublicIPAddress")))
                    {
                        return node.FirstChild.InnerText;
                    }
                }
            }
            catch (Exception exception)
            {
                errorMessage = exception.Message;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
            return null;
        }

        public static bool GetLocalIPs(out string[] ips)
        {
            List<string> list = new List<string>();
            try
            {
                foreach (IPAddress address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                {
                    if (address.AddressFamily.ToString() == "InterNetwork")
                    {
                        string item = address.ToString();
                        if (item.StartsWith("127"))
                        {
                            list.Add(item);
                        }
                        else
                        {
                            list.Insert(0, item);
                        }
                    }
                }
                ips = list.ToArray();
                return true;
            }
            catch
            {
                ips = new string[0];
                return false;
            }
        }

        public static void GetPublicIPAsync()
        {
            publicIpLookupThread = new Thread(new ThreadStart(GameServerConfigChanger.DoGetPublicIPAsync));
            publicIpLookupThread.Start();
        }

        private static void GetPublicIPAsyncCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ipLookupRetryCount++;
                if (ipLookupRetryCount < ipLookupUrls.Length)
                {
                    GetPublicIPAsync();
                }
            }
            else if (GetPublicIpCompleted != null)
            {
                GetPublicIpCompleted(e.Result);
            }
        }

        // Nested Types
        public delegate void GetPublicIpCompletedHandler(string publicIp);
    }
}
