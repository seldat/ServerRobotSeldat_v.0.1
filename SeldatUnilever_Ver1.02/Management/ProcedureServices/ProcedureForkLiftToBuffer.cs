using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using DoorControllerService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SeldatMRMS.Management;
using SeldatMRMS.Management.DoorServices;
using SeldatMRMS.Management.RobotManagent;
using SeldatMRMS.Management.TrafficManager;
using SelDatUnilever_Ver1._00.Management.DeviceManagement;
using static DoorControllerService.DoorService;
using static SeldatMRMS.Management.RobotManagent.RobotBaseService;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;
using static SeldatMRMS.Management.TrafficRobotUnity;
using static SelDatUnilever_Ver1._00.Management.DeviceManagement.DeviceItem;

namespace SeldatMRMS
{

    public class ProcedureForkLiftToBuffer : ProcedureControlServices
    {
        public struct DataForkLiftToBuffer
        {
            // public Pose PointCheckInGate;
            // public Pose PointOfGate;
            // public Pose PointFrontLineGate;
            // public PointDetect PointPickPalletIn;
            // public Pose PointCheckInBuffer;
            // public Pose PointFrontLineBuffer;
            // public PointDetectBranching PointDetectLineBranching;
            // public PointDetect PointDropPallet;
        }
        // DataForkLiftToBuffer points;
        ForkLift StateForkLift;
        //  Thread ProForkLift;
        public RobotUnity robot;
        public DoorService door;
        ResponseCommand resCmd;
        TrafficManagementService Traffic;
        public bool onFlagResetedGate = false;
        private DeviceRegistrationService deviceService;

        public override event Action<Object> ReleaseProcedureHandler;

        public void Registry(DeviceRegistrationService deviceService)
        {
            this.deviceService = deviceService;
        }
        // public override event Action<Object> ErrorProcedureHandler;
        public ProcedureForkLiftToBuffer(RobotUnity robot, DoorManagementService doorservice, TrafficManagementService trafficService) : base(robot)
        {
            StateForkLift = ForkLift.FORBUF_IDLE;
            resCmd = ResponseCommand.RESPONSE_NONE;
            this.robot = robot;
            // this.points = new DataForkLiftToBuffer();
            door = doorservice.DoorMezzamineUp;
            // this.points.PointFrontLineGate = this.door.config.PointFrontLine;
            // this.points.PointPickPalletIn = this.door.config.PointOfPallet;
            this.Traffic = trafficService;
            procedureCode = ProcedureCode.PROC_CODE_FORKLIFT_TO_BUFFER;

        }
        public void Start(ForkLift state = ForkLift.FORBUF_ROBOT_GOTO_CHECKIN_GATE)
        {
            // public void Start (ForkLiftToBuffer state = ForkLiftToBuffer.FORBUF_ROBOT_RELEASED) {
            robot.robotTag = RobotStatus.WORKING;
            errorCode = ErrorCode.RUN_OK;
            robot.ProcedureAs = ProcedureControlAssign.PRO_FORKLIFT_TO_BUFFER;
            StateForkLift = state;

            Task ProForkLift = new Task(() => this.Procedure(this));
            procedureStatus = ProcedureStatus.PROC_ALIVE;
            ProForkLift.Start();
            ProRun = true;
            ProRunStopW = true;
            robot.prioritLevel.OnAuthorizedPriorityProcedure = false;
            robot.robotRegistryToWorkingZone.onRobotwillCheckInsideGate = true;
            order.startTimeProcedure = DateTime.Now;
        }
        public void Destroy()
        {
            Global_Object.onFlagDoorBusy = false;
            ProRunStopW = false;
            robot.orderItem = null;
            robot.robotTag = RobotStatus.IDLE;
            // StateForkLiftToBuffer = ForkLiftToBuffer.FORBUF_ROBOT_RELEASED;
            StateForkLift = ForkLift.FORMAC_ROBOT_DESTROY;


        }

        public void RestoreOrderItem()
        {


            OrderItem _order = new OrderItem();
            _order.activeDate = order.activeDate;
            _order.bufferId = order.bufferId;

            dynamic product = new JObject();
            product.timeWorkId = order.timeWorkId;
            product.activeDate = order.activeDate;
            product.productId = order.productId;
            product.productDetailId = order.productDetailId;
            product.palletStatus = PalletStatus.P.ToString();
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

            deviceService.FindDeviceItem(_order.userName).AddOrderCreatePlan(_order);
        }

        public void Procedure(object ojb)
        {
            ProcedureForkLiftToBuffer FlToBuf = (ProcedureForkLiftToBuffer)ojb;
            RobotUnity rb = FlToBuf.robot;
            // DataForkLiftToBuffer p = FlToBuf.points;
            DoorService ds = FlToBuf.door;
            TrafficManagementService Traffic = FlToBuf.Traffic;
            ForkLiftToMachineInfo flToMachineInfo = new ForkLiftToMachineInfo();
            rb.mcuCtrl.TurnOnLampRb();
            robot.ShowText(" Start -> " + procedureCode);
            //  StateForkLift = ForkLift.FORBUF_IDLE;
            while (ProRun)
            {
                switch (StateForkLift)
                {
                    case ForkLift.FORBUF_IDLE:
                        robot.ShowText("FORBUF_IDLE");
                        break;
                    case ForkLift.FORBUF_ROBOT_GOTO_CHECKIN_GATE: //gui toa do di den khu vuc checkin cong
                        if (rb.PreProcedureAs == ProcedureControlAssign.PRO_READY)
                        {
                            if (false == rb.CheckInGateFromReadyZoneBehavior(ds.config.PointFrontLine.Position))
                            {
                              
                                robot.ShowText("FORBUF_ROBOT_GOTO_BACK_FRONTLINE_READY");
                                StateForkLift = ForkLift.FORBUF_ROBOT_GOTO_BACK_FRONTLINE_READY;

                            }
                        }
                        else
                        {
                            robot.robotTag = RobotStatus.WORKING;
                            if (Traffic.RobotIsInArea("OPA4", rb.properties.pose.Position))
                            {
                                if (rb.SendPoseStamped(ds.config.PointFrontLine))
                                {
                                    StateForkLift = ForkLift.FORBUF_ROBOT_CAME_CHECKIN_GATE;
                                    robot.ShowText("FORBUF_ROBOT_CAME_CHECKIN_GATE");
                                }
                            }
                            else
                            {

                                if (rb.SendPoseStamped(ds.config.PointCheckInGate))
                                {
                                    StateForkLift = ForkLift.FORBUF_ROBOT_WAITTING_GOTO_CHECKIN_GATE;
                                    robot.ShowText("FORBUF_ROBOT_WAITTING_GOTO_CHECKIN_GATE");
                                }
                            }
                        }
                 
                        break;

                    case ForkLift.FORBUF_ROBOT_GOTO_BACK_FRONTLINE_READY:

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
                                        StateForkLift = ForkLift.FORBUF_ROBOT_WAITTING_GOTO_GATE;
                                        robot.ShowText("FORBUF_ROBOT_WAITTING_GOTO_GATE");
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
                        break;
                   case ForkLift.FORBUF_ROBOT_WAITTING_GOTO_CHECKIN_GATE:
                        if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT)
                        //if ( robot.ReachedGoal())
                        {
                         
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                            rb.UpdateRiskAraParams(0, rb.properties.L2, rb.properties.WS, rb.properties.DistInter);
                            StateForkLift = ForkLift.FORBUF_ROBOT_CAME_CHECKIN_GATE;
                            robot.ShowText("FORBUF_ROBOT_CAME_CHECKIN_GATE");
                        }
                        break;
                    case ForkLift.FORBUF_ROBOT_CAME_CHECKIN_GATE: // đã đến vị trí, kiem tra va cho khu vuc cong san sang de di vao.
                                                                  // robot.ShowText( "FORBUF_ROBOT_WAITTING_GOTO_GATE ===> FLAG " + Traffic.HasRobotUnityinArea(ds.config.PointFrontLine.Position));
                        if (false == robot.CheckInZoneBehavior(ds.config.PointFrontLine.Position))
                        {
                            
                            rb.UpdateRiskAraParams(4, rb.properties.L2, rb.properties.WS, rb.properties.DistInter);
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                            if (rb.SendPoseStamped(ds.config.PointFrontLine))
                            {
                                StateForkLift = ForkLift.FORBUF_ROBOT_WAITTING_GOTO_GATE;
                                robot.ShowText("FORBUF_ROBOT_WAITTING_GOTO_GATE");
                            }
                        }
                        break;
                    case ForkLift.FORBUF_ROBOT_WAITTING_GOTO_GATE:
                        if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT)
                        //if ( robot.ReachedGoal())
                        {
                            robot.SwitchToDetectLine(true);
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                            StateForkLift = ForkLift.FORBUF_ROBOT_CAME_GATE_POSITION;
                            robot.ShowText("FORBUF_ROBOT_CAME_GATE_POSITION");
                        }
                        break;
                    case ForkLift.FORBUF_ROBOT_CAME_GATE_POSITION: // da den khu vuc cong , gui yeu cau mo cong.
                        robot.robotRegistryToWorkingZone.onRobotwillCheckInsideGate = false;
                        ds.openDoor(DoorService.DoorType.DOOR_BACK);

                        StateForkLift = ForkLift.FORBUF_ROBOT_WAITTING_OPEN_DOOR;
                        robot.ShowText("FORBUF_ROBOT_WAITTING_OPEN_DOOR");
                        break;
                    case ForkLift.FORBUF_ROBOT_WAITTING_OPEN_DOOR: //doi mo cong
                        RetState ret = ds.checkOpen(DoorService.DoorType.DOOR_BACK);
                        if (RetState.DOOR_CTRL_SUCCESS == ret)
                        {
                            StateForkLift = ForkLift.FORBUF_ROBOT_OPEN_DOOR_SUCCESS;
                            robot.ShowText("FORBUF_ROBOT_OPEN_DOOR_SUCCESS");
                        }
                        else if (RetState.DOOR_CTRL_ERROR == ret)
                        {
                            robot.ShowText("FORBUF_ROBOT_OPEN_DOOR_ERROR");
                            StateForkLift = ForkLift.FORBUF_ROBOT_CAME_GATE_POSITION;
                            Thread.Sleep(50);
                        }
                        break;
                    case ForkLift.FORBUF_ROBOT_OPEN_DOOR_SUCCESS: // mo cua thang cong ,gui toa do line de robot di vao gap hang
                        // rb.SendCmdLineDetectionCtrl(RequestCommandLineDetect.REQUEST_LINEDETECT_PALLETUP);
                        if (rb.SendCmdAreaPallet(ds.config.infoPallet))
                        {
                            StateForkLift = ForkLift.FORBUF_ROBOT_WAITTING_PICKUP_PALLET_IN;
                            robot.ShowText("FORBUF_ROBOT_WAITTING_PICKUP_PALLET_IN");
                        }
                        break;
                    case ForkLift.FORBUF_ROBOT_WAITTING_PICKUP_PALLET_IN: // doi robot gap hang
                        if (resCmd == ResponseCommand.RESPONSE_LINEDETECT_PALLETUP)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            // FlToBuf.UpdatePalletState(PalletStatus.F);
                            //   rb.SendCmdPosPallet (RequestCommandPosPallet.REQUEST_GOBACK_FRONTLINE);
                            StateForkLift = ForkLift.FORBUF_ROBOT_WAITTING_GOBACK_FRONTLINE_GATE;
                            robot.ShowText("FORBUF_ROBOT_WAITTING_GOBACK_FRONTLINE_GATE");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ForkLift.FORBUF_ROBOT_WAITTING_GOBACK_FRONTLINE_GATE:
                        if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            ds.LampOff(DoorService.DoorType.DOOR_FRONT);
                            ds.closeDoor(DoorService.DoorType.DOOR_BACK);

                            StateForkLift = ForkLift.FORBUF_ROBOT_WAITTING_CLOSE_GATE;
                            robot.ShowText("FORBUF_ROBOT_WAITTING_CLOSE_GATE");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ForkLift.FORBUF_ROBOT_WAITTING_CLOSE_GATE: 
                        try
                        {
                            //if (true == ds.Close(DoorService.DoorType.DOOR_BACK))
                            //{

                            robot.SwitchToDetectLine(false);
                            flToMachineInfo = GetPriorityTaskForkLiftToMachine(order.productId);
                            if (flToMachineInfo == null)
                            {
                                rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                                try

                                {
                                    if (rb.SendPoseStamped(FlToBuf.GetCheckInBuffer(true)))

                                    {
                                        Global_Object.onFlagDoorBusy = false;
                                        StateForkLift = ForkLift.FORBUF_ROBOT_WAITTING_GOTO_CHECKIN_BUFFER;
                                        onFlagResetedGate = false;
                                        robot.ShowText("FORBUF_ROBOT_WAITTING_GOTO_CHECKIN_BUFFER");
                                    }
                                    else
                                    {
                                    }
                                }
                                catch
                                {
                                    Console.WriteLine("Error at rb.SendPoseStamped(FlToBuf.GetCheckInBuffer(true)); ");
                                }
                            }
                            else
                            {

                                FreePlanedBuffer();
                                StateForkLift = ForkLift.FORMAC_ROBOT_GOTO_FRONTLINE_MACHINE;
                                robot.ShowText("FORMAC_ROBOT_GOTO_FRONTLINE_MACHINE");
                            }
                            //}
                            //else
                            //{
                            //   // StateForkLift = ForkLift.FORBUF_ROBOT_WAITTING_GOBACK_FRONTLINE_GATE;
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
                    case ForkLift.FORBUF_ROBOT_WAITTING_GOTO_CHECKIN_BUFFER: // doi robot di den khu vuc checkin cua vung buffer
                        //Global_Object.onFlagDoorBusy = false;
                        if (!Traffic.HasRobotUnityinArea("GATE_CHECKOUT", robot))
                        {
                            //robot.ShowText("RELEASED ZONE");
                            if (!onFlagResetedGate)
                            {
                                Global_Object.onFlagRobotComingGateBusy = false;
                                robot.ReleaseWorkingZone();
                                onFlagResetedGate = true;
                            }
                        }
                        if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT)
                        //if (rb.checkNewPci())
                        //{
                        //    if (robot.ReachedGoal(rb.getPointCheckInConfirm()))
                        {
                           
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                            StateForkLift = ForkLift.FORBUF_ROBOT_WAITTING_ZONE_BUFFER_READY;
                            robot.ShowText("FORBUF_ROBOT_WAITTING_ZONE_BUFFER_READY");
                        }
                        //}
                        break;
                    case ForkLift.FORBUF_ROBOT_WAITTING_ZONE_BUFFER_READY: // doi khu vuc buffer san sang de di vao
                        try
                        {
                            if (false == robot.CheckInZoneBehavior(FlToBuf.GetAnyPointInBuffer(true).Position))
                            {
                               
                                // createPlanBuffer();
                                rb.prioritLevel.OnAuthorizedPriorityProcedure = false;

                                if (rb.SendPoseStamped(FlToBuf.GetFrontLineBuffer(true)))
                                {
                                    StateForkLift = ForkLift.FORBUF_ROBOT_WAITTING_CAME_FRONTLINE_BUFFER;
                                    robot.ShowText("FORBUF_ROBOT_WAITTING_CAME_FRONTLINE_BUFFER");
                                }
                            }
                        }
                        catch (System.Exception)
                        {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ForkLift.FORBUF_ROBOT_WAITTING_CAME_FRONTLINE_BUFFER:
                        try
                        {
                            if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT)
                            //if (robot.ReachedGoal())
                            {
                                robot.SwitchToDetectLine(true);
                                resCmd = ResponseCommand.RESPONSE_NONE;
                                if (rb.SendCmdAreaPallet(FlToBuf.GetInfoOfPalletBuffer(PistonPalletCtrl.PISTON_PALLET_DOWN, true)))
                                {
                                    // rb.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_FORWARD_DIRECTION);
                                    rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                                    StateForkLift = ForkLift.FORBUF_ROBOT_WAITTING_DROPDOWN_PALLET_BUFFER;
                                    robot.ShowText("FORBUF_ROBOT_WAITTING_DROPDOWN_PALLET_BUFFER");
                                }
                            }
                        }
                        catch (System.Exception)
                        {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            CheckUserHandleError(this);
                        }
                        break;

                    case ForkLift.FORBUF_ROBOT_WAITTING_DROPDOWN_PALLET_BUFFER:
                        if (resCmd == ResponseCommand.RESPONSE_LINEDETECT_PALLETDOWN)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            FlToBuf.UpdatePalletState(PalletStatus.W);
                            //   rb.SendCmdPosPallet (RequestCommandPosPallet.REQUEST_GOBACK_FRONTLINE);
                            StateForkLift = ForkLift.FORBUF_ROBOT_WAITTING_GOBACK_FRONTLINE_BUFFER;
                            robot.ShowText("FORBUF_ROBOT_WAITTING_GOBACK_FRONTLINE_BUFFER");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ForkLift.FORBUF_ROBOT_WAITTING_GOBACK_FRONTLINE_BUFFER: // đợi
                        if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE)
                        {
                            robot.ReleaseWorkingZone();
                            robot.SwitchToDetectLine(false);
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                            StateForkLift = ForkLift.FORBUF_ROBOT_RELEASED;
                            robot.ShowText("FORBUF_ROBOT_RELEASED");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ForkLift.FORBUF_ROBOT_RELEASED: // trả robot về robotmanagement để nhận quy trình mới
                        rb.PreProcedureAs = ProcedureControlAssign.PRO_FORKLIFT_TO_BUFFER;
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


                    ///////////////////////////////////////////////////////
                    case ForkLift.FORMAC_ROBOT_GOTO_FRONTLINE_MACHINE:
                        try
                        {
                       
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                            if (rb.SendPoseStamped(flToMachineInfo.frontLinePose))
                            {
                                Global_Object.onFlagDoorBusy = false;
                                StateForkLift = ForkLift.FORMAC_ROBOT_WAITTING_CAME_FRONTLINE_MACHINE;
                                robot.ShowText("FORMAC_ROBOT_WAITTING_CAME_FRONTLINE_MACHINE");
                                onFlagResetedGate = false;
                            }
                        }
                        catch (System.Exception)
                        {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            CheckUserHandleError(this);
                        }
                        break;

                    case ForkLift.FORMAC_ROBOT_WAITTING_CAME_FRONTLINE_MACHINE:
                        try
                        {
                           // Global_Object.onFlagDoorBusy = false;
                            if (!Traffic.HasRobotUnityinArea("GATE_CHECKOUT", robot))
                            {
                                if (!onFlagResetedGate)
                                {
                                    onFlagResetedGate = true;
                                    Global_Object.onFlagRobotComingGateBusy = false;
                                    robot.ReleaseWorkingZone();
                                }
                            }
                            if (resCmd == ResponseCommand.RESPONSE_LASER_CAME_POINT)
                            //if (robot.ReachedGoal())
                            {
                                robot.SwitchToDetectLine(true);
                           
                                if (rb.SendCmdAreaPallet(flToMachineInfo.infoPallet))
                                {
                                    rb.prioritLevel.OnAuthorizedPriorityProcedure = true;
                                    StateForkLift = ForkLift.FORMAC_ROBOT_WAITTING_DROPDOWN_PALLET_MACHINE;
                                    robot.ShowText("FORMAC_ROBOT_WAITTING_DROPDOWN_PALLET_MACHINE");
                                }
                            }
                        }
                        catch (System.Exception)
                        {
                            errorCode = ErrorCode.CAN_NOT_GET_DATA;
                            CheckUserHandleError(this);
                        }
                        break;

                    case ForkLift.FORMAC_ROBOT_WAITTING_DROPDOWN_PALLET_MACHINE:
                        if (resCmd == ResponseCommand.RESPONSE_LINEDETECT_PALLETDOWN)
                        {
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            // FlToBuf.UpdatePalletState(PalletStatus.W);
                            //   rb.SendCmdPosPallet (RequestCommandPosPallet.REQUEST_GOBACK_FRONTLINE);
                            StateForkLift = ForkLift.FORMAC_ROBOT_WAITTING_GOBACK_FRONTLINE_MACHINE;
                            robot.ShowText("FORMAC_ROBOT_WAITTING_GOBACK_FRONTLINE_MACHINE");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ForkLift.FORMAC_ROBOT_WAITTING_GOBACK_FRONTLINE_MACHINE: // đợi
                        if (resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE)
                        {
                            robot.SwitchToDetectLine(false);
                            resCmd = ResponseCommand.RESPONSE_NONE;
                            rb.prioritLevel.OnAuthorizedPriorityProcedure = false;
                            StateForkLift = ForkLift.FORMAC_ROBOT_RELEASED;
                            robot.ShowText("FORMAC_ROBOT_RELEASED");
                        }
                        else if (resCmd == ResponseCommand.RESPONSE_ERROR)
                        {
                            errorCode = ErrorCode.DETECT_LINE_ERROR;
                            CheckUserHandleError(this);
                        }
                        break;
                    case ForkLift.FORMAC_ROBOT_RELEASED: // trả robot về robotmanagement để nhận quy trình mới
                        robot.orderItem = null;
                       // Global_Object.onFlagDoorBusy = false;
                        robot.SwitchToDetectLine(false);
                       // robot.robotTag = RobotStatus.IDLE;
                        robot.ReleaseWorkingZone();
                        rb.PreProcedureAs = ProcedureControlAssign.PRO_FORKLIFT_TO_MACHINE;
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
                        order.totalTimeProcedure = order.endTimeProcedure.Subtract(order.startTimeProcedure).TotalMinutes;
                        SaveOrderItem(order);
                        break;
                    case ForkLift.FORMAC_ROBOT_DESTROY: // trả robot về robotmanagement để nhận quy trình mới
                        robot.SwitchToDetectLine(false);
                        robot.ReleaseWorkingZone();
                        robot.prioritLevel.OnAuthorizedPriorityProcedure = false;
                        ProRun = false;
                        UpdateInformationInProc(this, ProcessStatus.F);
                        order.status = StatusOrderResponseCode.ROBOT_ERROR;
                        selectHandleError = SelectHandleError.CASE_ERROR_EXIT;
                        procedureStatus = ProcedureStatus.PROC_KILLED;
                        // RestoreOrderItem();
                        FreePlanedBuffer();
                        order.endTimeProcedure = DateTime.Now;
                        order.totalTimeProcedure = order.endTimeProcedure.Subtract(order.startTimeProcedure).TotalMinutes;
                        SaveOrderItem(order);
                        break;
                    //////////////////////////////////////////////////////
                    default:
                        break;
                }
                Thread.Sleep(5);
            }
            StateForkLift = ForkLift.FORBUF_IDLE;
        }
        /*   protected override void CheckUserHandleError(object obj)
           {
               try
               {
                   CheckUserHandleError(this);
               }
               catch
               {

               }
           }*/
        public override void FinishStatesCallBack(Int32 message)
        {
            this.resCmd = (ResponseCommand)message;
        /*  if (this.resCmd == ResponseCommand.RESPONSE_FINISH_GOBACK_FRONTLINE)
            {
               
            }*/
        }
        public class ForkLiftToMachineInfo
        {
            public Pose frontLinePose;
            public String infoPallet;
        }
        public ForkLiftToMachineInfo GetPriorityTaskForkLiftToMachine(int productId)
        {
            ForkLiftToMachineInfo forkLiftToMachineInfo = null;
            try
            {

                bool onHasOrder = false;
                foreach (DeviceItem deviceItem in deviceService.GetDeviceItemList())
                {
                    foreach (OrderItem order in deviceItem.PendingOrderList)
                    {
                        if (order.typeReq == TyeRequest.TYPEREQUEST_BUFFER_TO_MACHINE)
                        {
                            if (order.productId == productId)
                            {
                                forkLiftToMachineInfo = new ForkLiftToMachineInfo();
                                forkLiftToMachineInfo.frontLinePose = order.palletAtMachine.linePos;
                                JInfoPallet infoPallet = new JInfoPallet();

                                infoPallet.pallet = PistonPalletCtrl.PISTON_PALLET_DOWN; /* dropdown */
                                infoPallet.bay = order.palletAtMachine.bay;
                                infoPallet.hasSubLine = "no"; /* no */
                                infoPallet.dir_main = (TrafficRobotUnity.BrDirection)order.palletAtMachine.directMain;
                                infoPallet.dir_sub = (TrafficRobotUnity.BrDirection)order.palletAtMachine.directSub;
                                infoPallet.dir_out = (TrafficRobotUnity.BrDirection)order.palletAtMachine.directOut;
                                infoPallet.line_ord = order.palletAtMachine.line_ord;
                                infoPallet.row = order.palletAtMachine.row;

                                forkLiftToMachineInfo.infoPallet = JsonConvert.SerializeObject(infoPallet);
                                order.status = StatusOrderResponseCode.CHANGED_FORKLIFT;
                                onHasOrder = true;
                                deviceItem.PendingOrderList.Remove(order);
                                break;
                            }

                        }
                    }
                    if (onHasOrder)
                        break;
                }
            }
            catch
            {
                Console.WriteLine("Error in GetPriorityTaskForkLiftToMachine");
            }
            return forkLiftToMachineInfo;
        }
    }
}
