using System;
using System.Diagnostics;
using System.Threading;
using SeldatMRMS.Management.RobotManagent;
using SeldatMRMS.Management.TrafficManager;
using static SeldatMRMS.Management.RobotManagent.RobotBaseService;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;
using static SeldatMRMS.Management.TrafficRobotUnity;
using static SelDatUnilever_Ver1._00.Management.DeviceManagement.DeviceItem;

namespace SeldatMRMS
{
    public class ProcedureMachineToReturn : ProcedureControlServices
    {
        public struct DataMachineToReturn
        {
            // public Pose PointFrontLineMachine;
            // public PointDetect PointPickPallet;
            // public Pose PointCheckInReturn;
            // public Pose PointFrontLineReturn;
            // public PointDetect PointDropPallet;
        }
        DataMachineToReturn points;
        MachineToReturn StateMachineToReturn;
        Thread ProMachineToReturn;
        public RobotUnity robot;
        ResponseCommand resCmd;
        TrafficManagementService Traffic;
        public override event Action<Object> ReleaseProcedureHandler;
        // public override event Action<Object> ErrorProcedureHandler;
        public ProcedureMachineToReturn(RobotUnity robot, TrafficManagementService traffiicService) : base(robot)
        {
            StateMachineToReturn = MachineToReturn.MACRET_IDLE;
            this.robot = robot;
            this.points = new DataMachineToReturn();
            this.Traffic = traffiicService;
            procedureCode = ProcedureCode.PROC_CODE_MACHINE_TO_RETURN;
        }

        public void Start(MachineToReturn state = MachineToReturn.MACRET_ROBOT_GOTO_FRONTLINE_MACHINE)
        {
            robot.orderItem = null;
            errorCode = ErrorCode.RUN_OK;
            robot.robotTag = RobotStatus.WORKING;
            robot.ProcedureAs = ProcedureControlAssign.PRO_MACHINE_TO_RETURN;
            StateMachineToReturn = state;
            ProMachineToReturn = new Thread(this.Procedure);
            ProMachineToReturn.Start(this);
            ProRun = true;
            robot.prioritLevel.OnAuthorizedPriorityProcedure = false;
            order.startTimeProcedure = DateTime.Now;
        }
        public void Destroy()
        {
            robot.orderItem = null;
            robot.robotTag = RobotStatus.IDLE;
            // StateMachineToReturn = MachineToReturn.MACRET_ROBOT_RELEASED;
            robot.prioritLevel.OnAuthorizedPriorityProcedure = false;
            ProRun = false;
            UpdateInformationInProc(this, ProcessStatus.F);
            order.status = StatusOrderResponseCode.ROBOT_ERROR;
            selectHandleError = SelectHandleError.CASE_ERROR_EXIT;
            order.endTimeProcedure = DateTime.Now;
            order.totalTimeProcedure = order.endTimeProcedure.Subtract(order.startTimeProcedure).TotalSeconds;
            SaveOrderItem(order);
            //   this.robot.DestroyRegistrySolvedForm();
        }
        public void Procedure(object ojb)
        {
            ProcedureMachineToReturn BfToRe = (ProcedureMachineToReturn)ojb;
            RobotUnity rb = BfToRe.robot;
            DataMachineToReturn p = BfToRe.points;
            TrafficManagementService Traffic = BfToRe.Traffic;
            robot.ShowText(" Start -> " + procedureCode);
            while (ProRun)
            {
                switch (StateMachineToReturn)
                {
                    case MachineToReturn.MACRET_IDLE:
                        break;
                    case MachineToReturn.MACRET_ROBOT_GOTO_FRONTLINE_MACHINE: // doi khu vuc buffer san sang de di vao
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
                                            if (rb.SendPoseStamped(BfToRe.GetFrontLineMachine()))
                                            {
                                                StateMachineToReturn = MachineToReturn.MACRET_ROBOT_WAITTING_CAME_FRONTLINE_MACHINE;
                                                robot.ShowText("MACRET_ROBOT_WAITTING_CAME_FRONTLINE_MACHINE");
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
                                if (rb.SendPoseStamped(BfToRe.GetFrontLineMachine()))
                                {
                                    StateMachineToReturn = MachineToReturn.MACRET_ROBOT_WAITTING_CAME_FRONTLINE_MACHINE;
                                    robot.ShowText("MACRET_ROBOT_WAITTING_CAME_FRONTLINE_MACHINE");
                                }
                            }
                        }
                        catch (System.Exception)
                        {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            CheckUserHandleError(this);
                        }
                        break;
                    case MachineToReturn.MACRET_ROBOT_WAITTING_CAME_FRONTLINE_MACHINE:
                        try
                        {
                            if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT)
                            //if (robot.ReachedGoal())
                            {
                                resCmd = ResponseCommand.RESPONSE_NONE;
                                if (rb.SendCmdAreaPallet(BfToRe.GetInfoOfPalletMachine(PistonPalletCtrl.PISTON_PALLET_UP)))
                                {
                                    // rb.SendCmdLineDetectionCtrl(RequestCommandLineDetect.REQUEST_LINEDETECT_PALLETUP);
                                    rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                                    StateMachineToReturn = MachineToReturn.MACRET_ROBOT_WAITTING_PICKUP_PALLET_MACHINE;
                                    robot.ShowText("MACRET_ROBOT_WAITTING_PICKUP_PALLET_MACHINE");
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
                    // case MachineToReturn.MACRET_ROBOT_GOTO_PICKUP_PALLET_MACHINE:
                    //     if (true == rb.CheckPointDetectLine(BfToRe.GetPointPallet(), rb))
                    //     {
                    //         rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_LINEDETECT_COMING_POSITION);
                    //         StateMachineToReturn = MachineToReturn.MACRET_ROBOT_WAITTING_PICKUP_PALLET_MACHINE;
                    //     }
                    //     break;
                    case MachineToReturn.MACRET_ROBOT_WAITTING_PICKUP_PALLET_MACHINE:
                        if (resCmd == ResponseCommand.RESPONSE_LINEDETECT_PALLETUP)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            BfToRe.UpdatePalletState(PalletStatus.F);
                            StateMachineToReturn = MachineToReturn.MACRET_ROBOT_WAITTING_GOBACK_FRONTLINE_MACHINE;
                            robot.ShowText("MACRET_ROBOT_WAITTING_GOBACK_FRONTLINE_MACHINE");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case MachineToReturn.MACRET_ROBOT_WAITTING_GOBACK_FRONTLINE_MACHINE: // đợi
                        try
                        {
                            if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE)
                            {
                                resCmd = ResponseCommand.RESPONSE_NONE;
                                rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                                if (rb.SendPoseStamped(BfToRe.GetCheckInReturn()))
                                {
                                    StateMachineToReturn = MachineToReturn.MACRET_ROBOT_GOTO_CHECKIN_RETURN;
                                    robot.ShowText("MACRET_ROBOT_GOTO_CHECKIN_RETURN");
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
                    case MachineToReturn.MACRET_ROBOT_GOTO_CHECKIN_RETURN: // dang di
                        if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT)
                        //if (robot.ReachedGoal())
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                            rb.UpdateRiskAraParams(0, rb.properties.L2, rb.properties.WS, rb.properties.DistInter);
                            StateMachineToReturn = MachineToReturn.MACRET_ROBOT_CAME_CHECKIN_RETURN;
                            robot.ShowText("MACRET_ROBOT_CAME_CHECKIN_RETURN");
                        }
                        break;
                    case MachineToReturn.MACRET_ROBOT_CAME_CHECKIN_RETURN: // đã đến vị trí
                        try
                        {
                            if (false == robot.CheckInZoneBehavior(BfToRe.GetFrontLineReturn().Position))
                            {
                                rb.UpdateRiskAraParams(40, rb.properties.L2, rb.properties.WS, rb.properties.DistInter);
                                rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                                if (rb.SendPoseStamped(BfToRe.GetFrontLineReturn()))
                                {
                                    StateMachineToReturn = MachineToReturn.MACRET_ROBOT_GOTO_FRONTLINE_RETURN;
                                    robot.ShowText("MACRET_ROBOT_GOTO_FRONTLINE_RETURN");
                                }
                            }
                        }
                        catch (System.Exception)
                        {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            CheckUserHandleError(this);
                        }
                        break;
                    case MachineToReturn.MACRET_ROBOT_GOTO_FRONTLINE_RETURN: // dang di
                        try
                        {
                            if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT)
                            //if ( robot.ReachedGoal())
                            {
                                resCmd = ResponseCommand.RESPONSE_NONE;

                                if (rb.SendCmdAreaPallet(BfToRe.GetInfoOfPalletReturn(PistonPalletCtrl.PISTON_PALLET_DOWN)))
                                {
                                    rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                                    StateMachineToReturn = MachineToReturn.MACRET_ROBOT_WAITTING_DROPDOWN_PALLET;
                                    robot.ShowText("MACRET_ROBOT_WAITTING_DROPDOWN_PALLET");
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
                    // case MachineToReturn.MACRET_ROBOT_CAME_CHECKIN_RETURN: // đã đến vị trí
                    //     if (false == Traffic.HasRobotUnityinArea(BfToRe.GetFrontLineReturn().Position))
                    //     {
                    //         rb.SendPoseStamped(BfToRe.GetFrontLineReturn());
                    //         StateMachineToReturn = MachineToReturn.MACRET_ROBOT_GOTO_FRONTLINE_DROPDOWN_PALLET;
                    //     }
                    //     break;
                    // case MachineToReturn.MACRET_ROBOT_GOTO_FRONTLINE_DROPDOWN_PALLET:
                    //     if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT)
                    //     {
                    //         resCmd = ResponseCommand.RESPONSE_NONE;
                    //         StateMachineToReturn = MachineToReturn.MACRET_ROBOT_CAME_FRONTLINE_DROPDOWN_PALLET;
                    //     }
                    //     break;
                    // case MachineToReturn.MACRET_ROBOT_CAME_FRONTLINE_DROPDOWN_PALLET:  // đang trong tiến trình dò line và thả pallet
                    //     rb.SendCmdLineDetectionCtrl(RequestCommandLineDetect.REQUEST_LINEDETECT_PALLETDOWN);
                    //     StateMachineToReturn = MachineToReturn.MACRET_ROBOT_WAITTING_GOTO_POINT_DROP_PALLET;
                    //     break;
                    // case MachineToReturn.MACRET_ROBOT_WAITTING_GOTO_POINT_DROP_PALLET:
                    //     if (true == rb.CheckPointDetectLine(BfToRe.GetPointPallet(), rb))
                    //     {
                    //         rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_LINEDETECT_COMING_POSITION);
                    //         StateMachineToReturn = MachineToReturn.MACRET_ROBOT_WAITTING_DROPDOWN_PALLET;
                    //     }
                    //     break;
                    case MachineToReturn.MACRET_ROBOT_WAITTING_DROPDOWN_PALLET:
                        if (resCmd == ResponseCommand.RESPONSE_LINEDETECT_PALLETDOWN)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            BfToRe.UpdatePalletState(PalletStatus.W);
                            StateMachineToReturn = MachineToReturn.MACRET_ROBOT_WAITTING_GOTO_FRONTLINE;
                            robot.ShowText("MACRET_ROBOT_WAITTING_GOTO_FRONTLINE");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case MachineToReturn.MACRET_ROBOT_WAITTING_GOTO_FRONTLINE:
                        if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                            StateMachineToReturn = MachineToReturn.MACRET_ROBOT_RELEASED;
                            robot.ShowText("MACRET_ROBOT_RELEASED");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case MachineToReturn.MACRET_ROBOT_RELEASED: // trả robot về robotmanagement để nhận quy trình mới
                        robot.orderItem = null;
                        robot.robotTag = RobotStatus.IDLE;
                        rb.PreProcedureAs = ProcedureControlAssign.PRO_MACHINE_TO_RETURN;
                        // if (errorCode == ErrorCode.RUN_OK) {
                        ReleaseProcedureHandler(this);
                        // } else {
                        //     ErrorProcedureHandler (this);
                        // }
                        ProRun = false;
                        robot.ShowText("RELEASED");
                        UpdateInformationInProc(this, ProcessStatus.S);
                        order.status = StatusOrderResponseCode.FINISHED;
                        order.endTimeProcedure = DateTime.Now;
                        order.totalTimeProcedure = order.endTimeProcedure.Subtract(order.startTimeProcedure).TotalSeconds; SaveOrderItem(order);
                        break;
                    default:
                        break;
                }
                Thread.Sleep(5);
            }
            StateMachineToReturn = MachineToReturn.MACRET_IDLE;
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