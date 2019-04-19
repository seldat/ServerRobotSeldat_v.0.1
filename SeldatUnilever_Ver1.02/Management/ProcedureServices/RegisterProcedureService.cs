using DoorControllerService;
using SeldatMRMS.Management;
using SeldatMRMS.Management.DoorServices;
using SeldatMRMS.Management.RobotManagent;
using SeldatMRMS.Management.TrafficManager;
using SelDatUnilever_Ver1._00.Management.ChargerCtrl;
using SelDatUnilever_Ver1._00.Management.DeviceManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SeldatMRMS.DBProcedureService;
using static SelDatUnilever_Ver1._00.Management.DeviceManagement.DeviceItem;

namespace SeldatMRMS
{
    public class RegisterProcedureService
    {
        public DoorManagementService doorService;
        //protected DoorManagementService doorService;
        protected ChargerManagementService chargerService;
        protected TrafficManagementService trafficService;
        protected DeviceRegistrationService deviceService;
        protected RobotManagementService robotManagementService;
        protected virtual bool Cancel() { return false; }

        public class RegisterProcedureItem
        {
            public OrderItem orderItem;
            public ProcedureControlServices item;
              public ProcedureDataItems procedureDataItems;
            public RobotUnity robot;
            public static bool currentErrorStatus = false;
            public void Start()
            {
                //if(itemProcService != null)
                    //itemProcService.
            }
            public void Stop()
            {
                //if(itemProcService != null)
                //itemProcService.
            }
            public void Dispose()
            {
                //if(itemProcService != null)
                //itemProcService.
            }
        }

      //  protected List<RegisterProcedureItem> RegisterProcedureItemList = new List<RegisterProcedureItem>();
        public RegisterProcedureService() { }
        public enum ProcedureItemSelected
        {
            PROCEDURE_FORLIFT_TO_BUFFER = 0,
            PROCEDURE_BUFFER_TO_MACHINE,
            PROCEDURE_BUFFER_TO_RETURN,
            PROCEDURE_MACHINE_TO_RETURN,
            PROCEDURE_ROBOT_TO_READY,
            PROCEDURE_ROBOT_READY_TO_READY,
            PROCEDURE_ROBOT_TO_CHARGE,
            PROCEDURE_RETURN_TO_GATE,
            PROCEDURE_FORLIFT_TO_MACHINE
        }
        public void RegistryService(TrafficManagementService trafficService)
        {
            this.trafficService = trafficService;
        }
        public void RegistryService(RobotManagementService robotManagementService)
        {
            this.robotManagementService = robotManagementService;
        }
        public void RegistryService(DoorManagementService doorService)
        {
            this.doorService = doorService;
        }
        public void RegistryService(ChargerManagementService chargerService)
        {
            this.chargerService = chargerService;
        }
        public void RegistryService(DeviceRegistrationService deviceService)
        {
            this.deviceService = deviceService;
        }
        public void StoreProceduresInDataBase()
        {

        }
        public void RegisteAnItem(ProcedureControlServices item, ProcedureDataItems procedureDataItems, RobotUnity robot)
        {


        }
        protected virtual void ReleaseProcedureItemHandler(Object  item)
        {
           /* Task.Run(() =>
            {
                var element = RegisterProcedureItemList.Find(e => e.item == item);
                element.procedureDataItems.EndTime = DateTime.Now;
                element.procedureDataItems.StatusProcedureDelivered = "OK";
                RegisterProcedureItemList.Remove(element);
            });*/
        }
        protected virtual void ErrorApprearInProcedureItem(Object item)
        {
            /*
            // chờ xử lý // error staus is true;
            // báo sự cố cho lớp robotmanagement // đợi cho chờ xử lý// hủy bỏ quy trình 
            // add order lại list device
            RestoreOrderItem(item.order);
             */
        }
        public void RestoreOrderItem(OrderItem item)
        {
            deviceService.FindDeviceItem(item.userName).AddOrder(item);
        }
        protected virtual void AskPriority() { }
    }
}
