using SeldatMRMS.Management.RobotManagent;
using SeldatMRMS.Management.TrafficManager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static SelDatUnilever_Ver1._00.Management.TrafficManager.TrafficRounterService;

namespace SeldatMRMS.Management
{
    public class TrafficRobotUnity : RobotUnityService
    {
        public enum RobotBahaviorAtAnyPlace
        {
            ROBOT_PLACE_ROAD,
            ROBOT_PLACE_HIGHWAY,
            ROBOT_PLACE_BUFFER,
            ROBOT_PLACE_HIGHWAY_DETECTLINE,
            ROBOT_PLACE_HIGHWAY_READY,
            ROBOT_PLACE_IDLE
        }
        public class PriorityLevel
        {
            public PriorityLevel()
            {
                this.IndexOnMainRoad = 0;
                this.OnAuthorizedPriorityProcedure = false;
            }
            public int IndexOnMainRoad { get; set; } //  Index on Road;
            public bool OnAuthorizedPriorityProcedure { get; set; }

        }
        public enum TrafficBehaviorState
        {
            HEADER_TOUCH_TAIL,
            HEADER_TOUCH_HEADER,
            HEADER_TOUCH_SIDE,
            HEADER_TOUCH_NOTOUCH,
            MODE_FREE,
            SLOW_DOWN,
            NORMAL_SPEED
        }
        public enum RobotStatus
        {
            WORKING,
            CHARGING,
            READY,
            IDLE
        }
        public class RobotRegistryToWorkingZone
        {
            public String WorkingZone;
            public bool onRobotGoingInsideZone = false;
            public RobotRegistryToWorkingZone()
            {
                WorkingZone = "";
                onRobotGoingInsideZone = false;
            }
            public void Release()
            {
                WorkingZone = "";
                onRobotGoingInsideZone = false;
            }
            public void SetZone(String namez)
            {
                WorkingZone = namez;
                onRobotGoingInsideZone = true;
            }
        }
        // public enum MvDirection{
        //     INCREASE_X = 0,
        // 	INCREASE_Y,
        // 	DECREASE_X,
        // 	DECREASE_Y
        // // }
        public enum BrDirection
        {
            FORWARD = 0,
            DIR_LEFT,
            DIR_RIGHT
        }
        // public class PointDetect {
        //     public Point p;
        //     public MvDirection mvDir;
        //     public PointDetect(Point p, MvDirection mv)
        //     {
        //         this.p = p;
        //         mvDir = mv;
        //     }
        // }

        // public class PointDetectBranching{
        //     public PointDetect xy;
        //     public BrDirection brDir;
        // }
        public enum PistonPalletCtrl
        {
            PISTON_PALLET_UP = 0,
            PISTON_PALLET_DOWN
        }
        public class JInfoPallet
        {
            public PistonPalletCtrl pallet;
            public BrDirection dir_main;
            public Int32 bay;
            public String hasSubLine;
            public BrDirection dir_sub;
            public BrDirection dir_out;
            public int line_ord;
            public Int32 row;
            // public Int32 palletId;
        }
        private List<RobotUnity> RobotUnitylist;
        public bool onFlagSupervisorTraffic;
        public bool onFlagSelfTraffic;
        public bool onFlagSafeYellowcircle = false;
        public bool onFlagSafeBluecircle = false;
        public bool onFlagSafeSmallcircle = false;

        private Dictionary<String, RobotUnity> RobotUnityRiskList = new Dictionary<string, RobotUnity>();
        private TrafficBehaviorState TrafficBehaviorStateTracking;
        protected TrafficManagementService trafficManagementService;
        private RobotUnity robotModeFree;
        private const double DistanceToSetSlowDown = 80; // sau khi dừng robot phai doi khoan cach len duoc tren 8m thi robot bat dau hoat dong lai bình thuong 8m
        private const double DistanceToSetNormalSpeed = 12; // sau khi dừng robot phai doi khoan cach len duoc tren 8m thi robot bat dau hoat dong lai bình thuong 12m
        public RobotRegistryToWorkingZone robotRegistryToWorkingZone;
        public RobotStatus robotTag;
        public TrafficRobotUnity() : base()
        {
            TurnOnSupervisorTraffic(false);
            TurnOnCtrlSelfTraffic(true);
            RobotUnitylist = new List<RobotUnity>();
            prioritLevel = new PriorityLevel();
            robotRegistryToWorkingZone = new RobotRegistryToWorkingZone();


        }
        public void StartTraffic()
        {
            new Thread(TrafficUpdate).Start();

        }
        public PriorityLevel prioritLevel;
        public void RegisteRobotInAvailable(Dictionary<String, RobotUnity> RobotUnitylistdc)
        {
            foreach (var r in RobotUnitylistdc.Values)
            {
                if (!r.properties.NameId.Equals(this.properties.NameId))
                    this.RobotUnitylist.Add(r);
            }
            TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_NOTOUCH;
        }
        public void Registry(TrafficManagementService trafficManagementService)
        {
            this.trafficManagementService = trafficManagementService;
        }
        public RobotUnity CheckIntersection(bool turnon)
        {
            RobotUnity robot = null;
            if (turnon)
            {
                if (RobotUnitylist.Count > 0)
                {
                    foreach (RobotUnity r in RobotUnitylist)
                    {
                        if (r.robotTag==RobotStatus.WORKING)
                        {
                            Point thCV = TopHeaderCv();
                            Point mdCV0 = MiddleHeaderCv();
                            Point mdCV1 = MiddleHeaderCv1();
                            Point mdCV2 = MiddleHeaderCv2();
                            Point bhCV = BottomHeaderCv();
                            // bool onTouch= FindHeaderIntersectsFullRiskArea(this.TopHeader()) | FindHeaderIntersectsFullRiskArea(this.MiddleHeader()) | FindHeaderIntersectsFullRiskArea(this.BottomHeader());
                            // bool onTouch = r.FindHeaderIntersectsFullRiskAreaCv(thCV) | r.FindHeaderIntersectsFullRiskAreaCv(mdCV) | r.FindHeaderIntersectsFullRiskAreaCv(bhCV);

                            bool onTouch0 = r.FindHeaderInsideCircleArea(mdCV0, 2 * r.Radius_S);
                            bool onTouch1 = r.FindHeaderInsideCircleArea(mdCV1, 2 * r.Radius_S);
                            bool onTouch2 = r.FindHeaderInsideCircleArea(mdCV2, 2 * r.Radius_S);
                            if (onTouch0 || onTouch1 || onTouch2)
                            {
                                //  robotLogOut.ShowTextTraffic(r.properties.Label+" => CheckIntersection");
                                SetSpeed(RobotSpeedLevel.ROBOT_SPEED_STOP);
                                robot = r;
                                break;
                            }
                            else
                            {
                                SetSpeed(RobotSpeedLevel.ROBOT_SPEED_NORMAL);
                            }
                        }
                    }
                }
            }
            return robot;
        }
        public int CheckSafeDistance() // KIểm tra khoản cách an toàn/ nếu đang trong vùng close với robot khác thì giảm tốc độ, chuyển sang chế độ dò risk area
        {
            int iscloseDistance = 0;
            foreach (RobotUnity r in RobotUnitylist)
            {
                if (r.onFlagSupervisorTraffic)
                {
                    Point rP = MiddleHeaderCv();
                    // bool onFound = r.FindHeaderIsCloseRiskArea(this.properties.pose.Position);
                    bool onFound = r.FindHeaderIsCloseRiskAreaCv(rP);

                    if (onFound)
                    {
                        // if robot in list is near but add in risk list robot
                        //     robotLogOut.ShowTextTraffic(r.properties.Label + "- Intersection");

                        if (!RobotUnityRiskList.ContainsKey(r.properties.NameId))
                        {
                            RobotUnityRiskList.Add(r.properties.NameId, r);
                        }
                        // reduce speed robot control
                        iscloseDistance = 2;
                    }
                    else
                    {
                        // if robot in list is far but before registe in list, must remove in list
                        RemoveRiskList(r.properties.NameId);
                        double rd = ExtensionService.CalDistance(Global_Object.CoorCanvas(this.properties.pose.Position), Global_Object.CoorCanvas(r.properties.pose.Position));
                        if (rd < DistanceToSetSlowDown && rd > 60)
                            iscloseDistance = 1;
                        else
                            iscloseDistance = 0;

                    }
                }
            }
            return iscloseDistance;
        }
        public void RemoveRiskList(String NameID)
        {
            if (RobotUnityRiskList.ContainsKey(NameID))
            {
                RobotUnityRiskList.Remove(NameID);
            }
        }
        public void DetectTouchedPosition(RobotUnity robot) // determine traffic state
        {

            Point thCV = Global_Object.CoorCanvas(TopHeader());
            Point mhCV = Global_Object.CoorCanvas(MiddleHeader());
            Point mhCV1 = MiddleHeaderCv1();
            Point mhCV2 = MiddleHeaderCv2();
            Point mhCV3 = MiddleHeaderCv3();
            Point bhCV = Global_Object.CoorCanvas(BottomHeader());
            //if (robot.FindHeaderIntersectsRiskAreaHeader(this.TopHeader()) || robot.FindHeaderIntersectsRiskAreaHeader(this.MiddleHeader())|| robot.FindHeaderIntersectsRiskAreaHeader(this.BottomHeader()))
            /*  if (robot.FindHeaderIntersectsRiskAreaHeaderCv(thCV) || robot.FindHeaderIntersectsRiskAreaHeaderCv(mhCV) || robot.FindHeaderIntersectsRiskAreaHeaderCv(bhCV)
                )
              {
                  TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_HEADER;
                  robotLogOut.ShowTextTraffic(this.properties.Label + " =>" + TrafficBehaviorStateTracking + " " + robot.properties.Label);
              }
              // else if (robot.FindHeaderIntersectsRiskAreaTail(this.TopHeader()) || robot.FindHeaderIntersectsRiskAreaTail(this.MiddleHeader()) || robot.FindHeaderIntersectsRiskAreaTail(this.BottomHeader()))


              else if (robot.FindHeaderIntersectsRiskAreaTailCv(thCV) || robot.FindHeaderIntersectsRiskAreaTailCv(mhCV) || robot.FindHeaderIntersectsRiskAreaTailCv(bhCV) )
              {
                  TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_TAIL;
                  robotLogOut.ShowTextTraffic(this.properties.Label + " =>" + TrafficBehaviorStateTracking + " " + robot.properties.Label);
              }
              //   else if (robot.FindHeaderIntersectsRiskAreaRightSide(this.TopHeader())|| robot.FindHeaderIntersectsRiskAreaRightSide(this.MiddleHeader())|| robot.FindHeaderIntersectsRiskAreaRightSide(this.BottomHeader()))
              else if (robot.FindHeaderIntersectsRiskAreaRightSideCv(thCV) || robot.FindHeaderIntersectsRiskAreaRightSideCv(mhCV) || robot.FindHeaderIntersectsRiskAreaRightSideCv(bhCV) 

                  )

              {
                  TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_SIDE;
                  robotLogOut.ShowTextTraffic(this.properties.Label + " =>" + TrafficBehaviorStateTracking + " " + robot.properties.Label);
              }
              //  else if (robot.FindHeaderIntersectsRiskAreaLeftSide(this.TopHeader()) || robot.FindHeaderIntersectsRiskAreaLeftSide(this.MiddleHeader()) || robot.FindHeaderIntersectsRiskAreaLeftSide(this.BottomHeader()))
              else if (robot.FindHeaderIntersectsRiskAreaLeftSideCv(thCV) || robot.FindHeaderIntersectsRiskAreaLeftSideCv(mhCV) || robot.FindHeaderIntersectsRiskAreaLeftSideCv(bhCV) 
                                 )
              {
                  TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_SIDE;
                  robotLogOut.ShowTextTraffic(this.properties.Label + " =>" + TrafficBehaviorStateTracking + " " + robot.properties.Label);
              }
              */

            //if (robot.FindHeaderIntersectsRiskAreaHeader(this.TopHeader()) || robot.FindHeaderIntersectsRiskAreaHeader(this.MiddleHeader())|| robot.FindHeaderIntersectsRiskAreaHeader(this.BottomHeader()))
            if (robot.FindHeaderIntersectsRiskAreaHeaderCv(thCV) || robot.FindHeaderIntersectsRiskAreaHeaderCv(mhCV) || robot.FindHeaderIntersectsRiskAreaHeaderCv(bhCV)
               || robot.FindHeaderIntersectsRiskAreaHeaderCv(mhCV1) || robot.FindHeaderIntersectsRiskAreaHeaderCv(mhCV2) || robot.FindHeaderIntersectsRiskAreaHeaderCv(mhCV3))
            {
                TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_HEADER;
                //  robotLogOut.ShowTextTraffic(this.properties.Label + " =>" + TrafficBehaviorStateTracking + " " + robot.properties.Label);
            }
            // else if (robot.FindHeaderIntersectsRiskAreaTail(this.TopHeader()) || robot.FindHeaderIntersectsRiskAreaTail(this.MiddleHeader()) || robot.FindHeaderIntersectsRiskAreaTail(this.BottomHeader()))


            else if (robot.FindHeaderIntersectsRiskAreaTailCv(thCV) || robot.FindHeaderIntersectsRiskAreaTailCv(mhCV) || robot.FindHeaderIntersectsRiskAreaTailCv(bhCV) ||
                robot.FindHeaderIntersectsRiskAreaTailCv(mhCV1) || robot.FindHeaderIntersectsRiskAreaTailCv(mhCV2) || robot.FindHeaderIntersectsRiskAreaTailCv(mhCV3))
            {
                TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_TAIL;
                // robotLogOut.ShowTextTraffic(this.properties.Label + " =>" + TrafficBehaviorStateTracking + " " + robot.properties.Label);
            }
            //   else if (robot.FindHeaderIntersectsRiskAreaRightSide(this.TopHeader())|| robot.FindHeaderIntersectsRiskAreaRightSide(this.MiddleHeader())|| robot.FindHeaderIntersectsRiskAreaRightSide(this.BottomHeader()))
            /* else if (robot.FindHeaderIntersectsRiskAreaRightSideCv(thCV) || robot.FindHeaderIntersectsRiskAreaRightSideCv(mhCV) || robot.FindHeaderIntersectsRiskAreaRightSideCv(bhCV) ||
                 robot.FindHeaderIntersectsRiskAreaRightSideCv(mhCV1) || robot.FindHeaderIntersectsRiskAreaRightSideCv(mhCV2) || robot.FindHeaderIntersectsRiskAreaRightSideCv(mhCV3)
                 )

             {
                 TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_SIDE;
             //  robotLogOut.ShowTextTraffic(this.properties.Label + " =>" + TrafficBehaviorStateTracking + " " + robot.properties.Label);
             }
             //  else if (robot.FindHeaderIntersectsRiskAreaLeftSide(this.TopHeader()) || robot.FindHeaderIntersectsRiskAreaLeftSide(this.MiddleHeader()) || robot.FindHeaderIntersectsRiskAreaLeftSide(this.BottomHeader()))
             else if (robot.FindHeaderIntersectsRiskAreaLeftSideCv(thCV) || robot.FindHeaderIntersectsRiskAreaLeftSideCv(mhCV) || robot.FindHeaderIntersectsRiskAreaLeftSideCv(bhCV) ||
                 robot.FindHeaderIntersectsRiskAreaLeftSideCv(mhCV1) || robot.FindHeaderIntersectsRiskAreaLeftSideCv(mhCV2) || robot.FindHeaderIntersectsRiskAreaLeftSideCv(mhCV3)
                 )
             {
                 TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_SIDE;
              //robotLogOut.ShowTextTraffic(this.properties.Label + " =>" + TrafficBehaviorStateTracking + " " + robot.properties.Label);
             }*/


        }
        /*   public void TrafficBehavior(RobotUnity robot)
           {
               switch (TrafficBehaviorStateTracking)
               {
                   case TrafficBehaviorState.HEADER_TOUCH_NOTOUCH:
                       SetSpeed(RobotSpeedLevel.ROBOT_SPEED_NORMAL);
                     // robotLogOut.ShowTextTraffic(this.properties.Label + " => NORMAL");
                       // robot speed normal;
                       break;
                   case TrafficBehaviorState.HEADER_TOUCH_HEADER:
                       // Find condition priority
                       // index level of road
                       // procedure Flag is set

                       if (prioritLevel.OnAuthorizedPriorityProcedure)
                       {
                           SetSpeed(RobotSpeedLevel.ROBOT_SPEED_NORMAL);
                           // SetSpeed(RobotSpeedLevel.ROBOT_SPEED_STOP);
                           //TrafficBehaviorStateTracking = TrafficBehaviorState.MODE_FREE;
                           // robotModeFree = robot;
                           //  robotLogOut.ShowTextTraffic(this.properties.Label + " => STOP");
                       }
                       else
                       {

                           if (prioritLevel.IndexOnMainRoad < robot.prioritLevel.IndexOnMainRoad)
                           {
                               SetSpeed(RobotSpeedLevel.ROBOT_SPEED_STOP);
                               //  robotLogOut.ShowTextTraffic(this.properties.Label + " => STOP");
                           }
                           else
                           {
                               SetSpeed(RobotSpeedLevel.ROBOT_SPEED_STOP);
                              // SetSpeed(RobotSpeedLevel.ROBOT_SPEED_NORMAL);
                               // robotLogOut.ShowTextTraffic(this.properties.Label + " => STOP");
                           }
                       }

                       break;
                   case TrafficBehaviorState.HEADER_TOUCH_TAIL:
                       SetSpeed(RobotSpeedLevel.ROBOT_SPEED_STOP);
                       //TrafficBehaviorStateTracking = TrafficBehaviorState.MODE_FREE;
                       //robotModeFree = robot;
                    // robotLogOut.ShowTextTraffic(this.properties.Label+ " => STOP");
                       // robot stop
                       break;
                   case TrafficBehaviorState.HEADER_TOUCH_SIDE:
                       SetSpeed(RobotSpeedLevel.ROBOT_SPEED_STOP);
                       //TrafficBehaviorStateTracking = TrafficBehaviorState.MODE_FREE;
                      // robotModeFree = robot;
                 //   robotLogOut.ShowTextTraffic(this.properties.Label+ " => STOP");
                       break;

               }
           }*/
        public void TurnOnSupervisorTraffic(bool onflagtraffic)
        {
            onFlagSupervisorTraffic = onflagtraffic;
            SetSafeSmallcircle(onflagtraffic);
            if (!onflagtraffic)
            {
                properties.L1 = 0;
                properties.L2 = 0;
                properties.WS = 0;
                properties.DistInter = 0;


                L1Cv = 0;
                L2Cv = 0;
                WSCv = 0;
                DistInterCv = 0;

                Radius_S = 0;
                Radius_B = 0;
                Radius_Y = 0;
                onFlagSafeSmallcircle = false;

            }
        }
        public void TurnOnCtrlSelfTraffic(bool _onflagSelftraffic)
        {
            this.onFlagSelfTraffic = _onflagSelftraffic;
        }
        public void SetTrafficAtCheckIn(bool onset) // khi robot tai check in
        {
            /* if (onset)
             {
                 onFlagSupervisorTraffic = false;
                 UpdateRiskAraParams(0, DfL2, DfWS, DfDistanceInter);
             }*/
            /* else
             {
                 onFlagSupervisorTraffic = true;
                 UpdateRiskAraParams(DfL1, DfL2, DfWS, DfDistanceInter);
             }*/

        }
        public void ReDrawRobotGrapphic()
        {

        }
        public void TrafficUpdate()
        {
            while (true)
            {
                try
                {
                    prioritLevel.IndexOnMainRoad = trafficManagementService.FindIndexZoneRegister(properties.pose.Position);
                    if (onFlagSupervisorTraffic)
                    {

                        // cập nhật vùng riskzone // update vùng risk area cho robot
                        ZoneRegister rZR = trafficManagementService.Find(properties.pose.Position, 0, 200);
                        if (rZR != null)
                        {
                            properties.L1 = rZR.L1;
                            properties.L2 = rZR.L2;
                            properties.WS = rZR.WS;
                            properties.DistInter = rZR.distance;


                            L1Cv = rZR.L1 * properties.Scale;
                            L2Cv = rZR.L2 * properties.Scale;
                            WSCv = rZR.WS * properties.Scale;
                            DistInterCv = rZR.distance * properties.Scale;

                            


                            //UpdateRiskAraParams(rZR.L1, rZR.L2, rZR.WS, rZR.distance);
                        }
                        else
                        {
                            UpdateRiskAraParams(DfL1, DfL2, DfWS, DfDistanceInter);
                        }
                        //SupervisorTraffic();
                       // SetSpeed(RobotSpeedLevel.ROBOT_SPEED_NORMAL);
                         RobotBehavior();
                    }
                    // giám sát an toàn

                }
                catch { Console.WriteLine("TrafficRobotUnity Error in TrafficUpdate"); }
                Thread.Sleep(500);
            }

        }
        /* protected override void SupervisorTraffic()
         {
             if (onFlagSelfTraffic)
             {
                 int numMode = CheckSafeDistance();
                 if (numMode == 0)
                 {
                     if (RobotUnityRiskList.Count > 0)
                     {
                         RobotUnityRiskList.Clear();
                     }
                     TrafficBehaviorStateTracking = TrafficBehaviorState.HEADER_TOUCH_NOTOUCH;
                     TrafficBehavior(null);
                 }
                 else if(numMode==1)
                 {
                     SetSpeed(RobotSpeedLevel.ROBOT_SPEED_SLOW);
                    // robotLogOut.ShowTextTraffic("Slow Motion");
                 }
                 else if (numMode==2)
                 {
                     RobotUnity robot = CheckIntersection(true);
                    // if (robot != null)
                    // {
                    //     DetectTouchedPosition(robot);
                    //     TrafficBehavior(robot);
                    // }
                 }

             }
             else
             {
                 SetSpeed(RobotSpeedLevel.ROBOT_SPEED_NORMAL);
             }
         }*/

        // Finding has any Robot in Zone that Robot is going to come
        public bool FindRobotInWorkingZone(Point anyPoint)
        {
            bool hasRobot = false;
            String nameZone = trafficManagementService.DetermineArea(anyPoint, 0, 200);
            if (nameZone != "")
            {
                foreach (RobotUnity r in RobotUnitylist)
                {
                    if (r.robotRegistryToWorkingZone.WorkingZone.Equals(nameZone))
                    {
                        hasRobot = true;
                        break;
                    }
                }
            }
            return hasRobot;
        }
        public RobotUnity DetermineRobotInWorkingZone(Point anyPoint)
        {
            RobotUnity robot = null;
            String nameZone = trafficManagementService.DetermineArea(anyPoint, 0, 200);
            if (nameZone != "")
            {
                foreach (RobotUnity r in RobotUnitylist)
                {
                    if (r.robotRegistryToWorkingZone.WorkingZone.Equals(nameZone))
                    {
                        robot = r;
                        break;
                    }
                }
            }
            return robot;
        }
        // set zonename Robot will working
        public void SetWorkingZone(String nameZone)
        {
            robotRegistryToWorkingZone.SetZone(nameZone);
        }
        // release zonename Robot out
        public void ReleaseWorkingZone()
        {
            robotRegistryToWorkingZone.Release();
        }
        // ứng xử tai check in zone với bắt vị trí anypoint
        public bool CheckInZoneBehavior(Point anyPoint)
        {
            if (anyPoint == null)
                return true; // un available
            if (FindRobotInWorkingZone(anyPoint))
                return true;
            else
            {
                String nameZone = trafficManagementService.DetermineArea(anyPoint, 0, 200);
                if (nameZone != "")
                {
                    SetWorkingZone(nameZone);
                    return false; // available
                }
                return true;
            }
        }
        public bool CheckInGateFromReadyZoneBehavior(Point anyPoint)
        {
            if (anyPoint == null)
                return true; // un available
            RobotUnity robot = DetermineRobotInWorkingZone(anyPoint);
            if (robot != null)
            {
                return true;

            }
            else
            {
                String nameZone = trafficManagementService.DetermineArea(anyPoint, 0, 200);
                if (nameZone != "")
                {
                    SetWorkingZone(nameZone);
                    return false; // available
                }

            }
            return true;
        }
        public bool CheckRobotWorkinginReady()
        {
            bool hasRobotWorking = false;
            foreach (RobotUnity robot in RobotUnitylist)
            {
                if (trafficManagementService.GetTypeZone(robot.properties.pose.Position, 0, 200) == TypeZone.READY)
                {
                    if (robot.robotTag == RobotStatus.WORKING)
                    {
                        hasRobotWorking = true;
                        break;
                    }
                }
            }
            return hasRobotWorking;
        }
        // kiểm tra vong tròn an toàn quyết định điều khiển giao thông theo nguyên tắt
        // + vòng tròn nhỏ an toàn được mở on trong trường hợp robot nằm trên đường chính
        // + vòng tròn xanh tượng trưng xin vào đường chính từ đường nhỏ giao với đường chính, nếu có robot nào xuất hiện trong vòng tròn nhỏ robot vẽ vòng tròn xnh phải đứng lại ưu tiên cho robot khác làm việc
        // + vòng tròn vàng mức ưu tiên cai nhất, nó xuất hiện và được vẻ ra khi robot trong khu vực đường lớn giao và đang làm nhiệm vụ dò line. robot nào xu61t hiện trong vòng tròn này buột phải ngưng mọi hoạt động

        public void RobotBehavior()
        {
            RobotBahaviorAtAnyPlace robotBahaviorAtAnyPlace = RobotBahaviorAtAnyPlace.ROBOT_PLACE_IDLE;
            TypeZone _type = trafficManagementService.GetTypeZone(properties.pose.Position, 0, 200);
            //onFlagDetectLine = true;
            if(_type== TypeZone.READY)
            {
                //SetSpeed(RobotSpeedLevel.ROBOT_SPEED_NORMAL);
            }
            if (_type == TypeZone.HIGHWAY && onFlagDetectLine == false)
            {
                robotBahaviorAtAnyPlace = RobotBahaviorAtAnyPlace.ROBOT_PLACE_HIGHWAY;
            }
            if (_type == TypeZone.HIGHWAY && onFlagDetectLine == true)
            {
                robotBahaviorAtAnyPlace = RobotBahaviorAtAnyPlace.ROBOT_PLACE_HIGHWAY_DETECTLINE; ;
            }
            if (_type == TypeZone.ROAD)
            {
                
                robotBahaviorAtAnyPlace = RobotBahaviorAtAnyPlace.ROBOT_PLACE_ROAD;
            }
            if (_type == TypeZone.BUFFER)
            {
                robotBahaviorAtAnyPlace = RobotBahaviorAtAnyPlace.ROBOT_PLACE_BUFFER;
            }
            switch (robotBahaviorAtAnyPlace)
            {
                case RobotBahaviorAtAnyPlace.ROBOT_PLACE_IDLE:
                    //  SetSpeed(RobotSpeedLevel.ROBOT_SPEED_STOP);
                    SetSafeSmallcircle(true);
                    break;
                case RobotBahaviorAtAnyPlace.ROBOT_PLACE_HIGHWAY:
                    /*SetSafeSmallcircle(true);
                    SetSafeBluecircle(false);
                    SetSafeYellowcircle(false);*/
                    if (CheckYellowCircle())
                    {
                        SetSpeed(RobotSpeedLevel.ROBOT_SPEED_STOP);
                    }
                    else
                    {
                        // mở vòng tròn nhỏ vá kiểm tra va chạm
                        CheckIntersection(true);
                    }
                    break;
                case RobotBahaviorAtAnyPlace.ROBOT_PLACE_ROAD:
                    // kiem tra vong tròn xanh
                    SetSafeSmallcircle(true);
                    SetSafeBluecircle(true);
                    CheckBlueCircle();
                    CheckYellowCircle();
                    break;
                case RobotBahaviorAtAnyPlace.ROBOT_PLACE_HIGHWAY_DETECTLINE:
                   // SetSafeSmallcircle(true);
                    SetSafeBluecircle(false);
                    SetSafeYellowcircle(true);
                    SetSpeed(RobotSpeedLevel.ROBOT_SPEED_NORMAL);
                    break;
                case RobotBahaviorAtAnyPlace.ROBOT_PLACE_BUFFER:
                    SetSafeSmallcircle(false);
                    SetSafeBluecircle(false);
                    SetSafeYellowcircle(false);
                    CheckIntersection(false);
                    // tắt vòng tròn nhỏ
                    break;
            }
        }
        public void CheckBlueCircle() // khi robot bặt vòng tròn xanh. chính nó phải ngưng nếu dò ra có robot nào trong vùng vòng tròn này ngược lại với vòng tròn vàng
        {
            foreach (RobotUnity r in RobotUnitylist)
            {
                if (r.prioritLevel.IndexOnMainRoad >= prioritLevel.IndexOnMainRoad)
                {
                    // va chạm vòng tròn an toàn nhỏ ra quyết định ngưng robot
                    CheckIntersection(true);
                }

                else
                {
                    // kiểm tra có robot nào nằm trong vòng tròn và trạng thái đang là việc an toàn này kg?
                    Point cB = CenterOnLineCv(Center_B);
                    if(r.robotTag==RobotStatus.WORKING)
                    {
                        if (FindHeaderInsideCircleArea(r.MiddleHeaderCv(), cB, Radius_B))
                        {
                            SetSpeed(RobotSpeedLevel.ROBOT_SPEED_STOP);
                            break;
                        }
                        else
                        {
                            SetSpeed(RobotSpeedLevel.ROBOT_SPEED_NORMAL);
                        }

                    }
                    else
                    {
                            SetSpeed(RobotSpeedLevel.ROBOT_SPEED_NORMAL);
                    }
                }
            }
        }
        public bool CheckYellowCircle() // khi robot bặt vòng tròn vàng. tất cả robot khác ngưng nếu dò ra có robot nào trong vùng vòng tròn này
        {
            bool flagInsideYellowCircle = false;
            foreach (RobotUnity r in RobotUnitylist)
            {
                // kiểm tra có robot chinh  nó có nằm trong vòng tròn vàng nào không nếu có ngưng
                if (r.onFlagSafeYellowcircle)
                {
                    Point cY = CenterOnLineCv(Center_Y);
                    if (r.FindHeaderInsideCircleArea(MiddleHeaderCv(), cY, Radius_Y))
                    {
                        SetSpeed(RobotSpeedLevel.ROBOT_SPEED_STOP);
                        flagInsideYellowCircle = true;
                        break;
                    }
                }
            }
            return flagInsideYellowCircle;
        }
        public void SetSafeYellowcircle(bool flagonoff)
        {
            if (flagonoff)
                Radius_Y = 40;
            else
                Radius_Y = 0;
            onFlagSafeYellowcircle = flagonoff;
        }
        public void SetSafeBluecircle(bool flagonoff)
        {
            if (flagonoff)
                Radius_B = 40;
            else
                Radius_B = 0;
            onFlagSafeBluecircle = flagonoff;
        }
        public void SetSafeSmallcircle(bool flagonoff)
        {
            if (flagonoff)
                Radius_S = 40;
            else
                Radius_S = 0;
            onFlagSafeSmallcircle = flagonoff;
        }
        public void SwitchToDetectLine(bool flagonoff)
        {
            onFlagDetectLine = flagonoff;
        }
    }
}
