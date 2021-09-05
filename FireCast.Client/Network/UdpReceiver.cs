using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FireCast.Client.Network
{
    public class UdpReceiver : INetworkReceiver
    {
        private readonly UdpClient _udpClient;
        private IPEndPoint remoteEndPoint;
        public UdpReceiver(string ipAddress, int port)
        {
            this._udpClient = new UdpClient(port);
        }
        public void Dispose()
        {
            this._udpClient?.Close();
            this._udpClient?.Dispose();
        }

        public async Task<byte[]> ReceiveNextPackage()
        {
            var udpResult = await _udpClient.ReceiveAsync();
            this.remoteEndPoint = udpResult.RemoteEndPoint;
            return udpResult.Buffer;
        }   
    }
}
