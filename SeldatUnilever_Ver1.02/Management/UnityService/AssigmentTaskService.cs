using SeldatMRMS;
using SeldatMRMS.Management.RobotManagent;
using SelDatUnilever_Ver1._00.Management.DeviceManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
            Task threadprocessAssignAnTaskWait = new Task(MainProcessAssignTask_Wait);
            Task threadprocessAssignTaskReady = new Task(MainProcessAssignTask_Ready);


            threadprocessAssignAnTaskWait.Start();
            threadprocessAssignTaskReady.Start();
      
        }
        public void Dispose()
        {
            Alive = false;
        }
        public void Stop()
        {
            Alive = false;
        }
        public void MainProcessAssignTask_Wait()
        {
            while(Alive)
            {
                AssignWaitTask();
              //  Task.Delay(500);
            }
        }
        public void MainProcessAssignTask_Ready()
        {
            while (Alive)
            {
           
                AssignTaskAtReady();
                //  Task.Delay(500);
            }
        }
        OrderItem orderItem_wait = null;
        RobotUnity robotwait = null;
        int cntOrderNull_wait = 1;
        public void AssignWaitTask()
        {
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
                                    if (DetermineAmoutOfDeviceToAssignAnTask() > 0)
                                    {
                                        processAssignAnTaskWait = ProcessAssignAnTaskWait.PROC_ANY_CHECK_HAS_ANTASK;
                                    }
                                    else
                                    {
                                        processAssignAnTaskWait = ProcessAssignAnTaskWait.PROC_ANY_CHECK_ROBOT_GOTO_READY; // mở lại 
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
                                if (orderItem_wait.typeReq == TyeRequest.TYPEREQUEST_FORLIFT_TO_BUFFER || orderItem_wait.typeReq == TyeRequest.TYPEREQUEST_FORLIFT_TO_MACHINE)
                                {

                                    if (DetermineRobotWorkInGate())
                                    {
                                        MoveElementToEnd();
                                        cntOrderNull_wait++;
                                        break;
                                    }
                                }
                                else
                                {
                                    /*if (DetermineAmoutOfDeviceToAssignAnTask() > 0)
                                    {
                                        if (FindRobotUnitySameOrderItem(orderItem_wait.userName))
                                        {
                                            MoveElementToEnd();
                                            cntOrderNull_wait++;
                                            break;
                                        }
                                    }*/
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

                       
                        if (orderItem_wait != null)
                        {
                            if (!orderItem_wait.onAssiged) //kiem tra da gan task
                            {
                                orderItem_wait.onAssiged = true;
                                processAssignAnTaskWait = ProcessAssignAnTaskWait.PROC_ANY_ASSIGN_ANTASK;
                                orderItem_wait.robot = robotwait.properties.Label;
                                robotwait.orderItem = orderItem_wait;
                                cntOrderNull_wait = 0;
                            }
                            else
                            {
                                MoveElementToEnd();
                                processAssignAnTaskWait = ProcessAssignAnTaskWait.PROC_ANY_GET_ANROBOT_IN_WAITTASKLIST;
                            }
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
               Thread.Sleep(200);
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
                                    if (DetermineAmoutOfDeviceToAssignAnTask() > 0)
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

                                if (orderItem_ready.typeReq == TyeRequest.TYPEREQUEST_FORLIFT_TO_BUFFER || orderItem_ready.typeReq == TyeRequest.TYPEREQUEST_FORLIFT_TO_MACHINE)
                                {
                                    if (DetermineRobotWorkInGate())
                                    {
                                        MoveElementToEnd();
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                processAssignTaskReady = ProcessAssignTaskReady.PROC_READY_GET_ANROBOT_INREADYLIST;
                                break;
                            }
                        }

                        if (orderItem_ready != null)
                        {
                            if (!orderItem_ready.onAssiged)
                            {
                                orderItem_ready.onAssiged = true;
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
                        }
                        else
                        {
                            MoveElementToEnd();
                            processAssignTaskReady = ProcessAssignTaskReady.PROC_READY_GET_ANROBOT_INREADYLIST;
                        }
                        break;
                    case ProcessAssignTaskReady.PROC_READY_ASSIGN_ANTASK:
                        if (!robotatready.CheckRobotWorkinginReady() )
                        {
                            //  if (!trafficService.HasRobotUnityinArea("RD5") && !trafficService.HasRobotUnityinArea("OPA3") && !trafficService.HasRobotUnityinArea("READY") )
                            if (!trafficService.HasRobotUnityinArea("ATR"))
                            {
                                robotatready.TurnOnSupervisorTraffic(true);
                                Console.WriteLine(processAssignTaskReady);
                                SelectProcedureItem(robotatready, orderItem_ready);
                                deviceItemsList[0].RemoveFirstOrder();
                                MoveElementToEnd(); // sort Task List
                                processAssignTaskReady = ProcessAssignTaskReady.PROC_READY_CHECK_ROBOT_OUTSIDEREADY;
                            }
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
                Thread.Sleep(200);
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
        public bool DetermineRobotWorkInGate()
        {
         
                if (!Global_Object.onFlagRobotComingGateBusy )
                {
                   // Global_Object.onFlagDoorBusy = true;
                    Global_Object.onFlagRobotComingGateBusy = true;
                    return false;
                }
                else
                    return true;
            
        }
        public int DetermineAmoutOfDeviceToAssignAnTask()
        {
            try
            {
                int cntOrderWeight = 0;
                if (deviceItemsList.Count > 0)
                {
                    foreach (DeviceItem item in deviceItemsList)
                    {
                        if (item.PendingOrderList.Count > 0)
                        {
                            cntOrderWeight++;
                        }
                    }
                    if (cntOrderWeight > 0) // có nhiều device đang có task
                    {
                        return 1; // 
                    }
                }
            }
            catch
            {
                MessageBox.Show("Loi Cap Task");
            }
            return -1; // chỉ có 1 device đang có task
        }
        public void AssignTaskGoToReady(RobotUnity robot)
        {
            procedureService.Register(ProcedureItemSelected.PROCEDURE_ROBOT_TO_READY, robot, null);
        }

    }
}
