using FireCast.Client.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace FireCast.Client
{
    public class FrameBuffer
    {
        private readonly Queue<byte[]> bytesInBuffer;
        private Frame frame;
        public FrameBuffer()
        {
            this.bytesInBuffer = new Queue<byte[]>();
            frame = new Frame();
        }
        public void AddToBufferQueue(byte[] bytes)
        {
            this.bytesInBuffer.Enqueue(bytes);
        }

        public int GetBufferLength()
        {
            return this.bytesInBuffer.Count;
        }

        public Frame ProcessBuffer()
        {
            if(bytesInBuffer.Count > 0)
            {
                var bytesFrme = bytesInBuffer.Dequeue();
                if (bytesFrme.Length == 3)
                {
                    ProcessHeaderPackage(bytesFrme);
                }
                else
                {
                    ProcessPayloadPackage(bytesFrme);
                }
                return GetFrame();
            }
            return null;
        }
        private Frame GetFrame()
        {
            Frame innerFrame = null;
            if (frame.IsFrameComplete())
            {
                innerFrame = frame;
                frame = new Frame();
            }
            return innerFrame;
        }
        private void ProcessPayloadPackage(byte[] buffer)
        {
            CheckFrameSignature(buffer);

            byte[] payload = new byte[buffer.Length - 3];

            Array.Copy(buffer, 3, payload, 0, payload.Length);

            frame.PackageChunks.Add(new Chunk()
            {
                Order = buffer[0],
                Data = payload
            });

        }
        private void ProcessHeaderPackage(byte[] buffer)
        {
            CheckFrameSignature(buffer);

            if (buffer[0] != 0)
            {
                frame.Chunks = buffer[0];
                frame.Header = buffer;
            }
            else
            {
                frame.Tail = buffer;
            }
        }
        private void CheckFrameSignature(byte[] buffer)
        {
            if (frame.Signarute == null)
            {
                frame.Signarute = new byte[2];
                frame.Signarute[0] = buffer[1];
                frame.Signarute[1] = buffer[2];
            }
            else
            {
                if (frame.Signarute[0] != buffer[1] || frame.Signarute[1] != buffer[2])
                {
                    this.frame = new Frame();
                    frame.Signarute = new byte[2];
                    frame.Signarute[0] = buffer[1];
                    frame.Signarute[1] = buffer[2];
                }
            }
        }
    }
}
