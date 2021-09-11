
namespace FireCast.Lib
{
    public static class HashHelper
    {
        public static UInt64 CreateSimpleHash(byte[] bytes, int headerLength = 0)
        {
            UInt64 hash = 0;

            ulong multiplier = 1;

            byte[] clearArray = new byte[bytes.Length - headerLength];

            Array.Copy(bytes, headerLength, clearArray, 0, clearArray.Length);

            for(int x = 0; x < clearArray.Length; x++)
            {
                hash += clearArray[x] * multiplier;
                multiplier *= 37;
            }

            return hash;
        }
    }
}
