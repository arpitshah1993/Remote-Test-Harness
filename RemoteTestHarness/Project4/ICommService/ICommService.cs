/////////////////////////////////////////////////////////////////////
// ICommService.cs - It will act as communication channels between //
// the server                                                     //
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
 * and receriver of the the test hanrness server. It will communicate
 * with other server, process received messages and enqueue the message
 * in testRequest blocking queeu if it is test request.  
 * 
 * Public Interface 
 * ================
 * public Message fromString(string msgStr)                       //It will convert the string into message object
 * public override string ToString()                               //It convert message object to string and return it.
 *
 * Build Process
 * =============
 * - Required Files: ICommService.cs 
 * - Compiler Command: csc ICommService.cs 
 * 
 * Maintainance History
 * ====================
 * ver 1.0 : 20 November 2016
 *     - first release 
 */
using System;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Linq;

namespace Project4
{
    [ServiceContract(Namespace = "Project4")]
    public interface ICommService
    {
        [OperationContract(IsOneWay = true)]
        void sendMessage(Message msg);

        [OperationContract(IsOneWay = true)]
        void upLoadFile(Message msg,string receiverDirectoryPath);

        [OperationContract]
        byte[] downloadFile(Message msg);

        [OperationContract]
        string browseFiles();
    }


    [DataContract]
    public class Message
    {
        [DataMember]
        public string toUrl { get; set; }
        [DataMember]
        public string fromUrl { get; set; }
        [DataMember]
        public MessageType type { get; set; }
        [DataMember]
        public string author { get; set; } = "";
        [DataMember]
        public DateTime time { get; set; } = DateTime.Now;
        [DataMember]
        public string body { get; set; } = "";
        [DataMember]
        public string filename { get; set; }
        [DataMember]
        public byte[] transferStream { get; set; }

        public Message(string bodyStr = "")
        {
            body = bodyStr;
            type = 0;
        }

        public enum MessageType
        {
            Undefined=0,
            TestRequest = 1,
            TestResults,
            TestResultsQuery,
            TestResultsReply,
            LogsQuery,
            LogsReply,
            FilesReply,
            FilesRequest,
            File,
            Notify,
        }
        
        /// <summary>
        /// It will convert the string into message object
        /// </summary>
        /// <param name="msgStr"></param>
        /// <returns></returns>
        public Message fromString(string msgStr)
        {
            Message msg = new Message();
            try
            {
                string[] parts = msgStr.Split(',');
                for (int i = 0; i < parts.Count(); ++i)
                    parts[i] = parts[i].Trim();

                msg.toUrl = parts[0].Substring(4);
                msg.fromUrl = parts[1].Substring(6);
              //  msg.type = parts[2].Substring(6) as MessageType;
                msg.author = parts[3].Substring(8);
                msg.time = DateTime.Parse(parts[4].Substring(6));
                if (parts[5].Count() > 6)
                    msg.body = parts[5].Substring(6);
            }
            catch
            {
                Console.Write("\n  string parsing failed in Message.fromString(string)");
                return null;
            }
            return msg;
        }

        /// <summary>
        /// It convert message object to string and return it.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string temp = "to: " + toUrl;
            temp += ", from: " + fromUrl;
            temp += ", type: " + type;
            if (author != "")
                temp += ", author: " + author;
            temp += ", time: " + time;
            temp += ", body:\n" + body;
            return temp;
        }
    }
}
