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
        public event EventHandler<Frame> OnFrameComposed;
        private readonly Queue<byte[]> receivedBytes;
        private Frame Frame;
        public UdpReceiver(string ipAddress, int port)
        {
            this._udpClient = new UdpClient(port);
            this.receivedBytes = new Queue<byte[]>();
        }
        public void Dispose()
        {
            this._udpClient?.Close();
            this._udpClient?.Dispose();
        }

        public async Task StartReceiverAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Frame = new Frame();
            new Task(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (receivedBytes.Count > 0)
                    {
                        byte[] buffer = receivedBytes.Dequeue();
                        if (buffer.Length == 3)
                        {
                            ProcessHeaderPackage(buffer);
                        }
                        else
                        {
                            ProcessPayloadPackage(buffer);
                        }
                        CheckFrameComplicity();
                    }
                }
            }).Start();
            while (!cancellationToken.IsCancellationRequested)
            {
                var udpResult = await this._udpClient.ReceiveAsync();
                if(udpResult.Buffer == null)
                {

                }
                this.receivedBytes.Enqueue(udpResult.Buffer); 
            }
        }
        private void ProcessPayloadPackage(byte[] buffer)
        {
            CheckFrameSignature(buffer);

            byte[] payload = new byte[buffer.Length - 3];

            Array.Copy(buffer, 3, payload, 0, payload.Length);

            Frame.PackageChunks.Add(new Chunk()
            {
                Order = buffer[0],
                Data = payload
            });

        }
        private void ProcessHeaderPackage(byte[] buffer)
        {
            CheckFrameSignature(buffer);
            
            if(buffer[0] != 0)
            {
                Frame.Chunks = buffer[0];
                Frame.Header = buffer;
            }
            else
            {
                Frame.Tail = buffer;
            }
        }
        private void CheckFrameSignature(byte[] buffer)
        {
            if (Frame.Signarute == null)
            {
                Frame.Signarute = new byte[2];
                Frame.Signarute[0] = buffer[1];
                Frame.Signarute[1] = buffer[2];
            }
            else
            {
                if (Frame.Signarute[0] != buffer[1] || Frame.Signarute[1] != buffer[2])
                {
                    this.Frame = new Frame();
                    Frame.Signarute = new byte[2];
                    Frame.Signarute[0] = buffer[1];
                    Frame.Signarute[1] = buffer[2];
                }
            }
        }
        private void CheckFrameComplicity()
        {
            if (Frame.IsFrameComplete())
            {
                this?.OnFrameComposed.Invoke(this, Frame);
                Frame = new Frame();
            }
        }
    }
}
