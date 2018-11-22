using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Mandelbrot {
    public class MandelBrotImage : Control {
        public MandelBrotImage() {
            this.DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);

            e.Graphics.DrawImage(MandelBrot.Generate(this.ClientSize, new PointF(.41f, .21f), .05f, defaultColorMapper ), new Point(0, 0));

            Color defaultColorMapper(float x) {
                return RGB((float)Math.Pow(x, .1) * .25f - 0.1f, (float)Math.Pow(x, .7), (float)Math.Pow(x, .5) * .62f);
            }
        }

        public static Color RGB(float r, float g, float b) {
            return Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
        }
    }

    public static class MandelBrot {
        public static int MaxMandelNumber { get; set; } = 500;
        public static Color backgroundColor = Color.FromArgb(0, 0, 0);

        public static Bitmap Generate(Size canvasSize, PointF center, float scale, FloatToColorMapper colorFromIterationCount) {
            //canvas will be the return value
            Bitmap canvas = new Bitmap(canvasSize.Width, canvasSize.Height);

            for (int pixelX = 0; pixelX < canvasSize.Width; pixelX++) {
                for (int pixelY = 0; pixelY < canvasSize.Height; pixelY++) {
                    double mappedX = center.X + ((double)canvasSize.Width / (double)canvasSize.Height / -2 + (double)pixelX / (double)canvasSize.Height) * scale;
                    double mappedY = center.Y + (-0.5 + (double)pixelY / (double)canvasSize.Height) * scale;

                    int? mandelNumber = MandelNumber(mappedX, mappedY);

                    canvas.SetPixel(pixelX, pixelY, mandelNumber != null ?
                        colorFromIterationCount((float)mandelNumber / (float)MaxMandelNumber)
                        : backgroundColor);
                }
            }

            return canvas;
        }

        public static int? MandelNumber(double x, double y) {
            double a = 0.0, b = 0.0;
            int result = 0;

            const double MAX_MANDEL_DISTANCE = 2.0;

            while (Pythagoras(a, b) < MAX_MANDEL_DISTANCE) {
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
