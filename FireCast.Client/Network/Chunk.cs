using System;
using System.Collections.Generic;
using System.Text;

namespace FireCast.Client.Network
{
    public class Chunk
    {
        public short Order { get; set; }
        public byte[] Data { get; set; }
    }
}
