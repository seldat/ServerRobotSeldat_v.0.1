using Newtonsoft.Json;
using SeldatMRMS;
using SelDatUnilever_Ver1._00.Management.ChargerCtrl;
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

using static SelDatUnilever_Ver1._00.Management.ChargerCtrl.ChargerCtrl;

namespace SeldatUnilever_Ver1._02.Management.ChargerCtrl
{
    /// <summary>
    /// Interaction logic for ConfigureCharger.xaml
    /// </summary>
    public partial class ConfigureCharger : Window
    {
        ChargerManagementService chargerManagementService;
        public ConfigureCharger(ChargerManagementService chargerManagementService, string cultureName = null)
        {
            try
            {
                InitializeComponent();
                ApplyLanguage(cultureName);
                Loaded += ConfigureCharger_Loaded;
                this.chargerManagementService = chargerManagementService;
                DataContext = chargerManagementService;
            }
            catch { }
        }

        private void ConfigureCharger_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyLanguage(Thread.CurrentThread.CurrentCulture.ToString());
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
            ChargerInfoConfig cf = (sender as Button).DataContext as ChargerInfoConfig;
            this.chargerManagementService.FixedConfigure(cf.Id,cf);
            this.chargerManagementService.SaveConfig(JsonConvert.SerializeObject(MainDataGrid.ItemsSource, Formatting.Indented));
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
