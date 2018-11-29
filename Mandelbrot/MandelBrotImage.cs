using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace Mandelbrot {
    public delegate Color MandelColorMapper(int mandelNumber);
    public delegate PointD PixelToCoordinate(Point pixelCoords);

    public class MandelbrotImage : Control {
        public PointD Center { get; set; } = new PointD(0.0, 0.0);
        public double ZoomScale { get; set; } = 2.0;
        public int ThreadCount { get; set; } = 8; //experimentation found that 8 is the optimal value for my own computer

        public MandelbrotImage() {
            this.DoubleBuffered = true;
            this.MouseClick += OnMandelMouseClick;
        }

        /// <summary>
        /// Changes the center of the image to the mouse position
        /// </summary>
        private void OnMandelMouseClick(object sender, MouseEventArgs e) {
            var newCenter = DefaultPixelToMandelCoordsMapper(this.Center, this.ClientSize, this.ZoomScale)(e.Location);
            this.Center = new PointD(newCenter.X, -newCenter.Y);
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            
            int offsetY = this.ClientSize.Height / ThreadCount;
            int restOffsetY = this.ClientSize.Height % ThreadCount;
            int width = this.ClientSize.Width;
            int height = this.ClientSize.Height;

            Console.WriteLine(restOffsetY);

            Thread[] threads = new Thread[ThreadCount];
            Bitmap[] images = new Bitmap[ThreadCount];

            Stopwatch sw = new Stopwatch();
            sw.Start();
            
            //generate images
            for (int i = 0; i < ThreadCount; i++) {
                //assign i to a new variable because i will be modified by the loop (i.e. the i++ statement)
                int threadId = i;

                int clipHeight = offsetY + (threadId < restOffsetY ? 1 : 0);
                int totalOffsetY = offsetY * threadId + Math.Min(threadId, restOffsetY);

                threads[threadId] = new Thread(() => {
                    images[threadId] = this.Generate(this.ClientSize, new Rectangle(0, totalOffsetY, width, clipHeight),
                        this.Center, this.ZoomScale, DefaultColorMapper);
                });
                threads[threadId].Start();
            }
            
            //wait for threads to finish and draw the generated images
            for (int i = 0; i < ThreadCount; i++) {
                threads[i].Join();

                int totalOffsetY = offsetY * i + Math.Min(i, restOffsetY);

                e.Graphics.DrawImage(images[i], new Point(0, totalOffsetY));
            }

            sw.Stop();
            Console.WriteLine($"time: {sw.ElapsedMilliseconds}");
        }

        public int MaxMandelNumber { get; set; }
        public static readonly Color backgroundColor = Color.FromArgb(0, 0, 0);

        /// <summary>
        /// Generates the mandelbrot image
        /// </summary>
        public Bitmap Generate(Size totalCanvasSize, Rectangle clip, PointD mandelCenter, double scale, MandelColorMapper colorFromIterationCount) {
            //canvas will be the return value
            Bitmap canvas = new Bitmap(clip.Width, clip.Height);

            var pixelMapper = DefaultPixelToMandelCoordsMapper(mandelCenter, totalCanvasSize, scale);

            Point pixelCoordinate = new Point(0, 0);
            for (pixelCoordinate.Y = clip.Top; pixelCoordinate.Y < clip.Bottom; pixelCoordinate.Y++) {
                for (pixelCoordinate.X = clip.Left; pixelCoordinate.X < clip.Right; pixelCoordinate.X++) {

                    PointD mandelCoordinates = pixelMapper(pixelCoordinate);
                    int? mandelNumber = MandelNumber(mandelCoordinates);
                    Color pixelColor = mandelNumber != null ?
                        colorFromIterationCount((int)mandelNumber)
                        : backgroundColor;

                    canvas.SetPixel(pixelCoordinate.X - clip.X, pixelCoordinate.Y - clip.Y, pixelColor);
                }
            }

            return canvas;
        }

        /// <summary>
        /// returns a lambda that maps pixel coordinates relative to the top-left of the MandelbrotImage
        /// to the coordinates in the mandelbrot
        /// </summary>        
        public PixelToCoordinate DefaultPixelToMandelCoordsMapper(PointD center, Size canvasSize, double scale) {
            double canvasWidth = canvasSize.Width;
            double canvasHeight = canvasSize.Height;
            double offsetX = canvasWidth / canvasHeight / -2.0;
            const double offsetY = -0.5;

            return (pixelCoords) => new PointD(
                center.X + (offsetX + pixelCoords.X / canvasHeight) * scale,
                -(center.Y + (offsetY + pixelCoords.Y / canvasHeight) * scale)
            );
        }

        /// <summary>
        /// Generates the mandel number for a given coordinate in the picture. Returns null if mandelNumber > this.maxMandelNumber
        /// </summary>
        /// <returns></returns>
        public int? MandelNumber(PointD coordinates) {            
            double a = 0.0, b = 0.0;
            int result = 0;
            //destructuring the PointD appears to be faster than passing the entire struct to the mandelTransform method for some reason
            double x = coordinates.X, y = coordinates.Y;

            const double MAX_MANDEL_DISTANCE = 2.0;

            while (Pythagoras(a, b) < MAX_MANDEL_DISTANCE * MAX_MANDEL_DISTANCE) {
                (a, b) = MandelTransform(a, b, x, y);

                if (++result > this.MaxMandelNumber) {
                    return null;
                }
            }

            return result;
        }

        /// <summary>
        /// Applies the core function of the mandelbrot (z^2 + c)
        /// </summary>
        /// <returns>new value for a and b</returns>
        private static (double newA, double newB) MandelTransform(double a, double b, double x, double y) {
            return (a * a - b * b + x,
                    2.0 * a * b + y);
        }

        public static double Pythagoras(double a, double b) {
            return a * a + b * b;
        }

        /// <summary>
        /// The method that maps mandel numbers to a color
        /// </summary>
        public static Color DefaultColorMapper(int mandelNumber) {
            return Color.FromArgb(Math.Min(255, mandelNumber / 2), Math.Min(255, mandelNumber * 2 / 3), Math.Min(255, mandelNumber));
        }
    }
}
