/////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs - It is GUI for clients to ineract with      //
// application.                                                    //
//                                                                 //
// Application: CSE681 - Software Modelling and Analysis,          //
// Remote Test Harness Project-4                                   //
// Author:      Arpit Shah, Syracuse University,                   //
//              aushah@syr.edu, (646) 288-9410                     //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operation:
 * ================
 * This is main Window in which diiferent user control will appear 
 * according to clients input.
 * 
 * Public Interface 
 * ================
 *  public static void ReceivedMessageProcessing(Message msg)                          // it processes the received message from receiver queue
 * public string browseFiles()        //It browse dll files from Repository and give visibilty to client to select proper test library files to create test request.
 * public void sendMessage(Message msg)   //It receives message as parameter from all the different files of the client package and send it to Communication package for further process.
 *  
 * Build Process
 * =============
 * - Required Files: HRTimer.cs Sender.cs Receiver.cs Utlities.cs TestRequest.cs ICommService.cs MainWindow.xaml.cs
 * - Compiler Command: csc HRTimer.cs Sender.cs Receiver.cs Utlities.cs TestRequest.cs ICommService.cs MainWindow.xaml.cs
 * 
 * Maintainance History
 * ====================
 * ver 1.0 : 20 November 2016
 *     - first release 
 */
using Project4;
using System;
using System.Windows;
using System.Windows.Controls;
using System.ServiceModel;

namespace Client2GUI
{
    using System.IO;
    using System.Threading;
    using Util = Utilities;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DockPanel UIDck = null;
        string author;
        string authorType;
        string localUrl { get; set; } = "http://localhost:8079/CommService";
        string localDirectoryPath { get; set; } = "../../ClientDirectory/";
        public delegate void testCompleted();
        public static testCompleted testDoneState = null;
        Sender sndr;
        Receiver rcvr;
        static TestHarnessMainUI td;

        /// <summary>
        /// Process received message from receiver queue of Client according to their type
        /// </summary>
        /// <param name="msg"></param>
        public static void ReceivedMessageProcessing(Message msg)
        {
                if (msg.type == Message.MessageType.Notify)
                {
                    Console.Write("\nError: {0}---------------------Requirment#3\n", msg.body);
                    return;
                }
            if (msg.type == Message.MessageType.TestResults)
            {
                testDoneState?.Invoke();
                if (td != null)
                    td.chnageInTestResult(msg);
                return;
            }
            if (msg.type == Message.MessageType.TestResultsReply)
            {
                Console.Write("\n Got test result from Test Harness and invoke the delegate to send a request for logs by Thread Id:{0}---------Requiremeent#6", Thread.CurrentThread.ManagedThreadId);
                td.populatingSearchedFile(msg);
                return;
            }
        }

        /// <summary>
        /// It browse dll files from Repository and give visibilty to client
        /// to select proper test library files to create test request.
        /// </summary>
        /// <returns></returns>
        public string browseFiles()
        {
            Console.Write("\n Browsing Files from Repository");
            ICommService proxy = new ChannelFactory<ICommService>(new BasicHttpBinding(), new EndpointAddress(Util.repository)).CreateChannel();
            if (sndr.Connect(Util.repository))
            {
                return proxy.browseFiles();
            }
            else
            {
                Console.Write("\n Restart the application to restart the Repository Server", "Not Able to Connect!");
                return string.Empty;
            }
        }

        /// <summary>
        /// It receives message as parameter from all the different files of the client package and
        /// send it to Communication package for further process.
        /// </summary>
        /// <param name="msg"></param>
        public void sendMessage(Message msg)
        {
            msg.fromUrl = localUrl;
            if (msg.type == Message.MessageType.TestResultsQuery)
            {
                //Sending keywords to serch log files related to it.
                if (sndr.Connect(msg.toUrl))
                {
                    sndr.sendMessage(msg);
                }
                return;
            }
            if (msg.type == Message.MessageType.LogsQuery)
            {
                //Sending a message to get particular log files by its name
                QueryingLogFilesbySelectedName(msg);
                return;
            }
            if (msg.type == Message.MessageType.FilesReply)
            {
                //Sending files to Repository
                ProcessingFileReply(msg);
                return;
            }
            if (msg.type == Message.MessageType.TestRequest)
            {
                SendingTestRequest(msg);
            }
        }

        /// <summary>
        /// Send Test request to test harness
        /// </summary>
        /// <param name="msg"></param>
        private void SendingTestRequest(Message msg)
        {
            msg.toUrl = Util.testHarness;
            msg.author = author;
            if (sndr.Connect(msg.toUrl))
            {
                sndr.sendMessage(msg);
            }
            else
            {
                MessageBox.Show("Connection Failed. Test Request cant be sent.", "Error!");
            }
        }

        /// <summary>
        /// Sending a message to get particular log files by its name
        /// </summary>
        /// <param name="msg"></param>
        private void QueryingLogFilesbySelectedName(Message msg)
        {
            FileRequest file = new FileRequest();
            file.fileName = msg.body;
            file.fileUploadPath = localDirectoryPath;
            msg.toUrl = Util.repository;
            msg.body = file.ToXml();
            if (sndr.Connect(msg.toUrl))
            {
                sndr.sendMessage(msg);
            }
        }

        /// <summary>
        /// Sending files to Repository
        /// </summary>
        /// <param name="msg"></param>
        private void ProcessingFileReply(Message msg)
        {
            msg.toUrl = Util.repository;
            string[] filenames = msg.body.Split(';');
            msg.body = null;
            foreach (string file in filenames)
            {
                if (System.IO.File.Exists(file))
                {
                    msg.filename = file;
                    sndr.sendMessage(msg, System.IO.Path.GetFullPath(Util.repositoryDirectoryPath + "/DLLDirectory"));
                }
            }
        }

        /// <summary>
        /// Sending files to Repository for demo
        /// </summary>
        /// <param name="msg"></param>
        private void ProcessingFileReplyForDemo(Message msg)
        {
            msg.toUrl = Util.repository;
            string[] filenames = msg.body.Split(';');
            msg.body = null;
            foreach (string file in filenames)
            {
                if (System.IO.File.Exists(Path.GetFullPath(localDirectoryPath + file)))
                {
                    msg.filename = Path.GetFullPath(localDirectoryPath + file);
                    sndr.sendMessage(msg, System.IO.Path.GetFullPath(Util.repositoryDirectoryPath + "/DLLDirectory"));
                }
            }
        }

        /// <summary>
        /// It will tempDock sa UI dock for further use.
        /// </summary>
        /// <param name="tempDck"></param>
        void setDock(DockPanel tempDck)
        {
            this.UIDck = tempDck;
        }

        /// <summary>
        /// it is the function which is called at start for demo purpose
        /// </summary>
        void automaticDemo()
        {
            try
            {
                TestRequest req = new TestRequest();
                req.author = "Arpit";
                req.authortype = "Devs";
                Project4.TestElement ele = new Project4.TestElement();
                ele.testDriver = "TestDriver2.dll";
                ele.testCodes.Add("TestCode2.dll");
                req.tests.Add(ele);
                Message msg = new Message();
                msg.author = "Arpit";
                msg.type = Message.MessageType.TestRequest;
                msg.toUrl = Util.testHarness;
                msg.fromUrl = localUrl;
                req.timeStamp = msg.time;

                Console.Write("\n Uploading  test Files first to the test repository---------Requirement#6");
                Message filemsg = new Message();
                filemsg.body = "TestDriver2.dll";
                filemsg.type = Message.MessageType.FilesReply;
                ProcessingFileReplyForDemo(filemsg);
                Thread.Sleep(3000);

                msg.body = req.ToXml();
                HRTimer.HiResTimer hrdemo = new HRTimer.HiResTimer();
                hrdemo.Start();
                if (sndr.Connect(msg.toUrl))
                {
                    Console.Write("\n Sending test request to Test harness serevr---------Requiremeent#6");
                    sndr.sendMessage(msg);
                }
                ProcessAfterCompletionOfTestExecution(msg, hrdemo);
            }
            catch (Exception ex)
            {
                Console.Write("\n exception thrown:{0}", ex.Message);
            }
        }

        /// <summary>
        /// Process activated when test execution got completed
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="hrdemo"></param>
        private void ProcessAfterCompletionOfTestExecution(Message msg, HRTimer.HiResTimer hrdemo)
        {
            testDoneState = (() =>
            {
                Message getlog = new Message();
                getlog.type = Message.MessageType.LogsQuery;
                string fileName = msg.author + msg.time.ToString("_dd-MM-yyyy_hh-mm-ss-ffff-tt") + ".txt";
                getlog.body = fileName;
                getlog.fromUrl = localUrl;
                Console.Write("\n Querying repository for log files---------Requirement#6");
                QueryingLogFilesbySelectedName(getlog);

                int i = 0;
                string file = "../../ClientDirectory/" + fileName;
                while (!File.Exists(System.IO.Path.GetFullPath(file)))
                {
                    if (i == 5)
                    {
                        Console.Write("\n Not Able to get the file", "Error!");
                        return;
                    }
                    Thread.Sleep(2000);
                    i++;
                }

                string filePath = Path.GetFullPath(file);
                string contents = null;
                if (Utilities.waitForFileToClose(3, filePath))
                {
                    contents = Utilities.ReadFromBinrayToText(filePath);
                }
                else
                {
                    contents = "File using by someone else, you cant have access of the log right NOW!";
                }
                Console.Write("\n Get queried for log files---------Requirement#6\n");
                Console.Write("\n Result Logs for Test Request" + Path.GetFileName(file) + "\n" + contents + "---------Requirment#12\n");
                hrdemo.Stop();
                Console.Write("\n Communication Latency time(uSecond): {0} -------Requirment#12", hrdemo.ElapsedMicroseconds);
                Console.Write("\n******************Ends Automatic Demonstration******************");
                Console.Write("\n******************Started Actual Application,Start Using GUI please.******************");
            });
        }

        public MainWindow()
        {
            Console.Write("\n  starting CommService");
            Console.Write("\n =====================\n");
            Console.Title = "Client2GUI(8079)";
            sndr = new Sender(localUrl);
            string localPort = Util.urlPort(localUrl);
            string localAddr = Util.urlAddress(localUrl);
            rcvr = new Receiver(localPort, localAddr);
            if (rcvr.StartService())
            {
                rcvr.doService(rcvr.defaultServiceAction<MainWindow>());
            }
            Console.Write("\n  Client2GUI Listening");
            //calling automatic demo
            Console.Write("\n******************Started Automatic Demonstration******************");
            automaticDemo();
            InitializeComponent();
            UIDck = dckPanel;
            UIDck.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
            WelcomeLogin wlcm = new WelcomeLogin();
            wlcm.btnSignInClicked += welComePageSignInButtonClicked;
            wlcm.Height = dckPanel.Height;
            wlcm.Width = dckPanel.Width;
            UIDck.Children.Add(wlcm);
        }

        
        private void welComePageSignInButtonClicked(object sender, SignInInfoEventArgs e)
        {
            try
            {
                author = e.authorName;
                authorType = e.authorType;
                td= new TestHarnessMainUI(author,authorType);
                td.browse = browseFiles;
                td.sndMsg = sendMessage;
                UIDck.Children.Clear();
                UIDck.Children.Add(td);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"Error!");
            }
        }
    }
}
