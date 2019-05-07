using System;
using System.Windows;
using SeldatUnilever_Ver1._02.Management.RobotManagent;
/*L1=10
L2=5
H=6
theta=-%pi/2;
X1=sqrt(L1^2+(H/2)^2)*cos(theta+atan(H/2,L1))
Y1=sqrt(L1^2+(H/2)^2)*sin(theta+atan(H/2,L1))

X2=sqrt(L1^2+(H/2)^2)*cos(theta+atan(-H/2,L1))
Y2=sqrt(L1^2+(H/2)^2)*sin(theta+atan(-H/2,L1))


X3=sqrt(L2^2+(H/2)^2)*cos(theta+atan(-H/2,-L2))
Y3=sqrt(L2^2+(H/2)^2)*sin(theta+atan(-H/2,-L2))

X4=sqrt(L2^2+(H/2)^2)*cos(theta+atan(H/2,-L2))
Y4=sqrt(L2^2+(H/2)^2)*sin(theta+atan(H/2,-L2))

X=[ X1 X2 X3 X4]
Y=[ Y1 Y2 Y3 Y4]
plot([0 X1],[0 Y1],'g.-')
plot([0 X2],[0 Y2],'g-.')
plot([0 X3],[0 Y3],'.g-')*/
// Three checked points have intersection : TopHeader/ Middle Header / Bottom Header
// These three points must contact to the risk areas
namespace SeldatMRMS.Management.RobotManagent
{

    public class RobotUnityService : RobotUnityControl
    {

        public double DistInterCv;
        public double L1Cv;
        public double L2Cv;
        public double WSCv;

        public double DfL1 = 40;
        public double DfL2 = 40;
        public double DfWS = 50;
        public double DfDistanceInter = 40;

        public double DfL1Cv = 40;
        public double DfL2Cv = 40;
        public double DfWSCv = 50;//60
        public double DfDistInterCv = 40;

        public double Radius_S;
        public double Radius_B;
        public double Radius_Y;
        public double Center_S;
        public double Center_B;
        public double Center_Y;

        public RobotUnityService()
        {
        }

        public void UpdateRiskAraParams(double L1, double L2, double WS, double distanceInter)
        {
            properties.L1 = L1;
            properties.L2 = L2;
            properties.WS = WS;
            properties.DistInter = distanceInter;

            L1Cv = L1 * properties.Scale;
            L2Cv = L2 * properties.Scale;
            WSCv = WS * properties.Scale;
            DistInterCv = distanceInter * properties.Scale;
            //Draw();

        }
        public virtual Point TopHeader()
        {
            double x = properties.pose.Position.X + Math.Sqrt((Math.Abs(properties.L1) * Math.Abs(properties.L1)) + Math.Abs(properties.WS / 2) * Math.Abs(properties.WS / 2)) * Math.Cos(properties.pose.AngleW + Math.Atan2(-properties.WS / 2, properties.L1));
            double y = properties.pose.Position.Y + Math.Sqrt(Math.Abs(properties.L1) * Math.Abs(properties.L1) + Math.Abs(properties.WS / 2) * Math.Abs(properties.WS / 2)) * Math.Sin(properties.pose.AngleW + Math.Atan2(-properties.WS / 2, properties.L1));
            return new Point(x, y);
        }
        public virtual Point BottomHeaderCv()//TopHeaderCv()// 
        {
            double AngleW = -properties.pose.AngleW;
            double x = Global_Object.CoorCanvas(properties.pose.Position).X + Math.Sqrt((Math.Abs(L1Cv) * Math.Abs(L1Cv)) + Math.Abs(WSCv / 2) * Math.Abs(WSCv / 2)) * Math.Cos(AngleW + Math.Atan2(-WSCv / 2, L1Cv));
            double y = Global_Object.CoorCanvas(properties.pose.Position).Y + Math.Sqrt(Math.Abs(L1Cv) * Math.Abs(L1Cv) + Math.Abs(WSCv / 2) * Math.Abs(WSCv / 2)) * Math.Sin(AngleW + Math.Atan2(-WSCv / 2, L1Cv));
            return new Point(x, y);
        }
        public virtual Point BottomHeader()
        {
            double x = properties.pose.Position.X + Math.Sqrt(Math.Abs(properties.L1) * Math.Abs(properties.L1) + Math.Abs(properties.WS / 2) * Math.Abs(properties.WS / 2)) * Math.Cos(properties.pose.AngleW + Math.Atan2(properties.WS / 2, properties.L1));
            double y = properties.pose.Position.Y + Math.Sqrt(Math.Abs(properties.L1) * Math.Abs(properties.L1) + Math.Abs(properties.WS / 2) * Math.Abs(properties.WS / 2)) * Math.Sin(properties.pose.AngleW + Math.Atan2(properties.WS / 2, properties.L1));
            return new Point(x, y);
        }
        public virtual Point TopHeaderCv()//BottomHeaderCv()
        {
            double AngleW = -properties.pose.AngleW;
            double x = Global_Object.CoorCanvas(properties.pose.Position).X + Math.Sqrt(Math.Abs(L1Cv) * Math.Abs(L1Cv) + Math.Abs(WSCv / 2) * Math.Abs(WSCv / 2)) * Math.Cos(AngleW + Math.Atan2(WSCv / 2, L1Cv));
            double y = Global_Object.CoorCanvas(properties.pose.Position).Y + Math.Sqrt(Math.Abs(L1Cv) * Math.Abs(L1Cv) + Math.Abs(WSCv / 2) * Math.Abs(WSCv / 2)) * Math.Sin(AngleW + Math.Atan2(WSCv / 2, L1Cv));
            return new Point(x, y);
        }
        public virtual Point MiddleHeader()
        {
            return new Point((TopHeader().X + BottomHeader().X) / 2, (TopHeader().Y + BottomHeader().Y) / 2);
        }

        public virtual Point MiddleHeaderCv()
        {
            if (L1Cv != 0 && WSCv != 0)
                return new Point((TopHeaderCv().X + BottomHeaderCv().X) / 2, (TopHeaderCv().Y + BottomHeaderCv().Y) / 2);
            else
                return new Point (-1,-1);

        }

        public virtual Point MiddleHeaderCv1()
        {
            double PRx = Global_Object.CoorCanvas(properties.pose.Position).X;
            double PRy = Global_Object.CoorCanvas(properties.pose.Position).Y;
            if (L1Cv != 0 && WSCv != 0)
                return new Point((PRx + MiddleHeaderCv().X) / 2, (PRy + MiddleHeaderCv().Y) / 2);
            else
                return new Point(-1, -1);
        }
        public virtual Point MiddleHeaderCv2()
        {
            if (L1Cv != 0 && WSCv != 0)
                return new Point((MiddleHeaderCv1().X + MiddleHeaderCv().X) / 2, (MiddleHeaderCv1().Y + MiddleHeaderCv().Y) / 2);
            else
                return new Point(-1, -1);
        }
        public virtual Point MiddleHeaderCv3()
        {
            double PRx = Global_Object.CoorCanvas(properties.pose.Position).X;
            double PRy = Global_Object.CoorCanvas(properties.pose.Position).Y;
            if (L1Cv != 0 && WSCv != 0)
                return new Point((MiddleHeaderCv1().X + PRx) / 2, (MiddleHeaderCv1().Y + PRy) / 2);
            else
                return new Point(-1, -1);
        }

        public virtual Point TopTail()
        {
            double x = properties.pose.Position.X + Math.Sqrt(Math.Abs(properties.L2) * Math.Abs(properties.L2) + Math.Abs(properties.WS / 2) * Math.Abs(properties.WS / 2)) * Math.Cos(properties.pose.AngleW + Math.Atan2(-properties.WS / 2, -properties.L2));
            double y = properties.pose.Position.Y + Math.Sqrt(Math.Abs(properties.L2) * Math.Abs(properties.L2) + Math.Abs(properties.WS / 2) * Math.Abs(properties.WS / 2)) * Math.Sin(properties.pose.AngleW + Math.Atan2(-properties.WS / 2, -properties.L2));
            return new Point(x, y);
        }
        public virtual Point BottomTailCv()//TopTailCv()
        {
            double AngleW = -properties.pose.AngleW;
            double x = Global_Object.CoorCanvas(properties.pose.Position).X + Math.Sqrt(Math.Abs(L2Cv) * Math.Abs(L2Cv) + Math.Abs(WSCv / 2) * Math.Abs(WSCv / 2)) * Math.Cos(AngleW + Math.Atan2(-WSCv / 2, -L2Cv));
            double y = Global_Object.CoorCanvas(properties.pose.Position).Y + Math.Sqrt(Math.Abs(L2Cv) * Math.Abs(L2Cv) + Math.Abs(WSCv / 2) * Math.Abs(WSCv / 2)) * Math.Sin(AngleW + Math.Atan2(-WSCv / 2, -L2Cv));
            return new Point(x, y);
        }

        public virtual Point MiddleTail()
        {
            return new Point((TopTail().X + BottomTail().X) / 2, (TopTail().Y + BottomTail().Y) / 2);
        }
        public virtual Point BottomTail()
        {
            double x = properties.pose.Position.X + Math.Sqrt(Math.Abs(properties.L2) * Math.Abs(properties.L2) + Math.Abs(properties.WS / 2) * Math.Abs(properties.WS / 2)) * Math.Cos(properties.pose.AngleW + Math.Atan2(properties.WS / 2, -properties.L2));
            double y = properties.pose.Position.Y + Math.Sqrt(Math.Abs(properties.L2) * Math.Abs(properties.L2) + Math.Abs(properties.WS / 2) * Math.Abs(properties.WS / 2)) * Math.Sin(properties.pose.AngleW + Math.Atan2(properties.WS / 2, -properties.L2));
            return new Point(x, y);
        }
        public virtual Point TopTailCv()//BottomTailCv()
        {
            double AngleW = -properties.pose.AngleW;
            double x = Global_Object.CoorCanvas(properties.pose.Position).X + Math.Sqrt(Math.Abs(L2Cv) * Math.Abs(L2Cv) + Math.Abs(WSCv / 2) * Math.Abs(WSCv / 2)) * Math.Cos(AngleW + Math.Atan2(WSCv / 2, -L2Cv));
            double y = Global_Object.CoorCanvas(properties.pose.Position).Y + Math.Sqrt(Math.Abs(L2Cv) * Math.Abs(L2Cv) + Math.Abs(WSCv / 2) * Math.Abs(WSCv / 2)) * Math.Sin(AngleW + Math.Atan2(WSCv / 2, -L2Cv));
            return new Point(x, y);
        }




        public virtual Point LeftSide()
        {
            //return new Point((BottomHeader().X + BottomTail().X) / 2, (BottomHeader().Y + BottomTail().Y) / 2);
            return new Point((TopHeader().X + TopTail().X) / 2, (TopHeader().Y + TopTail().Y) / 2);

            //Console.WriteLine("rISK left " + LeftSide());

        }// ko dung nha ^^
        public virtual Point RightSide()
        {
            //  return new Point((TopHeader().X + TopTail().X) / 2, (TopHeader().Y + TopTail().Y) / 2);

            return new Point((BottomHeader().X + BottomTail().X) / 2, (BottomHeader().Y + BottomTail().Y) / 2);
        }// ko dung 

        public virtual Point MiddleAll()
        {
            return new Point((TopHeader().X + TopTail().X) / 2, (TopHeader().Y + BottomHeader().Y) / 2);
        }

        public virtual Point MiddleAllCv()
        {
            return new Point((TopHeaderCv().X + TopTailCv().X) / 2, (TopHeaderCv().Y + BottomHeaderCv().Y) / 2);
        }

        public Point[] RiskAreaHeader()  // From Point : TopHeader / BottomHeader / RigtSide // LeftSide
        {
            //  Console.WriteLine("rISK Header " + TopHeader1() + " " + BottomHeader1() + " " + TopHeader2() + " " + BottomHeader2());
            return new Point[3] { TopHeader(), BottomHeader(), MiddleAll() };
        }

        public Point[] RiskAreaHeaderCv()  // From Point : TopHeader / BottomHeader / RigtSide // LeftSide
        {
            //  Console.WriteLine("rISK Header " + TopHeader1() + " " + BottomHeader1() + " " + TopHeader2() + " " + BottomHeader2());
            return new Point[3] { TopHeaderCv(), BottomHeaderCv(), MiddleAllCv() };
        }
        public Point[] RiskAreaTail()  // From Point : TopTail / BottomTail / RigtSide // LeftSide
        {
            // Console.WriteLine("rISK tAIL " + TopTail1() + " " + BottomTail1() + " " + TopTail2() + " " + BottomTail2());

            return new Point[3] { TopTail(), BottomTail(), MiddleAll() };
        }
        public Point[] RiskAreaTailCv()  // From Point : TopTail / BottomTail / RigtSide // LeftSide
        {
            // Console.WriteLine("rISK tAIL " + TopTail1() + " " + BottomTail1() + " " + TopTail2() + " " + BottomTail2());

            return new Point[3] { TopTailCv(), BottomTailCv(), MiddleAllCv() };
        }

        public Point[] RiskAreaRightSide()  // From Point : TopHeader / TopTail / Middle TAil //Middle HEader
        {
            //  Console.WriteLine("Right Side " + BottomHeader2() + " " + BottomTail2() + " " + RightHeader() + " " + RightTail());
            return new Point[3] { BottomTail(), BottomHeader(), MiddleAll() };

        }
        public Point[] RiskAreaRightSideCv()  // From Point : TopHeader / TopTail / Middle TAil //Middle HEader
        {
            //  Console.WriteLine("Right Side " + BottomHeader2() + " " + BottomTail2() + " " + RightHeader() + " " + RightTail());
            return new Point[3] { BottomTailCv(), BottomHeaderCv(), MiddleAllCv() };

        }
        public Point[] RiskAreaLeftSide()  // From Point : BOttom Header / Bottom Tail / Middle TAil //Middle HEader
        {
            // Console.WriteLine("Left Side "+TopHeader() +" "+ TopTail() +" "+ LeftHeader()+" "+ LeftTail());
            return new Point[3] { TopHeader(), TopTail(), MiddleAll() };
        }
        public Point[] RiskAreaLeftSideCv()  // From Point : BOttom Header / Bottom Tail / Middle TAil //Middle HEader
        {
            // Console.WriteLine("Left Side "+TopHeader() +" "+ TopTail() +" "+ LeftHeader()+" "+ LeftTail());
            return new Point[3] { TopHeaderCv(), TopTailCv(), MiddleAllCv() };
        }
        public Point[] FullRiskArea()
        {
            return new Point[4] { TopHeader(), BottomHeader(), BottomTail(), TopTail() };
        }
        public Point[] FullRiskAreaCv()
        {
            /* Console.WriteLine(TopHeaderCv().ToString());
             Console.WriteLine(BottomHeaderCv().ToString());
             Console.WriteLine(BottomTailCv().ToString());
             Console.WriteLine(TopTailCv().ToString());*/
            return new Point[4] { TopHeaderCv(), BottomHeaderCv(), BottomTailCv(), TopTailCv() };
        }
        public bool FindHeaderIsCloseRiskArea(Point p)
        {
            // return ExtensionService.CalDistance(TopHeader(),p)<properties.DistanceIntersection || ExtensionService.CalDistance(BottomHeader(), p) < properties.DistanceIntersection || ExtensionService.CalDistance(MiddleHeader(), p) < properties.DistanceIntersection ? true:false;
            //Console.WriteLine("Vi tri robot "+ this.properties.NameID+" = " + properties.pose.Position);
            // Console.WriteLine("Vi tien gan " + p.ToString());
            // Console.WriteLine("kHOAN CACH " + ExtensionService.CalDistance(properties.pose.Position, p));

            return ExtensionService.CalDistance(properties.pose.Position, p) < properties.DistInter ? true : false;

        }
        public bool FindHeaderIsCloseRiskAreaCv(Point p)
        {
            // return ExtensionService.CalDistance(TopHeader(),p)<properties.DistanceIntersection || ExtensionService.CalDistance(BottomHeader(), p) < properties.DistanceIntersection || ExtensionService.CalDistance(MiddleHeader(), p) < properties.DistanceIntersection ? true:false;
            //Console.WriteLine("Vi tri robot "+ this.properties.NameID+" = " + properties.pose.Position);
            // Console.WriteLine("Vi tien gan " + p.ToString());
            // Console.WriteLine("kHOAN CACH " + ExtensionService.CalDistance(properties.pose.Position, p));
            Point pp = Global_Object.CoorCanvas(properties.pose.Position);
            double ddd = ExtensionService.CalDistance(Global_Object.CoorCanvas(properties.pose.Position), p);
            return ExtensionService.CalDistance(Global_Object.CoorCanvas(properties.pose.Position), p) < 40 ? true : false;

        }
        public bool FindHeaderIntersectsFullRiskArea(Point p)
        {
            return ExtensionService.IsInPolygon(FullRiskArea(), p);
        }
        public bool FindHeaderIntersectsFullRiskAreaCv(Point p)
        {

            return ExtensionService.IsInPolygon(FullRiskAreaCv(), p);
        }
        public bool FindHeaderIntersectsRiskAreaHeader(Point p)
        {
            return ExtensionService.IsInPolygon(RiskAreaHeader(), p);
        }
        public bool FindHeaderIntersectsRiskAreaHeaderCv(Point p)
        {
            return ExtensionService.IsInPolygon(RiskAreaHeaderCv(), p);
        }
        public bool FindHeaderIntersectsRiskAreaTail(Point p)
        {
            return ExtensionService.IsInPolygon(RiskAreaTail(), p);
        }
        public bool FindHeaderIntersectsRiskAreaTailCv(Point p)
        {
            return ExtensionService.IsInPolygon(RiskAreaTailCv(), p);
        }
        public bool FindHeaderIntersectsRiskAreaLeftSide(Point p)
        {

            return ExtensionService.IsInPolygon(RiskAreaLeftSide(), p);
        }
        public bool FindHeaderIntersectsRiskAreaLeftSideCv(Point p)
        {

            return ExtensionService.IsInPolygon(RiskAreaLeftSideCv(), p);
        }
        public bool FindHeaderIntersectsRiskAreaRightSide(Point p)
        {
            return ExtensionService.IsInPolygon(RiskAreaRightSide(), p);
        }
        public bool FindHeaderIntersectsRiskAreaRightSideCv(Point p)
        {
            return ExtensionService.IsInPolygon(RiskAreaRightSideCv(), p);
        }

        // Tìm vị trí trọng tâm của đường tròn từ 2 điểm. tính từ tâm robot. nếu dc=0 tâm vòng tròn tại tâm robot
        public virtual Point BottomHeaderCenterCv(double dc)//TopHeaderCv()// 
        {
            double AngleW = -properties.pose.AngleW;
            double x = Global_Object.CoorCanvas(properties.pose.Position).X + Math.Sqrt((Math.Abs(dc) * Math.Abs(dc)) + Math.Abs(WSCv / 2) * Math.Abs(WSCv / 2)) * Math.Cos(AngleW + Math.Atan2(-WSCv / 2, dc));
            double y = Global_Object.CoorCanvas(properties.pose.Position).Y + Math.Sqrt(Math.Abs(dc) * Math.Abs(dc) + Math.Abs(WSCv / 2) * Math.Abs(WSCv / 2)) * Math.Sin(AngleW + Math.Atan2(-WSCv / 2, dc));
            return new Point(x, y);
        }
        public virtual Point TopHeaderCenterCv(double dc)//BottomHeaderCv()
        {
            double AngleW = -properties.pose.AngleW;
            double x = Global_Object.CoorCanvas(properties.pose.Position).X + Math.Sqrt(Math.Abs(dc) * Math.Abs(dc) + Math.Abs(WSCv / 2) * Math.Abs(WSCv / 2)) * Math.Cos(AngleW + Math.Atan2(WSCv / 2, dc));
            double y = Global_Object.CoorCanvas(properties.pose.Position).Y + Math.Sqrt(Math.Abs(dc) * Math.Abs(dc) + Math.Abs(WSCv / 2) * Math.Abs(WSCv / 2)) * Math.Sin(AngleW + Math.Atan2(WSCv / 2, dc));
            return new Point(x, y);
        }
        public virtual Point CenterOnLineCv(double dc)//BottomHeaderCv()
        {
            return new Point((BottomHeaderCenterCv(dc).X + TopHeaderCenterCv(dc).X) / 2.0, (BottomHeaderCenterCv(dc).Y + TopHeaderCenterCv(dc).Y) / 2.0);
        }
        public String valueSC = "";
        public String valueBigC = "";
        public bool FindHeaderInsideCircleArea(Point pheader, double r)
        {
            Point ccPoint = Global_Object.CoorCanvas(properties.pose.Position);
            double leftSideEq = (pheader.X - ccPoint.X) * (pheader.X - ccPoint.X) + (pheader.Y - ccPoint.Y) * (pheader.Y - ccPoint.Y);
            valueSC = Math.Sqrt(leftSideEq)+"";
            if (Math.Sqrt(leftSideEq) <= r)
                return true;
            else
                return false;
        }
        public bool FindHeaderInsideCircleArea(Point pheader, Point center, double r)
        {
            Point ccPoint = Global_Object.CoorCanvas(properties.pose.Position);
            double leftSideEq = (pheader.X - center.X) * (pheader.X - center.X) + (pheader.Y - center.Y) * (pheader.Y - center.Y);
            valueBigC = Math.Sqrt(leftSideEq) + "";
            if (Math.Sqrt(leftSideEq) <= r)
                return true;
            else
                return false;
        }
    }
}
