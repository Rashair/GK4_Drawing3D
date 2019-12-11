using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace GK4_Lab1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // conf
        private int sideSize;

        private Size initialCanvasSize;
        private const int fovMult = 90;
        private double fov = 1 * fovMult;
        private const double rotationDegree = 4;

        // variables
        private WriteableBitmap bmp;

        private readonly DispatcherTimer timer;
        private double alfa = 30;

        public MainWindow()
        {
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 80);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            alfa += rotationDegree * Math.PI / 180.0;
            bmp.Clear();
            DrawCube(bmp);
        }

        private void MainCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            int w = (int)mainCanvas.ActualWidth;
            int h = (int)mainCanvas.ActualHeight;
            initialCanvasSize = mainCanvas.RenderSize;
            sideSize = (int)(Math.Min(w, h) / 2.0);
            SetBitmapImage(w, h);
            timer.Start();
        }

        private void SetBitmapImage(int width, int height)
        {
            mainCanvas.Children.Clear();
            this.bmp = BitmapFactory.New(width, height);
            DrawCube(bmp);

            var host = new Image();
            host.Source = bmp;
            mainCanvas.Children.Add(host);
        }

        private void DrawCube(WriteableBitmap bmp)
        {
            const int side = 1;
            (double x, double y, double z, double w) = (0, 0, 0, 1);
            DenseVector p1 = new DenseVector(new double[] { x, y, z, w });
            var p2 = new DenseVector(new double[] { x + side, y, z, w });
            var p3 = new DenseVector(new double[] { x, y, z + side, w });
            var p4 = new DenseVector(new double[] { x + side, y, z + side, w });
            var p5 = new DenseVector(new double[] { x, y + side, z, w });
            var p6 = new DenseVector(new double[] { x + side, y + side, z, w });
            var p7 = new DenseVector(new double[] { x, y + side, z + side, w });
            var p8 = new DenseVector(new double[] { x + side, y + side, z + side, w });

            var n = 1.0;
            var f = 40.0;
            var a = 1.0;

            var e = 1.0 / Math.Tan((fov * Math.PI) / 180.0 / 2);
            var projMatrix = DenseMatrix.OfArray(new double[,] {
                { e,  0  , 0  , 0 },
                { 0,  e/a  , 0  , 0 },
                { 0,  0  , -(f+n)/(f-n), -(2*f*n)/(f-n) },
                { 0,  0  , -1  ,  0 }
            });

            var viewMatrix = DenseMatrix.OfArray(new double[,] {
                { 0,  1  , 0  , -0.5 },
                { 0,  0  , 1  , -0.5 },
                { 1,  0  , 0  , -3 },
                { 0,  0  , 0  ,  1 }
            });

            var modelMatrix = DenseMatrix.OfArray(new double[,] {
                { Math.Cos(alfa),  -Math.Sin(alfa)  , 0  , 0.5 },
                { Math.Sin(alfa),  Math.Cos(alfa) , 0  , 0.4},
                { 0,  0  , 1  , 0.3 },
                { 0,  0  , 0  ,  1 }
            });

            var mult = projMatrix * viewMatrix * modelMatrix;
            p1 = GetPointOnBitmap(mult, p1);
            p2 = GetPointOnBitmap(mult, p2);
            p3 = GetPointOnBitmap(mult, p3);
            p4 = GetPointOnBitmap(mult, p4);
            p5 = GetPointOnBitmap(mult, p5);
            p6 = GetPointOnBitmap(mult, p6);
            p7 = GetPointOnBitmap(mult, p7);
            p8 = GetPointOnBitmap(mult, p8);

            using var context = bmp.GetBitmapContext();
            // p2 p6 p1 p5 - upper
            // p4 p8 p3 p7 - lower

            // lower
            DrawLine(bmp, p4, p8);
            DrawLine(bmp, p8, p7);
            DrawLine(bmp, p7, p3);
            DrawLine(bmp, p3, p4);

            // middle
            DrawLine(bmp, p4, p2);
            DrawLine(bmp, p8, p6);
            DrawLine(bmp, p7, p5);
            DrawLine(bmp, p3, p1);

            // upper
            DrawLine(bmp, p2, p6);
            DrawLine(bmp, p6, p5);
            DrawLine(bmp, p5, p1);
            DrawLine(bmp, p1, p2);

            // triangles
            // sides
            DrawLine(bmp, p4, p6);
            DrawLine(bmp, p3, p5);
            DrawLine(bmp, p8, p5);
            DrawLine(bmp, p4, p1);

            // bases
            DrawLine(bmp, p4, p7);
            DrawLine(bmp, p2, p5);
        }

        private DenseVector GetPointOnBitmap(DenseMatrix mult, DenseVector vs)
        {
            vs = mult * vs;
            vs /= vs.Last();

            const double shift = 1;
            return DenseVector.OfVector((vs + shift) * sideSize);
        }

        private void DrawLine(WriteableBitmap bmp, DenseVector p1, DenseVector p2)
        {
            (int x1, int y1) = ((int)p1.Values[0], (int)p1.Values[1]);
            (int x2, int y2) = ((int)p2.Values[0], (int)p2.Values[1]);

            bmp.DrawLine(x1, y1, x2, y2, Colors.Black);
        }

        private void MainCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (bmp == null)
            {
                return;
            }

            timer.Stop();
            var ratio = (initialCanvasSize.Width) / (e.NewSize.Width);
            fov = Math.Min(130, ratio * fovMult);
            SetBitmapImage((int)e.NewSize.Width, (int)e.NewSize.Height);
            timer.Start();
        }
    }
}