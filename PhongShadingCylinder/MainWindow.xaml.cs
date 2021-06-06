using PhongShadingCylinder.Transformations;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PhongShadingCylinder
{
    public partial class MainWindow : Window
    {
        private float deltaAngle = 1;
        private float angleX = 0;
        private float angleY = 0;
        private float angleZ = 0;
        private Matrix4x4 transformMatrix;
        private Mesh mesh = null;
        private MeshCreator meshCreator = new MeshCreator();

        private new float Width => (float)base.Width;
        private new float Height => (float)base.Height;

        public bool DrawNormals { get; set; }
        public bool FillTriangles { get; set; }
        public bool DrawLines { get; set; } = true;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            var dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(rotateShape);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 20);
            dispatcherTimer.Start();
            mesh = meshCreator.CreateCylinderMesh(40, 70, new Vector3(0, 0, 0), 40);

            CalclateTransformMatrix();
            DrawCylinder();
        }
        private void rotateShape(object sender, EventArgs e)
        {
            angleX += deltaAngle;
            Scene.Children.Clear();
            CalclateTransformMatrix();
            DrawCylinder();
        }

        private void DrawCylinder()
        {
            DrawTriangles(mesh.Triangles);
            //DrawTriangles(mesh.TrianglesSide);
            //DrawTriangles(mesh.TrianglesBottom);
            //DrawTriangles(mesh.TrianglesTop);
        }

        private void DrawTriangles(List<Triangle> trainagles)
        {
            foreach (var triangle in trainagles)
            {
                var p1 = Project(triangle.Vertices[0].Position);
                var p2 = Project(triangle.Vertices[1].Position);
                var p3 = Project(triangle.Vertices[2].Position);
                DrawTriangle(p1, p2, p3);
                if (DrawNormals)
                {
                    var p1Normal = Project(GetNormalEndPoint(triangle.Vertices[0]));
                    var p2Normal = Project(GetNormalEndPoint(triangle.Vertices[1]));
                    var p3Normal = Project(GetNormalEndPoint(triangle.Vertices[2]));
                    DrawLine(p1, p1Normal);
                    DrawLine(p2, p2Normal);
                    DrawLine(p3, p3Normal);
                }
            }
        }

        private Vector3 GetNormalEndPoint(Vertex vertex)
        {
            var scale = 10;
            return new Vector3 (vertex.Position.X + scale * vertex.Normal.X,
                                vertex.Position.Y + scale * vertex.Normal.Y,
                                vertex.Position.Z + scale * vertex.Normal.Z);
        }


        private void CalclateTransformMatrix()
        {
            var angleRadiansX = Rotator.AngleToRadians(angleX);
            var angleRadiansY = Rotator.AngleToRadians(angleY);
            var angleRadiansZ = Rotator.AngleToRadians(angleZ);
            
            transformMatrix = (
                                Rotator.RotationMatrix(new Vector3(angleRadiansX, angleRadiansY, angleRadiansZ))
                                * 
                                Translator.TranslationMatrix(new Vector3(0, 0, 150))
                              )
                                * 
                                PerspectiveProjector.ProjectionMatrix(Width, Height);
        }

        private Vector2 Project(Vector3 vector)
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
            line.StrokeEndLineCap = PenLineCap.Triangle;
            Scene.Children.Add(line);
        }

        private void DrawTriangle(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            var poly = new Polygon();
            var points = new PointCollection();
            points.Add(new Point(p1.X, p1.Y));
            points.Add(new Point(p2.X, p2.Y));
            points.Add(new Point(p3.X, p3.Y));
            poly.Points = points;
            poly.Stroke = Brushes.Pink;
            if (DrawLines)
                poly.StrokeThickness = 1;
            else
                poly.StrokeThickness = 0;
            if (FillTriangles)
                poly.Fill = Brushes.Blue;
            Scene.Children.Add(poly);
        }
    }
}
