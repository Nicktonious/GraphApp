using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SystAnalys_lr1
{

    class DijkstraAlgorithm
    {
        public Dictionary<Vertex, int> ShortestPath(Graph graph, Vertex source)
        {
            // Инициализация кратчайших расстояний до всех вершин в графе
            var distances = new Dictionary<Vertex, int>();
            foreach (var vertex in graph.Vertices)
            {
                distances[vertex] = int.MaxValue;
            }
            distances[source] = 0;

            // Очередь с приоритетом для обработки вершин
            var queue = new SortedSet<(int, Vertex)>();
            queue.Add((0, source));

            while (queue.Count > 0)
            {
                var (distance, currentVertex) = queue.First();
                queue.Remove(queue.First());

                // Просмотр смежных вершин
                foreach (var edge in graph.GetEdgesFromVertex(currentVertex))
                {
                    var neighbor = edge.GetOtherVertex(currentVertex);
                    var totalDistance = distance + edge.Weight;

                    if (totalDistance < distances[neighbor])
                    {
                        // Обновление кратчайшего расстояния и добавление вершины в очередь
                        distances[neighbor] = totalDistance;
                        queue.Add((totalDistance, neighbor));
                    }
                }
            }

            return distances;
        }
    }

    class Graph
    {
        public List<Vertex> Vertices { get; }
        public List<Edge> Edges { get; }

        public Graph()
        {
            Vertices = new List<Vertex>();
            Edges = new List<Edge>();
        }

        public void AddVertex(Vertex vertex)
        {
            Vertices.Add(vertex);
        }

        public void AddEdge(Edge edge)
        {
            Edges.Add(edge);
        }

        public List<Edge> GetEdgesFromVertex(Vertex vertex)
        {
            return Edges.Where(e => e.V1 == vertex || e.V2 == vertex).ToList();
        }
    }


    class Vertex
    {
        public string Name { get; set; }
        public int x, y;
        public Brush Color { get; set; }

        public Vertex(int x, int y, string name, Brush color)
        {
            this.x = x;
            this.y = y;
            this.Name = name;
            this.Color = color;
        }
    }

    class Edge
    {
        public int v1, v2;
        private int weight = 0;
        private static int i = 0;

        public Edge(Vertex v1, Vertex v2, int weight)
        {
            this.V1 = v1;
            this.V2 = v2;
            this.weight = weight;
            this.Name = ((char)(i++ + 'a')).ToString();
        }
        public int Weight
        {
            get => weight;
            set
            {
                weight = value;
            }
        }

        public Vertex GetOtherVertex(Vertex vertex)
        {
            if (vertex == V1)
                return V2;
            else if (vertex == V2)
                return V1;
            else
                throw new ArgumentException("Given vertex is not connected to this edge");
        }
        public readonly string Name;
        public Vertex V1 { get; set; }
        public Vertex V2 { get; set; }
    }

    class DrawGraph
    {
        Bitmap bitmap;
        Pen blackPen;
        Pen redPen;
        Pen darkGoldPen;
        Graphics gr;
        Font fo;
        Brush br;
        PointF point;
        public int R = 20; //радиус окружности вершины

        public DrawGraph(int width, int height)
        {
            bitmap = new Bitmap(width, height);
            gr = Graphics.FromImage(bitmap);
            clearSheet();
            blackPen = new Pen(Color.Black);
            blackPen.Width = 2;
            redPen = new Pen(Color.Red);
            redPen.Width = 2;
            darkGoldPen = new Pen(Color.DarkGoldenrod);
            darkGoldPen.Width = 2;
            fo = new Font("Arial", 15);
            br = Brushes.Black;
        }

        public Bitmap GetBitmap()
        {
            return bitmap;
        }

        public void clearSheet()
        {
            gr.Clear(Color.White);
        }

        public void drawVertex(int x, int y, string number, Brush color)
        {
            gr.FillEllipse(color, (x - R), (y - R), 2 * R, 2 * R);
            gr.DrawEllipse(blackPen, (x - R), (y - R), 2 * R, 2 * R);
            point = new PointF(x - 9, y - 9);
            gr.DrawString(number, fo, br, point);
        }

        public void drawSelectedVertex(int x, int y)
        {
            gr.DrawEllipse(redPen, (x - R), (y - R), 2 * R, 2 * R);
        }

        public void drawEdge(Edge e)
        {
            var v1 = e.V1;
            var v2 = e.V2;
            if (v1 == v2)
            {
                gr.DrawArc(darkGoldPen, (v1.x - 2 * R), (v1.y - 2 * R), 2 * R, 2 * R, 90, 270);
                point = new PointF(v1.x - (int)(2.75 * R), v1.y - (int)(2.75 * R));
                gr.DrawString($"{e.Name}: {e.Weight}", fo, br, point);
                drawVertex(v1.x, v1.y, v1.Name, v1.Color);
            }
            else
            {
                gr.DrawLine(darkGoldPen, v1.x, v1.y, v2.x, v2.y);
                point = new PointF((v1.x + v2.x) / 2, (v1.y + v2.y) / 2);
                gr.DrawString($"{e.Name}: {e.Weight}", fo, br, point);
                drawVertex(v1.x, v1.y, v1.Name, v1.Color);
                drawVertex(v2.x, v2.y, v2.Name, v2.Color);
            }
        }

        public void DrawALLGraph(List<Vertex> V, List<Edge> E)
        {
            //рисуем ребра
            for (int i = 0; i < E.Count; i++)
                foreach (Edge e in E)
                {
                    drawEdge(e);
                }
            //рисуем вершины
            for (int i = 0; i < V.Count; i++)
            {
                drawVertex(V[i].x, V[i].y, V[i].Name, V[i].Color);
            }
        }

        //заполняет матрицу смежности
        public void FillAdjacencyMatrix(int numberV, List<Edge> E, List<Vertex> V, int[,] matrix)
        {
            for (int i = 0; i < numberV; i++)
                for (int j = 0; j < numberV; j++)
                    matrix[i, j] = 0;

            for (int i = 0; i < numberV; i++)
            {
                for (int j = 0; j < numberV; j++)
                {
                    var temp_e = E.Find(e => e.V1 == V[i] && e.V2 == V[j]);
                    if (temp_e != null)
                    {
                        matrix[i, j] = temp_e.Weight;
                        matrix[j, i] = temp_e.Weight;
                    }
                }
            }
        }

        //заполняет матрицу инцидентности
        public void FillIncidenceMatrix(int numberV, List<Edge> E, List<Vertex> V, int[,] matrix)
        {
            for (int i = 0; i < numberV; i++)
                for (int j = 0; j < E.Count; j++)
                    matrix[i, j] = 0;

            for (int i = 0; i < numberV; i++)
            {
                for (int j = 0; j < E.Count; j++)
                {
                    if (E[j].V1 == V[i] || E[j].V2 == V[i]) matrix[i, j] = 1;
                }
            }
        }


    }
}