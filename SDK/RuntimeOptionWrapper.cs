using Saffron.ExternalRuntime;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public class RuntimeOptionWrapper
{
    public IntPtr Pointer;
    public int Size;

    public RuntimeOptionWrapper() { }

    public RuntimeOptionWrapper(RuntimeOption option)
    {
        byte[] bytes = Utils.ConvertToByteArray(option);
        IntPtr ptr = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes, 0, ptr, bytes.Length);

        Pointer = ptr;
        Size = bytes.Length;
    }

    public RuntimeOptionWrapper(IntPtr pointer)
    {
        var option = Marshal.PtrToStructure<RuntimeOptionWrapper>(pointer);
        if (option == null)
        {
            throw new Exception("RuntimeOptionWrapper is null");
        }

        Pointer = option.Pointer;
        Size = option.Size;
    }

    public RuntimeOption ToRuntimeOption()
    {
        byte[] bytes = new byte[Size];
        Marshal.Copy(Pointer, bytes, 0, Size);

        var option = Utils.ToObject<RuntimeOption>(bytes);
        if (option == null)
        {
            throw new Exception("RuntimeOption is null");
        }
        return option;
    }

    public void Free()
    {
        Marshal.FreeHGlobal(Pointer);
    }
}
