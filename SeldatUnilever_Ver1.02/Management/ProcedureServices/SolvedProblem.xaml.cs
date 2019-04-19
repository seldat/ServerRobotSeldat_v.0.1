using Newtonsoft.Json;
using SeldatMRMS;
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
using static SeldatMRMS.ProcedureControlServices;

namespace SeldatMRMS
{
    /// <summary>
    /// Interaction logic for SolvedProblem.xaml
    /// </summary>
    public partial class SolvedProblem : Window
    {
        public Object obj;
        public SolvedProblem(string cultureName = null)
        {
            try
            {
                InitializeComponent();
                ApplyLanguage(cultureName);
            }
            catch { }

        }
        public void Registry(Object obj)
        {
            this.obj=obj;
        }
        public void Dispose()
        {
            this.obj = null;
        }
        public SolvedProblem(Object obj, string cultureName = null)
        {
            InitializeComponent();
            ApplyLanguage(cultureName);
           /* this.obj = obj;
            if (obj.GetType() == typeof(ProcedureForkLiftToBuffer))
            {
                ProcedureForkLiftToBuffer proc = obj as ProcedureForkLiftToBuffer;
                ShowInformation(proc);
            }
            else if (obj.GetType() == typeof(ProcedureBufferToMachine))
            {
                ProcedureBufferToMachine proc = obj as ProcedureBufferToMachine;
                ShowInformation(proc);
            }
            else if (obj.GetType() == typeof(ProcedureMachineToReturn))
            {
                ProcedureMachineToReturn proc = obj as ProcedureMachineToReturn;
                ShowInformation(proc);
            }
            else if (obj.GetType() == typeof(ProcedureBufferToReturn))
            {
                ProcedureBufferToReturn proc = obj as ProcedureBufferToReturn;
                ShowInformation(proc);
            }
            else if (obj.GetType() == typeof(ProcedureRobotToReady))
            {
                ProcedureRobotToReady proc = obj as ProcedureRobotToReady;
                ShowInformation(proc);
            }
            else if (obj.GetType() == typeof(ProcedureRobotToCharger))
            {
                ProcedureRobotToCharger proc = obj as ProcedureRobotToCharger;
                ShowInformation(proc);
            }*/
        }

        public void ApplyLanguage(string cultureName = null)
        {
            if (cultureName != null)
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);

            ResourceDictionary dict = new ResourceDictionary();
            switch (Thread.CurrentThread.CurrentCulture.ToString())
            {
                case "vi-VN":
                    dict.Source = new Uri("..\\Lang\\Vietnamese.xaml", UriKind.Relative);
                    break;
                // ...
                default:
                    dict.Source = new Uri("..\\Lang\\English.xaml", UriKind.Relative);
                    break;
            }
            this.Resources.MergedDictionaries.Add(dict);
        }

        public void ShowInformation()
        {
            if (obj.GetType() == typeof(ProcedureForkLiftToBuffer))
            {
                var proc = obj as ProcedureForkLiftToBuffer;
                txt_robotname.Content = proc.robot.properties.NameId;
                txt_procedurecode.Content = proc.procedureCode.ToString();
                txt_errorcode.Content = proc.errorCode;
                txt_datetime.Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                FlowDocument myFlowDoc = new FlowDocument();
                myFlowDoc.Blocks.Add(new Paragraph(new Run("[Robot]====================================")));
                myFlowDoc.Blocks.Add(new Paragraph(new Run(ObjectDumper.Dump(proc.robot.properties))));
                myFlowDoc.Blocks.Add(new Paragraph(new Run("[Door]=====================================")));
                myFlowDoc.Blocks.Add(new Paragraph(new Run(ObjectDumper.Dump(proc.door.config))));
                detailInfo.Document = myFlowDoc;
            }
            else if (obj.GetType() == typeof(ProcedureBufferToMachine))
            {
                var proc = obj as ProcedureBufferToMachine;
                txt_robotname.Content = proc.robot.properties.NameId;
                txt_procedurecode.Content = proc.procedureCode.ToString();
                txt_errorcode.Content = proc.errorCode;
                txt_datetime.Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                FlowDocument myFlowDoc = new FlowDocument();
                myFlowDoc.Blocks.Add(new Paragraph(new Run("[Robot]====================================")));
                myFlowDoc.Blocks.Add(new Paragraph(new Run(ObjectDumper.Dump(proc.robot.properties))));
                detailInfo.Document = myFlowDoc;
            }
            else if (obj.GetType() == typeof(ProcedureMachineToReturn))
            {
                var proc = obj as ProcedureMachineToReturn;
                txt_robotname.Content = proc.robot.properties.NameId;
                txt_procedurecode.Content = proc.procedureCode.ToString();
                txt_errorcode.Content = proc.errorCode;
                txt_datetime.Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                FlowDocument myFlowDoc = new FlowDocument();
                myFlowDoc.Blocks.Add(new Paragraph(new Run("[Robot]====================================")));
                myFlowDoc.Blocks.Add(new Paragraph(new Run(ObjectDumper.Dump(proc.robot.properties))));
                detailInfo.Document = myFlowDoc;
            }
            else if (obj.GetType() == typeof(ProcedureBufferToReturn))
            {
                var proc = obj as ProcedureBufferToReturn;
                txt_robotname.Content = proc.robot.properties.NameId;
                txt_procedurecode.Content = proc.procedureCode.ToString();
                txt_errorcode.Content = proc.errorCode;
                txt_datetime.Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                FlowDocument myFlowDoc = new FlowDocument();
                myFlowDoc.Blocks.Add(new Paragraph(new Run("[Robot]====================================")));
                myFlowDoc.Blocks.Add(new Paragraph(new Run(ObjectDumper.Dump(proc.robot.properties))));
                detailInfo.Document = myFlowDoc;
            }
            else if (obj.GetType() == typeof(ProcedureRobotToReady))
            {
                var proc = obj as ProcedureRobotToReady;
                txt_robotname.Content = proc.robot.properties.NameId;
                txt_procedurecode.Content = proc.procedureCode.ToString();
                txt_errorcode.Content = proc.errorCode;
                txt_datetime.Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                FlowDocument myFlowDoc = new FlowDocument();
                myFlowDoc.Blocks.Add(new Paragraph(new Run("[Robot]====================================")));
                myFlowDoc.Blocks.Add(new Paragraph(new Run(ObjectDumper.Dump(proc.robot.properties))));
                detailInfo.Document = myFlowDoc;
            }
            else if (obj.GetType() == typeof(ProcedureRobotToCharger))
            {
                var proc = obj as ProcedureRobotToCharger;
                txt_robotname.Content = proc.robot.properties.NameId;
                txt_procedurecode.Content = proc.procedureCode.ToString();
                txt_errorcode.Content = proc.errorCode;
                txt_datetime.Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                FlowDocument myFlowDoc = new FlowDocument();
                myFlowDoc.Blocks.Add(new Paragraph(new Run("[Robot]====================================")));
                myFlowDoc.Blocks.Add(new Paragraph(new Run(ObjectDumper.Dump(proc.robot.properties))));
                myFlowDoc.Blocks.Add(new Paragraph(new Run("[Charger]====================================")));
                myFlowDoc.Blocks.Add(new Paragraph(new Run(ObjectDumper.Dump(proc.chargerCtrl.cf))));
                detailInfo.Document = myFlowDoc;
            }
            else if (obj.GetType() == typeof(ProcedureReturnToGate))
            {
                var proc = obj as ProcedureForkLiftToBuffer;
                txt_robotname.Content = proc.robot.properties.NameId;
                txt_procedurecode.Content = proc.procedureCode.ToString();
                txt_errorcode.Content = proc.errorCode;
                txt_datetime.Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                FlowDocument myFlowDoc = new FlowDocument();
                myFlowDoc.Blocks.Add(new Paragraph(new Run("[Robot]====================================")));
                myFlowDoc.Blocks.Add(new Paragraph(new Run(ObjectDumper.Dump(proc.robot.properties))));
                myFlowDoc.Blocks.Add(new Paragraph(new Run("[Door]=====================================")));
                myFlowDoc.Blocks.Add(new Paragraph(new Run(ObjectDumper.Dump(proc.door.config))));
                detailInfo.Document = myFlowDoc;
            }

        }
        public void UpdateInformation(SelectHandleError shError)
        {
            if (obj.GetType() == typeof(ProcedureForkLiftToBuffer))
            {
                var proc = obj as ProcedureForkLiftToBuffer;
                proc.procedureDataItemsDB.GetParams("F");
                proc.robot.properties.detailInfo = new TextRange(detailInfo.Document.ContentStart, detailInfo.Document.ContentEnd).Text;
                proc.robot.properties.problemContent = new TextRange(problemInfo.Document.ContentStart, problemInfo.Document.ContentEnd).Text;
                proc.robot.properties.solvedProblemContent = new TextRange(solvedProblemInfo.Document.ContentStart, solvedProblemInfo.Document.ContentEnd).Text;
                proc.robotTaskDB.procedureContent = JsonConvert.SerializeObject(proc.robot.properties).ToString();
               // MessageBox.Show(JsonConvert.SerializeObject(proc.robotTaskDB));
               // MessageBox.Show(JsonConvert.SerializeObject(proc.procedureDataItemsDB));
                proc.SendHttpRobotTaskItem(proc.robotTaskDB);
                proc.SendHttpProcedureDataItem(proc.procedureDataItemsDB);
                proc.selectHandleError = shError;


            }
            else if (obj.GetType() == typeof(ProcedureBufferToMachine))
            {
                var proc = obj as ProcedureBufferToMachine;
                proc.procedureDataItemsDB.GetParams("F");
                proc.robot.properties.detailInfo = new TextRange(detailInfo.Document.ContentStart, detailInfo.Document.ContentEnd).Text;
                proc.robot.properties.problemContent = new TextRange(problemInfo.Document.ContentStart, problemInfo.Document.ContentEnd).Text;
                proc.robot.properties.solvedProblemContent = new TextRange(solvedProblemInfo.Document.ContentStart, solvedProblemInfo.Document.ContentEnd).Text;
                proc.robotTaskDB.procedureContent = JsonConvert.SerializeObject(proc.robot.properties).ToString();
              //  MessageBox.Show(JsonConvert.SerializeObject(proc.robotTaskDB));
             //   MessageBox.Show(JsonConvert.SerializeObject(proc.procedureDataItemsDB));
                proc.SendHttpRobotTaskItem(proc.robotTaskDB);
                proc.SendHttpProcedureDataItem(proc.procedureDataItemsDB);
                proc.selectHandleError = shError;


            }
            else if (obj.GetType() == typeof(ProcedureMachineToReturn))
            {
                var proc = obj as ProcedureMachineToReturn;
                proc.procedureDataItemsDB.GetParams("F");
                proc.robot.properties.detailInfo = new TextRange(detailInfo.Document.ContentStart, detailInfo.Document.ContentEnd).Text;
                proc.robot.properties.problemContent = new TextRange(problemInfo.Document.ContentStart, problemInfo.Document.ContentEnd).Text;
                proc.robot.properties.solvedProblemContent = new TextRange(solvedProblemInfo.Document.ContentStart, solvedProblemInfo.Document.ContentEnd).Text;
                proc.robotTaskDB.procedureContent = JsonConvert.SerializeObject(proc.robot.properties).ToString();
             //   MessageBox.Show(JsonConvert.SerializeObject(proc.robotTaskDB));
             //   MessageBox.Show(JsonConvert.SerializeObject(proc.procedureDataItemsDB));
                proc.SendHttpRobotTaskItem(proc.robotTaskDB);
                proc.SendHttpProcedureDataItem(proc.procedureDataItemsDB);
                proc.selectHandleError = shError;


            }
            else if (obj.GetType() == typeof(ProcedureBufferToReturn))
            {
                var proc = obj as ProcedureBufferToReturn;
                proc.procedureDataItemsDB.GetParams("F");
                proc.robot.properties.detailInfo = new TextRange(detailInfo.Document.ContentStart, detailInfo.Document.ContentEnd).Text;
                proc.robot.properties.problemContent = new TextRange(problemInfo.Document.ContentStart, problemInfo.Document.ContentEnd).Text;
                proc.robot.properties.solvedProblemContent = new TextRange(solvedProblemInfo.Document.ContentStart, solvedProblemInfo.Document.ContentEnd).Text;
                proc.robotTaskDB.procedureContent = JsonConvert.SerializeObject(proc.robot.properties).ToString();
             //   MessageBox.Show(JsonConvert.SerializeObject(proc.robotTaskDB));
            //    MessageBox.Show(JsonConvert.SerializeObject(proc.procedureDataItemsDB));
                proc.SendHttpRobotTaskItem(proc.robotTaskDB);
                proc.SendHttpProcedureDataItem(proc.procedureDataItemsDB);
                proc.selectHandleError = shError;


            }
            else if (obj.GetType() == typeof(ProcedureRobotToReady))
            {
                var proc = obj as ProcedureRobotToReady;
                proc.readyChargerProcedureDB.Registry(proc.robotTaskDB.robotTaskId);
                proc.readyChargerProcedureDB.GetParams("F");
                proc.robot.properties.detailInfo = new TextRange(detailInfo.Document.ContentStart, detailInfo.Document.ContentEnd).Text;
                proc.robot.properties.problemContent = new TextRange(problemInfo.Document.ContentStart, problemInfo.Document.ContentEnd).Text;
                proc.robot.properties.solvedProblemContent = new TextRange(solvedProblemInfo.Document.ContentStart, solvedProblemInfo.Document.ContentEnd).Text;
                proc.robotTaskDB.procedureContent = JsonConvert.SerializeObject(proc.robot.properties).ToString();
            //    MessageBox.Show(JsonConvert.SerializeObject(proc.robotTaskDB));
             //   MessageBox.Show(JsonConvert.SerializeObject(proc.readyChargerProcedureDB));
                proc.SendHttpRobotTaskItem(proc.robotTaskDB);
                proc.SendHttpReadyChargerProcedureDB(proc.readyChargerProcedureDB);
                proc.selectHandleError = shError;

            }
            else if (obj.GetType() == typeof(ProcedureRobotToCharger))
            {
                var proc = obj as ProcedureRobotToCharger;
                proc.readyChargerProcedureDB.Registry(proc.chargerCtrl, proc.robotTaskDB.robotTaskId);
                proc.readyChargerProcedureDB.GetParams("F");
                proc.robot.properties.detailInfo = new TextRange(detailInfo.Document.ContentStart, detailInfo.Document.ContentEnd).Text;
                proc.robot.properties.problemContent = new TextRange(problemInfo.Document.ContentStart, problemInfo.Document.ContentEnd).Text;
                proc.robot.properties.solvedProblemContent = new TextRange(solvedProblemInfo.Document.ContentStart, solvedProblemInfo.Document.ContentEnd).Text;
                proc.robotTaskDB.procedureContent = JsonConvert.SerializeObject(proc.robot.properties).ToString();
             //   MessageBox.Show(JsonConvert.SerializeObject(proc.robotTaskDB));
            //    MessageBox.Show(JsonConvert.SerializeObject(proc.readyChargerProcedureDB));
                proc.SendHttpRobotTaskItem(proc.robotTaskDB);
                proc.SendHttpReadyChargerProcedureDB(proc.readyChargerProcedureDB);
                proc.selectHandleError = shError;


            }
        }

        private void cancelProcBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void destroyProcBtn_Click(object sender, RoutedEventArgs e)
        {
         
            UpdateInformation(SelectHandleError.CASE_ERROR_EXIT);
            
            Hide();
            Dispose();
        }

        private void contProcBtn_Click(object sender, RoutedEventArgs e)
        {
      
            UpdateInformation(SelectHandleError.CASE_ERROR_CONTINUOUS);
            Hide();
            Dispose();
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ShowInformation();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
