/////////////////////////////////////////////////////////////////////
// TestHarnessMainUI.xaml.cs - It is GUI for clients to ineract    //
//  with application.                                              //
//                                                                 //
// Application: CSE681 - Software Modelling and Analysis,          //
// Remote Test Harness Project-4                                   //
// Author:      Arpit Shah, Syracuse University,                   //
//              aushah@syr.edu, (646) 288-9410                     //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operation:
 * ================
 * It is user control which will get added in Main window
 * when client sign in in test hanness client.
 * 
 * Public Interface 
 * ================
 *  public static void change(Message msg)               // It will update the datagrid of UI as request get executed or started
 *   public string[] browseRepository() //It will inititate function on main window to get browse files using communication channel
 *   public void addTestElement()//add test element in send test request tab.
 *   public void chnageInTestResult(Message msg) //this method will be called by ManWindow when it gets message about change in test request or result information.
 *   public void populatingSearchedFile(Message msg)//It will populating the searched log files from repository
 *  public static System.Collections.ObjectModel.ObservableCollection<TestViewrDataGrid> getData() //return the data which will be used to fill the datagrid
 *   
 *   
 * Build Process
 * =============
 * - Required Files: HRTimer.cs Sender.cs Receiver.cs Utlities.cs TestRequest.cs ICommService.cs TestHarnessMainUI.xaml.cs
 * - Compiler Command: csc HRTimer.cs Sender.cs Receiver.cs Utlities.cs TestRequest.cs ICommService.cs TestHarnessMainUI.xaml.cs
 * 
 * Maintainance History
 * ====================
 * ver 1.0 : 20 November 2016
 *     - first release 
 */
using Project4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using HRTimer;

namespace Client2GUI
{
    public class TestViewrDataGrid
    {
        public int id { get; set; }
        public string authorName { get; set; }
        public DateTime dateTime { get; set; }
        public string status { get; set; }
        public ulong time { get; set; }
        HiResTimer hrt = new HiResTimer();
        static int tempId=0;
        private static System.Collections.ObjectModel.ObservableCollection<TestViewrDataGrid> td = new System.Collections.ObjectModel.ObservableCollection<TestViewrDataGrid>();
        public static void add(string authorName, DateTime dateTime, string status)
        {
            TestViewrDataGrid newData = new TestViewrDataGrid();
            newData.id = ++tempId;
            newData.authorName = authorName;
            newData.dateTime = dateTime;
            newData.status = status;
            Console.Write("\n Starting HRTimer to measure Communication Latency time.");
            newData.hrt.Start();
            td.Add(newData);
        }

        /// <summary>
        /// It will update the datagrid of UI 
        /// as request get executed or started
        /// </summary>
        /// <param name="msg"></param>
        public static void change(Message msg)
        {
            foreach (var d in td)
            {
                if (d.authorName == msg.author && d.dateTime.Equals(msg.time))
                {
                    d.hrt.Stop();
                    Console.Write("\n Stopped HRTimer to measure Communicatiob Latency time.");
                    Console.Write("\n Updating datagrid of Test Result viewer since we et some update about test request execution.");
                    d.status = msg.body;
                    d.time = d.hrt.ElapsedMicroseconds;
                    return;
                }
            }
        }

        /// <summary>
        /// Return data of the TestViewrDataGrid object
        /// </summary>
        /// <returns></returns>
        public static System.Collections.ObjectModel.ObservableCollection<TestViewrDataGrid> getData()
        {
            return td;
        }
    } 

    /// <summary>
    /// Interaction logic for TestHarnessMainUI.xaml
    /// </summary>
    public partial class TestHarnessMainUI : UserControl
    {
        static readonly object _object = new object();
        string author,authorType;
        List<Project4.TestElement> testCollection = new List<Project4.TestElement>();
        TestElement te = null;

        //differnt delegates to update UI using dispatcher or child User control UI
        public delegate void sendMessagedelegate(Message msg);
        public delegate void progressBarControl(ProgressBar bar,string type);
        public delegate string browserepositorydelegate();
        public delegate void displaytestHanessScreenUpdate();
        public delegate void populateSearchedFile(string files);
        public delegate void FileLoaderFromKeyworddelagate(bool i);
        public FileLoaderFromKeyworddelagate loadController = null;
        public populateSearchedFile populateControl = null;
        public delegate void adddatagrid(string authorName, DateTime dateTime, string status);
        public adddatagrid addcontrol = null;
        public displaytestHanessScreenUpdate update = null;
        public browserepositorydelegate browse = null;
        public progressBarControl control = null;
        public sendMessagedelegate sndMsg = null;

        //keep the track of previous tab
        private int previoustab = -1;

        public TestHarnessMainUI(string author,string authorType)
        {
            this.author = author;
            this.authorType = authorType;
            InitializeComponent();
            dataGrid.IsReadOnly = true;
            richTextBox.IsReadOnly = true;
            loadController = GetFileLoaderFromKeyword;
            lblFileCompletionNotification.Content = "";
            update = updateDataGrid;
            populateControl = ((files) =>
              {
                  if (!String.IsNullOrEmpty(files))
                  {
                      lbxSeachFiles.ItemsSource = files.Split(';');
                  }
                  else
                  {
                      string[] str = new string[] { "Sorry.!Not found any files related to this keyWords.\r\n Try with New Keywords!" };
                      lbxSeachFiles.ItemsSource = str;
                  }
                  getFileLoaderFromKeyword.Visibility = Visibility.Hidden;
                  btn.IsEnabled = true;
              }
            );
            control = stopProgressBar;
            addcontrol = TestViewrDataGrid.add;
            progressBar.Visibility = Visibility.Hidden;
            pgsbarGetLogforTestRequets.Visibility = Visibility.Hidden;
            tabControl.SelectionChanged += tabControl_SelectedIndexChanged;
        }

        /// <summary>
        /// It is used to update progress bar and button while client trying
        /// to query log files
        /// </summary>
        /// <param name="condition"></param>
        private void GetFileLoaderFromKeyword(bool condition)
        {
            if (condition)
            {
                btnGetLogforKeyWords.IsEnabled = false;
                getFileLoaderFromKeyword.Visibility = Visibility.Visible;
            }
            else
            {
                btnGetLogforKeyWords.IsEnabled = true;
                btn.IsEnabled = true;
                getFileLoaderFromKeyword.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// It will inititate function on main window to get
        /// browse files using communication channel
        /// </summary>
        /// <returns></returns>
        public string[] browseRepository()
        {
            while (browse == null) ;
            return (browse.Invoke()).Split(';');
        }

        
        private void tabControl_SelectedIndexChanged(Object sender, EventArgs e)
        {
            int currentTab = tabControl.SelectedIndex;
            if ((currentTab - previoustab != 0))
            {
                previoustab = currentTab;
                if (currentTab == 0)
                {
                    stackPanel.Children.Clear();
                    testCollection.Clear();
                    addTestElement();
                    stackPanel.CanVerticallyScroll = true;
                    return;
                }
                if (currentTab == 3)
                {
                    updateDataGrid();
                    return;
                }
            }
        }

        /// <summary>
        /// add test element in send test request tab.
        /// </summary>
        public void addTestElement()
        {
           te = new TestElement();
            string[] files = browseRepository();
            
            te.populate(files);
            stackPanel.Children.Add(te);
            te.Width = 280;
        }

        /// <summary>
        /// It will update the data grid of result viewer tab
        /// </summary>
        private void updateDataGrid()
        {
            dataGrid.ItemsSource = null;
            dataGrid.ItemsSource = TestViewrDataGrid.getData();
        }

        private void btnNewTestElement_Click(object sender, RoutedEventArgs e)
        {
            if (te.wrapUpData() == null)
                return;
            te.disableAllField();
            testCollection.Add(te.wrapUpData());
            addTestElement();
        }

        /// <summary>
        /// this method will be called by ManWindow when it gets message 
        /// about change in test request or result information.
        /// </summary>
        /// <param name="msg"></param>
        public void chnageInTestResult(Message msg)
        {
            TestViewrDataGrid.change(msg);
            this.Dispatcher.BeginInvoke(update, null);
        }

        /// <summary>
        /// It will populating the searched log files from repository
        /// </summary>
        /// <param name="msg"></param>
        public void populatingSearchedFile(Message msg)
        {
            this.Dispatcher.BeginInvoke(populateControl, new object[] { msg.body });
        }

        private void btnSendFiles_Click(object sender, RoutedEventArgs e)
        {
            Console.Write("\n Sending files to the Repository.");
            btnSendFiles.IsEnabled = false;
            string selectedFileNames = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd).Text.Replace("\r\n", "");
            if(string.IsNullOrEmpty(selectedFileNames))
            {
                MessageBox.Show("Select some files before Sending.", "Warning!");
                btnSendFiles.IsEnabled = true;
                return;
            }
            Message msg = new Message();
            msg.body = selectedFileNames;
            msg.type = Message.MessageType.FilesReply;
            lblFileCompletionNotification.Content = "Sending Files...";
            progressBar.Visibility = Visibility.Visible;
            progressBar.IsIndeterminate = true;
           
            ThreadStart proc = (() =>
              {
                  if (sndMsg != null)
                      sndMsg.Invoke(msg);
                  this.Dispatcher.BeginInvoke(control, new object[] { progressBar,"SendingFiles"});
                  });
            Thread t = new Thread(proc);
            t.Start();
        }

        /// <summary>
        /// Used to chnage the status of progress bar and button on UI.
        /// by calling this method frok dispatcher.
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="type"></param>
        private void stopProgressBar(ProgressBar pb,string type)
        {
            if (string.Equals(type, "GettingLogs"))
                {
                btnGetLogs.IsEnabled = true;
            }
            else if (string.Equals(type, "SendingFiles"))
            {
                lblFileCompletionNotification.Content = "File has been sent successfully.";
                btnSendFiles.IsEnabled = true;
            }
            pb.Visibility = Visibility.Hidden;
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            System.Windows.Forms.OpenFileDialog d = new System.Windows.Forms.OpenFileDialog();
            d.Filter= "Dll Files|*.dll";
            d.InitialDirectory = System.IO.Path.GetFullPath("../../ClientDirectory/");
            d.Multiselect = true;
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (string fileName in d.FileNames)
                {
                    sb.AppendLine(fileName +";");
                }
                richTextBox.AppendText("\n"+sb.ToString());
            }
        }

        /// <summary>
        /// it used to clear whole SendFiletoRepo tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            richTextBox.SelectAll();
            richTextBox.Selection.Text = "";
            lblFileCompletionNotification.Content = "";
        }
        
        /// <summary>
        /// it will get the logs and display it 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGetLogs_Click(object sender, RoutedEventArgs e)
        {
            btnGetLogs.IsEnabled = false;
            TestViewrDataGrid data = dataGrid.SelectedItem as TestViewrDataGrid;
            Message msg = new Message();
            msg.type = Message.MessageType.LogsQuery;
            if (data == null)
            {
                MessageBox.Show("Select Tests Request to get the Logs");
                btnGetLogs.IsEnabled = true;
                return;
            }
            if (data.status == "Processing")
            {
                MessageBox.Show("Test Execution is mot Done. Wait till the completion!");
                btnGetLogs.IsEnabled = true;
                return;
            }
            string fileName = data.authorName + data.dateTime.ToString("_dd-MM-yyyy_hh-mm-ss-ffff-tt") + ".txt";
            Console.Write("\n Requesting log file:{0} from Repository.", fileName);

            msg.body = fileName;
            string file = "../../ClientDirectory/" + msg.body;
            if (sndMsg != null)
                sndMsg.Invoke(msg);
            pgsbarGetLogforTestRequets.Visibility = Visibility.Visible;
            pgsbarGetLogforTestRequets.IsIndeterminate = true;
            ThreadStart t = GetlogfileThreadStart(fileName, file);
            Thread p = new Thread(t);
            p.Start();
        }

        /// <summary>
        /// Defining threadstart to get the log file and show it on UI
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        private ThreadStart GetlogfileThreadStart(string fileName, string file)
        {
            ThreadStart t = (() =>
            {
                //to limit the try for waiting for file
                int i = 0;
                while (!File.Exists(System.IO.Path.GetFullPath(file)))
                {
                    if (i == 5)
                    {
                        MessageBox.Show("Not Able to get the file", "Error!");
                        this.Dispatcher.BeginInvoke(loadController, new object[] { false });
                        return;
                    }
                    Thread.Sleep(2000);
                    i++;
                }

                string filePath = System.IO.Path.GetFullPath(file);
                string contents = null;
                if (Utilities.waitForFileToClose(3, filePath))
                {
                    contents = Utilities.ReadFromBinrayToText(filePath);
                }
                else
                {
                    contents = "File using by someone else, you cant have access of the log right NOW!";
                }
                this.Dispatcher.BeginInvoke(control, new object[] { pgsbarGetLogforTestRequets, "GettingLogs" });
                MessageBox.Show("Result Logs for Test Request" + fileName + "\n" + contents, "TestResultLogs");
            });
            return t;
        }

        /// <summary>
        /// For querying the log files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Click(object sender, RoutedEventArgs e)
        {
            Console.Write("\n Searching log files having {0} keyword", tbxKeyWords.Text);
            btn.IsEnabled = false;
            if (string.IsNullOrEmpty(tbxKeyWords.Text))
            {
                MessageBox.Show("Fill all the required fields.", "Warning!");
                btn.IsEnabled = true;
                return;
            }
            Message msg = new Message();
            msg.toUrl = Utilities.repository;
            msg.type = Message.MessageType.TestResultsQuery;
            msg.author = author;
            msg.body = tbxKeyWords.Text;
            if (sndMsg != null)
                sndMsg.Invoke(msg);
        }

        /// <summary>
        /// this is used to get logs of queried files 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGetLogforKeyWords_Click(object sender, RoutedEventArgs e)
        {
            btn.IsEnabled = false;
            btnGetLogforKeyWords.IsEnabled = false;
            if (!(lbxSeachFiles.SelectedItem != null && !(lbxSeachFiles.SelectedItem.ToString().Contains("Sorry.!Not found any files related")))) 
            {
                MessageBox.Show("Select the file to get the logs.", "Warning!");
                btn.IsEnabled = true;
            btnGetLogforKeyWords.IsEnabled = true;
            return;
            }
            Message msg = new Message();
            msg.type = Message.MessageType.LogsQuery;
            string fileName = lbxSeachFiles.SelectedItem.ToString();
            Console.Write("\n Requesting log file:{0} from Repository.",fileName );
            msg.body = fileName;
            string file = "../../ClientDirectory/" + msg.body;
            if (sndMsg != null)
                sndMsg.Invoke(msg);
            getFileLoaderFromKeyword.Visibility = Visibility.Visible;
            getFileLoaderFromKeyword.IsIndeterminate = true;
            ThreadStart t = (() =>
            {
                int i = 0; //to limit the try for waiting for file
                while (!File.Exists(System.IO.Path.GetFullPath(file)))
                {
                    if (i == 5)
                    {
                        MessageBox.Show("Not Able to get the file", "Error!");
                        this.Dispatcher.BeginInvoke(loadController, new object[] { false });
                        return;
                    }
                    Thread.Sleep(2000);
                    i++;
                }
                string filePath = System.IO.Path.GetFullPath(file);
                string contents = null;
                if (Utilities.waitForFileToClose(3, filePath))
                {
                    contents = Utilities.ReadFromBinrayToText(filePath);
                }
                else
                {
                    contents = "File using by someone else, you cant have access of the log right NOW!";
                }
                this.Dispatcher.BeginInvoke(loadController, new object[] {false});
                MessageBox.Show("Result Logs for Test Request" + fileName + "\n" + contents,"TestResultLogs");
            });
            Thread p = new Thread(t);
            p.Start();
        }

        /// <summary>
        /// It will create and send testrequest to client
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            Console.Write("\n Sending Test Request to the Test Harness server");
            if (te.wrapUpData() == null)
                return;
            testCollection.Add(te.wrapUpData());
            ThreadStart sendTestRequets = (() =>
              {
                  Message msg = new Message();
                  msg.type = Message.MessageType.TestRequest;
                  TestRequest testRequest = new TestRequest();
                  testRequest.timeStamp = msg.time;
                  testRequest.author = author;
                  testRequest.authortype = authorType;
                  if (addcontrol != null)
                      this.Dispatcher.BeginInvoke(addcontrol, new object[] { testRequest.author, msg.time, "Processing" });
                  testRequest.tests.AddRange(testCollection);
                  msg.body = testRequest.ToXml();
                  if (sndMsg != null)
                      sndMsg.Invoke(msg);
              });
            Thread t = new Thread(sendTestRequets);
            t.Start();
            stackPanel.Children.Clear();
            dataGrid.ItemsSource = TestViewrDataGrid.getData();
            tabControl.SelectedIndex = 3;
        }
    }
}

