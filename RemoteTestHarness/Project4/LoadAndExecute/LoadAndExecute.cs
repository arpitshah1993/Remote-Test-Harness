/////////////////////////////////////////////////////////////////////
// LoadAndExecute.cs - parsing data from test request,loading the  //
// test code and test drivers, executing them                       //
//                                                                 //
// Application: CSE681 - Software Modelling and Analysis,          //
//  Remote Test Harness Project-4                                  //
// Author:      Arpit Shah, Syracuse University,                   //
//              aushah@syr.edu, (646) 288-9410                     //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operation:
 * ================
 * This module is providing functonality to load all the dlls files
 * from authorName_DatetimeStamp directory.Then, it will find the test 
 * driver from that loaded files and run that test drivers on repspected
 * test codes. It also adds an imopratant logs about the
 * processes in between the test execution.
 * 
 * Public Interface 
 * ================
 * LoadAndExecute()            //Constructor
 * public TestResult DoTest(List<string> files, TestResult result,string tempDirectoryPath) //Start testing by loading required test dlls files and running them.
 *
 * Build Process
 * =============
 * - Required Files: Directory.cs Client.cs ITest.cs 
 * - Compiler Command: csc Directory.cs Client.cs ITest.cs 
 * 
 * Maintainance History
 * ====================
 * ver 1.0 : 20 November 2016
 *     - first release 
 */
using Project4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;

namespace TestHarness
{
    public class LoadAndExecute : MarshalByRefObject
    {
        // structure to collect data of test driver which we gave to execute
        private struct TestData
        {
            public string Name;
            public ITest testDriver;
        }

        public LoadAndExecute()
        {
            Console.Write("\n Creating LoadAndExecute in AppDomain: {0} by Thread Id:{1}\n", AppDomain.CurrentDomain.FriendlyName, Thread.CurrentThread.ManagedThreadId);
        }

        private List<TestData> testDriver = new List<TestData>();

        /// <summary>
        /// Start testing by loading required test Request files and running them.
        /// </summary>
        /// <param name="files"></param>
        /// <param name="result"></param>
        /// <param name="tempDirectoryPath"></param>
        /// <returns></returns>
        public TestResult DoTest(List<string> files, TestResult result, string tempDirectoryPath)
        {
            loadfiles(files, result, tempDirectoryPath);
            runTest(result);
            return result;
        }

        /// <summary>
        /// Execute the test driver on recommended test codes and return the result.
        /// </summary>
        /// <param name="test"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private bool execute(ITest test, TestResult result)
        {
            if (test == null)
            {
                Console.Write("\nTest driver reference is null\n");
                return false;
            }
            try
            {
                Console.Write("\n Calling test() in Test Driver method which returns bool value as Test's Result by Thread Id:{0}---------------Requirement #5", Thread.CurrentThread.ManagedThreadId);
                return test.test();
            }
            catch (ThreadAbortException ex)  // use more explicit catch conditions first
            {
                result.addLog(string.Format("Exception: {0}", ex.Message));
                Console.Write("\n Caught ThreadAbortException in AppDomain: {1} Message: {0} by Thread Id: {2}", ex.Message, AppDomain.CurrentDomain.FriendlyName, Thread.CurrentThread.ManagedThreadId);
                Thread.ResetAbort();  // if you don't reset abort will be rethrown at end of catch clause
            }
            catch (Exception ex)
            {
                result.addLog(string.Format("Exception: {0}", ex.Message));
                Console.Write("\n Exception caught in AppDomain: {1} Message: {0} by Thread Id: {2}", ex.Message, AppDomain.CurrentDomain.FriendlyName, Thread.CurrentThread.ManagedThreadId);
            }
            return false;
        }

        /// <summary>
        /// runs the test and popuate the test result
        /// </summary>
        /// <param name="result"></param>
        private void runTest(TestResult result)
        {
            if (testDriver.Count == 0)
                return;
            foreach (TestData td in testDriver)
            {
                //strating watch to count elapsed time of execution
                Stopwatch watch = new Stopwatch();
                try
                {
                    Console.Write("\n Running test  driver {0} in AppDomain: {1} by Thread Id:{2}", td.Name, AppDomain.CurrentDomain.FriendlyName, Thread.CurrentThread.ManagedThreadId);
                    watch.Reset();
                    //starting watch to measure elapsed time
                    watch.Start();
                    if (execute(td.testDriver, result) == true)
                    {
                        watch.Stop();
                        result.passed = true;
                        result.addLog("Test Result: Passed");
                        Console.Write("\n Test Result: Passed in AppDomain: {0} by Thread Id: {1}\n", AppDomain.CurrentDomain.FriendlyName, Thread.CurrentThread.ManagedThreadId);
                    }
                    else
                    {
                        result.passed = false;
                        result.addLog("Test Result: Failed");
                        Console.Write("\n Test Result: Failed in AppDomain: {0} by Thread Id: {1}\n", AppDomain.CurrentDomain.FriendlyName, Thread.CurrentThread.ManagedThreadId);
                    }
                }
                catch (Exception ex)
                {
                    result.passed = false;
                    result.addLog(string.Format("Exception: {0}", ex.Message));
                    Console.Write("\n Exception caught: {0} in AppDomain: {1} by Thread Id: {2}", ex.Message, AppDomain.CurrentDomain.FriendlyName, Thread.CurrentThread.ManagedThreadId);
                }
                finally
                {
                    watch.Stop();
                    result.addLog(string.Format("Elapsed Time to execute per tests(milliSeconds):{0}", watch.Elapsed.TotalMilliseconds));
                }
            }
            testDriver.Clear();
        }

        /// <summary>
        /// loading files to run each test.
        /// </summary>
        /// <param name="files"></param>
        /// <param name="result"></param>
        /// <param name="tempDirectoryPath"></param>
        /// <returns></returns>
        private bool loadfiles(List<string> files, TestResult result, string tempDirectoryPath)
        {
            Console.Write("\n Loading files from temoprary directory in AppDomain: {0} by Thread Id: {1}\n", AppDomain.CurrentDomain.FriendlyName, Thread.CurrentThread.ManagedThreadId);
            foreach (string file in files)
            {
                string tempPath = Path.Combine(tempDirectoryPath, file);
                Console.Write("\n Loading: \"{0}\" in AppDomain: {1} by Thread Id: {2}", tempPath, AppDomain.CurrentDomain.FriendlyName, Thread.CurrentThread.ManagedThreadId);
                result.addLog(string.Format("Loading: \"{0}\" in AppDomain: {1}", tempPath, AppDomain.CurrentDomain.FriendlyName));
                try
                {
                    Assembly assem = Assembly.LoadFrom(tempPath);
                    Type[] types = assem.GetExportedTypes();

                    foreach (Type t in types)
                    {
                        if (t.IsClass && typeof(ITest).IsAssignableFrom(t))  // searching whether this type derive from ITest 
                        {
                            Console.Write("\n Searched test driver derives from ITest Interface: typeof(ITest).IsAssignableFrom(typeofTestDriver): {0}, by Thread Id: {1}------------Requirement #5", typeof(ITest).IsAssignableFrom(t),Thread.CurrentThread.ManagedThreadId);
                            ITest tdr = (ITest)Activator.CreateInstance(t);    // create instance of test driver

                            // save type name and reference to created type on managed heap
                            TestData td = new TestData();
                            td.Name = t.Name;
                            td.testDriver = tdr;
                            testDriver.Add(td);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Write("\n Exception: {0} caught in AppDomain: {1} by Thread Id: {2}", ex.Message, AppDomain.CurrentDomain.FriendlyName, Thread.CurrentThread.ManagedThreadId);
                    result.addLog(string.Format("Exception: {0} caught in AppDomain: {1}", ex.Message, AppDomain.CurrentDomain.FriendlyName));
                }
            }
            Console.Write("\n");
            return testDriver.Count > 0;
        }
    }

    class Program
    {
#if (TEST_LOADANDEXECUTE)
        static void Main(string[] args)
        {
            try
            {
                string direcPath = Path.GetFullPath("../LoadAndExecute/" + "temp/");
                List<string> files = Directory.GetFiles(Path.GetFullPath(direcPath)).ToList<string>();
                TestResult tr = new TestResult();
                LoadAndExecute ld = new LoadAndExecute();
                tr = ld.DoTest(files, tr, direcPath);
                Console.Write("\n{0}", tr.ToString());
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
