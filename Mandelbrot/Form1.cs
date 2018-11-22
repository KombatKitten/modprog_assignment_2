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
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();

            this.ClientSize = new Size(1800, 1000);

            this.WindowState = FormWindowState.Maximized;
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);

            e.Graphics.DrawImage(MandelBrot.Generate(this.ClientSize, new PointF(0.0f, 0.0f), 2f, new DefaultColorMapper()), new Point(0, 0));
        }
    }

    public static class MandelBrot {
        public static int MaxMandelNumber { get; set; } = 255;
        public static Color backgroundColor = Color.FromArgb(0, 0, 0);

        public static Bitmap Generate(Size canvasSize, PointF center, float scale, IIntToColorMapper colorFromIterationCount) {
            //canvas will be returned
            Bitmap canvas = new Bitmap(canvasSize.Width, canvasSize.Height);

            for(int x = 0; x < canvasSize.Width; x++) {
                for(int y = 0; y < canvasSize.Height; y++) {
                    //TODO: map bitmap x, y to coords in mandelbrot using center and scale

                    double mappedX = center.X + ((double)canvasSize.Width / (double) canvasSize.Height / -2 + (double)x / (double)canvasSize.Height) * scale;
                    double mappedY = center.Y + (-0.5 + (double)y / (double)canvasSize.Height) * scale;

                    int? mandelNumber = MandelNumber(mappedX, mappedY);

                    canvas.SetPixel(x, y, mandelNumber != null ?
                        colorFromIterationCount.Map((int) mandelNumber)
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

    public interface IIntToColorMapper {
        Color Map(int input);
    }

    public class DefaultColorMapper : IIntToColorMapper {
        public Color Map(int input) {
            input = Math.Min(input, 255);

            return Color.FromArgb(input, input, input);
        }
    }
}
