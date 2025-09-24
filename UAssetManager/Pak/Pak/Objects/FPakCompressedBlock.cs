using System.Runtime.InteropServices;

namespace UAssetManager.Pak.Pak.Objects;

[StructLayout(LayoutKind.Sequential)]
public struct FPakCompressedBlock
{
    public long CompressedStart;
    public long CompressedEnd;
    public long Size => CompressedEnd - CompressedStart;

    public override string ToString() => $"From {CompressedStart} To {CompressedEnd} (={Size})";
}