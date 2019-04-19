using Newtonsoft.Json;
using SeldatMRMS.Management.TrafficManager;
using SeldatUnilever_Ver1._02.Management.RobotManagent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using static SeldatMRMS.Management.RobotManagent.RobotBaseService;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;
using static SelDatUnilever_Ver1._00.Management.ChargerCtrl.ChargerCtrl;

namespace SeldatMRMS.Management.RobotManagent
{
    public class RobotManagementService
    {
        public const Int32 AmountofRobotUnity = 3;
        private const Int32 BAT_LOW_LEVEL = 15;
        public class ResultRobotReady
        {
            public RobotUnity robot;
            public bool onReristryCharge = false;

        }
        public ListCollectionView Grouped_PropertiesRobotUnity { get; private set; }
        public List<PropertiesRobotUnity> PropertiesRobotUnity_List;
        public Dictionary<String,RobotUnity>  RobotUnityRegistedList = new Dictionary<string, RobotUnity>();
        public List<RobotUnity> RobotUnityWaitTaskList = new List<RobotUnity>();
        public List<RobotUnity> RobotUnityReadyList = new List<RobotUnity>();
        public ConfigureRobotUnity configureForm;
        private TrafficManagementService trafficManagementService;
        private Canvas canvas;
        public RobotManagementService(Canvas canvas) {
            this.canvas = canvas;
            PropertiesRobotUnity_List = new List<PropertiesRobotUnity>();
            Grouped_PropertiesRobotUnity = (ListCollectionView)CollectionViewSource.GetDefaultView(PropertiesRobotUnity_List);
            configureForm = new ConfigureRobotUnity(this, Thread.CurrentThread.CurrentCulture.ToString());
            //LoadConfigure();
          //TestRobotProceure();
        }

        public void TestRobotReadyProceure()
        {
//#if false
            PropertiesRobotUnity prop1 = new PropertiesRobotUnity();
            prop1.NameId = "RSD" + RobotUnityRegistedList.Count;
            prop1.L1 = 2.5;
            prop1.L2 = 4;
            prop1.WS = 3;
            prop1.Label = "Robot1";
            prop1.BatteryLevelRb = 40;
            prop1.Url = "ws://192.168.1.181:9090";
            prop1.ipMcuCtrl = "192.168.1.211";
            prop1.portMcuCtrl = 8081;
            prop1.DistInter = 8;
            prop1.BatteryLowLevel = BAT_LOW_LEVEL;
            prop1.RequestChargeBattery = false;
            prop1.Width = 1.8;
            prop1.Height = 2.5;
            prop1.Length = 2.2;
            prop1.ChargeID = ChargerId.CHARGER_ID_1;
            prop1.Scale = 10;
            RobotUnity r1 = new RobotUnity();
            r1.Initialize(this.canvas);
            r1.UpdateProperties(prop1);
            r1.ConnectionStatusHandler += ConnectionStatusHandler;
            PropertiesRobotUnity_List.Add(r1.properties);
            RobotUnityRegistedList.Add(r1.properties.NameId, r1);

            
            // đăng ký robot list to many robot quan trong
            // AddRobotUnityReadyList(r1);
         //   AddRobotUnityReadyList(r1);
            r1.RegistryRobotService(this);

            r1.TurnOnSupervisorTraffic(false);
            /*    r1.properties.pose.Position = new Point(-7.2,0.5);
                r1.properties.pose.Angle = -180;
                r1.properties.pose.AngleW = -180*Math.PI/180;*/

            PropertiesRobotUnity prop2 = new PropertiesRobotUnity();
            prop2.NameId = "RSD" + RobotUnityRegistedList.Count;
            prop2.L1 = 2.5;
            prop2.L2 = 4;
            prop2.WS = 3;
            prop2.Label = "Robot2";
            prop2.BatteryLevelRb = 40;
            prop2.Url = "ws://192.168.1.182:9090";
            prop2.ipMcuCtrl = "192.168.1.212";
            prop2.portMcuCtrl = 8081;
            prop2.DistInter = 8;
            prop2.BatteryLowLevel = BAT_LOW_LEVEL;
            prop2.RequestChargeBattery = false;
            prop2.Width = 1.8;
            prop2.Height = 2.5;
            prop2.Length = 2.2;
            prop2.ChargeID = ChargerId.CHARGER_ID_2;
            prop2.Scale = 10;
            RobotUnity r2 = new RobotUnity();
            r2.Initialize(this.canvas);
            r2.UpdateProperties(prop2);
            r2.ConnectionStatusHandler += ConnectionStatusHandler;
            PropertiesRobotUnity_List.Add(r2.properties);
            RobotUnityRegistedList.Add(r2.properties.NameId, r2);
          //  r2.Start(prop2.Url);
            // đăng ký robot list to many robot quan trong
        //    AddRobotUnityReadyList(r2);
            r2.RegistryRobotService(this);

            r2.TurnOnSupervisorTraffic(false);

//#endif

            PropertiesRobotUnity prop3 = new PropertiesRobotUnity();
            prop3.NameId = "RSD" + RobotUnityRegistedList.Count;
            prop3.L1 = 2.5;
            prop3.L2 = 4;
            prop3.WS = 3;
            prop3.Label = "Robot3";
            prop3.BatteryLevelRb = 40;
            prop3.Url = "ws://192.168.1.183:9090";
            prop3.ipMcuCtrl = "192.168.1.213";
            prop3.portMcuCtrl = 8081;
            prop3.DistInter = 8;
            prop3.BatteryLowLevel = BAT_LOW_LEVEL;
            prop3.RequestChargeBattery = false;
            prop3.Width = 1.8;
            prop3.Height = 2.5;
            prop3.Length = 2.2;
            prop3.ChargeID = ChargerId.CHARGER_ID_3;
            prop3.Scale = 10;
            RobotUnity r3 = new RobotUnity();
            r3.Initialize(this.canvas);
            r3.UpdateProperties(prop3);
            r3.ConnectionStatusHandler += ConnectionStatusHandler;
            PropertiesRobotUnity_List.Add(r3.properties);
            RobotUnityRegistedList.Add(r3.properties.NameId, r3);
         //   r3.Start(prop3.Url);
            // đăng ký robot list to many robot quan trong
            // AddRobotUnityReadyList(r1);
            //AddRobotUnityReadyList(r3);
            r3.RegistryRobotService(this);

            r3.TurnOnSupervisorTraffic(false);

            r1.properties.pose.Position = new Point(-8,8);
            r1.properties.pose.Angle = 90;
            r1.properties.pose.AngleW = 90 * Math.PI / 180;

            r1.properties.poseRoot.Position = new Point(-8, 8);
            r1.properties.poseRoot.Angle = 90;
            r1.properties.poseRoot.AngleW = 90 * Math.PI / 180;

            r2.properties.pose.Position = new Point(-5, 8);
            r2.properties.pose.Angle = 90;
            r2.properties.pose.AngleW = 90 * Math.PI / 180;

            r2.properties.poseRoot.Position = new Point(-5, 8);
            r2.properties.poseRoot.Angle = 90;
            r2.properties.poseRoot.AngleW = 90 * Math.PI / 180;

            r3.properties.pose.Position = new Point(-2, 8);
            r3.properties.pose.Angle =90;
            r3.properties.pose.AngleW = 90 * Math.PI / 180;

            r3.properties.poseRoot.Position = new Point(-2, 8);
            r3.properties.poseRoot.Angle = 90;
            r3.properties.poseRoot.AngleW = 90 * Math.PI / 180;

            r1.Registry(trafficManagementService);
            r2.Registry(trafficManagementService);
            r3.Registry(trafficManagementService);

            r2.RegisteRobotInAvailable(RobotUnityRegistedList);
            r1.RegisteRobotInAvailable(RobotUnityRegistedList);
            r3.RegisteRobotInAvailable(RobotUnityRegistedList);

            r1.StartTraffic();
            r2.StartTraffic();
            r3.StartTraffic();

            r1.PreProcedureAs = ProcedureControlAssign.PRO_READY;
            r2.PreProcedureAs = ProcedureControlAssign.PRO_READY;
            r3.PreProcedureAs = ProcedureControlAssign.PRO_READY;

            // add robot trong traffic quản lý
            trafficManagementService.RegistryRobotList(RobotUnityRegistedList);

           //
          //  r1.Start(prop1.Url);
          //  r2.Start(prop2.Url);
          //  r3.Start(prop3.Url);

        }
        public void Initialize()
        {
            PropertiesRobotUnity prop1 = new PropertiesRobotUnity();
            prop1.NameId = "RSD"+ RobotUnityRegistedList.Count;
            prop1.L1 = 4;
            prop1.L2 = 4;
            prop1.WS = 6;
            prop1.Label = "Robot1";
            prop1.BatteryLevelRb = 40;
            prop1.Url = "ws://192.168.80.181:9090";
            prop1.ipMcuCtrl = "192.168.1.211";
            prop1.portMcuCtrl = 8081;
            prop1.DistInter = 4;
            prop1.BatteryLowLevel = BAT_LOW_LEVEL;
            prop1.RequestChargeBattery = false;
            prop1.Width = 1.8;
            prop1.Height = 2.5;
            prop1.Length = 2.2;
            prop1.ChargeID= ChargerId.CHARGER_ID_1;
            prop1.Scale = 1;
            RobotUnity r1 = new RobotUnity();
            r1.Initialize(this.canvas);
            r1.UpdateProperties(prop1);
            r1.ConnectionStatusHandler += ConnectionStatusHandler;
            PropertiesRobotUnity_List.Add (r1.properties);
            RobotUnityRegistedList.Add (r1.properties.NameId, r1);
            r1.Registry (trafficManagementService);
            r1.Start (prop1.Url);
            // đăng ký robot list to many robot quan trong
            AddRobotUnityReadyList (r1);
            r1.RegisteRobotInAvailable (RobotUnityRegistedList);
#if false
            PropertiesRobotUnity prop2 = new PropertiesRobotUnity ();
            prop2.NameId = "RSD" + RobotUnityRegistedList.Count;
            prop2.L1 = 40;
            prop2.L2 = 40;
            prop2.WS = 60;
            prop2.Label = "Robot2";
            prop2.BatteryLevelRb = 40;
            prop2.Url = "ws://192.168.1.182:9090";
            prop2.ipMcuCtrl = "192.168.1.212";
            prop2.portMcuCtrl = 8081;
            prop2.DistInter = 40;
            prop2.BatteryLowLevel = BAT_LOW_LEVEL;
            prop2.RequestChargeBattery = false;
            prop2.Width = 1.8;
            prop2.Height = 2.5;
            prop2.Length = 2.2;
            prop2.ChargeID = ChargerId.CHARGER_ID_2;
            prop2.Scale = 10;
            RobotUnity r2 = new RobotUnity();
            r2.Initialize(this.canvas);
            r2.UpdateProperties(prop2);
            r2.ConnectionStatusHandler += ConnectionStatusHandler;
            PropertiesRobotUnity_List.Add(r2.properties);
            RobotUnityRegistedList.Add(r2.properties.NameId, r2);
            r2.Registry(trafficManagementService);
            r2.Start (prop2.Url);
            AddRobotUnityReadyList (r2);
            r2.RegisteRobotInAvailable (RobotUnityRegistedList);

            PropertiesRobotUnity prop3 = new PropertiesRobotUnity();
            prop3.NameId = "RSD" + RobotUnityRegistedList.Count;
            prop3.L1 = 40;
            prop3.L2 = 40;
            prop3.WS = 60;
            prop3.Label = "Robot3";
            prop3.BatteryLevelRb = 40;
            prop3.Url = "ws://192.168.1.183:9090";
            prop3.ipMcuCtrl = "192.168.1.213";
            prop3.portMcuCtrl = 8081;
            prop3.DistInter = 40;
            prop3.BatteryLowLevel = BAT_LOW_LEVEL;
            prop3.RequestChargeBattery = false;
            prop3.Width = 1.8;
            prop3.Height = 2.5;
            prop3.Length = 2.2;
            prop3.ChargeID = ChargerId.CHARGER_ID_3;
            prop3.Scale = 10;

            RobotUnity r3 = new RobotUnity();
            r3.Initialize(this.canvas);
            r3.UpdateProperties(prop3);
            r3.ConnectionStatusHandler += ConnectionStatusHandler;
            PropertiesRobotUnity_List.Add(r2.properties);
            RobotUnityRegistedList.Add(r3.properties.NameId, r3);
            r3.Registry(trafficManagementService);
            r3.Start (prop3.Url);
            AddRobotUnityReadyList (r3);
            r3.RegisteRobotInAvailable (RobotUnityRegistedList);
            Grouped_PropertiesRobotUnity.Refresh ();
#endif
        }
        public void Registry(TrafficManagementService trafficManagementService)
        {
            this.trafficManagementService = trafficManagementService;
            foreach(RobotUnity robot in RobotUnityRegistedList.Values)
            {
                robot.Registry(this.trafficManagementService);
            }
        }
        public void ConnectionStatusHandler(Object obj, RosSocket.ConnectionStatus status)
        {
            RobotUnity robot = obj as RobotUnity;
            if (status==RosSocket.ConnectionStatus.CON_OK)
            {
              //  RobotUnityReadyList.Add(robot.properties.NameID,robot);
            }
        }
        public void AddRobotUnity()
        {
                PropertiesRobotUnity_List.Add(new RobotUnity().properties);
                Grouped_PropertiesRobotUnity.Refresh();
        }
        public void SaveConfig(String data)
        {
                String path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ConfigRobot.json");
                System.IO.File.WriteAllText(path, data);   
        }
        public bool LoadConfigure()
        {
            String path= Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ConfigRobot.json");
            if(!File.Exists(path))
            {
                Initialize();
                SaveConfig(JsonConvert.SerializeObject(PropertiesRobotUnity_List, Formatting.Indented).ToString());
                return false;
            }
            else
            {
                try
                {
                    String data = File.ReadAllText(path);
                    if (data.Length > 0)
                    {
                        List<PropertiesRobotUnity> tempPropertiestRobotList = JsonConvert.DeserializeObject<List<PropertiesRobotUnity>>(data);
                        foreach(var e in tempPropertiestRobotList)
                        {
                            PropertiesRobotUnity_List.Add(e);
                            RobotUnity robot = new RobotUnity();
                           
                            robot.Initialize(this.canvas);
                            robot.UpdateProperties(e);
                            //robot.Registry(trafficManagementService);
                            RobotUnityRegistedList.Add(e.NameId,robot);
                            robot.Start(robot.properties.Url);
                            AddRobotUnityReadyList(robot);
                            robot.RegisteRobotInAvailable(RobotUnityRegistedList);
                         
                        }
                        Grouped_PropertiesRobotUnity.Refresh();
                        return true;
                    }                   
                }
                catch { }
            }
            return false;
        }
        public void AddRobotUnityWaitTaskList(RobotUnity robot)
        {
                if (!RobotUnityWaitTaskList.Contains(robot))
                {
                    RobotUnityWaitTaskList.Add(robot);
                }
        }
        public void RemoveRobotUnityWaitTaskList(RobotUnity robot)
        {
            try
            {
                if (RobotUnityWaitTaskList.Contains(robot))
                {
                    RobotUnityWaitTaskList.Remove(robot);

                }
            }
            catch
            {
                Console.WriteLine("Xóa Waittask List Không Thành Công !");
            }
        }
        public void DestroyAllRobotUnity()
        {
            foreach (var item in RobotUnityRegistedList.Values)
            {
                item.Dispose();
            }
            RobotUnityRegistedList.Clear();
        }
        public void DestroyRobotUnity(String nameID)
        {
            if(RobotUnityRegistedList.ContainsKey(nameID))
            {
                RobotUnity robot = RobotUnityRegistedList[nameID];
                robot.Dispose();
                RobotUnityRegistedList.Remove(nameID);
            }
   
        }
        public ResultRobotReady GetRobotUnityWaitTaskItem0()
        {
           
                ResultRobotReady result = null;
                
                if (RobotUnityWaitTaskList.Count > 0)
                {

                    int index = 0;
                    do
                    {
                        RobotUnity robot = RobotUnityWaitTaskList[index];
                        if (robot.properties.IsConnected)
                        {
                                result = new ResultRobotReady() { robot = robot, onReristryCharge = robot.getBattery() };
                                if (robot.getBattery())
                                {
                                        RemoveRobotUnityWaitTaskList(robot);
                                }
                                break;
                    }
                        index++;
                    } while (RobotUnityWaitTaskList.Count < index && RobotUnityWaitTaskList.Count > 0);
                }
      
            return result;
        }
        public void MoveRobotWaitTask()
        {

        }
        public void AddRobotUnityReadyList(RobotUnity robot)
        {
            if (!RobotUnityReadyList.Contains(robot))
            {
                RobotUnityReadyList.Add(robot);
            }
        }
        
        public ResultRobotReady GetRobotUnityReadyItem0()
        {
            ResultRobotReady result = null;
            if (RobotUnityReadyList.Count > 0)
            {
                int index = 0;
                do
                {
                        RobotUnity robot = RobotUnityReadyList[index];
                        try
                        {
                            if (robot.properties.IsConnected)
                            {
                                result = new ResultRobotReady() { robot = robot, onReristryCharge = robot.getBattery() };
                                if (robot.getBattery())
                                {
                                    RemoveRobotUnityReadyList(robot);
                                }

                                break;
                            }
                        }
                        catch
                        {
                            Console.WriteLine("Error ReadyTask in  RobotManagement Service Remove Robot");
                        }
                        index++;
                }while (RobotUnityReadyList.Count < index && RobotUnityReadyList.Count > 0) ;
            }
            return result;
        }
        public void MoveElementToEnd(Dictionary<String,RobotUnity> dic,int newPos, int oldPos)
        {

        }
        public void RemoveRobotUnityReadyList(RobotUnity robot)
        {

            if (RobotUnityReadyList.Contains(robot))
            {
                RobotUnityReadyList.Remove(robot);
            }

        }
        public void StopAt(String nameID)
        {
            if (RobotUnityRegistedList.ContainsKey(nameID))
                RobotUnityRegistedList[nameID].SetSpeed(RobotSpeedLevel.ROBOT_SPEED_STOP);
        }
        public void RunAt(String nameID)
        {
            if (RobotUnityRegistedList.ContainsKey(nameID))
                RobotUnityRegistedList[nameID].SetSpeed(RobotSpeedLevel.ROBOT_SPEED_NORMAL);
        }
        public void Stop()
        {
            foreach (RobotUnity r in RobotUnityReadyList)
                r.SetSpeed(RobotSpeedLevel.ROBOT_SPEED_STOP);
        }
        public void Run()
        {
            foreach (RobotUnity r in RobotUnityReadyList)
                r.SetSpeed(RobotSpeedLevel.ROBOT_SPEED_NORMAL);
        }
        public void RemoveRobotUnityRegistedList(String nameID)
        {
            if (RobotUnityRegistedList.ContainsKey(nameID))
                RobotUnityRegistedList.Remove(nameID);
        }
        public void FixedPropertiesRobotUnity(String nameID,PropertiesRobotUnity properties)
        {
            /* DialogResult result = System.Windows.Forms.MessageBox.Show("Bạn chắc chắn Robot đang nằm ở vùng Charge/Ready?", "Confirmation", MessageBoxButtons.YesNo);
             if (result == DialogResult.Yes)
             {
                 RobotUnity Rd = RobotUnityRegistedList[nameID];
                 Rd.RemoveDraw();
                 Rd.Dispose();
                 RemoveRobotUnityRegistedList(nameID);
                 RemoveRobotUnityWaitTaskList(nameID);
                 RemoveRobotUnityReadyList(nameID);
                 Rd = null;
                 RobotUnity rn = new RobotUnity();
                 // cài đặt canvas
                 rn.Initialize(this.canvas);
                 rn.UpdateProperties(properties);

                 // update properties

                 // connect ros
                 rn.Start(properties.Url);
                 // đăng ký giao thông
                 rn.Registry(trafficManagementService);
                 RobotUnityRegistedList.Add(nameID, rn);
                 RobotUnityReadyList.Add( rn);
                 // dăng ký robot list
                 rn.RegisteRobotInAvailable(RobotUnityRegistedList);
             }
             else if (result == DialogResult.No)
             {
                 //...
             }*/

        }
    }
}
