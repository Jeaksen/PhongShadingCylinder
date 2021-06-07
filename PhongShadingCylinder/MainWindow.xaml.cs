using PhongShadingCylinder.Transformations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PhongShadingCylinder
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private Matrix4x4 transformMatrix;
        private Mesh mesh = null;
        private MeshCreator meshCreator = new MeshCreator();
        private Dictionary<Key, EventHandler> keyEventHandlers = new();
        private Dictionary<Key, bool> keyEventsHandled = new();
        private DispatcherTimer dispatcher = new DispatcherTimer();
        private Point previousMousePosition;
        private float deltaAngle = 1;
        private float deltaPosition = 1;
        private float _angleX = 0;
        private float _angleY = 0;
        private float _angleZ = 0;
        private float cylinderAngleX = 0;
        private float cylinderAngleY = 0;
        private float cylinderAngleZ = 0;
        private float _positionX = 0;
        private float _positionY = 0;
        private float cylinderRadius = 40;
        private float cylinderHeight = 70;
        private int cylinderDivisionPoints = 40;

        private new float Width => (float)Scene.ActualWidth;
        private new float Height => (float)Scene.ActualHeight;

        public bool DrawNormals { get; set; }
        public bool FillTriangles { get; set; }
        public bool DrawLines { get; set; } = true;

        public float PositionX
        {
            get => _positionX;
            set
            {
                if (value != _positionX)
                {
                    _positionX = value;
                    RaisePropertyChanged();
                    Redraw();
                }
            }
        }

        public float PositionY
        {
            get => _positionY;
            set
            {
                if (value != _positionY)
                {
                    _positionY = value;
                    RaisePropertyChanged();
                    Redraw();
                }
            }
        }

        public float AngleX
        {
            get => _angleX;
            set
            {
                if (value != _angleX)
                {
                    _angleX = value;
                    RaisePropertyChanged();
                    Redraw();
                }
            }
        }

        public float AngleY
        {
            get => _angleY;
            set
            {
                if (value != _angleY)
                {
                    _angleY = value;
                    RaisePropertyChanged();
                    Redraw();
                }
            }
        }

        private Vector3 CameraPosition => new Vector3(PositionX, PositionY, 0);
        private Vector3 CameraRotation => new Vector3(Rotator.AngleToRadians(AngleX), Rotator.AngleToRadians(AngleY), 0);


        public MainWindow()
        {
            InitializeComponent();
            InitializeKeyEventHandlers();
            InitializeDispatcher();
            previousMousePosition = Mouse.GetPosition(this);
            DataContext = this;
            mesh = meshCreator.CreateCylinderMesh(cylinderRadius, cylinderHeight, new Vector3(0, -cylinderHeight / 2, 0), cylinderDivisionPoints);
            Loaded += new RoutedEventHandler(WindowInitialized);
        }

        private void WindowInitialized(object sender, RoutedEventArgs e)
        {
            Redraw();
        }

        private void InitializeKeyEventHandlers()
        {
            keyEventHandlers.Add(Key.Left, new EventHandler(MoveCameraLeft));
            keyEventsHandled.Add(Key.Left, false);
            keyEventHandlers.Add(Key.Right, new EventHandler(MoveCameraRight));
            keyEventsHandled.Add(Key.Right, false);
            keyEventHandlers.Add(Key.Up, new EventHandler(MoveCameraUp));
            keyEventsHandled.Add(Key.Up, false);
            keyEventHandlers.Add(Key.Down, new EventHandler(MoveCameraDown));
            keyEventsHandled.Add(Key.Down, false);
        }

        private void InitializeDispatcher()
        {
            dispatcher.Interval = new TimeSpan(0, 0, 0, 0, 20);
            dispatcher.Start();
        }

        private void rotateShape(object sender, EventArgs e)
        {
            _angleX += deltaAngle;
            Redraw();
        }

        private void Redraw()
        {
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
                if (p1.HasValue && p2.HasValue && p3.HasValue)
                {
                    DrawTriangle(p1.Value, p2.Value, p3.Value);
                    if (DrawNormals)
                    {
                        var p1Normal = Project(GetNormalEndPoint(triangle.Vertices[0]));
                        var p2Normal = Project(GetNormalEndPoint(triangle.Vertices[1]));
                        var p3Normal = Project(GetNormalEndPoint(triangle.Vertices[2]));
                        DrawLine(p1.Value, p1Normal.Value);
                        DrawLine(p2.Value, p2Normal.Value);
                        DrawLine(p3.Value, p3Normal.Value);
                    }
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
            var cylinderAngleRadiansX = Rotator.AngleToRadians(cylinderAngleX);
            var cylinderAngleRadiansY = Rotator.AngleToRadians(cylinderAngleY);
            var cylinderAngleRadiansZ = Rotator.AngleToRadians(cylinderAngleZ);

            transformMatrix = (
                                Rotator.RotationMatrix(new Vector3(cylinderAngleRadiansX, cylinderAngleRadiansY, cylinderAngleRadiansZ))
                                *
                                Translator.TranslationMatrix(new Vector3(0, 0, 150))
                              );
        }

        private Vector2? Project(Vector3 vector)
        {
            var transformed = Vector4.Transform(vector, transformMatrix);
            var cameraTransformed = CameraTransformer.Transform(transformed, CameraPosition, CameraRotation);
            if (cameraTransformed.Z < 0)
                return null;
            var projected = PerspectiveProjector.Project(cameraTransformed, Width, Height);
            //var result3d = PerspectiveProjector.Normalize(transformed);
            var result2d = CoordinateTranslator.Translate3dTo2d(projected, Width, Height);
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

        private void Scene_MouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(this);
            if (e.RightButton == MouseButtonState.Pressed)
            {
                var vector = Point.Subtract(position, previousMousePosition);
                AngleX += (float)vector.Y / 10;
                AngleY += (float)vector.X / 10;
                Console.WriteLine("Left");
                e.Handled = true;
            }
            previousMousePosition = position;
        }

        private void Scene_KeyDown(object sender, KeyEventArgs e)
        {
            if (keyEventsHandled.TryGetValue(e.Key, out bool pressed) && !pressed)
            {
                dispatcher.Tick += keyEventHandlers.GetValueOrDefault(e.Key);
                keyEventsHandled[e.Key] = true;
            }
        }

        private void Scene_KeyUp(object sender, KeyEventArgs e)
        {
            if (keyEventsHandled.TryGetValue(e.Key, out bool pressed) && pressed)
            {
                dispatcher.Tick -= keyEventHandlers.GetValueOrDefault(e.Key);
                keyEventsHandled[e.Key] = false;
            }
        }

        private void MoveCameraLeft(object sender, EventArgs e)
        {
            PositionX += deltaPosition;
        }

        private void MoveCameraRight(object sender, EventArgs e)
        {
            PositionX -= deltaPosition;
        }

        private void MoveCameraUp(object sender, EventArgs e)
        {
            PositionY += deltaPosition;
        }

        private void MoveCameraDown(object sender, EventArgs e)
        {
            PositionY -= deltaPosition;
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var m = Matrix4x4.CreateRotationX(3.14f);
            Console.WriteLine(m);
        }


        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null && propertyName != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
