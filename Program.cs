using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;

namespace Corona_s_Bizarre_Adventure
{
    class Program
    {
        static int INF;
        static Dictionary<string, Dictionary<string, double>> adj;
        static Queue<Tuple<string, string>> q;
        static Dictionary<string, int> population;
        static Dictionary<string, int> T;
        static List<string> cities;
        static string sourceCity;

        static void Main(string[] args)
        {
            init();
            parsePopulation();
            parseMap();
            foreach (KeyValuePair<string, Dictionary<string, double>> entry in adj)
            {
                Console.WriteLine(entry.Key);
                foreach (KeyValuePair <string, double> item in entry.Value)
                {
                    Console.WriteLine(item.Key + " " + item.Value);
                }
            }
            while (true)
            {
                Console.WriteLine("Masukkin waktunya euy");
                int curTime = Convert.ToInt32(Console.ReadLine());
                reset();
                foreach (KeyValuePair<string, double> item in adj[sourceCity])
                {
                    q.Enqueue(Tuple.Create(sourceCity, item.Key));
                }

                foreach (Tuple<string, string> edges in q)
                {
                    Console.Write(edges.Item1 + " -> " + edges.Item2 + ", ");
                }
                Console.WriteLine();
                foreach (KeyValuePair<string, int> enumT in T)
                {
                    Console.WriteLine("T(" + enumT.Key + "): " + enumT.Value);
                }
                while (q.Count != 0)
                {
                    Tuple<string, string> cur = q.Peek();
                    q.Dequeue();
                    Console.WriteLine("SPREAD: " + spread(cur.Item1, cur.Item2, curTime));
                    if (spread(cur.Item1, cur.Item2, curTime))
                    {
                        int t = findt(cur.Item1, cur.Item2);
                        int expectedT = T[cur.Item1] + t;
                        if (expectedT <= T[cur.Item2])
                        {
                            T[cur.Item2] = expectedT;
                            foreach (KeyValuePair<string, double> edge in adj[cur.Item2])
                            {
                                q.Enqueue(Tuple.Create(cur.Item2, edge.Key));
                            }
                        }
                    }
                    foreach (Tuple<string, string> edges in q)
                    {
                        Console.Write(edges.Item1 + " -> " + edges.Item2 + ", ");
                    }
                    Console.WriteLine();
                    foreach (KeyValuePair<string, int> enumT in T)
                    {
                        Console.WriteLine("T(" + enumT.Key + "): " + enumT.Value);
                    }
                }
            }
            Console.ReadKey();
        }

        static int findt(string A, string B)
        {
            double tmp;
            tmp = -4 * Math.Log((Convert.ToDouble(population[A]) * adj[A][B] - 1) / (Convert.ToDouble(population[A] - 1)));
            int ret = Convert.ToInt32(Math.Ceiling(tmp));
            return ret;
        }
        static void init()
        {
            adj = new Dictionary<string, Dictionary<string, double>>();
            q = new Queue<Tuple<string, string>>();
            population = new Dictionary<string, int>();
            cities = new List<string>();
            T = new Dictionary<string, int>();
            INF = Convert.ToInt32(1e9);
        }

        static void parseMap()
        {
            string[] lines = System.IO.File.ReadAllLines(@"../../map.txt");
            int n = int.Parse(lines[0]);
            for (int i = 1; i <= n; i++)
            {
                var p = lines[i].Split();
                string from = p[0], to = p[1];
                double tr = Convert.ToDouble(p[2], CultureInfo.InvariantCulture);
                adj[from][to] = tr;
            }
        }

        static void parsePopulation()
        {
            string[] lines = System.IO.File.ReadAllLines(@"../../pop.txt");
            var firstLine = lines[0].Split();
            int n = int.Parse(firstLine[0]);
            sourceCity = firstLine[1];
            for (int i = 1; i <= n; i++)
            {
                var p = lines[i].Split();
                string city = p[0];
                int pops = int.Parse(p[1]);
                population[city] = pops;
                cities.Add(city);
                adj[city] = new Dictionary<string, double>();
                T[city] = INF;
            }
            T[sourceCity] = 0;
        }

        static double calcI(string A, int tA)
        {
            double nume = population[A];
            double denom = 1 + (population[A] - 1) * Math.Exp(-0.25 * tA);
            double ret = nume / denom;
            return ret;
        }
           

        static bool spread(string A, string B, int curTime)
        {
            double res = calcI(A, curTime - T[A]) * adj[A][B];
            return (res > 1);
        }

        static void reset()
        {
            for (int i = 0; i < cities.Count; i++)
            {
                T[cities[i]] = INF;
            }
            T[sourceCity] = 0;
        }
    }
}
