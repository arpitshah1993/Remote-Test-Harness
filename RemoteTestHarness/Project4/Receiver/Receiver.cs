/////////////////////////////////////////////////////////////////////
// Receiver.cs - It will used to receive object from queue.        //
// Each server in this application uses one receiver queue.        //
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
 * server has one receiver channel who receives the messages coming from
 * other server and enqueue it into this receiver queue. That messages 
 * get deqeueud processed one by one by one separate thread.
 * 
 * Public Interface 
 * ================
 *  public Receiver(string Port = "8080", string Address = "localhost") //constructor sets initial listening endpoint address
 *   public bool StartService() //Create CommService and listener and start it 
 *   public void doService(Action serviceAction) //run the service action
 *   public Action defaultServiceAction<T>() where T : class //serviceAction defines what happens to received messages
 * private Message getMessage() //application hosting Receiver calls this method to get the message
 *  public void shutDown() //send closeReceiver message to local Receiver
 * 
 * Build Process
 * =============
 * - Required Files: CommService.cs ICommService.cs Utlities.cs 
 * - Compiler Command: csc CommService.cs ICommService.cs Utlities.cs 
 * 
 * Maintainance History
 * ====================
 * ver 1.0 : 20 November 2016
 *     - first release 
 */
//
using System;
using System.ServiceModel;
using System.Threading;

namespace Project4
{
    using Util = Utilities;

    public class Receiver
    {
        private string address { get; set; }
        private string port { get; set; }

        CommService svc = null;
        ServiceHost host = null;

        /// <summary>
        /// constructor sets initial listening endpoint address
        /// </summary>
        /// <param name="Port"></param>
        /// <param name="Address"></param>
        public Receiver(string Port = "8080", string Address = "localhost")
        {
            address = Address;
            port = Port;
        }

        /// <summary>
        /// creates listener but does not start it
        /// </summary>
        /// <returns></returns>
        private ServiceHost CreateListener()
        {
            string url = "http://" + this.address + ":" + this.port + "/CommService";
            BasicHttpBinding binding = new BasicHttpBinding();
            Uri address = new Uri(url);
            Type service = typeof(CommService);
            ServiceHost host = new ServiceHost(service, address);
            host.AddServiceEndpoint(typeof(ICommService), binding, address);
            Console.WriteLine("Host is established for  {0}", this.port);
            return host;
        }

        /// <summary>
        /// Create CommService and listener and start it 
        /// </summary>
        /// <returns></returns>
        public bool StartService()
        {
            if (Util.verbose)
                Console.Write("\n  Receiver starting service");
            try
            {
                host = CreateListener();
                host.Open();
                svc = new CommService();
            }
            catch (Exception ex)
            {
                Console.Write("\n\n --- creation of Receiver listener failed ---\n");
                Console.Write("\n    {0}", ex.Message);
                Console.Write("\n    exiting\n\n");
                return false;
            }
            return true;
        }

        /// <summary>
        /// serviceAction defines what happens to received messages.
        /// Default service action is to display each received message.
        /// serverProcessMessage(msg) does nothing, but can be overridden
        /// to provide additional server processing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Action defaultServiceAction<T>() where T : class
        {
            Action serviceAction = () =>
            {
                if (Util.verbose)
                    Console.Write("\n  starting Receiver.defaultServiceAction");
                Message msg = null;
                while (true)
                {
                    msg = getMessage();   // note use of non-service method to deQ messages
                    Console.Write("\n  Received message:  by Thread ID:{0}", Thread.CurrentThread.ManagedThreadId);
                    Console.Write("\n  sender is {0}", msg.fromUrl);
                    if(!string.IsNullOrEmpty(msg.body))
                        Console.Write("\n  Message body is {0}", msg.body);
                    if (msg.type != Message.MessageType.Undefined)
                        Console.Write("\n  Message Type is {0}\n", msg.type);
                    Console.WriteLine();
                    typeof(T).GetMethod("ReceivedMessageProcessing").Invoke(null, new object[] { msg });
                    if (msg.body == "closeReceiver")
                        break;
                }
            };
            return serviceAction;
        }

        /// <summary>
        /// run the service action
        /// </summary>
        /// <param name="serviceAction"></param>
        public void doService(Action serviceAction)
        {
            ThreadStart ts = () =>
            {
                if (Util.verbose)
                    Console.Write("\n  doService thread started");
                serviceAction.Invoke();  // usually has while loop that runs until closed
            };
            Thread t = new Thread(ts);
            t.IsBackground = true;
            t.Start();
        }

        /// <summary>
        /// application hosting Receiver calls this method to
        /// get the message
        /// </summary>
        /// <returns></returns>
        private Message getMessage()
        {
            if (Util.verbose)
                Console.Write("\n  calling CommService.getMessage()");
            Message msg = svc.getMessage();
            if (Util.verbose)
                Console.Write("\n  returned from CommService.getMessage()");
            return msg;
        }

        /// <summary>
        /// send closeReceiver message to local Receiver
        /// </summary>
        public void shutDown()
        {
            Console.Write("\n  local receiver shutting down");
            Message msg = new Message();
            msg.body = "closeReceiver";
            msg.toUrl = Util.makeUrl(address, port);
            msg.fromUrl = msg.toUrl;
            Util.showMessage(msg);
            svc.sendMessage(msg);
            host.Close();
        }

        /// <summary>
        /// getting ports and addresses from commandline
        /// </summary>
        /// <param name="args"></param>
        private void ProcessCommandLine(string[] args)
        {
            if (args.Length > 0)
            {
                port = args[0];
            }
            if (args.Length > 1)
            {
                address = args[1];
            }
        }

#if (TEST_RECEIVER)

        static void Main(string[] args)
        {
            Util.verbose = true;

            Console.Title = "CommService Receiver";
            Console.Write("\n  Starting CommService Receiver");
            Console.Write("\n ===============================\n");

            Receiver rcvr = new Receiver();
            rcvr.ProcessCommandLine(args);

            Console.Write("\n  Receiver url = {0}\n", Util.makeUrl(rcvr.address, rcvr.port));

            // serviceAction defines what the server does with received messages

            if (rcvr.StartService())
            {
                //rcvr.doService();
                rcvr.doService(rcvr.defaultServiceAction<Receiver>());  // equivalent to rcvr.doService()
            }
            Console.Write("\n  press any key to exit: ");
            Console.ReadKey();
            Console.Write("\n\n");
        }
#endif
    }
}
