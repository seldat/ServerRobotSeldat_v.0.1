using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SeldatMRMS.Management.RobotManagent;
using SeldatMRMS.Management.TrafficManager;
using SelDatUnilever_Ver1._00.Management.DeviceManagement;
using static SeldatMRMS.Management.RobotManagent.RobotBaseService;
using static SeldatMRMS.Management.RobotManagent.RobotUnity;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;
using static SeldatMRMS.Management.TrafficRobotUnity;
using static SelDatUnilever_Ver1._00.Management.DeviceManagement.DeviceItem;

namespace SeldatMRMS
{
    public class ProcedureBufferToMachine : ProcedureControlServices
    {
        public class DataBufferToMachine
        {
            // public Pose PointCheckInBuffer;
            // public Pose PointFrontLineBuffer;
            // public PointDetectBranching PointDetectLineBranching;
            // public PointDetect PointPickPallet;
            // public Pose PointFrontLineMachine;
            // public PointDetect PointDropPallet;
        }
        // DataBufferToMachine points;
        BufferToMachine StateBufferToMachine;

        public RobotUnity robot;
        bool onUpdatedPalletState = false;
        ResponseCommand resCmd;
        TrafficManagementService Traffic;
        private DeviceRegistrationService deviceService;
        public void Registry(DeviceRegistrationService deviceService)
        {
            this.deviceService = deviceService;
        }
        public override event Action<Object> ReleaseProcedureHandler;
        // public override event Action<Object> ErrorProcedureHandler;
        public ProcedureBufferToMachine(RobotUnity robot, TrafficManagementService trafficService) : base(robot)
        {
            StateBufferToMachine = BufferToMachine.BUFMAC_IDLE;
            this.robot = robot;
            // this.points = new DataBufferToMachine();
            this.Traffic = trafficService;
            procedureCode = ProcedureCode.PROC_CODE_BUFFER_TO_MACHINE;
        }

        public void Start(BufferToMachine state = BufferToMachine.BUFMAC_ROBOT_GOTO_CHECKIN_BUFFER)
        {
            errorCode = ErrorCode.RUN_OK;
            robot.robotTag = RobotStatus.WORKING;
            robot.ProcedureAs = ProcedureControlAssign.PRO_BUFFER_TO_MACHINE;
            StateBufferToMachine = state;
            Task ProBuferToMachine = new Task(() => this.Procedure(this));
            ProBuferToMachine.Start();
            procedureStatus = ProcedureStatus.PROC_ALIVE;
            ProRun = true;
            robot.prioritLevel.OnAuthorizedPriorityProcedure = false;

        }
        public void Destroy()
        {
            robot.robotTag = RobotStatus.IDLE;
            StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_DESTROY;
        }

        public void RestoreOrderItem()
        {
            OrderItem _order = new OrderItem();
            _order.activeDate = order.activeDate;
            _order.bufferId = order.bufferId;

            dynamic product = new JObject();
            product.timeWorkId = order.timeWorkId;
            product.activeDate = order.activeDate;
            order.dateTime = (string)DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt");
            product.productId = order.productId;
            product.productDetailId = order.productDetailId;
            // chu y sua 
            product.palletStatus = PalletStatus.W.ToString(); // W

            _order.dataRequest = product.ToString();
            _order.dateTime = order.dateTime;
            _order.deviceId = order.deviceId;
            _order.palletAtMachine = order.palletAtMachine;
            _order.palletId = order.palletId;
            _order.palletStatus = order.palletStatus;
            _order.planId = order.planId;
            _order.productDetailId = order.productDetailId;
            _order.productDetailName = order.productDetailName;
            _order.productId = order.productId;
            _order.robot = "";
            _order.typeReq = order.typeReq;
            _order.updUsrId = order.updUsrId;
            _order.userName = order.userName;
            _order.lengthPallet = order.lengthPallet;
            _order.palletAmount = order.palletAmount;
            _order.bufferId = order.bufferId;
            _order.status = StatusOrderResponseCode.PENDING;

            deviceService.FindDeviceItem(_order.userName).AddOrder(_order);
        }
        public void Procedure(object ojb)
        {
            ProcedureBufferToMachine BfToMa = (ProcedureBufferToMachine)ojb;
            RobotUnity rb = BfToMa.robot;
            // DataBufferToMachine p = BfToMa.points;
            TrafficManagementService Traffic = BfToMa.Traffic;
            robot.ShowText(" Start -> " + procedureCode);
            //GetFrontLineBuffer(false);
            //  ProRun = false;
            while (ProRun)
            {
                switch (StateBufferToMachine)
                {
                    case BufferToMachine.BUFMAC_IDLE:
                        robot.ShowText("BUFMAC_IDLE");
                        break;
                    case BufferToMachine.BUFMAC_ROBOT_GOTO_CHECKIN_BUFFER: // bắt đầu rời khỏi vùng GATE đi đến check in/ đảm bảo check out vùng cổng để robot kế tiếp vào làm việc
                        robot.ShowText("BUFMAC_ROBOT_GOTO_CHECKIN_BUFFER");
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
                                            if (rb.SendPoseStamped(BfToMa.GetCheckInBuffer()))
                                            {
                                                StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_WAITTING_GOTO_CHECKIN_BUFFER;
                                                robot.ShowText("BUFMAC_ROBOT_WAITTING_GOTO_CHECKIN_BUFFER");
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
                                    } while (true);
                                    sw.Stop();
                                }
                            }
                            else
                            {
                                if (rb.SendPoseStamped(BfToMa.GetCheckInBuffer()))
                                {
                                    StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_WAITTING_GOTO_CHECKIN_BUFFER;
                                    robot.ShowText("BUFMAC_ROBOT_WAITTING_GOTO_CHECKIN_BUFFER");
                                }
                            }
                        }
                        catch (System.Exception)
                        {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;

                            CheckUserHandleError(this);
                        }
                        break;
                    case BufferToMachine.BUFMAC_ROBOT_WAITTING_GOTO_CHECKIN_BUFFER: // doi robot di den khu vuc checkin cua vung buffer
                        //if (rb.checkNewPci())
                        //{
                        //    bool onComePoint = robot.ReachedGoal(rb.getPointCheckInConfirm());
                        if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT)
                        //if (onComePoint)
                        {
                            robot.SetTrafficAtCheckIn(true);
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            //rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                            StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_WAITTING_ZONE_BUFFER_READY;
                            robot.ShowText("BUFMAC_ROBOT_WAITTING_ZONE_BUFFER_READY");
                        }
                        //}
                        break;
                    case BufferToMachine.BUFMAC_ROBOT_WAITTING_ZONE_BUFFER_READY: // doi khu vuc buffer san sang de di vao
                        try
                        {
                            if (false == robot.CheckInZoneBehavior(BfToMa.GetAnyPointInBuffer().Position))
                            {
                                robot.SetTrafficAtCheckIn(false);
                                //rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                                if (rb.SendPoseStamped(BfToMa.GetFrontLineBuffer()))
                                {
                                    StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_WAITTING_CAME_FRONTLINE_BUFFER;
                                    robot.ShowText("BUFMAC_ROBOT_WAITTING_CAME_FRONTLINE_BUFFER");
                                }
                            }
                        }
                        catch (System.Exception)
                        {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            CheckUserHandleError(this);
                        }
                        break;
                    case BufferToMachine.BUFMAC_ROBOT_WAITTING_CAME_FRONTLINE_BUFFER:
                        try
                        {
                            //bool onComePoint2 = robot.ReachedGoal();
                            if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT)
                            //if (onComePoint2)
                            {
                                // 

                                robot.SwitchToDetectLine(true);
                                robot.TurnOnCtrlSelfTraffic(false);
                                resCmd = ResponseCommand.RESPONSE_NONE;
                                rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                                if (rb.SendCmdAreaPallet(BfToMa.GetInfoOfPalletBuffer(PistonPalletCtrl.PISTON_PALLET_UP)))
                                {
                                    StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_WAITTING_PICKUP_PALLET_BUFFER;
                                    robot.ShowText("BUFMAC_ROBOT_WAITTING_PICKUP_PALLET_BUFFER");
                                }
                            }
                        }
                        catch (System.Exception)
                        {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            CheckUserHandleError(this);
                        }
                        break;
                    // case BufferToMachine.BUFMAC_ROBOT_WAITTING_GOTO_POINT_BRANCHING:
                    //     if (true == rb.CheckPointDetectLine(BfToMa.GetPointDetectBranching().xy, rb))
                    //     {
                    //         if (BfToMa.GetPointDetectBranching().brDir == BrDirection.DIR_LEFT)
                    //         {
                    //             rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_TURN_LEFT);
                    //         }
                    //         else if (BfToMa.GetPointDetectBranching().brDir == BrDirection.DIR_RIGHT)
                    //         {
                    //             rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_TURN_RIGHT);
                    //         }
                    //         StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_WAITTING_GOTO_POINT_BRANCHING;
                    //     }
                    //     break;
                    // case BufferToMachine.BUFMAC_ROBOT_CAME_POINT_BRANCHING:  //doi bobot re
                    //     if ((resCmd == ResponseCommand.RESPONSE_FINISH_TURN_LEFT) || (resCmd == ResponseCommand.RESPONSE_FINISH_TURN_RIGHT))
                    //     {
                    //         resCmd = ResponseCommand.RESPONSE_NONE;
                    //         rb.SendCmdLineDetectionCtrl(RequestCommandLineDetect.REQUEST_LINEDETECT_PALLETUP);
                    //         StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_GOTO_PICKUP_PALLET_BUFFER;
                    //     }
                    //     break;
                    // case BufferToMachine.BUFMAC_ROBOT_GOTO_PICKUP_PALLET_BUFFER:
                    //     if (true == rb.CheckPointDetectLine(BfToMa.GetPointPallet(), rb))
                    //     {
                    //         rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_LINEDETECT_COMING_POSITION);
                    //         StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_WAITTING_PICKUP_PALLET_BUFFER;
                    //     }
                    //     break;
                    case BufferToMachine.BUFMAC_ROBOT_WAITTING_PICKUP_PALLET_BUFFER:
                        if (resCmd == ResponseCommand.RESPONSE_LINEDETECT_PALLETUP)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            BfToMa.UpdatePalletState(PalletStatus.F);
                            onUpdatedPalletState = true;
                            StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_WAITTING_GOBACK_FRONTLINE_BUFFER;
                            robot.ShowText("BUFMAC_ROBOT_WAITTING_GOBACK_FRONTLINE_BUFFER");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case BufferToMachine.BUFMAC_ROBOT_WAITTING_GOBACK_FRONTLINE_BUFFER: // đợi
                        try
                        {
                            if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE)
                            {
                                robot.SwitchToDetectLine(false);
                                robot.TurnOnCtrlSelfTraffic(true);
                                resCmd = ResponseCommand.RESPONSE_NONE;
                                rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                                if (rb.SendPoseStamped(BfToMa.GetFrontLineMachine()))
                                {
                                    StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_GOTO_FRONTLINE_DROPDOWN_PALLET;
                                    robot.ShowText("BUFMAC_ROBOT_GOTO_FRONTLINE_DROPDOWN_PALLET");
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
                    case BufferToMachine.BUFMAC_ROBOT_GOTO_FRONTLINE_DROPDOWN_PALLET:
                        try
                        {
                            //bool onComePoint3 = robot.ReachedGoal();
                            if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT)
                            //if (onComePoint3)
                            {
                                robot.SwitchToDetectLine(true);
                                robot.TurnOnCtrlSelfTraffic(false);
                                if (rb.SendCmdAreaPallet(BfToMa.GetInfoOfPalletMachine(PistonPalletCtrl.PISTON_PALLET_DOWN)))
                                {
                                    rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                                    StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_WAITTING_DROPDOWN_PALLET;
                                    robot.ShowText("BUFMAC_ROBOT_WAITTING_DROPDOWN_PALLET");
                                }
                            }
                        }
                        catch (System.Exception)
                        {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            CheckUserHandleError(this);
                        }
                        break;
                    // case BufferToMachine.BUFMAC_ROBOT_CAME_FRONTLINE_DROPDOWN_PALLET:  // đang trong tiến trình dò line và thả pallet
                    //     rb.SendCmdLineDetectionCtrl(RequestCommandLineDetect.REQUEST_LINEDETECT_PALLETDOWN);
                    //     StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_WAITTING_GOTO_POINT_DROP_PALLET;
                    //     break;
                    // case BufferToMachine.BUFMAC_ROBOT_WAITTING_GOTO_POINT_DROP_PALLET:
                    //     if (true == rb.CheckPointDetectLine(BfToMa.GetPointPallet(), rb))
                    //     {
                    //         rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_LINEDETECT_COMING_POSITION);
                    //         StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_WAITTING_DROPDOWN_PALLET;
                    //     }
                    //     break;
                    case BufferToMachine.BUFMAC_ROBOT_WAITTING_DROPDOWN_PALLET:
                        if (resCmd == ResponseCommand.RESPONSE_LINEDETECT_PALLETDOWN)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_WAITTING_GOTO_FRONTLINE;
                            robot.ShowText("BUFMAC_ROBOT_WAITTING_GOTO_FRONTLINE");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case BufferToMachine.BUFMAC_ROBOT_WAITTING_GOTO_FRONTLINE:
                        if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE)
                        {
                            robot.SwitchToDetectLine(false);
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                            StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_RELEASED;
                            robot.ShowText("BUFMAC_ROBOT_RELEASED");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case BufferToMachine.BUFMAC_ROBOT_RELEASED: // trả robot về robotmanagement để nhận quy trình mới
                        robot.SwitchToDetectLine(false);
                        // Release WorkinZone Robot
                        robot.robotTag = RobotStatus.IDLE;
                        robot.ReleaseWorkingZone();
                        robot.TurnOnCtrlSelfTraffic(true);
                        rb.PreProcedureAs = ProcedureControlAssign.PRO_BUFFER_TO_MACHINE;
                        // if (errorCode == ErrorCode.RUN_OK) {
                        ReleaseProcedureHandler(this);
                        // } else {
                        //     ErrorProcedureHandler (this);
                        // }
                        ProRun = false;
                        robot.ShowText("RELEASED");
                        UpdateInformationInProc(this, ProcessStatus.S);
                        order.status = StatusOrderResponseCode.FINISHED;
                        break;
                    case BufferToMachine.BUFMAC_ROBOT_DESTROY:
                        robot.SwitchToDetectLine(false);
                        robot.ReleaseWorkingZone();
                        // StateBufferToMachine = BufferToMachine.BUFMAC_ROBOT_RELEASED;
                        robot.prioritLevel.OnAuthorizedPriorityProcedure = false;
                        ProRun = false;
                        UpdateInformationInProc(this, ProcessStatus.F);
                        order.status = StatusOrderResponseCode.ROBOT_ERROR;
                        //reset status pallet Faile H->Ws
                        if (!onUpdatedPalletState)
                            UpdatePalletState(PalletStatus.W);
                        selectHandleError = SelectHandleError.CASE_ERROR_EXIT;
                        procedureStatus = ProcedureStatus.PROC_KILLED;
                        FreeHoldBuffer();
                        //this.robot.DestroyRegistrySolvedForm();
                        break;
                    default:
                        break;
                }
                Thread.Sleep(5);
            }
            StateBufferToMachine = BufferToMachine.BUFMAC_IDLE;
        }
        public override void FinishStatesCallBack(Int32 message)
        {
            this.resCmd = (ResponseCommand)message;
        }
        protected override void CheckUserHandleError(object obj)
        {
            if (errorCode == ErrorCode.CAN_NOT_GET_DATA)
            {
                if (!this.Traffic.RobotIsInArea("READY", robot.properties.pose.Position))
                {
                    ProRun = false;
                    robot.setColorRobotStatus(RobotStatusColorCode.ROBOT_STATUS_CAN_NOTGET_DATA);
                    order.status = StatusOrderResponseCode.NO_BUFFER_DATA;
                    robot.TurnOnSupervisorTraffic(true);
                    robot.TurnOnCtrlSelfTraffic(true);
                    robot.PreProcedureAs = robot.ProcedureAs;
                    // reset pallet state

                    ReleaseProcedureHandler(obj);
                    return;
                }
                else
                {
                    ProRun = false;
                    robot.setColorRobotStatus(RobotStatusColorCode.ROBOT_STATUS_CAN_NOTGET_DATA);
                    order.status = StatusOrderResponseCode.NO_BUFFER_DATA;
                    robot.TurnOnSupervisorTraffic(true);
                    robot.TurnOnCtrlSelfTraffic(true);
                    return;
                }
            }
            base.CheckUserHandleError(obj);
        }
    }
}
