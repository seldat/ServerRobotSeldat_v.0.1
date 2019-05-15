using SeldatMRMS;
using SeldatMRMS.Management.RobotManagent;
using SelDatUnilever_Ver1._00.Management.DeviceManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DoorControllerService.DoorService;
using static SeldatMRMS.Management.RobotManagent.RobotManagementService;
using static SeldatMRMS.RegisterProcedureService;
using static SelDatUnilever_Ver1._00.Management.DeviceManagement.DeviceItem;

namespace SelDatUnilever_Ver1._00.Management.UnityService
{
    public class AssigmentTaskService : TaskRounterService
    {

        public AssigmentTaskService() { }
        public void FinishTask(String userName)
        {
            var item = deviceItemsList.Find(e => e.userName == userName);
            item.RemoveFirstOrder();
        }
        public void Start()
        {
            Alive = true;
            processAssignAnTaskWait = ProcessAssignAnTaskWait.PROC_ANY_GET_ANROBOT_IN_WAITTASKLIST;
            processAssignTaskReady = ProcessAssignTaskReady.PROC_READY_GET_ANROBOT_INREADYLIST;
           // Task threadprocessAssignAnTaskWait = new Task(AssignTask);
          //  Task threadprocessAssignTaskReady = new Task(AssignTaskAtReady);

            Task threadprocess = new Task(MainProcessAssignTask);
            //threadprocessAssignAnTaskWait.Start();
            //threadprocessAssignTaskReady.Start();
            threadprocess.Start();
        }
        public void Dispose()
        {
            Alive = false;
        }
        public void MainProcessAssignTask()
        {
            while(Alive)
            {
                AssignTask();
                AssignTaskAtReady();
                Task.Delay(500);
            }
        }
        OrderItem orderItem_wait = null;
        RobotUnity robotwait = null;
        int cntOrderNull_wait = 1;
        public void AssignTask()
        {

           
           // while (Alive)
            {

#if false
                //procedureService.doorService.DoorMezzamineUp.Open(DoorType.DOOR_FRONT);
                //Thread.Sleep(3000);
                //procedureService.doorService.DoorMezzamineUp.Close(DoorType.DOOR_FRONT);
                //Thread.Sleep(3000);
                //procedureService.doorService.DoorMezzamineUp.Open(DoorType.DOOR_BACK);
                //Thread.Sleep(3000);
                //procedureService.doorService.DoorMezzamineUp.Close(DoorType.DOOR_BACK);
                //Thread.Sleep(3000);
                //procedureService.doorService.DoorMezzamineReturn.Open(DoorType.DOOR_FRONT);
                //Thread.Sleep(1000);
                //procedureService.doorService.DoorMezzamineReturn.Close(DoorType.DOOR_FRONT);
                //Thread.Sleep(1000);
                //procedureService.doorService.DoorMezzamineReturn.Open(DoorType.DOOR_BACK);
                //Thread.Sleep(1000);
                //procedureService.doorService.DoorMezzamineReturn.Close(DoorType.DOOR_BACK);
                //Thread.Sleep(1000);
                procedureService.doorService.DoorMezzamineUp.LampOn(DoorType.DOOR_FRONT);
                Thread.Sleep(2000);
                procedureService.doorService.DoorMezzamineUp.LampOff(DoorType.DOOR_FRONT);
                Thread.Sleep(2000);
#else
                //Console.WriteLine(processAssignAnTaskWait);
                switch (processAssignAnTaskWait)
                {
                    case ProcessAssignAnTaskWait.PROC_ANY_IDLE:
                        break;
                    case ProcessAssignAnTaskWait.PROC_ANY_GET_ANROBOT_IN_WAITTASKLIST:
                        if (robotManageService.RobotUnityWaitTaskList.Count > 0)
                        {
                            ResultRobotReady result = robotManageService.GetRobotUnityWaitTaskItem0();
                            if (result != null)
                            {
                                robotwait = result.robot;
                                if (result.onReristryCharge)
                                {
                                    // registry charge procedure
                                    procedureService.Register(ProcedureItemSelected.PROCEDURE_ROBOT_TO_READY, robotwait, null);
                                    robotManageService.RemoveRobotUnityWaitTaskList(robotwait);
                                }
                                else
                                {
                                    if (deviceItemsList.Count > 0)
                                    {
                                        processAssignAnTaskWait = ProcessAssignAnTaskWait.PROC_ANY_CHECK_HAS_ANTASK;
                                    }

                                }
                            }
                        }
                        break;
                    case ProcessAssignAnTaskWait.PROC_ANY_CHECK_HAS_ANTASK:

                        orderItem_wait = Gettask();
                        if (orderItem_wait != null)
                        {
                            if (robotwait != null)
                            {

                                if (DetermineRobotWorkInGate(orderItem_wait.typeReq, robotwait.properties.NameId))
                                {
                                    MoveElementToEnd();
                                    cntOrderNull_wait++;
                                    break;
                                }
                            }
                            else
                            {
                                processAssignAnTaskWait = ProcessAssignAnTaskWait.PROC_ANY_GET_ANROBOT_IN_WAITTASKLIST;
                                break;
                            }
                        }
                        // xác định số lượng device đang có task và chỉ phân phối duy nhất 1 task cho một robot trên cùng thời điểm, không có trường hợp nhiểu
                        // device có task mà nhiều robot cùng nhận task đó
                        if (DetermineAmoutOfDeviceToAssignAnTask() > 0)
                        {
                            if (FindRobotUnitySameOrderItem(orderItem_wait.userName))
                            {
                                MoveElementToEnd();
                                break;
                            }
                        }
                        if (orderItem_wait != null)
                        {
                            processAssignAnTaskWait = ProcessAssignAnTaskWait.PROC_ANY_ASSIGN_ANTASK;
                            orderItem_wait.robot = robotwait.properties.Label;
                            robotwait.orderItem = orderItem_wait;
                            cntOrderNull_wait = 0;
                            break;
                        }
                        else
                        {
                            MoveElementToEnd();
                            cntOrderNull_wait++;
                        }
                        if (cntOrderNull_wait > deviceItemsList.Count) // khi robot không còn nhận duoc task
                        {
                            //processAssignAnTaskWait = ProcessAssignAnTaskWait.PROC_ANY_GET_ANROBOT_IN_WAITTASKLIST; // remove
                            processAssignAnTaskWait = ProcessAssignAnTaskWait.PROC_ANY_CHECK_ROBOT_GOTO_READY; // mở lại 
                            cntOrderNull_wait = 0;
                        }
                        else
                        {
                            processAssignAnTaskWait = ProcessAssignAnTaskWait.PROC_ANY_GET_ANROBOT_IN_WAITTASKLIST;
                        }
                        break;
                    case ProcessAssignAnTaskWait.PROC_ANY_CHECK_ROBOT_GOTO_READY:
                        robotwait.TurnOnSupervisorTraffic(true);
                        procedureService.Register(ProcedureItemSelected.PROCEDURE_ROBOT_TO_READY, robotwait, null);
                        robotManageService.RemoveRobotUnityWaitTaskList(robotwait);
                        processAssignAnTaskWait = ProcessAssignAnTaskWait.PROC_ANY_GET_ANROBOT_IN_WAITTASKLIST;
                        break;
                    case ProcessAssignAnTaskWait.PROC_ANY_ASSIGN_ANTASK:
                        robotwait.TurnOnSupervisorTraffic(true);
                        SelectProcedureItem(robotwait, orderItem_wait);
                        // xoa order đầu tiên trong danh sach devicelist[0] sau khi gán task
                        deviceItemsList[0].RemoveFirstOrder();
                        MoveElementToEnd(); // sort Task List
                        // xoa khoi list cho
                        robotManageService.RemoveRobotUnityWaitTaskList(robotwait);
                        processAssignAnTaskWait = ProcessAssignAnTaskWait.PROC_ANY_GET_ANROBOT_IN_WAITTASKLIST;
                        break;

                }
#endif
                Thread.Sleep(500);
            }
        }
        public void SelectProcedureItem(RobotUnity robot, OrderItem orderItem)
        {
            if (orderItem.typeReq == DeviceItem.TyeRequest.TYPEREQUEST_FORLIFT_TO_BUFFER)
            {
                procedureService.Register(ProcedureItemSelected.PROCEDURE_FORLIFT_TO_BUFFER, robot, orderItem);
            }
            else if (orderItem.typeReq == DeviceItem.TyeRequest.TYPEREQUEST_BUFFER_TO_MACHINE)
            {
                procedureService.Register(ProcedureItemSelected.PROCEDURE_BUFFER_TO_MACHINE, robot, orderItem);
            }
            else if (orderItem.typeReq == DeviceItem.TyeRequest.TYPEREQUEST_MACHINE_TO_RETURN)
            {
                procedureService.Register(ProcedureItemSelected.PROCEDURE_MACHINE_TO_RETURN, robot, orderItem);
            }
            else if (orderItem.typeReq == DeviceItem.TyeRequest.TYPEREQUEST_BUFFER_TO_RETURN)
            {
                procedureService.Register(ProcedureItemSelected.PROCEDURE_BUFFER_TO_RETURN, robot, orderItem);
            }
            else if (orderItem.typeReq == DeviceItem.TyeRequest.TYPEREQUEST_FORLIFT_TO_MACHINE)
            {
                procedureService.Register(ProcedureItemSelected.PROCEDURE_FORLIFT_TO_MACHINE, robot, orderItem);
            }
            else if (orderItem.typeReq == DeviceItem.TyeRequest.TYPEREQUEST_WMS_RETURNPALLET_BUFFER)
            {
                procedureService.Register(ProcedureItemSelected.PROCEDURE_BUFFER_TO_RETURN, robot, orderItem);
            }
            // procedure;
        }
         OrderItem orderItem_ready = null;
            RobotUnity robotatready = null;
        public void AssignTaskAtReady()
        {
           
            //while (Alive)
            {

                //Console.WriteLine(processAssignTaskReady);
                switch (processAssignTaskReady)
                {
                    case ProcessAssignTaskReady.PROC_READY_IDLE:
                        break;
                    case ProcessAssignTaskReady.PROC_READY_GET_ANROBOT_INREADYLIST:

                        if (robotManageService.RobotUnityReadyList.Count > 0)
                        {
                            ResultRobotReady result = robotManageService.GetRobotUnityReadyItem0();
                            if (result != null)
                            {
                                robotatready = result.robot;
                                if (result.onReristryCharge)
                                {
                                    // registry charge procedure
                                    procedureService.Register(ProcedureItemSelected.PROCEDURE_ROBOT_TO_CHARGE, robotatready, null);
                                }
                                else
                                {
                                    //
                                    if (deviceItemsList.Count > 0)
                                    {
                                        processAssignTaskReady = ProcessAssignTaskReady.PROC_READY_CHECK_HAS_ANTASK;
                                    }
                                }
                            }
                        }
                        break;
                    case ProcessAssignTaskReady.PROC_READY_CHECK_HAS_ANTASK:
                        orderItem_ready = Gettask();
                        if (orderItem_ready != null)
                        {
                            if (robotatready != null)
                            {
                                if (DetermineRobotWorkInGate(orderItem_ready.typeReq, robotatready.properties.NameId))
                                {
                                    MoveElementToEnd();
                                    break;
                                }
                            }
                            else
                            {
                                processAssignTaskReady = ProcessAssignTaskReady.PROC_READY_GET_ANROBOT_INREADYLIST;
                                break;
                            }
                        }
                // xác định số lượng device đang có task và chỉ phân phối duy nhất 1 task cho một robot trên cùng thời điểm, không có trường hợp nhiểu
                // device có task mà nhiều robot cùng nhận task đó
                    if (DetermineAmoutOfDeviceToAssignAnTask()>0)
                        {
                            if(FindRobotUnitySameOrderItem(orderItem_ready.userName))
                            {
                                MoveElementToEnd();
                               break;
                            }
                        }
                        if (orderItem_ready != null)
                        {
                            Console.WriteLine(processAssignTaskReady);
                            orderItem_ready.robot = robotatready.properties.Label;
                            robotatready.orderItem = orderItem_ready;
                            processAssignTaskReady = ProcessAssignTaskReady.PROC_READY_SET_TRAFFIC_RISKAREA_ON;
                        }
                        else
                        {
                            MoveElementToEnd();
                            processAssignTaskReady = ProcessAssignTaskReady.PROC_READY_GET_ANROBOT_INREADYLIST;
                        }
                        break;
                    case ProcessAssignTaskReady.PROC_READY_ASSIGN_ANTASK:
                        if (!robotatready.CheckRobotWorkinginReady() || !trafficService.HasRobotUnityinArea("RD5"))
                        {
                            robotatready.TurnOnSupervisorTraffic(true);
                            Console.WriteLine(processAssignTaskReady);
                            SelectProcedureItem(robotatready, orderItem_ready);
                            deviceItemsList[0].RemoveFirstOrder();
                            MoveElementToEnd(); // sort Task List
                            processAssignTaskReady = ProcessAssignTaskReady.PROC_READY_CHECK_ROBOT_OUTSIDEREADY;
                        }
                        break;
                    case ProcessAssignTaskReady.PROC_READY_SET_TRAFFIC_RISKAREA_ON:
                        robotatready.TurnOnSupervisorTraffic(true);
                        processAssignTaskReady = ProcessAssignTaskReady.PROC_READY_ASSIGN_ANTASK;
                        break;
                    case ProcessAssignTaskReady.PROC_READY_CHECK_ROBOT_OUTSIDEREADY:

                        // kiem tra robot vẫn còn tai vung ready
                        if (!trafficService.RobotIsInArea("READY", robotatready.properties.pose.Position))
                        {
                            // xoa khoi list cho
                            robotManageService.RemoveRobotUnityReadyList(robotatready);
                            processAssignTaskReady = ProcessAssignTaskReady.PROC_READY_GET_ANROBOT_INREADYLIST;
                        }

                        break;
                }
                Thread.Sleep(500);
            }

        }
        public bool FindRobotUnitySameOrderItem(String userName)
        {
            bool hasRobotSameOrderItem = false;
            foreach(RobotUnity robot in robotManageService.RobotUnityRegistedList.Values)
            {
                if(robot.orderItem!=null)
                {
                    if(robot.orderItem.userName.Equals(userName))
                    {
                        hasRobotSameOrderItem = true; ;
                        break;
                    }
                }
            }
            return hasRobotSameOrderItem;
        }
        public bool DetermineRobotWorkInGate(TyeRequest tyeRequest,String nameid)
        {
            if (tyeRequest == TyeRequest.TYPEREQUEST_FORLIFT_TO_BUFFER || tyeRequest == TyeRequest.TYPEREQUEST_FORLIFT_TO_MACHINE)
            {
                if (!Global_Object.onFlagRobotComingGateBusy)
                {
                    Global_Object.onFlagRobotComingGateBusy = true;
                }
                else
                    return true;
            }
            return false;
        }
        public int DetermineAmoutOfDeviceToAssignAnTask()
        {
           
            int cntOrderWeight = 0;
            if(deviceItemsList.Count>1)
            {
                foreach(DeviceItem item in deviceItemsList)
                {
                    if(item.PendingOrderList.Count>0)
                    {
                        cntOrderWeight++;
                    }
                }
                if(cntOrderWeight>1) // có nhiều device đang có task
                {
                    return 1; // 
                }
            }
            return 0; // chỉ có 1 device đang có task
        }
        public void AssignTaskGoToReady(RobotUnity robot)
        {
            procedureService.Register(ProcedureItemSelected.PROCEDURE_ROBOT_TO_READY, robot, null);
        }

    }
}
