using Newtonsoft.Json;
using SeldatMRMS;
using SeldatMRMS.Management;
using SeldatMRMS.Management.RobotManagent;
using SeldatUnilever_Ver1._02.Management.TrafficManager;
using System;
using System.Collections.Generic;
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

namespace SelDatUnilever_Ver1._00.Management.TrafficManager
{
    public class TrafficRounterService
    {
        public enum DIROUT_OPZONE
        {

        }
        public enum IndexZoneDefaultFind
        {

            ZONE_OP = 255,
            ZONE_STREET = 254,
            ZONE_RDSTR = 200

        }
        public enum TypeZone
        {
            IDLE,
            HIGHWAY,
            BUFFER,
            ROAD,
            READY,
            OPZ, // Operation Zone
            GATE_CHECKOUT,
            GATE_CHECKINT,

        }
        public ListCollectionView Grouped_PropertiesTrafficZoneList { get; private set; }
        public List<ZoneRegister> PropertiesTrafficZoneList;
        public ListCollectionView Grouped_PropertiesRiskZoneList { get; private set; }
        public List<RiskZoneRegister> PropertiesRiskZoneList;
        public const int HasRobotUnityinAreaValue = 200;
        protected List<RobotUnity> RobotUnityListOnTraffic = new List<RobotUnity>();
        public class RiskZoneRegister : NotifyUIBase
        {
            private String _NameId;
            public String NameId { get => _NameId; set { _NameId = value; RaisePropertyChanged("NameId"); } }
            private String _TypeZone;
            public String TypeZone { get => _TypeZone; set { _TypeZone = value; RaisePropertyChanged("TypeZone"); } }
            private int _Index;
            public int Index { get => _Index; set { _Index = value; RaisePropertyChanged("Index"); } }
            private Point _Point1;
            public Point Point1 { get => _Point1; set { _Point1 = value; RaisePropertyChanged("Point1"); } }
            private Point _Point2;
            public Point Point2 { get => _Point2; set { _Point2 = value; RaisePropertyChanged("Point2"); } }
            private Point _Point3;
            public Point Point3 { get => _Point3; set { _Point3 = value; RaisePropertyChanged("Point3"); } }
            private Point _Point4;
            public Point Point4 { get => _Point4; set { _Point4 = value; RaisePropertyChanged("Point4"); } }
            public String _Detail;
            public String Detail { get => _Detail; set { _Detail = value; RaisePropertyChanged("Detail"); } }
            private double _L1;
            public double L1 { get => _L1; set { _L1 = value; RaisePropertyChanged("L1"); } }
            private double _L2;
            public double L2 { get => _L2; set { _L2 = value; RaisePropertyChanged("L2"); } }
            private double _WS;
            public double WS { get => _WS; set { _WS = value; RaisePropertyChanged("WS"); } }
            private double _distance;
            public double distance { get => _distance; set { _distance = value; RaisePropertyChanged("Distance"); } }
            private double _Speed;
            public double Speed { get => _Speed; set { _Speed = value; RaisePropertyChanged("Speed"); } }
            public Point[] GetZone()
            {
                return new Point[4] { Point1, Point2, Point3, Point4 };
            }
        }
        public class ZoneRegister : NotifyUIBase
        {
            private String _NameId;
            public String NameId { get => _NameId; set { _NameId = value; RaisePropertyChanged("NameId"); } }
            private TypeZone _Type;
            public TypeZone Type { get => _Type; set { _Type = value; RaisePropertyChanged("Type"); } }
            private int _Index;
            public int Index { get => _Index; set { _Index = value; RaisePropertyChanged("Index"); } }
            private Point _Point1;
            public Point Point1 { get => _Point1; set { _Point1 = value; RaisePropertyChanged("Point1"); } }
            private Point _Point2;
            public Point Point2 { get => _Point2; set { _Point2 = value; RaisePropertyChanged("Point2"); } }
            private Point _Point3;
            public Point Point3 { get => _Point3; set { _Point3 = value; RaisePropertyChanged("Point3"); } }
            private Point _Point4;
            public Point Point4 { get => _Point4; set { _Point4 = value; RaisePropertyChanged("Point4"); } }
            private TrafficRobotUnity.BrDirection _Dir_Out;
            public TrafficRobotUnity.BrDirection Dir_Out { get => _Dir_Out; set { _Dir_Out = value; RaisePropertyChanged("Dir_Out"); } }
            public String _Detail;
            public String Detail { get => _Detail; set { _Detail = value; RaisePropertyChanged("Detail"); } }
            private String _ZonesCheckGoInside = "";
            public String ZonesCheckGoInside { get => _ZonesCheckGoInside; set { _ZonesCheckGoInside = value; RaisePropertyChanged("ZonesCheckGoInside"); } }
            private double _Radius_S; // small
            public double Radius_S { get => _Radius_S; set { _Radius_S = value; RaisePropertyChanged("Radius_S"); } }
            private double _Center_S; // small
            public double Center_S { get => _Center_S; set { _Center_S = value; RaisePropertyChanged("Center_S"); } }
            private double _Radius_B; // blue
            public double Radius_B { get => _Radius_B; set { _Radius_B = value; RaisePropertyChanged("Radius_B"); } }
            private double _Center_B; // small
            public double Center_B { get => _Center_B; set { _Center_B = value; RaisePropertyChanged("Center_B"); } }
            private double _Radius_Y; // yellow
            public double Radius_Y { get => _Radius_Y; set { _Radius_Y = value; RaisePropertyChanged("Radius_Y"); } }
            private double _Center_Y; // small
            public double Center_Y { get => _Center_Y; set { _Center_Y = value; RaisePropertyChanged("Center_Y"); } }
            private double _L1;
            public double L1 { get => _L1; set { _L1 = value; RaisePropertyChanged("L1"); } }
            private double _L2;
            public double L2 { get => _L2; set { _L2 = value; RaisePropertyChanged("L2"); } }
            private double _WS;
            public double WS { get => _WS; set { _WS = value; RaisePropertyChanged("WS"); } }
            private double _distance;
            public double distance { get => _distance; set { _distance = value; RaisePropertyChanged("Distance"); } }
            private double _Speed;
            public double Speed { get => _Speed; set { _Speed = value; RaisePropertyChanged("Speed"); } }
            public Point[] GetZone()
            {
                return new Point[4] { Point1, Point2, Point3, Point4 };
            }
        }
        public class RobotRegistryInArea
        {
            public RobotUnity robot;
            public String nameArea;
        }
        public Dictionary<String, ZoneRegister> ZoneRegisterList = new Dictionary<string, ZoneRegister>();
        public Dictionary<String, RiskZoneRegister> RiskZoneRegisterList = new Dictionary<string, RiskZoneRegister>();
        public List<RobotRegistryInArea> robotRegistryList = new List<RobotRegistryInArea>();
        public ConfigureRiskZone configureRiskZone;
        public ConfigureArea configureArea;
        public TrafficRounterService()
        {
            PropertiesTrafficZoneList = new List<ZoneRegister>();
            Grouped_PropertiesTrafficZoneList = (ListCollectionView)CollectionViewSource.GetDefaultView(PropertiesTrafficZoneList);
            PropertiesRiskZoneList = new List<RiskZoneRegister>();
            Grouped_PropertiesRiskZoneList = (ListCollectionView)CollectionViewSource.GetDefaultView(PropertiesRiskZoneList);
            LoadConfigureZone();
            LoadConfigureRiskZone();
            configureArea = new ConfigureArea(this, Thread.CurrentThread.CurrentCulture.ToString());
            configureRiskZone = new ConfigureRiskZone(this, Thread.CurrentThread.CurrentCulture.ToString());
            //configureRiskZone.Show();
        }
        public void InitializeZone()
        {
            ZoneRegister ptemp = new ZoneRegister();
            ptemp.NameId = "OPA" + ZoneRegisterList.Count;
            ptemp.Point1 = new Point(0, 0);
            ptemp.Point2 = new Point(10, 10);
            ptemp.Point3 = new Point(3, 4);
            ptemp.Point4 = new Point(5, 5);
            PropertiesTrafficZoneList.Add(ptemp);
            Grouped_PropertiesTrafficZoneList.Refresh();
            ZoneRegisterList.Add(ptemp.NameId, ptemp);

        }
        public void RegistryRobotList(Dictionary<String, RobotUnity> RobotUnitylistdc)
        {
            foreach (var r in RobotUnitylistdc.Values)
            {
                RobotUnityListOnTraffic.Add(r);
            }
        }
        public void InitializeRiskZone()
        {

            RiskZoneRegister pRtemp = new RiskZoneRegister();
            pRtemp.NameId = "OPA" + RiskZoneRegisterList.Count;
            pRtemp.Point1 = new Point(0, 0);
            pRtemp.Point2 = new Point(10, 10);
            pRtemp.Point3 = new Point(3, 4);
            pRtemp.Point4 = new Point(5, 5);
            pRtemp.L1 = 40;
            pRtemp.L2 = 40;
            pRtemp.WS = 60;
            pRtemp.distance = 40;
            pRtemp.Speed = (double)RobotUnity.RobotSpeedLevel.ROBOT_SPEED_NORMAL;
            PropertiesRiskZoneList.Add(pRtemp);
            Grouped_PropertiesRiskZoneList.Refresh();
            RiskZoneRegisterList.Add(pRtemp.NameId, pRtemp);

        }
        public void AddConfigZone()
        {
            ZoneRegister ptemp = new ZoneRegister();
            ptemp.NameId = "OPA" + ZoneRegisterList.Count;
            PropertiesTrafficZoneList.Add(ptemp);
            Grouped_PropertiesTrafficZoneList.Refresh();
            ZoneRegisterList.Add(ptemp.NameId, ptemp);
            SaveConfigZone(JsonConvert.SerializeObject(PropertiesTrafficZoneList, Formatting.Indented).ToString());
        }
        public void AddConfigRiskZone()
        {
            RiskZoneRegister ptemp = new RiskZoneRegister();
            ptemp.NameId = "OPA" + RiskZoneRegisterList.Count;
            PropertiesRiskZoneList.Add(ptemp);
            Grouped_PropertiesRiskZoneList.Refresh();
            RiskZoneRegisterList.Add(ptemp.NameId, ptemp);
            SaveConfigRiskZone(JsonConvert.SerializeObject(PropertiesRiskZoneList, Formatting.Indented).ToString());
        }
        public void SaveConfigZone(String data)
        {
            String path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ConfigZone.json");
            System.IO.File.WriteAllText(path, data);
        }
        public void SaveConfigRiskZone(String data)
        {
            String path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ConfigRiskZone.json");
            System.IO.File.WriteAllText(path, data);
        }
        public bool LoadConfigureZone()
        {
            String path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ConfigZone.json");
            if (!File.Exists(path))
            {
                InitializeZone();
                SaveConfigZone(JsonConvert.SerializeObject(PropertiesRiskZoneList, Formatting.Indented).ToString());

                return false;
            }
            else
            {
                try
                {
                    String data = File.ReadAllText(path);
                    if (data.Length > 0)
                    {
                        List<ZoneRegister> tempPropertiestZ = JsonConvert.DeserializeObject<List<ZoneRegister>>(data);
                        tempPropertiestZ.ForEach(e => { PropertiesTrafficZoneList.Add(e); ZoneRegisterList.Add(e.NameId, e); });
                        Grouped_PropertiesTrafficZoneList.Refresh();
                        return true;
                    }
                }
                catch { }
            }
            return false;
        }
        public bool LoadConfigureRiskZone()
        {
            String path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ConfigRiskZone.json");

            if (!File.Exists(path))
            {
                InitializeRiskZone();
                SaveConfigRiskZone(JsonConvert.SerializeObject(PropertiesRiskZoneList, Formatting.Indented).ToString());

                return false;
            }
            else
            {
                try
                {
                    String data = File.ReadAllText(path);
                    if (data.Length > 0)
                    {
                        List<RiskZoneRegister> tempPropertiestRZ = JsonConvert.DeserializeObject<List<RiskZoneRegister>>(data);
                        tempPropertiestRZ.ForEach(e => { PropertiesRiskZoneList.Add(e); RiskZoneRegisterList.Add(e.NameId, e); });
                        Grouped_PropertiesRiskZoneList.Refresh();
                        return true;
                    }
                }
                catch { }
            }
            return false;
        }

        public ZoneRegister Find(Point p, double min, double max)// sử dụng với chỉ số mặt định sẽ được bỏ qua
        {
            ZoneRegister trz = null;
            foreach (var rz in ZoneRegisterList.Values)
            {
                if (rz.Index >= min && rz.Index <= max)
                {
                    if (ExtensionService.IsInPolygon(rz.GetZone(), p))
                    {
                        trz = rz;
                        break;
                    }
                }

            }
            return trz;
        }
        public RiskZoneRegister FindRiskZone(Point p)
        {
            RiskZoneRegister trz = null;
            foreach (var rz in RiskZoneRegisterList.Values)
            {
                if (ExtensionService.IsInPolygon(rz.GetZone(), p))
                {
                    trz = rz;
                    break;
                }
            }
            return trz;
        }
        public int FindIndexZoneRegister(Point p) // nho hon vung OP
        {
            int index = -1;
            foreach (ZoneRegister z in ZoneRegisterList.Values)
            {
                if (z.Index < 200) // kiem tra lai số 225
                {
                    if (ExtensionService.IsInPolygon(z.GetZone(), p))
                    {
                        index = z.Index;
                        break;
                    }
                }
            }
            return index;
        }
        public int FindAmoutOfRobotUnityinArea(String areaName)
        {
            int amout = 0;
            foreach (RobotUnity r in RobotUnityListOnTraffic)
            {

                if (ExtensionService.IsInPolygon(ZoneRegisterList["areaName"].GetZone(), r.properties.pose.Position))
                {
                    amout++;
                }
            }
            return amout;
        }
        public String DetermineArea(Point position)
        {
            String zoneName = "";
            foreach (var r in ZoneRegisterList.Values) // xác định khu vực đến
            {

                if (ExtensionService.IsInPolygon(r.GetZone(), position))
                {
                    zoneName = r.NameId;
                    break;
                }
            }
            return zoneName;
        }
        public String DetermineArea(Point position, int min, int max)
        {
            String zoneName = "";
            foreach (var r in ZoneRegisterList.Values) // xác định khu vực đến
            {
                if (r.Index >= min && r.Index <= max)
                {
                    if (ExtensionService.IsInPolygon(r.GetZone(), position))
                    {
                        zoneName = r.NameId;
                        break;
                    }
                }
            }
            return zoneName;
        }
        public TypeZone GetTypeZone(Point position, int min, int max)
        {
            TypeZone _type = TypeZone.IDLE;
            int index= max+200;
            foreach (var r in ZoneRegisterList.Values) // xác định khu vực đến
            {
                if (r.Index >= min && r.Index <= max)
                {
                    if (ExtensionService.IsInPolygon(r.GetZone(), position))
                    {
                        if (index > r.Index)
                        {
                            index = r.Index;
                            _type = r.Type;
                        }
                        
                    }
                }
            }
            return _type;
        }
        public TrafficRobotUnity.BrDirection GetDirDirection_Zone(Point p)
        {
            TrafficRobotUnity.BrDirection dir = TrafficRobotUnity.BrDirection.FORWARD;
            foreach (var r in ZoneRegisterList.Values) // xác định khu vực đến
            {

                if (ExtensionService.IsInPolygon(r.GetZone(), p))
                {
                    dir = r.Dir_Out;
                    break;
                }
            }
            return dir;
        }
        public bool HasRobotUnityinArea(Point goal, int min, int max)
        {
            String zoneName = "";
            bool hasRobot = false;
            foreach (var r in ZoneRegisterList.Values) // xác định khu vực đến
            {

                if (r.Index >= min && r.Index <= max)
                {
                    // Console.WriteLine("-----------------" + goal.ToString());
                    // Console.WriteLine("--- "+ r.NameId + "--- "+ JsonConvert.SerializeObject(r.GetZone()).ToString());
                    if (ExtensionService.IsInPolygon(r.GetZone(), goal))
                    {
                        zoneName = r.NameId;
                        break;
                    }
                }
            }
            try
            {
                foreach (RobotUnity r in RobotUnityListOnTraffic) // xác định robot có trong khu vực
                {

                    if (ExtensionService.IsInPolygon(ZoneRegisterList[zoneName].GetZone(), r.properties.pose.Position))
                    {
                        hasRobot = true;
                        break;
                    }
                }
            }
            catch { }
            return hasRobot;
        }

        public bool HasRobotRegistriedUnityinArea(Point goal)
        {
            String zoneName = "";
            bool hasRobot = false;
            foreach (var r in ZoneRegisterList.Values) // xác định khu vực đến
            {

                if (r.Index < HasRobotUnityinAreaValue)
                {
                    // Console.WriteLine("-----------------" + goal.ToString());
                    // Console.WriteLine("--- "+ r.NameId + "--- "+ JsonConvert.SerializeObject(r.GetZone()).ToString());
                    if (ExtensionService.IsInPolygon(r.GetZone(), goal))
                    {
                        zoneName = r.NameId;
                        break;
                    }
                }
            }
            try
            {
                foreach (RobotRegistryInArea r in robotRegistryList) // xác định robot có trong khu vực
                {

                    if (zoneName.Equals(r.nameArea))
                    {
                        hasRobot = true;
                        break;
                    }
                }
            }
            catch { }
            return hasRobot;
        }
        public bool HasRobotUnityinArea(String AreaName)
        {
            bool hasRobot = false;
            foreach (RobotUnity r in RobotUnityListOnTraffic) // xác định robot có trong khu vực
            {

                if (ExtensionService.IsInPolygon(ZoneRegisterList[AreaName].GetZone(), r.properties.pose.Position))
                {
                    hasRobot = true;
                    break;
                }
            }
            return hasRobot;
        }
        public bool HasRobotUnityinArea(String AreaName, RobotUnity robot)
        {
            bool hasRobot = false;
            if (ExtensionService.IsInPolygon(ZoneRegisterList[AreaName].GetZone(), robot.properties.pose.Position))
            {
                hasRobot = true;
            }
            return hasRobot;
        }
        public bool RobotIsInArea(String AreaName, Point position)
        {
            bool ret = false;
            foreach (var r in ZoneRegisterList.Values) // xác định khu vực đến
            {
                if (r.NameId.Equals(AreaName))
                {
                    if (ExtensionService.IsInPolygon(r.GetZone(), position))
                    {
                        ret = true;
                        break;
                    }
                }
            }
            return ret;
        }
        public void FixedPropertiesZoneRegister(String nameID)
        {

        }
        public void ClearZoneRegister(String nameID)
        {
            ZoneRegisterList.Remove(nameID);
            int index = PropertiesTrafficZoneList.FindIndex(e => e.NameId.Equals(nameID));
            PropertiesTrafficZoneList.RemoveAt(index);
            Grouped_PropertiesTrafficZoneList.Refresh();
        }
        public void ClearRiskZoneRegister(String nameID)
        {
            RiskZoneRegisterList.Remove(nameID);
            int index = PropertiesRiskZoneList.FindIndex(e => e.NameId.Equals(nameID));
            PropertiesRiskZoneList.RemoveAt(index);
            Grouped_PropertiesRiskZoneList.Refresh();
        }
    }
}
