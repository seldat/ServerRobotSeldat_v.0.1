using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SeldatMRMS;
using SeldatMRMS.Management.RobotManagent;
using SelDatUnilever_Ver1;
using SelDatUnilever_Ver1._00.Communication.HttpBridge;
using SelDatUnilever_Ver1._00.Management.ChargerCtrl;
using System;
using System.Collections.Generic;
using System.Windows;
using static SeldatMRMS.ProcedureControlServices;
using static SelDatUnilever_Ver1._00.Management.DeviceManagement.DeviceItem;

namespace SeldatMRMS
{
    public class DBProcedureService:CollectionDataService
    {
        public enum ProcessStatus
        {
            F=0, // failed
            S=1, //Sucess
            E // Error
        }

        public DBProcedureService(RobotUnity robot) {
            


        }
        public void SendHttpProcedureDataItem(ProcedureDataItemsDB procedureDataItemsDB)
        {
            String url= Global_Object.url + "reportRobot/insertUpdateListRobotProcess";
            List<ProcedureDataItemsDB> listproc = new List<ProcedureDataItemsDB>();
            listproc.Add(procedureDataItemsDB);         
            new BridgeClientRequest().PostCallAPI(url, JsonConvert.SerializeObject(listproc).ToString());
        }
        public void SendHttpReadyChargerProcedureDB(ReadyChargerProcedureDB readyChargerProcedureDB)
        {
            String url = Global_Object.url + "reportRobot/insertUpdateListRobotCharge";
            List<ReadyChargerProcedureDB> listproc = new List<ReadyChargerProcedureDB>();
            listproc.Add(readyChargerProcedureDB);
            new BridgeClientRequest().PostCallAPI(url, JsonConvert.SerializeObject(listproc).ToString());
        }
        public void SendHttpRobotTaskItem(RobotTaskDB robotTaskDB)
        {
            String url = Global_Object.url+ "reportRobot/insertListRobotTask";
            List<RobotTaskDB> listrot = new List<RobotTaskDB>();
            listrot.Add(robotTaskDB);
            new BridgeClientRequest().PostCallAPI(url, JsonConvert.SerializeObject(listrot));
        }
        public class ProcedureDataItemsDB
        {
            ProcedureCode prcode;
            OrderItem order;
            public ProcedureDataItemsDB(ProcedureCode prcode,String robotTaskId) {
                this.prcode = prcode;
                this.order = order;
                rpBeginDatetime = DateTime.Now.ToString("yyyy/MM/dd hh:mm:tt");
                creUsrId = Global_Object.userLogin;
                updUsrId = Global_Object.userLogin;
                this.robotTaskId = robotTaskId;
            }
            public String robotProcessId { get; set; }
            public string robotTaskId { get; set; }
            public int gateKey { get; set; }
            public int planId { get; set; }
            public int deviceId { get; set; }
            public int productId { get; set; }
            public int productDetailId { get; set; }
            public int bufferId { get; set; }
            public int palletId { get; set; }
            public int operationType { get; set; }
            public string rpBeginDatetime { get; set; }
            public string rpEndDatetime { get; set; }
            public string orderContent { get; set; }
            public String robotProcessStastus { get; set; }
            public int creUsrId { get; set; }
            public int updUsrId { get; set; }
            public void SetOrderItem(OrderItem order)
            {
                this.order = order;
            }
            public void GetParams(String status)
            {
                planId = order.planId;
                deviceId = order.deviceId;
                productDetailId = order.productDetailId;
                productId = order.productId;
                palletId = order.palletId;
                bufferId = order.bufferId;
                operationType = (int)prcode ;
                orderContent = JsonConvert.SerializeObject(order);
                rpEndDatetime = DateTime.Now.ToString("yyyy/MM/dd hh:mm:tt");
                robotProcessStastus = status;
            }
        }
        public struct GateTaskDB
        {
            public int gateKey;
            public string gateTaskStastus;
            public string gtProcedureContent;
            public int creUsrId;
            public int updUsrId;
        }
        public class RobotTaskDB
        {
              RobotUnity robot;
              public RobotTaskDB(RobotUnity robot) {
                robotTaskId = robot.properties.NameId+"-"+DateTime.Now.ToString("yyyyMMddHHmmtt");
                robotId = robot.properties.NameId;
                creUsrId = Global_Object.userLogin;
                updUsrId = Global_Object.userLogin;
              }
              public String robotTaskId { get; set; }
              public String robotId { get; set; }
              public String procedureContent{ get; set; }
              public int creUsrId { get; set; }
              public int updUsrId { get ; set; }
             /* public String detailInfo;
              public String problemContent;
              public String solvedProblemContent;*/

        }
        public class ProcedureDataItems
        {
            public DateTime StartTaskTime { get; set; }
            public DateTime EndTime { get; set; }
            public String StatusProcedureDelivered { get; set; }
            public String ErrorStatusID { get; set; } // if have
        }

        public class ReadyChargerProcedureDB
        {
            ChargerCtrl chargerCtrl;
            public ReadyChargerProcedureDB()
            {
                rcBeginDatetime = DateTime.Now.ToString("yyyy//MM/dd hh:mm:tt");
            }
            public int robotChargeId;
            public string robotTaskId;
            public int chargeId;
            public int timeWorkId;
            public String rcBeginDatetime;// ": "2018-12-29 13:23:05" //Không có hoặc chuôi rỗng lây ngày hệ thống
            public String rcEndDatetime;//":  "2018-12-29 13:23:05" //Không có hoặc chuôi rỗng lây ngày hệ thống 
            public double currentBattery;
            public String robotChargeStatus;
            public void Registry(ChargerCtrl chargerCtrl, String robotTaskId)
            {
                this.chargerCtrl = chargerCtrl;
                this.robotTaskId = robotTaskId;
                rcEndDatetime = DateTime.Now.ToString("yy//MM/dd hh:mm tt");
            }
            public void Registry(String robotTaskId)
            {
                this.robotTaskId = robotTaskId;
                rcEndDatetime = DateTime.Now.ToString("yy//MM/dd hh:mm tt");
            }
            public void GetParams(String status)
            {
                rcEndDatetime = DateTime.Now.ToString("yy//MM/dd hh:mm tt");
                robotChargeStatus=status;
            }
           
        }
        public void UpdateInformationInProc(Object obj, ProcessStatus status)
        {
            /*try
            {
                if (obj.GetType() == typeof(ProcedureForkLiftToBuffer))
                {
                    var proc = obj as ProcedureForkLiftToBuffer;
                    proc.procedureDataItemsDB.GetParams(status.ToString());
                    // proc.robot.properties.detailInfo = new TextRange(detailInfo.Document.ContentStart, detailInfo.Document.ContentEnd).Text;
                    proc.robotTaskDB.procedureContent = JsonConvert.SerializeObject(proc.robot.properties).ToString();
                    // MessageBox.Show(JsonConvert.SerializeObject(proc.robotTaskDB));
                    // MessageBox.Show(JsonConvert.SerializeObject(proc.procedureDataItemsDB));
                    proc.SendHttpRobotTaskItem(proc.robotTaskDB);
                    proc.SendHttpProcedureDataItem(proc.procedureDataItemsDB);
                    // proc.selectHandleError = shError;


                }
                else if (obj.GetType() == typeof(ProcedureBufferToMachine))
                {
                    var proc = obj as ProcedureBufferToMachine;
                    proc.procedureDataItemsDB.GetParams(status.ToString());
                    //proc.robot.properties.detailInfo = new TextRange(detailInfo.Document.ContentStart, detailInfo.Document.ContentEnd).Text;
                    proc.robotTaskDB.procedureContent = JsonConvert.SerializeObject(proc.robot.properties).ToString();
                    //  MessageBox.Show(JsonConvert.SerializeObject(proc.robotTaskDB));
                    //   MessageBox.Show(JsonConvert.SerializeObject(proc.procedureDataItemsDB));
                    proc.SendHttpRobotTaskItem(proc.robotTaskDB);
                    proc.SendHttpProcedureDataItem(proc.procedureDataItemsDB);
                    //  proc.selectHandleError = shError;


                }
                else if (obj.GetType() == typeof(ProcedureMachineToReturn))
                {
                    var proc = obj as ProcedureMachineToReturn;
                    proc.procedureDataItemsDB.GetParams(status.ToString());
                    // proc.robot.properties.detailInfo = new TextRange(detailInfo.Document.ContentStart, detailInfo.Document.ContentEnd).Text;
                    proc.robotTaskDB.procedureContent = JsonConvert.SerializeObject(proc.robot.properties).ToString();
                    //   MessageBox.Show(JsonConvert.SerializeObject(proc.robotTaskDB));
                    //   MessageBox.Show(JsonConvert.SerializeObject(proc.procedureDataItemsDB));
                    proc.SendHttpRobotTaskItem(proc.robotTaskDB);
                    proc.SendHttpProcedureDataItem(proc.procedureDataItemsDB);
                    // proc.selectHandleError = shError;


                }
                else if (obj.GetType() == typeof(ProcedureBufferToReturn))
                {
                    var proc = obj as ProcedureBufferToReturn;
                    proc.procedureDataItemsDB.GetParams(status.ToString());
                    //  proc.robot.properties.detailInfo = new TextRange(detailInfo.Document.ContentStart, detailInfo.Document.ContentEnd).Text;
                    proc.robotTaskDB.procedureContent = JsonConvert.SerializeObject(proc.robot.properties).ToString();
                    //   MessageBox.Show(JsonConvert.SerializeObject(proc.robotTaskDB));
                    //    MessageBox.Show(JsonConvert.SerializeObject(proc.procedureDataItemsDB));
                    proc.SendHttpRobotTaskItem(proc.robotTaskDB);
                    proc.SendHttpProcedureDataItem(proc.procedureDataItemsDB);



                }
                else if (obj.GetType() == typeof(ProcedureRobotToReady))
                {
                    var proc = obj as ProcedureRobotToReady;
                    proc.readyChargerProcedureDB.Registry(proc.robotTaskDB.robotTaskId);
                    proc.readyChargerProcedureDB.GetParams(status.ToString());
                    //  proc.robot.properties.detailInfo = new TextRange(detailInfo.Document.ContentStart, detailInfo.Document.ContentEnd).Text;
                    proc.robotTaskDB.procedureContent = JsonConvert.SerializeObject(proc.robot.properties).ToString();
                    //    MessageBox.Show(JsonConvert.SerializeObject(proc.robotTaskDB));
                    //   MessageBox.Show(JsonConvert.SerializeObject(proc.readyChargerProcedureDB));
                    proc.SendHttpRobotTaskItem(proc.robotTaskDB);
                    proc.SendHttpReadyChargerProcedureDB(proc.readyChargerProcedureDB);


                }
                else if (obj.GetType() == typeof(ProcedureRobotToCharger))
                {
                    var proc = obj as ProcedureRobotToCharger;
                    proc.readyChargerProcedureDB.Registry(proc.chargerCtrl, proc.robotTaskDB.robotTaskId);
                    proc.readyChargerProcedureDB.GetParams(status.ToString());
                    // proc.robot.properties.detailInfo = new TextRange(detailInfo.Document.ContentStart, detailInfo.Document.ContentEnd).Text;
                    proc.robotTaskDB.procedureContent = JsonConvert.SerializeObject(proc.robot.properties).ToString();
                    //   MessageBox.Show(JsonConvert.SerializeObject(proc.robotTaskDB));
                    //    MessageBox.Show(JsonConvert.SerializeObject(proc.readyChargerProcedureDB));
                    proc.SendHttpRobotTaskItem(proc.robotTaskDB);
                    proc.SendHttpReadyChargerProcedureDB(proc.readyChargerProcedureDB);
                    //proc.selectHandleError = shError;


                }
            }
            catch { }*/
        }
        public void UpdateInformation(Object obj)
        {
            if (obj.GetType() == typeof(ProcedureForkLiftToBuffer))
            {
                var proc = obj as ProcedureForkLiftToBuffer;
                proc.procedureDataItemsDB.GetParams("F");
               // proc.robot.properties.detailInfo = new TextRange(detailInfo.Document.ContentStart, detailInfo.Document.ContentEnd).Text;
                proc.robotTaskDB.procedureContent = JsonConvert.SerializeObject(proc.robot.properties).ToString();
                // MessageBox.Show(JsonConvert.SerializeObject(proc.robotTaskDB));
                // MessageBox.Show(JsonConvert.SerializeObject(proc.procedureDataItemsDB));
                proc.SendHttpRobotTaskItem(proc.robotTaskDB);
                proc.SendHttpProcedureDataItem(proc.procedureDataItemsDB);
            //    proc.selectHandleError = shError;


            }
            else if (obj.GetType() == typeof(ProcedureBufferToMachine))
            {
                var proc = obj as ProcedureBufferToMachine;
                proc.procedureDataItemsDB.GetParams("F");
               // proc.robot.properties.detailInfo = new TextRange(detailInfo.Document.ContentStart, detailInfo.Document.ContentEnd).Text;
                proc.robotTaskDB.procedureContent = JsonConvert.SerializeObject(proc.robot.properties).ToString();
                //  MessageBox.Show(JsonConvert.SerializeObject(proc.robotTaskDB));
                //   MessageBox.Show(JsonConvert.SerializeObject(proc.procedureDataItemsDB));
                proc.SendHttpRobotTaskItem(proc.robotTaskDB);
                proc.SendHttpProcedureDataItem(proc.procedureDataItemsDB);
              //  proc.selectHandleError = shError;


            }
            else if (obj.GetType() == typeof(ProcedureMachineToReturn))
            {
                var proc = obj as ProcedureMachineToReturn;
                proc.procedureDataItemsDB.GetParams("S");
                //proc.robot.properties.detailInfo = new TextRange(detailInfo.Document.ContentStart, detailInfo.Document.ContentEnd).Text;
                proc.robotTaskDB.procedureContent = JsonConvert.SerializeObject(proc.robot.properties).ToString();
                //   MessageBox.Show(JsonConvert.SerializeObject(proc.robotTaskDB));
                //   MessageBox.Show(JsonConvert.SerializeObject(proc.procedureDataItemsDB));
                proc.SendHttpRobotTaskItem(proc.robotTaskDB);
                proc.SendHttpProcedureDataItem(proc.procedureDataItemsDB);
               // proc.selectHandleError = shError;


            }
            else if (obj.GetType() == typeof(ProcedureBufferToReturn))
            {
                var proc = obj as ProcedureBufferToReturn;
                proc.procedureDataItemsDB.GetParams("S");
               // proc.robot.properties.detailInfo = new TextRange(detailInfo.Document.ContentStart, detailInfo.Document.ContentEnd).Text;
                proc.robotTaskDB.procedureContent = JsonConvert.SerializeObject(proc.robot.properties).ToString();
                //   MessageBox.Show(JsonConvert.SerializeObject(proc.robotTaskDB));
                //    MessageBox.Show(JsonConvert.SerializeObject(proc.procedureDataItemsDB));
                proc.SendHttpRobotTaskItem(proc.robotTaskDB);
                proc.SendHttpProcedureDataItem(proc.procedureDataItemsDB);
            //    proc.selectHandleError = shError;


            }
            else if (obj.GetType() == typeof(ProcedureRobotToReady))
            {
                var proc = obj as ProcedureRobotToReady;
                proc.readyChargerProcedureDB.Registry(proc.robotTaskDB.robotTaskId);
                proc.readyChargerProcedureDB.GetParams("S");
               // proc.robot.properties.detailInfo = new TextRange(detailInfo.Document.ContentStart, detailInfo.Document.ContentEnd).Text;
                proc.robotTaskDB.procedureContent = JsonConvert.SerializeObject(proc.robot.properties).ToString();
                //    MessageBox.Show(JsonConvert.SerializeObject(proc.robotTaskDB));
                //   MessageBox.Show(JsonConvert.SerializeObject(proc.readyChargerProcedureDB));
                proc.SendHttpRobotTaskItem(proc.robotTaskDB);
                proc.SendHttpReadyChargerProcedureDB(proc.readyChargerProcedureDB);
              //  proc.selectHandleError = shError;

            }
            else if (obj.GetType() == typeof(ProcedureRobotToCharger))
            {
                var proc = obj as ProcedureRobotToCharger;
                proc.readyChargerProcedureDB.Registry(proc.chargerCtrl, proc.robotTaskDB.robotTaskId);
                proc.readyChargerProcedureDB.GetParams("S");
               // proc.robot.properties.detailInfo = new TextRange(detailInfo.Document.ContentStart, detailInfo.Document.ContentEnd).Text;
                proc.robotTaskDB.procedureContent = JsonConvert.SerializeObject(proc.robot.properties).ToString();
                //   MessageBox.Show(JsonConvert.SerializeObject(proc.robotTaskDB));
                //    MessageBox.Show(JsonConvert.SerializeObject(proc.readyChargerProcedureDB));
                proc.SendHttpRobotTaskItem(proc.robotTaskDB);
                proc.SendHttpReadyChargerProcedureDB(proc.readyChargerProcedureDB);
              //  proc.selectHandleError = shError;


            }
        }
    }
}
