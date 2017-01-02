/////////////////////////////////////////////////////////////////////
// Repository.cs - It will work as file manager who keep all the   //
// and dlls and logs file and give it other server when requested  //
//                                                                 //
// Application: CSE681 - Software Modelling and Analysis,          //
// Remote Test Harness Project-4                                   //
// Author:      Arpit Shah, Syracuse University,                   //
//              aushah@syr.edu, (646) 288-9410                     //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operation:
 * ================
 * This file handles all the request about files this repository 
 * contains. It will reply with inforamtion about the dlls and logs files
 * it is requested.
 * 
 * Public Interface 
 * ================
 * public static void ReceivedMessageProcessing(Message msg)                          // it processes the received message from receiver queue
 *  
 * Build Process
 * =============
 * - Required Files: Sender.cs Receiver.cs Utlities.cs TestRequest.cs ICommService.cs Repository.cs
 * - Compiler Command: csc Sender.cs Receiver.cs Utlities.cs TestRequest.cs ICommService.cs Repository.cs
 * 
 * Maintainance History
 * ====================
 * ver 1.0 : 20 November 2016
 *     - first release 
 */
using System;
using System.Linq;
using System.Text;
using System.IO;

namespace Project4
{
    using Util = Utilities;
    class Repository
    {
        static private string repositorypath { get; } = "../../../Repository/RepositoryDirectory";
        string localUrl { get; set; } = "http://localhost:8082/CommService";
        static private Receiver rcvr=null;
        static private Sender sndr = null;

        /// <summary>
        /// it processes the received message from receiver queue
        /// </summary>
        /// <param name="msg"></param>
        public static void ReceivedMessageProcessing(Message msg)
        {
            if (msg.type == Message.MessageType.TestResultsQuery)
            {
                ProcessingTestResultsQuery(msg);
                return;
            }
            if (msg.type == Message.MessageType.LogsQuery)
            {
                Console.Write("\n Handling logs queries coming from client and sending LogsReply as needed by Thread Id:{0}------Requirment#9", System.Threading.Thread.CurrentThread.ManagedThreadId);
                ProcessingLogsQuery(msg);
                return;
            }
        }

        /// <summary>
        /// it will process the logs query and send the file it needs
        /// to the requested server.
        /// </summary>
        /// <param name="msg"></param>
        private static void ProcessingLogsQuery(Message msg)
        {
            Console.Write("\n Sending resquested log file to requester.");
            string logFileName = msg.body.FromXml<FileRequest>().fileName;
            string path = System.IO.Path.GetFullPath(repositorypath + "/LogDirectory");
            string[] file = System.IO.Directory.GetFiles(path, logFileName);
            if (!file.Any())
            {
                sndr.CreateAndSendMessage(msg, null);
            }
            else
            {
                sndr.CreateAndSendMessage(msg, file.First());
            }
            
        }

        /// <summary>
        /// It will process the testresult query, search all the files names
        /// who contains that data and send it requested server
        /// </summary>
        /// <param name="msg"></param>
        private static void ProcessingTestResultsQuery(Message msg)
        {
            Console.Write("\n Sending List of Files have keywords which is searched by client.");
            string queryText = msg.body;
            StringBuilder queryResults = new StringBuilder();
            string path = System.IO.Path.GetFullPath(repositorypath + "/LogDirectory");
            string[] files = System.IO.Directory.GetFiles(path, "*.txt");
            foreach (string file in files)
            {
                string contents = Util.ReadFromBinrayToText(file);
                if (contents.Contains(queryText) || Path.GetFileName(file).Contains(queryText))
                {
                    string name = Path.GetFileName(file);
                    queryResults.Append(name + ";");
                }
            }
            sndr.CreateAndSendMessage(msg, queryResults.ToString());
        }


        static void Main(string[] args)
        {
            Console.Write("\n  starting CommService");
            Console.Write("\n =====================\n");
            Console.Title = "Repository(8082)";
            Repository clnt = new Repository();

            string localPort = Util.urlPort(clnt.localUrl);
            string localAddr = Util.urlAddress(clnt.localUrl);
            rcvr = new Receiver(localPort, localAddr);
            if (rcvr.StartService())
            {
                rcvr.doService(rcvr.defaultServiceAction<Repository>());
            }
            Console.Write("\n  Repository Listening");
            sndr = new Sender(clnt.localUrl);  // Sender needs localUrl for start message

            Util.waitForUser();
            sndr.shutdown();
            rcvr.shutDown();

            Console.Write("\n\n");
       }

    }
}
    

