   M S B A M L   `   `   `  ����E  AClient1GUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:Client1GUI
Client1GUI  5      "    Client1GUI.TestHarnessMainUI   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      *localclr-namespace:Client1GUI     5      .��5	      � .+�5
   
   �� -   5          ��Name$  label��5
      $��Label��6   $
��Left=�6/   $��4��6J   $��
105,74,0,0q�6U   $	��Top=�6i   $��4��6�   �� 5   
   -   5         ��Name$ 
tabControl��5      $��Stretch=�6)   $��
5,-5,-5,10q�6G   $	��340��6[   .@�5      �� -   5        $��TestRequestGenerate��5      $��SendTestRequest��61   $��-2,-2,-2.5,0q�6J   $��20��6`   $	��Top=�6l   .��5      � $��0,-30,-3,-22q�6   $	��453��6-   $	��Top=�6:   .+�5      �� $��Visible=�6#   $��Visible=�6K   $��10,55,10,92q�6q    J� RenderTransformOrigin$ 0.511,0.682&�6�   .��5      �� -   5        $��
stackPanel��5   %   $
��Left=�67   $	��Top=�6R   $	��290��6j   5      �� 5      -   5         ��Name$ btnSend��5      $
��Send��6.   $
��Left=�6=   $��238,405,0,0q�6X   $	��Top=�6m   $��75��6�   $ 0.516,-1.43&�6�   6�   �� 5      -   5        $ btnNewTestElement��5      $��Add New Test Element��68   $
��Left=�6W   $��181,380,0,0q�6r   $	��Top=�6�   $	��132��6�   6�   5      5      �� 5      -   5        $��FileManagerTab��5      $��SendFiletoRepo��6,   .��5      � ���	#FFE5E5E5$��
0,-24,0,24q�6.   .+�5      �� -   5        $  label1��5      $C��=Select the DLL file/s which you want to
 send to Repository:��6,   $
��Left=�6|   $��	27,44,0,0q�6�   $	��Top=�6�   $	��254��6�   $��43��6�   �� 5      -	   5        $ 	btnBrowse��5      $��Browse��60   $
��Left=�6A   $	��Top=�6\   $��75��6t   $��
239,67,0,0q�6   6�   �� 5      -
   5         ��Name$ richTextBox��5   "   $
��Left=�67   $	��125��6R   $��	27,92,0,0q�6_   $	��Top=�6r   $	��287��6�   . �5      /� 5      �� 5      -   5        $ btnSendFiles��5      $��	SendFiles��63   $
��Left=�6G   $��239,244,0,0q�6b   $	��Top=�6w   $��75��6�   6�   � 5      -   5        $��progressBar��5   "   $
��Left=�65   $��10��6P   $��
27,413,0,0q�6\   $	��Top=�6p   $	��287��6�   �� 5       -   5        $#  lblFileCompletionNotification��5       $�� ��6C   $
��Left=�6N   $��
27,386,0,0q�6i   $	��Top=�6}   $	��254��6�   �� 5!      -   5        $ btnClear��5!      $��	Clear All��6/   $
��Left=�6C   $��
27,244,0,0q�6^   $	��Top=�6r   $��74��6�   6�   5"      5#      �� 5$      -   5        $��GetFiles��5$      $��GetLogs��6&   .��5%      � ���	#FFE5E5E5.+�5&      �� -   5        $  lblInfo��5&      $����This functionality basically search log files on the basis of 
 the keyword you provide in the textbox and retun all 
the files it gets. hen you should select one of the file and 
 click GET LOG button to get log for that file.��6-   $
��Left=�6'  $��	10,10,0,0q�6B  $	��Top=�6U  $��76��6m  $	��314��6y  �� 5'      -   5        $  lblkeyWords��5'      $��Enter KeyWords:��61   $
��Left=�6K   $��	28,91,0,0q�6f   $	��Top=�6y   �� 5(      -   5         ��Name$ tbxKeyWords��5(      $
��Left=�63   $��23��6N   $��
130,91,0,0q�6Z    �� TextWrapping$
 Wrap=�6n   $	��Top=�6�   $	��180��6�   �� 5)      -   5        $	 btn��5)      $��Search��6*   $
��Left=�6;   $��235,119,0,0q�6V   $	��Top=�6k   $��75��6�   6�   �� 5*      -   5         ��Name$ lbxSeachFiles��5*      $
��Left=�65   $	��246��6P   $��
10,144,0,0q�6]   $	��Top=�6q   $	��300��6�   � 5+      -   5        $��getFileLoaderFromKeyword��5+   "   $
��Left=�6C   $��15��6^   $��
10,396,0,0q�6j   $	��Top=�6~   $	��100��6�   �� 5,      -   5        $ btnGetLogforKeyWords��5,      $��Get Log��6;   $
��Left=�6M   $��235,396,0,0q�6h   $	��Top=�6}   $��75��6�   6�   5-      5.      �� 5/      -   5        $��Result��5/      $��TestResultViewer��6$   .��50      � ���	#FFE5E5E5.+�51      &   System.Windows.Controls.DataGrid  -   5          Name$ dataGrid��51      $
��Left=�61   $	��Top=�6L   $	��386��6d   $	��334��6q   	   AutoGenerateColumns	 .  
   Columns
 52      0  *System.Windows.Controls.DataGridTextColumn  53      ,  &System.Windows.Controls.DataGridColumn   Header$ Id��61   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6=   �� *id+  54      $ 
AuthorName��61    6E   �� *
authorName+  55      $ 	DateStamp��61    6D   �� *
dateTime+  56      $ State��61    6@   �� *status+  57      $ Com.Latency(uSec)��61    6L   �� *time+59      � 5:      -   5        $ ��pgsbarGetLogforTestRequets��5:   "   $
��Left=�6D   $��10��6_   $��
10,395,0,0q�6k   $	��Top=�6   $��71��6�   �� 5;      -   5        $ 
btnGetLogs��5;      $��GetLogs��61   $
��Left=�6C   $��242,396,0,0q�6^   $	��Top=�6s   $��74��6�   6�   5<      5=      5?      5A      5B      