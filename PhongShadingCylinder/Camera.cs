using PhongShadingCylinder.Transformations;
using System;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace PhongShadingCylinder
{
    public class Camera : INotifyPropertyChanged
    {
        private Vector3 position;
        private Vector3 rotation;
        private Matrix4x4 matrix;

        public int MaxXAngle { get; set; } = 60;
        public int MaxYAngle { get; set; } = 360;

        public float PositionX
        {
            get => position.X;
            set
            {
                if (value != position.X)
                {
                    position.X = value;
                    RecalculateMatrix(); 
                    RaisePropertyChanged();
                }
            }
        }

        public float PositionY
        {
            get => position.Y;
            set
            {
                if (value != position.Y)
                {
                    position.Y = value;
                    RecalculateMatrix(); 
                    RaisePropertyChanged();
                }
            }
        }

        public float PositionZ
        {
            get => position.Z;
            set
            {
                if (value != position.Z)
                {
                    position.Z = value;
                    RecalculateMatrix(); 
                    RaisePropertyChanged();
                }
            }
        }

        public float AngleX
        {
            get => rotation.X;
            set
            {
                if (value != rotation.X)
                {
                    rotation.X = ClampAngle(value, MaxXAngle);
                    RecalculateMatrix(); 
                    RaisePropertyChanged();
                }
            }
        }

        public float AngleY
        {
            get => rotation.Y;
            set
            {
                if (value != rotation.Y)
                {
                    rotation.Y = ClampAngle(value, MaxYAngle);
                    RecalculateMatrix(); 
                    RaisePropertyChanged();
                }
            }
        }

        public Vector3 Position
        {
            get => position;
            set
            {
                if (value != position)
                {
                    position = value;
                    RecalculateMatrix();
                    RaisePropertyChanged();
                    RaisePropertyChanged("PositionX");
                    RaisePropertyChanged("PositionY");
                    RaisePropertyChanged("PositionZ");
                }
            }
        }

        public Vector3 Rotation
        {
            get => rotation;
            set
            {
                if (value != rotation)
                {
                    
                    rotation = value;
                    RecalculateMatrix(); 
                    RaisePropertyChanged();
                    RaisePropertyChanged("AngleX");
                    RaisePropertyChanged("AngleY");
                    RaisePropertyChanged("AngleZ");
                }
            }
        }

        public Matrix4x4 Matrix => matrix;


        public Vector3 VectorTo(Vector3 from) => Vector3.Normalize(position - from);

        public void MoveLeft(float distance)
        {
            var vector = new Vector3(-distance, 0, 0);
            var rotated = Rotator.Rotate(vector, Rotation);
            Position += rotated;
        }

        public void MoveRight(float distance)
        {
            var vector = new Vector3(distance, 0, 0);
            var rotated = Rotator.Rotate(vector, Rotation);
            Position += rotated;
        }

        public void MoveUp(float distance)
        {
            var vector = new Vector3(0, distance, 0);
            var rotated = Rotator.Rotate(vector, Rotation);
            Position += rotated;
        }

        public void MoveDown(float distance)
        {
            var vector = new Vector3(0, -distance, 0);
            var rotated = Rotator.Rotate(vector, Rotation);
            Position += rotated;
        }


        public void ZoomIn(float distance)
        {
            var vector = new Vector3(0, 0, distance);
            var rotated = Rotator.Rotate(vector, Rotation);
            Position += rotated;
        }

        public void ZoomOut(float distance)
        {
            var vector = new Vector3(0, 0, -distance);
            var rotated = Rotator.Rotate(vector, Rotation);
            Position += rotated;
        }


        public void RotateUp(float angle)
        {
            AngleX -= angle;
        }

        public void RotateDown(float angle)
        {
            AngleX += angle;
        }

        public void RotateLeft(float angle)
        {
            AngleY += angle;
        }

        public void RotateRight(float angle)
        {
            AngleY -= angle;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private float ClampAngle(float value, float maxAngle)
        {
            if (value >= 360)
                value = value - 360;
            else if (value < 0)
                value = 360 + value;
            
            if (value < maxAngle || value > 360 - maxAngle)
                return value;
            float distanceLower = value - maxAngle;
            float distanceUpper = 360 - maxAngle - value;
            if (distanceLower > distanceUpper)
                return 360 - maxAngle;
            return maxAngle;

        }

        private void RecalculateMatrix()
        {
            matrix = CameraTransformer.TransformMatrix(Position, Rotation);
        }

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null && propertyName != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
