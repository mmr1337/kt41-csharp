using System.Diagnostics;
using System.Globalization;
using System.Runtime;
using Kt41;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

int scale = args.Contains("--stress") ? 100 : 1;
var retained = new List<byte[]>(GcWorkload.LongCount * scale);
var largeObjects = new List<byte[]>(GcWorkload.LohCount * scale);

var results = new[]
{
    Measure("ShortLived", () => GcWorkload.ShortLived(scale)),
    Measure("LongLived", () => GcWorkload.LongLived(retained, scale)),
    Measure("HeavyLoh", () => GcWorkload.HeavyLoh(largeObjects, scale)),
    Measure("Forced Gen2", GcWorkload.CollectGen2)
};

GC.KeepAlive(retained);
GC.KeepAlive(largeObjects);

Console.WriteLine($"Server GC: {GCSettings.IsServerGC} | .NET {Environment.Version} | Load x{scale}");
Console.WriteLine($"{"Scenario",-19} {"ms",9} {"Gen0",6} {"Gen1",6} {"Gen2",6}");

foreach (var (name, elapsed, collections) in results)
{
    Console.WriteLine($"{name,-19} {elapsed,9:F3} {collections.Gen0,6} {collections.Gen1,6} {collections.Gen2,6}");
}

static Measurement Measure(string name, Action scenario)
{
    var before = GcCounts.Read();

    long started = Stopwatch.GetTimestamp();
    scenario();
    double elapsed = Stopwatch.GetElapsedTime(started).TotalMilliseconds;

    return new Measurement(name, elapsed, GcCounts.Read().DifferenceFrom(before));
}

readonly record struct Measurement(string Name, double Elapsed, GcCounts Collections);

readonly record struct GcCounts(int Gen0, int Gen1, int Gen2)
{
    public static GcCounts Read() =>
        new(GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));

    public GcCounts DifferenceFrom(GcCounts before) =>
        new(Gen0 - before.Gen0, Gen1 - before.Gen1, Gen2 - before.Gen2);
}
