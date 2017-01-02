/////////////////////////////////////////////////////////////////////
// Directory.cs - Hold a path of TestHarness Directory where all the// 
// dll files of the packages are being kept.                        //
//                                                                 //
// Application: CSE681 - Software Modelling and Analysis,          //
// Test Harness Project-2                                          //
// Author:      Arpit Shah, Syracuse University,                   //
//              aushah@syr.edu, (646) 288-9410                     //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operation:
 * ================
 * This module is providing absolute path of the main File Directory
 * where all dlls files for the packages are kept. 
 * 
 * Public Interface 
 * ================
 * FileDirectory fileDir = new FileDirectory();         // calls constructor
 * public static string absolutePath()                  // return aboslute path
 * 
 * Build Process
 * =============
 * - Required Files: Directory.cs 
 * - Compiler Command: csc Directory.cs
 * 
 * Maintainance History
 * ====================
 * ver 1.0 : 20 November 2016
 *     - first release
 */

using System;
using System.IO;

namespace TestHarness
{
    public class FileDirectory
    {
        //path of the file directory which contains all the dlls of the Test harness packages.
        private string _path = "../../../FileDirectory/";
        private static string _absolutePath = null;
        
        /// <summary>
        /// Retruns absolute Path for of the File Directory
        /// </summary>
        /// <returns></returns>
        public static string absolutePath()
        {
                if (_absolutePath == null)
                {
                    new FileDirectory();
                }
                return _absolutePath;
        }

        //setting absolute path when this class in initialized
        public FileDirectory()
        {
            _absolutePath = Path.GetFullPath(_path);
        }
    }

    class Program
    {

#if (TEST_DIRECTORY)
        static void Main(string[] args)
        {
            FileDirectory fileDir = new FileDirectory();
           
            try
            {
                Console.WriteLine(FileDirectory.absolutePath());
            }
            catch (Exception ex)
            {
                Console.WriteLine("File is not copied because of exception: {0}.", ex.Message);
            }

        }
#endif
    }
}
