/////////////////////////////////////////////////////////////////////
// TestElement.xaml.cs - It is User controlGUI for clients to      //
//ineract with application.                                        //
//                                                                 //
// Application: CSE681 - Software Modelling and Analysis,          //
// Remote Test Harness Project-4                                   //
// Author:      Arpit Shah, Syracuse University,                   //
//              aushah@syr.edu, (646) 288-9410                     //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operation:
 * ================
 * It provides text box and list box to select test driver and test codes
 * for client
 * 
 * Public Interface 
 * ================
 *  public Project4.TestElement wrapUpData()        //Wrap up all the data about test elemnt and send it to main window
 *  public void populate(string[] files)       //Populate the browsed test codes and test drivers from the Repository
 *   public void disableAllField()              //Disabling the field whose data is captured
 * Build Process
 * =============
 * - Required Files: HRTimer.cs Sender.cs Receiver.cs Utlities.cs TestRequest.cs ICommService.cs TestElement.xaml.cs
 * - Compiler Command: csc HRTimer.cs Sender.cs Receiver.cs Utlities.cs TestRequest.cs ICommService.cs TestElement.xaml.cs
 * 
 * Maintainance History
 * ====================
 * ver 1.0 : 20 November 2016
 *     - first release 
 */
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Client2GUI
{
    /// <summary>
    /// Interaction logic for TestElement.xaml
    /// </summary>
    public partial class TestElement : UserControl
    {
        public TestElement()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Wrap up(Capturing) all the data about test elemnt and send it to main window
        /// </summary>
        /// <returns></returns>
        public Project4.TestElement wrapUpData()
        {
            Project4.TestElement te = new Project4.TestElement();
            te.testName = textBox.Text;
            te.testDriver = comboBox.Text;
            foreach(var item in listBox.SelectedItems)
            {
                te.testCodes.Add(item.ToString());
            }
            if (string.IsNullOrEmpty(te.testName) || string.IsNullOrEmpty(te.testDriver) || !te.testCodes.Any())
            {
                MessageBox.Show("Fill all the required fields.", "Warning!");
                return null;
            }
            else
                return te;
        }

        /// <summary>
        /// Disabling the field whose data is captured
        /// </summary>
        public void disableAllField()
        {
            textBox.IsEnabled = false;
            comboBox.IsEnabled = false;
            listBox.IsEnabled = false;
        }


        /// <summary>
        /// Populate the browsed test codes and test drivers from the Repository
        /// </summary>
        /// <param name="files"></param>
       public void populate(string[] files)
        {
            if (files == null)
                return;
            comboBox.ItemsSource = files;
            listBox.ItemsSource = files;
        }
    }
}
