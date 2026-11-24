using System.Runtime;
using Xunit;

namespace Kt41.Tests;

public class GcWorkloadTests
{
    [Fact]
    public void GcModeMatchesBuildConfiguration()
    {
#if SERVER_GC
        Assert.True(Environment.ProcessorCount > 1, "Для Server GC нужны минимум два логических процессора.");
        Assert.True(GCSettings.IsServerGC);
#else
        Assert.False(GCSettings.IsServerGC);
#endif
    }

    [Fact]
    public void ShortLivedAllocatesSmallArrays()
    {
        long before = GC.GetAllocatedBytesForCurrentThread();

        GcWorkload.ShortLived(1);

        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.True(allocated >= 300_000L * 200);
    }

    [Fact]
    public void LongLivedRetainsFiveThousandArrays()
    {
        var retained = new List<byte[]>();

        GcWorkload.LongLived(retained, 1);
        GcWorkload.CollectGen2();

        Assert.Equal(5_000, retained.Count);
        Assert.All(retained, bytes => Assert.Equal(512, bytes.Length));
    }

    [Fact]
    public void HeavyLohAllocatesLargeArraysInGenerationTwo()
    {
        var retained = new List<byte[]>();

        GcWorkload.HeavyLoh(retained, 1);

        Assert.Equal(20, retained.Count);
        Assert.All(retained, bytes =>
        {
            Assert.Equal(100_000, bytes.Length);
            Assert.Equal(2, GC.GetGeneration(bytes));
        });
    }

    [Fact]
    public void BlockingCollectionIncrementsGen2()
    {
        int before = GC.CollectionCount(2);

        GcWorkload.CollectGen2();

        Assert.True(GC.CollectionCount(2) > before);
    }
}
