using SeldatMRMS;
using SeldatMRMS.Management.RobotManagent;
using SeldatMRMS.Management.UnityService;
using SeldatMRMS.RobotView;
using SeldatUnilever_Ver1._02.Form;
using SeldatUnilever_Ver1._02.Management.Statistics;
using SelDatUnilever_Ver1._00.Management.DeviceManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static SelDatUnilever_Ver1._00.Management.DeviceManagement.DeviceItem;

namespace SeldatUnilever_Ver1._02
{
    public class Student
    {
        public String Name { get; set; }
        public String Lastname { get; set; }
    }

    public class School
    {
        public String Name { get; set; }
        public List<Student> Students { get; set; }
    }

    public static class SchoolData
    {
        /// <summary>
        /// Returns a list of schools containing a list of students
        /// </summary>
        public static IList<School> GetSchoolData()
        {
            IList<School> schools = new List<School>(){
            new School() {
                Name = "school1",
                Students = new List<Student>() {
                    new Student(){Name="name0",Lastname="lastname0"},
                    new Student(){Name="name1",Lastname="lastname1"},
                    new Student(){Name="name2",Lastname="lastname2"},
                    new Student(){Name="name3",Lastname="lastname3"},
            }},
            new School() {
                Name = "school2" ,
                Students = new List<Student>() {
                    new Student(){Name="name10",Lastname="lastname10"},
                    new Student(){Name="name11",Lastname="lastname11"},
                    new Student(){Name="name12",Lastname="lastname12"},
                    new Student(){Name="name13",Lastname="lastname13"},
            }}
        };

            return schools;
        }
    }

    public class ViewModel : DependencyObject
    {
        public IList<School> Schools { get; set; }

        public ViewModel()
        {
            Schools = SchoolData.GetSchoolData();
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SoundPlayer Player = null;
        public System.Timers.Timer stationtimer;

        public System.Timers.Timer robotTimer;




        public bool drag = true;
        public UnityManagementService unityService;
        public CanvasControlService canvasControlService;
        CtrlRobot ctrR;

        public MainWindow()
        {
            InitializeComponent();
            ApplyLanguage();
            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
            canvasMatrixTransform = new MatrixTransform(1, 0, 0, -1, 0, 0);

            ImageBrush img = LoadImage("Map_aTan___Copy2");
            map.Width = img.ImageSource.Width;
            map.Height = img.ImageSource.Height;
            map.Background = img;
            canvasControlService = new CanvasControlService(this);
            DataContext = canvasControlService;




            //DataContext = this;
            //DataContext = new ViewModel();

        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
        }

        private void OnTimedOrderListEvent(object sender, ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke(new ThreadStart(() =>
            {
                try
                {
                    canvasControlService.ReloadListDeviceItems();
                }
                catch { Console.WriteLine("Error reload device list"); }
            }));
        }


        private void OnTimedRedrawRobotEvent(object sender, ElapsedEventArgs e)
        {
        }

        private void CenterWindowOnScreen()
        {
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth / 2) - (windowWidth / 2);
            this.Top = (screenHeight / 2) - (windowHeight / 2);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CenterWindowOnScreen();
            myManagementWindow.Visibility = Visibility.Hidden;
            LoginForm frm = new LoginForm(Thread.CurrentThread.CurrentCulture.ToString());
            frm.ShowDialog();
            if (Global_Object.userAuthor <= 2)
            {
                myManagementWindow.Visibility = Visibility.Visible;
                /* Dispatcher.BeginInvoke(new ThreadStart(() =>
              //    {
              //        canvasControlService.ReloadAllStation();
              //    }));*/
                unityService = new UnityManagementService(this);
                unityService.Initialize();
                ctrR = new CtrlRobot(unityService.robotManagementService);

                stationtimer = new System.Timers.Timer();
                stationtimer.Interval = 5000;
                stationtimer.Elapsed += OnTimedOrderListEvent;
                stationtimer.AutoReset = true;
                stationtimer.Enabled = true;

            }



        }


        private void btn_ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            ChangePassForm changePassForm = new ChangePassForm(Thread.CurrentThread.CurrentCulture.ToString());
            changePassForm.ShowDialog();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            myManagementWindow.Visibility = Visibility.Hidden;

            Global_Object.userAuthor = -2;
            Global_Object.userLogin = -2;
            Global_Object.userName = "";

            LoginForm frm = new LoginForm(Thread.CurrentThread.CurrentCulture.ToString());
            frm.ShowDialog();
            if (Global_Object.userLogin <= 2)
            {
                myManagementWindow.Visibility = Visibility.Visible;
            }
        }

        public ImageBrush LoadImage(string name)
        {
            System.Drawing.Bitmap bmp = (System.Drawing.Bitmap)Properties.Resources.ResourceManager.GetObject(name);
            ImageBrush img = new ImageBrush();
            img.ImageSource = ImageSourceForBitmap(bmp);
            return img;
        }

        public ImageSource ImageSourceForBitmap(System.Drawing.Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;

            ApplyLanguage(menuItem.Tag.ToString());

        }

        private void ApplyLanguage(string cultureName = null)
        {
            if (cultureName != null)
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);

            ResourceDictionary dict = new ResourceDictionary();
            switch (Thread.CurrentThread.CurrentCulture.ToString())
            {
                case "vi-VN":
                    dict.Source = new Uri("..\\Lang\\Vietnamese.xaml", UriKind.Relative);
                    break;
                // ...
                default:
                    dict.Source = new Uri("..\\Lang\\English.xaml", UriKind.Relative);
                    break;
            }
            this.Resources.MergedDictionaries.Add(dict);

            // check/uncheck the language menu items based on the current culture
            foreach (var item in languageMenuItem.Items)
            {
                MenuItem menuItem = item as MenuItem;
                if (menuItem.Tag.ToString() == Thread.CurrentThread.CurrentCulture.Name)
                    menuItem.IsChecked = true;
                else
                    menuItem.IsChecked = false;
            }
        }

        private void Btn_MapReCenter_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(unityService.robotManagementService.RobotUnityRegistedList.Count + "");

            unityService.assigmentTaskService.AssignTaskGoToReady(unityService.robotManagementService.RobotUnityRegistedList.ElementAt(0).Value);
        }

        private void Ctrl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {
            try
            {
                ctrR.Show();
            }
            catch { }
        }

        private void Btn_Robot_Click(object sender, RoutedEventArgs e)
        {
            unityService.OpenConfigureForm("RCF");
        }

        private void Btn_Area_Click(object sender, RoutedEventArgs e)
        {
            unityService.OpenConfigureForm("ACF");
        }

        private void Btn_Charge_Click(object sender, RoutedEventArgs e)
        {
            unityService.OpenConfigureForm("CCF");
        }

        private void Btn_Door_Click(object sender, RoutedEventArgs e)
        {
            unityService.OpenConfigureForm("DCF");
        }

        private void Btn_Statistics_Click(object sender, RoutedEventArgs e)
        {
            Statistics statistics = new Statistics(Thread.CurrentThread.CurrentCulture.ToString());
            statistics.ShowDialog();
        }

        private void DeviceItemsListDg_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (DeviceItemsListDg.SelectedItem != null)
            {
                DeviceItem temp = DeviceItemsListDg.SelectedItem as DeviceItem;
                canvasControlService.ReloadListOrderItems(temp);
            }
        }

        private void Btn_Test_Click(object sender, RoutedEventArgs e)
        {
            //unityService.deviceRegistrationService.AddNewDeviceItem();

            // canvasControlService.ReloadListDeviceItems();

            try
            {
                ctrR.Show();
            }
            catch { }
        }

        private void btn_3Dmap_Click(object sender, RoutedEventArgs e)
        {
            RobotView3D robotView = new RobotView3D();
            robotView.loadAWareHouseMap();
            robotView.RegisterRobotUnityList(new List<RobotUnity>(unityService.robotManagementService.RobotUnityRegistedList.Values));

            robotView.Show();
        }

        private void btn_Stop_Click(object sender, RoutedEventArgs e)
        {
            unityService.robotManagementService.Stop();
            btn_Play.IsEnabled = true;
            btn_Stop.IsEnabled = false;
            btn_Play_icon.Foreground = new SolidColorBrush(Colors.Green);
            btn_Stop_icon.Foreground = new SolidColorBrush(Colors.Red);
            unityService.assigmentTaskService.Dispose();

        }

        private void btn_Play_Click(object sender, RoutedEventArgs e)
        {
            unityService.robotManagementService.Run();
            btn_Play.IsEnabled = false;
            btn_Stop.IsEnabled = true;
            btn_Play_icon.Foreground = new SolidColorBrush(Colors.Red);
            btn_Stop_icon.Foreground = new SolidColorBrush(Colors.Green);
            unityService.assigmentTaskService.Start();
        }

        private void btn_RiskArea_Click(object sender, RoutedEventArgs e)
        {
            unityService.OpenConfigureForm("RACF");
        }

        private void OrderItemsListDg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void DeviceItemsListDg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Btn_Sound_Click(object sender, RoutedEventArgs e)
        {
            PlayWav(Properties.Resources.ALARM, true);
        }
        private void PlayWav(Stream stream, bool play_looping)
        {
            // Stop the player if it is running.
            if (Player != null)
            {
                Player.Stop();
                Player.Dispose();
                Player = null;
            }

            // If we have no stream, we're done.
            if (stream == null) return;

            // Make the new player for the WAV stream.
            Player = new SoundPlayer(stream);

            // Play.
            if (play_looping)
                Player.PlayLooping();
            else
                Player.Play();
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {

        }

   

        private void RemoveOrder_Click(object sender, RoutedEventArgs e)
        {
            OrderItem orderItem = (sender as Button).DataContext as OrderItem;
            Task.Run(() => { 
               
               
                    DeviceItem devI = unityService.deviceRegistrationService.deviceItemList.Find(item => item.userName == orderItem.userName);
                    devI.RemoveCallBack(orderItem);
            });


        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            OrderItem orderItem = (sender as Button).DataContext as OrderItem;
            Task.Run(() => {

                    DeviceItem devI = unityService.deviceRegistrationService.deviceItemList.Find(item => item.userName == orderItem.userName);
                    devI.ReorderCallBack(orderItem);
            });
        }
    }
}
