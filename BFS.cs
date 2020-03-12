using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CobaWPF
{
	class BFS
	{
        static int INF = Convert.ToInt32(1e9);
        static Dictionary<string, int> T;
        static int CurTime;
        static string sourceCity;
        static List<string> cities;
        static Dictionary<string, double> Population;
        static Dictionary<string, Dictionary<string, double>> AdjList;
        static Dictionary<string, Dictionary<string, int>> Color;
        static Queue<Tuple<string, string>> q;

        public static Tuple<List<Queue<Tuple<string, string>>>, List<Dictionary<string, int>>, List<Dictionary<string, Dictionary<string, int>>>> RunBFS(int _CurTime, string _sourceCity, List<string> _cities, Dictionary<string, double> _Population, Dictionary<string, Dictionary<string, double>> _AdjList)
        {
            CurTime = _CurTime;
            sourceCity = _sourceCity;
            cities = _cities;
            Population = _Population;
            AdjList = _AdjList;
            q = new Queue<Tuple<string, string>>();
            T = new Dictionary<string, int>();
            Color = new Dictionary<string, Dictionary<string, int>>();
            for (int i = 0; i < cities.Count; i++)
            {
                T[cities[i]] = INF;
                if (!AdjList.ContainsKey(cities[i]))
                {
                    AdjList[cities[i]] = new Dictionary<string, double>();
                }
            }
            T[sourceCity] = 0;

            foreach (KeyValuePair<string, double> item in AdjList[sourceCity])
            {
                q.Enqueue(Tuple.Create(sourceCity, item.Key));
            }

            foreach (KeyValuePair<string, Dictionary<string, double>> item in AdjList)
            {
                Color[item.Key] = new Dictionary<string, int>();
                foreach(KeyValuePair<string, double> item2 in AdjList[item.Key])
                {
                    Color[item.Key][item2.Key] = 0;
                }
            }

            List<Queue<Tuple<string, string>>> ListQ = new List<Queue<Tuple<string, string>>>();
            List<Dictionary<string, int>> ListT = new List<Dictionary<string, int>>();
            List<Dictionary<string, Dictionary<string, int>>> ListColor = new List<Dictionary<string, Dictionary<string, int>>>();
            ListQ.Add(new Queue<Tuple<string, string>>(q));
            ListT.Add(new Dictionary<string, int>(T));
            ListColor.Add(Clone(Color));
            while (q.Count != 0)
            {
                Tuple<string, string> cur = q.Peek();
                q.Dequeue();
                if (spread(cur.Item1, cur.Item2, CurTime))
                {
                    Color[cur.Item1][cur.Item2] = -1;
                    int t = findt(cur.Item1, cur.Item2);
                    int expectedT = T[cur.Item1] + t;
                    if (expectedT <= T[cur.Item2])
                    {
                        T[cur.Item2] = expectedT;
                        //Console.WriteLine(cur.Item2 + " " + AdjList[cur.Item2].Count);
                        foreach (KeyValuePair<string, double> edge in AdjList[cur.Item2])
                        {
                            q.Enqueue(Tuple.Create(cur.Item2, edge.Key));
                        }
                    }
                } else
                {
                    Color[cur.Item1][cur.Item2] = 1;
                }
                ListQ.Add(new Queue<Tuple<string, string>>(q)); 
                ListT.Add(new Dictionary<string, int>(T));
                ListColor.Add(Clone(Color));
            }
            return Tuple.Create(ListQ, ListT, ListColor);
        }

        static int findt(string A, string B)
        {
            double tmp;
            tmp = -4 * Math.Log((Convert.ToDouble(Population[A]) * AdjList[A][B] - 1) / (Convert.ToDouble(Population[A] - 1)));
            int ret = Convert.ToInt32(Math.Ceiling(tmp));
            return ret;
        }
        
        static double calcI(string A, int tA)
        {
            double nume = Population[A];
            double denom = 1 + (Population[A] - 1) * Math.Exp(-0.25 * tA);
            double ret = nume / denom;
            return ret;
        }


        static public bool spread(string A, string B, int curTime)
        {
            double res = calcI(A, curTime - T[A]) * AdjList[A][B];
            return (res > 1);
        }

        static Dictionary<string, Dictionary<string, int>> Clone(Dictionary<string, Dictionary<string, int>> Color)
        {
            Dictionary<string, Dictionary<string, int>> NewColor = new Dictionary<string, Dictionary<string, int>>();
            foreach (KeyValuePair<string, Dictionary<string, int>> item in Color)
            {
                NewColor[item.Key] = new Dictionary<string, int>();
                foreach (KeyValuePair<string, int> item2 in item.Value)
                {
                    NewColor[item.Key][item2.Key] = item2.Value;
                }
            }
            return NewColor;
        }
    }
}

