/////////////////////////////////////////////////////////////////////
// TestDriver1.cs - Executing a test on Testcode 1                 //
//                                                                 //
// Application: CSE681 - Software Modelling and Analysis,          //
//  Remote Test Harness Project-4                                   //
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
 * - Required Files: TestDriver1.cs, TestCode1.cs
 * - Compiler Command: csc TestDriver1.cs, TestCode1.cs
 *
 * Maintainance History
 * ====================
 * ver 1.0 : 20 november 2016
 *     - first release 
 */

using System;
using TestHarness;

namespace TestDemo
{
    public class TestDriver1 : ITest
    {
        private TestCode1 code;

        /// <summary>
        /// Testdriver constructor
        /// </summary>
        public TestDriver1()
        {
            code = new TestCode1();
        }

        /// <summary>
        ///  This can't be used by any code that doesn't know the name of this class.
        ///  We are using it just to give some sort of reference of this testdriver to ITest while executing test stub.
        /// </summary>
        /// <returns></returns>
        public static ITest create()
        {
            return new TestDriver1();
        }

        /// <summary>
        // test method is where all the testing gets done 
        /// </summary>
        /// <returns></returns>
        public bool test()
        {
            bool result = true;
            try
            {
                int expectedOutput = 5;
                if (code.add(3, 2) == expectedOutput)
                { result &= true; }
                else
                { result &= false; }

                expectedOutput = 1;
                if (code.sub(3, 2) == expectedOutput)
                { result &= true; }
                else
                { result &= false; }

                expectedOutput = 6;
                if (code.multi(3, 2) == expectedOutput)
                { result &= true; }
                else
                { result &= false; }
            }
            catch (Exception ex)
            {
                Console.Write("\nException caught in child domain: {0} \n", ex.Message);
                result &= false;
            }
            return result;
        }

#if (TEST_DRIVER1)
        // test stub
        static void Main(string[] args)
        {
            try
            {
                Console.Write("\nLocal test:\n");

                ITest test = create();

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
#endif
    }
}
