using PhongShadingCylinder.Transformations;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace PhongShadingCylinder
{
    public class Camera : INotifyPropertyChanged
    {
        private Vector3 position;
        private Vector3 rotation;

        public float PositionX
        {
            get => position.X;
            set
            {
                if (value != position.X)
                {
                    position.X = value;
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
                    rotation.X = value;
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
                    rotation.Y = value;
                    RaisePropertyChanged();
                }
            }
        }

        public float AngleZ
        {
            get => rotation.Z;
            set
            {
                if (value != rotation.Z)
                {
                    rotation.Z = value;
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
                    RaisePropertyChanged();
                    RaisePropertyChanged("AngleX");
                    RaisePropertyChanged("AngleY");
                    RaisePropertyChanged("AngleZ");
                }
            }
        }

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

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null && propertyName != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
