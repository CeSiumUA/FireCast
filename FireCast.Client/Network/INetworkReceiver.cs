using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FireCast.Client.Network
{
    public interface INetworkReceiver : IDisposable
    {
        public Task<byte[]> ReceiveNextPackage();
    }
}
