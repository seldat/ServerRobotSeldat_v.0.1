using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SeldatMRMS;
using SeldatMRMS.Management;
using SelDatUnilever_Ver1._00.Communication.HttpBridge;
using SelDatUnilever_Ver1._00.Management.ProcedureServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;
using static SeldatMRMS.Management.TrafficRobotUnity;
using static SelDatUnilever_Ver1._00.Management.DeviceManagement.DeviceItem;

namespace SelDatUnilever_Ver1
{


    public class CollectionDataService
    {
        public enum PalletStatus
        {
            F = 200, // Free pallet
            W = 201, // Have Pallet
            P = 202,
            H =203

        }
        //public int planID { get; set; }
        // public int productID { get; set; }
        // public int planID { get; set; }
        protected OrderItem order;
        //public String typeRequest; // FL: ForkLift// BM: BUFFER MACHINE // PR: Pallet return
        //public String activeDate;
        // public int timeWorkID;
        public List<Pose> checkInBuffer = new List<Pose>();
        
        protected int palletId { get; set; }
        protected int planId { get; set; }
        public CollectionDataService()
        {
           // clientRequest = new BridgeClientRequest();
           // clientRequest.ReceiveResponseHandler += ReceiveResponseHandler;
            planId = -1;
        }
        public CollectionDataService(OrderItem order)
        {
            this.order = order;
            //clientRequest = new BridgeClientRequest();
           // clientRequest.ReceiveResponseHandler += ReceiveResponseHandler;

        }
        public virtual void AssignAnOrder(OrderItem order)
        {
            this.order = order;
        }
        public String createPlanBuffer()
        {
            dynamic product = new JObject();
            product.timeWorkId= 1;
            product.activeDate =order.activeDate;
            product.productId =order.productId;
            product.productDetailId=order.productDetailId;
            product.updUsrId = Global_Object.userLogin;
            product.palletAmount=1;
            String response = RequestDataProcedure(product.ToString(), Global_Object.url + "plan/createPlanPallet");
            return response;
        }

        public void FreePlanedBuffer()
        {
                String url = Global_Object.url + "pallet/updatePalletStatus";
                int _palletId = GetPalletId(order.planId,true);
                if (_palletId > 0)
                {
                    dynamic product = new JObject();
                    product.palletId = _palletId;
                    product.planId = order.planId;
                    product.palletStatus = PalletStatus.F.ToString();
                    product.updUsrId = Global_Object.userLogin;
                    var data = RequestDataProcedure(product.ToString(), url);

                }
            
        }

        public void FreeHoldBuffer()
        {
            String url = Global_Object.url + "pallet/updatePalletStatus";
            int _palletId = GetPalletId(order.planId,false);
            if (_palletId > 0)
            {
                dynamic product = new JObject();
                product.palletId = _palletId;
                product.planId = order.planId;
                product.palletStatus = PalletStatus.W.ToString();
                product.updUsrId = Global_Object.userLogin;
                var data = RequestDataProcedure(product.ToString(), url);

            }

        }
        // lấy pallet info cho viec free the plan planed
        public int GetPalletId(int planId,bool onPlanId)
        {
            int palletId = -1;
            try
            {
                String collectionData = RequestDataProcedure(order.dataRequest, Global_Object.url + "plan/getListPlanPallet");
                if (collectionData.Length > 0)
                {
                    try
                    {
                        JArray results = JArray.Parse(collectionData);
                        if (onPlanId)
                        {

                            foreach (var result in results)
                            {
                                int temp_planId = (int)result["planId"];
                                if (temp_planId == planId)
                                {
                                    var bufferResults = result["buffers"][0];
                                    var palletInfo = bufferResults["pallets"][0];
                                    palletId = (int)palletInfo["palletId"];
                                    break;
                                }
                            }
                        }
                        else
                        {
                            var result = results[0];
                            var bufferResults = result["buffers"][0];
                            foreach (var palletInfo in bufferResults["pallets"])
                            {
                                try
                                {
                                    palletId = (int)palletInfo["palletId"];
                                    break;
                                }
                                catch
                                { }
                            }
                        }
                    }
                    catch { }

                }
            }
            catch
            {
                Console.WriteLine("Errror Get palletID");
            }
            return palletId;
        }
       
        public String RequestDataProcedure(String dataReq, String url)
        {
            //String url = Global_Object.url+"plan/getListPlanPallet";
            // String url = "http://localhost:8080";
            BridgeClientRequest clientRequest=new BridgeClientRequest();
            var data = clientRequest.PostCallAPI(url, dataReq);
            return data.Result;
        }

        public Pose GetCheckInBuffer(bool onPlandId=false)
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
                                        String checkinResults = (String)buffer["bufferCheckIn"];
                                        JObject stuff = JObject.Parse(checkinResults);
                                        double x = (double)stuff["checkin"]["x"];
                                        double y = (double)stuff["checkin"]["y"];
                                        double angle = (double)stuff["checkin"]["angle"];
                                        poseTemp = new Pose(x, y, angle);
                                        planId = order.planId;
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
                        foreach (var buffer in result["buffers"])
                        {
                            if (buffer["pallets"].Count() > 0)
                            {
                                String checkinResults = (String)buffer["bufferCheckIn"];
                                JObject stuff = JObject.Parse(checkinResults);
                                double x = (double)stuff["checkin"]["x"];
                                double y = (double)stuff["checkin"]["y"];
                                double angle = (double)stuff["checkin"]["angle"];
                                poseTemp = new Pose(x, y, angle);
                                planId = order.planId;
                                break;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        //var bufferResults = result["buffers"][0];
                        //String checkinResults = (String)bufferResults["bufferCheckIn"];
                        //JObject stuff = JObject.Parse(checkinResults);
                        //double x = (double)stuff["checkin"]["x"];
                        //double y = (double)stuff["checkin"]["y"];
                        //double angle = (double)stuff["checkin"]["angle"];
                        //poseTemp = new Pose(x, y, angle);
                        //planId = order.planId;
                    }
                }
            }
            catch { Console.WriteLine("Error check in data collection"); }
            return poseTemp;
        }
        public Pose GetAnyPointInBuffer(bool onPlandId = false) // đổi 
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
                                        String checkinResults = (String)buffer["bufferCheckIn"];
                                        JObject stuff = JObject.Parse(checkinResults);
                                        double x = (double)stuff["headpoint"]["x"];
                                        double y = (double)stuff["headpoint"]["y"];
                                        double angle = (double)stuff["headpoint"]["angle"];
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
                        foreach (var buffer in result["buffers"])
                        {
                            if (buffer["pallets"].Count() > 0)
                            {
                                //var bufferResults = result["buffers"][0];
                                String checkinResults = (String)buffer["bufferCheckIn"];
                                JObject stuff = JObject.Parse(checkinResults);
                                double x = (double)stuff["headpoint"]["x"];
                                double y = (double)stuff["headpoint"]["y"];
                                double angle = (double)stuff["headpoint"]["angle"];
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
            }
            catch
            {
                Console.WriteLine("Get AnyPoint Error");
            }
            return poseTemp;
        }

        public Pose GetFrontLineBuffer(bool onPlandId = false)
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
                        //foreach (var palletInfo in bufferResults["pallets"])
                        //{
                        //    // var palletInfo = bufferResults["pallets"][0];
                        //    try
                        //    {
                        //        JObject stuff = JObject.Parse((String)palletInfo["dataPallet"]);
                        //        double x = (double)stuff["line"]["x"];
                        //        double y = (double)stuff["line"]["y"];
                        //        double angle = (double)stuff["line"]["angle"];
                        //        poseTemp = new Pose(x, y, angle);
                        //        break;
                        //    }
                        //    catch { }
                        //}

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

        public Pose GetFrontLineMachine()
        {
            return order.palletAtMachine.linePos;
        }


        public Pose GetCheckInReturn()
        {
            dynamic product = new JObject();
            product.palletStatus = order.palletStatus;
            Pose poseTemp = null;
            String collectionData = RequestDataProcedure(product.ToString(), Global_Object.url + "buffer/getListBufferReturn");
            if (collectionData.Length > 0)
            {
                JArray results = JArray.Parse(collectionData);
                var result = results[0];
                foreach (var buffer in result["buffers"])
                {
                    if (buffer["pallets"].Count() > 0)
                    {
                        String checkinResults = (String)buffer["bufferCheckIn"];
                        JObject stuff = JObject.Parse(checkinResults);
                        double x = (double)stuff["checkin"]["x"];
                        double y = (double)stuff["checkin"]["y"];
                        double angle = (double)stuff["checkin"]["angle"];
                        poseTemp = new Pose(x, y, angle);
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                

            }
            return poseTemp;
        }

        public Pose GetFrontLineReturn()
        {

            Pose poseTemp = null;
            dynamic product = new JObject();
            product.palletStatus = order.palletStatus;
            String collectionData = RequestDataProcedure(product.ToString(), Global_Object.url + "buffer/getListBufferReturn");
            if (collectionData.Length > 0)
            {
                JArray results = JArray.Parse(collectionData);
                var result = results[0];
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
                

            }
            return poseTemp;
        }


        /*
         */
        public String GetInfoOfPalletBuffer(TrafficRobotUnity.PistonPalletCtrl pisCtrl, bool onPlandId = false)
        {
            JInfoPallet infoPallet = new JInfoPallet();
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
                                foreach (var buffer in result["buffers"])
                                {
                                    //var bufferResults = result["buffers"][0];
                                    if (buffer["pallets"].Count() > 0)
                                    {
                                        var palletInfo = buffer["pallets"][0];

                                        palletId = (int)palletInfo["palletId"];
                                        JObject stuff = JObject.Parse((String)palletInfo["dataPallet"]);

                                        int row = (int)stuff["pallet"]["row"];
                                        int bay = (int)stuff["pallet"]["bay"];
                                        int directMain = (int)stuff["pallet"]["dir_main"];
                                        int directSub = (int)stuff["pallet"]["dir_sub"];
                                        int directOut = (int)stuff["pallet"]["dir_out"];
                                        int line_ord = (int)stuff["pallet"]["line_ord"];
                                        string subline = (string)stuff["pallet"]["hasSubLine"];

                                        infoPallet.pallet = pisCtrl; /* dropdown */
                                        infoPallet.dir_main = (TrafficRobotUnity.BrDirection)directMain;
                                        infoPallet.bay = bay;
                                        infoPallet.hasSubLine = subline; /* yes or no */
                                        infoPallet.dir_sub = (TrafficRobotUnity.BrDirection)directSub; /* right */
                                        infoPallet.dir_out = (TrafficRobotUnity.BrDirection)directOut;
                                        infoPallet.row = row;
                                        infoPallet.line_ord = line_ord;
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
                                var palletInfo = buffer["pallets"][0];
                                palletId = (int)palletInfo["palletId"];
                                JObject stuff = JObject.Parse((String)palletInfo["dataPallet"]);
                                int row = (int)stuff["pallet"]["row"];
                                int bay = (int)stuff["pallet"]["bay"];
                                int directMain = (int)stuff["pallet"]["dir_main"];
                                int directSub = (int)stuff["pallet"]["dir_sub"];
                                int directOut = (int)stuff["pallet"]["dir_out"];
                                int line_ord = (int)stuff["pallet"]["line_ord"];
                                string subline = (string)stuff["pallet"]["hasSubLine"];

                                infoPallet.pallet = pisCtrl; /* dropdown */
                                infoPallet.dir_main = (TrafficRobotUnity.BrDirection)directMain;
                                infoPallet.bay = bay;
                                infoPallet.hasSubLine = subline; /* yes or no */
                                infoPallet.dir_sub = (TrafficRobotUnity.BrDirection)directSub; /* right */
                                infoPallet.dir_out = (TrafficRobotUnity.BrDirection)directOut;
                                infoPallet.row = row;
                                infoPallet.line_ord = line_ord;
                                break;
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("Error at GetInfoOfPalletBuffer");
                return "";
            }
            return JsonConvert.SerializeObject(infoPallet);
        }

        public String GetInfoOfPalletMachine(TrafficRobotUnity.PistonPalletCtrl pisCtrl)
        {
            JInfoPallet infoPallet = new JInfoPallet();

            infoPallet.pallet = pisCtrl; /* dropdown */
            infoPallet.bay = order.palletAtMachine.bay;
            infoPallet.hasSubLine = "no"; /* no */
            infoPallet.dir_main = (TrafficRobotUnity.BrDirection)order.palletAtMachine.directMain;
            infoPallet.dir_sub = (TrafficRobotUnity.BrDirection)order.palletAtMachine.directSub;
            infoPallet.dir_out= (TrafficRobotUnity.BrDirection)order.palletAtMachine.directOut;
            infoPallet.line_ord = order.palletAtMachine.line_ord;
            infoPallet.row = order.palletAtMachine.row;

            return JsonConvert.SerializeObject(infoPallet);
        }

        public String GetInfoOfPalletReturn(TrafficRobotUnity.PistonPalletCtrl pisCtrl)
        {
            JInfoPallet infoPallet = new JInfoPallet();

            dynamic product = new JObject();
            product.palletStatus = order.palletStatus;
            String collectionData = RequestDataProcedure(product.ToString(), Global_Object.url + "buffer/getListBufferReturn");
            if (collectionData.Length > 0)
            {
                JArray results = JArray.Parse(collectionData);
                var result = results[0];
                foreach (var buffer in result["buffers"])
                {
                    if (buffer["pallets"].Count() > 0)
                    {
                        var palletInfo = buffer["pallets"][0];
                        palletId = (int)palletInfo["palletId"];
                        JObject stuff = JObject.Parse((String)palletInfo["dataPallet"]);
                        int row = (int)stuff["pallet"]["row"];
                        int bay = (int)stuff["pallet"]["bay"];
                        int directMain = (int)stuff["pallet"]["dir_main"];
                        int directSub = (int)stuff["pallet"]["dir_sub"];
                        int directOut = (int)stuff["pallet"]["dir_out"];
                        int line_ord = (int)stuff["pallet"]["line_ord"];
                        string hasSubLine = (string)stuff["pallet"]["hasSubLine"];
                        infoPallet.pallet = pisCtrl; /* dropdown */
                        infoPallet.bay = bay;
                        infoPallet.hasSubLine = hasSubLine;
                        infoPallet.dir_main = (TrafficRobotUnity.BrDirection)directMain;
                        infoPallet.dir_sub = (TrafficRobotUnity.BrDirection)directSub;
                        infoPallet.dir_out = (TrafficRobotUnity.BrDirection)directOut;
                        infoPallet.row = row;
                        infoPallet.line_ord = line_ord;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                
                

            }
            return JsonConvert.SerializeObject(infoPallet);
        }

        public void UpdatePalletState(PalletStatus palletStatus)
        {
            String url = Global_Object.url + "pallet/updatePalletStatus";
            dynamic product = new JObject();
            product.palletId = palletId;
            product.planId = planId;
            product.palletStatus = palletStatus.ToString();
            product.updUsrId = Global_Object.userLogin;
            var data = RequestDataProcedure( product.ToString(),url);

        }

        protected virtual void ReceiveResponseHandler(String msg) { }
        protected virtual void ErrorHandler(ProcedureMessages.ProcMessage procMessage) { }
    }
}
