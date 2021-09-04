using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FireCast.Server.Network
{
    public class UserDatagramSenderManager : INetworkManager
    {
        private const int MAximumUDP_PacketSize = 65535;
        private const int PackageHeaderLength = 3;
        private readonly Random _random;
        private readonly UdpClient _sender;
        private readonly IPEndPoint _iPEndPoint;
        public UserDatagramSenderManager(string address, int port)
        {
            this._random = new Random();
            this._sender = new UdpClient(address, port);
            this._iPEndPoint = new IPEndPoint(IPAddress.Parse(address), port);
            this._sender.Connect(_iPEndPoint);
        }

        public async Task SendImage(byte[] bytesToSend)
        {
            var packages = GetCipheredPAckages(bytesToSend);
            foreach(var package in packages)
            {
                await _sender.SendAsync(package, package.Length);
            }
        }

        private List<byte[]> GetCipheredPAckages(byte[] bytes)
        {
            var payloadLength = MAximumUDP_PacketSize - PackageHeaderLength;
            int packagesCount = bytes.Length / payloadLength;
            var remnant = bytes.Length % payloadLength;
            packagesCount += remnant == 0 ? 0 : 1;
            List<byte[]> packages = new List<byte[]>(packagesCount + 2);
            byte[] guidBytes = new byte[2];
            _random.NextBytes(guidBytes);
            byte[] headerPackage = new byte[PackageHeaderLength];
            headerPackage[0] = (byte)packagesCount;
            Array.Copy(guidBytes, 0, headerPackage, 1, guidBytes.Length);
            packages.Add(headerPackage);
            for(int x = 0; x < packagesCount; x++)
            {
                var bytesToCopy = payloadLength;
                if (remnant != 0 && x == packagesCount - 1)
                {
                    bytesToCopy = remnant;
                }
                byte[] package = new byte[bytesToCopy + PackageHeaderLength];
                Array.Copy(headerPackage, 0, package, 0, PackageHeaderLength);
                package[0] = (byte)x;
                Array.Copy(bytes, x * payloadLength, package, PackageHeaderLength, bytesToCopy);
                packages.Add(package);
            }
            byte[] tailPackage = new byte[PackageHeaderLength];
            Array.Copy(headerPackage, 0, tailPackage, 0, PackageHeaderLength);
            tailPackage[0] = 0;
            packages.Add(tailPackage);
            return packages;
        }
        public void Dispose()
        {
            this._sender?.Close();
            this._sender?.Dispose();
        }
    }
}
