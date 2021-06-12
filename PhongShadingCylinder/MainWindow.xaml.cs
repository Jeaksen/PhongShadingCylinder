using PhongShadingCylinder.Transformations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Linq;
using System.Windows.Media.Imaging;

namespace PhongShadingCylinder
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private MeshCreator meshCreator = new MeshCreator();
        private Dictionary<Key, EventHandler> keyEventHandlers = new();
        private Dictionary<Key, bool> keyEventsHandled = new();
        private DispatcherTimer dispatcher = new DispatcherTimer();
        private Point previousMousePosition;
        private LightSource lightSource;
        private Cylinder cylinder;
        private Mesh mesh = null;
        private FillingAlgorithm fillingAlgorithm = new FillingAlgorithm();
        private WriteableBitmap lightingBitmap;

        private bool _drawNormals;
        private bool _fillTriangles = true;
        private bool _drawLines;
        private Vector3 _cameraPosition = new Vector3(0, 80, -150);
        private Vector3 _cameraRotation = new Vector3(0, 0, 0);

        private new int Width => (int)(base.Width - Options.ActualWidth);
        private new int Height => (int)base.Height;
        private float ScrollDistanceMultiplier => 0.2f;
        private float MoveCameraDistance => 2f;
        private float RotateDistanceMultiplier => 0.1f;

        public bool DrawNormals
        {
            get => _drawNormals;
            set
            {
                if (value != _drawNormals)
                {
                    _drawNormals = value;
                    RaisePropertyChanged();
                    Redraw();
                }
            }
        }

        public bool FillTriangles
        {
            get => _fillTriangles;
            set
            {
                if (value != _fillTriangles)
                {
                    _fillTriangles = value;
                    RaisePropertyChanged();
                    Redraw();
                }
            }
        }

        public bool DrawLines
        {
            get => _drawLines;
            set
            {
                if (value != _drawLines)
                {
                    _drawLines = value;
                    RaisePropertyChanged();
                    Redraw();
                }
            }
        }

        public float PositionX
        {
            get => _cameraPosition.X;
            set
            {
                if (value != _cameraPosition.X)
                {
                    _cameraPosition.X = value;
                    RaisePropertyChanged();
                    Redraw();
                }
            }
        }

        public float PositionY
        {
            get => _cameraPosition.Y;
            set
            {
                if (value != _cameraPosition.Y)
                {
                    _cameraPosition.Y = value;
                    RaisePropertyChanged();
                    Redraw();
                }
            }
        }

        public float PositionZ
        {
            get => _cameraPosition.Z;
            set
            {
                if (value != _cameraPosition.Z)
                {
                    _cameraPosition.Z = value;
                    RaisePropertyChanged();
                    Redraw();
                }
            }
        }

        public float AngleX
        {
            get => _cameraRotation.X;
            set
            {
                if (value != _cameraRotation.X)
                {
                    _cameraRotation.X = value;
                    RaisePropertyChanged();
                    Redraw();
                }
            }
        }

        public float AngleY
        {
            get => _cameraRotation.Y;
            set
            {
                if (value != _cameraRotation.Y)
                {
                    _cameraRotation.Y = value;
                    RaisePropertyChanged();
                    Redraw();
                }
            }
        }

        public float AngleZ
        {
            get => _cameraRotation.Z;
            set
            {
                if (value != _cameraRotation.Z)
                {
                    _cameraRotation.Z = value;
                    RaisePropertyChanged();
                    Redraw();
                }
            }
        }

        private Vector3 CameraPosition
        {
            get => _cameraPosition;
            set
            {
                if (value != _cameraPosition)
                {
                    _cameraPosition = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged("PositionX");
                    RaisePropertyChanged("PositionY");
                    RaisePropertyChanged("PositionZ");
                    Redraw();
                }
            }
        }
      
        private Vector3 CameraRotation
        {
            get => _cameraRotation;
            set
            {
                if (value != _cameraRotation)
                {
                    _cameraRotation = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged("AngleX");
                    RaisePropertyChanged("AngleY");
                    RaisePropertyChanged("AngleZ");
                    Redraw();
                }
            }
        }


        public MainWindow()
        {
            InitializeComponent();
            InitializeKeyEventHandlers();
            previousMousePosition = Mouse.GetPosition(this);
            DataContext = this;
            lightSource = new LightSource()
            {
                Intensity = Colors.White,
                Position = new Vector3(0, 50 , lightDistance),
                AmbientColor = new Color() { R = 40, G = 40, B = 40, A = 255 }
            };

            cylinder = new Cylinder()
            {
                Height = 70,
                Radius = 40,
                Position = new Vector3(0, -70 / 2, 0),
                DivisionPointsCount = 34,
                DiffuseReflectivity = 1f,
                SpecularReflectivity = 1f,
                SpecularReflectionExponent = 100
            };
            mesh = meshCreator.CreateCylinderMesh(cylinder.Radius, cylinder.Height, cylinder.Position, cylinder.DivisionPointsCount);
            Loaded += new RoutedEventHandler(WindowInitialized);
        }


        private void WindowInitialized(object sender, RoutedEventArgs e)
        {
            InitializeDispatcher();
        }

        private void InitializeKeyEventHandlers()
        {
            keyEventHandlers.Add(Key.A, new EventHandler(MoveCameraLeft));
            keyEventsHandled.Add(Key.A, false);
            keyEventHandlers.Add(Key.D, new EventHandler(MoveCameraRight));
            keyEventsHandled.Add(Key.D, false);
            keyEventHandlers.Add(Key.W, new EventHandler(MoveCameraUp));
            keyEventsHandled.Add(Key.W, false);
            keyEventHandlers.Add(Key.S, new EventHandler(MoveCameraDown));
            keyEventsHandled.Add(Key.S, false);
            keyEventHandlers.Add(Key.E, new EventHandler(ZoomInCamera));
            keyEventsHandled.Add(Key.E, false);
            keyEventHandlers.Add(Key.Q, new EventHandler(ZoomOutCamera));
            keyEventsHandled.Add(Key.Q, false);
            keyEventHandlers.Add(Key.I, new EventHandler(RotateUpCamera));
            keyEventsHandled.Add(Key.I, false);
            keyEventHandlers.Add(Key.K, new EventHandler(RotateDownCamera));
            keyEventsHandled.Add(Key.K, false);
            keyEventHandlers.Add(Key.J, new EventHandler(RotateLeftCamera));
            keyEventsHandled.Add(Key.J, false);
            keyEventHandlers.Add(Key.L, new EventHandler(RotateRightCamera));
            keyEventsHandled.Add(Key.L, false);

        }

        private void InitializeDispatcher()
        {
            dispatcher.Interval = new TimeSpan(0, 0, 0, 0, 20);
            dispatcher.Tick += new EventHandler(RedrawScene);
            dispatcher.Tick += new EventHandler(MoveLightSource);
            dispatcher.Start();
        }


        private void Scene_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Width > 0 && Height > 0)
            {
                lightingBitmap = new WriteableBitmap(
                                        Width,
                                        Height,
                                        96,
                                        96,
                                        PixelFormats.Bgr32,
                                        null);
            }
        }

        private void Scene_MouseMove(object sender, MouseEventArgs e)
        {
            //var position = e.GetPosition(this);
            //if (e.RightButton == MouseButtonState.Pressed)
            //{
            //    var distance = Point.Subtract(position, previousMousePosition);
            //    var vector = new Vector3(-(float)(distance.Y * RotateDistanceMultiplier), (float)(distance.X * RotateDistanceMultiplier), 0);
            //    CameraRotation += vector;
            //}
            //previousMousePosition = position;
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

        private void Scene_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //var vector = new Vector3(0, 0, e.Delta * ScrollDistanceMultiplier);
            //var rotated = Rotator.Rotate(vector, CameraRotation);
            //CameraPosition += rotated;
        }

        private void RedrawScene(object sender, EventArgs e)
        {
            Scene.Children.Clear();
            ClearLightingBitmap();
            DrawLightSource();
            DrawCylinder();
            //ImgTest.Source = lightingBitmap;
        }

        private float lightAngle = 0;
        private float lightDistance = 100;
        private void MoveLightSource(object sender, EventArgs e)
        {
            lightSource.Position.X = lightDistance * MathF.Sin(lightAngle);
            lightSource.Position.Z = lightDistance * MathF.Cos(lightAngle);
            lightAngle += 0.1f;
        }


        private void Redraw()
        {
            //Scene.Children.Clear();
            //DrawLightSource();
            //DrawCylinder();
            //ClearLightingBitmap();
        }

        private void ClearLightingBitmap()
        {
            try
            {
                // Reserve the back buffer for updates.
                lightingBitmap.Lock();

                unsafe
                {
                    // Get a pointer to the back buffer.
                    IntPtr pBackBuffer = lightingBitmap.BackBuffer;
                    for (int i = 0; i < lightingBitmap.BackBufferStride * lightingBitmap.PixelHeight; i++)
                    {
                        //*((int*)pBackBuffer) = int.MaxValue;
                        *((int*)pBackBuffer) = 0;
                        pBackBuffer += 1;
                    }
                }

                // Specify the area of the bitmap that changed.
                lightingBitmap.AddDirtyRect(new Int32Rect(0, 0, Width, Height));
            }
            finally
            {
                // Release the back buffer and make it available for display.
                lightingBitmap.Unlock();
            }
        }

        private void DrawLightSource()
        {
            var point = Project(lightSource.Position);
            if (point.HasValue)
            {
                var ellipse = new Ellipse();
                ellipse.Width = 16;
                ellipse.Height = 16;
                ellipse.Fill = new SolidColorBrush(lightSource.Intensity);
                ellipse.Stroke = Brushes.Black;
                ellipse.StrokeThickness = 1;
                Scene.Children.Add(ellipse);
                Canvas.SetLeft(ellipse, point.Value.X);
                Canvas.SetTop(ellipse, point.Value.Y);
            }
        }

        private void DrawCylinder()
        {
            DrawTriangles(mesh.Triangles);
        }

        private void DrawTriangles(List<Triangle> triangles)
        {
            foreach (var triangle in triangles)
            {
                if (!IsTriangleVisible(triangle))
                    continue;

                var p1 = Project(triangle.Vertices[0].Position);
                var p2 = Project(triangle.Vertices[1].Position);
                var p3 = Project(triangle.Vertices[2].Position);
                if (p1.HasValue && p2.HasValue && p3.HasValue)
                {
                    triangle.Vertices[0].ProjectedPosition = p1.Value;
                    triangle.Vertices[1].ProjectedPosition = p2.Value;
                    triangle.Vertices[2].ProjectedPosition = p3.Value;

                    var interpolatedFillVertices = fillingAlgorithm.Fill(triangle, Width, Height);
                    var brush = CreateLightingBrush(interpolatedFillVertices);
                    if (brush == null)
                        continue;

                    DrawTriangle(p1.Value, p2.Value, p3.Value, brush);

                    if (DrawNormals)
                    {
                        var p1Normal = Project(GetNormalEndPoint(triangle.Vertices[0]));
                        var p2Normal = Project(GetNormalEndPoint(triangle.Vertices[1]));
                        var p3Normal = Project(GetNormalEndPoint(triangle.Vertices[2]));
                        if (p1Normal.HasValue)
                            DrawLine(p1.Value, p1Normal.Value);
                        if (p2Normal.HasValue)
                            DrawLine(p2.Value, p2Normal.Value);
                        if (p3Normal.HasValue)
                            DrawLine(p3.Value, p3Normal.Value);
                    }
                }
            }
        }

        private bool IsTriangleVisible(Triangle triangle)
        {
            return IsVertexVisible(triangle.Vertices[0]) 
                || IsVertexVisible(triangle.Vertices[1]) 
                || IsVertexVisible(triangle.Vertices[2]);
        }

        private bool IsVertexVisible(Vertex vertex)
        {
            var toCamera = GetVectorToCamera(vertex.Position);
            var dot = Vector3.Dot(toCamera, vertex.Normal);
            return dot > 0;
        }

        private Vector3? Project(Vector3 vector)
        {
            var input = new Vector4(vector, 1);
            var cameraTransformed = CameraTransformer.Transform(input, CameraPosition, CameraRotation);
            if (cameraTransformed.Z == 0)
                return new Vector3(cameraTransformed.X,
                                   cameraTransformed.Y,
                                   0);
            if (cameraTransformed.Z < 0)
                return null;
            var projected = PerspectiveProjector.Project(cameraTransformed, Width, Height);
            var result2d = CoordinateTranslator.Translate3dTo2d(projected, Width, Height);
            return result2d;
        }

        private ImageBrush CreateLightingBrush(List<Vertex> interpolatedPoints)
        {
            if (interpolatedPoints.Count == 0)
                return null;

            int xMax = (int)interpolatedPoints.Max(p => p.ProjectedPosition.X);
            int xMin = (int)interpolatedPoints.Min(p => p.ProjectedPosition.X);
            int yMax = (int)interpolatedPoints.Max(p => p.ProjectedPosition.Y);
            int yMin = (int)interpolatedPoints.Min(p => p.ProjectedPosition.Y);
            int width = xMax - xMin + 1;
            int height = yMax - yMin + 1;
            if (width <= 0 || height <= 0)
                return null;

            try
            {
                lightingBitmap.Lock();

                unsafe
                {
                    IntPtr pBackBuffer = lightingBitmap.BackBuffer;

                    Color color;
                    int x, y;
                    foreach (var vertex in interpolatedPoints)
                    {
                        if (!IsPointInBounds(vertex.ProjectedPosition))
                            continue;
                        x = (int)(vertex.ProjectedPosition.X);
                        y = (int)(vertex.ProjectedPosition.Y);
                        IntPtr currentBuffer = pBackBuffer + y * lightingBitmap.BackBufferStride + x * 4;
                        color = CalculatePointIlumination(vertex.Position, vertex.Normal);
                        int color_data = color.R << 16; // R
                        color_data |= color.G << 8;   // G
                        color_data |= color.B << 0;   // B
                        *((int*)currentBuffer) = color_data;
                    }
                }

                lightingBitmap.AddDirtyRect(new Int32Rect(0, 0, Width, Height));
            }
            finally
            {
                lightingBitmap.Unlock();
            }

            var brush = new ImageBrush(lightingBitmap);
            brush.Stretch = Stretch.None;
            brush.TileMode = TileMode.None;
            brush.Viewbox = new Rect(xMin, yMin, width, height);
            brush.ViewboxUnits = BrushMappingMode.Absolute;

            return brush;
        }

        private Color CalculatePointIlumination(Vector3 position, Vector3 normal)
        {
            var color = lightSource.AmbientColor;
            var vectorToLight = GetVectorToLight(position);
            var lightNormalDotProduct = Vector3.Dot(normal, vectorToLight);
            if (lightNormalDotProduct > 0)
            {
                var diffusionColor = lightSource.Intensity * (cylinder.DiffuseReflectivity * lightNormalDotProduct);
                color += diffusionColor;

                var vectorToLightReflected = 2 * lightNormalDotProduct * normal - vectorToLight;
                var vectorToCamera = GetVectorToCamera(position);
                var reflectionDotResult = Vector3.Dot(vectorToLightReflected, vectorToCamera);
                if (reflectionDotResult > 0)
                {
                    var reflectionColor = lightSource.Intensity * (cylinder.SpecularReflectivity * MathF.Pow(reflectionDotResult, cylinder.SpecularReflectionExponent));
                    color += reflectionColor;
                }
            }
            color.Clamp();
            return color;
        }

        private Vector3 GetNormalEndPoint(Vertex vertex)
        {
            var scale = 10;
            return new Vector3(vertex.Position.X + scale * vertex.Normal.X,
                                vertex.Position.Y + scale * vertex.Normal.Y,
                                vertex.Position.Z + scale * vertex.Normal.Z);
        }

        private Vector3 GetVectorToCamera(Vector3 position)
        {
            return Vector3.Normalize(CameraPosition - position);
        }

        private Vector3 GetVectorToLight(Vector3 position)
        {
            return Vector3.Normalize(lightSource.Position - position);
        }

        private void DrawLine(Vector3 p1, Vector3 p2)
        {
            var line = new Line();
            line.X1 = p1.X;
            line.Y1 = p1.Y;
            line.X2 = p2.X;
            line.Y2 = p2.Y;
            line.Stroke = Brushes.Red;
            line.StrokeThickness = 2;
            line.StrokeEndLineCap = PenLineCap.Triangle;
            Scene.Children.Add(line);
        }

        private void DrawTriangle(Vector3 p1, Vector3 p2, Vector3 p3, ImageBrush brush)
        {
            var poly = new Polygon();
            var points = new PointCollection();
            points.Add(new Point(p1.X, p1.Y));
            points.Add(new Point(p2.X, p2.Y));
            points.Add(new Point(p3.X, p3.Y));
            poly.Points = points;
            poly.StrokeLineJoin = PenLineJoin.Bevel;

            if (FillTriangles)
            {
                poly.Fill = brush;
                poly.Stroke = brush;
            }
            if (DrawLines)
                poly.Stroke = Brushes.Black;

            Scene.Children.Add(poly);
        }

        private bool IsPointInBounds(Vector3 point)
        {
            return point.X >= 0 && point.X < Width && point.Y >= 0 && point.Y < Height;
        }





        private void MoveCameraLeft(object sender, EventArgs e)
        {
            var vector = new Vector3(-MoveCameraDistance, 0, 0);
            var rotated = Rotator.Rotate(vector, CameraRotation);
            CameraPosition += rotated;
        }

        private void MoveCameraRight(object sender, EventArgs e)
        {
            var vector = new Vector3(MoveCameraDistance, 0, 0);
            var rotated = Rotator.Rotate(vector, CameraRotation);
            CameraPosition += rotated;
        }

        private void MoveCameraUp(object sender, EventArgs e)
        {
            var vector = new Vector3(0, MoveCameraDistance, 0);
            var rotated = Rotator.Rotate(vector, CameraRotation);
            CameraPosition += rotated;
        }

        private void MoveCameraDown(object sender, EventArgs e)
        {
            var vector = new Vector3(0, -MoveCameraDistance, 0);
            var rotated = Rotator.Rotate(vector, CameraRotation);
            CameraPosition += rotated;
        }

        private float scrollDistance = 3f;

        private void ZoomInCamera(object sender, EventArgs e)
        {
            var vector = new Vector3(0, 0, scrollDistance);
            var rotated = Rotator.Rotate(vector, CameraRotation);
            CameraPosition += rotated;
        }

        private void ZoomOutCamera(object sender, EventArgs e)
        {
            var vector = new Vector3(0, 0, -scrollDistance);
            var rotated = Rotator.Rotate(vector, CameraRotation);
            CameraPosition += rotated;
        }

        private float rotateDistance = 1.5f;

        private void RotateUpCamera(object sender, EventArgs e)
        {
            AngleX -= rotateDistance;
        }

        private void RotateDownCamera(object sender, EventArgs e)
        {
            AngleX += rotateDistance;
        }

        private void RotateLeftCamera(object sender, EventArgs e)
        {
            AngleY += rotateDistance;
        }

        private void RotateRightCamera(object sender, EventArgs e)
        {
            AngleY -= rotateDistance;
        }


        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null && propertyName != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
