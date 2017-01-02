/////////////////////////////////////////////////////////////////////
// WelcomeLogin.xaml.cs - It is GUI for clients to sign in into    //
// the application.                                                //
//                                                                 //
// Application: CSE681 - Software Modelling and Analysis,          //
// Remote Test Harness Project-4                                   //
// Author:      Arpit Shah, Syracuse University,                   //
//              aushah@syr.edu, (646) 288-9410                     //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operation:
 * ================
 * It is user control which will get added in Main window as a starting screen
 * to sign in in test hanness client.
 * 
 * Public Interface 
 * ================
 *  public class SignInInfoEventArgs : EventArgs                //Creating customized event argument
 *  private void btnSignIn_Click(object sender, EventArgs e)    // It will get the author name and author type which will be used around the application.
 *  
 * Build Process
 * =============
 * - Required Files: HRTimer.cs Sender.cs Receiver.cs Utlities.cs TestRequest.cs ICommService.cs
 * - Compiler Command: csc HRTimer.cs Sender.cs Receiver.cs Utlities.cs TestRequest.cs ICommService.cs
 * - public static System.Collections.ObjectModel.ObservableCollection<TestViewrDataGrid> getData()
 * 
 * Maintainance History
 * ====================
 * ver 1.0 : 20 November 2016
 *     - first release 
 */
using System;
using System.Windows;
using System.Windows.Controls;

namespace Client2GUI
{
    /// <summary>
    /// Creating customized event argument
    /// </summary>
    public class SignInInfoEventArgs : EventArgs
    {
        public string authorName { get; set; }
        public string authorType { get; set; }
    }

    /// <summary>
    /// Interaction logic for WelcomeLogin.xaml
    /// </summary>
    public partial class WelcomeLogin : UserControl
    {
        public delegate void btnSignInClickeddelegate(object sender, SignInInfoEventArgs e);
        public event btnSignInClickeddelegate btnSignInClicked;
        public WelcomeLogin()
        {
            InitializeComponent();
        }

        /// <summary>
        /// It will get the author name and author type which will be used around the application.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSignIn_Click(object sender, EventArgs e)
        {
            SignInInfoEventArgs evnt=new SignInInfoEventArgs();
            evnt.authorName = tbxAuthorName.Text;
            evnt.authorType = tbxAuthorType.Text;
            if (string.IsNullOrEmpty(evnt.authorName) || string.IsNullOrEmpty(evnt.authorType))
            {
                MessageBox.Show("Fill all the required fields.","Warning!");
                return;
            }
            btnSignInClicked?.Invoke(sender, evnt);
        }
    }
}
