using DoorControllerService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SeldatUnilever_Ver1._02.Management.DoorServices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static DoorControllerService.DoorService;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;
using static SeldatMRMS.Management.TrafficRobotUnity;

namespace SeldatMRMS.Management.DoorServices
{
    public class DoorManagementService
    {
        public const Int32 AmountofDoor = 4;
        public ListCollectionView Grouped_PropertiesDoor { get; private set; }
        public List<DoorInfoConfig> PropertiesDoor_List;
        private List<DoorInfoConfig> DoorInfoConfigList;
        public DoorService DoorMezzamineUp;
        public DoorService DoorMezzamineReturn;
        public DoorElevator DoorElevator;
        public DoorConfigure doorConfigure;
        public DoorManagementService(){
            // LoadDoorConfigure();
            DoorInfoConfigList = new List<DoorInfoConfig>();
            PropertiesDoor_List = new List<DoorInfoConfig>();
            Grouped_PropertiesDoor = (ListCollectionView)CollectionViewSource.GetDefaultView(PropertiesDoor_List);

            LoadConfigure();
            
            DoorMezzamineUp = new DoorService(DoorInfoConfigList[0]);
            DoorMezzamineReturn = new DoorService(DoorInfoConfigList[1]);
            try
            {
                doorConfigure = new DoorConfigure(this);
            }
            catch { }
        }
    
        public void AddDoor()
        {
                PropertiesDoor_List.Add(new DoorInfoConfig());
                Grouped_PropertiesDoor.Refresh();
        }
        public void Initialize()
        {
            DoorInfoConfig doorICF_MUB = new DoorInfoConfig()
            {
                Id = DoorId.DOOR_MEZZAMINE_UP,
                Ip = "192.168.1.240",
                Port = 8081,
                infoPallet = "{\"pallet\":0,\"dir_main\":1,\"dir_out\":1,\"bay\":1,\"hasSubLine\":\"no\",\"line_ord\":0,\"dir_sub\":0,\"row\":0}",
                PointFrontLineStr = "2.54,-6.78,90",
                PointCheckInGateStr = "17.88,0.7,0"
            };
            doorICF_MUB.ParsePointCheckInGateValue(doorICF_MUB.PointCheckInGateStr);
            doorICF_MUB.ParsePointFrontLineValue(doorICF_MUB.PointFrontLineStr);
            PropertiesDoor_List.Add(doorICF_MUB);
            DoorInfoConfigList.Add(doorICF_MUB);

            DoorInfoConfig doorICF_MRB = new DoorInfoConfig()
            {
                Id = DoorId.DOOR_MEZZAMINE_RETURN,
                Ip = "192.168.1.241",
                Port = 8081,
                infoPallet = "{\"pallet\":1,\"dir_main\":1,\"dir_out\":1,\"bay\":1,\"hasSubLine\":\"no\",\"line_ord\":0,\"dir_sub\":0,\"row\":0}",
                 PointFrontLineStr = "2.54,-6.78,90",
                PointCheckInGateStr = "17.88,0.7,0"
            };
            doorICF_MRB.ParsePointCheckInGateValue(doorICF_MRB.PointCheckInGateStr);
            doorICF_MRB.ParsePointFrontLineValue(doorICF_MRB.PointFrontLineStr);
            PropertiesDoor_List.Add(doorICF_MRB);
            DoorInfoConfigList.Add(doorICF_MRB);

            Grouped_PropertiesDoor.Refresh();
        }
        public void SaveConfig(String data)
        {
            String path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ConfigDoor.json");
            System.IO.File.WriteAllText(path,data);
        }
        public bool LoadConfigure()
        {
            String path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ConfigDoor.json");
            if (!File.Exists(path))
            {
                Initialize();
                SaveConfig(JsonConvert.SerializeObject(PropertiesDoor_List, Formatting.Indented).ToString());
                return false;
            }
            else
            {
                try
                {
                    String data = File.ReadAllText(path);
                    if (data.Length > 0)
                    {
                        JArray results = JArray.Parse(data);
                        foreach (var result in results)
                        {
                            DoorInfoConfig doorICF = new DoorInfoConfig()
                            {
                                Id = (DoorId)((int)result["Id"]),
                                Ip = (String)result["Ip"],
                                Port = (int)result["Port"],
                                infoPallet =(String)result["infoPallet"],
                                PointFrontLineStr = (String)result["PointFrontLineStr"],
                                PointCheckInGateStr = (String)result["PointCheckInGateStr"]
                            };
                            doorICF.ParsePointCheckInGateValue(doorICF.PointCheckInGateStr);
                            doorICF.ParsePointFrontLineValue(doorICF.PointFrontLineStr);
                            PropertiesDoor_List.Add(doorICF);
                            DoorInfoConfigList.Add(doorICF);
                        }
                        Grouped_PropertiesDoor.Refresh();
                        return true;
                    }
                }
                catch { }
            }
            return false;
        }
       
        public void ResetAllDoors()
        {

        }
        public void DisposeAllDoors()
        {

        }
        public void FixedConfigure(DoorId id, DoorInfoConfig dcf)
        {
           /* if (ChargerStationList.ContainsKey(id))
            {
                ChargerStationList[(ChargerId)id].UpdateConfigure(chcf);
            }*/
        }
    }
}
