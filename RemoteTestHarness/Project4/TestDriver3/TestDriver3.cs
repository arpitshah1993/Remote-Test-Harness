/////////////////////////////////////////////////////////////////////
// TestDriver3.cs - Executing a test on BlockingQueue.cs           //
//                                                                 //
// Application: CSE681 - Software Modelling and Analysis,          //
// Test Harness Project-2                                          //
// Author:      Arpit Shah, Syracuse University,                   //
//              aushah@syr.edu, (646) 288-9410                     //
// Source:      Jim Fawcett                                        //
/////////////////////////////////////////////////////////////////////
/* Module Operation:
 * ================
 * This is test driver which is used to test some predefined function
 * of test code.
 * 
 * Public Interface 
 * ================
 * ITest create()             //To give some sort of reference of this testdriver to ITest while executing test stub
 * bool test()                //function which has the code to execute the test on pre defined test code.
 *
 * Build Process
 * =============
 * - Required Files: TestDriver3.cs, BlockingQueue.cs
 * - Compiler Command: csc TestDriver3.cs, BlockingQueue.cs
 *
 * Maintainance History
 * ====================
 * ver 1.0 : 05 October 2016
 *     - first release 
 */
using System;
using TestHarness;

namespace TestDemo
{
    public class TestDriver3 : ITest
    {
        private BlockingQueue<string> codeQ;

        /// <summary>
        /// Testdriver constructor
        /// </summary>
        public TestDriver3()
        {
            codeQ = new BlockingQueue<string>();
        }

        /// <summary>
        ///  This can't be used by any code that doesn't know the name of this class.
        ///  We are using it just to give some sort of reference of this testdriver to ITest while executing test stub.
        /// </summary>
        /// <returns></returns>
        public static ITest create()
        {
            return new TestDriver3();
        }

        /// <summary>
        // test method is where all the testing gets done 
        /// </summary>
        /// <returns></returns>
        public bool test()
        {
            try
            {
                bool testresult = true;
                string expectedOutput = "Test";
                codeQ.enQ(expectedOutput);
                if (string.Equals(expectedOutput, codeQ.deQ()))
                {
                    testresult &= true;
                }
                else
                {
                    testresult &= false;
                }

                expectedOutput = null;
                if (string.Equals(expectedOutput, codeQ.deQ()))
                {
                    testresult &= true;
                }
                else
                {
                    testresult &= false;
                }
                return testresult;
            }
            catch (Exception ex)
            {
                Console.Write("\nException caught in child domain: {0}\n", ex.Message);
            }
            return false;

        }

        static void Main(string[] args)
        {
            try
            {
                Console.Write("\nLocal test:\n");

                ITest test = TestDriver3.create();

                if (test.test() == true)
                    Console.Write("Test Result: test passed");
                else
                    Console.Write("Test Result: test failed");
                Console.Write("\n\n");
            }
            catch (Exception ex)
            {
                Console.Write("\nException caught ex: {0}\n", ex.Message);
            }
        }
    }
}
