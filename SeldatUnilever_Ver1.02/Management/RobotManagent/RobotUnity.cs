using SeldatMRMS.RobotView;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using static SelDatUnilever_Ver1._00.Management.ChargerCtrl.ChargerCtrl;
using static SelDatUnilever_Ver1._00.Management.TrafficManager.TrafficRounterService;

namespace SeldatMRMS.Management.RobotManagent
{
    public class RobotUnity : RobotBaseService
    {

        Ellipse headerPoint;
        Ellipse headerPoint1;
        Ellipse headerPoint2;
        Ellipse headerPoint3;
        Path safetyArea;
        Ellipse SmallCircleArea;
        Ellipse BlueCircleArea;
        Ellipse YellowCircleArea;
        double angle = 0.0f;
        public Point org = new Point(600, 350);
        public double rad = 0;
        public double anglestep = 0;
        Rect area = new Rect(30, 30, 500, 500);
        Point loc = new Point(0, 0);
        public event Action<string> RemoveHandle;
        public enum RobotStatusColorCode
        {
            ROBOT_STATUS_OK = 0,
            ROBOT_STATUS_RUNNING,
            ROBOT_STATUS_ERROR,
            ROBOT_STATUS_WAIT_FIX,
            ROBOT_STATUS_DISCONNECT,
            ROBOT_STATUS_CONNECT,
            ROBOT_STATUS_RECONNECT,
            ROBOT_STATUS_CAN_NOTGET_DATA
        }
        public struct Props
        {
            public string name;
            public bool isSelected;
            public bool isHovering;
            public Grid mainGrid;
            public Grid statusGrid;

            public Label rbID;
            public Label rbTask;
            public Rectangle headLed;
            public Rectangle tailLed;
            public TranslateTransform rbTranslate;
            public TransformGroup rbTransformGroup;
            public RotateTransform rbRotateTransform;
            public TranslateTransform contentTranslate;
            public TransformGroup contentTransformGroup;
            public RotateTransform contentRotateTransform;
            public Border statusBorder;
            public List<Point> eightCorner;
        }
        public Canvas canvas;
        public Props props;
        public Border border;
        public LoadedConfigureInformation loadConfigureInformation;
        public RobotManagementService robotService;
        public SolvedProblem solvedProblem;
        public Robot3D robot3DModel = null;
        public RobotUnity()
        {
            solvedProblem = new SolvedProblem();

        }
        MenuItem problemSolutionItem = new MenuItem();
        MenuItem startItem = new MenuItem();
        MenuItem pauseItem = new MenuItem();
        MenuItem connectItem = new MenuItem();
        MenuItem retryconnectItem = new MenuItem();
        MenuItem disconnectItem = new MenuItem();
        MenuItem turnOnOffItem = new MenuItem();
        MenuItem addReadyListItem = new MenuItem();
        MenuItem addWaitTaskListItem = new MenuItem();
        MenuItem logOutItem = new MenuItem();
        public void Initialize(Canvas canvas)
        {
            this.canvas = canvas;
            //ModelVisual3D layer = new ModelVisual3D();
            // robot3DModel = new Robot3D(properties.NameId, layer);
            safetyArea = new Path();
            safetyArea.Stroke = new SolidColorBrush(Colors.MediumBlue);
            safetyArea.StrokeThickness = 1;
            border = new Border();
            border.ToolTip = "";
            border.ToolTipOpening += ChangeToolTipContent;
            props.isSelected = false;
            props.isHovering = false;
            border.ContextMenu = new ContextMenu();


            problemSolutionItem.Header = "Problem Solution";
            problemSolutionItem.Click += PoblemSolutionItem;
            //===================================

            startItem.Header = "Start";
            startItem.Click += StartMenu;
            //===================================

            pauseItem.Header = "Pause";
            pauseItem.Click += PauseMenu;
            pauseItem.IsEnabled = true;

            logOutItem.Header = "Log";
            logOutItem.Click += LogOut;
            logOutItem.IsEnabled = true;

            addReadyListItem.Header = "Add To Ready Mode";
            addReadyListItem.Click += AddReadyListMenu;
            addReadyListItem.IsEnabled = true;

            addWaitTaskListItem.Header = "Add To WaitTask Mode";
            addWaitTaskListItem.Click += AddWaitTaskListMenu;
            addWaitTaskListItem.IsEnabled = true;

            connectItem.Header = "Connect";
            connectItem.Click += ConnectMenu;
            connectItem.IsEnabled = true;

            retryconnectItem.Header = "ReConnect";
            retryconnectItem.Click += ReConnectMenu;
            retryconnectItem.IsEnabled = true;

            disconnectItem.Header = "Disconnect";
            disconnectItem.Click += DisConnectMenu;
            disconnectItem.IsEnabled = false;

            turnOnOffItem.Header = "Set On/Off Traffic";
            turnOnOffItem.Click += SetOnOffTrafficMenu;
            turnOnOffItem.IsEnabled = true;




            border.ContextMenu.Items.Add(problemSolutionItem);
            border.ContextMenu.Items.Add(startItem);
            border.ContextMenu.Items.Add(pauseItem);
            border.ContextMenu.Items.Add(logOutItem);

            border.ContextMenu.Items.Add(connectItem);
            border.ContextMenu.Items.Add(retryconnectItem);
            border.ContextMenu.Items.Add(disconnectItem);
            border.ContextMenu.Items.Add(turnOnOffItem);

            border.ContextMenu.Items.Add(addReadyListItem);
            border.ContextMenu.Items.Add(addWaitTaskListItem);

            //====================EVENT=====================
            //MouseLeave += MouseLeavePath;
            //MouseMove += MouseHoverPath;
            //MouseLeftButtonDown += MouseLeftButtonDownPath;
            //MouseRightButtonDown += MouseRightButtonDownPath;
            //===================CREATE=====================
            //Name = "Robotx" + Global_Mouse.EncodeTransmissionTimestamp();
            props.mainGrid = new Grid();
            props.statusGrid = new Grid();
            props.statusBorder = new Border();
            props.rbID = new Label();
            props.rbTask = new Label();
            props.headLed = new Rectangle();
            props.tailLed = new Rectangle();
            props.eightCorner = new List<Point>();
            for (int i = 0; i < 8; i++)
            {
                Point temp = new Point();
                props.eightCorner.Add(temp);
            }
            props.rbRotateTransform = new RotateTransform();
            props.rbTranslate = new TranslateTransform();
            props.rbTransformGroup = new TransformGroup();
            props.contentRotateTransform = new RotateTransform();
            props.contentTranslate = new TranslateTransform();
            props.contentTransformGroup = new TransformGroup();
            // robotProperties = new Properties(this);
            //===================STYLE=====================
            //Robot border
            border.Width = 22;
            border.Height = 15;
            border.BorderThickness = new Thickness(1);
            border.BorderBrush = new SolidColorBrush(Colors.Linen);
            border.Background = new SolidColorBrush(Colors.Blue);
            border.CornerRadius = new CornerRadius(3);
            border.RenderTransformOrigin = new Point(0.5, 0.5);
            //mainGrid
            props.mainGrid.Background = new SolidColorBrush(Colors.Transparent);
            for (int i = 0; i < 3; i++)
            {
                ColumnDefinition colTemp = new ColumnDefinition();
                //colTemp.Name = properties.NameID + "xL" + i;
                if ((i == 0) || (i == 2))
                {
                    colTemp.Width = new GridLength(1);
                }
                props.mainGrid.ColumnDefinitions.Add(colTemp);
            }
            //headLed
            props.headLed.Height = 7;
            props.headLed.Fill = new SolidColorBrush(Colors.LightYellow);
            Grid.SetColumn(props.headLed, 2);
            //tailLed
            props.tailLed.Height = 7;
            props.tailLed.Fill = new SolidColorBrush(Colors.OrangeRed);
            Grid.SetColumn(props.tailLed, 0);
            //statusBorder
            props.statusBorder.Width = 10;
            props.statusBorder.Height = 13;
            props.statusBorder.RenderTransformOrigin = new Point(0.5, 0.5);
            Grid.SetColumn(props.statusBorder, 1);
            //statusGrid
            for (int i = 0; i < 2; i++)
            {
                RowDefinition rowTemp = new RowDefinition();
                // rowTemp.Name = properties.NameID + "xR" + i;
                props.statusGrid.RowDefinitions.Add(rowTemp);
            }
            //rbID
            props.rbID.Padding = new Thickness(0);
            props.rbID.Margin = new Thickness(-5, 0, -5, 0);
            props.rbID.HorizontalAlignment = HorizontalAlignment.Center;
            props.rbID.VerticalAlignment = VerticalAlignment.Bottom;
            props.rbID.Content = "27";
            props.rbID.Foreground = new SolidColorBrush(Colors.Yellow);
            props.rbID.FontFamily = new FontFamily("Calibri");
            props.rbID.FontSize = 6;
            props.rbID.FontWeight = FontWeights.Bold;
            Grid.SetRow(props.rbID, 0);

            //rbTask
            props.rbTask.Padding = new Thickness(0);
            props.rbTask.Margin = new Thickness(-5, -1, -5, -1);
            props.rbTask.HorizontalAlignment = HorizontalAlignment.Center;
            props.rbTask.VerticalAlignment = VerticalAlignment.Top;
            props.rbTask.Content = "9999";
            props.rbTask.Foreground = new SolidColorBrush(Colors.LawnGreen);
            props.rbTask.FontFamily = new FontFamily("Calibri");
            props.rbTask.FontSize = 6;
            props.rbTask.FontWeight = FontWeights.Bold;
            Grid.SetRow(props.rbTask, 1);

            //===================CHILDREN===================
            props.statusGrid.Children.Add(props.rbID);
            props.statusGrid.Children.Add(props.rbTask);
            props.statusBorder.Child = props.statusGrid;
            props.mainGrid.Children.Add(props.headLed);
            props.mainGrid.Children.Add(props.tailLed);
            props.mainGrid.Children.Add(props.statusBorder);
            props.rbTransformGroup.Children.Add(props.rbRotateTransform);
            props.rbTransformGroup.Children.Add(props.rbTranslate);
            border.RenderTransform = props.rbTransformGroup;
            props.contentTransformGroup.Children.Add(props.contentRotateTransform);
            props.contentTransformGroup.Children.Add(props.contentTranslate);
            props.statusBorder.RenderTransform = props.contentTransformGroup;
            border.Child = props.mainGrid;
            this.canvas.Children.Add(border);
            headerPoint = new Ellipse();
            headerPoint.Width = 5;
            headerPoint.Height = 5;
            headerPoint.Fill = new SolidColorBrush(Colors.Red);

            headerPoint1 = new Ellipse();
            headerPoint1.Width = 5;
            headerPoint1.Height = 5;
            headerPoint1.Fill = new SolidColorBrush(Colors.Red);

            headerPoint2 = new Ellipse();
            headerPoint2.Width = 5;
            headerPoint2.Height = 5;
            headerPoint2.Fill = new SolidColorBrush(Colors.Red);

            headerPoint3 = new Ellipse();
            headerPoint3.Width = 5;
            headerPoint3.Height = 5;
            headerPoint3.Fill = new SolidColorBrush(Colors.Red);

            canvas.Children.Add(safetyArea);
            canvas.Children.Add(headerPoint);
            canvas.Children.Add(headerPoint1);
            canvas.Children.Add(headerPoint2);
            canvas.Children.Add(headerPoint3);
            setColorRobotStatus(RobotStatusColorCode.ROBOT_STATUS_DISCONNECT);

            SmallCircleArea = new Ellipse();
            SmallCircleArea.Stroke = new SolidColorBrush(Colors.Black);
            canvas.Children.Add(SmallCircleArea);

            BlueCircleArea = new Ellipse();
            BlueCircleArea.Stroke = new SolidColorBrush(Colors.Blue);
            canvas.Children.Add(BlueCircleArea);

            YellowCircleArea = new Ellipse();
            YellowCircleArea.Stroke = new SolidColorBrush(Colors.OrangeRed);
            canvas.Children.Add(YellowCircleArea);
            new Thread(() => {
                while (true)
                {
                    Draw();
                    Thread.Sleep(700);
                }
            }).Start();
            //  robotLogOut.SetName(properties.Label);

        }

        private void Itemw0_Checked(object sender, RoutedEventArgs e)
        {

        }

        public void RegistrySolvedForm(Object obj)
        {
            //  if(obj.GetType()==typeof(ProcedureControlServices))
            {
                solvedProblem.Registry(obj);
                //solvedProblem.Show();
            }
        }
        public void RegistryRobotService(RobotManagementService robotService)
        {
            this.robotService = robotService;
        }
        public void DestroyRegistrySolvedForm()
        {
            //  if(obj.GetType()==typeof(ProcedureControlServices))
            {
                solvedProblem.Dispose();
                //solvedProblem.Show();
            }
        }
        public void DisplaySolvedForm()
        {
            try
            {
                if (solvedProblem.obj != null)
                    solvedProblem.Show();
                else
                    MessageBox.Show("Không có nội dung lỗi!!");
            }
            catch { }
        }
        public Point CirclePoint(double radius, double angleInDegrees, Point origin)
        {
            double x = (double)(radius * Math.Cos(angleInDegrees * Math.PI / 180)) + origin.X;
            double y = (double)(radius * Math.Sin(angleInDegrees * Math.PI / 180)) + origin.Y;
            return new Point(x, y);
        }
        //public void setColorRobotStatus(RobotStatusColorCode rsc)
        //{
        //    switch (rsc)
        //    {
        //        case RobotStatusColorCode.ROBOT_STATUS_OK:
        //            border.Background = new SolidColorBrush(Colors.Blue);
        //            break;
        //        case RobotStatusColorCode.ROBOT_STATUS_ERROR:
        //            border.Background = new SolidColorBrush(Colors.Red);
        //            break;
        //    }
        //}

        public void setColorRobotStatus(RobotStatusColorCode rsc)
        {
            base.setColorRobotStatus(rsc, this);
        }

        private void ChangeToolTipContent(object sender, ToolTipEventArgs e)
        {
            try
            {
                TypeZone typezone = trafficManagementService.GetTypeZone(properties.pose.Position, 0, 200);
                double angle = -properties.pose.Angle;
                Point position = Global_Object.CoorLaser(properties.pose.Position);
                border.ToolTip = "Name: " + properties.Label + Environment.NewLine + "Zone: " + typezone +
                    Environment.NewLine + " Location: " + position.X.ToString("0.00") + " / " +
                    position.Y.ToString("0.00") + " / " + angle.ToString("0.00") + Environment.NewLine +
                    "Working Zone: " + robotRegistryToWorkingZone.WorkingZone + Environment.NewLine +
                    "Radius _S" + Radius_S + Environment.NewLine +
                    "Radius _Y" + Radius_Y + Environment.NewLine +
                    "Radius _B" + Radius_B + Environment.NewLine +
                    "Speed Set :" + properties.speedInSpecicalArea + Environment.NewLine +
                    "STATE: " + STATE_SPEED + Environment.NewLine +
                    "MIDDLE :" + MiddleHeaderCv().X.ToString("0.00") + " /" + MiddleHeaderCv().Y.ToString("0.00") + Environment.NewLine+
                    "MIDDLE1 :" + MiddleHeaderCv1().X.ToString("0.00") + " /" + MiddleHeaderCv1().Y.ToString("0.00") + Environment.NewLine+
                    "MIDDLE2 :" + MiddleHeaderCv2().X.ToString("0.00") + " /" + MiddleHeaderCv2().Y.ToString("0.00") + Environment.NewLine +
                    "ValueR SC:" + valueSC+ Environment.NewLine +
                    "ValueR BigC:" + valueBigC + Environment.NewLine+
                    "RobotTag:" + robotTag + Environment.NewLine;
            }
            catch { }
        }

        private void PoblemSolutionItem(object sender, RoutedEventArgs e)
        {
            DisplaySolvedForm();
        }
        private void PauseMenu(object sender, RoutedEventArgs e)
        {
            SetSpeed(RobotSpeedLevel.ROBOT_SPEED_STOP);
        }
        private void AddReadyListMenu(object sender, RoutedEventArgs e)
        {
            string msgtext = "Hãy đảm bảo Robot đã được đưa về vị trí trạm Sạc, Quy trình trước đó sẽ được hủy !";
            string txt = "Cảnh báo";
            MessageBoxButton button = MessageBoxButton.OKCancel;
            MessageBoxResult result = MessageBox.Show(msgtext, txt, button);
            switch (result)
            {
                case MessageBoxResult.OK:
                    DisposeProcedure();
                    TurnOnSupervisorTraffic(false);
                    this.PreProcedureAs = ProcedureControlAssign.PRO_READY;
                    robotService.RemoveRobotUnityReadyList(this);
                    robotService.RemoveRobotUnityWaitTaskList(this);
                    robotService.AddRobotUnityReadyList(this);
                    Draw();
                    break;
                case MessageBoxResult.Cancel:
                    break;
            }
        }
        public void LogOut(object sender, RoutedEventArgs e)
        {
            robotLogOut.Show();
        }
        public void ShowText(String text)
        {
            robotLogOut.ShowText(this.properties.Label, text);

        }
        private void ReConnectMenu(object sender, RoutedEventArgs e)
        {
            if (webSocket != null)
            {
                webSocket.Connect();
            }
        }
        private void ConnectMenu(object sender, RoutedEventArgs e)
        {

            // Dispose();
            if (!properties.IsConnected)
            {
                onBinding = true;
                Start(properties.Url);
                connectItem.IsEnabled = false;
                disconnectItem.IsEnabled = true;
                MessageBox.Show("Để robot có thể tiếp tục hãy add Robot vào Ready Mode hoặc TaskWait Mode !");
                Radius_S = 4 * properties.Scale;
                Radius_B = 4 * properties.Scale;
                Radius_Y = 4 * properties.Scale;
                Center_S = 0;
                Center_B = 3 * properties.Scale;
                Center_Y = 3 * properties.Scale;
                SetSpeed(RobotSpeedLevel.ROBOT_SPEED_NORMAL);
            }
        }
        private void DisConnectMenu(object sender, RoutedEventArgs e)
        {

            DisposeProcedure();
            Dispose();
            KillPID();
            MessageBox.Show("Đã Xóa Khỏi  Ready Mode hoặc TaskWait Mode !");
            onBinding = false;
            Reset();
            Draw();

        }
        public void Reset()
        {
            properties.pose.Position = properties.poseRoot.Position;
            properties.pose.Angle = properties.poseRoot.Angle;
            properties.pose.AngleW = properties.poseRoot.AngleW;
            connectItem.IsEnabled = true;
            disconnectItem.IsEnabled = false;
            TurnOnSupervisorTraffic(false);
            properties.IsConnected = false;
            robotService.RemoveRobotUnityReadyList(this);
            robotService.RemoveRobotUnityWaitTaskList(this);
            robotRegistryToWorkingZone.Release();
            setColorRobotStatus(RobotStatusColorCode.ROBOT_STATUS_DISCONNECT);
            Radius_S =0;
            Radius_B = 0;
            Radius_Y = 0;
            Center_S = 0;
            Center_B = 0;
            Center_Y = 0;
        }
        public void SetOnOffTrafficMenu(object sender, RoutedEventArgs e)
        {

            TurnOnSupervisorTraffic(onFlagSupervisorTraffic ? false : true);
        }
        public void AddWaitTaskListMenu(object sender, RoutedEventArgs e)
        {


            string msgtext = "Quy trình trước đó sẽ được hủy, Robot sẽ bắt đầu với quy trình mới !";
            string txt = "Cảnh báo";
            MessageBoxButton button = MessageBoxButton.OKCancel;
            MessageBoxResult result = MessageBox.Show(msgtext, txt, button);
            switch (result)
            {
                case MessageBoxResult.OK:

                    DisposeProcedure();
                    TurnOnSupervisorTraffic(true);
                    this.PreProcedureAs = ProcedureControlAssign.PRO_IDLE;
                    robotService.RemoveRobotUnityReadyList(this);
                    robotService.RemoveRobotUnityWaitTaskList(this);
                    robotService.AddRobotUnityWaitTaskList(this);
                    Draw();
                    break;
                case MessageBoxResult.Cancel:
                    break;
            }
        }
        private void StartMenu(object sender, RoutedEventArgs e)
        {
            SetSpeed(RobotSpeedLevel.ROBOT_SPEED_NORMAL);
        }
        public void RemoveDraw()
        {
            this.canvas.Children.Remove(border);
            this.canvas.Children.Remove(headerPoint);
            this.canvas.Children.Remove(safetyArea);
            //RemoveHandle(props.name);
        }
        public override void Draw()
        {

            try
            {
                if (properties.IsConnected)
                {
                    setColorRobotStatus(RobotStatusColorCode.ROBOT_STATUS_CONNECT);
                }
                else
                {
                    setColorRobotStatus(RobotStatusColorCode.ROBOT_STATUS_DISCONNECT);
                }

                //  if (properties.IsConnected)
                {

                    //Render Robot
                    this.border.Dispatcher.BeginInvoke(new System.Threading.ThreadStart(() =>
                    {

                        props.rbRotateTransform.Angle = -properties.pose.Angle;
                        Point cPoint = Global_Object.CoorCanvas(properties.pose.Position);
                        // Console.WriteLine("Robot : " + cPoint.X + " / " + cPoint.Y);
                        //props.rbTranslate = new TranslateTransform(cPoint.X, cPoint.Y);

                        props.rbTranslate = new TranslateTransform(cPoint.X - (border.Width / 2), cPoint.Y - (border.Height / 2));
                        //  Console.WriteLine("----------");
                        //  Console.WriteLine("Translate : " + props.rbTranslate.X + " / "+ props.rbTranslate.Y);

                        props.rbTransformGroup.Children[1] = props.rbTranslate;
                        //Render Status
                        props.contentRotateTransform.Angle = (properties.pose.Angle);
                        props.contentTranslate = new TranslateTransform(0, 0);
                        props.contentTransformGroup.Children[1] = props.contentTranslate;
                        // headerPoint.RenderTransform = new TranslateTransform(MiddleHeader().X-5, MiddleHeader().Y-5);
                        // headerPoint.RenderTransform = new TranslateTransform(Global_Object.CoorCanvas(MiddleHeader()).X-5, Global_Object.CoorCanvas(MiddleHeader()).Y+0.5);
                        headerPoint.RenderTransform = new TranslateTransform(MiddleHeaderCv().X - 2.5, MiddleHeaderCv().Y - 1);
                        headerPoint1.RenderTransform = new TranslateTransform(MiddleHeaderCv1().X - 2.5, MiddleHeaderCv1().Y - 1);
                        headerPoint2.RenderTransform = new TranslateTransform(MiddleHeaderCv2().X - 2.5, MiddleHeaderCv2().Y - 1);
                        headerPoint3.RenderTransform = new TranslateTransform(MiddleHeaderCv3().X - 2.5, MiddleHeaderCv3().Y - 1);


                        PathGeometry pgeometry = new PathGeometry();
                        PathFigure pF = new PathFigure();
                        pF.StartPoint = TopHeaderCv();

                        // pF.StartPoint = new Point(TopHeader().X * 10, TopHeader().Y * 10);
                        LineSegment pp = new LineSegment();

                        pF.Segments.Add(new LineSegment() { Point = BottomHeaderCv() });
                        pF.Segments.Add(new LineSegment() { Point = BottomTailCv() });
                        pF.Segments.Add(new LineSegment() { Point = TopTailCv() });
                        pF.Segments.Add(new LineSegment() { Point = TopHeaderCv() });
                        // pF.Segments.Add(new LineSegment() { Point = new Point(BottomHeader().X*10, BottomHeader().Y * 10) });
                        //pF.Segments.Add(new LineSegment() { Point = new Point(BottomTail().X * 10, BottomTail().Y * 10) });
                        //  pF.Segments.Add(new LineSegment() { Point = new Point(TopTail().X * 10, TopTail().Y * 10) });
                        //pF.Segments.Add(new LineSegment() { Point = new Point(TopHeader().X * 10, TopHeader().Y * 10) });
                        pgeometry.Figures.Add(pF);
                        safetyArea.Data = pgeometry;

                        props.rbID.Content = properties.pose.Position.X.ToString("0");
                        props.rbTask.Content = properties.pose.Position.Y.ToString("0");


                        if (onFlagSafeSmallcircle)
                        {
                            SmallCircleArea.Width = 2 * Radius_S;
                            SmallCircleArea.Height = 2 * Radius_S;

                            TranslateTransform tr = new TranslateTransform(cPoint.X - (2 * Radius_S / 2), cPoint.Y - (2 * Radius_S / 2));
                            SmallCircleArea.RenderTransform = tr;
                        }
                        else
                        {

                            SmallCircleArea.Width = Radius_S;
                            SmallCircleArea.Height = Radius_S;

                            TranslateTransform tr = new TranslateTransform(0, 0);
                            SmallCircleArea.RenderTransform = tr;
                        }

                        if (onFlagSafeYellowcircle)
                        {
                            YellowCircleArea.Width = 2 * Radius_Y;
                            YellowCircleArea.Height = 2 * Radius_Y;

                            Point cc = CenterOnLineCv(30);
                            TranslateTransform tr = new TranslateTransform(cc.X - (2 * Radius_Y / 2), cc.Y - (2 * Radius_Y / 2));
                            YellowCircleArea.RenderTransform = tr;
                        }
                        else
                        {
                            YellowCircleArea.Width = Radius_Y;
                            YellowCircleArea.Height = Radius_Y;

                            Point cc = CenterOnLineCv(30);
                            TranslateTransform tr = new TranslateTransform(0, 0);
                            YellowCircleArea.RenderTransform = tr;
                        }

                        if (onFlagSafeBluecircle)
                        {
                            BlueCircleArea.Width = 2 * Radius_B;
                            BlueCircleArea.Height = 2 * Radius_B;

                            Point cc = CenterOnLineCv(30);
                            TranslateTransform tr = new TranslateTransform(cc.X - (2 * Radius_B / 2), cc.Y - (2 * Radius_B / 2));
                            BlueCircleArea.RenderTransform = tr;
                        }
                        else
                        {
                            BlueCircleArea.Width = Radius_B;
                            BlueCircleArea.Height = Radius_B;

                            Point cc = CenterOnLineCv(30);
                            TranslateTransform tr = new TranslateTransform(0, 0);
                            BlueCircleArea.RenderTransform = tr;
                        }


                    }));
                }

            }
            catch { }

        }

        public override void UpdateProperties(PropertiesRobotUnity proR)
        {

            base.UpdateProperties(proR);
            DfL1 = proR.L1;
            DfL2 = proR.L2;
            DfWS = proR.WS;
            DfDistanceInter = proR.DistInter;

            DfL1Cv = proR.L1 * properties.Scale;
            DfL2Cv = proR.L2 * properties.Scale;
            DfWSCv = proR.WS * properties.Scale;
            DfDistInterCv = proR.DistInter * properties.Scale;

            L1Cv = proR.L1 * properties.Scale;
            L2Cv = proR.L2 * properties.Scale;
            WSCv = proR.WS * properties.Scale;
            DistInterCv = proR.DistInter * properties.Scale;
            //Draw ();

        }
    }
}
