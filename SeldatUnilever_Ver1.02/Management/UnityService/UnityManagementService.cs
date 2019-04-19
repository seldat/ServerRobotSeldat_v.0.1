using SeldatMRMS.Management.DoorServices;
using SeldatMRMS.Management.RobotManagent;
using SeldatMRMS.Management.TrafficManager;
using SeldatUnilever_Ver1._02;
using SeldatUnilever_Ver1._02.DTO;
using SelDatUnilever_Ver1._00.Management.ChargerCtrl;
using SelDatUnilever_Ver1._00.Management.DeviceManagement;
using SelDatUnilever_Ver1._00.Management.UnityService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SeldatMRMS.Management.UnityService
{
    public class UnityManagementService : NotifyUIBase
    {
       //SolvedProblem sf; 

        public RobotManagementService robotManagementService { get; set; }
        public DoorManagementService doorManagementService { get; set; }
        public ProcedureManagementService procedureManagementService { get; set; }
        public TrafficManagementService trafficService { get; set; }
        public AssigmentTaskService assigmentTaskService { get; set; }
        public DeviceRegistrationService deviceRegistrationService { get; set; }
        public ChargerManagementService chargerService;

        

        private MainWindow mainWindow;
        public UnityManagementService(MainWindow mainWindow )
        {
            this.mainWindow = mainWindow;
        }
        public void Initialize()
        {
            robotManagementService = new RobotManagementService(this.mainWindow.map);
            doorManagementService = new DoorManagementService();
            procedureManagementService = new ProcedureManagementService();
            chargerService = new ChargerManagementService();
            trafficService = new TrafficManagementService();
            deviceRegistrationService = new DeviceRegistrationService(12000);

            assigmentTaskService = new AssigmentTaskService();
            assigmentTaskService.RegistryService(robotManagementService);
            assigmentTaskService.RegistryService(procedureManagementService);
            assigmentTaskService.RegistryService(deviceRegistrationService.GetDeviceItemList());
            assigmentTaskService.RegistryService(trafficService);
            procedureManagementService.RegistryService(trafficService);
            procedureManagementService.RegistryService(robotManagementService);
            procedureManagementService.RegistryService(doorManagementService);
            procedureManagementService.RegistryService(chargerService);
            procedureManagementService.RegistryService(deviceRegistrationService);


            robotManagementService.Registry(trafficService);
            // robotManagementService.TestRobotProceure();
            robotManagementService.TestRobotReadyProceure();
            //robotManagementService.robot2test();

            deviceRegistrationService.listen();
            deviceRegistrationService.RegistryMainWindow(this.mainWindow);

            //assigmentTaskService.Start();
            MessageBox.Show("Bấm Start Để Bắt Đầu !");
        }
        public void Dispose()
        {

        }
        public void OpenConfigureForm(String frm)
        {
            switch(frm)
            {
                case "ACF":
                    trafficService.configureArea.Show();
                    break;
                case "RACF":
                    trafficService.configureRiskZone.Show();
                    break;
                case "CCF":
                    chargerService.configureForm.Show();
                    break;
                case "DCF":
                    doorManagementService.doorConfigure.Show();
                    break;
                case "RCF":
                    robotManagementService.configureForm.Show();
                    break;
            }
        }
    }
}
