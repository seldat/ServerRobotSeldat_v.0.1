using System;
using System.Diagnostics;
using System.Threading;
using SeldatMRMS.Management.RobotManagent;
using SelDatUnilever_Ver1._00.Management.ComSocket;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;
using static SeldatMRMS.Management.TrafficRobotUnity;

namespace SelDatUnilever_Ver1._00.Management.ChargerCtrl
{
    public class ChargerCtrl : TranferData
    {
        public enum ChargerId
        {
            CHARGER_ID_1 = 1,
            CHARGER_ID_2,
            CHARGER_ID_3,
        }

        public enum ChargerState
        {
            ST_READY = 0x01,
            //ST_CHARGING,/* 02 */
            ST_ERROR, /* 03 */
            ST_CHARGE_FULL, /* 04 */
            ST_PUSH_PISTON, /* 05 */
            ST_CHARGING,
            // ST_CONTACT_GOOD, /* 06 */
            // ST_CONTACT_FAIL /* 07 */
        }

        public enum  ErrorCodeCharger
        {
            TRUE = 0,
            FALSE,
            ERROR_CONNECT
        }

        private enum CmdCharge
        {
            CMD_GET_ID = 0x01,
            RES_GET_ID, /*0x02 */
            CMD_SET_ID, /*0x03 */
            RES_SET_ID, /*0x04 */
            CMD_GET_STATUS, /*0x05 */
            RES_GET_STATUS, /*0x06 */
            CMD_GET_BAT_LEVEL, /*0x07 */
            RES_GET_BAT_LEVEL, /*0x08 */
            CMD_START_CHARGE, /*0x09 */
            RES_START_CHARGE, /*0x0A */
            CMD_STOP_CHARGE, /*0x0B */
            RES_STOP_CHARGE, /*0x0C */
        }

        public class ChargerInfoConfig:NotifyUIBase
        {
            private String _Ip;
            public String Ip { get => _Ip; set { _Ip = value; RaisePropertyChanged("Ip"); } }
            private Int32 _Port;
            public Int32 Port { get => _Port; set { _Port = value; RaisePropertyChanged("Port"); } }
            private  ChargerId _Id;
            public ChargerId Id { get => _Id; set { _Id = value; RaisePropertyChanged("Id"); } }
            private Int32 _IdStr;
            public Int32 IdStr { get => _IdStr; set { _IdStr = value; RaisePropertyChanged("IdStr"); } }
            public Pose PointFrontLine;
            private String _PointFrontLineStr;
            public String PointFrontLineStr { get => _PointFrontLineStr; set { _PointFrontLineStr = value; RaisePropertyChanged("PointFrontLineStr"); } }
            private String _PointOfPallet;
            public String PointOfPallet{ get => _PointOfPallet; set { _PointOfPallet = value; RaisePropertyChanged("PointOfPalletStr"); } }
            public void ParsePointFrontLineValue(String value)
            {
                try
                {
                    double xx = double.Parse(value.Split(',')[0]);
                    double yy = double.Parse(value.Split(',')[1]);
                    double angle = double.Parse(value.Split(',')[2]);
                    PointFrontLine = new Pose(xx,yy,angle);
                }
                catch { }
            }
        }
        public ChargerInfoConfig cf;
        public ChargerCtrl(ChargerInfoConfig cf) : base(cf.Ip, cf.Port)
        {
            //this.SetId(cf.Id);
            this.cf = cf;
        }
        public void UpdateConfigure(ChargerInfoConfig cf)
        {
            this.cf = null;
            this.cf = cf;
        }
        public bool GetId(ref DataReceive data)
        {
            bool ret = false;
            byte[] dataSend = new byte[6];

            dataSend[0] = 0xFA;
            dataSend[1] = 0x55;
            dataSend[2] = (byte)CmdCharge.CMD_GET_ID;
            dataSend[3] = 0x04;
            dataSend[4] = 0x00;
            dataSend[5] = CalChecksum(dataSend, 3);
            ret = this.Tranfer(dataSend, ref data);
            return ret;
        }
        public bool SetId(ChargerId id)
        {
            bool ret = false;
            byte[] dataSend = new byte[7];

            dataSend[0] = 0xFA;
            dataSend[1] = 0x55;
            dataSend[2] = (byte)CmdCharge.CMD_SET_ID;
            dataSend[3] = 0x05;
            dataSend[4] = 0x00;
            dataSend[5] = (byte)id;
            dataSend[6] = CalChecksum(dataSend, 4);
            ret = this.Tranfer(dataSend);
            return ret;
        }

        public bool GetState(ref DataReceive data)
        {
            bool ret = false;
            byte[] dataSend = new byte[6];

            dataSend[0] = 0xFA;
            dataSend[1] = 0x55;
            dataSend[2] = (byte)CmdCharge.CMD_GET_STATUS;
            dataSend[3] = 0x04;
            dataSend[4] = 0x00;
            dataSend[5] = CalChecksum(dataSend, 3);
            ret = this.Tranfer(dataSend, ref data);
            return ret;
        }

        public bool GetBatteryLevel(ref DataReceive data)
        {
            bool ret = false;
            byte[] dataSend = new byte[6];

            dataSend[0] = 0xFA;
            dataSend[1] = 0x55;
            dataSend[2] = (byte)CmdCharge.CMD_GET_BAT_LEVEL;
            dataSend[3] = 0x04;
            dataSend[4] = 0x00;
            dataSend[5] = CalChecksum(dataSend, 3);
            ret = this.Tranfer(dataSend, ref data);
            return ret;
        }
        public bool StartCharge()
        {
            bool ret = false;
            byte[] dataSend = new byte[6];

            dataSend[0] = 0xFA;
            dataSend[1] = 0x55;
            dataSend[2] = (byte)CmdCharge.CMD_START_CHARGE;
            dataSend[3] = 0x04;
            dataSend[4] = 0x00;
            dataSend[5] = CalChecksum(dataSend, 3);
            ret = this.Tranfer(dataSend);
            return ret;
        }
        public bool StopCharge()
        {
            bool ret = false;
            byte[] dataSend = new byte[6];

            dataSend[0] = 0xFA;
            dataSend[1] = 0x55;
            dataSend[2] = (byte)CmdCharge.CMD_STOP_CHARGE;
            dataSend[3] = 0x04;
            dataSend[4] = 0x00;
            dataSend[5] = CalChecksum(dataSend, 3);
            ret = this.Tranfer(dataSend);
            return ret;
        }

        public ErrorCodeCharger WaitState(ChargerState status, UInt32 timeOut)
        {
            ErrorCodeCharger result = ErrorCodeCharger.TRUE;
            Stopwatch sw = new Stopwatch();
            DataReceive st = new DataReceive();
            sw.Start();
            do
            {
                Thread.Sleep(1000);
                if (sw.ElapsedMilliseconds > timeOut)
                {
                    result = ErrorCodeCharger.FALSE;
                    break;
                }
                if(!this.GetState(ref st)){
                    result = ErrorCodeCharger.ERROR_CONNECT;
                }
                if (st.data[0] == (byte)ChargerState.ST_ERROR) {
                    return ErrorCodeCharger.FALSE;
                }
                Console.WriteLine("status++++++++++++++++++===+++++++++++++++++++++++++ :{0}", st.data[0]);
            } while (st.data[0] != (byte)status);
            sw.Stop();
            return result;
        }
        public ErrorCodeCharger GetBatteryAndStatus(ref DataReceive batLevel,ref DataReceive status)
        {
            ErrorCodeCharger result = ErrorCodeCharger.TRUE;
            Thread.Sleep(5000);
            if(!this.GetState(ref status)){
                result = ErrorCodeCharger.ERROR_CONNECT;
            }
            if(!this.GetBatteryLevel(ref batLevel)){
                result = ErrorCodeCharger.ERROR_CONNECT;
            }
            return result;
        }
    }
}
