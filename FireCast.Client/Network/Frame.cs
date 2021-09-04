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
    }
}
