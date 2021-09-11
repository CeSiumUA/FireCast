using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FireCast.Server.Network
{
    public interface INetworkManager : IDisposable
    {
        public Task SendImage(byte[] bytesToSend);
        public Task SendImage(Bitmap bitmap, List<Rectangle> rectangles);
    }
}
