using SeldatMRMS.Management.RobotManagent;
using System;

namespace SeldatMRMS
{
    public class ControlService:DBProcedureService
    {
       public ControlService(RobotUnity robot):base(robot)
       {
            if (robot != null)
            {
               // robot.ZoneHandler += ZoneHandler;
                robot.FinishStatesCallBack += FinishStatesCallBack;
               // robot.AmclPoseHandler += AmclPoseHandler;
                //if(doorService!=null)
                //    doorService.ReceiveRounterEvent += ReceiveRounterEvent;
            }
            //if(doorService!=null)
            //{

            //}
       }
       // robot control
       public virtual void ZoneHandler(Communication.Message message) { }
       public virtual void FinishStatesCallBack(Int32 message) { }
       public virtual void AmclPoseHandler(Communication.Message message) { }
       public virtual void CtrlRobotSpeed() { }
       public virtual void MoveBaseGoal() { }
       public virtual void AcceptDoSomething() { }
        // door control
       public virtual void ReceiveRounterEvent(String message) { }
    }
}
