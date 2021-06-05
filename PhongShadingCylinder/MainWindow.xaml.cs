using PhongShadingCylinder.Transformations;
using System;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PhongShadingCylinder
{
    public partial class MainWindow : Window
    {
        Vector4 pointFrontLeftBottom;
        Vector4 pointFrontRightBottom;
        Vector4 pointFrontLeftTop;
        Vector4 pointFrontRightTop;
        Vector4 pointBackLeftBottom;
        Vector4 pointBackRightBottom;
        Vector4 pointBackLeftTop;
        Vector4 pointBackRightTop;
        Vector4 pointDiamondTop;
        Vector4 pointDiamondBottom;

        private float SizeX = 30, SizeY = 30, SizeZ = 40;
        private float angle = 0;
        private float deltaAngle = 1;
        private Matrix4x4 transformMatrix;

        private new float Width => (float)base.Width;
        private new float Height => (float)base.Height;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            InitializePoints();
            var dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(rotateShape);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 20);
            dispatcherTimer.Start();
            //var meshCreator = new MeshCreator();
            //var mesh = meshCreator.CreateCylinderMesh(10, 10, new Vector3(0, 0, 0), 5);
        }

        private void rotateShape(object sender, EventArgs e)
        {
            Redraw();
            angle += deltaAngle;
        }

        private void InitializePoints()
        {
            pointFrontLeftBottom = new Vector4(-SizeX, -SizeY, -SizeZ, 1);
            pointFrontRightBottom = new Vector4(SizeX, -SizeY, -SizeZ, 1);
            pointFrontLeftTop = new Vector4(-SizeX, SizeY, -SizeZ, 1);
            pointFrontRightTop = new Vector4(SizeX, SizeY, -SizeZ, 1);
            pointBackLeftBottom = new Vector4(-SizeX, -SizeY, SizeZ, 1);
            pointBackRightBottom = new Vector4(SizeX, -SizeY, SizeZ, 1);
            pointBackLeftTop = new Vector4(-SizeX, SizeY, SizeZ, 1);
            pointBackRightTop = new Vector4(SizeX, SizeY, SizeZ, 1);
            pointDiamondTop = new Vector4(0, 2 * SizeY, 0, 1);
            pointDiamondBottom = new Vector4(0, -2 * SizeY, 0, 1);
        }

        private void Redraw()
        {
            Scene.Children.Clear();
            CalclateTransformMatrix();
            DrawDiamond();
        }

        private void CalclateTransformMatrix()
        {
            var angleRadians = Rotator.AngleToRadians(angle);
            
            transformMatrix = (
                                Rotator.RotationMatrix(new Vector3(0, angleRadians, 0))
                                * 
                                Translator.TranslationMatrix(new Vector3(0, 0, 150))
                              )
                                * 
                                PerspectiveProjector.ProjectionMatrix(Width, Height);
        }

        private Vector2 Project(Vector4 vector)
        {
            var tempRes = Vector4.Transform(vector, transformMatrix);
            var result3d = PerspectiveProjector.Normalize(tempRes);
            var result2d = CoordinateTranslator.Translate3dTo2d(result3d, Width, Height);
            return result2d;
        }

        private void DrawLine(Vector2 p1, Vector2 p2)
        {
            var line = new Line();
            line.X1 = p1.X;
            line.Y1 = p1.Y;
            line.X2 = p2.X;
            line.Y2 = p2.Y;
            line.Stroke = Brushes.Pink;
            line.StrokeThickness = 3;
            line.StrokeEndLineCap = PenLineCap.Round;
            Scene.Children.Add(line);
        }

        private void DrawDiamond()
        {
            // FRONT
            DrawLine(Project(pointFrontLeftBottom),
                    Project(pointFrontRightBottom));

            DrawLine(Project(pointFrontLeftBottom),
                    Project(pointFrontLeftTop));

            DrawLine(Project(pointFrontRightTop),
                    Project(pointFrontLeftTop));

            DrawLine(Project(pointFrontRightTop),
                    Project(pointFrontRightBottom));

            // SIDES
            DrawLine(Project(pointFrontLeftBottom),
                    Project(pointBackLeftBottom));

            DrawLine(Project(pointFrontRightBottom),
                    Project(pointBackRightBottom));

            DrawLine(Project(pointFrontLeftTop),
                    Project(pointBackLeftTop));

            DrawLine(Project(pointFrontRightTop),
                    Project(pointBackRightTop));

            // BACK

            DrawLine(Project(pointBackLeftBottom),
                    Project(pointBackRightBottom));

            DrawLine(Project(pointBackLeftBottom),
                    Project(pointBackLeftTop));

            DrawLine(Project(pointBackRightTop),
                    Project(pointBackLeftTop));

            DrawLine(Project(pointBackRightTop),
                    Project(pointBackRightBottom));

            // TOP DIAMOND

            DrawLine(Project(pointDiamondTop),
                    Project(pointBackLeftTop));

            DrawLine(Project(pointDiamondTop),
                    Project(pointBackRightTop));

            DrawLine(Project(pointDiamondTop),
                    Project(pointFrontLeftTop));

            DrawLine(Project(pointDiamondTop),
                    Project(pointFrontRightTop));

            // BOTTOM DIAMOND

            DrawLine(Project(pointDiamondBottom),
                    Project(pointBackLeftBottom));

            DrawLine(Project(pointDiamondBottom),
                    Project(pointBackRightBottom));

            DrawLine(Project(pointDiamondBottom),
                    Project(pointFrontLeftBottom));

            DrawLine(Project(pointDiamondBottom),
                    Project(pointFrontRightBottom));

        }


        //private void rotateShape(object sender, EventArgs e)
        //{
        //    Scene.Children.Clear();
        //    if (one)
        //    {
        //        var poly = new Polygon();
        //        var points = new PointCollection();
        //        points.Add(new Point(0, 0));
        //        points.Add(new Point(100, 0));
        //        points.Add(new Point(0, 100));
        //        poly.Points = points;
        //        poly.Stroke = Brushes.Black;
        //        poly.StrokeThickness = 2;
        //        poly.Fill = Brushes.Blue;
        //        one = false;
        //        Scene.Children.Add(poly);
        //    }
        //    else
        //    {
        //        var line = new Line();
        //        line.X1 = 0;
        //        line.Y1 = 0;
        //        line.X2 = 100;
        //        line.Y2 = 150;
        //        line.Stroke = Brushes.Pink;
        //        line.StrokeThickness = 3;
        //        one = true;
        //        Scene.Children.Add(line);
        //    }
        //}
    }
}
