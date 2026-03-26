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

            nRenderer = InstantiateRenderer("GlyphEngine.NRenderer");
            wRenderer = InstantiateRenderer("GlyphEngine.WRenderer");
        }

        private IRenderer InstantiateRenderer(string name)
        {
            var type = typeof(IRenderer).Assembly.GetType(name);
            var renderer = Activator.CreateInstance(type) as IRenderer;

            var faker = renderer as IFakable;
            faker?.Fake(true);

            return renderer;
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

        [Benchmark]
        public void TestN()
        {
            foreach (var p in pixels)
            {
                nRenderer?.SetPixel(p.X, p.Y, p.Glyph, p.Color, p.BackgroundColor);
            }
            nRenderer?.Render();
        }

        [Benchmark]
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
