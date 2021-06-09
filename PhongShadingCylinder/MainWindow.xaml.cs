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
        private Matrix4x4 transformMatrix;
        private LightSource lightSource;
        private Cylinder cylinder;
        private Mesh mesh = null;
        private FillingAlgorithm fillingAlgorithm = new FillingAlgorithm();
        private WriteableBitmap lightingBitmap;

        private float cylinderAngleX = 0;
        private float cylinderAngleY = 0;
        private float cylinderAngleZ = 0;
        private bool _drawNormals;
        private bool _fillTriangles = true;
        private bool _drawLines;
        private Vector3 _cameraPosition = new Vector3(0, 0, -150);
        private Vector3 _cameraRotation = new Vector3(0, 1, 0);

        private new float Width => (float)ActualWidth;
        private new float Height => (float)ActualHeight;
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
            InitializeDispatcher();
            previousMousePosition = Mouse.GetPosition(this);
            DataContext = this;
            lightSource = new LightSource()
            {
                Intensity = Colors.Salmon,
                Position = new Vector3(100, 40, -100),
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
            Redraw();
            //var poly1 = new Polygon();
            //var points1 = new PointCollection();
            //points1.Add(new Point(100, 200));
            //points1.Add(new Point(100, 300));
            //points1.Add(new Point(200, 300));
            //poly1.Points = points1;
            //poly1.StrokeLineJoin = PenLineJoin.Bevel;
            //poly1.Fill = CreateImageBrush(points1.Select(p => new Vector3((float)p.X, (float)p.Y, 0)), Colors.Aqua);
            //poly1.Stroke = Brushes.DarkGray;


            //var poly2 = new Polygon();
            //var points2 = new PointCollection();
            //points2.Add(new Point(150, 100));
            //points2.Add(new Point(250, 100));
            //points2.Add(new Point(250, 200));
            //poly2.Points = points2;
            //poly2.StrokeLineJoin = PenLineJoin.Bevel;

            //poly2.Stroke = Brushes.DarkGray;
            //poly2.Fill = imageBrush;

            //var poly3 = new Polygon();
            //var points3 = new PointCollection();
            //points3.Add(new Point(400, 200));
            //points3.Add(new Point(450, 100));
            //points3.Add(new Point(500, 200));
            //poly3.Points = points3;
            //poly3.StrokeLineJoin = PenLineJoin.Bevel;

            //poly3.Stroke = Brushes.DarkGray;
            //poly3.Fill = imageBrush;

            //var poly4 = new Polygon();
            //var points4 = new PointCollection();
            //points4.Add(new Point(400, 200));
            //points4.Add(new Point(450, 300));
            //points4.Add(new Point(500, 200));
            //poly4.Points = points4;
            //poly4.StrokeLineJoin = PenLineJoin.Bevel;

            //poly4.Stroke = Brushes.DarkGray;
            //poly4.Fill = imageBrush;

            //Scene.Children.Add(poly1);
            //Scene.Children.Add(poly2);
            //Scene.Children.Add(poly3);
            //Scene.Children.Add(poly4);
        }

        private ImageBrush CreateImageBrush(IEnumerable<Vector3> points, Color color)
        {
            int xMax = (int)points.Max(p => p.X);
            int xMin = (int)points.Min(p => p.X);
            int yMax = (int)points.Max(p => p.Y);
            int yMin = (int)points.Min(p => p.Y);
            int rectWidth = xMax - xMin;
            int rectHeight = yMax - yMin;
            int width = (int)Width;
            int height = (int)Height;
            if (width == 0 || height == 0)
                return null;

            try
            {
                // Reserve the back buffer for updates.
                lightingBitmap.Lock();

                unsafe
                {
                    // Get a pointer to the back buffer.
                    IntPtr pBackBuffer = lightingBitmap.BackBuffer;
                    
                    for (int x = xMin; x < xMax; x++)
                    {
                        for (int y = yMin; y < yMax; y++)
                        {
                            IntPtr currentBuffer = pBackBuffer + y * lightingBitmap.BackBufferStride + x * 4;
                            int color_data = color.R << 16; // R
                            color_data |= color.G << 8;   // G
                            color_data |= color.B << 0;   // B
                            *((int*)currentBuffer) = color_data;
                        }
                    }
                }

                // Specify the area of the bitmap that changed.
                lightingBitmap.AddDirtyRect(new Int32Rect(xMin, yMin, rectWidth, rectHeight));
            }
            finally
            {
                // Release the back buffer and make it available for display.
                lightingBitmap.Unlock();
            }

            var brush = new ImageBrush(lightingBitmap);
            brush.Stretch = Stretch.None;
            brush.TileMode = TileMode.None;
            brush.Viewbox = new Rect(xMin - 10, yMin, rectWidth, rectHeight);
            brush.ViewboxUnits = BrushMappingMode.Absolute;

            return brush;
        }

        private BitmapSource CreateBitmap(IEnumerable<Vector3> points, Color color)
        {
            int xMax = (int)points.Max(p => p.X) + 1;
            int xMin = (int)points.Min(p => p.X);
            int yMax = (int)points.Max(p => p.Y) + 1;
            int yMin = (int)points.Min(p => p.Y);
            int width = xMax - xMin;
            int height = yMax - yMin;
            if (width == 0 || height == 0)
                return null;
            var byts = new byte[width * height * 4];

            for (long i = 0; i < width * height * 4; i += 4)
            {
                byts[i] = color.B;
                byts[i + 1] = color.G;
                byts[i + 2] = color.R;
                byts[i + 3] = 255;
            }

            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, null, byts, 4 * width);
        }

        private BitmapSource CreateLightingBitmap(IEnumerable<Vector3> points, List<Vertex> interpolatedPoints)
        {
            int xMax = (int)points.Max(p => p.X) + 1;
            int xMin = (int)points.Min(p => p.X);
            int yMax = (int)points.Max(p => p.Y) + 1;
            int yMin = (int)points.Min(p => p.Y);
            int width = xMax - xMin;
            int height = yMax - yMin;
            if (width == 0 || height == 0)
                return null;
            var bytes = new byte[width * height * 4];
            int x, y;
            Color c;
            foreach (var vertex in interpolatedPoints)
            {
                x = (int)(vertex.ProjectedPosition.X - xMin);
                y = (int)(vertex.ProjectedPosition.Y - yMin);
                c = CalculateCylinderPointIllumination(vertex.Position, vertex.Normal);
                bytes[4 * (y * width + x)] = c.B;
                bytes[4 * (y * width + x) + 1] = c.G;
                bytes[4 * (y * width + x) + 2] = c.R;
                bytes[4 * (y * width + x) + 3] = 255;
            }

            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, null, bytes, 4 * width);
        }

        private ImageBrush CreateLightingBrush(List<Vertex> interpolatedPoints)
        {
            interpolatedPoints = interpolatedPoints.Where(v => IsPointInBounds(v.ProjectedPosition.X, v.ProjectedPosition.Y)).ToList();

            if (interpolatedPoints.Count == 0)
                return null;

            int xMax = (int)interpolatedPoints.Max(p => p.ProjectedPosition.X);
            int xMin = (int)interpolatedPoints.Min(p => p.ProjectedPosition.X);
            int yMax = (int)interpolatedPoints.Max(p => p.ProjectedPosition.Y);
            int yMin = (int)interpolatedPoints.Min(p => p.ProjectedPosition.Y);
            int width = xMax - xMin;
            int height = yMax - yMin;
            if (width == 0 || height == 0)
                return null;

            try
            {
                // Reserve the back buffer for updates.
                lightingBitmap.Lock();

                unsafe
                {
                    // Get a pointer to the back buffer.
                    IntPtr pBackBuffer = lightingBitmap.BackBuffer;

                    Color color;
                    int x, y;
                    foreach (var vertex in interpolatedPoints)
                    {
                        x = (int)(vertex.ProjectedPosition.X);
                        y = (int)(vertex.ProjectedPosition.Y);
                        if (IsPointInBounds(x, y))
                        {
                            color = CalculateCylinderPointIllumination(vertex.Position, vertex.Normal);
                            IntPtr currentBuffer = pBackBuffer + y * lightingBitmap.BackBufferStride + x * 4;
                            int color_data = color.R << 16; // R
                            color_data |= color.G << 8;   // G
                            color_data |= color.B << 0;   // B
                            *((int*)currentBuffer) = color_data;
                        }
                    }
                }

                // Specify the area of the bitmap that changed.
                lightingBitmap.AddDirtyRect(new Int32Rect(xMin, yMin, width, height));
            }
            finally
            {
                // Release the back buffer and make it available for display.
                lightingBitmap.Unlock();
            }

            var brush = new ImageBrush(lightingBitmap);
            brush.Stretch = Stretch.None;
            brush.TileMode = TileMode.None;
            brush.Viewbox = new Rect(xMin, yMin, width, height);
            brush.ViewboxUnits = BrushMappingMode.Absolute;

            return brush;
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
            cylinderAngleZ += 1;
            Redraw();
        }

        private void Scene_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int width = (int)MathF.Ceiling(Width);
            int height = (int)MathF.Ceiling(Height);
            lightingBitmap = new WriteableBitmap(
                width,
                height,
                96,
                96,
                PixelFormats.Bgr32,
                null);
            Redraw();
        }

        private void Scene_MouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(this);
            if (e.RightButton == MouseButtonState.Pressed)
            {
                var distance = Point.Subtract(position, previousMousePosition);
                var vector = new Vector3(-(float)(distance.Y * RotateDistanceMultiplier), (float)(distance.X * RotateDistanceMultiplier), 0);
                CameraRotation += vector;
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

        private void Scene_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var vector = new Vector3(0, 0, e.Delta * ScrollDistanceMultiplier);
            var rotated = Rotator.Rotate(vector, CameraRotation);
            CameraPosition += rotated;
        }



        private void Redraw()
        {
            Scene.Children.Clear();
            CalculateTransformMatrix();
            //DrawLightSource();
            DrawCylinder();
        }

        private void CalculateTransformMatrix()
        {
            transformMatrix = Matrix4x4.Identity;
            transformMatrix *= Rotator.RotationMatrix(new Vector3(cylinderAngleX, cylinderAngleY, cylinderAngleZ));
            //transformMatrix *= Translator.TranslationMatrix(new Vector3(0, 0, 0));
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
            //DrawTriangles(mesh.TrianglesSide);
            //DrawTriangles(mesh.TrianglesBottom);
            //DrawTriangles(mesh.TrianglesTop);
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

                    var interpolatedFillVertices = fillingAlgorithm.Fill(triangle, (int)Width, (int)Height);
                    var brush = CreateLightingBrush(interpolatedFillVertices);
                    if (brush == null)
                        continue;
                    //var positionAvg = (triangle.Vertices[0].Position + triangle.Vertices[1].Position + triangle.Vertices[2].Position) / 3;
                    //var normalAvg = (triangle.Vertices[0].Normal + triangle.Vertices[1].Normal + triangle.Vertices[2].Normal) / 3;
                    //var color = CalculateCylinderPointIllumination(positionAvg, normalAvg);
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
                && IsVertexVisible(triangle.Vertices[1]) 
                && IsVertexVisible(triangle.Vertices[2]);
        }

        private bool IsVertexVisible(Vertex vertex)
        {
            return Vector3.Dot(vertex.Normal, GetVectorToCamera(vertex.Position)) > 0;
        }

        private Vector3? Project(Vector3 vector)
        {
            var transformed = Vector4.Transform(vector, transformMatrix);
            var cameraTransformed = CameraTransformer.Transform(transformed, CameraPosition, CameraRotation);
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

        private Vector3 InverseProject(Vector3 vector)
        {
            var inverse2d = CoordinateTranslator.Translate2dTo3d(vector, Width, Height);
            var inverseProject = PerspectiveProjector.Inverse(inverse2d, Width, Height);
            var inverseCamera = CameraTransformer.Inverse(inverseProject, CameraPosition, CameraRotation);
            return new Vector3(inverseCamera.X,
                               inverseCamera.Y,
                               inverseCamera.Z);
        }

        private Color CalculateCylinderPointIllumination(Vector3 position, Vector3 normal)
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

        private void DrawTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            DrawTriangle(p1, p2, p3, Colors.Pink);
        }

        private void DrawTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Color color)
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
                poly.Stroke = new SolidColorBrush(color);
                poly.Fill = new SolidColorBrush(color);
            }
            if (DrawLines)
                poly.Stroke = Brushes.Black;
            Scene.Children.Add(poly);
        }

        private void DrawTriangle(Vector3 p1, Vector3 p2, Vector3 p3, ImageSource fillBitmap)
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
                var brush = new ImageBrush(fillBitmap);
                brush.Stretch = Stretch.None;
                poly.Fill = brush;
                poly.Stroke = brush;
            }
            if (DrawLines)
                poly.Stroke = Brushes.Black;

            Scene.Children.Add(poly);
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

        private Vector3 ClipPoint(Vector3 point)
        {
            point.X = Math.Clamp(point.X, 0, Width);
            point.Y = Math.Clamp(point.Y, 0, Height);
            return point;
        }

        private bool IsPointInBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        private bool IsPointInBounds(float x, float y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
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




        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null && propertyName != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
