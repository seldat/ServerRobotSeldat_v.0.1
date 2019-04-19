using DoorControllerService;
using Newtonsoft.Json;
using SeldatMRMS;
using SeldatMRMS.Management.DoorServices;
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

namespace SeldatUnilever_Ver1._02.Management.DoorServices
{
    /// <summary>
    /// Interaction logic for DoorConfigure.xaml
    /// </summary>
    public partial class DoorConfigure : Window
    {
        DoorManagementService doorManagementService;
        public DoorConfigure(DoorManagementService doorManagementService, string cultureName = null)
        {
            InitializeComponent();
            ApplyLanguage(cultureName);
            this.doorManagementService = doorManagementService;
            DataContext = doorManagementService;
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
            DoorService ds = (sender as Button).DataContext as DoorService;
            this.doorManagementService.SaveConfig(JsonConvert.SerializeObject(MainDataGrid.ItemsSource, Formatting.Indented));
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Global_Object.userLogin <=2)
            {
                MainDataGrid.IsEnabled = true;
            }
            else
            {
                MainDataGrid.IsEnabled = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
