using FireCast.Server.Processors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FireCast.Server.Network
{
    public class UserDatagramSenderManager : INetworkManager
    {
        private const int MAximumUDP_PacketSize = 60000;
        private const int PackageHeaderLength = 3;
        private readonly Random _random;
        private readonly UdpClient _sender;
        private readonly IPEndPoint _iPEndPoint;
        public UserDatagramSenderManager(string address, int port)
        {
            this._random = new Random();
            this._sender = new UdpClient();
            this._iPEndPoint = new IPEndPoint(IPAddress.Parse(address), port);
        }

        public async Task SendImage(byte[] bytesToSend)
        {
            var packages = ImageProcessor.GetCipheredPackages(bytesToSend, _random);
            await SendByteArray(packages);
        }

        public async Task SendImage(Bitmap bitmap, List<Rectangle> rectangles)
        {
            var packages = ImageProcessor.GetMappedImage(bitmap, _random, rectangles);
            await SendByteArray(packages);
        }

        private async Task SendByteArray(IEnumerable<byte[]> bytesToSend)
        {
            foreach (var package in bytesToSend)
            {
                await _sender.SendAsync(package, package.Length, _iPEndPoint);
            }
        }

        public void Dispose()
        {
            this._sender?.Close();
            this._sender?.Dispose();
        }
    }
}
