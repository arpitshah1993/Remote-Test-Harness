/////////////////////////////////////////////////////////////////////
// TestHarness.cs - It will act as main test harness server and    //
// handle server communication channels                            //
//                                                                 //
// Application: CSE681 - Software Modelling and Analysis,          //
//  Remote Test Harness Project-4                                  //
// Author:      Arpit Shah, Syracuse University,                   //
//              aushah@syr.edu, (646) 288-9410                     //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operation:
 * ================
 * It will act as main test harness server aand handles the sender
 * and receiver of the the test hanrness server. It will communicate
 * with other server, process received messages and enqueue the message
 * in testRequest blocking queue if it is test request.  
 * 
 * Public Interface 
 * ================
 * public static void ReceivedMessageProcessing(Message msg)                          // it processes the received message from receiver queue
 *
 * Build Process
 * =============
 * - Required Files: BlockingQueue.cs Execuitve.cs ICommService.cs Receiver.cs Sender.cs Utilities.cs
 * - Compiler Command: csc BlockingQueue.cs Execuitve.cs ICommService.cs Receiver.cs Sender.cs Utilities.cs
 * 
 * Maintainance History
 * ====================
 * ver 1.0 : 20 November 2016
 *     - first release 
 */
using System;
using System.Threading;
using TestHarness;

namespace Project4
{
    using Util = Utilities;
    class TestHarness
    {
        string localUrl { get; set; } = "http://localhost:8081/CommService";
        static readonly object _object = new object();

        /// <summary>
        /// Defines the process of the received messages
        /// </summary>
        /// <param name="msg"></param>
        public static void ReceivedMessageProcessing(Message msg)
        {
            if (msg.type==Message.MessageType.TestRequest)
            {
                Console.Write("\n Accepting Test request and enqueuing it into main Test harness Test request queue by Thraed Id: {0}-------Requirement#2,4", Thread.CurrentThread.ManagedThreadId);
                Executive.queue.enQ(msg);
            }
            if (msg.type == Message.MessageType.FilesReply)
            {
                Console.WriteLine(msg.body + " from clients receriver as callback");
            }
        }

        static void Main(string[] args)
        {
            Console.Write("\n  starting CommService ");
            Console.Write("\n =======================\n");
            Console.Title = "Test Harness(8081)";
            TestHarness clnt = new TestHarness();

            string localPort = Util.urlPort(clnt.localUrl);
            string localAddr = Util.urlAddress(clnt.localUrl);
            Receiver rcvr = new Receiver(localPort, localAddr);
            if (rcvr.StartService())
            {
                rcvr.doService(rcvr.defaultServiceAction<TestHarness>());
            }

            Console.Write("\n  TestHarness Listening");
            Sender sndr = new Sender(clnt.localUrl);

            ThreadStart testRequestExecution = (() =>
            {
                while (true)
                {
                    Executive executive = new Executive();
                    executive.sendMessageDelagate = ((msg) =>
                    {
                        lock (_object)
                        {
                            sndr.CreateAndSendMessage(msg);
                        }
                    });
                    executive.SetupRequestProcessing();
                }
            });

            for (int i = 0; i < 4; i++)
            {
                Thread t=new Thread(testRequestExecution);
                t.Start();
            }

            Util.waitForUser();
            sndr.shutdown();
            rcvr.shutDown();
            Console.Write("\n\n");
        }

    }
}
    

