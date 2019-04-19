using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SeldatMRMS;
using SeldatMRMS.Management;
using SeldatMRMS.Management.RobotManagent;
using SeldatMRMS.Management.TrafficManager;
using SelDatUnilever_Ver1._00.Communication.HttpBridge;
using SelDatUnilever_Ver1._00.Management.DeviceManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;
using static SeldatMRMS.Management.TrafficRobotUnity;
using static SelDatUnilever_Ver1._00.Management.DeviceManagement.DeviceItem;
using static SelDatUnilever_Ver1.CollectionDataService;

namespace SelDatUnilever_Ver1._00.Management.UnityService
{
   public class TaskRounterService
    {
        public enum ProcessAssignAnTaskWait
        {
            PROC_ANY_IDLE = 0,
            PROC_ANY_CHECK_HAS_ANTASK,
            PROC_ANY_ASSIGN_ANTASK,
            PROC_ANY_GET_ANROBOT_IN_WAITTASKLIST,
            PROC_ANY_CHECK_ROBOT_BATTERYLEVEL,
            PROC_ANY_SET_TRAFFIC_RISKAREA_ON,
            PROC_ANY_CHECK_ROBOT_OUTSIDEREADY,
            PROC_ANY_CHECK_ROBOT_GOTO_READY
        }
        protected enum ProcessAssignTaskReady
        {
            PROC_READY_IDLE = 0,
            PROC_READY_CHECK_HAS_ANTASK,
            PROC_READY_ASSIGN_ANTASK,
            PROC_READY_GET_ANROBOT_INREADYLIST,
            PROC_READY_CHECK_ROBOT_BATTERYLEVEL,
            PROC_READY_SET_TRAFFIC_RISKAREA_ON,
            PROC_READY_CHECK_ROBOT_OUTSIDEREADY,
        }
        protected ProcessAssignTaskReady processAssignTaskReady;
        
        protected ProcessAssignAnTaskWait processAssignAnTaskWait;
        protected ProcedureManagementService procedureService;
        public RobotManagementService robotManageService;
        public TrafficManagementService trafficService;
        public List<DeviceItem> deviceItemsList;
        public bool Alive = false;
        public void RegistryService(RobotManagementService robotManageService)
        {
            this.robotManageService = robotManageService;
        }
        public void RegistryService(TrafficManagementService trafficService)
        {
            this.trafficService = trafficService;
        }
        public void RegistryService(ProcedureManagementService procedureService)
        {
            this.procedureService = procedureService;
        }
        public void RegistryService(List<DeviceItem> deviceItemsList)
        {
            this.deviceItemsList = deviceItemsList;
        }
        public TaskRounterService() {
            //processAssignAnTaskState = ProcessAssignAnTask.PROC_IDLE;
        }
        public void MoveElementToEnd()
        {
            try
            {
                if (deviceItemsList.Count > 0)
                {
                    var element = deviceItemsList[0];
                    deviceItemsList.RemoveAt(0);
                    deviceItemsList.Add(element);
                }
            }
            catch { }
        }
        public OrderItem Gettask()
        {
            OrderItem item = null;
            if (deviceItemsList.Count > 0)
            {
                try
                {
                    item = deviceItemsList[0].GetOrder();
                    if (item == null)
                        return null;
                    if (item.typeReq == TyeRequest.TYPEREQUEST_BUFFER_TO_MACHINE)
                    {
                        if (CheckAvailableFrontLineBuffer(item, false) != null)
                        {
                            int palletId = GetPalletId(item);
                             if (palletId > 0)
                             {
                                 dynamic product = new JObject();
                                 UpdatePalletStateToHold(palletId, item);
                                 product.timeWorkId = item.timeWorkId;
                                 product.activeDate = item.activeDate;
                                 product.productId = item.productId;
                                 product.productDetailId = item.productDetailId;
                                 // chu y sua 
                                 product.palletStatus = PalletStatus.H.ToString(); // W
                                 item.dataRequest = product.ToString();
                                 item.status = StatusOrderResponseCode.DELIVERING;
                                 return item;
                             }
                            // return null;

                            return item;
                        }
                        else
                            return null;
                    }
                    else if (item.typeReq == TyeRequest.TYPEREQUEST_BUFFER_TO_RETURN)
                    {
                        if (CheckAvailableFrontLineBuffer(item, false) != null) // check buffer
                        {
                            if (CheckAvailableFrontLineReturn(item) != null) // check return 
                            {
                                //chưa sửa
                                item.status = StatusOrderResponseCode.DELIVERING;
                                return item;
                            }
                            else
                                return null;
                        }
                        else
                            return null;
                    }
                    else if (item.typeReq == TyeRequest.TYPEREQUEST_MACHINE_TO_RETURN)
                    {
                        if (CheckAvailableFrontLineReturn(item) != null)
                        {
                            item.status = StatusOrderResponseCode.DELIVERING;
                            return item;
                        }
                        else
                            return null;
                    }
                    else
                    {
                        item.status = StatusOrderResponseCode.DELIVERING;
                    }
                }
                catch { }
            }

            return item;
            
        }

        public void UpdatePalletStateToHold(int palletId,OrderItem item)
        {
            String url = Global_Object.url + "pallet/updatePalletStatus";
            dynamic product = new JObject();
            product.palletId = palletId;
            product.planId = item.planId;
            product.palletStatus = PalletStatus.H.ToString();
            product.updUsrId = Global_Object.userLogin;
            String collectionData = RequestDataProcedure(product.ToString(), url);
            Console.WriteLine(collectionData);

        }

        public Pose CheckAvailableFrontLineBuffer(OrderItem order, bool onPlandId = false)
        {
            Pose poseTemp = null;
            try
            {
                String collectionData = RequestDataProcedure(order.dataRequest, Global_Object.url + "plan/getListPlanPallet");
                if (collectionData.Length > 0)
                {
                    JArray results = JArray.Parse(collectionData);
                    if (onPlandId)
                    {
                        foreach (var result in results)
                        {
                            int temp_planId = (int)result["planId"];
                            if (temp_planId == order.planId)
                            {
                                //var bufferResults = result["buffers"][0];
                                foreach (var buffer in result["buffers"])
                                {
                                    if (buffer["pallets"].Count() > 0)
                                    {
                                        var palletInfo = buffer["pallets"][0];
                                        JObject stuff = JObject.Parse((String)palletInfo["dataPallet"]);
                                        double x = (double)stuff["line"]["x"];
                                        double y = (double)stuff["line"]["y"];
                                        double angle = (double)stuff["line"]["angle"];
                                        poseTemp = new Pose(x, y, angle);
                                        break;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }

                                break;

                            }
                        }
                    }
                    else
                    {

                        var result = results[0];
                        //var bufferResults = result["buffers"][0];
                        foreach (var buffer in result["buffers"])
                        {
                            if (buffer["pallets"].Count() > 0)
                            {
                                //JObject stuff = JObject.Parse((String)buffer["pallets"][0]["dataPallet"]);
                                var palletInfo = buffer["pallets"][0];
                                JObject stuff = JObject.Parse((String)palletInfo["dataPallet"]);
                                double x = (double)stuff["line"]["x"];
                                double y = (double)stuff["line"]["y"];
                                double angle = (double)stuff["line"]["angle"];
                                poseTemp = new Pose(x, y, angle);
                                break;
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                }
                //  Console.WriteLine(""+poseTemp.Position.ToString());
            }
            catch
            {
                Console.WriteLine("Error Front Line");
            }
            return poseTemp;
        }

        public int GetPalletId(OrderItem order)
        {
            int palletId = -1;
            String collectionData = RequestDataProcedure(order.dataRequest, Global_Object.url + "plan/getListPlanPallet");
            if (collectionData.Length > 0)
            {
                try
                {
                    JArray results = JArray.Parse(collectionData);
                    foreach (var result in results)
                    {
                        int temp_planId = (int)result["planId"];
                      //  if (temp_planId ==order.planId)
                        {
                            var bufferResults = result["buffers"][0];
                            var palletInfo = bufferResults["pallets"][0];
                            palletId = (int)palletInfo["palletId"];
                            break;
                        }
                    }
                }
                catch { }

            }
            return palletId;
        }
        public Pose CheckAvailableFrontLineReturn(OrderItem order)
        {

            Pose poseTemp = null;
            dynamic product = new JObject();
            product.palletStatus = order.palletStatus;
            String collectionData = RequestDataProcedure(product.ToString(), Global_Object.url + "buffer/getListBufferReturn");
            if (collectionData.Length > 0)
            {
                JArray results = JArray.Parse(collectionData);
                var result = results[0];
                //   var bufferResults = result["buffers"][0];

                var palletInfo = result["pallets"][0];
                JObject stuff = JObject.Parse((String)palletInfo["dataPallet"]);
                double x = (double)stuff["line"]["x"];
                double y = (double)stuff["line"]["y"];
                double angle = (double)stuff["line"]["angle"];
                poseTemp = new Pose(x, y, angle);

            }
            return poseTemp;
        }
        public String RequestDataProcedure(String dataReq, String url)
        {
            //String url = Global_Object.url+"plan/getListPlanPallet";
            BridgeClientRequest clientRequest = new BridgeClientRequest();
            // String url = "http://localhost:8080";
            var data = clientRequest.PostCallAPI(url, dataReq);
            return data.Result;
        }
    }
}
