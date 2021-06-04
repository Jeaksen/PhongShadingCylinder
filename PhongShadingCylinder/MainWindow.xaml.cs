using System;
using System.Numerics;
using System.Windows;

namespace PhongShadingCylinder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var meshCreator = new MeshCreator();
            var mesh = meshCreator.CreateCylinderMesh(10, 10, new Vector3(0, 0, 0), 5);
        }
    }
}
