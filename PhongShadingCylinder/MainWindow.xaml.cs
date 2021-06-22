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
        private readonly MeshCreator meshCreator = new MeshCreator();
        private readonly Dictionary<Key, EventHandler> keyEventHandlers = new();
        private readonly Dictionary<Key, bool> keyEventsHandled = new();
        private readonly DispatcherTimer dispatcher = new DispatcherTimer();
        private readonly LightSource lightSource;
        private readonly Cylinder cylinder;
        private readonly Mesh mesh = null;
        private readonly FillingAlgorithm fillingAlgorithm = new FillingAlgorithm();
        private WriteableBitmap lightingBitmap;



        private Vector3 _cameraPosition = new Vector3(0, 80, -150);
        private Vector3 _cameraRotation = new Vector3(0, 0, 0);

        private new int Width => (int)(base.Width - Options.ActualWidth);
        private new int Height => (int)base.Height;

        private float lightAngle = 0;
        private float lightDistance = 100;
        private float scrollDistance = 3f;
        private float rotateDistance = 1.5f;
        private float ScrollDistanceMultiplier => 0.2f;
        private float MoveCameraDistance => 2f;
        private float RotateDistanceMultiplier => 0.1f;

        public float PositionX
        {
            get => _cameraPosition.X;
            set
            {
                if (value != _cameraPosition.X)
                {
                    _cameraPosition.X = value;
                    RaisePropertyChanged();
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
                }
            }
        }


        public MainWindow()
        {
            InitializeComponent();
            InitializeKeyEventHandlers();
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
                DiffuseReflectivity = new float[] { 0.7f, 0.7f, 0.7f },
                SpecularReflectivity = new float[] { 0.7f, 0.7f, 0.7f },
                SpecularReflectionExponent = 2
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
            ImgTest.Source = lightingBitmap;
        }

        private void MoveLightSource(object sender, EventArgs e)
        {
            lightSource.Position.X = lightDistance * MathF.Sin(lightAngle);
            lightSource.Position.Z = lightDistance * MathF.Cos(lightAngle);
            lightAngle += 0.1f;
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
            var interpolatedFillVertices = new List<Vertex>();
            foreach (var triangle in mesh.Triangles)
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

                    interpolatedFillVertices.AddRange(fillingAlgorithm.Fill(triangle, Width, Height));
                }
            }
            DrawLighting(interpolatedFillVertices);
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
            if (cameraTransformed.Z <= 0)
                return null;
            var projected = PerspectiveProjector.Project(cameraTransformed, Width, Height);
            var result2d = CoordinateTranslator.Translate3dTo2d(projected, Width, Height);
            return result2d;
        }

        private void DrawLighting(List<Vertex> interpolatedPoints)
        {
            interpolatedPoints = interpolatedPoints.Where(v => IsPointInBounds(v.ProjectedPosition)).ToList();

            if (interpolatedPoints.Count == 0)
                return;

            int xMax = (int)interpolatedPoints.Max(p => p.ProjectedPosition.X);
            int xMin = (int)interpolatedPoints.Min(p => p.ProjectedPosition.X);
            int yMax = (int)interpolatedPoints.Max(p => p.ProjectedPosition.Y);
            int yMin = (int)interpolatedPoints.Min(p => p.ProjectedPosition.Y);
            int width = xMax - xMin + 1;
            int height = yMax - yMin + 1;
            if (width <= 0 || height <= 0)
                return;

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
        }

        private Color CalculatePointIlumination(Vector3 position, Vector3 normal)
        {
            var color = lightSource.AmbientColor;
            var vectorToLight = GetVectorToLight(position);
            var lightNormalDotProduct = Vector3.Dot(normal, vectorToLight);
            if (lightNormalDotProduct > 0)
            {
                //var diffusionColor = lightSource.Intensity * (cylinder.DiffuseReflectivity * lightNormalDotProduct);
                color += CreateColor(lightSource.Intensity, cylinder.DiffuseReflectivity, lightNormalDotProduct);

                var vectorReflectedLight = 2 * lightNormalDotProduct * normal - vectorToLight;
                var vectorToCamera = GetVectorToCamera(position);
                var reflectionDotResult = Vector3.Dot(vectorReflectedLight, vectorToCamera);
                if (reflectionDotResult > 0)
                {
                    //var reflectionColor = lightSource.Intensity * (cylinder.SpecularReflectivity * MathF.Pow(reflectionDotResult, cylinder.SpecularReflectionExponent));
                    color += CreateColor(lightSource.Intensity, cylinder.SpecularReflectivity, MathF.Pow(reflectionDotResult, cylinder.SpecularReflectionExponent));
                }
            }
            color.Clamp();
            return color;
        }

        private Color CreateColor(Color baseColor, float[] chanellMultipliers, float multiplier)
        {
            return new Color()
            {
                R = (byte)(baseColor.R * chanellMultipliers[0] * multiplier),
                G = (byte)(baseColor.G * chanellMultipliers[1] * multiplier),
                B = (byte)(baseColor.B * chanellMultipliers[2] * multiplier),
            };
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
