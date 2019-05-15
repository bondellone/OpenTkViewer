using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using OpenTK;
using OpenTK.Graphics;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Point = System.Drawing.Point;

namespace OpenTkViewer.Controls
{
    public class OpenTkControl : WindowsFormsHost
    {
        public GLControl GlControl { get; set; }

        public OpenTkControl()
        {
            var mode = new GraphicsMode(32, 24, 0, 8);
            GlControl = new GLControl(mode)
            {
                Dock = DockStyle.Fill,
                Location = new Point(0,0),
                VSync = false
            };

            Child = GlControl;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
        }

        public void Update()
        {
            GlControl.Invalidate();
        }
    }
}