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

    public partial class MainScreen : Form {
        const int MANDELBROT_OFFSET_Y = 200;

        public MainScreen() {
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
}
