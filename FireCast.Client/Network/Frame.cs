using System;
using System.Collections.Generic;
using System.Text;

namespace FireCast.Client.Network
{
    public class Frame
    {
        public byte[] Signarute { get; set; }
        public int Chunks { get; set; }
        public byte[] Header { get; set; }
        public List<Chunk> PackageChunks = new List<Chunk>();
        public byte[] Tail { get; set; }
        public bool IsFrameComplete()
        {
            return (Chunks == PackageChunks.Count)
                && (Tail?.Length == Header?.Length)
                && (Tail?.Length == 3)
                && (Header?.Length == 3)
                && (Signarute[0] == Tail[1])
                && (Signarute[1] == Tail[2]) 
                && (Signarute[0] == Header[1])
                && (Signarute[1] == Header[2])
                && (Chunks == Header[0]);
        }
    }
}
