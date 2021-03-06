using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FireCast.Server.Network
{
    public interface INetworkManager : IDisposable
    {
        public Task SendImage(byte[] bytesToSend);
    }
}
