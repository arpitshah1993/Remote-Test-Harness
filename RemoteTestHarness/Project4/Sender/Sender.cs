/////////////////////////////////////////////////////////////////////
// Sender.cs - It will used to send object from queue.             //
// Each server in this application uses one sender queue.         //
//                                                                 //
// Application: CSE681 - Software Modelling and Analysis,          //
//  Remote Test Harness Project-4                                  //
// Author:      Arpit Shah, Syracuse University,                   //
//              aushah@syr.edu, (646) 288-9410                     //
// Insturctor: Jim Fawcett                                         //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operation:
 * ================
 * This module supports all the server for communication channel. Each 
 * server has one sender channel who sends the messages going to
 * other server and enqueue it into this sender queue. That messages 
 * get deqeueud processed one by one by one separate thread.
 * 
 * Public Interface 
 * ================
 *   public Sender(string LocalUrl = "http://localhost:8081/CommServer") //constructor 
 *    public bool isConnected(string url) //returns is sender connected to the specified url
 *   public bool Connect(string remoteUrl) //Connect repeatedly tries to send messages to service
 * public virtual void sendMsgNotify(string msg)//overridable message annunciator
 *   public void setAction(Action sendAct) //set send action
 *    public void startSender() //send messages to remote Receivers
 *    public virtual SWTools.BlockingQueue<Message> defineSendProcessing() //Defines the processing which continously dequeues the message from sender queue
 *     public virtual void sendExceptionNotify(Exception ex, string msg = "") //overridable exception annunciator
 * public void shutdown() //send closeSender message to local sender and close it
 *  public void CreateAndSendMessage(Message msg, string reply = null) //Create and send message as a reply of some message or normal message
 *  public bool sendMessage(Message msg, string receiverDirectoryPath = null) //Send message to the receiver, if it is message then it will enqueue in the sender queue, it it is file then it will upload it.
 * 
 * Build Process
 * =============
 * - Required Files: BlockingQueue.cs ICommService.cs Utlities.cs TestRequest.cs
 * - Compiler Command: csc BlockingQueue.cs ICommService.cs Utlities.cs TestRequest.cs
 * 
 * Maintainance History
 * ====================
 * ver 1.0 : 20 November 2016
 *     - first release 
 */
using System;
using System.Collections.Generic;
using System.Threading;
using System.ServiceModel;

namespace Project4
{
    using System.IO;
    using Util = Utilities;

    public class Sender
    {
        private string localUrl { get; set; } = "http://localhost:8081/CommService";
        private string remoteUrl { get; set; } = "http://localhost:8080/CommService";
        private int MaxConnectAttempts { get; set; } = 10;

        ICommService proxy = null;
        SWTools.BlockingQueue<Message> sendQ = null;
        Dictionary<string, ICommService> proxyStore = new Dictionary<string, ICommService>();
        Action sendAction = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="LocalUrl"></param>
        public Sender(string LocalUrl = "http://localhost:8081/CommServer")
        {
            localUrl = LocalUrl;
            sendQ = defineSendProcessing();
            startSender();
        }

        /// <summary>
        /// An instance of the proxy is what the client uses to make
        /// calls on the server's remote service instance. 
        /// </summary>
        /// <param name="remoteUrl"></param>
        /// <returns></returns>
        ICommService CreateProxy(string remoteUrl)
        {
            BasicHttpBinding binding = new BasicHttpBinding();
            EndpointAddress address = new EndpointAddress(remoteUrl);
            ChannelFactory<ICommService> factory = new ChannelFactory<ICommService>(binding, address);
            return factory.CreateChannel();
        }
        //----< is sender connected to the specified url? >------------------

        /// <summary>
        /// returns is sender connected to the specified url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public bool isConnected(string url)
        {
            return proxyStore.ContainsKey(url);
        }

        /// <summary>
        /// Connect repeatedly tries to send messages to service
        /// </summary>
        /// <param name="remoteUrl"></param>
        /// <returns></returns>
        public bool Connect(string remoteUrl)
        {
            try
            {
                if (Util.verbose)
                    sendMsgNotify("attempting to connect");
                if (isConnected(remoteUrl))
                    return true;
                proxy = CreateProxy(remoteUrl);
                return connectionChecking(proxy, remoteUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown while connection: {0}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// check whether connection is established on not
        /// </summary>
        /// <param name="proxyCheck"></param>
        /// <param name="remoteUrl"></param>
        /// <returns></returns>
        private bool connectionChecking(ICommService proxyCheck, string remoteUrl)
        {
            int attemptNumber = 0;
            Message startMsg = new Message();
            startMsg.fromUrl = localUrl;
            startMsg.toUrl = remoteUrl;
            startMsg.body = "connection start message";
            while (attemptNumber < MaxConnectAttempts)
            {
                try
                {
                    proxyCheck.sendMessage(startMsg);    // will throw if server isn't listening yet
                    proxyStore[remoteUrl] = proxyCheck;  // remember this proxy
                    if (Util.verbose)
                        sendMsgNotify("connected");
                    return true;
                }
                catch
                {
                    ++attemptNumber;
                    sendAttemptNotify(attemptNumber);
                    Thread.Sleep(100);
                }
            }
            return false;
        }

        /// <summary>
        /// overridable message annunciator
        /// </summary>
        /// <param name="msg"></param>
        public virtual void sendMsgNotify(string msg)
        {
            Console.Write("\n  {0}\n", msg);
        }

        /// <summary>
        /// overridable attemptHandler
        /// </summary>
        /// <param name="attemptNumber"></param>
        public virtual void sendAttemptNotify(int attemptNumber)
        {
            Console.Write("\n  connection attempt #{0}", attemptNumber);
        }

        /// <summary>
        /// set send action
        /// </summary>
        /// <param name="sendAct"></param>
        public void setAction(Action sendAct)
        {
            sendAction = sendAct;
        }

        /// <summary>
        /// send messages to remote Receivers
        /// </summary>
        public void startSender()
        {
            sendAction.Invoke();
        }


        /// <summary>
        /// Create and send message as a reply of some message or normal message
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="reply"></param>
        public void CreateAndSendMessage(Message msg, string reply = null)
        {
            //Send log from test harness to Repo
            if (msg.type == Message.MessageType.File)
            {
                SendingLogFileToRepository(msg);
                return;
            }
            Message replyMsg = new Message();
            replyMsg.toUrl = msg.fromUrl;
            replyMsg.fromUrl = msg.toUrl;
            replyMsg.author = "TestHarness";

            if (msg.type == Message.MessageType.TestRequest)
            {
                ReplyingTestRquestMessage(msg, replyMsg);
                return;
            }

            if (msg.type == Message.MessageType.LogsQuery)
            {
                ReplyingLogQuery(msg, reply, replyMsg);
                return;
            }

            if (msg.type == Message.MessageType.TestResultsQuery)
            {
                ReplyingTestResultsQuery(reply, replyMsg);
                return;
            }

            if (msg.type == Message.MessageType.FilesRequest)
            {
                ProcessingFileRequest(msg);
                return;
            }
        }

        /// <summary>
        /// It will create send reply to send message to requsted client about logs
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="reply"></param>
        /// <param name="replyMsg"></param>
        private void ReplyingLogQuery(Message msg, string reply, Message replyMsg)
        {
            if (string.IsNullOrEmpty(reply))
            {
                Console.WriteLine("Requested file is not there.");
                return;
            }
            replyMsg.type = Message.MessageType.LogsReply;
            replyMsg.filename = reply;
            if (Connect(replyMsg.toUrl))
                sendMessage(replyMsg, msg.body.FromXml<FileRequest>().fileUploadPath);
        }

        /// <summary>
        /// Send a reply message to requested client about testresultsquery
        /// </summary>
        /// <param name="reply"></param>
        /// <param name="replyMsg"></param>
        private void ReplyingTestResultsQuery(string reply, Message replyMsg)
        {
            replyMsg.type = Message.MessageType.TestResultsReply;
            replyMsg.body = reply;
            if (Connect(replyMsg.toUrl))
                sendMessage(replyMsg);
        }

        /// <summary>
        /// It will send the testrequest test result to the requuested client.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="replyMsg"></param>
        private void ReplyingTestRquestMessage(Message msg, Message replyMsg)
        {
            replyMsg.type = Message.MessageType.TestResults;
            replyMsg.body = msg.body;
            replyMsg.author = msg.author;
            replyMsg.time = msg.time;
            if (Connect(replyMsg.toUrl))
                sendMessage(replyMsg);
        }

        /// <summary>
        /// It will help test harness to upload test result and log files to repository
        /// </summary>
        /// <param name="msg"></param>
        private void SendingLogFileToRepository(Message msg)
        {
            msg.fromUrl = Util.testHarness;
            msg.toUrl = Util.repository;
            Console.Write("\n  Sending message:by Thread ID:{0}", Thread.CurrentThread.ManagedThreadId);
            Console.Write("\n  Receiver is {0}", msg.toUrl);
            Console.Write("\n  Message Type is {0} for file:{1}\n", msg.type,Path.GetFileName(msg.filename));
            if (Connect(msg.toUrl))
                proxyStore[msg.toUrl].upLoadFile(msg, Util.repositoryDirectoryPath + "LogDirectory/");
        }

        /// <summary>
        /// It process file request messages
        /// </summary>
        /// <param name="msg"></param>
        private void ProcessingFileRequest(Message msg)
        {
            FileRequest request = msg.body.FromXml<FileRequest>();
            string[] fileNames = request.fileName.Split(';');
            msg.body = "";
            foreach (var file in fileNames)
            {
                msg.filename = file;
                if (Connect(msg.toUrl))
                    sendMessage(msg, request.fileUploadPath);
                Console.Write("  File: {0} Downloaded\n",Path.GetFileName(file));
            }
        }

        /// <summary>
        /// Send message to the receiver, if it is message then it will enqueue in
        /// the sender queue, it it is file then it will upload it.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="receiverDirectoryPath"></param>
        /// <returns></returns>
        public bool sendMessage(Message msg, string receiverDirectoryPath = null)
        {
            Console.Write("\n  Sending message: by Thread ID:{0}", Thread.CurrentThread.ManagedThreadId);
            Console.Write("\n  Receiver is {0}", msg.toUrl);
            if (!string.IsNullOrEmpty(msg.body) )
            {
                Console.Write("\n  Message body is {0}", msg.body);
            }
            if (msg.type != Message.MessageType.Undefined)
                Console.Write("\n  Message Type is {0}", msg.type);
            Console.WriteLine();
            if (!proxyStore.ContainsKey(msg.toUrl))
            {
                ICommService fileproxy = CreateProxy(msg.toUrl);
                if (!connectionChecking(fileproxy, msg.toUrl))
                    return false;
            }

            if (msg.type == Message.MessageType.LogsReply)
            {
                UploadsQueriedLogFileAsReply(msg, receiverDirectoryPath);
                return true;
            }

            if (msg.type == Message.MessageType.FilesRequest)
            {
                DownloadFileFromRepoToRequester(msg, receiverDirectoryPath);
                return true;
            }
            else if (msg.type == Message.MessageType.FilesReply)
            {
                UploadingDllFilesToRepository(msg, receiverDirectoryPath);
                return true;
            }
            else
            {
                sendQ.enQ(msg);
                return true;
            }
        }

        /// <summary>
        /// It will upload dll files to repository
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="receiverDirectoryPath"></param>
        private void UploadingDllFilesToRepository(Message msg, string receiverDirectoryPath)
        {
            if (Util.verbose)
                Console.Write("\n  sender sending message to service {0}", msg.toUrl);
            string fqname = msg.filename;
            msg.body = "";
            using (var inputStream = new FileStream(fqname, FileMode.Open))
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
                msg.transferStream = bytes;
                proxyStore[msg.toUrl].upLoadFile(msg, receiverDirectoryPath);
            }
        }

        /// <summary>
        /// It uploads queried log file to requester
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="receiverDirectoryPath"></param>
        private void UploadsQueriedLogFileAsReply(Message msg, string receiverDirectoryPath)
        {
            Console.Write("\n tranfering files using stream file transfer without enqueueing the messages--------------Requirement#6");
            if (Util.verbose)
                Console.Write("\n  sender sending message to service {0}", msg.toUrl);
            string fqname = Util.repositoryDirectoryPath + "LogDirectory/" + Path.GetFileName(msg.filename);
            msg.body = "";
            using (var inputStream = new FileStream(fqname, FileMode.Open))
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
                msg.transferStream = bytes;
                proxyStore[msg.toUrl].upLoadFile(msg, receiverDirectoryPath);
            }
        }

        /// <summary>
        /// It downloads files from repository to requester directory
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="receiverDirectoryPath"></param>
        private void DownloadFileFromRepoToRequester(Message msg, string receiverDirectoryPath)
        {
            string rfilename = receiverDirectoryPath + "/" + Path.GetFileName(msg.filename);
            msg.filename = Util.repositoryDirectoryPath + "DLLDirectory/" + Path.GetFileName(msg.filename);
            byte[] bytes = proxyStore[msg.toUrl].downloadFile(msg);
            if (bytes == null)
            {
                Message notify = new Message();
                notify.type = Message.MessageType.Notify;
                notify.fromUrl = msg.toUrl;
                notify.toUrl = msg.fromUrl;
                notify.body = "File" + Path.GetFileName(msg.filename) + " is not available on Repository.";
                if (Connect(notify.toUrl))
                    sendMessage(notify);
                Console.WriteLine("File" + Path.GetFileName(msg.filename) + " is not available on Repository.Informing Requester Client about this error.-------Requirment#3");
                return;
            }
            using (var outputStream = new FileStream(rfilename, FileMode.Create))
            {
                outputStream.Write(bytes, 0, bytes.Length);
            }
        }

        /// <summary>
        /// Defines the processing which continously dequeues the message from sender queue
        /// and call the services to send message to the receiver
        /// </summary>
        /// <returns></returns>
        public virtual SWTools.BlockingQueue<Message> defineSendProcessing()
        {
            SWTools.BlockingQueue<Message> sendQ = new SWTools.BlockingQueue<Message>();
            Action sendAction = () =>
            {
                ThreadStart sendThreadProc = () =>
          {
                  while (true)
                  {
                      try
                      {
                          Message smsg = sendQ.deQ();
                          if (smsg.body == "closeSender")
                          {
                              Console.Write("\n  send thread quitting\n\n");
                              break;
                          }
                          if (proxyStore.ContainsKey(smsg.toUrl))
                          {
                        // proxy already created so use it
                        if (Util.verbose)
                                  Console.Write("\n  sender sending message to service {0}", smsg.toUrl);
                              proxyStore[smsg.toUrl].sendMessage(smsg);
                          }
                          else
                          {
                        // create new proxy with Connect, save it, and use it
                        if (this.Connect(smsg.toUrl))  
                        {
                                  if (Util.verbose)
                                      Console.Write("\n  sender created proxy and sending message {0}", smsg.toUrl);
                                  proxyStore[smsg.toUrl] = this.proxy;  // save proxy
                            proxy.sendMessage(smsg);
                              }
                              else
                              {
                                  sendMsgNotify(String.Format("could not connect to {0}\n", smsg.toUrl));
                                  continue;
                              }
                          }
                      }
                      catch (Exception ex)
                      {
                          sendExceptionNotify(ex);
                          continue;
                      }
                  }
              };
                Thread t = new Thread(sendThreadProc);  // start the sendThread
          t.IsBackground = true;
                t.Start();
            };
            this.setAction(sendAction);
            return sendQ;
        }

        /// <summary>
        /// overridable exception annunciator
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="msg"></param>
        public virtual void sendExceptionNotify(Exception ex, string msg = "")
        {
            Console.Write("\n --- {0} ---\n", ex.Message);
        }

        /// <summary>
        /// sets urls from CommandLine if defined there
        /// </summary>
        /// <param name="args"></param>
        private void processCommandLine(string[] args)
        {
            if (args.Length > 0)
            {
                localUrl = Util.processCommandLineForLocal(args, localUrl);
                remoteUrl = Util.processCommandLineForRemote(args, remoteUrl);
            }
        }

        /// <summary>
        /// send closeSender message to local sender
        /// </summary>
        public void shutdown()
        {
            Message sdmsg = new Message();
            sdmsg.fromUrl = localUrl;
            sdmsg.toUrl = localUrl;
            sdmsg.body = "closeSender";
            Console.Write("\n  shutting down local sender");
            sendMessage(sdmsg);
        }

#if (TEST_SENDER)
        static void Main(string[] args)
        {
            Util.verbose = false;

            Console.Write("\n  starting CommService Sender");
            Console.Write("\n =============================\n");

            Console.Title = "CommService Sender";

            Sender sndr = new Sender("http://localhost:8081/CommService");

            sndr.processCommandLine(args);

            int numMsgs = 5;
            int counter = 0;
            Message msg = null;
            while (true)
            {
                msg = new Message();
                msg.fromUrl = sndr.localUrl;
                msg.toUrl = sndr.remoteUrl;
                msg.body = "Message #" + (++counter).ToString();
                Console.Write("\n  sending {0}", msg.body);
                sndr.sendMessage(msg);
                Thread.Sleep(30);
                if (counter >= numMsgs)
                    break;
            }

            msg = new Message();
            msg.fromUrl = sndr.localUrl;
            msg.toUrl = "http://localhost:9999/CommService";
            msg.body = "no listener for this message";
            Console.Write("\n  sending {0}", msg.body);
            sndr.sendMessage(msg);
            msg = new Message();
            msg.fromUrl = sndr.localUrl;
            msg.toUrl = sndr.remoteUrl;
            msg.body = "Message #" + (++counter).ToString();
            Console.Write("\n  sending {0}", msg.body);
            sndr.sendMessage(msg);
            msg = new Message();
            msg.fromUrl = sndr.localUrl;
            msg.toUrl = sndr.remoteUrl;
            msg.body = "closeSender";  // message for self and Receiver
            Console.Write("\n  sending {0}", msg.body);
            sndr.sendMessage(msg);
            sndr.shutdown();
        }
#endif
    }
}
