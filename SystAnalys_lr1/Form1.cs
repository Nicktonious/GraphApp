using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace SystAnalys_lr1
{
    public partial class Form1 : Form
    {
        int CountV = 1;
        DrawGraph G;
        List<Vertex> V;
        List<Edge> E;
        int[,] AMatrix; //матрица смежности
        int[,] IMatrix; //матрица инцидентности

        private List<Vertex> selection = new List<Vertex>();
        private bool onSelectPair = false;
        Vertex Selected1
        {
            get
            {
                if (selection.Count > 0) return selection[0];
                return null;
            }
        }
        Vertex Selected2
        {
            get
            {
                if (selection.Count == 2 && onSelectPair) return selection[1];
                return null;
            }
        }
        public Brush Color
        {
            get
            {
                var colors = new Dictionary<string, Brush>()
                {
                    { "Белый", Brushes.White },
                    { "Синий", Brushes.Blue },
                    { "Красный", Brushes.Red },
                    { "Зеленый", Brushes.Green }
                };
                var color = lb_colors?.SelectedItem?.ToString() ?? "";
                if (colors.ContainsKey(color)) return colors[color];
                return Brushes.White;
            }
        }

        public bool ColorFlag = false;

        public Form1()
        {
            InitializeComponent();
            V = new List<Vertex>();
            G = new DrawGraph(sheet.Width, sheet.Height);
            E = new List<Edge>();
            sheet.Image = G.GetBitmap();
        }

        //кнопка - выбрать вершину
        private void selectButton_Click(object sender, EventArgs e)
        {
            selectButton.Enabled = false;
            drawVertexButton.Enabled = true;
            drawEdgeButton.Enabled = true;
            deleteButton.Enabled = true;
            G.clearSheet();
            G.DrawALLGraph(V, E);
            sheet.Image = G.GetBitmap();
            //selected1 = -1;
            onSelectPair = false;
        }

        //кнопка - рисовать вершину
        private void drawVertexButton_Click(object sender, EventArgs e)
        {
            drawVertexButton.Enabled = false;
            selectButton.Enabled = true;
            drawEdgeButton.Enabled = true;
            deleteButton.Enabled = true;
            G.clearSheet();
            G.DrawALLGraph(V, E);
            sheet.Image = G.GetBitmap();
        }

        //кнопка - рисовать ребро
        private void drawEdgeButton_Click(object sender, EventArgs e)
        {
            drawEdgeButton.Enabled = false;
            selectButton.Enabled = true;
            drawVertexButton.Enabled = true;
            deleteButton.Enabled = true;
            G.clearSheet();
            G.DrawALLGraph(V, E);
            sheet.Image = G.GetBitmap();

            onSelectPair = true;
        }

        //кнопка - удалить элемент
        private void deleteButton_Click(object sender, EventArgs e)
        {
            deleteButton.Enabled = false;
            selectButton.Enabled = true;
            drawVertexButton.Enabled = true;
            drawEdgeButton.Enabled = true;
            onSelectPair = false;
            G.clearSheet();
            G.DrawALLGraph(V, E);
            sheet.Image = G.GetBitmap();
        }

        //кнопка - удалить граф
        private void deleteALLButton_Click(object sender, EventArgs e)
        {
            selectButton.Enabled = true;
            drawVertexButton.Enabled = true;
            drawEdgeButton.Enabled = true;
            deleteButton.Enabled = true;
            const string message = "Вы действительно хотите полностью удалить граф?";
            const string caption = "Удаление";
            var MBSave = MessageBox.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (MBSave == DialogResult.Yes)
            {
                V.Clear();
                E.Clear();
                G.clearSheet();
                sheet.Image = G.GetBitmap();
            }
        }

        //кнопка - матрица смежности
        private void buttonAdj_Click(object sender, EventArgs e)
        {
            createAdjAndOut();
        }

        //кнопка - матрица инцидентности 
        private void buttonInc_Click(object sender, EventArgs e)
        {
            createIncAndOut();
        }

        private void sheet_MouseClick(object sender, MouseEventArgs e)
        {
            Select(e.X, e.Y);
            //нажата кнопка "выбрать вершину", ищем степень вершины
            if (selectButton.Enabled == false)
            {
                if (Selected1 != null)
                {
                    Selected1.Color = Color;
                    ClearSelection();
                }
                var selected_v = GetSelectedEdge(e.X, e.Y) ?? GetSelectedLoop(e.X, e.Y);
                if (selected_v != null)
                {
                    var w = ShowMyDialogBox();
                    selected_v.Weight = w;
                }
                G.clearSheet();
                G.DrawALLGraph(V, E);
                sheet.Image = G.GetBitmap();
                return;

            }
            //нажата кнопка "рисовать вершину"
            if (drawVertexButton.Enabled == false)
            {
                DrawVertex(e.X, e.Y, (CountV++).ToString(), Color);
            }
            //нажата кнопка "рисовать ребро"
            if (drawEdgeButton.Enabled == false)
            {
                //Select(e.X, e.Y);
                if (e.Button == MouseButtons.Left)
                {
                    //var selected_v = V.Find(v => Math.Pow((v.x - e.X), 2) + Math.Pow((v.y - e.Y), 2) <= G.R * G.R);
                    if (Selected1 != null && Selected2 != null)
                        DrawEdge(Selected1, Selected2);
                }

                if (e.Button == MouseButtons.Right)
                {
                    ClearSelection();
                }
            }

            //нажата кнопка "удалить элемент"
            if (deleteButton.Enabled == false)
            {
                bool flag = false; //удалили ли что-нибудь по ЭТОМУ клику
                //ищем, возможно была нажата вершина

                if (Selected1 != null)
                {
                    var v_count = V.Count;
                    RemoveVertex(Selected1);
                    ClearSelection();
                    if (v_count != V.Count) flag = true;
                }
                //ищем, возможно было нажато ребро
                else
                {
                    Edge selected_e = GetSelectedLoop(e.X, e.Y) ?? GetSelectedEdge(e.X, e.Y);
                    if (selected_e != null)
                    {
                        E.Remove(selected_e);
                        flag = true;
                    }

                }
                //если что-то было удалено, то обновляем граф на экране
                if (flag)
                {
                    G.clearSheet();
                    G.DrawALLGraph(V, E);
                    sheet.Image = G.GetBitmap();
                }
            }
        }

        //создание матрицы смежности и вывод в листбокс
        private void createAdjAndOut()
        {
            AMatrix = new int[V.Count, V.Count];
            G.FillAdjacencyMatrix(V.Count, E, V, AMatrix);
            listBoxMatrix.Items.Clear();
            string sOut = "    ";
            for (int i = 0; i < V.Count; i++)
            {
                sOut += V[i].Name + "  ";
            }
            listBoxMatrix.Items.Add(sOut);
            for (int i = 0; i < V.Count; i++)
            {
                sOut = V[i].Name + " | ";
                for (int j = 0; j < V.Count; j++)
                {
                    string t = (AMatrix[i, j]).ToString();
                    if (t.Length < 2) sOut += t + "  ";
                    else sOut += t + " ";
                }
                listBoxMatrix.Items.Add(sOut);
            }
        }

        //создание матрицы инцидентности и вывод в листбокс
        private void createIncAndOut()
        {
            if (E.Count > 0)
            {
                IMatrix = new int[V.Count, E.Count];
                //var e_names = E.Select(e => e.Name).ToArray();
                //var v_names = V.Select(v => v.Name).ToArray(); 
                G.FillIncidenceMatrix(V.Count, E, V, IMatrix);
                listBoxMatrix.Items.Clear();
                string sOut = "    ";
                for (int i = 0; i < E.Count; i++)
                    sOut += E[i].Name + " ";
                listBoxMatrix.Items.Add(sOut);
                for (int i = 0; i < V.Count; i++)
                {
                    sOut = (V[i].Name) + " | ";
                    for (int j = 0; j < E.Count; j++)
                        sOut += IMatrix[i, j] + " ";
                    listBoxMatrix.Items.Add(sOut);
                }
            }
            else
                listBoxMatrix.Items.Clear();
        }

        //поиск элементарных цепей
        private void chainButton_Click(object sender, EventArgs e)
        {
            listBoxMatrix.Items.Clear();
            //1-white 2-black
            int[] color = new int[V.Count];
            for (int i = 0; i < V.Count - 1; i++)
            {
                for (int j = i + 1; j < V.Count; j++)
                {
                    for (int k = 0; k < V.Count; k++)
                        color[k] = 1;
                    DFSchain(i, j, E, color, (i + 1).ToString());
                }
            }
        }

        //обход в глубину. поиск элементарных цепей. (1-white 2-black)
        private void DFSchain(int u, int endV, List<Edge> E, int[] color, string s)
        {
            //вершину не следует перекрашивать, если u == endV (возможно в нее есть несколько путей)
            if (u != endV)
                color[u] = 2;
            else
            {
                listBoxMatrix.Items.Add(s);
                return;
            }
            for (int w = 0; w < E.Count; w++)
            {
                if (color[E[w].v2] == 1 && E[w].v1 == u)
                {
                    DFSchain(E[w].v2, endV, E, color, s + "-" + (E[w].v2 + 1).ToString());
                    color[E[w].v2] = 1;
                }
                else if (color[E[w].v1] == 1 && E[w].v2 == u)
                {
                    DFSchain(E[w].v1, endV, E, color, s + "-" + (E[w].v1 + 1).ToString());
                    color[E[w].v1] = 1;
                }
            }
        }

        //поиск элементарных циклов
        private void cycleButton_Click(object sender, EventArgs e)
        {
            listBoxMatrix.Items.Clear();
            //1-white 2-black
            int[] color = new int[V.Count];
            for (int i = 0; i < V.Count; i++)
            {
                for (int k = 0; k < V.Count; k++)
                    color[k] = 1;
                List<int> cycle = new List<int>();
                cycle.Add(i + 1);
                DFScycle(i, i, E, color, -1, cycle);
            }
        }

        //обход в глубину. поиск элементарных циклов. (1-white 2-black)
        //Вершину, для которой ищем цикл, перекрашивать в черный не будем. Поэтому, для избежания неправильной
        //работы программы, введем переменную unavailableEdge, в которой будет хранится номер ребра, исключаемый
        //из рассмотрения при обходе графа. В действительности это необходимо только на первом уровне рекурсии,
        //чтобы избежать вывода некорректных циклов вида: 1-2-1, при наличии, например, всего двух вершин.

        private void DFScycle(int u, int endV, List<Edge> E, int[] color, int unavailableEdge, List<int> cycle)
        {
            //если u == endV, то эту вершину перекрашивать не нужно, иначе мы в нее не вернемся, а вернуться необходимо
            if (u != endV)
                color[u] = 2;
            else
            {
                if (cycle.Count >= 2)
                {
                    cycle.Reverse();
                    string s = cycle[0].ToString();
                    for (int i = 1; i < cycle.Count; i++)
                        s += "-" + cycle[i].ToString();
                    bool flag = false; //есть ли палиндром для этого цикла графа в листбоксе?
                    for (int i = 0; i < listBoxMatrix.Items.Count; i++)
                        if (listBoxMatrix.Items[i].ToString() == s)
                        {
                            flag = true;
                            break;
                        }
                    if (!flag)
                    {
                        cycle.Reverse();
                        s = cycle[0].ToString();
                        for (int i = 1; i < cycle.Count; i++)
                            s += "-" + cycle[i].ToString();
                        listBoxMatrix.Items.Add(s);
                    }
                    return;
                }
            }
            for (int w = 0; w < E.Count; w++)
            {
                if (w == unavailableEdge)
                    continue;
                if (color[E[w].v2] == 1 && E[w].v1 == u)
                {
                    List<int> cycleNEW = new List<int>(cycle);
                    cycleNEW.Add(E[w].v2 + 1);
                    DFScycle(E[w].v2, endV, E, color, w, cycleNEW);
                    color[E[w].v2] = 1;
                }
                else if (color[E[w].v1] == 1 && E[w].v2 == u)
                {
                    List<int> cycleNEW = new List<int>(cycle);
                    cycleNEW.Add(E[w].v1 + 1);
                    DFScycle(E[w].v1, endV, E, color, w, cycleNEW);
                    color[E[w].v1] = 1;
                }
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (sheet.Image != null)
            {
                SaveFileDialog savedialog = new SaveFileDialog();
                savedialog.Title = "Сохранить картинку как...";
                savedialog.OverwritePrompt = true;
                savedialog.CheckPathExists = true;
                savedialog.Filter = "Image Files(*.BMP)|*.BMP|Image Files(*.JPG)|*.JPG|Image Files(*.GIF)|*.GIF|Image Files(*.PNG)|*.PNG|All files (*.*)|*.*";
                savedialog.ShowHelp = true;
                if (savedialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        sheet.Image.Save(savedialog.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    catch
                    {
                        MessageBox.Show("Невозможно сохранить изображение", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        public int ShowMyDialogBox()
        {
            var testDialog = new FormInput();

            testDialog.ShowDialog(this);
            int w = 0;
            if (testDialog.DialogRes == DialogResult.OK)
            {
                w = Convert.ToInt32(testDialog.TextBox1.Text);
            }

            testDialog.Dispose();
            return w;
        }

        private void btn_changeColor_Click(object sender, EventArgs e)
        {

            selectButton.Enabled = true;
            drawVertexButton.Enabled = true;
            drawEdgeButton.Enabled = true;
            deleteButton.Enabled = true;
            ColorFlag = !ColorFlag;
        }

        private void Select(int x, int y)
        {
            var selected_v = V.Find(v => Math.Pow((v.x - x), 2) + Math.Pow((v.y - y), 2) <= G.R * G.R);
            if (selected_v != null)
            {
                selection.Add(selected_v);
                if (selection.Count == 3)
                {
                    selection.RemoveAt(0);
                }
            }
        }
        private void ClearSelection()
        {
            selection.Clear();
        }

        private void DrawVertex(int x, int y, string name, Brush color)
        {
            V.Add(new Vertex(x, y, name, color));
            G.drawVertex(x, y, name, color);
            sheet.Image = G.GetBitmap();
        }
        private void DrawEdge(Vertex v1, Vertex v2)
        {
            // графически выделяет первую пикнутую вершину
            //G.drawSelectedVertex(v1.x, v1.y);
            int w = ShowMyDialogBox();
            var temp_e = new Edge(v1, v2, w);
            E.Add(temp_e);
            G.drawEdge(temp_e);


            sheet.Image = G.GetBitmap();

            ClearSelection();
        }
        private void RemoveVertex(Vertex v)
        {
            var edge = E.RemoveAll(e => (e.V1 == v) || (e.V2 == v));
            V.Remove(v);
        }
        private Edge GetSelectedEdge(int x, int y)
        {
            var selected_e = E.Find(ed => (((x - ed.V1.x) * (ed.V2.y - ed.V1.y) / (ed.V2.x - ed.V1.x) + ed.V1.y) <= (y + 4) &&
                ((x - ed.V1.x) * (ed.V2.y - ed.V1.y) / (ed.V2.x - ed.V1.x) + ed.V1.y) >= (y - 4)));

            if (selected_e != null) if ((selected_e.V1.x <= selected_e.V2.x && selected_e.V1.x <= x && x <= selected_e.V2.x) || (selected_e.V1.x >= selected_e.V2.x && selected_e.V1.x >= x && x >= selected_e.V2.x))
                {
                    return selected_e;
                }
            return null;
        }
        private Edge GetSelectedLoop(int x, int y)
        {
            var selected_e = E.Find(edge => (Math.Pow((edge.V1.x - G.R - x), 2) + Math.Pow((edge.V1.y - G.R - y), 2) <= ((G.R + 2) * (G.R + 2))) &&
                                (Math.Pow((edge.V1.x - G.R - x), 2) + Math.Pow((edge.V1.y - G.R - y), 2) >= ((G.R - 2) * (G.R - 2))));
            if (selected_e != null)
                if (selected_e.V1 == selected_e.V2) return selected_e;

            return null;
        }
    }
}
