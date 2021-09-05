using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FireCast.Server.Capture
{
    public class WindowsGraphicsProvider : IGraphicsProvider
    {
        public int DefaultScreen { get; set; }
        public int Height
        {
            get
            {
                return height;
            }
            set
            {
                this.height = value;
            }
        }
        public int Width
        {
            get
            {
                return width;
            }
            set
            {
                this.width = value;
            }
        }
        public WindowsGraphicsProvider()
        {
            this.DefaultScreen = 0;
            this.Height = Screen.AllScreens[DefaultScreen].Bounds.Height;
            this.Width = Screen.AllScreens[DefaultScreen].Bounds.Width;
        }
        public void Initialize()
        {
            bitImage = new Bitmap(this.Width, this.Height);
            graphics = Graphics.FromImage(bitImage);
        }
        public async Task<byte[]> GetRawInstantImage()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                if (bitImage == null || graphics == null) Initialize();
                graphics?.CopyFromScreen(0, 0, 0, 0, new Size(this.Width, this.Height));
                bitImage?.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }
        private int height = 600;
        private int width = 800;
        private int defaultScreen = 0;
        private Bitmap? bitImage;
        private Graphics? graphics;
    }
}
