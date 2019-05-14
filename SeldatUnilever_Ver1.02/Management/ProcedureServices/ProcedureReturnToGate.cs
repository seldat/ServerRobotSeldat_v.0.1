using System;
using System.Diagnostics;
using System.Threading;
using DoorControllerService;
using SeldatMRMS.Management.DoorServices;
using SeldatMRMS.Management.RobotManagent;
using SeldatMRMS.Management.TrafficManager;
using static DoorControllerService.DoorService;
using static SeldatMRMS.Management.RobotManagent.RobotBaseService;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;
using static SeldatMRMS.Management.TrafficRobotUnity;

namespace SeldatMRMS
{

    public class ProcedureReturnToGate : ProcedureControlServices
    {
        public struct DataReturnToGate
        {
            // public Pose PointCheckInReturn;
            // public Pose PointFrontLineReturn;
            // public Pose PointFrontLineGate;
            // public PointDetect PointPickPallet;
            // public Pose PointCheckInGate;
            // public Pose PointOfGate;
            // public PointDetect PointDropPallet;
        }
        // DataReturnToGate points;
        ReturnToGate StateReturnToGate;
        Thread ProReturnToGate;
        public RobotUnity robot;
        public DoorManagementService door;
        ResponseCommand resCmd;
        TrafficManagementService Traffic;

        public override event Action<Object> ReleaseProcedureHandler;
        // public override event Action<Object> ErrorProcedureHandler;
        public ProcedureReturnToGate(RobotUnity robot, DoorManagementService doorservice, TrafficManagementService traffiicService) : base(robot)
        {
            StateReturnToGate = ReturnToGate.RETGATE_IDLE;
            resCmd = ResponseCommand.RESPONSE_NONE;
            this.robot = robot;
            base.robot = robot;
            // this.points = new DataReturnToGate();
            this.door = doorservice;
            this.Traffic = traffiicService;
            errorCode = ErrorCode.RUN_OK;
            procedureCode = ProcedureCode.PROC_CODE_RETURN_TO_GATE;
        }
        public void Start(ReturnToGate state = ReturnToGate.RETGATE_ROBOT_WAITTING_GOTO_CHECKIN_RETURN)
        {
            errorCode = ErrorCode.RUN_OK;
            robot.robotTag = RobotStatus.WORKING;
            robot.ProcedureAs = ProcedureControlAssign.PRO_RETURN_TO_GATE;
            StateReturnToGate = state;
            ProReturnToGate = new Thread(this.Procedure);
            ProReturnToGate.Start(this);
            ProRun = true;
            robot.prioritLevel.OnAuthorizedPriorityProcedure = false;
            order.startTimeProcedure = DateTime.Now;
        }
        public void Destroy()
        {
            // StateReturnToGate = ReturnToGate.RETGATE_ROBOT_RELEASED;
            robot.robotTag = RobotStatus.IDLE;
            robot.prioritLevel.OnAuthorizedPriorityProcedure = false;
            ProRun = false;
            UpdateInformationInProc(this, ProcessStatus.F);
            order.endTimeProcedure = DateTime.Now;
            order.totalTimeProcedure = order.endTimeProcedure.Subtract(order.startTimeProcedure).TotalSeconds;
            SaveOrderItem(order);
        }
        public void Procedure(object ojb)
        {
            ProcedureReturnToGate ReToGate = (ProcedureReturnToGate)ojb;
            RobotUnity rb = ReToGate.robot;
            // DataReturnToGate p = ReToGate.points;
            DoorService ds = ReToGate.door.DoorMezzamineReturn;
            TrafficManagementService Traffic = ReToGate.Traffic;
            robot.ShowText(" Start -> " + procedureCode);
            while (ProRun)
            {
                switch (StateReturnToGate)
                {
                    case ReturnToGate.RETGATE_IDLE:
                        break;
                    case ReturnToGate.RETGATE_ROBOT_WAITTING_GOTO_CHECKIN_RETURN: // doi robot di den khu vuc checkin cua vung buffer
                        try
                        {
                            if (rb.PreProcedureAs == ProcedureControlAssign.PRO_READY)
                            {
                                if (rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_GOBACK_FRONTLINE_TURN_RIGHT))
                                {
                                    Stopwatch sw = new Stopwatch();
                                    sw.Start();
                                    do
                                    {
                                        if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE)
                                        {
                                            resCmd = ResponseCommand.RESPONSE_NONE;
                                            if (rb.SendPoseStamped(ReToGate.GetCheckInBuffer()))
                                            {
                                                StateReturnToGate = ReturnToGate.RETGATE_ROBOT_WAITTING_ZONE_RETURN_READY;
                                                robot.ShowText("RETGATE_ROBOT_WAITTING_ZONE_RETURN_READY");
                                            }
                                            break;
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
                                    } while (ProRun);
                                    sw.Stop();
                                }
                            }
                            else
                            {
                                if (rb.SendPoseStamped(ReToGate.GetCheckInBuffer()))
                                {
                                    StateReturnToGate = ReturnToGate.RETGATE_ROBOT_WAITTING_ZONE_RETURN_READY;
                                    robot.ShowText("RETGATE_ROBOT_WAITTING_ZONE_RETURN_READY");
                                }
                            }
                        }
                        catch (System.Exception)
                        {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ReturnToGate.RETGATE_ROBOT_WAITTING_ZONE_RETURN_READY: // doi khu vuc buffer san sang de di vao
                        try
                        {
                            if (false == robot.CheckInZoneBehavior(ReToGate.GetFrontLineReturn().Position))
                            {
                                if (rb.SendPoseStamped(ReToGate.GetFrontLineReturn()))
                                {
                                    StateReturnToGate = ReturnToGate.RETGATE_ROBOT_WAITTING_CAME_FRONTLINE_RETURN;
                                    robot.ShowText("RETGATE_ROBOT_WAITTING_CAME_FRONTLINE_RETURN");
                                }
                            }
                        }
                        catch (System.Exception)
                        {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ReturnToGate.RETGATE_ROBOT_WAITTING_CAME_FRONTLINE_RETURN:
                        try
                        {
                            if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT)
                            {
                                resCmd = ResponseCommand.RESPONSE_NONE;
                                if (rb.SendCmdAreaPallet(ReToGate.GetInfoOfPalletReturn(PistonPalletCtrl.PISTON_PALLET_UP)))
                                {
                                    // rb.SendCmdLineDetectionCtrl(RequestCommandLineDetect.REQUEST_LINEDETECT_PALLETUP);
                                    rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                                    StateReturnToGate = ReturnToGate.RETGATE_ROBOT_WAITTING_PICKUP_PALLET_RETURN;
                                    robot.ShowText("RETGATE_ROBOT_WAITTING_PICKUP_PALLET_RETURN");
                                }
                            }
                            else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                            {
                                errorCode = ErrorCode.DETECT_LINE_ERROR;
                                CheckUserHandleError(this);
                            }
                        }
                        catch (System.Exception)
                        {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            CheckUserHandleError(this);
                        }
                        break;
                    // case ReturnToGate.RETGATE_ROBOT_GOTO_PICKUP_PALLET_RETURN:
                    //     if (true == rb.CheckPointDetectLine(ReToGate.GetPointPallet(), rb))
                    //     {
                    //         rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_LINEDETECT_COMING_POSITION);
                    //         StateReturnToGate = ReturnToGate.RETGATE_ROBOT_WAITTING_PICKUP_PALLET_RETURN;
                    //     }
                    //     break;
                    case ReturnToGate.RETGATE_ROBOT_WAITTING_PICKUP_PALLET_RETURN:
                        if (resCmd == ResponseCommand.RESPONSE_LINEDETECT_PALLETUP)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            ReToGate.UpdatePalletState(PalletStatus.F);
                            StateReturnToGate = ReturnToGate.RETGATE_ROBOT_WAITTING_GOBACK_FRONTLINE_RETURN;
                            robot.ShowText("RETGATE_ROBOT_WAITTING_GOBACK_FRONTLINE_RETURN");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ReturnToGate.RETGATE_ROBOT_WAITTING_GOBACK_FRONTLINE_RETURN: // đợi
                        if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                            if (rb.SendPoseStamped(ds.config.PointCheckInGate))
                            {
                                StateReturnToGate = ReturnToGate.RETGATE_ROBOT_WAITTING_GOTO_CHECKIN_GATE;
                                robot.ShowText("RETGATE_ROBOT_WAITTING_GOTO_CHECKIN_GATE");
                            }
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    // case ReturnToGate.RETGATE_ROBOT_GOTO_CHECKIN_GATE: //gui toa do di den khu vuc checkin cong
                    //     rb.SendPoseStamped(ds.config.PointCheckInGate);
                    //     StateReturnToGate = ReturnToGate.RETGATE_ROBOT_WAITTING_GOTO_CHECKIN_GATE;
                    //     break;
                    case ReturnToGate.RETGATE_ROBOT_WAITTING_GOTO_CHECKIN_GATE:
                        if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                            StateReturnToGate = ReturnToGate.RETGATE_ROBOT_CAME_CHECKIN_GATE;
                            robot.ShowText("RETGATE_ROBOT_CAME_CHECKIN_GATE");
                        }
                        break;
                    case ReturnToGate.RETGATE_ROBOT_CAME_CHECKIN_GATE: // đã đến vị trí, kiem tra va cho khu vuc cong san sang de di vao.
                        if (false == robot.CheckInZoneBehavior(ds.config.PointFrontLine.Position))
                        {
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                            if (rb.SendPoseStamped(ds.config.PointFrontLine))
                            {
                                StateReturnToGate = ReturnToGate.RETGATE_ROBOT_WAITTING_GOTO_GATE;
                                robot.ShowText("RETGATE_ROBOT_WAITTING_GOTO_GATE");
                            }
                        }
                        break;
                    case ReturnToGate.RETGATE_ROBOT_WAITTING_GOTO_GATE:
                        if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                            StateReturnToGate = ReturnToGate.RETGATE_ROBOT_CAME_GATE_POSITION;
                            robot.ShowText("RETGATE_ROBOT_CAME_GATE_POSITION");
                        }
                        break;
                    case ReturnToGate.RETGATE_ROBOT_CAME_GATE_POSITION: // da den khu vuc cong , gui yeu cau mo cong.
                        ds.openDoor(DoorService.DoorType.DOOR_BACK);
                        StateReturnToGate = ReturnToGate.RETGATE_ROBOT_WAITTING_OPEN_DOOR;
                        robot.ShowText("RETGATE_ROBOT_WAITTING_OPEN_DOOR");
                        break;
                    case ReturnToGate.RETGATE_ROBOT_WAITTING_OPEN_DOOR: //doi mo cong
                        RetState ret = ds.checkOpen(DoorService.DoorType.DOOR_BACK);
                        if (ret == RetState.DOOR_CTRL_SUCCESS)
                        {
                            if (rb.SendCmdAreaPallet(ds.config.infoPallet))
                            {
                                StateReturnToGate = ReturnToGate.RETGATE_ROBOT_WAITTING_DROPDOWN_PALLET_RETURN;
                                robot.ShowText("RETGATE_ROBOT_WAITTING_DROPDOWN_PALLET_RETURN");
                            }
                        }
                        else if (ret == RetState.DOOR_CTRL_ERROR)
                        {
                            StateReturnToGate = ReturnToGate.RETGATE_ROBOT_CAME_GATE_POSITION;
                        }
                        break;
                    // case ReturnToGate.RETGATE_ROBOT_OPEN_DOOR_SUCCESS: // mo cua thang cong ,gui toa do line de robot di vao
                    //     rb.SendCmdLineDetectionCtrl(RequestCommandLineDetect.REQUEST_LINEDETECT_PALLETDOWN);
                    //     StateReturnToGate = ReturnToGate.RETGATE_ROBOT_GOTO_POSITION_PALLET_RETURN;
                    //     break;
                    // case ReturnToGate.RETGATE_ROBOT_GOTO_POSITION_PALLET_RETURN:
                    //     if (true == rb.CheckPointDetectLine(ds.config.PointOfPallet, rb))
                    //     {
                    //         rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_LINEDETECT_COMING_POSITION);
                    //         StateReturnToGate = ReturnToGate.RETGATE_ROBOT_WAITTING_DROPDOWN_PALLET_RETURN;
                    //     }
                    //     break;
                    case ReturnToGate.RETGATE_ROBOT_WAITTING_DROPDOWN_PALLET_RETURN: // doi robot gap hang
                        if (resCmd == ResponseCommand.RESPONSE_LINEDETECT_PALLETDOWN)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            // ReToGate.UpdatePalletState(PalletStatus.W);
                            StateReturnToGate = ReturnToGate.RETGATE_ROBOT_WAITTING_GOBACK_FRONTLINE_GATE;
                            robot.ShowText("RETGATE_ROBOT_WAITTING_GOBACK_FRONTLINE_GATE");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ReturnToGate.RETGATE_ROBOT_WAITTING_GOBACK_FRONTLINE_GATE:
                        if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            ds.closeDoor(DoorService.DoorType.DOOR_BACK);
                            StateReturnToGate = ReturnToGate.RETGATE_ROBOT_WAITTING_CLOSE_GATE;
                            robot.ShowText("RETGATE_ROBOT_WAITTING_CLOSE_GATE");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ReturnToGate.RETGATE_ROBOT_WAITTING_CLOSE_GATE: // doi dong cong.
                        //if (true == ds.WaitClose(DoorService.DoorType.DOOR_BACK, TIME_OUT_CLOSE_DOOR))
                        //{
                        StateReturnToGate = ReturnToGate.RETGATE_ROBOT_RELEASED;
                        rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                        robot.ShowText("RETGATE_ROBOT_WAITTING_CLOSE_GATE");
                        //}
                        //else
                        //{
                        //    errorCode = ErrorCode.CLOSE_DOOR_ERROR;
                        //    CheckUserHandleError(this);
                        //}
                        break;

                    case ReturnToGate.RETGATE_ROBOT_RELEASED: // trả robot về robotmanagement để nhận quy trình mới
                        robot.robotTag = RobotStatus.IDLE;
                        rb.PreProcedureAs = ProcedureControlAssign.PRO_RETURN_TO_GATE;
                        // if (errorCode == ErrorCode.RUN_OK) {
                        ReleaseProcedureHandler(this);
                        // } else {
                        // ErrorProcedureHandler (this);
                        // }
                        ProRun = false;
                        robot.ShowText("RELEASED");
                        UpdateInformationInProc(this, ProcessStatus.S);
                        order.endTimeProcedure = DateTime.Now;
                        order.totalTimeProcedure = order.endTimeProcedure.Subtract(order.startTimeProcedure).TotalSeconds;
                        SaveOrderItem(order);
                        break;
                    default:
                        break;
                }
                Thread.Sleep(5);
            }
            StateReturnToGate = ReturnToGate.RETGATE_IDLE;
        }

        public override void FinishStatesCallBack(Int32 message)
        {
            this.resCmd = (ResponseCommand)message;
            if (this.resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE)
            {
                robot.ReleaseWorkingZone();
            }
        }
    }
}