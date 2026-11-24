using System.Runtime.CompilerServices;

namespace Kt41;

internal static class GcWorkload
{
    internal const int ShortCount = 300_000;
    internal const int LongCount = 5_000;
    internal const int LohCount = 20;

    internal static void ShortLived(int scale)
    {
        int count = ShortCount * scale;
        for (int i = 0; i < count; i++)
        {
            GC.KeepAlive(AllocateSmallArray());
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static byte[] AllocateSmallArray() => new byte[200];

    internal static void LongLived(List<byte[]> retained, int scale)
    {
        int count = LongCount * scale;
        for (int i = 0; i < count; i++)
        {
            retained.Add(new byte[512]);
        }
    }

    internal static void HeavyLoh(List<byte[]> retained, int scale)
    {
        int count = LohCount * scale;
        for (int i = 0; i < count; i++)
        {
            retained.Add(new byte[100_000]);
        }
    }

    internal static void CollectGen2()
    {
        GC.Collect(2, GCCollectionMode.Forced, blocking: true);
    }
}
