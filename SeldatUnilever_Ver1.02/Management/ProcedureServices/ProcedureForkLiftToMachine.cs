using DoorControllerService;
using SeldatMRMS;
using SeldatMRMS.Management.DoorServices;
using SeldatMRMS.Management.RobotManagent;
using SeldatMRMS.Management.TrafficManager;
using System;
using System.Diagnostics;
using System.Threading;
using static DoorControllerService.DoorService;
using static SeldatMRMS.Management.RobotManagent.RobotBaseService;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;
using static SeldatMRMS.Management.TrafficRobotUnity;
using static SelDatUnilever_Ver1._00.Management.DeviceManagement.DeviceItem;

namespace SeldatUnilever_Ver1._02.Management.ProcedureServices
{
    public class ProcedureForkLiftToMachine : ProcedureControlServices
    {
        ForkLiftToMachine StateForkLiftToMachine;
        Thread ProForkLiftToMachine;
        public RobotUnity robot;
        public DoorService door;
        ResponseCommand resCmd;
        TrafficManagementService Traffic;

        public override event Action<Object> ReleaseProcedureHandler;
        // public override event Action<Object> ErrorProcedureHandler;
        public ProcedureForkLiftToMachine(RobotUnity robot, DoorManagementService doorservice, TrafficManagementService traffiicService) : base(robot)
        {
            StateForkLiftToMachine = ForkLiftToMachine.FORMACH_IDLE;
            resCmd = ResponseCommand.RESPONSE_NONE;
            this.robot = robot;
            door = doorservice.DoorMezzamineUp;
            this.Traffic = traffiicService;
            procedureCode = ProcedureCode.PROC_CODE_FORKLIFT_TO_BUFFER;

        }
        public void Start(ForkLiftToMachine state = ForkLiftToMachine.FORMACH_ROBOT_GOTO_CHECKIN_GATE)
        {
            errorCode = ErrorCode.RUN_OK;
            robot.ProcedureAs = ProcedureControlAssign.PRO_FORKLIFT_TO_MACHINE;
            StateForkLiftToMachine = state;
            ProForkLiftToMachine = new Thread(this.Procedure);
            ProForkLiftToMachine.Start(this);
            ProRun = true;
            ProRunStopW = true;
            robot.prioritLevel.OnAuthorizedPriorityProcedure = false;
            robot.robotRegistryToWorkingZone.onRobotwillCheckInsideGate = true;
            order.startTimeProcedure = DateTime.Now;
        }
        public void Destroy()
        {
            ProRunStopW = false;
            robot.orderItem = null;
            robot.SwitchToDetectLine(false);
            robot.robotTag = RobotStatus.IDLE;
            robot.ReleaseWorkingZone();
            order.status = StatusOrderResponseCode.ROBOT_ERROR;
            robot.prioritLevel.OnAuthorizedPriorityProcedure = false;
            ProRun = false;
            UpdateInformationInProc(this, ProcessStatus.F);
            order.status = StatusOrderResponseCode.ROBOT_ERROR;
            selectHandleError = SelectHandleError.CASE_ERROR_EXIT;
            this.robot.DestroyRegistrySolvedForm();
            procedureStatus = ProcedureStatus.PROC_KILLED;
            order.endTimeProcedure = DateTime.Now;
            order.totalTimeProcedure = order.endTimeProcedure.Subtract(order.startTimeProcedure).TotalMinutes;
            SaveOrderItem(order);
            // RestoreOrderItem();
        }
        public void Procedure(object ojb)
        {
            ProcedureForkLiftToMachine FlToMach = (ProcedureForkLiftToMachine)ojb;
            RobotUnity rb = FlToMach.robot;
            DoorService ds = FlToMach.door;
            TrafficManagementService Traffic = FlToMach.Traffic;
            rb.mcuCtrl.TurnOnLampRb();
            robot.ShowText(" Start -> " + procedureCode);
            while (ProRun)
            {
                switch (StateForkLiftToMachine)
                {
                    case ForkLiftToMachine.FORMACH_IDLE:
                        robot.ShowText("FORMACH_IDLE");
                        break;
                    case ForkLiftToMachine.FORMACH_ROBOT_GOTO_CHECKIN_GATE: //gui toa do di den khu vuc checkin cong
                        if (rb.PreProcedureAs == ProcedureControlAssign.PRO_READY)
                        {
                            if (false == robot.CheckInGateFromReadyZoneBehavior(ds.config.PointFrontLine.Position))
                            {
                                robot.robotTag = RobotStatus.WORKING;
                                if (rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_GOBACK_FRONTLINE_TURN_LEFT))
                                {
                                    Stopwatch sw = new Stopwatch();
                                    sw.Start();
                                    do
                                    {
                                        if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE)
                                        {
                                            resCmd = ResponseCommand.RESPONSE_NONE;
                                            if (rb.SendPoseStamped(ds.config.PointFrontLine))
                                            {
                                                StateForkLiftToMachine = ForkLiftToMachine.FORMACH_ROBOT_WAITTING_GOTO_GATE;
                                                robot.ShowText("FORMACH_ROBOT_WAITTING_GOTO_GATE");
                                                break;
                                            }
                                           
                                        }
                                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                                        {
                                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                                            CheckUserHandleError(this);
                                            break;
                                        }
                                        if (sw.ElapsedMilliseconds > TIME_OUT_WAIT_GOTO_FRONTLINE)
                                        {
                                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                                            CheckUserHandleError(this);
                                            break;
                                        }
                                        Thread.Sleep(100);
                                    } while (ProRunStopW);
                                    sw.Stop();
                                }
                            }
                        }
                        else
                        {
                            robot.robotTag = RobotStatus.WORKING;
                            if (Traffic.RobotIsInArea("OPA4", rb.properties.pose.Position))
                            {
                                rb.SendPoseStamped(ds.config.PointFrontLine);
                                StateForkLiftToMachine = ForkLiftToMachine.FORMACH_ROBOT_WAITTING_GOTO_GATE;
                                robot.ShowText("FORMACH_ROBOT_CAME_CHECKIN_GATE");
                            }
                            else
                            {
                                rb.SendPoseStamped(ds.config.PointCheckInGate);
                                StateForkLiftToMachine = ForkLiftToMachine.FORMACH_ROBOT_WAITTING_GOTO_CHECKIN_GATE;
                                robot.ShowText("FORMACH_ROBOT_WAITTING_GOTO_CHECKIN_GATE");
                            }
                        }
                        break;
                    case ForkLiftToMachine.FORMACH_ROBOT_WAITTING_GOTO_CHECKIN_GATE:
                        if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT)
                        //if (robot.ReachedGoal())
                        {

                            
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                            rb.UpdateRiskAraParams(0, rb.properties.L2, rb.properties.WS, rb.properties.DistInter);
                            StateForkLiftToMachine = ForkLiftToMachine.FORMACH_ROBOT_CAME_CHECKIN_GATE;
                            robot.ShowText("FORMACH_ROBOT_CAME_CHECKIN_GATE");
                        }
                        break;
                    case ForkLiftToMachine.FORMACH_ROBOT_CAME_CHECKIN_GATE: // đã đến vị trí, kiem tra va cho khu vuc cong san sang de di vao.
                                                                            // robot.ShowText( "FORMACH_ROBOT_WAITTING_GOTO_GATE ===> FLAG " + Traffic.HasRobotUnityinArea(ds.config.PointFrontLine.Position));
                        if (false == robot.CheckInZoneBehavior(ds.config.PointFrontLine.Position))
                        {
                           
                            rb.UpdateRiskAraParams(40, rb.properties.L2, rb.properties.WS, rb.properties.DistInter);
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                            rb.SendPoseStamped(ds.config.PointFrontLine);
                            StateForkLiftToMachine = ForkLiftToMachine.FORMACH_ROBOT_WAITTING_GOTO_GATE;
                            robot.ShowText("FORMACH_ROBOT_WAITTING_GOTO_GATE");
                        }
                        break;
                    case ForkLiftToMachine.FORMACH_ROBOT_WAITTING_GOTO_GATE:
                        if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT)
                        //if (robot.ReachedGoal())
                        {
                            robot.SwitchToDetectLine(true);
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                            StateForkLiftToMachine = ForkLiftToMachine.FORMACH_ROBOT_CAME_GATE_POSITION;
                            robot.ShowText("FORMACH_ROBOT_CAME_GATE_POSITION");
                        }
                        break;
                    case ForkLiftToMachine.FORMACH_ROBOT_CAME_GATE_POSITION: // da den khu vuc cong , gui yeu cau mo cong.
                        robot.robotRegistryToWorkingZone.onRobotwillCheckInsideGate = false;
                        ds.openDoor(DoorService.DoorType.DOOR_BACK);
                        StateForkLiftToMachine = ForkLiftToMachine.FORMACH_ROBOT_WAITTING_OPEN_DOOR;
                        robot.ShowText("FORMACH_ROBOT_WAITTING_OPEN_DOOR");
                        break;
                    case ForkLiftToMachine.FORMACH_ROBOT_WAITTING_OPEN_DOOR: //doi mo cong
                        RetState ret = ds.checkOpen(DoorService.DoorType.DOOR_BACK);
                        if (ret == RetState.DOOR_CTRL_SUCCESS)
                        {
                            StateForkLiftToMachine = ForkLiftToMachine.FORMACH_ROBOT_OPEN_DOOR_SUCCESS;
                            robot.ShowText("FORMACH_ROBOT_OPEN_DOOR_SUCCESS");
                        }
                        else if (ret == RetState.DOOR_CTRL_ERROR)
                        {
                            robot.ShowText("FORMACH_ROBOT_OPEN_DOOR_ERROR");
                            StateForkLiftToMachine = ForkLiftToMachine.FORMACH_ROBOT_CAME_GATE_POSITION;
                            Thread.Sleep(50);
                        }
                        break;
                    case ForkLiftToMachine.FORMACH_ROBOT_OPEN_DOOR_SUCCESS: // mo cua thang cong ,gui toa do line de robot di vao gap hang
                        // rb.SendCmdLineDetectionCtrl(RequestCommandLineDetect.REQUEST_LINEDETECT_PALLETUP);
                        rb.SendCmdAreaPallet(ds.config.infoPallet);
                        StateForkLiftToMachine = ForkLiftToMachine.FORMACH_ROBOT_WAITTING_PICKUP_PALLET_IN;
                        robot.ShowText("FORMACH_ROBOT_WAITTING_PICKUP_PALLET_IN");
                        break;
                    case ForkLiftToMachine.FORMACH_ROBOT_WAITTING_PICKUP_PALLET_IN: // doi robot gap hang
                        if (resCmd == ResponseCommand.RESPONSE_LINEDETECT_PALLETUP)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            // FlToMach.UpdatePalletState(PalletStatus.F);
                            StateForkLiftToMachine = ForkLiftToMachine.FORMACH_ROBOT_WAITTING_GOBACK_FRONTLINE_GATE;
                            robot.ShowText("FORMACH_ROBOT_WAITTING_GOBACK_FRONTLINE_GATE");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ForkLiftToMachine.FORMACH_ROBOT_WAITTING_GOBACK_FRONTLINE_GATE:
                        if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE)
                        {
                            robot.SwitchToDetectLine(false);
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            ds.LampOff(DoorService.DoorType.DOOR_FRONT);
                            ds.closeDoor(DoorService.DoorType.DOOR_BACK);
                            StateForkLiftToMachine = ForkLiftToMachine.FORMACH_ROBOT_WAITTING_CLOSE_GATE;
                            robot.ShowText("FORMACH_ROBOT_WAITTING_CLOSE_GATE");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ForkLiftToMachine.FORMACH_ROBOT_WAITTING_CLOSE_GATE: // doi dong cong.
                        try
                        {
                            //if (true == ds.Close(DoorService.DoorType.DOOR_BACK))
                            //{
                       
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                            rb.SendPoseStamped(FlToMach.GetFrontLineMachine());
                            StateForkLiftToMachine = ForkLiftToMachine.FORMACH_ROBOT_WAITTING_CAME_FRONTLINE_MACHINE;
                            robot.ShowText("FORMACH_ROBOT_WAITTING_CAME_FRONTLINE_MACHINE");
                            //}
                            //else
                            //{
                            //    // errorCode = ErrorCode.CLOSE_DOOR_ERROR;
                            //    // CheckUserHandleError(this);
                            //}
                        }
                        catch (System.Exception)
                        {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            CheckUserHandleError(this);
                        }
                        break;

                    case ForkLiftToMachine.FORMACH_ROBOT_WAITTING_CAME_FRONTLINE_MACHINE:
                        try
                        {
                            if (!Traffic.HasRobotUnityinArea("GATE_CHECKOUT", robot))
                            {
                                Global_Object.onFlagRobotComingGateBusy = false;
                                robot.ReleaseWorkingZone();
                            }
                            if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT)
                            //if (robot.ReachedGoal())
                            {

                                //robot.TurnOnCtrlSelfTraffic(false);
                                //rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                                rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_DROPDOWN_PALLET);
                                StateForkLiftToMachine = ForkLiftToMachine.FORMACH_ROBOT_WAITTING_DROPDOWN_PALLET_MACHINE;
                                robot.ShowText("FORMACH_ROBOT_WAITTING_DROPDOWN_PALLET_MACHINE");
                            }
                        }
                        catch (System.Exception)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;

                    case ForkLiftToMachine.FORMACH_ROBOT_WAITTING_DROPDOWN_PALLET_MACHINE:
                        if (resCmd == ResponseCommand.RESPONSE_FINISH_DROPDOWN_PALLET)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            StateForkLiftToMachine = ForkLiftToMachine.FORMACH_ROBOT_RELEASED;
                            robot.ShowText("FORMACH_ROBOT_RELEASED");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    //case ForkLiftToMachine.FORMACH_ROBOT_WAITTING_GOBACK_FRONTLINE_MACHINE: // đợi
                    //    if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE)
                    //    {
                    //        resCmd = ResponseCommand.RESPONSE_NONE;
                    //        rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                    //        StateForkLiftToMachine = ForkLiftToMachine.FORMACH_ROBOT_RELEASED;
                    //        robot.ShowText("FORMACH_ROBOT_RELEASED");
                    //    }
                    //    else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                    //    {
                    //        errorCode = ErrorCode.DETECT_LINE_ERROR;
                    //        CheckUserHandleError(this);
                    //    }
                    //    break;
                    case ForkLiftToMachine.FORMACH_ROBOT_RELEASED: // trả robot về robotmanagement để nhận quy trình mới
                        robot.orderItem = null;
                        //Global_Object.onFlagDoorBusy = false;
                        robot.SwitchToDetectLine(false);
                        robot.robotTag = RobotStatus.IDLE;
                        robot.ReleaseWorkingZone();
                     
                        rb.PreProcedureAs = ProcedureControlAssign.PRO_FORKLIFT_TO_MACHINE;
                        order.status = StatusOrderResponseCode.FINISHED;
                        // if (errorCode == ErrorCode.RUN_OK) {
                        ReleaseProcedureHandler(this);
                        // } else {
                        //     ErrorProcedureHandler (this);
                        // }
                        ProRun = false;
                        robot.ShowText("RELEASED");
                        UpdateInformationInProc(this, ProcessStatus.S);
                        order.endTimeProcedure = DateTime.Now;
                        order.totalTimeProcedure = order.endTimeProcedure.Subtract(order.startTimeProcedure).TotalMinutes;
                        SaveOrderItem(order);
                        break;
                    default:
                        break;
                }
                Thread.Sleep(5);
            }
            StateForkLiftToMachine = ForkLiftToMachine.FORMACH_IDLE;
        }
        public override void FinishStatesCallBack(Int32 message)
        {
            this.resCmd = (ResponseCommand)message;
            /*if (this.resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE)
            {
                robot.ReleaseWorkingZone();
            }*/
        }
    }
}
