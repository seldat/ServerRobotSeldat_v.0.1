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
    public class AssigmentTaskService:TaskRounterService
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
            Task threadprocessAssignAnTaskWait=new Task(AssignTask);
            Task threadprocessAssignTaskReady =new Task(AssignTaskAtReady);
            threadprocessAssignAnTaskWait.Start();
            threadprocessAssignTaskReady.Start();
        }
        public void Dispose()
        {
            Alive = false;
        }
        public void AssignTask()
        {
            OrderItem orderItem = null;
            RobotUnity robot = null;
            int cntOrderNull=1;
            while (Alive)
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
                                robot = result.robot;
                                if (result.onReristryCharge)
                                {
                                    // registry charge procedure
                                    procedureService.Register(ProcedureItemSelected.PROCEDURE_ROBOT_TO_READY, robot, null);
                                    robotManageService.RemoveRobotUnityWaitTaskList(robot);
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

                            orderItem = Gettask();

                            if (orderItem != null)
                            {
                                processAssignAnTaskWait = ProcessAssignAnTaskWait.PROC_ANY_ASSIGN_ANTASK;
                                orderItem.robot = robot.properties.Label;
                                cntOrderNull = 0;
                                break;
                            }
                            else
                            {
                                MoveElementToEnd();
                                cntOrderNull++;
                            }
                            if (cntOrderNull > deviceItemsList.Count) // khi robot không còn nhận duoc task
                            {
                                    //processAssignAnTaskWait = ProcessAssignAnTaskWait.PROC_ANY_GET_ANROBOT_IN_WAITTASKLIST; // remove
                                    processAssignAnTaskWait = ProcessAssignAnTaskWait.PROC_ANY_CHECK_ROBOT_GOTO_READY; // mở lại 
                                        cntOrderNull = 0;
                            }
                            else
                            {
                                processAssignAnTaskWait = ProcessAssignAnTaskWait.PROC_ANY_GET_ANROBOT_IN_WAITTASKLIST;
                            }
                        break;
                case ProcessAssignAnTaskWait.PROC_ANY_CHECK_ROBOT_GOTO_READY:
                        robot.TurnOnSupervisorTraffic(true);
                        procedureService.Register(ProcedureItemSelected.PROCEDURE_ROBOT_TO_READY, robot, null);
                        robotManageService.RemoveRobotUnityWaitTaskList(robot);
                        processAssignAnTaskWait = ProcessAssignAnTaskWait.PROC_ANY_GET_ANROBOT_IN_WAITTASKLIST;
                    break;
                case ProcessAssignAnTaskWait.PROC_ANY_ASSIGN_ANTASK:
                        robot.TurnOnSupervisorTraffic(true);
                        SelectProcedureItem(robot, orderItem);
                        // xoa order đầu tiên trong danh sach devicelist[0] sau khi gán task
                        deviceItemsList[0].RemoveFirstOrder();
                        MoveElementToEnd(); // sort Task List
                        // xoa khoi list cho
                        robotManageService.RemoveRobotUnityWaitTaskList(robot);
                        processAssignAnTaskWait = ProcessAssignAnTaskWait.PROC_ANY_GET_ANROBOT_IN_WAITTASKLIST;
                        break;

                }
#endif
                Thread.Sleep(500);
            }
        }
        public void SelectProcedureItem(RobotUnity robot,OrderItem orderItem)
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
            // procedure;
        }
        public void AssignTaskAtReady()
        {
            OrderItem orderItem=null;
            RobotUnity robot = null;
                while (Alive)
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
                                robot = result.robot;
                                if (result.onReristryCharge)
                                {
                                    // registry charge procedure
                                    procedureService.Register(ProcedureItemSelected.PROCEDURE_ROBOT_TO_CHARGE, robot, null);
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
                            orderItem = Gettask();
                            if (orderItem != null)
                            {
                                Console.WriteLine(processAssignTaskReady);
                                orderItem.robot = robot.properties.Label;
                                processAssignTaskReady = ProcessAssignTaskReady.PROC_READY_SET_TRAFFIC_RISKAREA_ON;
                            }
                            else
                            {
                                MoveElementToEnd();
                                processAssignTaskReady = ProcessAssignTaskReady.PROC_READY_GET_ANROBOT_INREADYLIST;
                            }
                            break;
                        case ProcessAssignTaskReady.PROC_READY_ASSIGN_ANTASK:
                           // if (!trafficService.HasRobotUnityinArea("READY"))
                            {
                                robot.TurnOnSupervisorTraffic(true);
                                Console.WriteLine(processAssignTaskReady);
                                SelectProcedureItem(robot, orderItem);
                                deviceItemsList[0].RemoveFirstOrder();
                                MoveElementToEnd(); // sort Task List
                                processAssignTaskReady = ProcessAssignTaskReady.PROC_READY_CHECK_ROBOT_OUTSIDEREADY;
                            }
                            break;
                        case ProcessAssignTaskReady.PROC_READY_SET_TRAFFIC_RISKAREA_ON:
                            robot.TurnOnSupervisorTraffic(true);
                            processAssignTaskReady = ProcessAssignTaskReady.PROC_READY_ASSIGN_ANTASK;
                            break;
                        case ProcessAssignTaskReady.PROC_READY_CHECK_ROBOT_OUTSIDEREADY:

                        // kiem tra robot vẫn còn tai vung ready
                             if(!trafficService.RobotIsInArea("READY",robot.properties.pose.Position))
                            {
                                    // xoa khoi list cho
                                robotManageService.RemoveRobotUnityReadyList(robot);
                                processAssignTaskReady = ProcessAssignTaskReady.PROC_READY_GET_ANROBOT_INREADYLIST;
                            }

                            break;
                    }
                    Thread.Sleep(500);
                }
         
        }
        public void AssignTaskGoToReady(RobotUnity robot)
        {
            procedureService.Register(ProcedureItemSelected.PROCEDURE_ROBOT_TO_READY, robot, null);
        }

    }
}
