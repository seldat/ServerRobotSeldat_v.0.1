using SelDatUnilever_Ver1._00.Management;
using SelDatUnilever_Ver1._00.Management.ComSocket;
using System;
using System.Diagnostics;
using System.Threading;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;
using static SeldatMRMS.Management.TrafficRobotUnity;

namespace DoorControllerService
{
    public class DoorService : TranferData
    {
        private enum CmdDoor
        {
            //CMD_GET_ID_DOOR = 0x61,
            //RES_GET_ID_DOOR, /*0x62 */
            //CMD_SET_ID_DOOR, /*0x63 */
            //RES_SET_ID_DOOR, /*0x64 */
            CMD_GET_STATUS_DOOR = 0x65, /*0x65 */
            RES_GET_STATUS_DOOR, /*0x66 */
            CMD_OPEN_DOOR, /*0x67 */
            RES_OPEN_DOOR, /*0x68 */
            CMD_CLOSE_DOOR, /*0x69 */
            RES_CLOSE_DOOR, /*0x6A */

            CMD_ON_LAMP, /*0x6B */
            RES_ON_LAMP, /*0x6C */
            CMD_OFF_LAMP, /*0x6D */
            RES_OFF_LAMP, /*0x6E */
        }
        public enum DoorId
        {
            DOOR_MEZZAMINE_UP = 0x01,
            DOOR_MEZZAMINE_RETURN, /* 0x02 */
            DOOR_ELEVATOR, /* 0x03 */
        }
        public enum DoorType
        {
            DOOR_FRONT = 0x01,
            DOOR_BACK, /* 0x02 */
        };
        public enum DoorStatus
        {
            DOOR_UNKNOW = 0x00,
            DOOR_CLOSE,
            DOOR_OPEN, /* 0x02 */
            DOOR_CLOSING, /* 0x03 */
            DOOR_OPENING, /* 0x04 */
            DOOR_ERROR, /* 0x05 */
            DOOR_START_CLOSE,
            DOOR_START_OPEN
        }

        public enum StateCtrl
        {
            DOOR_ST_IDLE = 0x00,
            DOOR_ST_OPEN_FRONT,
            DOOR_ST_WAITTING_OPEN_DOOR_FRONT,
            DOOR_ST_OPEN_FRONT_SUCCESS,
            DOOR_ST_CLOSE_DOOR_FRONT,
            DOOR_ST_WAITTING_CLOSE_DOOR_FRONT,
            DOOR_ST_CLOSE_DOOR_FRONT_SUCCESS,

            DOOR_ST_OPEN_DOOR_BACK,
            DOOR_ST_WAITTING_OPEN_DOOR_BACK,
            DOOR_ST_OPEN_DOOR_BACK_SUCCESS,
            DOOR_ST_CLOSE_DOOR_BACK,
            DOOR_ST_WAITTING_CLOSE_DOOR_BACK,
            DOOR_ST_CLOSE_DOOR_BACK_SUCCESS,
            DOOR_ST_ERROR
        }

        public enum RetState
        {
            DOOR_CTRL_SUCCESS = 0,
            DOOR_CTRL_WAITTING,
            DOOR_CTRL_ERROR
        }

        public class DoorInfoConfig : NotifyUIBase
        {

            private String _Ip;
            public String Ip { get => _Ip; set { _Ip = value; RaisePropertyChanged("Ip"); } }
            private Int32 _Port;
            public Int32 Port { get => _Port; set { _Port = value; RaisePropertyChanged("Port"); } }
            private DoorId _Id;
            public DoorId Id { get => _Id; set { _Id = value; RaisePropertyChanged("Id"); } }
            public Pose PointCheckInGate;
            private String _PointCheckInGateStr;
            public String PointCheckInGateStr { get => _PointCheckInGateStr; set { _PointCheckInGateStr = value; RaisePropertyChanged("PointCheckInGateStr"); } }
            public Pose PointFrontLine;
            private String _PointFrontLineStr;
            public String PointFrontLineStr { get => _PointFrontLineStr; set { _PointFrontLineStr = value; RaisePropertyChanged("PointFrontLineStr"); } }

            private String _infoPallet;
            public String infoPallet { get => _infoPallet; set { _infoPallet = value; RaisePropertyChanged("infoPallet"); } }

            public void ParsePointCheckInGateValue(String value)
            {
                try
                {
                    double xx = double.Parse(value.Split(',')[0]);
                    double yy = double.Parse(value.Split(',')[1]);
                    double angle = double.Parse(value.Split(',')[2]);
                    PointCheckInGate = new Pose(xx, yy, angle);
                }
                catch { }
            }
            public void ParsePointFrontLineValue(String value)
            {
                try
                {
                    double xx = double.Parse(value.Split(',')[0]);
                    double yy = double.Parse(value.Split(',')[1]);
                    double angle = double.Parse(value.Split(',')[2]);
                    PointFrontLine = new Pose(xx, yy, angle);
                }
                catch { }
            }
        }

        public DoorInfoConfig config;
        private Thread doorFrontServiceThread;
        private Thread doorBackServiceThread;
        private StateCtrl stateCtrlDoorFront;
        private StateCtrl stateCtrlDoorBack;
        //public Stopwatch elapsedTimeFront_;
        //public Stopwatch elapsedTimeBack_;
        private const UInt32 TIME_OUT_WAIT_DOOR_FRONT = 8000;
        private const UInt32 TIME_OUT_WAIT_DOOR_BACK = 8000;
        private const UInt32 NUM_TRY_OPEN_DOOR = 10;
        private const UInt32 NUM_TRY_CLOSE_DOOR = 10;
        private UInt32 numTryOpen = 0;
        private UInt32 numTryClose = 0;
        private bool socketBusy = false;

        public DoorService(DoorInfoConfig cf) : base(cf.Ip, cf.Port)
        {
            config = cf;
            doorFrontServiceThread = new Thread(this.doorFrontCtrlProcess);
            doorFrontServiceThread.Start(this);
            doorBackServiceThread = new Thread(this.doorBackCtrlProcess);
            doorBackServiceThread.Start(this);
            stateCtrlDoorFront = StateCtrl.DOOR_ST_IDLE;
            stateCtrlDoorBack = StateCtrl.DOOR_ST_IDLE;
            //elapsedTimeFront_ = new Stopwatch();
            //elapsedTimeBack_ = new Stopwatch();
            this.numTryClose = 0;
            this.numTryOpen = 0;
            //SetId(cf.id);
        }

        //public void setStateCtrlFront(StateCtrl state)
        //{
        //    this.stateCtrlDoorFront = state;
        //}
        //public void setStateCtrlBack(StateCtrl state)
        //{
        //    this.stateCtrlDoorBack = state;
        //}
        public void openDoor(DoorType type)
        {
            this.numTryOpen = 0;
            switch (type)
            {
                case DoorType.DOOR_FRONT:
                    if ((this.stateCtrlDoorFront == StateCtrl.DOOR_ST_IDLE) || (this.stateCtrlDoorFront == StateCtrl.DOOR_ST_ERROR) ||
                    (this.stateCtrlDoorFront == StateCtrl.DOOR_ST_OPEN_FRONT_SUCCESS) || (this.stateCtrlDoorFront == StateCtrl.DOOR_ST_CLOSE_DOOR_FRONT_SUCCESS))
                    {
                        this.stateCtrlDoorFront = StateCtrl.DOOR_ST_OPEN_FRONT;
                    }
                    break;
                case DoorType.DOOR_BACK:
                    if ((this.stateCtrlDoorBack == StateCtrl.DOOR_ST_IDLE) || (this.stateCtrlDoorBack == StateCtrl.DOOR_ST_ERROR) ||
                    (this.stateCtrlDoorBack == StateCtrl.DOOR_ST_OPEN_DOOR_BACK_SUCCESS) || (this.stateCtrlDoorBack == StateCtrl.DOOR_ST_CLOSE_DOOR_BACK_SUCCESS))
                    {
                        this.stateCtrlDoorBack = StateCtrl.DOOR_ST_OPEN_DOOR_BACK;
                    }
                    break;
                default:
                    break;
            }
        }

        public void closeDoor(DoorType type)
        {
            this.numTryClose = 0;
            switch (type)
            {
                case DoorType.DOOR_FRONT:
                    //if ((this.stateCtrlDoorFront == StateCtrl.DOOR_ST_IDLE) || (this.stateCtrlDoorFront == StateCtrl.DOOR_ST_ERROR) ||
                    //(this.stateCtrlDoorFront == StateCtrl.DOOR_ST_OPEN_FRONT_SUCCESS) || (this.stateCtrlDoorFront == StateCtrl.DOOR_ST_CLOSE_DOOR_FRONT_SUCCESS))
                    //{
                    this.stateCtrlDoorFront = StateCtrl.DOOR_ST_CLOSE_DOOR_FRONT;
                    //}                     
                    break;
                case DoorType.DOOR_BACK:
                    //if ((this.stateCtrlDoorBack == StateCtrl.DOOR_ST_IDLE) || (this.stateCtrlDoorBack == StateCtrl.DOOR_ST_ERROR) ||
                    //(this.stateCtrlDoorBack == StateCtrl.DOOR_ST_OPEN_DOOR_BACK_SUCCESS) || (this.stateCtrlDoorBack == StateCtrl.DOOR_ST_CLOSE_DOOR_BACK_SUCCESS))
                    //{
                    this.stateCtrlDoorBack = StateCtrl.DOOR_ST_CLOSE_DOOR_BACK;
                    //}
                    break;
                default:
                    break;
            }
        }

        public RetState checkOpen(DoorType type)
        {
            switch (type)
            {
                case DoorType.DOOR_FRONT:
                    if (this.stateCtrlDoorFront == StateCtrl.DOOR_ST_OPEN_FRONT_SUCCESS)
                    {
                        return RetState.DOOR_CTRL_SUCCESS;
                    }
                    else if (this.stateCtrlDoorFront == StateCtrl.DOOR_ST_ERROR)
                    {
                        Console.WriteLine("OPEN_DOOR_FRONT_ERROR");
                        return RetState.DOOR_CTRL_ERROR;
                    }
                    break;
                case DoorType.DOOR_BACK:
                    if (this.stateCtrlDoorBack == StateCtrl.DOOR_ST_OPEN_DOOR_BACK_SUCCESS)
                    {
                        return RetState.DOOR_CTRL_SUCCESS;
                    }
                    else if (this.stateCtrlDoorBack == StateCtrl.DOOR_ST_ERROR)
                    {
                        Console.WriteLine("OPEN_DOOR_BACK_ERROR");
                        return RetState.DOOR_CTRL_ERROR;
                    }
                    break;
                default:
                    break;
            }
            return RetState.DOOR_CTRL_WAITTING;
        }

        public RetState checkClose(DoorType type)
        {
            switch (type)
            {
                case DoorType.DOOR_FRONT:
                    if (this.stateCtrlDoorFront == StateCtrl.DOOR_ST_CLOSE_DOOR_FRONT_SUCCESS)
                    {
                        return RetState.DOOR_CTRL_SUCCESS;
                    }
                    else if (this.stateCtrlDoorFront == StateCtrl.DOOR_ST_ERROR)
                    {
                        Console.WriteLine("CLOSE_DOOR_FRONT_ERROR");
                        return RetState.DOOR_CTRL_ERROR;
                    }
                    break;
                case DoorType.DOOR_BACK:
                    if (this.stateCtrlDoorBack == StateCtrl.DOOR_ST_CLOSE_DOOR_BACK_SUCCESS)
                    {
                        return RetState.DOOR_CTRL_SUCCESS;
                    }
                    else if (this.stateCtrlDoorBack == StateCtrl.DOOR_ST_ERROR)
                    {
                        Console.WriteLine("CLOSE_DOOR_BACK_ERROR");
                        return RetState.DOOR_CTRL_ERROR;
                    }
                    break;
                default:
                    break;
            }
            return RetState.DOOR_CTRL_WAITTING;
        }

        public void doorFrontCtrlProcess(object ojb)
        {
            DoorService dS = (DoorService)ojb;
            Stopwatch elapsedTimeFront = new Stopwatch();
            elapsedTimeFront.Start();
            while (true)
            {
                switch (this.stateCtrlDoorFront)
                {
                    case StateCtrl.DOOR_ST_IDLE:

                        break;
                    case StateCtrl.DOOR_ST_OPEN_FRONT:
                        if (this.socketBusy == false)
                        {
                            Console.WriteLine("DOOR_ST_OPEN_FRONT");
                            this.socketBusy = true;
                            if (this.Open(DoorType.DOOR_FRONT))
                            {
                                elapsedTimeFront.Restart();
                                this.stateCtrlDoorFront = StateCtrl.DOOR_ST_WAITTING_OPEN_DOOR_FRONT;
                            }
                            else
                            {
                                this.numTryOpen++;
                                if (this.numTryOpen >= NUM_TRY_OPEN_DOOR)
                                {
                                    this.numTryOpen = 0;
                                    this.stateCtrlDoorFront = StateCtrl.DOOR_ST_ERROR;
                                }
                            }
                        }
                        this.socketBusy = false;
                        break;
                    case StateCtrl.DOOR_ST_WAITTING_OPEN_DOOR_FRONT:
                        if (elapsedTimeFront.ElapsedMilliseconds >= TIME_OUT_WAIT_DOOR_FRONT)
                        {
                            elapsedTimeFront.Stop();
                            this.stateCtrlDoorFront = StateCtrl.DOOR_ST_OPEN_FRONT;
                            this.numTryOpen++;
                            Console.WriteLine("TIME_OUT_WAIT_OPEN_DOOR_FRONT");
                            if (this.numTryOpen >= NUM_TRY_OPEN_DOOR)
                            {
                                this.numTryOpen = 0;
                                this.stateCtrlDoorFront = StateCtrl.DOOR_ST_ERROR;
                            }
                        }
                        else
                        {
                            if (this.socketBusy == false)
                            {
                                this.socketBusy = true;
                                DataReceive status = new DataReceive();
                                try
                                {
                                    this.GetStatus(ref status, DoorType.DOOR_FRONT);
                                    if (status.data[0] == (byte)DoorStatus.DOOR_OPEN)
                                    {
                                        this.stateCtrlDoorFront = StateCtrl.DOOR_ST_OPEN_FRONT_SUCCESS;
                                        elapsedTimeFront.Stop();
                                        Console.WriteLine("DOOR_ST_OPEN_FRONT_SUCCESS");
                                    }
                                }
                                catch
                                {

                                }
                            }
                            this.socketBusy = false;
                        }
                        Thread.Sleep(50);
                        break;
                    case StateCtrl.DOOR_ST_OPEN_FRONT_SUCCESS:

                        break;
                    case StateCtrl.DOOR_ST_CLOSE_DOOR_FRONT:
                        if (this.socketBusy == false)
                        {
                            this.socketBusy = true;
                            Console.WriteLine("DOOR_ST_CLOSE_DOOR_FRONT");
                            if (this.Close(DoorType.DOOR_FRONT))
                            {
                                elapsedTimeFront.Restart();
                                this.stateCtrlDoorFront = StateCtrl.DOOR_ST_WAITTING_CLOSE_DOOR_FRONT;
                            }
                            else
                            {
                                this.numTryClose++;
                                if (this.numTryClose >= NUM_TRY_CLOSE_DOOR)
                                {
                                    this.numTryClose = 0;
                                    this.stateCtrlDoorFront = StateCtrl.DOOR_ST_ERROR;
                                }
                            }
                        }
                        this.socketBusy = false;
                        break;
                    case StateCtrl.DOOR_ST_WAITTING_CLOSE_DOOR_FRONT:
                        if (elapsedTimeFront.ElapsedMilliseconds >= TIME_OUT_WAIT_DOOR_FRONT)
                        {
                            elapsedTimeFront.Stop();
                            this.stateCtrlDoorFront = StateCtrl.DOOR_ST_CLOSE_DOOR_FRONT;
                            this.numTryClose++;
                            Console.WriteLine("TIME_OUT_WAIT_CLOSE_DOOR_FRONT");
                            if (this.numTryClose >= NUM_TRY_CLOSE_DOOR)
                            {
                                this.numTryClose = 0;
                                this.stateCtrlDoorFront = StateCtrl.DOOR_ST_ERROR;
                            }
                        }
                        else
                        {
                            if (this.socketBusy == false)
                            {
                                this.socketBusy = true;
                                DataReceive status = new DataReceive();
                                try
                                {
                                    this.GetStatus(ref status, DoorType.DOOR_FRONT);
                                    if (status.data[0] == (byte)DoorStatus.DOOR_CLOSE)
                                    {
                                        this.stateCtrlDoorFront = StateCtrl.DOOR_ST_CLOSE_DOOR_FRONT_SUCCESS;
                                        elapsedTimeFront.Stop();
                                        Console.WriteLine("DOOR_ST_CLOSE_DOOR_FRONT_SUCCESS");
                                    }
                                }
                                catch
                                {
                                }
                            }
                            this.socketBusy = false;
                        }
                        Thread.Sleep(50);
                        break;
                    case StateCtrl.DOOR_ST_CLOSE_DOOR_FRONT_SUCCESS:

                        break;
                    case StateCtrl.DOOR_ST_ERROR:

                        break;
                    default:
                        break;
                }
                Thread.Sleep(50);
            }
        }

        public void doorBackCtrlProcess(object ojb)
        {
            DoorService dS = (DoorService)ojb;
            Stopwatch elapsedTimeBack = new Stopwatch();
            elapsedTimeBack.Start();
            while (true)
            {
                switch (this.stateCtrlDoorBack)
                {
                    case StateCtrl.DOOR_ST_IDLE:

                        break;
                    case StateCtrl.DOOR_ST_OPEN_DOOR_BACK:
                        if (this.socketBusy == false)
                        {
                            this.socketBusy = true;
                            Console.WriteLine("DOOR_ST_OPEN_DOOR_BACK");
                            if (this.Open(DoorType.DOOR_BACK))
                            {
                                elapsedTimeBack.Restart();
                                this.stateCtrlDoorBack = StateCtrl.DOOR_ST_WAITTING_OPEN_DOOR_BACK;
                            }
                            else
                            {
                                this.numTryOpen++;
                                if (this.numTryOpen >= NUM_TRY_OPEN_DOOR)
                                {
                                    this.numTryOpen = 0;
                                    this.stateCtrlDoorBack = StateCtrl.DOOR_ST_ERROR;
                                }
                            }
                        }
                        this.socketBusy = false;
                        break;
                    case StateCtrl.DOOR_ST_WAITTING_OPEN_DOOR_BACK:
                        if(elapsedTimeBack.ElapsedMilliseconds >= TIME_OUT_WAIT_DOOR_BACK)
                        {
                            elapsedTimeBack.Stop();
                            this.stateCtrlDoorBack = StateCtrl.DOOR_ST_OPEN_DOOR_BACK;
                            this.numTryOpen++;
                            Console.WriteLine("TIME_OUT_WAIT_OPEN_DOOR_BACK");
                            if (this.numTryOpen >= NUM_TRY_OPEN_DOOR)
                            {
                                this.numTryOpen = 0;
                                this.stateCtrlDoorBack = StateCtrl.DOOR_ST_ERROR;
                            }
                        }
                        else
                        {
                            if (this.socketBusy == false)
                            {
                                this.socketBusy = true;
                                DataReceive status = new DataReceive();
                                try
                                {
                                    this.GetStatus(ref status, DoorType.DOOR_BACK);
                                    if (status.data[0] == (byte)DoorStatus.DOOR_OPEN)
                                    {
                                        this.stateCtrlDoorBack = StateCtrl.DOOR_ST_OPEN_DOOR_BACK_SUCCESS;
                                        elapsedTimeBack.Stop();
                                        Console.WriteLine("DOOR_ST_OPEN_DOOR_BACK_SUCCESS");
                                    }
                                }
                                catch
                                {
                                }
                            }
                            this.socketBusy = false;
                        }
                        Thread.Sleep(50);
                        break;
                    case StateCtrl.DOOR_ST_OPEN_DOOR_BACK_SUCCESS:

                        break;
                    case StateCtrl.DOOR_ST_CLOSE_DOOR_BACK:
                        if (this.socketBusy == false)
                        {
                            this.socketBusy = true;
                            Console.WriteLine("DOOR_ST_CLOSE_DOOR_BACK");
                            if (this.Close(DoorType.DOOR_BACK))
                            {
                                elapsedTimeBack.Restart();
                                this.stateCtrlDoorBack = StateCtrl.DOOR_ST_WAITTING_CLOSE_DOOR_BACK;
                            }
                            else
                            {
                                this.numTryClose++;
                                if (this.numTryClose >= NUM_TRY_CLOSE_DOOR)
                                {
                                    this.numTryClose = 0;
                                    this.stateCtrlDoorBack = StateCtrl.DOOR_ST_ERROR;
                                }
                            }
                        }
                        this.socketBusy = false;
                        break;
                    case StateCtrl.DOOR_ST_WAITTING_CLOSE_DOOR_BACK:
                        if (elapsedTimeBack.ElapsedMilliseconds >= TIME_OUT_WAIT_DOOR_BACK)
                        {
                            elapsedTimeBack.Stop();
                            this.stateCtrlDoorBack = StateCtrl.DOOR_ST_CLOSE_DOOR_BACK;
                            this.numTryClose++;
                            Console.WriteLine("TIME_OUT_WAIT_CLOSE_DOOR_BACK");
                            if (this.numTryClose >= NUM_TRY_CLOSE_DOOR)
                            {
                                this.numTryClose = 0;
                                this.stateCtrlDoorBack = StateCtrl.DOOR_ST_ERROR;
                            }
                        }
                        else
                        {
                            if (this.socketBusy == false)
                            {
                                this.socketBusy = true;
                                DataReceive status = new DataReceive();
                                try
                                {
                                    this.GetStatus(ref status, DoorType.DOOR_BACK);
                                    if (status.data[0] == (byte)DoorStatus.DOOR_CLOSE)
                                    {
                                        this.stateCtrlDoorBack = StateCtrl.DOOR_ST_CLOSE_DOOR_BACK_SUCCESS;
                                        elapsedTimeBack.Stop();
                                        Console.WriteLine("DOOR_ST_CLOSE_DOOR_BACK_SUCCESS");
                                    }
                                }
                                catch
                                {
                                }
                            }
                            this.socketBusy = false;
                        }
                        Thread.Sleep(50);
                        break;
                    case StateCtrl.DOOR_ST_CLOSE_DOOR_BACK_SUCCESS:

                        break;
                    case StateCtrl.DOOR_ST_ERROR:

                        break;
                    default:
                        break;
                }
                Thread.Sleep(50);
            }
        }

        //        public bool GetId(ref DataReceive data)
        //        {
        //#if true
        //            bool ret = true;
        //#else
        //            bool ret = false;
        //            byte[] dataSend = new byte[6];

        //            dataSend[0] = 0xFA;
        //            dataSend[1] = 0x55;
        //            dataSend[2] = (byte)CmdDoor.CMD_GET_ID_DOOR;
        //            dataSend[3] = 0x04;
        //            dataSend[4] = 0x00;
        //            dataSend[5] = CalChecksum(dataSend,3);
        //            ret = this.Tranfer(dataSend,ref data);
        //#endif
        //            return ret;
        //        }
        //        public bool SetId(DoorId id)
        //        {
        //#if true
        //            bool ret = true;
        //#else
        //            bool ret = false;
        //            byte[] dataSend = new byte[7];

        //            dataSend[0] = 0xFA;
        //            dataSend[1] = 0x55;
        //            dataSend[2] = (byte)CmdDoor.CMD_SET_ID_DOOR;
        //            dataSend[3] = 0x05;
        //            dataSend[4] = 0x00;
        //            dataSend[5] = (byte)id;
        //            dataSend[6] = CalChecksum(dataSend,4);
        //            ret = this.Tranfer(dataSend);
        //#endif
        //            return ret;
        //        }
        private bool GetStatus(ref DataReceive data, DoorType id)
        {
#if false
            bool ret = true;
#else
            bool ret = false;
            byte[] dataSend = new byte[7];

            dataSend[0] = 0xFA;
            dataSend[1] = 0x55;
            dataSend[2] = (byte)CmdDoor.CMD_GET_STATUS_DOOR;
            dataSend[3] = 0x05;
            dataSend[4] = 0x00;
            dataSend[5] = (byte)id;
            dataSend[6] = CalChecksum(dataSend, 4);
            ret = this.Tranfer(dataSend, ref data);
#endif
            return ret;
        }
        private bool Open(DoorType id)
        {
#if false
            bool ret = true;
#else
            bool ret = false;
            byte[] dataSend = new byte[7];

            dataSend[0] = 0xFA;
            dataSend[1] = 0x55;
            dataSend[2] = (byte)CmdDoor.CMD_OPEN_DOOR;
            dataSend[3] = 0x05;
            dataSend[4] = 0x00;
            dataSend[5] = (byte)id;
            dataSend[6] = CalChecksum(dataSend, 4);
            ret = this.Tranfer(dataSend);
#endif
            return ret;
        }
        private bool Close(DoorType id)
        {
#if false
            bool ret = true;
#else
            bool ret = false;
            byte[] dataSend = new byte[7];

            dataSend[0] = 0xFA;
            dataSend[1] = 0x55;
            dataSend[2] = (byte)CmdDoor.CMD_CLOSE_DOOR;
            dataSend[3] = 0x05;
            dataSend[4] = 0x00;
            dataSend[5] = (byte)id;
            dataSend[6] = CalChecksum(dataSend, 4);
            ret = this.Tranfer(dataSend);
#endif
            return ret;
        }

        //private bool checkElapsedTime(Stopwatch elapsedTime,UInt32 timeOut)
        //{
        //    bool ret = false;
        //    if (elapsedTime.ElapsedMilliseconds >= timeOut)
        //    {
        //        ret = true;
        //    }
        //    return ret;
        //}

        //private bool checkElapsedTimeBack(UInt32 timeOut)
        //{
        //    bool ret = false;
        //    if (elapsedTimeBack.ElapsedMilliseconds >= timeOut)
        //    {
        //        ret = true;
        //    }
        //    return ret;
        //}
        //        public bool WaitOpen(DoorType id, UInt32 timeOut)
        //        {
        //            bool result = true;
        //#if true
        //            Stopwatch sw = new Stopwatch();
        //            DataReceive status = new DataReceive();
        //            //this.Open(id);
        //            sw.Start();
        //            do 
        //            {
        //                Thread.Sleep(100);
        //                if (sw.ElapsedMilliseconds > timeOut)
        //                {
        //                    result = false;
        //                    break;
        //                }
        //                this.GetStatus(ref status,id);
        //                if (status.data[0] == (byte)DoorStatus.DOOR_ERROR) {
        //                    result = false;
        //                    break;
        //                }
        //            } while (status.data[0] != (byte)DoorStatus.DOOR_OPEN);
        //            sw.Stop();
        //#endif
        //            return result;
        //        }

        //        public bool WaitClose(DoorType id, UInt32 timeOut)
        //        {
        //            bool result = true;
        //#if true
        //            Stopwatch sw = new Stopwatch();
        //            DataReceive status = new DataReceive();
        //            //this.Close(id);
        //            sw.Start();
        //            do 
        //            {
        //                Thread.Sleep(100);
        //                if (sw.ElapsedMilliseconds > timeOut)
        //                {
        //                    result = false;
        //                    break;
        //                }
        //                this.GetStatus(ref status,id);
        //                if (status.data[0] == (byte)DoorStatus.DOOR_ERROR)
        //                {
        //                    result = false;
        //                    break;
        //                }
        //            } while (status.data[0] != (byte)DoorStatus.DOOR_CLOSE);
        //            sw.Stop();
        //#endif
        //            return result;
        //        }

        public bool LampOn(DoorType id)
        {
            bool ret = false;
            byte[] dataSend = new byte[7];

            dataSend[0] = 0xFA;
            dataSend[1] = 0x55;
            dataSend[2] = (byte)CmdDoor.CMD_ON_LAMP;
            dataSend[3] = 0x05;
            dataSend[4] = 0x00;
            dataSend[5] = (byte)id;
            dataSend[6] = CalChecksum(dataSend, 4);
            ret = this.Tranfer(dataSend);
            return ret;
        }

        public bool LampOff(DoorType id)
        {
            bool ret = false;
            byte[] dataSend = new byte[7];

            dataSend[0] = 0xFA;
            dataSend[1] = 0x55;
            dataSend[2] = (byte)CmdDoor.CMD_OFF_LAMP;
            dataSend[3] = 0x05;
            dataSend[4] = 0x00;
            dataSend[5] = (byte)id;
            dataSend[6] = CalChecksum(dataSend, 4);
            ret = this.Tranfer(dataSend);
            return ret;
        }
    }
}
