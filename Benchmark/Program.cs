using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using ConcurrentLab6;

namespace Benchmark
{
    public class GraphBenchmark
    {
        [Params(50, 75, 100, 125, 150, 175, 200)]
        public int nodes;
        GraphPrim graphPrim;
        public GraphBenchmark()
        {
            graphPrim = new GraphPrim();
        }

        [Benchmark(Description = "Serial Simple Prime")]
        public void TestSimpleSerial()
        {
            graphPrim.graph = GraphPrim.generate_graph(nodes, nodes / 2);
            graphPrim.reset();
            graphPrim.serial_prime_alg();
        }

        [Benchmark(Description = "Parallel Simple Prime")]
        public void TestSimpleParallel()
        {
            graphPrim.graph = GraphPrim.generate_graph(nodes, nodes / 2);
            graphPrim.reset();
            graphPrim.parallel_prime_alg();
        }

        [Benchmark(Description = "Serial FullJoin Prime")]
        public void TestFullJoinSerial()
        {
            graphPrim.graph = GraphPrim.generate_graph(nodes, 20, true);
            graphPrim.reset();
            graphPrim.serial_prime_alg();
        }

        [Benchmark(Description = "Parallel FullJoin Prime")]
        public void TestFullJoinParallel()
        {
            graphPrim.graph = GraphPrim.generate_graph(nodes, 20, true);
            graphPrim.reset();
            graphPrim.parallel_prime_alg();
        }

        [Benchmark(Description = "Serial RareJoin Prime")]
        public void TestRareJoinSerial()
        {
            graphPrim.graph = GraphPrim.generate_graph(nodes, nodes / 10);
            graphPrim.reset();
            graphPrim.serial_prime_alg();
        }

        [Benchmark(Description = "Parallel RareJoin Prime")]
        public void TestRareJoinParallel()
        {
            graphPrim.graph = GraphPrim.generate_graph(nodes, nodes / 10);
            graphPrim.reset();
            graphPrim.parallel_prime_alg();
        }
    }

    class Program
    {
        
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<GraphBenchmark>();
        }
    }
}
