using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireCast.Server.Capture
{
    public interface IGraphicsProvider
    {
        int DefaultScreen { get; set; }
        int Height { get; set; }
        int Width { get; set; }
        byte[] GetRawInstantImage();
        Bitmap GetCapturedImage();
    }
}
