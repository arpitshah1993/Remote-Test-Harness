/////////////////////////////////////////////////////////////////////
// TestCode2.cs - Code to test                                      //
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
 *string stringAdder(string a, string b)    //adding two string
 * string stringUpper(string a)             //changing case of the string to UPPER case
 * getCharAtIndex(string a, int index)      //Getting char at particular index
 * 
 * Build Process
 * =============
 * - Required Files: TestCode2.cs
 * - Compiler Command: csc TestCode2.cs
 *
 * Maintainance History
 * ====================
 * ver 1.0 : 20 november 2016
 *     - first release 
 */

using System;

namespace TestDemo
{
    public class TestCode2
    {
        //adding two string
        public string stringAdder(string a, string b)
        {
            return a + b;
        }

        //changing case of the string to UPPER case
        public string stringUpper(string a)
        {
            string temp = a.ToUpper();
            return temp;
        }

        //Getting char at particular index
        public char getCharAtIndex(string a, int index)
        {
            if (a.Length >= index)
                return a[index + 1];
            else
            {
                System.Text.StringBuilder sb = null;
                sb.Append("won't work");
                return 'v';
            }
        }

#if (TEST_CODE2)
        static void Main(string[] args)
        {
            try
            {
                TestCode2 ctt = new TestCode2();
                Console.Write("\nstring Upper\n");
                string Temp = ctt.stringUpper("this is a test");
                Console.Write("\n{0}\n", Temp);
                Console.Write("\nstring Adder\n");
                Temp = ctt.stringAdder("this is", " a test");
                Console.Write("\n{0}\n", Temp);
                Console.Write("\nChar finder\n");
                char foundChar = ctt.getCharAtIndex("this is a test", 2);
                Console.Write("\n{0}\n", foundChar);
            }
            catch (Exception ex)
            {
                Console.Write("\nException caught ex: {0}\n", ex.Message);
            }
        }
#endif
    }

}
