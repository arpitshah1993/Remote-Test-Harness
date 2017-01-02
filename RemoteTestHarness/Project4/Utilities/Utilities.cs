/////////////////////////////////////////////////////////////////////
// Utilities.cs - It provides static methods to process the inputs //
//                                                                 //
// Application: CSE681 - Software Modelling and Analysis,          //
//  Remote Test Harness Project-4                                  //
// Author:      Arpit Shah, Syracuse University,                   //
//              aushah@syr.edu, (646) 288-9410                     //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operation:
 * ================
*  It has bunch of static objects and methods. This methods and objects
*  will be used by all the class and packages when they need to process
*  some their inputs.
 * 
 * Public Interface 
 * ================
 *  public static void title(this string aString, char underline = '-') // Print the message as a title
 *  static public string processCommandLineForLocal(string[] args, string localUrl) // getting ports and addresses from commandline
 * static public bool waitForFileToClose(int count, string path) //Waits for the files to close if it is open for some time
 *  static public string ReadFromBinrayToText(string filePath) //Waits for the files to close if it is open for some time
 *   static public bool IsFileInUse(string path) //Will return true if file is open or in use.
 *  static public List<string> GetFilesFromTestRequest(TestRequest testRequest) //Get the all the file names from the test request
 *  static public string ToXml(this object obj) //Convert any type of object to XML and return it.
 *  static public T FromXml<T>(this string xml)//deserialize XML to object
 *  static public string processCommandLineForRemote(string[] args, string remoteUrl)//getting ports and addresses from commandline
 *    public static void waitForUser() //wait for user till it gets some input
 *     public static void showMessage(Message msg)//print the message
 *  
 *  
 * Build Process
 * =============
 * - Required Files: TestRequest.cs Utlities.cs ICommService.cs
 * - Compiler Command: csc TestRequest.cs Utlities.cs ICommService.cs
 * 
 * Maintainance History
 * ====================
 * ver 1.0 : 20 November 2016
 *     - first release 
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace Project4
{
    static public class Utilities
    {
        public static string testHarness { get; set; } = "http://localhost:8081/CommService";
        public static string repository { get; set; } = "http://localhost:8082/CommService";
        public static string repositoryDirectoryPath { get; set; } = "../../../Repository/RepositoryDirectory/";

        /// <summary>
        /// Print the message as a title
        /// </summary>
        /// <param name="aString"></param>
        /// <param name="underline"></param>
        public static void title(this string aString, char underline = '-')
        {
            Console.Write("\n  {0}", aString);
            Console.Write("\n {0}", new string(underline, aString.Length + 2));
        }

        /// <summary>
        /// getting ports and addresses from commandline
        /// </summary>
        /// <param name="args"></param>
        /// <param name="localUrl"></param>
        /// <returns></returns>
        static public string processCommandLineForLocal(string[] args, string localUrl)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                if ((args.Length > i + 1) && (args[i] == "/l" || args[i] == "/L"))
                {
                    localUrl = args[i + 1];
                }
            }
            return localUrl;
        }

        /// <summary>
        /// Waits for the files to close if it is open for some time
        /// </summary>
        /// <param name="count"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        static public bool waitForFileToClose(int count, string path)
        {
            for (int i = 0; i < 3; i++)
            {
                if (!IsFileInUse(path))
                    return true;
                Thread.Sleep(500);
            }
            if (!IsFileInUse(path))
                return true;
            else
                return false;
        }

        /// <summary>
        /// It will Read log files and convert them from binray to Text 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        static public string ReadFromBinrayToText(string filePath)
        {
            try
            {
                string contents = null;
                using (var inputStream = new FileStream(filePath, FileMode.Open))
                {
                    byte[] bytes = new byte[inputStream.Length];
                    int numBytesToRead = (int)inputStream.Length;
                    int numBytesRead = 0;
                    while (numBytesToRead > 0)
                    {
                        // Read may return anything from 0 to numBytesToRead.
                        int n = inputStream.Read(bytes, numBytesRead, numBytesToRead);

                        // Break when the end of the file is reached.
                        if (n == 0)
                            break;

                        numBytesRead += n;
                        numBytesToRead -= n;
                    }
                    char[] chars = new char[bytes.Length / sizeof(char)];
                    System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
                    contents= new string(chars);
                }
                return contents;
            }
            catch (IOException)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Will return true if file is open or in use.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static public bool IsFileInUse(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("'path' cannot be null or empty.", "path");
            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read)) { }
            }
            catch (IOException ex)
            {
                Console.Write("\n Exception:{0}", ex.Message);
                return true;
            }
            return false;
        }

/// <summary>
/// Get the all the file names from the test request
/// </summary>
/// <param name="testRequest"></param>
/// <returns></returns>
        static public List<string> GetFilesFromTestRequest(TestRequest testRequest)
        {
           return  (testRequest.tests.SelectMany(n => n.testCodes)).Concat(testRequest.tests.Select(n => n.testDriver)).ToList(); 
        }

        /// <summary>
        /// Convert any type of object to XML and return it.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        static public string ToXml(this object obj)
        {
            XmlSerializerNamespaces nmsp = new XmlSerializerNamespaces();
            nmsp.Add("", "");

            var sb = new StringBuilder();
            try
            {
                var serializer = new XmlSerializer(obj.GetType());
                using (StringWriter writer = new StringWriter(sb))
                {
                    serializer.Serialize(writer, obj, nmsp);
                }
            }
            catch (Exception ex)
            {
                Console.Write("\n  exception thrown:");
                Console.Write("\n  {0}", ex.Message);
            }
            return sb.ToString();
        }

        /// <summary>
        /// deserialize XML to object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <returns></returns>
        static public T FromXml<T>(this string xml)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(new StringReader(xml));
            }
            catch (Exception ex)
            {
                Console.Write("\n  deserialization failed\n  {0}", ex.Message);
                return default(T);
            }
        }

        /// <summary>
        /// getting ports and addresses from commandline
        /// </summary>
        /// <param name="args"></param>
        /// <param name="remoteUrl"></param>
        /// <returns></returns>
        static public string processCommandLineForRemote(string[] args, string remoteUrl)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                if ((args.Length > i + 1) && (args[i] == "/r" || args[i] == "/R"))
                {
                    remoteUrl = args[i + 1];
                }
            }
            return remoteUrl;
        }

        /// <summary>
        /// helper functions to construct url strings
        /// </summary>
        public static string makeUrl(string address, string port)
        {
            return "http://" + address + ":" + port + "/CommService";
        }
        public static string urlPort(string url)
        {
            int posColon = url.LastIndexOf(':');
            int posSlash = url.LastIndexOf('/');
            string port = url.Substring(posColon + 1, posSlash - posColon - 1);
            return port;
        }
        public static string urlAddress(string url)
        {
            int posFirstColon = url.IndexOf(':');
            int posLastColon = url.LastIndexOf(':');
            string port = url.Substring(posFirstColon + 3, posLastColon - posFirstColon - 3);
            return port;
        }
        public static void swapUrls(ref Message msg)
        {
            string temp = msg.fromUrl;
            msg.fromUrl = msg.toUrl;
            msg.toUrl = temp;
        }

        public static bool verbose { get; set; } = false;

        /// <summary>
        /// wait for user till it gets some input
        /// </summary>
        public static void waitForUser()
        {
            Thread.Sleep(200);
            Console.Write("\n  press any key to quit: ");
            Console.ReadKey();
        }

        /// <summary>
        /// Print the message
        /// </summary>
        /// <param name="msg"></param>
        public static void showMessage(Message msg)
        {
            Console.Write("\n  msg.fromUrl: {0}", msg.fromUrl);
            Console.Write("\n  msg.toUrl:   {0}", msg.toUrl);
            Console.Write("\n  msg.body: {0}", msg.body);
        }

#if(TEST_UTILITIES)
        static void Main(string[] args)
        {
            "testing utilities".title('=');
            Console.WriteLine();

            "testing makeUrl".title();
            string localUrl = Utilities.makeUrl("localhost", "7070");
            string remoteUrl = Utilities.makeUrl("localhost", "7071");
            Console.Write("\n  localUrl  = {0}", localUrl);
            Console.Write("\n  remoteUrl = {0}", remoteUrl);
            Console.WriteLine();

            "testing url parsing".title();
            string port = urlPort(localUrl);
            string addr = urlAddress(localUrl);
            Console.Write("\n  local port = {0}", port);
            Console.Write("\n  local addr = {0}", addr);
            Console.WriteLine();

            "testing processCommandLine".title();
            localUrl = Utilities.processCommandLineForLocal(args, localUrl);
            remoteUrl = Utilities.processCommandLineForRemote(args, remoteUrl);
            Console.Write("\n  localUrl  = {0}", localUrl);
            Console.Write("\n  remoteUrl = {0}", remoteUrl);
            Console.WriteLine();

            "testing swapUrls(ref Message msg)".title();
            Message msg = new Message();
            msg.toUrl = "http://localhost:8080/CommService";
            msg.fromUrl = "http://localhost:8081/CommService";
            msg.body = "swapee";
            Utilities.showMessage(msg);
            Console.WriteLine();

            Utilities.swapUrls(ref msg);
            Utilities.showMessage(msg);

            "Serlization of msg".title();
            Message msgtest = new Message();
            msg.toUrl = "http://localhost:8080/CommService";
            msg.fromUrl = "http://localhost:8081/CommService";
            msg.body = "abc";
            string xml = msgtest.ToXml();
            Console.Write("\n Serlized message:{0}",xml);

            "Deserlization of msg".title();
            Message getbackmsg = xml.FromXml<Message>();
            Utilities.showMessage(msg);
            Console.WriteLine();

            "Wait for file if it is in use".title();
            Utilities.waitForFileToClose(3,Path.GetFullPath("../../../Utilities/" + "TEmp.txt"));
            Console.Write("\n File can be used now.");

            Console.Write("\n\n");
        }
#endif
    }
}

