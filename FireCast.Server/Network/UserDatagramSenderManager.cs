using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireCast.Server.Network
{
    public class UserDatagramSenderManager : INetworkManager
    {
        private const int MAximumUDP_PacketSize = 65535;
        private const int PackageHeaderLength = 17;
        public UserDatagramSenderManager()
        {

        }

        public async Task SendImage(byte[] bytesToSend)
        {
            var packages = GetCipheredPAckages(bytesToSend);

        }

        private List<byte[]> GetCipheredPAckages(byte[] bytes)
        {
            var payloadLength = MAximumUDP_PacketSize - PackageHeaderLength;
            int packagesCount = bytes.Length / payloadLength;
            packagesCount += bytes.Length % payloadLength == 0 ? 0 : 1;
            List<byte[]> packages = new List<byte[]>(packagesCount + 2);
            Guid guid = Guid.NewGuid();
            byte[] guidBytes = guid.ToByteArray();
            byte[] headerPackage = new byte[PackageHeaderLength];
            headerPackage[0] = (byte)packagesCount;
            Array.Copy(guidBytes, 0, headerPackage, 1, guidBytes.Length);
            packages.Add(headerPackage);
            for(int x = 0; x < packagesCount; x++)
            {
                byte[] package = new byte[MAximumUDP_PacketSize];
                Array.Copy(headerPackage, 0, package, 0, PackageHeaderLength);
                package[0] = (byte)x;
                Array.Copy(bytes, x * payloadLength, package, PackageHeaderLength, payloadLength);
                packages.Add(package);
            }
            byte[] tailPackage = new byte[PackageHeaderLength];
            Array.Copy(headerPackage, 0, tailPackage, 0, PackageHeaderLength);
            tailPackage[0] = 0;
            packages.Add(tailPackage);
            return packages;
        }
    }
}
