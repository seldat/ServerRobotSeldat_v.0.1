using Newtonsoft.Json.Linq;
using SeldatMRMS;
using SeldatMRMS.Management.DoorServices;
using SeldatUnilever_Ver1._02;
using SelDatUnilever_Ver1._00.Communication.HttpBridge;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static DoorControllerService.DoorService;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;
using static SelDatUnilever_Ver1.CollectionDataService;

namespace SelDatUnilever_Ver1._00.Management.DeviceManagement
{
    public class DeviceItem : NotifyUIBase
    {
        public enum StatusOrderResponseCode
        {
            ORDER_STATUS_RESPONSE_SUCCESS = 200,
            ORDER_STATUS_RESPONSE_ERROR_DATA = 201,
            ORDER_STATUS_RESPONSE_NOACCEPTED = 202,
            ORDER_STATUS_DOOR_BUSY = 203,
            PENDING = 300,
            DELIVERING = 301,
            FINISHED = 302,
            ROBOT_ERROR = 303,
            NO_BUFFER_DATA = 304,
            CHANGED_FORKLIFT = 305,
            DESTROYED = 306,


        }

        public class StatusOrderResponse
        {
            public int status;
            public String ErrorMessage;
        }
        public enum PalletCtrl
        {
            Pallet_CTRL_DOWN = 0,
            Pallet_CTRL_UP = 1

        }
        public class DataPallet
        {
            public int row;
            public int bay;
            public int directMain;
            public int directSub;
            public int directOut;
            public int line_ord;
            public PalletCtrl palletCtrl;
            public Pose linePos;
        }
        public enum TyeRequest
        {
            TYPEREQUEST_FORLIFT_TO_BUFFER = 1,
            TYPEREQUEST_BUFFER_TO_MACHINE = 2,
            TYPEREQUEST_BUFFER_TO_RETURN = 3,
            TYPEREQUEST_MACHINE_TO_RETURN = 4,
            TYPEREQUEST_RETURN_TO_GATE = 5,
            TYPEREQUEST_CLEAR = 6,
            TYPEREQUEST_OPEN_FRONTDOOR_DELIVERY_PALLET = 7,
            TYPEREQUEST_CLOSE_FRONTDOOR_DELIVERY_PALLET = 8,
            TYPEREQUEST_OPEN_FRONTDOOR_RETURN_PALLET = 9,
            TYPEREQUEST_CLOSE_FRONTDOOR_RETURN_PALLET = 10,
            TYPEREQUEST_CLEAR_FORLIFT_TO_BUFFER = 11,
            TYPEREQUEST_FORLIFT_TO_MACHINE = 12, // santao jujeng cap bottle
            TYPEREQUEST_WMS_RETURNPALLET_BUFFER = 13, // santao jujeng cap bottle
            TYPEREQUEST_CHARGE = 14, // santao jujeng cap bottle
            TYPEREQUEST_GOTO_READY = 15, // santao jujeng cap bottle
        }
        public enum TabletConTrol
        {
            TABLET_MACHINE = 10000,
            TABLET_FORKLIFT = 10001
        }
        public enum CommandRequest
        {
            CMD_DATA_ORDER_BUFFERTOMACHINE = 100,
            CMD_DATA_ORDER_RETURN = 100,
            CMD_DATA_ORDER_FORKLIFT = 101,
            CMD_DATA_STATE = 102
        }
        public class OrderItem
        {
            public OrderItem() { }
            public String userName { get; set; }
            public String robot { get; set; }
            public TyeRequest typeReq { get; set; } // FL: ForkLift// BM: BUFFER MACHINE // PR: Pallet return

            public StatusOrderResponseCode status { get; set; }
            private String OrderId { get; set; }
            public int planId { get; set; }
            public int deviceId;
            public String productDetailName { get; set; }
            public int productId { get; set; }
            public int productDetailId { get; set; }
       

              public String activeDate;
            public String dateTime { get; set; }

            public int timeWorkId;
            public String palletStatus;
            public int palletId;
            public int updUsrId;
            public int lengthPallet;
            public String dataRequest;
            // public bool status = false; // chua hoan thanh
            public DataPallet palletAtMachine;

            public int bufferId;
            public int palletAmount;
            public DateTime startTimeProcedure=new DateTime();
            public DateTime endTimeProcedure = new DateTime();
            public double totalTimeProcedure { get; set; }
            public bool onAssiged = false;




        }
        public string userName { get; set; } // dia chi Emei
        public string codeID;
        public List<OrderItem> PendingOrderList { get; set; }
        public List<OrderItem> OrderedItemList { get; set; }
        public int orderedAmount = 0;
        public int doneAmount = 0;
        public MainWindow mainWindow;

        public DeviceItem()
        {
            PendingOrderList = new List<OrderItem>();
            OrderedItemList = new List<OrderItem>();
        }
        public DeviceItem(MainWindow mainWindow)
        {
            PendingOrderList = new List<OrderItem>();
            OrderedItemList = new List<OrderItem>();
            this.mainWindow = mainWindow;
        }
        public void state(CommandRequest pCommandRequest, String data)
        {
            switch (pCommandRequest)
            {
                case CommandRequest.CMD_DATA_ORDER_BUFFERTOMACHINE:
                    break;
                case CommandRequest.CMD_DATA_ORDER_FORKLIFT:
                    break;
                case CommandRequest.CMD_DATA_STATE:
                    break;
            }
        }
        public void StatusOrderItem(OrderItem item, StatusOrderResponse statusOrderResponse)
        {

        }
        public void RemoveFirstOrder()
        {
            if (PendingOrderList.Count > 0)
            {
                PendingOrderList.RemoveAt(0);
            }
        }
        public void AddOrder(OrderItem hasOrder)
        {
            try
            {
                PendingOrderList.Add(hasOrder);
                OrderedItemList.Add(hasOrder);
            }
            catch { }
        }
        public void AddOrderCreatePlan(OrderItem order)
        {
            try
            {

                if (Convert.ToInt32(CreatePlanBuffer(order)) > 0)
                {
                    PendingOrderList.Add(order);
                    OrderedItemList.Add(order);
                }
            }
            catch { }
        }
        public OrderItem GetOrder()
        {
            if (PendingOrderList.Count > 0)
            {
                return PendingOrderList[0];
            }
            return null;
        }
        public void ClearOrderList()
        {
            if (PendingOrderList.Count > 0)
            {
                PendingOrderList.Clear();
            }
        }
        public void rounter(String data)
        {

        }
        public StatusOrderResponse ParseData(String dataReq)
        {
            StatusOrderResponse statusOrderResponse = null;
            try
            {
                JObject results = JObject.Parse(dataReq);
                int typeReq = (int)results["typeReq"];
                if (typeReq == (int)TyeRequest.TYPEREQUEST_FORLIFT_TO_BUFFER)
                {
                    if (PendingOrderList.Count==0)
                    {
                        if(Global_Object.onFlagDoorBusy)
                        {
                            statusOrderResponse = new StatusOrderResponse() { status = (int)StatusOrderResponseCode.ORDER_STATUS_DOOR_BUSY, ErrorMessage = "" };
                            return statusOrderResponse;
                        }
                    }
                    else
                    {
                        statusOrderResponse = new StatusOrderResponse() { status = (int)StatusOrderResponseCode.ORDER_STATUS_DOOR_BUSY, ErrorMessage = "" };
                        return statusOrderResponse;
                    }
                    
                    OrderItem order = new OrderItem();
                    order.typeReq = (TyeRequest)typeReq;
                    order.userName = (String)results["userName"];
                    order.productDetailId = (int)results["productDetailId"];
                    order.productDetailName = (String)results["productDetailName"];
                    order.productId = (int)results["productId"];
                    order.planId = (int)results["planId"];
                    order.deviceId = (int)results["deviceId"];
                    order.timeWorkId = (int)results["timeWorkId"];
                    order.activeDate = (string)results["activeDate"];
                    // order.palletStatus = (String)results["palletStatus"];
                    dynamic product = new JObject();
                    product.timeWorkId = order.timeWorkId;
                    product.activeDate = order.activeDate;
                    product.productId = order.productId;
                    product.productDetailId = order.productDetailId;

                    // chu y sua 
                    product.palletStatus = PalletStatus.P.ToString();
                    order.dataRequest = product.ToString();
                    order.status = StatusOrderResponseCode.PENDING;
                    order.dateTime = (string)DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt");
                    if (Convert.ToInt32(CreatePlanBuffer(order)) > 0)
                    {
                        Global_Object.onFlagDoorBusy = true;
                        PendingOrderList.Add(order);
                        OrderedItemList.Add(order);
                    }
                    else
                    {
                        statusOrderResponse = new StatusOrderResponse() { status = (int)StatusOrderResponseCode.ORDER_STATUS_RESPONSE_NOACCEPTED, ErrorMessage = "" };
                        return statusOrderResponse;
                    }
                    try
                    {
                        Global_Object.doorManagementServiceCtrl.DoorMezzamineUp.LampOn(DoorType.DOOR_FRONT);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("control lamp failed"+e);
                    }
                }
                if (typeReq == (int)TyeRequest.TYPEREQUEST_FORLIFT_TO_MACHINE)
                {
                    if (PendingOrderList.Count == 0)
                    {
                        if (Global_Object.onFlagDoorBusy)
                        {
                            statusOrderResponse = new StatusOrderResponse() { status = (int)StatusOrderResponseCode.ORDER_STATUS_DOOR_BUSY, ErrorMessage = "" };
                            return statusOrderResponse;
                        }
                    }
                    else
                    {
                        statusOrderResponse = new StatusOrderResponse() { status = (int)StatusOrderResponseCode.ORDER_STATUS_DOOR_BUSY, ErrorMessage = "" };
                        return statusOrderResponse;
                    }
                    Global_Object.onFlagDoorBusy = true;
                    OrderItem order = new OrderItem();
                    order.typeReq = (TyeRequest)typeReq;
                    order.userName = (String)results["userName"];
                    order.productDetailId = (int)results["productDetailId"];
                    order.productDetailName = (String)results["productDetailName"];
                    order.productId = (int)results["productId"];
                    order.planId = (int)results["planId"];
                    order.deviceId = (int)results["deviceId"];
                    order.timeWorkId = (int)results["timeWorkId"];
                    order.activeDate = (string)results["activeDate"];
                    // order.palletStatus = (String)results["palletStatus"];
                    dynamic product = new JObject();
                    product.timeWorkId = order.timeWorkId;
                    product.activeDate = order.activeDate;
                    product.productId = order.productId;
                    product.productDetailId = order.productDetailId;

                    // chu y sua 
                    product.palletStatus = PalletStatus.P.ToString();
                    order.dataRequest = product.ToString();
                    order.status = StatusOrderResponseCode.PENDING;
                    order.dateTime = (string)DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt");
                    DataPallet datapallet = GetLineMachineInfo(order.deviceId);
                    if (datapallet != null)
                    {
                        order.palletAtMachine = datapallet;
                        PendingOrderList.Add(order);
                        OrderedItemList.Add(order);
                    }
                    else
                    {
                        statusOrderResponse = new StatusOrderResponse() { status = (int)StatusOrderResponseCode.ORDER_STATUS_RESPONSE_NOACCEPTED, ErrorMessage = "" };
                        return statusOrderResponse;
                    }
                    try
                    {
                        Global_Object.doorManagementServiceCtrl.DoorMezzamineUp.LampOn(DoorType.DOOR_FRONT);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("control lamp failed");
                    }
                }
                else if (typeReq == (int)TyeRequest.TYPEREQUEST_BUFFER_TO_MACHINE)
                {
                    Console.WriteLine(dataReq);
                    Console.WriteLine("-----------------------------");
                    int len = (int)results["length"];
                    int palletAmountInBuffer = (int)results["palletAmount"];
                    int productDetailId = (int)results["productDetailId"];
                    int cntOrderReg = 0;
                    int orderAmount = 0;
                    foreach (OrderItem ord in PendingOrderList)
                    {
                        if (productDetailId == ord.productDetailId)
                        {
                            if(ord.status==StatusOrderResponseCode.PENDING)
                            cntOrderReg++;
                        }
                    }

                    if (cntOrderReg == 0) // chưa có order với productdetailID nào uoc đăng ký. thêm vào đúng số lượng trong orderlist
                    {
                        if (len <= palletAmountInBuffer) // nếu số lượn yêu cầu nhỏ hơn bằng số pallet có trong buffer add vào orderlist 
                            orderAmount = len;
                    }
                    else if (cntOrderReg >= palletAmountInBuffer) // số lượng yêu cầu trước đó bằng hoặc hơn số lượng yêu cầu hiện tại. không duoc phép đưa vào thêm
                    {
                        statusOrderResponse = new StatusOrderResponse() { status = (int)StatusOrderResponseCode.ORDER_STATUS_RESPONSE_NOACCEPTED, ErrorMessage = "" };
                        return statusOrderResponse;
                    }
                    else if (cntOrderReg < palletAmountInBuffer) // số lượng yêu cầu hiện tại nhỏ hơn thì phải tính lại số lượng để bổ sung vào thêm
                    {
                        int availableOrder = palletAmountInBuffer - cntOrderReg; // tính số lượng pallet có thể 
                        int willOrder = availableOrder - len; // số lượng pallet sẽ duoc add thêm vào orederlist
                        if (willOrder >= 0)
                        {
                            orderAmount = len;
                        }
                        else
                        {
                            orderAmount =   len- availableOrder;
                        }

                    }
                    for (int i = 0; i < orderAmount; i++)
                    {

                        OrderItem order = new OrderItem();
                        order.typeReq = (TyeRequest)typeReq;
                        order.userName = (String)results["userName"];
                        order.productDetailId = (int)results["productDetailId"];
                        order.productDetailName = (String)results["productDetailName"];
                        order.productId = (int)results["productId"];
                        // order.planId = (int)results["planId"];
                        order.deviceId = (int)results["deviceId"];
                        order.timeWorkId = 1;
                        // order.activeDate = (string)DateTime.Now.ToString("yyyy-MM-dd");
                        // order.palletStatus = (String)results["palletStatus"];
                        String jsonDPst = (string)results["datapallet"][i];
                        JObject stuffPallet = JObject.Parse(jsonDPst);
                        double xx = (double)stuffPallet["line"]["x"];
                        double yy = (double)stuffPallet["line"]["y"];
                        double angle = (double)stuffPallet["line"]["angle"];
                        int row = (int)stuffPallet["pallet"]["row"];
                        int bay = (int)stuffPallet["pallet"]["bay"];
                        int directMain = (int)stuffPallet["pallet"]["dir_main"];
                        int directSub = (int)stuffPallet["pallet"]["dir_sub"];
                        int directOut = (int)stuffPallet["pallet"]["dir_out"];
                        int line_ord = (int)stuffPallet["pallet"]["line_ord"];
                        order.palletAtMachine = new DataPallet() { linePos = new Pose(xx, yy, angle), row = row, bay = bay, directMain = directMain, directSub = directSub, directOut = directOut, line_ord = line_ord };
                        dynamic product = new JObject();
                        product.timeWorkId = order.timeWorkId;
                        product.activeDate = order.activeDate;
                        order.dateTime = (string)DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt");
                        product.productId = order.productId;
                        product.productDetailId = order.productDetailId;
                        // chu y sua 
                        product.palletStatus = PalletStatus.W.ToString(); // W
                        order.dataRequest = product.ToString();

                        order.status = StatusOrderResponseCode.PENDING;
                        PendingOrderList.Add(order);
                        OrderedItemList.Add(order);
                    }

                }
                else if (typeReq == (int)TyeRequest.TYPEREQUEST_MACHINE_TO_RETURN)
                {
                    int len = (int)results["length"];

                    for (int i = 0; i < len; i++)
                    {
                        OrderItem order = new OrderItem();
                        order.typeReq = (TyeRequest)typeReq;
                        order.userName = (String)results["userName"];
                        order.activeDate = (string)results["activeDate"];
                        order.deviceId = (int)results["deviceId"];
                        //order.palletStatus = (String)results["palletStatus"];
                        String jsonDPst = (string)results["datapallet"][i];
                        JObject stuffPallet = JObject.Parse(jsonDPst);
                        double xx = (double)stuffPallet["line"]["x"];
                        double yy = (double)stuffPallet["line"]["y"];
                        double angle = (double)stuffPallet["line"]["angle"];
                        int row = (int)stuffPallet["pallet"]["row"];
                        int bay = (int)stuffPallet["pallet"]["bay"];
                        int directMain = (int)stuffPallet["pallet"]["dir_main"];
                        int directSub = (int)stuffPallet["pallet"]["dir_sub"];
                        int dir_out = (int)stuffPallet["pallet"]["dir_out"];
                        int line_ord = (int)stuffPallet["pallet"]["line_ord"];
                        order.dateTime = (string)DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt");
                        order.palletAtMachine = new DataPallet() { linePos = new Pose(xx, yy, angle), row = row, bay = bay, directMain = directMain, directSub = directSub, directOut = dir_out, line_ord = line_ord };
                        dynamic product = new JObject();
                        //   product.timeWorkId = order.timeWorkId;
                        //   product.activeDate = order.activeDate;
                        //   product.productId = order.productId;

                        // chu y sua 
                        product.palletStatus = PalletStatus.F.ToString();
                        order.dataRequest = product.ToString();
                        order.status = StatusOrderResponseCode.PENDING;
                        PendingOrderList.Add(order);
                        OrderedItemList.Add(order);

                    }
                }
                else if (typeReq == (int)TyeRequest.TYPEREQUEST_BUFFER_TO_RETURN)
                {
                    OrderItem order = new OrderItem();
                    order.typeReq = (TyeRequest)typeReq;
                    order.userName = (String)results["userName"];
                    order.deviceId = (int)results["deviceId"];
                    order.productDetailId = (int)results["productDetailId"];
                    order.productId = (int)results["productId"];
                    order.timeWorkId = (int)results["timeWorkId"];
                    order.activeDate = (string)results["activeDate"];
                    // order.palletStatus = (String)results["palletStatus"];
                    dynamic product = new JObject();
                    product.timeWorkId = order.timeWorkId;
                    product.activeDate = order.activeDate;
                    product.productId = order.productId;
                    product.productDetailId = order.productDetailId;
                    order.dateTime = (string)DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt");
                    // chu y sua 
                    product.palletStatus = PalletStatus.W.ToString();
                    order.dataRequest = product.ToString();
                    order.status = StatusOrderResponseCode.PENDING;
                    PendingOrderList.Add(order);
                    OrderedItemList.Add(order);
                }
                else if(typeReq == (int)TyeRequest.TYPEREQUEST_WMS_RETURNPALLET_BUFFER)
                {
                    OrderItem order = new OrderItem();
                    order.typeReq = (TyeRequest)typeReq;
                    order.userName = (String)results["userName"];
                    order.productDetailId = (int)results["productDetailId"];
                    order.productDetailName = (String)results["productDetailName"];
                    order.productId = (int)results["productId"];
                    // order.planId = (int)results["planId"];
                    order.deviceId = (int)results["deviceId"];
                    order.bufferId = (int)results["bufferId"];
                    order.timeWorkId = 1;
                    // order.activeDate = (string)DateTime.Now.ToString("yyyy-MM-dd");
                    // order.palletStatus = (String)results["palletStatus"];
                    dynamic product = new JObject();
                    product.timeWorkId = order.timeWorkId;
                    product.activeDate = order.activeDate;
                    order.dateTime = (string)DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt");
                    product.productId = order.productId;
                    product.productDetailId = order.productDetailId;
                    // chu y sua 
                    product.palletStatus = PalletStatus.R.ToString(); // W
                    order.dataRequest = product.ToString();
                    order.status = StatusOrderResponseCode.PENDING;
                    PendingOrderList.Add(order);
                    OrderedItemList.Add(order);
                }
                else if (typeReq == (int)TyeRequest.TYPEREQUEST_CLEAR)
                {
                    String userName = (String)results["userName"];
                    // kiểm tra quy trình và hủy task 

                    foreach (OrderItem ord in PendingOrderList)
                    {
                        if (ord.userName.Equals(userName))
                            PendingOrderList.Remove(ord);
                    }
                    foreach (OrderItem ord in OrderedItemList)
                    {
                        if (ord.userName.Equals(userName))
                            if (ord.status == StatusOrderResponseCode.PENDING)
                            {
                                ord.status = StatusOrderResponseCode.DESTROYED;
                            }
                    }

                }
                /* else if (typeReq == (int)TyeRequest.TYPEREQUEST_CLEAR_FORLIFT_TO_BUFFER)
                 {
                     String userName = (String)results["userName"];
                     int productDetailId = (int)results["productDetailId"];

                     // kiểm tra quy trình và hủy task 

                     foreach (OrderItem ord in PendingOrderList)
                     {
                         if (ord.productDetailId == productDetailId)
                         {
                             PendingOrderList.Remove(ord);
                             break;
                         }
                     }
                     foreach (OrderItem ord in OrderedItemList)
                     {
                         if (ord.productDetailId == productDetailId)
                             if (ord.status == StatusOrderResponseCode.PENDING)
                             {
                                 ord.status = StatusOrderResponseCode.DESTROYED;
                                 break;
                             }
                     }

                 }*/
                else if (typeReq == (int)TyeRequest.TYPEREQUEST_OPEN_FRONTDOOR_DELIVERY_PALLET)
                {
                    // same deviceID forklift
                    try
                    {
                        Global_Object.doorManagementServiceCtrl.DoorMezzamineUp.openDoor(DoorType.DOOR_FRONT);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("control door failed");
                        statusOrderResponse = new StatusOrderResponse() { status = (int)StatusOrderResponseCode.ORDER_STATUS_RESPONSE_ERROR_DATA, ErrorMessage = e.Message };
                        return statusOrderResponse;
                    }
                }
                else if (typeReq == (int)TyeRequest.TYPEREQUEST_CLOSE_FRONTDOOR_DELIVERY_PALLET)
                {
                    // same deviceID forklift
                    try
                    {
                        Global_Object.doorManagementServiceCtrl.DoorMezzamineUp.closeDoor(DoorType.DOOR_FRONT);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("control door failed");
                        statusOrderResponse = new StatusOrderResponse() { status = (int)StatusOrderResponseCode.ORDER_STATUS_RESPONSE_ERROR_DATA, ErrorMessage = e.Message };
                        return statusOrderResponse;
                    }
                }
                statusOrderResponse = new StatusOrderResponse() { status = (int)StatusOrderResponseCode.ORDER_STATUS_RESPONSE_SUCCESS, ErrorMessage = "" };
            }
            catch (Exception e)
            {
                statusOrderResponse = new StatusOrderResponse() { status = (int)StatusOrderResponseCode.ORDER_STATUS_RESPONSE_ERROR_DATA, ErrorMessage = e.Message };
                return statusOrderResponse;
            }
            try
            {
                String path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "RecoderDataOrder.txt");
                if (!File.Exists(path))
                    File.Create(path);
                if (File.ReadAllBytes(path).Length > 3000000) // lon 3M thi xoa bot
                {
                    String[] lines = File.ReadAllLines(path);
                    Array.Clear(lines, 0, 615); // xoa bot text 615 vong tuong ung 1M
                }
                File.AppendAllText(path, DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt >> ") + dataReq + Environment.NewLine + "[Response] >> " + (statusOrderResponse.status) + Environment.NewLine);
            }
            catch { }
            return statusOrderResponse;
        }
        public String RequestDataProcedure(String dataReq, String url)
        {
            //String url = Global_Object.url+"plan/getListPlanPallet";
            BridgeClientRequest clientRequest = new BridgeClientRequest();
            // String url = "http://localhost:8080";
            var data = clientRequest.PostCallAPI(url, dataReq);

            return data.Result;
        }

        public void RemoveCallBack(OrderItem item)
        {
            if (item.typeReq == TyeRequest.TYPEREQUEST_FORLIFT_TO_BUFFER)
            {
                if (item.status == StatusOrderResponseCode.PENDING)
                {
                    PendingOrderList.Remove(item);
                    OrderedItemList.Remove(item);
                    FreePlanedBuffer(item);
                }
            }
            else if (item.typeReq == TyeRequest.TYPEREQUEST_BUFFER_TO_MACHINE)
            {
                if (item.status == StatusOrderResponseCode.PENDING)
                {
                    PendingOrderList.Remove(item);
                    OrderedItemList.Remove(item);
                }
            }
        }
        public void ReorderCallBack(OrderItem item)
        {
            if (item.typeReq == TyeRequest.TYPEREQUEST_FORLIFT_TO_BUFFER)
            {
                if (item.status == StatusOrderResponseCode.DESTROYED || item.status == StatusOrderResponseCode.NO_BUFFER_DATA || item.status == StatusOrderResponseCode.ROBOT_ERROR)
                {
                    PendingOrderList.Add(item);
                    OrderedItemList.Add(item);
                    CreatePlanBuffer(item);
                }
            }
            else if (item.typeReq == TyeRequest.TYPEREQUEST_BUFFER_TO_MACHINE)
            {
                // if (item.status == StatusOrderResponseCode.PENDING)
                {
                    PendingOrderList.Add(item);
                    OrderedItemList.Add(item);
                }
            }
        }
        public String CreatePlanBuffer(OrderItem order)
        {
            dynamic product = new JObject();
            product.timeWorkId = 1;
            product.activeDate = order.activeDate;
            product.productId = order.productId;
            product.productDetailId = order.productDetailId;
            product.updUsrId = Global_Object.userLogin;
            product.deviceId = order.deviceId;
            product.palletAmount = 1;
            String response = RequestDataProcedure(product.ToString(), Global_Object.url + "plan/createPlanPallet");
            return response;
        }

        public void FreePlanedBuffer(OrderItem order)
        {
            String url = Global_Object.url + "pallet/updatePalletStatus";
            int _palletId = GetPalletId(order);
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

        public DataPallet GetLineMachineInfo(int deviceId)
        {
            try
            {
                dynamic product = new JObject();
                product.deviceId = deviceId;
                Pose poseTemp = null;
                String collectionData = RequestDataProcedure(product.ToString(), Global_Object.url + "/device/getListDevicePallet");


                if (collectionData.Length > 0)
                {
                    JArray results = JArray.Parse(collectionData);
                    var result = results[0];
                    String jsonDPst = (string)result["dataPallet"];
                    JObject stuffPallet = JObject.Parse(jsonDPst);
                    double xx = (double)stuffPallet["line"]["x"];
                    double yy = (double)stuffPallet["line"]["y"];
                    double angle = (double)stuffPallet["line"]["angle"];
                    int row = (int)stuffPallet["pallet"]["row"];
                    int bay = (int)stuffPallet["pallet"]["bay"];
                    int directMain = (int)stuffPallet["pallet"]["dir_main"];
                    int directSub = (int)stuffPallet["pallet"]["dir_sub"];
                    int directOut = (int)stuffPallet["pallet"]["dir_out"];
                    int line_ord = (int)stuffPallet["pallet"]["line_ord"];
                    return new DataPallet() { linePos = new Pose(xx, yy, angle), row = row, bay = bay, directMain = directMain, directSub = directSub, directOut = directOut, line_ord = line_ord };

                }
            }
            catch { Console.WriteLine("Error in DeviceIte at GetLineMachineInfo"); }
            return null;
        }
        public int GetPalletId(OrderItem order)
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
                        foreach (var result in results)
                        {
                            int temp_planId = (int)result["planId"];
                            if (temp_planId == order.planId)
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
            }
            catch
            {
                Console.WriteLine("Errror Get palletID");
            }
            return palletId;
        }

    }

}
/*
 * {
  "deviceId": "1",
  "productId": "4",
  "productDetailId": "16",
  "typeReq": "2",
  "userName": "tab1",
  "planId": 1,
  "activeDate": "2018-12-25",
  "length":3,
  "datapallet": [
    {
      "line":{"X":1,"X":1,Angle:""},
      "pallet": {"row":1,"bay":2,"direct":1}
    },
    {
      "line":{"X":1,"X":1,Angle:""},
      "pallet": {"row":1,"bay":2,"direct":1}
    },
    {
      "line":{"X":1,"X":1,Angle:""},
      "pallet": {"row":1,"bay":2,"direct":1}
    }
  ]
}
 */
