/////////////////////////////////////////////////////////////////////
// Executive.cs - dequeuing the test request , creating            //
//child AppDomain and calling other execution processes.           //
//                                                                 //
// Application: CSE681 - Software Modelling and Analysis,          //
//  Remote Test Harness Project-4                                  //
// Author:      Arpit Shah, Syracuse University,                   //
//              aushah@syr.edu, (646) 288-9410                     //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operation:
 * ================
 * This is the main Test Executive module. It is dequeuing the test
 * request and creating thread(max 8 test request handling) for each of them. 
 * After that, Each thread will fetch the file name and get that file from Repository.
 * Then, it will create child appdomain and send that execute each test driver
 * by invoking ProcessInvoker class's testInvoker function and start 
 * process into child AppDomain.When compiler enters into
 * ProcessInvoker.testInvoker method it will be in child AppDoamin till
 * it completes test execution for that specific request.
 * 
 * Public Interface 
 * ================
 * Executive()                          // constructor
 * void SetupRequestProcessing()        //Dequeuing each test request and create child AppDomain for them.
 * string testInvoker(string request)   //It is used by each child Appdomain to ignite/start testing process in the that child Appdomain
 * 
 * Build Process
 * =============
 * - Required Files: Executive.cs BlockingQueue.cs Directory.cs ICommService.cs HRtimer.cs TestRequest.cs Utiltites.cs
 * - Compiler Command: csc Executive.cs BlockingQueue.cs Directory.cs ICommService.cs HRtimer.cs TestRequest.cs Utiltites.cs
 * 
 * Maintainance History
 * ====================
 * ver 1.0 : 20 November 2016
 *     - first release 
 */
using Project4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Policy;
using System.Threading;

namespace TestHarness
{
    public class Executive
    {
        //Created public queue which will be used by Test Haness package to enqueue test request coming from client
        public static SWTools.BlockingQueue<Message> queue { get; set; } = new SWTools.BlockingQueue<Message>();
        //created delegate to send 
        public delegate void sendMessage(Message msg);
        private static readonly Object _obj = new Object();
        private HRTimer.HiResTimer hrTestExecuitonTime = new HRTimer.HiResTimer();
        public sendMessage sendMessageDelagate = null;
        private string tempDirectoryPath { get; set; } = null;
        //Constructor
        public Executive()
        {
            Console.Write("\n Creating executive in AppDomain: {0} by ThreadId: {1}\n", AppDomain.CurrentDomain.FriendlyName, Thread.CurrentThread.ManagedThreadId);
            new FileDirectory();
        }

        /// <summary>
        /// Dequeuing each test request and create child AppDomain for them and send test result to client
        /// </summary>
        public void SetupRequestProcessing()
        {
            //Dequeuing the test request.
            Message testRequestMessage = queue.deQ();
            hrTestExecuitonTime.Start(); //start the timer
            AppDomain main = AppDomain.CurrentDomain;
            Console.Write("\n Starting in AppDomain: {0} by ThreadId: {1}\n", AppDomain.CurrentDomain.FriendlyName, Thread.CurrentThread.ManagedThreadId);
            Console.Write("\n Dequeued the test request in AppDomain: {0} by ThreadId: {1}-------Requirement#4\n", AppDomain.CurrentDomain.FriendlyName, Thread.CurrentThread.ManagedThreadId);
            Console.Write("\n HRTimer started to measure execution time in AppDomain: {0} by ThreadId: {1}\n", AppDomain.CurrentDomain.FriendlyName, Thread.CurrentThread.ManagedThreadId);
            if (testRequestMessage != null)
            {
                TestRequest testRequest = testRequestMessage.body.FromXml<TestRequest>();
                Console.Write("\n Parsed the test request in AppDomain: {0} by ThreadId: {1}\n", AppDomain.CurrentDomain.FriendlyName, Thread.CurrentThread.ManagedThreadId);
                //Path where temorary directory created
                tempDirectoryPath = CreateDirectories(testRequest);
                lock (_obj)
                {
                    GetRequiredFiles(testRequest, testRequestMessage.fromUrl);
                }
                bool testResultMsg = CreateChildAppDomain(testRequest, Path.GetFileName(tempDirectoryPath));
                testRequestMessage.body = (testResultMsg == true ? "Passed" : "Failed");
                //sending message to client
                Console.Write("\n Sending test result to resquester client in AppDomain: {0} by ThreadId: {1}---------Requirment#7\n", AppDomain.CurrentDomain.FriendlyName, Thread.CurrentThread.ManagedThreadId);
                sendMessageDelagate?.Invoke(testRequestMessage);

                if (Directory.Exists(tempDirectoryPath))
                {
                    Console.Write("\n Deleting temporary directory Name {0}: in AppDomain: {1} by ThreadId: {2}\n", Path.GetFileName(tempDirectoryPath), AppDomain.CurrentDomain.FriendlyName, Thread.CurrentThread.ManagedThreadId);
                    //Delete temporary directory which we created while executing test request.
                    Directory.Delete(tempDirectoryPath, true);
                }
            }
        }

        /// <summary>
        /// Copy(download) required files from Repository
        /// We dont have to care about file is not in Repository since client is 
        /// selecting library files by browsing repository only while sending the request.
        /// </summary>
        /// <param name="testRequest"></param>
        private void GetRequiredFiles(TestRequest testRequest, string clientUrl)
        {
            List<string> fileLists = Utilities.GetFilesFromTestRequest(testRequest);
            Message msg = new Message();
            msg.fromUrl = Utilities.testHarness;
            msg.toUrl = Utilities.repository;
            FileRequest rq = new FileRequest();
            rq.fileUploadPath = tempDirectoryPath;
            rq.fileName = string.Join(";", fileLists);
            msg.body = rq.ToXml();
            //we want to inform client if it is not able to download the file. so,
            msg.fromUrl = clientUrl;
            msg.type = Message.MessageType.FilesRequest;
            Console.Write("\n Sending message to Repository to download the required files from it in AppDomain: {0} by ThreadId: {1}\n", AppDomain.CurrentDomain.FriendlyName, Thread.CurrentThread.ManagedThreadId);
            sendMessageDelagate?.Invoke(msg);
        }

        /// <summary>
        /// Create Temporary Directory Path: ../../../ Repository / RepositoryDirectory to store required dlls to perform the test.
        /// </summary>
        /// <param name="testRequest"></param>
        /// <returns></returns>
        private string CreateDirectories(TestRequest testRequest)
        {
            string path = "../../../TestHarness/TestHarnessDirectory/";
            string temopraryFolderName = testRequest.author + testRequest.timeStamp.ToString("_dd-MM-yyyy_hh-mm-ss-ffff-tt");
            temopraryFolderName = temopraryFolderName.Replace(" ", "_").Replace("/", "-").Replace(":", "-");
            //path of temporary directory file
            path = Path.GetFullPath(path) + temopraryFolderName;
            //created temporary directory for new test request.
            Console.Write("\n Creating temporary Directory having Name:{0} in AppDomain: {1} by Thread Id:{1}\n", temopraryFolderName, AppDomain.CurrentDomain.FriendlyName, Thread.CurrentThread.ManagedThreadId);
            Directory.CreateDirectory(path);
            return path;
        }

        /// <summary>
        /// Creating child AppDomain and call invoker to start further process and return combined result of test request.
        /// </summary>
        private bool CreateChildAppDomain(TestRequest testrequest, string fileName)
        {
            AppDomainSetup domaininfo = new AppDomainSetup();
            // defines search path for assemblies which is File directory path in our case
            domaininfo.ApplicationBase
              = FileDirectory.absolutePath();

            Evidence adevidence = AppDomain.CurrentDomain.Evidence;

            Console.Write("\n Creating Child AppDomain in AppDomain: {0} by ThreadId: {1}\n", AppDomain.CurrentDomain.FriendlyName, Thread.CurrentThread.ManagedThreadId);
            AppDomain childAppDomain = AppDomain.CreateDomain("Child AppDomain");

            Console.Write("\n Invoking an PrecessInvoker to invoke process into child AppDomain in AppDomain: {0} by ThreadId: {1}\n", AppDomain.CurrentDomain.FriendlyName, Thread.CurrentThread.ManagedThreadId);
            //Invoking an precessInvoker to ignite process in child AppDomain  
            ProcessInvoker invoke = (ProcessInvoker)childAppDomain.CreateInstanceAndUnwrap(typeof(ProcessInvoker).Assembly.FullName, "TestHarness.ProcessInvoker");
            string testResults = invoke.testInvoker(testrequest.ToXml(), tempDirectoryPath, hrTestExecuitonTime);
            Console.Write("\n Unloading child AppDomain in AppDomain: {0} by ThreadId: {1}\n", AppDomain.CurrentDomain.FriendlyName, Thread.CurrentThread.ManagedThreadId);
            AppDomain.Unload(childAppDomain);
            //Sending test results and logs to Repository
            Console.Write("\n Sending test result logs to Repository in AppDomain: {0} by ThreadId: {1}\n--------------Requirment#7", AppDomain.CurrentDomain.FriendlyName, Thread.CurrentThread.ManagedThreadId);
            sendTestResult(testResults, fileName);
            //return back testrequest combined result to client
            if (testResults.Contains("False"))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Sned test reuslt and logs to Repsoitory
        /// </summary>
        /// <param name="str"></param>
        /// <param name="fileName"></param>
        private void sendTestResult(string str, string fileName)
        {
            Message msg = new Message();
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            msg.type = Message.MessageType.File;
            msg.filename = fileName + ".txt";
            msg.transferStream = bytes;
            sendMessageDelagate?.Invoke(msg);
        }
    }

    public class ProcessInvoker : MarshalByRefObject
    {
        /// <summary>
        /// It is used by each child Appdomain to ignite/start testing process in the that child Appdomain
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public string testInvoker(string testRequestXml, string tempDirectoryPath, HRTimer.HiResTimer hrTestExecuitonTime)
        {
            TestResults testResults = new TestResults();
            try
            {
                TestRequest testRequest = testRequestXml.FromXml<TestRequest>();
                Assembly assem = Assembly.LoadFrom(FileDirectory.absolutePath() + "LoadAndExecute.dll");
                Type typeLoadAndExecute = assem.GetType("TestHarness.LoadAndExecute");
                object obj = Activator.CreateInstance(typeLoadAndExecute);

                foreach (var testElement in testRequest.tests)
                {
                    Console.Write("\n Executing Test Element Name: {2} in AppDomain: {0} by ThreadId: {1}\n", AppDomain.CurrentDomain.FriendlyName, Thread.CurrentThread.ManagedThreadId, testElement.testName);
                    TestResult newTestResult = new TestResult();
                    newTestResult.testName = testElement.testName;
                    List<string> files = new List<string>();
                    files.AddRange(testElement.testCodes);
                    files.Add(testElement.testDriver);

                    // Get the method InitiateTesting to call.-
                    MethodInfo doTest = typeLoadAndExecute.GetMethod("DoTest");
                    object[] args = new object[3];
                    args[0] = files;
                    args[1] = newTestResult;
                    args[2] = tempDirectoryPath;
                    newTestResult = (TestResult)doTest.Invoke(obj, args);
                    testResults.add(newTestResult);
                }
                Console.Write("\n Test Request Execution Completed in AppDomain: {0} by ThreadId: {1}\n", AppDomain.CurrentDomain.FriendlyName, Thread.CurrentThread.ManagedThreadId);
                hrTestExecuitonTime.Stop();
                Console.Write("\n HRTimer stopped to measure execution time in AppDomain: {0} by ThreadId: {1}\n", AppDomain.CurrentDomain.FriendlyName, Thread.CurrentThread.ManagedThreadId);
                testResults.author = testRequest.author;
                testResults.executionTime = hrTestExecuitonTime.ElapsedMicroseconds;
            }
            catch (Exception ex)  // use more explicit catch conditions first
            {
                Console.Write("\n Exception caught in AppDomain: {1} Exception: {0}", ex.Message, AppDomain.CurrentDomain.FriendlyName);
            }
            return testResults.ToString();
        }

        public class Program
        {
#if (TEST_EXECUTIVE)
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
                    Executive e = new Executive();
                    Executive.queue.enQ(msg);
                    e.SetupRequestProcessing();
                    Console.ReadLine();
                }
                catch (Exception ex)
                {
                    Console.Write("\nException caught ex: {0}\n", ex.Message);
                }
            }
#endif
        }
    }
}

