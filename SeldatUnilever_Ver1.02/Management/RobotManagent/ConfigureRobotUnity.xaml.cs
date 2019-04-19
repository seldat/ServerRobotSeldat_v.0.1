using Newtonsoft.Json;
using SeldatMRMS;
using SeldatMRMS.Management.RobotManagent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;

namespace SeldatUnilever_Ver1._02.Management.RobotManagent
{
    /// <summary>
    /// Interaction logic for ConfigureRobotUnity.xaml
    /// </summary>
    public partial class ConfigureRobotUnity : Window
    {
        private RobotManagementService robotManagementService;
        public ConfigureRobotUnity(RobotManagementService robotManagementService, string cultureName = null)
        {
            try
            {
                InitializeComponent();
                ApplyLanguage(cultureName);
                this.robotManagementService = robotManagementService;
                DataContext = robotManagementService;
            }
            catch { }
        }

        public void ApplyLanguage(string cultureName = null)
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
        }

        private void FixedBtn_Click(object sender, RoutedEventArgs e)
        {
            PropertiesRobotUnity properties = (sender as Button).DataContext as PropertiesRobotUnity;
            robotManagementService.FixedPropertiesRobotUnity(properties.NameId,properties);
            robotManagementService.SaveConfig(JsonConvert.SerializeObject(MainDataGrid.ItemsSource, Formatting.Indented));
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Global_Object.userLogin <=2)
            {
                MainDataGrid.IsEnabled = true;
            }
            else
            {
                MainDataGrid.IsEnabled = true;
            }
        }
    }
}
