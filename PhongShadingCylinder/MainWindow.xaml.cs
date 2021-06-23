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
        private readonly Mesh mesh = null;
        private readonly FillingAlgorithm fillingAlgorithm = new FillingAlgorithm();
        private WriteableBitmap lightingBitmap;
        private Matrix4x4 projectionMatrix;

        private new int Width => (int)(base.Width - 3 * Options.ActualWidth / 4);
        private new int Height => (int)base.Height;

        private float lightAngle = 0;
        private float LightDistance => 100;
        private float ScrollDistance => 3f;
        private float rotateDistance => 1.5f;
        private float ScrollDistanceMultiplier => 0.2f;
        private float MoveCameraDistance => 2f;
        private float RotateDistanceMultiplier => 0.1f;

        public Camera Camera { get; set; }
        public LightSource LightSource { get; set; }
        public Cylinder Cylinder { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            InitializeKeyEventHandlers();
            DataContext = this;
            var intenstity = Colors.White;
            LightSource = new LightSource()
            {
                Intensity = intenstity,
                Position = new Vector3(0, LightDistance, LightDistance),
                AmbientColor = intenstity * 0.05f
            };
            Camera = new Camera()
            {
                Position = new Vector3(0, 0, -150),
                Rotation = new Vector3(0, 0, 0)
            };
            Cylinder = new Cylinder()
            {
                Height = 70,
                Radius = 40,
                Position = new Vector3(0, -70 / 2, 0),
                DivisionPointsCount = 34,
                DiffuseReflectivity = new float[] { 0.3f, 0.8f, 0.7f },
                SpecularReflectivity = new float[] { 0.9f, 0.9f, 0.9f },
                SpecularReflectionExponent = 25
            };
            mesh = meshCreator.CreateCylinderMesh(Cylinder.Radius, Cylinder.Height, Cylinder.Position, Cylinder.DivisionPointsCount);
            Loaded += new RoutedEventHandler(WindowInitialized);
        }


        private void WindowInitialized(object sender, RoutedEventArgs e)
        {
            RecalculateProjectionMatrix();
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
                RecalculateProjectionMatrix();
            }
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
            LightSource.Position.X = LightDistance * MathF.Sin(lightAngle);
            LightSource.Position.Z = LightDistance * MathF.Cos(lightAngle);
            LightSource.Position.Y = LightDistance * MathF.Cos(lightAngle / 2);
            lightAngle += 0.1f;
        }


        private void ClearLightingBitmap()
        {
            try
            {
                lightingBitmap.Lock();

                unsafe
                {
                    IntPtr pBackBuffer = lightingBitmap.BackBuffer;
                    for (long i = 0; i < lightingBitmap.BackBufferStride * lightingBitmap.PixelHeight; i++)
                    {
                        *((int*)pBackBuffer) = 0;
                        pBackBuffer += 1;
                    }
                }

                lightingBitmap.AddDirtyRect(new Int32Rect(0, 0, Width, Height));
            }
            finally
            {
                lightingBitmap.Unlock();
            }
        }

        private void DrawLightSource()
        {
            var point = Project(LightSource.Position);
            if (point.HasValue)
            {
                var ellipse = new Ellipse();
                ellipse.Width = 16;
                ellipse.Height = 16;
                ellipse.Fill = new SolidColorBrush(LightSource.Intensity);
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
                    if (IsProjectionVisible(triangle))
                        interpolatedFillVertices.AddRange(fillingAlgorithm.CalculateInteriorVertices(triangle, Width, Height));
                }
            }
            DrawLightingBitmap(interpolatedFillVertices);
        }

        private Vector3? Project(Vector3 vector)
        {
            var input = new Vector4(vector, 1);
            var cameraTransformed = Vector4.Transform(input, Camera.Matrix);
            if (cameraTransformed.Z <= 0)
                return null;
            var projected = Vector4.Transform(cameraTransformed, projectionMatrix);
            var result2d = CoordinateTranslator.Translate3dTo2dWithNormalization(projected, Width, Height);
            return result2d;
        }

        private void DrawLightingBitmap(List<Vertex> interpolatedPoints)
        {
            interpolatedPoints = interpolatedPoints.Where(v => IsPointInBounds(v.ProjectedPosition)).ToList();

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
                        x = (int)vertex.ProjectedPosition.X;
                        y = (int)vertex.ProjectedPosition.Y;
                        IntPtr currentBuffer = pBackBuffer + y * lightingBitmap.BackBufferStride + x * 4;
                        color = CalculatePointIllumination(vertex.Position, vertex.Normal);

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

        private Color CalculatePointIllumination(Vector3 position, Vector3 normal)
        {
            var color = LightSource.AmbientColor;
            var vectorToLight = LightSource.VectorTo(position);
            var lightNormalDotProduct = Vector3.Dot(normal, vectorToLight);
            if (lightNormalDotProduct > 0)
            {
                color += CreateColor(LightSource.Intensity, Cylinder.DiffuseReflectivity, lightNormalDotProduct);

                var vectorReflectedLight = 2 * lightNormalDotProduct * normal - vectorToLight;
                var vectorToCamera = Camera.VectorTo(position);
                var reflectionDotResult = Vector3.Dot(vectorReflectedLight, vectorToCamera);
                if (reflectionDotResult > 0)
                    color += CreateColor(LightSource.Intensity, Cylinder.SpecularReflectivity, MathF.Pow(reflectionDotResult, Cylinder.SpecularReflectionExponent));
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

        private bool IsTriangleVisible(Triangle triangle)
        {
            return IsVertexVisible(triangle.Vertices[0])
                || IsVertexVisible(triangle.Vertices[1])
                || IsVertexVisible(triangle.Vertices[2]);
        }

        private bool IsVertexVisible(Vertex vertex)
        {
            var toCamera = Camera.VectorTo(vertex.Position);
            var dot = Vector3.Dot(toCamera, vertex.Normal);
            return dot > 0;
        }

        private bool IsProjectionVisible(Triangle triangle)
        {
            return triangle.Vertices.Any(v => IsPointInBounds(v.ProjectedPosition));
        }

        private bool IsPointInBounds(Vector3 point)
        {
            return point.X >= 0 && point.X < Width && point.Y >= 0 && point.Y < Height;
        }

        private void RecalculateProjectionMatrix()
        {
            projectionMatrix = PerspectiveProjector.ProjectionMatrix(Width, Height);
        }



        private void MoveCameraLeft(object sender, EventArgs e)
        {
            Camera.MoveLeft(MoveCameraDistance);
        }

        private void MoveCameraRight(object sender, EventArgs e)
        {
            Camera.MoveRight(MoveCameraDistance);
        }

        private void MoveCameraUp(object sender, EventArgs e)
        {
            Camera.MoveUp(MoveCameraDistance);
        }

        private void MoveCameraDown(object sender, EventArgs e)
        {
            Camera.MoveDown(MoveCameraDistance);
        }

        private void ZoomInCamera(object sender, EventArgs e)
        {
            Camera.ZoomIn(ScrollDistance);
        }

        private void ZoomOutCamera(object sender, EventArgs e)
        {
            Camera.ZoomOut(ScrollDistance);
        }

        private void RotateUpCamera(object sender, EventArgs e)
        {
            Camera.RotateUp(rotateDistance);
        }

        private void RotateDownCamera(object sender, EventArgs e)
        {
            Camera.RotateDown(rotateDistance);
        }

        private void RotateLeftCamera(object sender, EventArgs e)
        {
            Camera.RotateLeft(rotateDistance);
        }

        private void RotateRightCamera(object sender, EventArgs e)
        {
            Camera.RotateRight(rotateDistance);
        }


        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null && propertyName != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
