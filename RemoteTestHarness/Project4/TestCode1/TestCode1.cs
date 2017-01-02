/////////////////////////////////////////////////////////////////////
// TestCode1.cs - Code to test                                      //
//                                                                 //
// Application: CSE681 - Software Modelling and Analysis,          //
//  Remote Test Harness Project-4                                   //
// Author:      Arpit Shah, Syracuse University,                   //
//              aushah@syr.edu, (646) 288-9410                     //
/////////////////////////////////////////////////////////////////////
/* Module Operation:
 * ================
 * This is test code which is tested by some test driver
 * 
 * Public Interface 
 * ================
 * int add(int a, int b)//add two int
 * int sub(int a, int b)//substitute two int
 *int multi(int a, int b)//multiply two int
 * 
 * Build Process
 * =============
 * - Required Files: TestCode1.cs
 * - Compiler Command: csc TestCode1.cs
 *
 * Maintainance History
 * ====================
 * ver 1.0 : 20 november 2016
 *     - first release 
 */

using System;

namespace TestDemo
{
    public class TestCode1
    {
        //add two int
        public int add(int a, int b)
        {
            return a + b;
        }
        //substitute two int
        public int sub(int a, int b)
        {
            return a - b;
        }
        //multiply two int
        public int multi(int a, int b)
        {
            return a * b;
        }
#if (TEST_CODE1)
        static void Main(string[] args)
        {
            try
            {
                TestCode1 ctt = new TestCode1();
                int ans = ctt.add(3, 2);
                Console.Write("\n" + ans + "\n");
                ans = ctt.sub(3, 2);
                Console.Write("\n" + ans + "\n");
                ans = ctt.multi(3, 2);
                Console.Write("\n" + ans + "\n");
            }
            catch (Exception ex)
            {
                Console.Write("\nException caught ex: {0}\n", ex.Message);
            }
        }
#endif
    }
}
