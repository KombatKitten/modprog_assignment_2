using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mandelbrot;

namespace Mandelbrot {
    public partial class MainScreen : Form {
        const int MANDELBROT_OFFSET_Y = 90;
        /// <summary>
        /// lalal
        /// </summary>
        public MainScreen() {
            InitializeComponent();

            //initialize controls
            Label centerXLabel = new Label() {
                Text = "Center X",
            };
            SetRectangle(centerXLabel, new Point(0, 0));
            SetRectangle(this.centerX, new Point(1, 0));

            Label centerYLabel = new Label() {
                Text = "Center Y",
            };
            SetRectangle(centerYLabel, new Point(0, 1));
            SetRectangle(this.centerY, new Point(1, 1));

            Label maxIterCountLabel = new Label() {
                Text = "Max Iterations",
            };
            SetRectangle(this.maxIterCount, new Point(3, 0));
            SetRectangle(maxIterCountLabel, new Point(2, 0));

            Label scaleLabel = new Label() {
                Text = "Scale",
            };
            SetRectangle(this.scale, new Point(3, 1));
            SetRectangle(scaleLabel, new Point(2, 1));

            Button okButton = new Button() {
                Text = "OK",
            };
            okButton.Click += this.OnOkButtonClick;
            SetRectangle(okButton, new Point(4, 1));

            Label threadCountLabel = new Label() {
                Text = "Number of Threads",
            };
            SetRectangle(this.threadCount, new Point(5, 0));
            SetRectangle(threadCountLabel, new Point(4, 0));

            ComboBox highlights = new ComboBox();
            SetRectangle(highlights, new Point(5, 1));

            highlights.SelectedValueChanged += (sender, e) => {
                ((ComboBoxItem)highlights.SelectedItem).Click();
            };

            var basis = new HighlightItem("Basis", new PointD(0.0, 0.0), 2.0, this.mandelbrotImage, this.scale, this.centerX, this.centerY);
            var infiniteZoom = new HighlightItem("Infinite Zoom", new PointD(-1.4845903478, 0.0), 1.0, this.mandelbrotImage, this.scale, this.centerX, this.centerY);
            var star = new HighlightItem("Star", new PointD(-0.6702091879, -0.4580609753), 0.07, this.mandelbrotImage, this.scale, this.centerX, this.centerY);
            var explosion = new HighlightItem("Explosion", new PointD(-0.7084314944, -0.2573555293), 0.00273082440794726, this.mandelbrotImage, this.scale, this.centerX, this.centerY);

            highlights.Items.AddRange(new ComboBoxItem[]{
                basis,
                infiniteZoom,
                star,
                explosion,
            });
            highlights.SelectedItem = basis;

            this.Controls.AddRange(new Control[]{
                centerXLabel,
                centerYLabel,
                maxIterCountLabel,
                scaleLabel,
                threadCountLabel,
                okButton,
                highlights,
                this.centerX,
                this.centerY,
                this.scale,
                this.maxIterCount,
                this.threadCount,
                this.mandelbrotImage
            });
            
            //add events
            this.Resize += OnFormResize;
            this.MouseWheel += OnMouseWheel;
            this.mandelbrotImage.MouseClick += OnMandelbrotClick;

            this.Text = "Mandelbrot";
            this.WindowState = FormWindowState.Maximized;
            this.UpdateMandelbrotImage();
        }

        /// <summary>
        /// Updates the values of the center coordinate NumericUpDowns
        /// </summary>
        private void OnMandelbrotClick(object sender, MouseEventArgs e) {
            this.centerX.Value = (decimal)this.mandelbrotImage.Center.X;
            this.centerY.Value = (decimal)this.mandelbrotImage.Center.Y;
            this.scale.Value = (decimal)this.mandelbrotImage.ZoomScale;
        }

        /// <summary>
        /// Zooms in and redraws the mandelbrotImage
        /// </summary>
        private void OnMouseWheel(object sender, MouseEventArgs e) {
            double scrollValue = Math.Max(0.01, 1.0 - e.Delta / 500.0);
            this.mandelbrotImage.ZoomScale *= scrollValue;
            this.scale.Value *= (decimal)scrollValue;

            //redraw the mandelbrotImage
            mandelbrotImage.Invalidate();
        }

        private void OnOkButtonClick(object sender, EventArgs e) {
            UpdateMandelbrotImage();
        }

        /// <summary>
        /// Transfers all the values from the input fields to the mandelbrotImage object
        /// </summary>
        private void UpdateMandelbrotImage() {
            this.mandelbrotImage.ZoomScale = (double)this.scale.Value;
            this.mandelbrotImage.Center = new PointD((double)this.centerX.Value, (double)this.centerY.Value);
            this.mandelbrotImage.MaxMandelNumber = (int)maxIterCount.Value;
            this.mandelbrotImage.ThreadCount = (int)this.threadCount.Value;

            this.mandelbrotImage.Invalidate();
        }

        /// <summary>
        /// Sets the location and size of a control
        /// </summary>
        /// <param name="frameLocation">the location in a virtual frame</param>
        private static void SetRectangle(Control control, Point frameLocation) {
            Size rowMargin = new Size(10, 10);
            int rowHeight = 40;
            int[] columns = new int[] { 0, 100, 300, 400, 600, 700, 900 };
            
            control.Location =
                new Point(columns[frameLocation.X] + rowMargin.Width, rowHeight * frameLocation.Y + rowMargin.Height);
            control.Size =
                new Size(columns[frameLocation.X + 1] - columns[frameLocation.X] - rowMargin.Width * 2, rowHeight - rowMargin.Height / 2);
        }

        /// <summary>
        /// Updates the size of the mandelbrotImage control
        /// </summary>
        private void OnFormResize(object sender, EventArgs e) {
            this.mandelbrotImage.Size = new Size(this.ClientSize.Width, this.ClientSize.Height - MANDELBROT_OFFSET_Y);

            //make sure the entire mandelbrot is redrawn
            this.mandelbrotImage.Invalidate();
        }

        //initialize controls
        public MandelbrotImage mandelbrotImage { get; private set; } = new MandelbrotImage() {
            Location = new Point(0, MANDELBROT_OFFSET_Y),
        };

        private NumericUpDown centerX = new NumericUpDown() {
                DecimalPlaces = 10,
                Minimum = -100,
            },
            centerY = new NumericUpDown() {
                DecimalPlaces = 10,
                Minimum = -100,
            },
            scale = new NumericUpDown() { Value = 2, DecimalPlaces = 20},
            maxIterCount = new NumericUpDown() { Minimum = 0, Maximum = 20000, Value = 600 },
            threadCount = new NumericUpDown() { Minimum = 1, Maximum = 100, Value = 8 };
    }
}

/// <summary>
/// Represent an item for <see cref="ComboBox.Items"/> with a specific test and <see cref="OnClick"/> event
/// </summary>
public class ComboBoxItem {
    public string Text { get; set; } = "No Text Specified";
    public event EventHandler OnClick;

    public void Click() {
        OnClick?.Invoke(this, new EventArgs());
    }

    public override string ToString() {
        return this.Text;
    }
}

public class HighlightItem : ComboBoxItem {
    public HighlightItem(string text, PointD newCenter, double scale, MandelbrotImage target,
        NumericUpDown scaleSelecter, NumericUpDown xSelector, NumericUpDown ySelector) {
        this.Text = text;

        this.OnClick += (sender, e) => {
            ///change selector values
            scaleSelecter.Value = (decimal)scale;
            xSelector.Value = (decimal)newCenter.X;
            ySelector.Value = (decimal)newCenter.Y;

            ///change <see cref="MandelbrotImage"/> values
            target.ZoomScale = scale;
            target.Center = newCenter;

            target.Invalidate();
        };
    }
}