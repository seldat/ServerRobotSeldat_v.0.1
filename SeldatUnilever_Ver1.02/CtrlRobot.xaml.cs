using SeldatMRMS.Management.RobotManagent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;

namespace SeldatUnilever_Ver1._02
{
    /// <summary>
    /// Interaction logic for CtrlRobot.xaml
    /// </summary>
    public partial class CtrlRobot : Window
    {
        RobotManagementService rms;
        public CtrlRobot(RobotManagementService rms)
        {
            InitializeComponent();
            this.rms = rms;
            txtBat.Text = "5";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            rms.RobotUnityRegistedList.ElementAt(0).Value.FinishedStatesPublish(2000);
            new Thread(()=>{
                rms.RobotUnityRegistedList.ElementAt(2).Value.SendCmdLineDetectionCtrl(RequestCommandLineDetect.REQUEST_LINEDETECT_PALLETUP);
        }).Start();
            }

        private void CmdPalletUp_Click(object sender, RoutedEventArgs e)
        {
            rms.RobotUnityRegistedList.ElementAt(0).Value.FinishedStatesPublish(3203);
            new Thread(() => {
                rms.RobotUnityRegistedList.ElementAt(2).Value.SendCmdAreaPallet("hello kh");
            }).Start();
            
        }

        private void CmdPalletDown_Click(object sender, RoutedEventArgs e)
        {
            rms.RobotUnityRegistedList.ElementAt(0).Value.FinishedStatesPublish(3204);

            new Thread(() => {
                rms.RobotUnityRegistedList.ElementAt(2).Value.SendCmdPosPallet(RequestCommandPosPallet.REQUEST_FORWARD_DIRECTION);

            }).Start();
}

        private void CmdBackFrontLine_Click(object sender, RoutedEventArgs e)
        {
            rms.RobotUnityRegistedList.ElementAt(0).Value.FinishedStatesPublish(3213);
            new Thread(() => {
                rms.RobotUnityRegistedList.ElementAt(2).Value.SendPoseStamped(new Pose());
            }).Start();
        }

        private void CmdBatLevel_Click(object sender, RoutedEventArgs e)
        {
            rms.RobotUnityRegistedList.ElementAt(0).Value.BatteryPublish(float.Parse(txtBat.Text));
        }

        private void CmdError_Click(object sender, RoutedEventArgs e)
        {
            rms.RobotUnityRegistedList.ElementAt(0).Value.FinishedStatesPublish(3215);
        }

        private void CmdGetInCharger_Click(object sender, RoutedEventArgs e)
        {
            rms.RobotUnityRegistedList.ElementAt(0).Value.FinishedStatesPublish(3206);
        }

        private void CmdGetOutCharger_Click(object sender, RoutedEventArgs e)
        {
            rms.RobotUnityRegistedList.ElementAt(0).Value.FinishedStatesPublish(3207);
            // String tamp = "1001";
            // rms.RobotUnityRegistedList.ElementAt(0).Value.TestLaserError(tamp);
        }

        private void LineCamePoint_Click(object sender, RoutedEventArgs e)
        {
            rms.RobotUnityRegistedList.ElementAt(0).Value.FinishedStatesPublish(3205);
            // String tamp = "11010";
            // rms.RobotUnityRegistedList.ElementAt(0).Value.TestLaserWarning(tamp);
        }
    }
}
