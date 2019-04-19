using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using SeldatUnilever_Ver1._02;
using SelDatUnilever_Ver1._00.Communication;
using SelDatUnilever_Ver1._00.Communication.HttpServerRounter;
using static SelDatUnilever_Ver1._00.Communication.HttpServerRounter.HttpProcessor;
using static SelDatUnilever_Ver1._00.Management.DeviceManagement.DeviceItem;

namespace SelDatUnilever_Ver1._00.Management.DeviceManagement
{
    public class DeviceRegistrationService : HttpServer
    {

        public MainWindow mainWindow;
        public List<DeviceItem> deviceItemList;

        public DeviceRegistrationService(int port) : base(port)
        {
            deviceItemList = new List<DeviceItem>();
            CreateFolder();
        }

        public void RegistryMainWindow (MainWindow mainWindow)
        {
            this.mainWindow = new MainWindow();
            this.mainWindow = mainWindow;
        }

        public void RemoveDeviceItem(String userName)
        {
            if (deviceItemList.Count > 0)
            {
                deviceItemList.RemoveAt(deviceItemList.FindIndex(e => e.userName == userName));
            }
        }
        public void CreateFolder()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            // Combine the base folder with your specific folder....
            string specificFolder = Path.Combine(folder, "SelDat\\DeviceItems");
            // CreateDirectory will check if folder exists and, if not, create it.
            // If folder exists then CreateDirectory will do nothing.
            Directory.CreateDirectory(specificFolder);
            if (!System.IO.Directory.Exists(specificFolder))
            {
                System.IO.Directory.CreateDirectory(specificFolder);
            }
        }
        public int HasDeviceItemAt(String userName)
        {
            return deviceItemList.FindIndex(e => e.userName.Equals(userName));
            //
        }
        public DeviceItem FindDeviceItem(String userName)
        {
            return deviceItemList.Find(e => e.userName == userName);
        }
        public override void handlePOSTRequest(HttpProcessor p, StreamReader inputData)
        {
            String data = inputData.ReadToEnd();
            JObject results = JObject.Parse(data);
            String userName = (String)results["userName"];
            StatusOrderResponse statusOrder = null;
            if (HasDeviceItemAt(userName) >= 0)
            {
                statusOrder= FindDeviceItem(userName).ParseData(data);
                
            }
            else
            {
                DeviceItem deviceItem = new DeviceItem(this.mainWindow);
                deviceItem.userName = userName;
                statusOrder=deviceItem.ParseData(data);
                deviceItemList.Add(deviceItem);
                
            }
            if(statusOrder.status==(int)StatusOrderResponseCode.ORDER_STATUS_RESPONSE_SUCCESS)
            {
                p.handlePOSTResponse(p, StatusHttPResponse.STATUS_MESSAGE_SUCCESS);
            }
            else if (statusOrder.status == (int)StatusOrderResponseCode.ORDER_STATUS_RESPONSE_ERROR_DATA)
            {
                p.handlePOSTResponse(p, StatusHttPResponse.STATUS_MESSAGE_ERROR);
            }
            if (statusOrder.status == (int)StatusOrderResponseCode.ORDER_STATUS_RESPONSE_NOACCEPTED)
            {
                p.handlePOSTResponse(p, StatusHttPResponse.STATUS_MESSAGE_NOACCEPTED);
            }
        }

        public List<DeviceItem> GetDeviceItemList()
        {
            return deviceItemList;
        }

    }
}