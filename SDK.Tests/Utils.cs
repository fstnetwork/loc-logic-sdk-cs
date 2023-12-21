namespace SDK.Tests;

public class TestUtils
{
    [Fact]
    public void Test_BytesToUInt128()
    {
        var expected1 = UInt128.Zero;
        byte[] bytes1 = new byte[16];
        Assert.Equal(expected1, Utils.BytesToUInt128(bytes1));

        var expected2 = UInt128.Parse("168936194220010199234602573438346025250");
        byte[] bytes2 = new byte[] {
            0x7F, 0x17, 0xED, 0x71, 0x8D, 0x45, 0x4A, 0x39, 0x85, 0xFD, 0x9A, 0x94, 0xB9, 0x7B,
            0x59, 0x22
        };
        Assert.Equal(expected2, Utils.BytesToUInt128(bytes2));

        // invalid length
        byte[] bytes3 = new byte[15];
        Assert.Throws<ArgumentException>(() => Utils.BytesToUInt128(bytes3));
    }

    [Fact]
    public void Test_UInt128ToBytes()
    {
        var expected1 = new byte[16];
        var value1 = UInt128.Zero;
        Assert.Equal(expected1, Utils.UInt128ToBytes(value1));

        var expected2 = new byte[] {
            0x7F, 0x17, 0xED, 0x71, 0x8D, 0x45, 0x4A, 0x39, 0x85, 0xFD, 0x9A, 0x94, 0xB9, 0x7B,
            0x59, 0x22
        };
        var value2 = UInt128.Parse("168936194220010199234602573438346025250");
        Assert.Equal(expected2, Utils.UInt128ToBytes(value2));
    }

    [Fact]
    public void Test_EncodeUint128()
    {
        var expected1 = "AAAAAAAAAAAAAAAAAAAAAA";
        var value1 = UInt128.Zero;
        Assert.Equal(expected1, Utils.EncodeUint128(value1));

        var expected2 = "ZX_AgxP-l0Vc2ta04_M8Tg";
        var value2 = UInt128.Parse("134915353883031613463100331535981821006");
        Assert.Equal(expected2, Utils.EncodeUint128(value2));

        var expected3 = "fxftcY1FSjmF_ZqUuXtZIg";
        var value3 = UInt128.Parse("168936194220010199234602573438346025250");
        Assert.Equal(expected3, Utils.EncodeUint128(value3));
    }
}