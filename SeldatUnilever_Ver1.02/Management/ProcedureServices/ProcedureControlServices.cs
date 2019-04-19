using System;
using System.Threading;
using SeldatMRMS.Management.RobotManagent;
using SeldatMRMS.Management.TrafficManager;
using static SeldatMRMS.Management.RobotManagent.RobotUnity;
using static SelDatUnilever_Ver1._00.Management.DeviceManagement.DeviceItem;

namespace SeldatMRMS {
    public class ProcedureControlServices : ControlService {
        public String ProcedureID { get; set; }
        public String DeliveryInfo { get; set; }
        public const long TIME_OUT_WAIT_GOTO_FRONTLINE = 240000;
        public TrafficManagementService TrafficService;
        public struct ContentDatabase { }
        public virtual event Action<Object> ReleaseProcedureHandler;
        public virtual event Action<Object> ErrorProcedureHandler;
        public ProcedureCode procedureCode;
        public virtual ContentDatabase RequestDataFromDataBase () { return new ContentDatabase (); }
        protected RobotUnity robot;
        public ProcedureDataItemsDB procedureDataItemsDB;
        public GateTaskDB gateTaskDB;
        public RobotTaskDB robotTaskDB;
        public ReadyChargerProcedureDB readyChargerProcedureDB;
        public OrderItem order;
        public ProcedureStatus procedureStatus;
        public const UInt32 TIME_OUT_OPEN_DOOR = 5000; /* ms */
        public const UInt32 TIME_OUT_CLOSE_DOOR = 5000; /* ms */
        public enum ProcedureStatus
        {
            PROC_ALIVE,
            PROC_KILLED
        }

        public enum ProcedureCode {
            PROC_CODE_BUFFER_TO_MACHINE = 0,
            PROC_CODE_FORKLIFT_TO_BUFFER,
            PROC_CODE_BUFFER_TO_RETURN,
            PROC_CODE_MACHINE_TO_RETURN,
            PROC_CODE_RETURN_TO_GATE,
            PROC_CODE_ROBOT_TO_READY,
            PROC_CODE_ROBOT_TO_CHARGE,
        }
        public enum ErrorCode {
            RUN_OK = 0,
            DETECT_LINE_ERROR,
            DETECT_LINE_CHARGER_ERROR,
            CONNECT_DOOR_ERROR,
            OPEN_DOOR_ERROR,
            CLOSE_DOOR_ERROR,
            CONNECT_CHARGER_ERROR,
            CONTACT_CHARGER_ERROR,
            LASER_CONTROL_ERROR,
            CAN_NOT_GET_DATA,
            ROBOT_CANNOT_CONNECT_SERVER_AFTER_CHARGE,
            CAN_NOT_TURN_OFF_PC,
            CAN_NOT_TURN_ON_PC,
            CONNECT_BOARD_CTRL_ROBOT_ERROR,
        }

        public ErrorCode errorCode;
        public void RegistrationTranfficService (TrafficManagementService TrafficService) {
            this.TrafficService = TrafficService;

        }
        public enum ForkLift {
            FORBUF_IDLE,
            FORBUF_ROBOT_GOTO_CHECKIN_GATE, // vị trí check in liệu có quy trình nào tại cổng
            FORBUF_ROBOT_WAITTING_GOTO_CHECKIN_GATE,
            FORBUF_ROBOT_CAME_CHECKIN_GATE, // đã đến vị trí, kiem tra khu vuc cong san sang de di vao.
            FORBUF_ROBOT_WAITTING_GOTO_GATE, // doi robot di den khu vuc cong
            FORBUF_ROBOT_CAME_GATE_POSITION, // da den khu vuc cong , gui yeu cau mo cong.
            FORBUF_ROBOT_WAITTING_OPEN_DOOR, //doi mo cong
            FORBUF_ROBOT_OPEN_DOOR_SUCCESS, // mo cua thang cong ,gui toa do line de robot di vao gap hang
            // FORBUF_ROBOT_WAITTING_CAME_FRONTLINE_PALLET_IN, //robot da den dau line , chuyen mode do line
            // FORBUF_ROBOT_CAME_FRONTLINE_PALLET_IN, //robot da den dau line , chuyen mode do line
            // FORBUF_ROBOT_WAITTING_GOTO_PALLET_IN, //cho robot den toa do pallet
            FORBUF_ROBOT_WAITTING_PICKUP_PALLET_IN, // doi robot gap hang
            FORBUF_ROBOT_WAITTING_GOBACK_FRONTLINE_GATE, //doi robot di tro lai dau line cong.
            // FORBUF_ROBOT_WAITTING_GOOUT_GATE, // doi robot di ra khoi cong
            FORBUF_ROBOT_WAITTING_CLOSE_GATE, // doi dong cong.
            FORBUF_ROBOT_WAITTING_GOTO_CHECKIN_BUFFER, // doi robot di den khu vuc checkin cua vung buffer
            FORBUF_ROBOT_WAITTING_ZONE_BUFFER_READY, // doi khu vuc buffer san sang de di vao
            FORBUF_ROBOT_WAITTING_CAME_FRONTLINE_BUFFER, // den dau line buffer, chuyen mode do line
            // FORBUF_ROBOT_WAITTING_GOTO_POINT_BRANCHING, // doi khu vuc buffer san sang de di vao
            // FORBUF_ROBOT_CAME_POINT_BRANCHING, //den dau line pallet, gui chieu quay (trai phai), va toa do pallet (option)
            // FORBUF_ROBOT_GOTO_DROPDOWN_PALLET_BUFFER,
            FORBUF_ROBOT_WAITTING_DROPDOWN_PALLET_BUFFER, // doi robot do line den pallet  va tha pallet
            FORBUF_ROBOT_WAITTING_GOBACK_FRONTLINE_BUFFER, // doi robot di den dau line buffer.
            FORBUF_ROBOT_RELEASED, // trả robot về robotmanagement để nhận quy trình mới
            /*ForkLiftToMachine*/
            FORMAC_ROBOT_WAITTING_PICKUP_PALLET_IN, // doi robot gap hang
            FORMAC_ROBOT_WAITTING_GOBACK_FRONTLINE_GATE, //doi robot di tro lai dau line cong.
            FORMAC_ROBOT_GOTO_FRONTLINE_MACHINE, // doi dong cong.
            FORMAC_ROBOT_WAITTING_CAME_FRONTLINE_MACHINE, // den dau line buffer, chuyen mode do line
            FORMAC_ROBOT_WAITTING_DROPDOWN_PALLET_MACHINE, // doi robot do line den pallet  va tha pallet
            FORMAC_ROBOT_WAITTING_GOBACK_FRONTLINE_MACHINE, // doi robot di den dau line buffer.
            FORMAC_ROBOT_RELEASED, // trả robot về robotmanagement để nhận quy trình mới
            FORMAC_ROBOT_DESTROY, // trả robot về robotmanagement để nhận quy trình mới
        }

        public enum ForkLiftToMachine
        {
           FORMACH_IDLE,
           FORMACH_ROBOT_GOTO_CHECKIN_GATE, // vị trí check in liệu có quy trình nào tại cổng
           FORMACH_ROBOT_WAITTING_GOTO_CHECKIN_GATE,
           FORMACH_ROBOT_CAME_CHECKIN_GATE, // đã đến vị trí, kiem tra khu vuc cong san sang de di vao.
           FORMACH_ROBOT_WAITTING_GOTO_GATE, // doi robot di den khu vuc cong
           FORMACH_ROBOT_CAME_GATE_POSITION, // da den khu vuc cong , gui yeu cau mo cong.
           FORMACH_ROBOT_WAITTING_OPEN_DOOR, //doi mo cong
           FORMACH_ROBOT_OPEN_DOOR_SUCCESS, // mo cua thang cong ,gui toa do line de robot di vao gap hang
           FORMACH_ROBOT_WAITTING_PICKUP_PALLET_IN, // doi robot gap hang
           FORMACH_ROBOT_WAITTING_GOBACK_FRONTLINE_GATE, //doi robot di tro lai dau line cong.
           FORMACH_ROBOT_WAITTING_CLOSE_GATE, // doi dong cong.
           FORMACH_ROBOT_WAITTING_CAME_FRONTLINE_MACHINE, // den dau line buffer, chuyen mode do line
           FORMACH_ROBOT_WAITTING_DROPDOWN_PALLET_MACHINE, // doi robot do line den pallet  va tha pallet
           //FORMACH_ROBOT_WAITTING_GOBACK_FRONTLINE_MACHINE, // doi robot di den dau line buffer.
           FORMACH_ROBOT_RELEASED, // trả robot về robotmanagement để nhận quy trình mới
        }

        public enum BufferToMachine {
            BUFMAC_IDLE,
            BUFMAC_ROBOT_GOTO_CHECKIN_BUFFER,
            BUFMAC_ROBOT_WAITTING_GOTO_CHECKIN_BUFFER, // doi robot di den khu vuc checkin cua vung buffer
            BUFMAC_ROBOT_WAITTING_ZONE_BUFFER_READY, // doi khu vuc buffer san sang de di vao
            BUFMAC_ROBOT_WAITTING_CAME_FRONTLINE_BUFFER, // den dau line buffer, chuyen mode do line
            // BUFMAC_ROBOT_WAITTING_GOTO_POINT_BRANCHING, // doi khu vuc buffer san sang de di vao
            // BUFMAC_ROBOT_CAME_POINT_BRANCHING, //den dau line pallet, gui chieu quay (trai phai), va toa do pallet (option)
            // BUFMAC_ROBOT_GOTO_PICKUP_PALLET_BUFFER,
            BUFMAC_ROBOT_WAITTING_PICKUP_PALLET_BUFFER, // doi robot do line den pallet  va tha pallet
            BUFMAC_ROBOT_WAITTING_GOBACK_FRONTLINE_BUFFER, // doi robot di den dau line buffer.

            // BUFMAC_ROBOT_GOTO_CHECKIN_MACHINE, //cho
            // BUFMAC_ROBOT_CAME_CHECKIN_MACHINE, // đã đến vị trí

            BUFMAC_ROBOT_GOTO_FRONTLINE_DROPDOWN_PALLET,
            // BUFMAC_ROBOT_CAME_FRONTLINE_DROPDOWN_PALLET, 
            // BUFMAC_ROBOT_WAITTING_GOTO_POINT_DROP_PALLET, 

            BUFMAC_ROBOT_WAITTING_DROPDOWN_PALLET,
            BUFMAC_ROBOT_WAITTING_GOTO_FRONTLINE,
            BUFMAC_ROBOT_RELEASED, // trả robot về robotmanagement để nhận quy trình mới
            BUFMAC_ROBOT_DESTROY, // trả robot về robotmanagement để nhận quy trình mới
        }

        public enum BufferToReturn {
            BUFRET_IDLE,
            BUFRET_ROBOT_GOTO_CHECKIN_BUFFER,
            BUFRET_ROBOT_WAITTING_GOTO_CHECKIN_BUFFER, // doi robot di den khu vuc checkin cua vung buffer
            BUFRET_ROBOT_WAITTING_ZONE_BUFFER_READY, // doi khu vuc buffer san sang de di vao
            BUFRET_ROBOT_WAITTING_CAME_FRONTLINE_BUFFER, // den dau line buffer, chuyen mode do line
            // BUFRET_ROBOT_WAITTING_GOTO_POINT_BRANCHING, // doi khu vuc buffer san sang de di vao
            // BUFRET_ROBOT_CAME_POINT_BRANCHING, //den dau line pallet, gui chieu quay (trai phai), va toa do pallet (option)
            // BUFRET_ROBOT_GOTO_PICKUP_PALLET_BUFFER,
            BUFRET_ROBOT_WAITTING_PICKUP_PALLET_BUFFER, // doi robot do line den pallet  va tha pallet
            BUFRET_ROBOT_WAITTING_GOBACK_FRONTLINE_BUFFER, // doi robot di den dau line buffer.

            BUFRET_ROBOT_GOTO_CHECKIN_RETURN, //cho
            BUFRET_ROBOT_CAME_CHECKIN_RETURN, // đã đến vị trí

            BUFRET_ROBOT_GOTO_FRONTLINE_DROPDOWN_PALLET, // cho phép dò line vàthả pallet
            // BUFRET_ROBOT_CAME_FRONTLINE_DROPDOWN_PALLET, // đang trong tiến trình dò line và thả pallet
            // BUFRET_ROBOT_WAITTING_GOTO_POINT_DROP_PALLET, // hoàn thành dò line và thả pallet

            BUFRET_ROBOT_WAITTING_DROPDOWN_PALLET, // quay lại vị trí đầu line
            BUFRET_ROBOT_WAITTING_GOTO_FRONTLINE,
            BUFRET_ROBOT_RELEASED, // trả robot về robotmanagement để nhận quy trình mới
        }

        public enum MachineToReturn {
            MACRET_IDLE,
            MACRET_ROBOT_GOTO_FRONTLINE_MACHINE,
            MACRET_ROBOT_WAITTING_CAME_FRONTLINE_MACHINE, // den dau line buffer, chuyen mode do line
            // MACRET_ROBOT_GOTO_PICKUP_PALLET_MACHINE,
            MACRET_ROBOT_WAITTING_PICKUP_PALLET_MACHINE, // doi robot do line den pallet  va tha pallet
            MACRET_ROBOT_WAITTING_GOBACK_FRONTLINE_MACHINE, // doi robot di den dau line buffer.

            MACRET_ROBOT_GOTO_CHECKIN_RETURN, //cho
            MACRET_ROBOT_CAME_CHECKIN_RETURN, // đã đến vị trí
            MACRET_ROBOT_GOTO_FRONTLINE_RETURN,

            // MACRET_ROBOT_GOTO_FRONTLINE_DROPDOWN_PALLET,  // cho phép dò line vàthả pallet
            // MACRET_ROBOT_CAME_FRONTLINE_DROPDOWN_PALLET, // đang trong tiến trình dò line và thả pallet
            // MACRET_ROBOT_WAITTING_GOTO_POINT_DROP_PALLET, // hoàn thành dò line và thả pallet

            MACRET_ROBOT_WAITTING_DROPDOWN_PALLET, // quay lại vị trí đầu line
            MACRET_ROBOT_WAITTING_GOTO_FRONTLINE,
            MACRET_ROBOT_RELEASED, // trả robot về robotmanagement để nhận quy trình mới
        }
        public enum ReturnToGate {
            RETGATE_IDLE,
            RETGATE_ROBOT_WAITTING_GOTO_CHECKIN_RETURN, // doi robot di den khu vuc checkin cua vung buffer
            RETGATE_ROBOT_WAITTING_ZONE_RETURN_READY, // doi khu vuc buffer san sang de di vao
            RETGATE_ROBOT_WAITTING_CAME_FRONTLINE_RETURN, // den dau line buffer, chuyen mode do line
            // RETGATE_ROBOT_GOTO_PICKUP_PALLET_RETURN,
            RETGATE_ROBOT_WAITTING_PICKUP_PALLET_RETURN, // doi robot do line den pallet  va tha pallet
            RETGATE_ROBOT_WAITTING_GOBACK_FRONTLINE_RETURN, // doi robot di den dau line buffer.
            RETGATE_ROBOT_RELEASED, // trả robot về robotmanagement để nhận quy trình mới
            // RETGATE_ROBOT_GOTO_CHECKIN_GATE, // vị trí check in liệu có quy trình nào tại cổng
            RETGATE_ROBOT_WAITTING_GOTO_CHECKIN_GATE,
            RETGATE_ROBOT_CAME_CHECKIN_GATE, // đã đến vị trí, kiem tra khu vuc cong san sang de di vao.
            RETGATE_ROBOT_WAITTING_GOTO_GATE, // doi robot di den khu vuc cong
            RETGATE_ROBOT_CAME_GATE_POSITION, // da den khu vuc cong , gui yeu cau mo cong.
            RETGATE_ROBOT_WAITTING_OPEN_DOOR, //doi mo cong
            // RETGATE_ROBOT_OPEN_DOOR_SUCCESS, // mo cua thang cong ,gui toa do line de robot di vao gap hang
            // RETGATE_ROBOT_GOTO_POSITION_PALLET_RETURN, //cho robot den toa do pallet
            RETGATE_ROBOT_WAITTING_DROPDOWN_PALLET_RETURN, // doi robot gap hang
            RETGATE_ROBOT_WAITTING_GOBACK_FRONTLINE_GATE, //doi robot di tro lai dau line cong.
            RETGATE_ROBOT_WAITTING_CLOSE_GATE, // doi dong cong.
        }

        public enum RobotGoToCharge {
            ROBCHAR_IDLE,
            // ROBCHAR_CHARGER_CHECKSTATUS, //kiểm tra kết nối và trạng thái sạc
            ROBCHAR_ROBOT_GOTO_CHARGER, //robot be lai vao tram sac
            ROBCHAR_ROBOT_START_CHARGE, //robot be lai vao tram sac
            ROBCHAR_WAITTING_ROBOT_CONTACT_CHARGER, //robot tiep xuc tram sac           
            ROBCHAR_ROBOT_ALLOW_CUTOFF_POWER_ROBOT, //cho phép cắt nguồn robot
            ROBCHAR_ROBOT_WAITTING_CUTOFF_POWER_PC, //cho phép cắt nguồn robot
            ROBCHAR_WAITTING_CHARGEBATTERY, //dợi charge battery và thông tin giao tiếp server và trạm sạc
            ROBCHAR_FINISHED_CHARGEBATTERY, //Hoàn Thành charge battery và thông tin giao tiếp server và trạm sạc
            ROBCHAR_ROBOT_WAITTING_RECONNECTING, //Robot mở nguồng và đợi connect lại
            ROBCHAR_ROBOT_GETOUT_CHARGER, //Robot mở nguồng và đợi connect lại
            ROBCHAR_ROBOT_WAITTING_GETOUT_CHARGER, //Robot mở nguồng và đợi connect lại
            // ROBCHAR_ROBOT_STATUS_GOOD_OPERATION, //điều kiện hoạt động tốt 
            // ROBCHAR_ROBOT_STATUS_BAD_OPERATION, //điều kiện hoạt động không tốt thông tin về procedure management chuyển sang quy trình xử lý sự cố
            ROBCHAR_ROBOT_RELEASED, // trả robot về robotmanagement để nhận quy trình mới
        }
        public enum RobotGoToReady {
            ROBREA_IDLE,
            ROBREA_ROBOT_GOTO_FRONTLINE_READYSTATION, // ROBOT cho tiến vào vị trí đầu line charge su dung laser
            ROBREA_ROBOT_WAITTING_GOTO_READYSTATION, // hoàn thành đến vùng check in/ kiểm tra có robot đang làm việc vùng này và lấy vị trí line và pallet
            ROBREA_ROBOT_WAIITNG_DETECTLINE_TO_READYSTATION, // đang đợi dò line để đến vị trí line trong buffer
            ROBREA_ROBOT_WAITTING_CAME_POSITION_READYSTATION, // đến vị 
            ROBREA_ROBOT_RELEASED
        }
        public ProcedureControlServices (RobotUnity robot) : base (robot) {
            this.robot = robot;
            this.selectHandleError = SelectHandleError.CASE_ERROR_WAITTING;
            robotTaskDB = new RobotTaskDB (robot);
            procedureDataItemsDB = new ProcedureDataItemsDB (procedureCode, robotTaskDB.robotTaskId);
            readyChargerProcedureDB = new ReadyChargerProcedureDB ();
        }

        public override void AssignAnOrder (OrderItem order) {
            this.order = order;

            procedureDataItemsDB.SetOrderItem (order);
            base.AssignAnOrder (order);
        }
        public RobotUnity GetRobotUnity () {
            return this.robot;
        }
        public bool ProRun;

        public enum SelectHandleError {
            CASE_ERROR_EXIT = 0,
            CASE_ERROR_CONTINUOUS,
            CASE_ERROR_WAITTING,
        }

        public SelectHandleError selectHandleError;

        protected virtual void CheckUserHandleError (object obj) {
            bool keepRun = true;
            ProcedureControlServices p = (ProcedureControlServices)obj;
            Debug (obj,"ErrorCode -> " + getStringError(p.errorCode));
            robot.RegistrySolvedForm(this);
            selectHandleError = SelectHandleError.CASE_ERROR_EXIT;
            while (keepRun)
            {
                switch (selectHandleError)
                {
                    case SelectHandleError.CASE_ERROR_WAITTING:
                        // Global_Object.PlayWarning ();
                        robot.border.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                            new Action(delegate () {
                                robot.setColorRobotStatus(RobotStatusColorCode.ROBOT_STATUS_ERROR);
                                order.status = StatusOrderResponseCode.ROBOT_ERROR;
                            }));
                        Thread.Sleep(1000);
                        break;
                    case SelectHandleError.CASE_ERROR_CONTINUOUS:
                        // Global_Object.StopWarning ();
                        robot.border.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                            new Action(delegate () {
                                robot.setColorRobotStatus(RobotStatusColorCode.ROBOT_STATUS_RUNNING);
                            }));
                        selectHandleError = SelectHandleError.CASE_ERROR_WAITTING;
                        order.status = StatusOrderResponseCode.DELIVERING;
                        ProRun = true;
                        keepRun = false;
                        break;
                    case SelectHandleError.CASE_ERROR_EXIT:
                        // Global_Object.StopWarning ();
                        robot.border.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                            new Action(delegate () {
                                robot.setColorRobotStatus(RobotStatusColorCode.ROBOT_STATUS_ERROR);

                            }));
                        order.status = StatusOrderResponseCode.ROBOT_ERROR;
                        robot.PreProcedureAs = robot.ProcedureAs;
               
                        ErrorProcedureHandler(obj);
                        ProRun = false;
                        keepRun = false;
                        break;
                    default:
                        break;
                }
                Thread.Sleep(500);
            }
            selectHandleError = SelectHandleError.CASE_ERROR_WAITTING;
        }
        protected void Debug (object ojb, string log) {
            ProcedureControlServices pCs = (ProcedureControlServices) ojb;
            RobotUnity rb = pCs.GetRobotUnity ();
            string rBid = rb.properties.Label + " => (^_^) ";

            switch (pCs.procedureCode) {
                case ProcedureCode.PROC_CODE_BUFFER_TO_MACHINE:
                    Console.WriteLine (rBid + "BUFFER_TO_MACHINE:" + log);
                    break;
                case ProcedureCode.PROC_CODE_FORKLIFT_TO_BUFFER:
                    Console.WriteLine (rBid + "FORKLIFT_TO_BUFFER:" + log);
                    break;
                case ProcedureCode.PROC_CODE_BUFFER_TO_RETURN:
                    Console.WriteLine (rBid + "BUFFER_TO_RETURN:" + log);
                    break;
                case ProcedureCode.PROC_CODE_MACHINE_TO_RETURN:
                    Console.WriteLine (rBid + "MACHINE_TO_RETURN:" + log);
                    break;
                case ProcedureCode.PROC_CODE_RETURN_TO_GATE:
                    Console.WriteLine (rBid + "RETURN_TO_GATE:" + log);
                    break;
                case ProcedureCode.PROC_CODE_ROBOT_TO_READY:
                    Console.WriteLine (rBid + "ROBOT_TO_READY:" + log);
                    break;
                case ProcedureCode.PROC_CODE_ROBOT_TO_CHARGE:
                    Console.WriteLine (rBid + "ROBOT_TO_CHARGE:" + log);
                    break;
                default:
                    break;
            }
        }

        protected string getStringError(ErrorCode erCode) {
            switch (erCode) {
                case ErrorCode.DETECT_LINE_ERROR:
                    return "DETECT_LINE_ERROR";
                case ErrorCode.DETECT_LINE_CHARGER_ERROR:
                    return "DETECT_LINE_CHARGER_ERROR";
                case ErrorCode.CONNECT_DOOR_ERROR:
                    return "CONNECT_DOOR_ERROR";
                case ErrorCode.OPEN_DOOR_ERROR:
                    return "OPEN_DOOR_ERROR";
                case ErrorCode.CLOSE_DOOR_ERROR:
                    return "CLOSE_DOOR_ERROR";
                case ErrorCode.CONNECT_CHARGER_ERROR:
                    return "CONNECT_CHARGER_ERROR";
                case ErrorCode.CONTACT_CHARGER_ERROR:
                    return "CONTACT_CHARGER_ERROR";
                case ErrorCode.LASER_CONTROL_ERROR:
                    return "LASER_CONTROL_ERROR";
                case ErrorCode.CAN_NOT_GET_DATA:
                    return "CAN_NOT_GET_DATA";
                case ErrorCode.ROBOT_CANNOT_CONNECT_SERVER_AFTER_CHARGE:
                    return "ROBOT_CANNOT_CONNECT_SERVER_AFTER_CHARGE";
                case ErrorCode.CAN_NOT_TURN_OFF_PC:
                    return "CAN_NOT_TURN_OFF_PC";
                case ErrorCode.CAN_NOT_TURN_ON_PC:
                    return "CAN_NOT_TURN_ON_PC";
                case ErrorCode.CONNECT_BOARD_CTRL_ROBOT_ERROR:
                    return "CONNECT_BOARD_CTRL_ROBOT_ERROR";
                default:
                    break;
            }
            return "";
        }
    }
}
