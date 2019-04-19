using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SeldatUnilever_Ver1._02.Management.RobotManagent
{
    /// <summary>
    /// Interaction logic for RobotLogOut.xaml
    /// </summary>
    public partial class RobotLogOut : Window
    {
        String title;
        public RobotLogOut()
        {
            InitializeComponent();
            
            
        }
        public void SetName(String name)
        {
            this.Title = name;
        }
        public void ShowText(String src, String txt)
        {
            object obj = new object();
            lock (obj)
            {
                    txt_logout.Dispatcher.Invoke(() =>
                    {
                        txt_logout.AppendText(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss tt") +" ["+src+"] >> " + txt + Environment.NewLine);
                        
                    });
            }
        }
        public void ShowTextTraffic(String txt)
        {
          /*  object obj = new object();
            lock (obj)
            {
                try
                {
                    Dispatcher.Invoke(() =>
                    {
                       // txt_logout_traffic.AppendText(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss tt") + ": " + txt + Environment.NewLine);
                     
                        // scroll it automatically
                       // txt_logout_traffic.ScrollToEnd();
                    });
                }
                catch { }
            }*/
        }
        public void Clear()
        {
            txt_logout.Document.Blocks.Clear();
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_clear_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private void Btn_save_Click(object sender, RoutedEventArgs e)
        {
            String procedureStr = new TextRange(txt_logout.Document.ContentStart, txt_logout.Document.ContentEnd).Text;
            String trafficStr = new TextRange(txt_logout.Document.ContentStart, txt_logout.Document.ContentEnd).Text;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.IO.File.WriteAllText(saveFileDialog.FileName+"_proc_"+DateTime.Now.ToString("yyyyMMddHHmmss tt")+".txt", procedureStr);
                System.IO.File.WriteAllText(saveFileDialog.FileName+"_traf_" + DateTime.Now.ToString("yyyyMMddHHmmss tt") + ".txt", trafficStr);
            }
        }
    }
}
