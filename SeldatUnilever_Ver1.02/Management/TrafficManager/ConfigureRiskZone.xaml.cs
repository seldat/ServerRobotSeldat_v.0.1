using Newtonsoft.Json;
using SeldatMRMS;
using SelDatUnilever_Ver1._00.Management.TrafficManager;
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
using static SelDatUnilever_Ver1._00.Management.TrafficManager.TrafficRounterService;

namespace SeldatUnilever_Ver1._02.Management.TrafficManager
{
    /// <summary>
    /// Interaction logic for ConfigureRiskZone.xaml
    /// </summary>
    public partial class ConfigureRiskZone : Window
    {
        TrafficRounterService trafficRounterService;
        public ConfigureRiskZone(TrafficRounterService trafficRounterService, string cultureName = null)
        {
            try
            {
                InitializeComponent();
                ApplyLanguage(cultureName);
                this.trafficRounterService = trafficRounterService;
                DataContext = trafficRounterService;
            }
            catch
            {

            }
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
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void btn_addItem_Click(object sender, RoutedEventArgs e)
        {
            trafficRounterService.AddConfigRiskZone();
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Global_Object.userLogin <=2)
            {
                MainDataGrid.IsEnabled = true;
                btn_addItem.IsEnabled = true;
            }
            else
            {
                MainDataGrid.IsEnabled = false;
                btn_addItem.IsEnabled = false;
            }
        }

        private void RemoveBtn_Click(object sender, RoutedEventArgs e)
        {
            RiskZoneRegister rzr = (sender as Button).DataContext as RiskZoneRegister;
            trafficRounterService.ClearRiskZoneRegister(rzr.NameId);
            trafficRounterService.SaveConfigRiskZone(JsonConvert.SerializeObject(MainDataGrid.ItemsSource, Formatting.Indented));
        }

        private void FixedBtn_Click(object sender, RoutedEventArgs e)
        {
            RiskZoneRegister rzr = (sender as Button).DataContext as RiskZoneRegister;
            trafficRounterService.SaveConfigRiskZone(JsonConvert.SerializeObject(MainDataGrid.ItemsSource, Formatting.Indented));

        }
    }
}
