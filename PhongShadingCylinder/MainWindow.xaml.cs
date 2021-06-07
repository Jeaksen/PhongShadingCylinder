﻿using PhongShadingCylinder.Transformations;
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

        private float cylinderAngleX = 0;
        private float cylinderAngleY = 0;
        private float cylinderAngleZ = 0;
        private bool _drawNormals;
        private bool _fillTriangles = true;
        private bool _drawLines;
        private Vector3 _cameraPosition = new Vector3(0, 0, -150);
        private Vector3 _cameraRotation = new Vector3(0, 0, 0);

        private new float Width => (float)Scene.ActualWidth;
        private new float Height => (float)Scene.ActualHeight;
        private float ScrollDistanceMultiplier => 0.2f;
        private float MoveCameraDistance => 1f;
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
                Intensity = Colors.White,
                Position = new Vector3(100, 40, -100)
            };

            cylinder = new Cylinder()
            {
                Height = 70,
                Radius = 40,
                Position = new Vector3(0, -70 / 2, 0),
                DivisionPointsCount = 80,
                DiffuseReflectivity = 0.6f,
                SpecularReflectivity = 0.8f,
                SpecularReflectionExponent = 20
            };
            mesh = meshCreator.CreateCylinderMesh(cylinder.Radius, cylinder.Height, cylinder.Position, cylinder.DivisionPointsCount);
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
            cylinderAngleZ += 1;
            Redraw();
        }

        private void Scene_SizeChanged(object sender, SizeChangedEventArgs e)
        {
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

        private void DrawTriangles(List<Triangle> trainagles)
        {
            foreach (var triangle in trainagles)
            {
                if (!IsTriangleVisible(triangle))
                    continue;

                var p1 = Project(triangle.Vertices[0].Position);
                var p2 = Project(triangle.Vertices[1].Position);
                var p3 = Project(triangle.Vertices[2].Position);
                if (p1.HasValue && p2.HasValue && p3.HasValue)
                {
                    var positionAvg = (triangle.Vertices[0].Position + triangle.Vertices[1].Position + triangle.Vertices[2].Position) / 3;
                    var normalAvg = (triangle.Vertices[0].Normal + triangle.Vertices[1].Normal + triangle.Vertices[2].Normal) / 3;
                    var color = CalculateCylinderPointIllumination(positionAvg, normalAvg);
                    DrawTriangle(p1.Value, p2.Value, p3.Value, color);

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

        private Vector2? Project(Vector3 vector)
        {
            var transformed = Vector4.Transform(vector, transformMatrix);
            var cameraTransformed = CameraTransformer.Transform(transformed, CameraPosition, CameraRotation);
            //if (cameraTransformed.Z == 0)
            //    return new Vector2(cameraTransformed.X, cameraTransformed.Y);
            if (cameraTransformed.Z < 0)
                return null;
            var projected = PerspectiveProjector.Project(cameraTransformed, Width, Height);
            //var result3d = PerspectiveProjector.Normalize(transformed);
            var result2d = CoordinateTranslator.Translate3dTo2d(projected, Width, Height);
            return result2d;
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

        private void DrawLine(Vector2 p1, Vector2 p2)
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

        private void DrawTriangle(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            DrawTriangle(p1, p2, p3, Colors.Pink);
        }

        private void DrawTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Color color)
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
