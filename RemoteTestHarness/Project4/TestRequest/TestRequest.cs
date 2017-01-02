///////////////////////////////////////////////////////////////////////
// TestRequest.cs - It provides testrequest, testelement, FileRequest,//
// TestResults, TestResults classes.                                //
//                                                                 //
// Application: CSE681 - Software Modelling and Analysis,          //
//  Remote Test Harness Project-4                                  //
// Author:      Arpit Shah, Syracuse University,                   //
//              aushah@syr.edu, (646) 288-9410                     //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operation:
 * ================
 * It is has some imprtant class like  testrequest, testelement, FileRequest,
 * TestResults, TestResults classes. It has also some function to support 
 * processing of this class instance.
 * 
 * Build Process
 * =============
 * - Required Files: TestRequest.cs
 * - Compiler Command: csc TestRequest.cs
 * 
 * Maintainance History
 * ====================
 * ver 1.0 : 20 November 2016
 *     - first release 
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace Project4
{
    public class TestElement  /* information about a single test */
    {
        public string testName { get; set; }
        public string testDriver { get; set; }
        public List<string> testCodes { get; set; } = new List<string>();

        public TestElement() { }

        public TestElement(string name)
        {
            testName = name;
        }

        /// <summary>
        /// adds test driver
        /// </summary>
        /// <param name="name"></param>
        public void addDriver(string name)
        {
            testDriver = name;
        }

        /// <summary>
        /// adds test code
        /// </summary>
        /// <param name="name"></param>
        public void addCode(string name)
        {
            testCodes.Add(name);
        }

        /// <summary>
        /// Conver it to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string temp = "\n    test: " + testName;
            temp += "\n      testDriver: " + testDriver;
            foreach (string testCode in testCodes)
                temp += "\n      testCode:   " + testCode;
            return temp;
        }
    }

    public class FileRequest /*to communicate using a File transfer messages containing this data*/
    {
        public string fileName { get; set; }
        public string fileUploadPath { get; set; }
    }

    public class TestRequest  /* a container for one or more TestElements */
    {
        public int ID { get; set; }
        public string author { get; set; }
        public string authortype { get; set; }
        public DateTime timeStamp { set; get; }
        public List<TestElement> tests { get; set; } = new List<TestElement>();

        public TestRequest() { }
        public TestRequest(string auth)
        {
            author = auth;
        }

        /// <summary>
        /// Convert it to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string temp = "\n  author: " + author;
            temp = "\n  authortype: " + authortype;
            temp = "\n  timeStamp: " + timeStamp;
            foreach (TestElement te in tests)
                temp += te.ToString();
            return temp;
        }
    }

    public class TestResult  /* information about processing of a single test */
    {
        public string testName { get; set; }
        public bool passed { get; set; }
        public string log { get; set; }

        public TestResult() { }
        public TestResult(string name, bool status)
        {
            testName = name;
            passed = status;
        }

        /// <summary>
        /// add logItem into logs
        /// </summary>
        /// <param name="logItem"></param>
        public void addLog(string logItem)
        {
            log += logItem +"\n";
        }

        /// <summary>
        /// Conver it to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\n    Test: " + testName + " " + passed);
            sb.Append("\n    log:  " + log);
            return sb.ToString();
        }
    }

    public class TestResults  /* a container for one or more TestResult instances */
    {
        public string author { get; set; }
        public DateTime timeStamp { get; } = DateTime.Now;
        public List<TestResult> results { get; set; } = new List<TestResult>();
        public ulong executionTime { get; set; }

        public TestResults() { }
        public TestResults(string auth, DateTime ts)
        {
            author = auth;
            timeStamp = ts;
        }

        /// <summary>
        /// add test result into test results
        /// </summary>
        /// <param name="rslt"></param>
        /// <returns></returns>
        public TestResults add(TestResult rslt)
        {
            results.Add(rslt);
            return this;
        }

        /// <summary>
        /// convert it to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\n  Author: " + author + " " + timeStamp.ToString());
            sb.Append("\n  Execution Time(uSec): " + executionTime.ToString());
            foreach (TestResult rslt in results)
            {
                sb.Append(rslt.ToString());
            }
            return sb.ToString();
        }
    }
    class Program
    {
#if (TEST_TESTREQUEST)
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
                Console.Write("\nTest Request:{0}",req.ToString());
                Console.Write("\nTest Element:{0}", ele.ToString());
                TestResults results = new TestResults();
                results.author = "Arpit";
                results.executionTime = 23399;
                TestResult result = new TestResult();
                result.testName = "First Test";
                result.passed = true;
                result.log = "Logs Added";
                results.add(result);
                Console.Write("\nTest element result:{0}",result.ToString());
                Console.Write("\nTestResults:{0}", results.ToString());
                FileRequest fr = new FileRequest();
                fr.fileName = "Test.dll";
                fr.fileUploadPath = "../../xyz/abc";
                Console.Write("\nFile Request:{0}", fr.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}.", ex.Message);
            }
            Console.ReadLine();
        }
#endif
    }
}
