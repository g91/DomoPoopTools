using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Numerics;

namespace Bobs_poop_Toolsx
{
    public class OverlayForm : Form
    {
        private SharpDX.Direct2D1.WindowRenderTarget device;
        private SolidColorBrush brush;
        private Point lastMousePosition;
        private Button button;
        private TextBox textBox;

        public OverlayForm()
        {
            // Set the form properties
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.Black;
            TransparencyKey = Color.Black;
            ShowInTaskbar = false;
            TopMost = true;
            Size = new Size(300, 200);

            // Initialize the DirectX device
            var factory = new Factory();
            device = new SharpDX.Direct2D1.WindowRenderTarget(factory, new RenderTargetProperties(), new HwndRenderTargetProperties
            {
                Hwnd = Handle,
                PixelSize = new Size2(ClientSize.Width, ClientSize.Height),
                PresentOptions = PresentOptions.Immediately
            });

            // Create a brush for drawing the square
            brush = new SolidColorBrush(device, new RawColor4(Color.Red.R / 255.0f, Color.Red.G / 255.0f, Color.Red.B / 255.0f, Color.Red.A / 255.0f));

            // Register the mouse event handlers
            MouseDown += OverlayForm_MouseDown;
            MouseMove += OverlayForm_MouseMove;
            MouseUp += OverlayForm_MouseUp;

            // Create the button
            button = new Button();
            button.Text = "Hooray";
            button.Location = new Point(100, 100);
            button.Click += Button_Click;
            Controls.Add(button);

            // Create the text box
            textBox = new TextBox();
            textBox.ReadOnly = true;
            textBox.Location = new Point(100, 25);
            textBox.Size = new Size(150, 20);
            Controls.Add(textBox);

            // Update the text box with the amount of system RAM
            UpdateRAMInfo();
        }

        private void Button_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Hooray!");
        }

        private void OverlayForm_MouseDown(object sender, MouseEventArgs e)
        {
            lastMousePosition = e.Location;
            Capture = true;
        }

        private void OverlayForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (Capture)
            {
                Location = new Point(Location.X + (e.X - lastMousePosition.X), Location.Y + (e.Y - lastMousePosition.Y));
            }
        }

        private void OverlayForm_MouseUp(object sender, MouseEventArgs e)
        {
            Capture = false;
        }
        public void UpdateRAMInfo()
        {
            textBox.Text = string.Format("{0} MB", Math.Round((new PerformanceCounter("Memory", "Available Bytes").NextValue() / 1024f) / 1024f, 2));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Clear the device and draw the objects
            device.BeginDraw();
            device.Clear(new RawColor4(Color.Black.R / 255.0f, Color.Black.G / 255.0f, Color.Black.B / 255.0f, Color.Black.A / 255.0f));
            device.FillRectangle(new RawRectangleF(ClientSize.Width / 2 - 3, ClientSize.Height / 2 - 3, ClientSize.Width / 2 + 3, ClientSize.Height / 2 + 3), brush);
            
            device.EndDraw();

            device.BeginDraw();
            device.DrawEllipse(new Ellipse(new RawVector2(ClientSize.Width / 2, ClientSize.Height / 2), ClientSize.Width / 2 - 10, ClientSize.Height / 2 - 10), brush);
            device.EndDraw();

            base.OnPaint(e);
        }
    }

    public class OverlayForm2 : Form
    {
        private SharpDX.Direct2D1.WindowRenderTarget device;
        private SolidColorBrush brush;
        private Point lastMousePosition;
        private TextBox textBox;

        public OverlayForm2()
        {
            // Set the form properties
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.Black;
            TransparencyKey = Color.Black;
            ShowInTaskbar = false;
            TopMost = true;
            TopMost = true;
            Size = new Size(300, 200);

            // Initialize the DirectX device
            var factory = new Factory();
            device = new SharpDX.Direct2D1.WindowRenderTarget(factory, new RenderTargetProperties(), new HwndRenderTargetProperties
            {
                Hwnd = Handle,
                PixelSize = new Size2(ClientSize.Width, ClientSize.Height),
                PresentOptions = PresentOptions.Immediately
            });

            // Create a brush for drawing the square
            brush = new SolidColorBrush(device, new RawColor4(Color.Red.R / 255.0f, Color.Red.G / 255.0f, Color.Red.B / 255.0f, Color.Red.A / 255.0f));
            
            
            // Register the mouse event handlers
            MouseDown += OverlayForm_MouseDown;
            MouseMove += OverlayForm_MouseMove;
            MouseUp += OverlayForm_MouseUp;

        }

        private void OverlayForm_MouseDown(object sender, MouseEventArgs e)
        {
            lastMousePosition = e.Location;
            Capture = true;
        }

        private void OverlayForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (Capture)
            {
                Location = new Point(Location.X + (e.X - lastMousePosition.X), Location.Y + (e.Y - lastMousePosition.Y));
            }
        }

        private void OverlayForm_MouseUp(object sender, MouseEventArgs e)
        {
            Capture = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {

            // Clear the device and draw the objects
            device.BeginDraw();
            device.Clear(new RawColor4(Color.Black.R / 255.0f, Color.Black.G / 255.0f, Color.Black.B / 255.0f, Color.Black.A / 255.0f));
            device.FillRectangle(new RawRectangleF(ClientSize.Width / 2 - 3, ClientSize.Height / 2 - 3, ClientSize.Width / 2 + 3, ClientSize.Height / 2 + 3), brush);

            device.EndDraw();

            device.BeginDraw();
            device.DrawEllipse(new Ellipse(new RawVector2(ClientSize.Width / 2, ClientSize.Height / 2), ClientSize.Width / 2 - 10, ClientSize.Height / 2 - 10), brush);
            device.EndDraw();

            base.OnPaint(e);
        }
    }
}
