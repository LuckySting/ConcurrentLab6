using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics;

namespace ConcurrentLab6
{
    public struct Edge
    {
        public int vert_a;
        public int vert_b;
        public int weight;
        public Edge(int vert_a, int vert_b, int weight)
        {
            this.vert_a = vert_a;
            this.vert_b = vert_b;
            this.weight = weight;
        }
    }

    public class GraphPrim
    {
        public List<Edge>[] graph;
        public bool[] ostov;
        public ConcurrentQueue<Edge> threads_output;

        public void reset()
        {
            threads_output = new ConcurrentQueue<Edge>();
            ostov = new bool[graph.Length];
            Array.Fill<bool>(ostov, false);
        }

        public static List<Edge>[] generate_graph(int nodes, int max_edges_per_node, bool full_join = false)
        {
            var g = new List<Edge>[nodes];
            for (int i = 0; i < g.Length; i++)
            {
                g[i] = new List<Edge>();
            }

            Random rnd = new Random();
            if (full_join)
            {
                for (int i = 0; i < nodes; i++)
                {
                    for (int j = 0; j < nodes; j++)
                    {
                        if (i != j)
                        {
                            int weight = rnd.Next(1, 50);
                            g[i].Add(new Edge(i, j, weight));
                            g[j].Add(new Edge(j, i, weight));
                        }
                    }
                }
            } else
            {
                for (int i = 0; i < nodes; i++)
                {
                    for (int e = 0; e < max_edges_per_node; e++)
                    {
                        int j = rnd.Next(0, nodes);
                        if (i != j)
                        {
                            int weight = rnd.Next(1, 50);
                            g[i].Add(new Edge(i, j, weight));
                            g[j].Add(new Edge(j, i, weight));
                        }
                    }
                }
            }
            return g;
        }

        public void find_min(object o)
        {
            int target_vert = (int)o;
            int min_weight = -1;
            Edge to_ans = new Edge(-1, -1, -1);
            foreach (List<Edge> vert in graph)
            {
                foreach (Edge edge in vert)
                {
                    if (((ostov[edge.vert_a] && edge.vert_b == target_vert) || (edge.vert_a == target_vert && ostov[edge.vert_b])) && (min_weight == -1 || edge.weight < min_weight))
                    {
                        to_ans = edge;
                        min_weight = edge.weight;
                    }
                }
            }
            if (to_ans.vert_a != -1)
            {
                threads_output.Enqueue(to_ans);
            }
        }

        public List<Edge> serial_prime_alg()
        {
            List<Edge> ans = new List<Edge>();
            ostov[graph.Length - 1] = true;
            while (true)
            {
                List<int> todo = new List<int>();
                for (int i = 0; i < ostov.Length; i++)
                {
                    if (!ostov[i])
                    {
                        todo.Add(i);
                    }
                }
                if (todo.Count == 0)
                {
                    break;
                }

                foreach (int t_v in todo)
                {
                    find_min(t_v);
                }

                if (threads_output.Count > 0)
                {
                    Edge to_ans = new Edge(-1, -1, -1);
                    int min_weight = -1;
                    while (threads_output.Count > 0)
                    {
                        Edge maybe;
                        threads_output.TryDequeue(out maybe);
                        if (min_weight == -1 || maybe.weight < min_weight)
                        {
                            to_ans = maybe;
                            min_weight = to_ans.weight;
                        }
                    }
                    if (to_ans.vert_a != -1)
                    {
                        ans.Add(to_ans);
                        int new_vert = ostov[to_ans.vert_a] ? to_ans.vert_b : to_ans.vert_a;
                        ostov[new_vert] = true;
                    }
                }
            }
            return ans;
        }

        public List<Edge> parallel_prime_alg()
        {
            List<Edge> ans = new List<Edge>();
            ostov[graph.Length - 1] = true;
            while (true)
            {
                List<int> todo = new List<int>();
                for (int i = 0; i < ostov.Length; i++)
                {
                    if (!ostov[i])
                    {
                        todo.Add(i);
                    }
                }
                if (todo.Count == 0)
                {
                    break;
                }
                int waiter = todo.Count;
                foreach(int t_v in todo)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(x => {
                        find_min(x);
                        Interlocked.Decrement(ref waiter);
                    }), t_v);
                }
                while (waiter > 0) ;

                if (threads_output.Count > 0)
                {
                    Edge to_ans = new Edge(-1, -1, -1);
                    int min_weight = -1;
                    while (threads_output.Count > 0)
                    {
                        Edge maybe;
                        threads_output.TryDequeue(out maybe);
                        if (min_weight == -1 || maybe.weight < min_weight)
                        {
                            to_ans = maybe;
                            min_weight = to_ans.weight;
                        }
                    }
                    if (to_ans.vert_a != -1)
                    {
                        ans.Add(to_ans);
                        int new_vert = ostov[to_ans.vert_a] ? to_ans.vert_b : to_ans.vert_a;
                        ostov[new_vert] = true;
                    }
                }
            }
            return ans;
        }

        public int calcWeight(List<Edge> edges)
        {
            int weight = 0;
            foreach(var edge in edges)
            {
                weight += edge.weight;
            }
            return weight;
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            var g = GraphPrim.generate_graph(5, 5);
            GraphPrim graph = new GraphPrim();
            graph.graph = g;
            graph.reset();
            var ans = graph.parallel_prime_alg();
        }
    }
}
