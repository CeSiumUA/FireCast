using FireCast.Lib;
using Xunit;

namespace FireCast.Tests;
public class HashingTest
{
    [Fact]
    public void Hashing_Test()
    {
        var bytes1 = new byte[] { 255, 56, 12, 73 };
        var bytes2 = new byte[] { 255, 56, 12, 73 };
        var hash1 = HashHelper.CreateSimpleHash(bytes1);
        var hash2 = HashHelper.CreateSimpleHash(bytes2);
        Assert.Equal(hash1, hash2);
    }
}