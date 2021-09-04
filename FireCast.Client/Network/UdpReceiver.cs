using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FireCast.Client.Network
{
    public class UdpReceiver : INetworkReceiver
    {
        private readonly UdpClient _udpClient;
        private readonly List<Frame> _frames;
        public UdpReceiver(string ipAddress, int port)
        {
            this._udpClient = new UdpClient(ipAddress, port);
            this._frames = new List<Frame>();
        }
        public void Dispose()
        {
            this._udpClient?.Close();
            this._udpClient?.Dispose();
        }

        public async Task StartReceiverAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var udpResult = await this._udpClient.ReceiveAsync();
                byte[] buffer = udpResult.Buffer;
                Frame frame;
                if(buffer.Length == 3)
                {
                    frame = ProcessHeaderPackage(buffer);
                }
                else
                {
                    frame = ProcessPayloadPackage(buffer);
                }
                CheckFrameComplicity(frame);
            }
        }
        private Frame ProcessPayloadPackage(byte[] buffer)
        {
            var packageOrder = buffer[0];
            var existingFrame = _frames.FirstOrDefault(x => x.Signarute[0] == buffer[1] && x.Signarute[1] == buffer[2]);
            bool existsInCollection = existingFrame != null;
            if (!existsInCollection)
            {
                existingFrame = new Frame();
                existingFrame.Signarute = new byte[2];
                existingFrame.Signarute[0] = buffer[1];
                existingFrame.Signarute[1] = buffer[2];
            }

            byte[] payload = new byte[buffer.Length - 3];

            Array.Copy(buffer, 3, payload, 0, payload.Length);

            existingFrame.PackageChunks.Add(new Chunk()
            {
                Order = buffer[0],
                Data = payload
            });

            if (!existsInCollection)
            {
                this._frames.Add(existingFrame);
            }
            return existingFrame;
        }
        private Frame ProcessHeaderPackage(byte[] buffer)
        {
            var existingFrame = _frames.FirstOrDefault(x => x.Signarute[0] == buffer[1] && x.Signarute[1] == buffer[2]);
            bool existsInCollection = existingFrame != null;
            if (!existsInCollection)
            {
                existingFrame = new Frame();
                existingFrame.Signarute = new byte[2];
                existingFrame.Signarute[0] = buffer[1];
                existingFrame.Signarute[1] = buffer[2];
            }

            byte chunks = buffer[0];

            if(chunks == 0)
            {
                existingFrame.Tail = buffer;
            }
            else
            {
                existingFrame.Header = buffer;
                existingFrame.Chunks = chunks;
            }

            if (!existsInCollection)
            {
                this._frames.Add(existingFrame);
            }
            return existingFrame;
        }
        private void CheckFrameComplicity(Frame frame)
        {

        }
    }
}
