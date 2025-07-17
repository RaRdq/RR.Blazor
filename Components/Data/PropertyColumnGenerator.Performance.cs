using System.Diagnostics;
using System.Reflection;

namespace RR.Blazor.Components.Data;

/// <summary>
/// Performance testing utilities for PropertyColumnGenerator
/// </summary>
public static class PropertyColumnGeneratorPerformance
{
    /// <summary>
    /// Test model for performance benchmarks
    /// </summary>
    public class TestModel
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public decimal Amount { get; set; }
        public bool IsActive { get; set; }
        public int Count { get; set; }
        public TestEnum Status { get; set; }
    }

    public enum TestEnum
    {
        None = 0,
        Active = 1,
        Inactive = 2,
        Pending = 3
    }

    /// <summary>
    /// Compare performance between original (simulated) and optimized approaches
    /// </summary>
    public static (long OptimizedMs, long SimulatedOriginalMs, double ImprovementFactor) BenchmarkPerformance(int iterations = 1000)
    {
        // Warm up
        PropertyColumnGenerator.GenerateColumns<TestModel>();
        
        // Test optimized approach (with caching)
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            PropertyColumnGenerator.GenerateColumns<TestModel>();
        }
        sw.Stop();
        var optimizedMs = sw.ElapsedMilliseconds;
        
        // Clear cache and test first call (simulates original approach)
        PropertyColumnGenerator.ClearCache<TestModel>();
        
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            // Clear cache each time to simulate original behavior
            PropertyColumnGenerator.ClearCache<TestModel>();
            PropertyColumnGenerator.GenerateColumns<TestModel>();
        }
        sw.Stop();
        var simulatedOriginalMs = sw.ElapsedMilliseconds;
        
        var improvementFactor = simulatedOriginalMs > 0 ? (double)simulatedOriginalMs / optimizedMs : 0;
        
        return (optimizedMs, simulatedOriginalMs, improvementFactor);
    }

    /// <summary>
    /// Test cache hit rates
    /// </summary>
    public static (int CacheHits, int CacheMisses) TestCacheEffectiveness(int iterations = 100)
    {
        PropertyColumnGenerator.ClearAllCaches();
        
        int cacheHits = 0;
        int cacheMisses = 0;
        
        for (int i = 0; i < iterations; i++)
        {
            var (columnCount, accessorCount, formatterCount) = PropertyColumnGenerator.GetCacheStats();
            bool hadCache = columnCount > 0;
            
            PropertyColumnGenerator.GenerateColumns<TestModel>();
            
            if (hadCache)
                cacheHits++;
            else
                cacheMisses++;
        }
        
        return (cacheHits, cacheMisses);
    }

    /// <summary>
    /// Test memory usage of cached vs uncached approach
    /// </summary>
    public static (long CachedMemory, long UncachedMemory) BenchmarkMemoryUsage(int iterations = 1000)
    {
        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        // Test cached approach
        var initialMemory = GC.GetTotalMemory(false);
        for (int i = 0; i < iterations; i++)
        {
            PropertyColumnGenerator.GenerateColumns<TestModel>();
        }
        var cachedMemory = GC.GetTotalMemory(false) - initialMemory;
        
        // Clear caches and test uncached approach
        PropertyColumnGenerator.ClearAllCaches();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        initialMemory = GC.GetTotalMemory(false);
        for (int i = 0; i < iterations; i++)
        {
            PropertyColumnGenerator.ClearCache<TestModel>();
            PropertyColumnGenerator.GenerateColumns<TestModel>();
        }
        var uncachedMemory = GC.GetTotalMemory(false) - initialMemory;
        
        return (cachedMemory, uncachedMemory);
    }

    /// <summary>
    /// Run comprehensive performance test suite
    /// </summary>
    public static string RunPerformanceTestSuite()
    {
        var results = new System.Text.StringBuilder();
        results.AppendLine("PropertyColumnGenerator Performance Test Results");
        results.AppendLine("=============================================");
        results.AppendLine();

        // Performance benchmark
        var (optimizedMs, originalMs, improvement) = BenchmarkPerformance(1000);
        results.AppendLine($"Performance Benchmark (1000 iterations):");
        results.AppendLine($"  Optimized (cached): {optimizedMs}ms");
        results.AppendLine($"  Simulated Original: {originalMs}ms");
        results.AppendLine($"  Improvement Factor: {improvement:F2}x faster");
        results.AppendLine();

        // Cache effectiveness
        var (hits, misses) = TestCacheEffectiveness(100);
        var hitRate = hits > 0 ? (double)hits / (hits + misses) * 100 : 0;
        results.AppendLine($"Cache Effectiveness (100 iterations):");
        results.AppendLine($"  Cache Hits: {hits}");
        results.AppendLine($"  Cache Misses: {misses}");
        results.AppendLine($"  Hit Rate: {hitRate:F1}%");
        results.AppendLine();

        // Memory usage
        var (cachedMem, uncachedMem) = BenchmarkMemoryUsage(500);
        var memoryReduction = uncachedMem > 0 ? (1.0 - (double)cachedMem / uncachedMem) * 100 : 0;
        results.AppendLine($"Memory Usage (500 iterations):");
        results.AppendLine($"  Cached Approach: {cachedMem:N0} bytes");
        results.AppendLine($"  Uncached Approach: {uncachedMem:N0} bytes");
        results.AppendLine($"  Memory Reduction: {memoryReduction:F1}%");
        results.AppendLine();

        // Cache statistics
        var (colCache, accCache, fmtCache) = PropertyColumnGenerator.GetCacheStats();
        results.AppendLine($"Final Cache Statistics:");
        results.AppendLine($"  Column Metadata Cache: {colCache} entries");
        results.AppendLine($"  Property Accessor Cache: {accCache} entries");
        results.AppendLine($"  Formatter Cache: {fmtCache} entries");

        return results.ToString();
    }
}