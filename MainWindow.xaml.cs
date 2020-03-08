using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
//using System.Windows.Shapes;
using Microsoft.Msagl.Drawing;
using Microsoft.Win32;

namespace CobaWPF
{
    public partial class MainWindow : Window
    {
        int INF = Convert.ToInt32(1e9);
        int CurTime;
        string sourceCity;
        List<string> cities;
        Dictionary<string, double> Population;
        Dictionary<string, Dictionary<string, double>> AdjList;
        Tuple<List<Queue<Tuple<string, string>>>, List<Dictionary<string, int>>> BFSMemo;
        Graph graph;

        public MainWindow()
        {
            InitializeComponent();
        }
        private void ViewGraph()
        {
            graph = new Graph();
            foreach (KeyValuePair<string, Dictionary<string, double>> entry1 in AdjList)
            {
                string from = entry1.Key;
                foreach (KeyValuePair<string, double> entry2 in entry1.Value)
                {
                    string to = entry2.Key;
                    graph.AddEdge(from, to);
                }
            }
            foreach (KeyValuePair<string, Dictionary<string, double>> entry1 in AdjList)
            {
                string from = entry1.Key;
                graph.FindNode(from).Attr.Shape = Shape.Circle;
                graph.FindNode(from).Attr.FillColor = Microsoft.Msagl.Drawing.Color.Green;
            }

            graph.Attr.LayerDirection = LayerDirection.LR;
            graph.Attr.BackgroundColor = Microsoft.Msagl.Drawing.Color.Transparent;

            graphControl.Graph = graph;
        }
        private void MapBrowseButton(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "C:\\Users\\nomight\\Desktop\\Kuliah\\Stima\\Tubes2\\Corona\'s Bizarre Adventure";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if ((bool)openFileDialog1.ShowDialog())
            {
                MapFile.Text = openFileDialog1.FileName;
            }

            Parser P = new Parser();
            AdjList = P.ParseMap(this.MapFile.Text);
        }

        private void PopulationBrowseButton(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "C:\\Users\\nomight\\Desktop\\Kuliah\\Stima\\Tubes2\\Corona\'s Bizarre Adventure";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if ((bool)openFileDialog1.ShowDialog())
            {
                PopulationFile.Text = openFileDialog1.FileName;
            }

            Parser P = new Parser();
            Tuple<string, List<string>, Dictionary<string, double>> temp = P.ParsePopulation(this.PopulationFile.Text);
            sourceCity = temp.Item1;
            cities = temp.Item2;
            Population = temp.Item3;
            OutputBox.Text = sourceCity;
        }

        void Animate()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            int i = 0;
            timer.Tick += (s, e) =>
            {
                graphControl.Graph = null;
                if (BFSMemo.Item1[i].Count > 0)
                {
                    Console.WriteLine("CURRENT: " + i + " " + BFSMemo.Item1.Count);
                    Node TempNode = graph.FindNode(BFSMemo.Item1[i].ElementAt(0).Item1);
                    foreach (Edge edge in TempNode.OutEdges.ToList())
                    {
                        if (edge.TargetNode == graph.FindNode(BFSMemo.Item1[i].ElementAt(0).Item2))
                        {
                            graph.RemoveEdge(edge);
                            break;
                        }
                    }

                    if (BFS.spread(BFSMemo.Item1[i].ElementAt(0).Item1, BFSMemo.Item1[i].ElementAt(0).Item2, CurTime))
                    {
                        graph.AddEdge(BFSMemo.Item1[i].ElementAt(0).Item1, BFSMemo.Item1[i].ElementAt(0).Item2).Attr.Color = Microsoft.Msagl.Drawing.Color.Red;
                    }
                    else
                    {
                        graph.AddEdge(BFSMemo.Item1[i].ElementAt(0).Item1, BFSMemo.Item1[i].ElementAt(0).Item2).Attr.Color = Microsoft.Msagl.Drawing.Color.Green;
                    }
                }
                graphControl.Graph = graph;
                OutputBox.Text = "";
                string temp = "";
                for (int j = 0; j < BFSMemo.Item1[i].Count; j++)
                {
                    temp += BFSMemo.Item1[i].ElementAt(j).Item1 + " -> " + BFSMemo.Item1[i].ElementAt(j).Item2 + "\n";
                }
                Console.WriteLine(temp);
                OutputBox.Text = temp;
                foreach (KeyValuePair<string, int> entry in BFSMemo.Item2[i])
                {
                    if (entry.Value != INF)
                    {
                        graph.FindNode(entry.Key).Attr.FillColor = Microsoft.Msagl.Drawing.Color.Red;
                    }
                }
            };
            timer.Tick += (s, e) =>
            {
                if (i == BFSMemo.Item1.Count-1) timer.Stop();
                else i++;
            };
            timer.Start();
        }

        private void SimulateButton(object sender, RoutedEventArgs e)
        {
            CurTime = int.Parse(this.TInput.Text);
            OutputBox.Text = CurTime.ToString();
            ViewGraph();
            BFSMemo = BFS.RunBFS(CurTime, sourceCity, cities, Population, AdjList);
            Animate();
        }
    }

    public class Parser
    {
        public Dictionary<string, Dictionary<string, double>> ParseMap(string filename)
        {
            Dictionary<string, Dictionary<string, double>> AdjList = new Dictionary<string, Dictionary<string, double>>();
            try
            {
                string[] lines = System.IO.File.ReadAllLines(filename);
                int n = int.Parse(lines[0]);
                for (int i = 1; i <= n; i++)
                {
                    var p = lines[i].Split();
                    string from = p[0], to = p[1];
                    double tr = Convert.ToDouble(p[2], CultureInfo.InvariantCulture);
                    if (!AdjList.ContainsKey(from)) AdjList[from] = new Dictionary<string, double>();
                    AdjList[from][to] = tr;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"The file could not be read: {e.Message}");
                return null;
            }
            return AdjList;
        }

        public Tuple<string, List<string>, Dictionary<string, double>> ParsePopulation(string filename)
        {
            string sourceCity;
            List<string> cities = new List<string>();
            Dictionary<string, double> Population = new Dictionary<string, double>();
            try
            {
                string[] lines = System.IO.File.ReadAllLines(filename);
                var firstLine = lines[0].Split();
                int n = int.Parse(firstLine[0]);
                sourceCity = firstLine[1];
                for (int i = 1; i <= n; i++)
                {
                    var p = lines[i].Split();
                    string city = p[0];
                    int pops = int.Parse(p[1]);
                    Population[city] = pops;
                    cities.Add(city);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"The file could not be read: {e.Message}");
                return null;
            }
            return Tuple.Create(sourceCity, cities, Population);
        }
    }
}
