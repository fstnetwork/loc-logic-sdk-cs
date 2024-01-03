namespace Saffron.Runtime.Services;

public class Utils
{
    public static Saffron.Common.U128 ZeroU128()
    {
        return new Saffron.Common.U128
        {
            HighBits = 0,
            LowBits = 0,
        };
    }

    public static Saffron.Common.Uuid ZeroUuid()
    {
        return new Saffron.Common.Uuid
        {
            HighBits = 0,
            LowBits = 0,
        };
    }

    public static Saffron.Common.Revision Revision(int value)
    {
        return new Saffron.Common.Revision
        {
            Value = value,
        };
    }
}