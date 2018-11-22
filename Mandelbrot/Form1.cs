using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mandelbrot {
    //Takes a floating point number from 0 to 1 and outputs a color
    public delegate Color FloatToColorMapper(float f);

    public partial class Form1 : Form {
        const int MANDELBROT_OFFSET_Y = 200;

        public Form1() {
            InitializeComponent();
            
            this.WindowState = FormWindowState.Maximized;
            
            this.Resize += OnFormResize;

            this.Controls.Add(this.mandelBrotImage);
        }

        private MandelBrotImage mandelBrotImage = new MandelBrotImage();

        private void OnFormResize(object sender, EventArgs e) {
            this.mandelBrotImage.Size = new Size(this.ClientSize.Width, this.ClientSize.Height - MANDELBROT_OFFSET_Y);
            this.mandelBrotImage.Location = new Point(0, MANDELBROT_OFFSET_Y);
        }
    }

    public class MandelBrotImage : Control{
        public MandelBrotImage() {
            this.DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);

            Console.WriteLine("Start drawing");
            e.Graphics.DrawImage(MandelBrot.Generate(this.ClientSize, new PointF(0.0f, 0.0f), 2f, defaultColorMapper), new Point(0, 0));

            Color defaultColorMapper(float x) {
                return RGB(x * .3f, x * x, x * .3f);
            }

            Console.WriteLine("Done drawing");
        }

        public static Color RGB(float r, float g, float b) {
            return Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
        }
    }

    public static class MandelBrot {
        public static int MaxMandelNumber { get; set; } = 100;
        public static Color backgroundColor = Color.FromArgb(0, 0, 0);

        public static Bitmap Generate(Size canvasSize, PointF center, float scale, FloatToColorMapper colorFromIterationCount) {
            //canvas will be the return value
            Bitmap canvas = new Bitmap(canvasSize.Width, canvasSize.Height);

            for(int pixelX = 0; pixelX < canvasSize.Width; pixelX++) {
                for(int pixelY = 0; pixelY < canvasSize.Height; pixelY++) {
                    double mappedX = center.X + ((double)canvasSize.Width / (double) canvasSize.Height / -2 + (double)pixelX / (double)canvasSize.Height) * scale;
                    double mappedY = center.Y + (-0.5 + (double)pixelY / (double)canvasSize.Height) * scale;

                    int? mandelNumber = MandelNumber(mappedX, mappedY);

                    canvas.SetPixel(pixelX, pixelY, mandelNumber != null ?
                        colorFromIterationCount((float)mandelNumber / (float) MaxMandelNumber)
                        : backgroundColor);
                }
            }

            return canvas;
        }

        public static int? MandelNumber(double x, double y) {
            double a = 0.0, b = 0.0;
            int result = 0;

            const double MAX_MANDEL_DISTANCE = 2.0;

            while(Pythagoras(a, b) < MAX_MANDEL_DISTANCE) {
                (a, b) = MandelTransform(a, b, x, y);

                if (++result > MaxMandelNumber) {
                    return null;
                }
            }

            return result;
        }

        private static (double, double) MandelTransform(double a, double b, double x, double y) {
            return (a * a - b * b + x,
                    2.0 * a * b + y);
        }

        public static double Pythagoras(double a, double b) {
            return Math.Sqrt(a * a + b * b);
        }
    }
}
