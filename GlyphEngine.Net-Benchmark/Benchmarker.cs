using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;

namespace GlyphEngine.Benchmark
{
    /// <summary>
    /// 测试 Renderer
    /// </summary>
    [Config(typeof(ThroughputConfig))]
    public class Benchmarker
    {
        private Random random;

        private IRenderer nRenderer;
        private IRenderer wRenderer;

        private List<CPixel> pixels;

        [GlobalSetup]
        public void GlobalSetup()
        {
            random = new Random(20260317);

            var ntype = typeof(IRenderer).Assembly.GetType("GlyphEngine.NRenderer");
            nRenderer = Activator.CreateInstance(ntype) as IRenderer;

            var wtype = typeof(IRenderer).Assembly.GetType("GlyphEngine.WRenderer");
            wRenderer = Activator.CreateInstance(wtype) as IRenderer;

            (nRenderer as IFakable).Fake(true);
            (wRenderer as IFakable).Fake(true);

            var w = Console.WindowWidth;
            var h = Console.WindowHeight;
            var n = w * h;
        }

        [IterationSetup]
        public void IterationSetup()
        {
            var w = Console.WindowWidth;
            var h = Console.WindowHeight;
            var n = w * h;

            pixels = new List<CPixel>(n);
            for (var i = 0; i < n; i++)
            {
                var pixel = new CPixel(random.Next(0, w),
                                       random.Next(0, h),
                                       (char)random.Next(32, 127),
                                       (ConsoleColor)random.Next(0, 16),
                                       (ConsoleColor)random.Next(0, 16));
                pixels.Add(pixel);
            }
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public void TestN()
        {
            foreach (var p in pixels)
            {
                nRenderer?.SetPixel(p.X, p.Y, p.Glyph, p.Color, p.BackgroundColor);
            }
            nRenderer?.Render();
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public void TestW()
        {
            foreach (var p in pixels)
            {
                wRenderer?.SetPixel(p.X, p.Y, p.Glyph, p.Color, p.BackgroundColor);
            }
            wRenderer?.Render();
        }

        [IterationCleanup]
        public void IterationCleanup()
        {
            pixels.Clear();
        }
    }

    public class ThroughputConfig : ManualConfig
    {
        public ThroughputConfig()
        {
            // 创建一个全新的Job，不使用DefaultJob
            var job = Job.Default
                         .WithIterationCount(15)
                         .WithInvocationCount(2048)
                         .WithUnrollFactor(1)
                         .WithStrategy(RunStrategy.Throughput);

            AddJob(job);
        }
    }
}
