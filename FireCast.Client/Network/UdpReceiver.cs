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
        private object _lock = new object();
        public event EventHandler<IEnumerable<Frame>> OnFramesComposed;
        private readonly Queue<byte[]> receivedBytes;

        public UdpReceiver(string ipAddress, int port)
        {
            this._udpClient = new UdpClient(port);
            this._frames = new List<Frame>();
            this.receivedBytes = new Queue<byte[]>();
        }
        public void Dispose()
        {
            this._udpClient?.Close();
            this._udpClient?.Dispose();
        }

        public async Task StartReceiverAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            new Task(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (receivedBytes.Count > 0)
                    {
                        byte[] buffer = receivedBytes.Dequeue();
                        Frame frame;
                        if (buffer.Length == 3)
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
            }).Start();
            while (!cancellationToken.IsCancellationRequested)
            {
                var udpResult = await this._udpClient.ReceiveAsync();
                this.receivedBytes.Enqueue(udpResult.Buffer); 
            }
        }
        private Frame ProcessPayloadPackage(byte[] buffer)
        {
            var packageOrder = buffer[0];
            Frame existingFrame;
            lock (_lock)
            {
                existingFrame = _frames.FirstOrDefault(x => x.Signarute[0] == buffer[1] && x.Signarute[1] == buffer[2]);
            }
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
                lock (_lock)
                {
                    this._frames.Add(existingFrame);
                }
            }
            return existingFrame;
        }
        private Frame ProcessHeaderPackage(byte[] buffer)
        {
            Frame existingFrame;

            lock (_lock)
            {
                existingFrame = _frames.FirstOrDefault(x => x.Signarute[0] == buffer[1] && x.Signarute[1] == buffer[2]);
            }
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
                lock (_lock)
                {
                    this._frames.Add(existingFrame);
                }
            }
            return existingFrame;
        }
        private void CheckFrameComplicity(Frame frame)
        {
            List<Frame> completedPackages;
            lock (_lock)
            {
                completedPackages = this._frames.Where(x => x.IsFrameComplete()).ToList();
            }
            foreach(var pckg in completedPackages)
            {
                lock (_lock)
                {
                    _frames.Remove(pckg);
                }
            }
            this?.OnFramesComposed.Invoke(this, completedPackages);
        }
    }
}
