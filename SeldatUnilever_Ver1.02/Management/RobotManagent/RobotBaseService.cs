using SeldatUnilever_Ver1._02.Management.ProcedureServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeldatMRMS.Management.RobotManagent
{
    public class RobotBaseService:TrafficRobotUnity
    {
        public enum ProcedureControlAssign 
        {
            PRO_ALL = 0,
            PRO_BUFFER_TO_MACHINE ,
            PRO_BUFFER_TO_RETURN,
            PRO_FORKLIFT_TO_BUFFER,
            PRO_MACHINE_TO_RETURN,
            PRO_RETURN_TO_GATE,
            PRO_CHARGE,
            PRO_READY,
            PRO_FORKLIFT_TO_MACHINE,
            PRO_IDLE
        }
        public enum RobotInModeCode
        {
            ROBOT_MODE_READY,
            ROBOT_MODE_WAIT,
            ROBOT_MODE_IDLE,
        }
        public RobotInModeCode robotInModeCode;
        public class ProcedureRegistryInRobotUnity
        {
            public ProcedureBufferToMachine pBM { get; set; }
            public ProcedureBufferToReturn pBR { get; set; }
            public ProcedureForkLiftToBuffer pFB { get; set; }
            public ProcedureForkLiftToMachine pFM { get; set; }
            public ProcedureMachineToReturn pMR { get; set; }
            public ProcedureRobotToCharger pRC { get; set; }
            public ProcedureRobotToReady pRR { get; set; }
        }
        public object ProcedureControl;
        public ProcedureRegistryInRobotUnity proRegistryInRobot=new ProcedureRegistryInRobotUnity();
        public ProcedureControlAssign  PreProcedureAs;
        public ProcedureControlAssign ProcedureRobotAssigned;
        public ProcedureControlAssign ProcedureAs;
        public bool SelectedATask { get; set; }
       
        public struct LoadedConfigureInformation
        {
            public bool IsLoadedStatus { get; set; }
            public String ErrorContent { get; set; }
        }

        public void DisposeProcedure()
        {
                    if (proRegistryInRobot.pBM != null)
                    {
                        proRegistryInRobot.pBM.Destroy();
                        //proRegistryInRobot.pBM = null;
                    }
                 
                    if (proRegistryInRobot.pMR != null)
                    {
                        proRegistryInRobot.pMR.Destroy();
                       // proRegistryInRobot.pMR = null;
                    }
                    if (proRegistryInRobot.pFB != null)
                    {
                        proRegistryInRobot.pFB.Destroy();
                      // proRegistryInRobot.pFB = null;
                    }
                    if (proRegistryInRobot.pFM != null)
                    {
                        proRegistryInRobot.pFM.Destroy();
                        // proRegistryInRobot.pFB = null;
                    }
                    if (proRegistryInRobot.pBR != null)
                    {
                        proRegistryInRobot.pBR.Destroy();
                       // proRegistryInRobot.pBR = null;
                    }
                    if (proRegistryInRobot.pRC != null)
                    {
                        proRegistryInRobot.pRC.Destroy();
                       // proRegistryInRobot.pRC = null;
                    }
                    if (proRegistryInRobot.pRR != null)
                    {
                        proRegistryInRobot.pRR.Destroy();
                       // proRegistryInRobot.pRR = null;
                    }
        }

        /* public void DisposeProcedure()
         {
             switch (ProcedureRobotAssigned)
             {
                 case ProcedureControlAssign.PRO_BUFFER_TO_MACHINE:
                     if (proRegistryInRobot.pBM != null)
                     {
                         proRegistryInRobot.pBM.Destroy();
                         proRegistryInRobot.pBM = null;
                     }
                     break;
                 case ProcedureControlAssign.PRO_MACHINE_TO_RETURN:
                     if (proRegistryInRobot.pMR != null)
                     {
                         proRegistryInRobot.pMR.Destroy();
                         proRegistryInRobot.pMR = null;
                     }
                     break;
                 case ProcedureControlAssign.PRO_FORKLIFT_TO_BUFFER:
                     if (proRegistryInRobot.pFB != null)
                     {
                         proRegistryInRobot.pFB.Destroy();
                         proRegistryInRobot.pFB = null;
                     }
                     break;
                 case ProcedureControlAssign.PRO_BUFFER_TO_RETURN:
                     if (proRegistryInRobot.pBR != null)
                     {
                         proRegistryInRobot.pBR.Destroy();
                         proRegistryInRobot.pBR = null;
                     }
                     break;
                 case ProcedureControlAssign.PRO_CHARGE:
                     if (proRegistryInRobot.pRC != null)
                     {
                         proRegistryInRobot.pRC.Destroy();
                         proRegistryInRobot.pRC = null;
                     }
                     break;
                 case ProcedureControlAssign.PRO_READY:
                     if (proRegistryInRobot.pRR != null)
                     {
                         proRegistryInRobot.pRR.Destroy();
                         proRegistryInRobot.pRR = null;
                     }
                     break;
             }
         }*/
    }
}
