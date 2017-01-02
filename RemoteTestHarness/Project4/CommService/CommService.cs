/////////////////////////////////////////////////////////////////////
// CommService.cs - It is implmentation of ICommService which      //
// basically provide functioanlity of communication channel        //
//                                                                 //
// Application: CSE681 - Software Modelling and Analysis,          //
//  Remote Test Harness Project-4                                  //
// Author:      Arpit Shah, Syracuse University,                   //
//              aushah@syr.edu, (646) 288-9410                     //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operation:
 * ================
 * It contains implemenattaion of all the funciton of the ICommService.
 * Each proxy which is creaed to communicate with other server uses this
 * communication channel's functions.  
 * 
 * Public Interface 
 * ================
 *  public CommService()                     // Constructor
 * public void sendMessage(Message msg) //It will enqueue the message to the receiver's queue
 * public string browseFiles() //It is used to browse the repository and retunrs the name of all files in the string separated by comma(;).
 * public Message getMessage() //called by server, blocks caller while empty
 * public byte[] downloadFile(Message msg)//download requested file to the directory of requested server
 *  public void upLoadFile(Message msg, string receiverDirectoryPath) //upload requested file to the directory of receriver server
 *    
 *
 * Build Process
 * =============
 * - Required Files: BlockingQueue.cs ICommService.cs Utilities.cs
 * - Compiler Command: csc BlockingQueue.cs ICommService.cs Utilities.cs
 * 
 * Maintainance History
 * ====================
 * ver 1.0 : 20 November 2016
 *     - first release 
 */
using System;
using System.IO;
using System.ServiceModel;

namespace Project4
{
    using System.Threading;
    using Util = Utilities;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class CommService : ICommService
    {
        // static rcvrQueue is shared by all instances of this class
        private static SWTools.BlockingQueue<Message> rcvrQueue =new SWTools.BlockingQueue<Message>();
        private int BlockSize = 1024;
        private byte[] block;

        /// <summary>
        /// Constructor
        /// </summary>
        public CommService()
        {
            Console.Write("\n Creating WCF channel Listener for Communication by Thread Id: {0}------Requirment#11",Thread.CurrentThread.ManagedThreadId);
            block = new byte[BlockSize];
        }

        /// <summary>
        /// It will enqueue the message to the receiver's queue
        /// </summary>
        /// <param name="msg"></param>
        public void sendMessage(Message msg)
        {
            if (Util.verbose)
                Console.Write("\n  this is CommService.sendMessage");
            rcvrQueue.enQ(msg);
        }

        /// <summary>
        /// It is used to browse the repository
        /// and retunrs the name of all files in the string separated by comma(;).
        /// </summary>
        /// <returns></returns>
        public string browseFiles()
        {
            System.Collections.Generic.List<string> fileNames = new System.Collections.Generic.List<string>();
            try
            {
                string[] files = Directory.GetFiles(Path.GetFullPath(Util.repositoryDirectoryPath + "DLLDirectory"), "*.dll");
                foreach (string file in files)
                {
                    fileNames.Add(Path.GetFileName(file));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
            return string.Join(";", fileNames);
        }

        /// <summary>
        /// called by server, blocks caller while empty
        /// </summary>
        /// <returns></returns>
        public Message getMessage()
        {
            if (Util.verbose)
                Console.Write("\n  this is CommService.getMessage");
            return rcvrQueue.deQ();
        }

        /// <summary>
        /// download requested file to the directory of requested server
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public byte[] downloadFile(Message msg)
        {
            Console.Write("\n\n Got Message to send file:{0} from server: {1}", Path.GetFileName(msg.filename), msg.fromUrl);
            string fqname = Path.GetFullPath(msg.filename);
            byte[] bytes;
            if (!File.Exists(fqname))
                return null;
            using (var inputStream = new FileStream(fqname, FileMode.Open))
            {
                bytes = new byte[inputStream.Length];
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
            }
            Console.Write(
              "\n  Sent file \"{0}\" of {1} bytes by Thread Id: {2}.\n",
              msg.filename, bytes.Length, Thread.CurrentThread.ManagedThreadId
            );
            return bytes;
        }

        /// <summary>
        /// upload requested file to the directory of receriver server, If file is in use, 
        /// It will wait for sometime, and if it is in use after that time, it will print
        /// the message on the console about this.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="receiverDirectoryPath"></param>
        public void upLoadFile(Message msg, string receiverDirectoryPath)
        {
            if (msg.type==Message.MessageType.File)
            {
                Console.Write("\n Storing log files coming from Test Harness by Thread Id:{0}-------------Requirment#8", Thread.CurrentThread.ManagedThreadId);
            }
            string filename = msg.filename;
            string rfilename = Path.Combine(Path.GetFullPath(receiverDirectoryPath), Path.GetFileName(filename));
            if (!Directory.Exists(receiverDirectoryPath))
                Directory.CreateDirectory(receiverDirectoryPath);

            if (!(File.Exists(rfilename) && !Util.waitForFileToClose(4, rfilename)))
            {
                using (var outputStream = new FileStream(rfilename, FileMode.Create))
                {
                    outputStream.Write(msg.transferStream, 0, msg.transferStream.Length);
                }
                Console.Write(
                  "\n  Received file \"{0}\" of {1} bytes. from server:{2} by Thread ID: {3}\n",
                  rfilename, msg.transferStream.Length, msg.fromUrl, Thread.CurrentThread.ManagedThreadId
                );
            }
            else
            {
                Console.Write(
                  "\n  File is already exists and in use, we cant open it right now by Thread ID: {0}.",Thread.CurrentThread.ManagedThreadId
                );
            }
        }
    }
    class Program
    {
#if(TEST_COMMSERVICE)
        static void Main(string[] args)
        {
            try
            {
                TestRequest req = new TestRequest();
                req.author = "Arpit";
                req.authortype = "Devs";
                TestElement ele = new TestElement();
                ele.testDriver = "TestDriver1.dll";
                ele.testCodes.Add("TestCode1.dll");
                req.tests.Add(ele);
                Message msg = new Message();
                msg.author = "Arpit";
                msg.toUrl = "http://localhost:8081/CommService";
                msg.fromUrl = "http://localhost:8080/CommService";
                msg.body = req.ToXml();
                msg.filename = "../../../CommService/TestDriver1.dll";
                CommService com = new CommService();
                Console.Write("\n Sent Message:{0}", msg.ToString());
                com.sendMessage(msg);
                Message recMsg = com.getMessage();
                Console.Write("\n Received Message:{0}", recMsg.ToString());
                string files = com.browseFiles();
                Console.Write("\nBrowsed files from Repository:{0}", files);
                msg.transferStream=com.downloadFile(msg);
                Console.Write("\nGot byte from File Downloaded successfully!");
                com.upLoadFile(msg,Path.GetFullPath("../../../CommService/" + "temp/"));
                Console.Write("\nUploaded that byte as a File");

            }
            catch (Exception ex)
            {
                Console.Write("\n Exception caught:{0}", ex.Message);
            }
            Console.ReadLine();
        }
#endif
    }
}
