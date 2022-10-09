using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GSP_LR_2_3_IAIT_9
{
    public partial class Form1 : Form
    {
        Graphics g;
        Pen DrawPen = new Pen(Color.Black, 1);
        int FillType = 0; // По умолчанию заливка построчная 
        bool drawLine = false; // Отрисовывать граничные линии
        List<Point> VertexList = new List<Point>(); // Список вершин многоугольника
        public Form1()
        {
            InitializeComponent();
            g = pictureBox1.CreateGraphics(); //инициализация графики
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Полная очистка экрана и списка вершин, после нажатия на кнопку
            g.Clear(Color.White);
            VertexList.Clear();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Выбор цвета после изменения значения comboBox1
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    DrawPen.Color = Color.Black;
                    break;
                case 1:
                    DrawPen.Color = Color.Red;
                    break;
                case 2:
                    DrawPen.Color = Color.Green;
                    break;
                case 3:
                    DrawPen.Color = Color.Blue;
                    break;
            }
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Выбор способа заливки, после изменения значения comboBox2
            FillType = comboBox2.SelectedIndex;
        }

        /**
         * Метод выполняющий заливку по ориентации
         * Если точки проставлялись по часовой стрелке -> закрашена внутренняя часть фигуры
         * Если против часовой -> внешняя
         */
        private void FillByOrient()
        {
            // Определяем наивысшую и наинизшую по Y координате точки
            Point pmin = VertexList[0];
            Point pmax = VertexList[0];
            int ymin = 0;
            int ymax = pictureBox1.Height;
            foreach (Point p in VertexList)
            {
                if (p.Y > pmax.Y) pmax = p;
                if (p.Y < pmin.Y) pmin = p;
            }
            ymin = pmin.Y < ymin ? ymin : pmin.Y;
            ymax = pmax.Y > ymax ? ymax : pmax.Y;
            // получаем индекс вершины с наибольшим Y и её соседей (учитвая граничные случаи)
            int j = VertexList.IndexOf(pmax);
            int jl = j == 0 ? VertexList.Count - 1 : j - 1;
            int jr = j == VertexList.Count - 1 ? 0 : j + 1;

            // Вычисляем площадь треугольника, заданного вершиной с наибольшим Y и её соседями
            double s = ((VertexList[jl].X * VertexList[j].Y)
                + (VertexList[jl].Y * VertexList[jr].X)
                + (VertexList[j].X * VertexList[jr].Y)
                - (VertexList[j].Y * VertexList[jr].X)
                - (VertexList[jl].Y * VertexList[j].X)
                - (VertexList[jl].X * VertexList[jr].Y)
            ) / 2;
            // Направление обхода
            bool CW = s < 0;
            if (CW) // Если обход против часовой, заполняем строки от 0 до ymin
            {
                for (int y = 0; y < ymin; y++)
                {
                    g.DrawLine(DrawPen, new Point(0, y), new Point(pictureBox1.Width, y));
                }
            }
            // списки x координат точек пересечения
            List<int> Xr = new List<int>();
            List<int> Xl = new List<int>();
            for (int y = ymin; y <= ymax; y++)
            {
                // очищаем списки координат
                Xr.Clear();
                Xl.Clear();
                for (int i = 0; i < VertexList.Count; i++)
                {
                    int k = i < VertexList.Count - 1 ? i + 1 : 0;
                    // обработка граничных случаев
                    if (((VertexList[i].Y < y) && (VertexList[k].Y >= y)) || ((VertexList[i].Y >= y) && (VertexList[k].Y < y)))
                    {
                        // формула определения x координаты пересечения двух отрезков, заданных вершинами
                        double x = VertexList[i].X + ((VertexList[k].X - VertexList[i].X) * (y - VertexList[i].Y)) / (VertexList[k].Y - VertexList[i].Y);
                        if (VertexList[k].Y - VertexList[i].Y > 0)
                        {
                            Xr.Add((int)x);
                        } else
                        {
                            Xl.Add((int)x);
                        }
                    }
                }
                if (CW) // Если обход против часовой, добавляем краевые точки
                {
                    Xl.Add(0);
                    Xr.Add(pictureBox1.Width);
                }
                Xl.Sort((a, b) => a.CompareTo(b)); // сортировка по возростанию
                Xr.Sort((a, b) => a.CompareTo(b)); // сортировка по возростанию
                // отрисовка
                for (int id = 0; id < Xl.Count && id < Xr.Count; id++)
                {
                    if (Xl[id] < Xr[id])
                    {
                        g.DrawLine(DrawPen, new Point(Xl[id], y), new Point(Xr[id], y));
                    } 
                }
            }
            if (CW) // Если обход против часовой, заполняем строки от ymax до границы полотна
            {
                for (int y = ymax; y < pictureBox1.Height; y++)
                {
                    g.DrawLine(DrawPen, new Point(0, y), new Point(pictureBox1.Width, y));
                }
            }
        }

        /**
         * Метод выполняющий заливку по прямым
         */
        private void FillByLine()
        {
            // Определяем наивысшую и наинизшую по Y координате точки
            Point pmin = VertexList[0];
            Point pmax = VertexList[0];
            int ymin = 0;
            int ymax = pictureBox1.Height;
            foreach (Point p in VertexList)
            {
                if (p.Y > pmax.Y) pmax = p;
                if (p.Y < pmin.Y) pmin = p;
            }
            ymin = pmin.Y < ymin ? ymin : pmin.Y;
            ymax = pmax.Y > ymax ? ymax : pmax.Y;

            List<int> xBoundaries = new List<int>();

            for (int y = ymin; y <= ymax; y++)
            {
                xBoundaries.Clear();
                for (int i = 0; i < VertexList.Count ; i++)
                {
                    int k = i < VertexList.Count - 1 ? i + 1 : 0;
                    if (((VertexList[i].Y < y) && (VertexList[k].Y >= y)) || ((VertexList[i].Y >= y) && (VertexList[k].Y < y)))
                    {
                        // формула определения x координаты пересечения двух отрезков, заданных вершинами
                        double x = VertexList[i].X + ((VertexList[k].X - VertexList[i].X) * (y - VertexList[i].Y)) / (VertexList[k].Y - VertexList[i].Y);
                        xBoundaries.Add((int)x);
                    }
                }
                xBoundaries.Sort((a, b) => a.CompareTo(b)); // сортировка по возростанию
                for (int el = 0; el < xBoundaries.Count - 1; el += 2)
                {
                    g.DrawLine(DrawPen, new Point(xBoundaries[el], y), new Point(xBoundaries[el + 1], y));
                }
            }
        }

        /**
         * Метод реинициализирующий полотно, при изменении размера окна
         */
        private void Form1_Resize(object sender, EventArgs e)
        {
            g = pictureBox1.CreateGraphics(); //реинициализация графики
        }

        /**
         * Рисование на полотне
         * На нажатие ЛКМ - ставится точка-вершина многоугольника
         * На нажатие ПКМ - попытка заливки получившейся по точкам фигуры
         * Если вершин < 3, заливка происходить не будет
         * После заливки, массив точек очищается
         */
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                VertexList.Add(new Point() { X = e.X, Y = e.Y });
                g.DrawEllipse(DrawPen, e.X - 2, e.Y - 2, 5, 5);
                int pointCount = VertexList.Count - 1;
                if (pointCount > 0 && drawLine) {
                    g.DrawLine(DrawPen, VertexList[pointCount - 1], VertexList[pointCount]);
                }
            } else if (e.Button == MouseButtons.Right)
            {
                int pointCount = VertexList.Count - 1;
                if (pointCount < 2)
                {
                    MessageBox.Show("Введено слишком мало точек для существования многоугольника, отрисовка невозможна");
                    return;
                }
                if(drawLine)
                {
                    g.DrawLine(DrawPen, VertexList[pointCount - 1], VertexList[pointCount]);
                    g.DrawLine(DrawPen, VertexList[pointCount], VertexList[0]); // соединяем первую и последнюю точки
                }
                // Заливка, в зависимости от типа
                if (FillType == 0)
                {
                    FillByLine();
                } else
                {
                    FillByOrient();
                }
                // Чистим массив точек после заливки
                VertexList.Clear();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            drawLine = !drawLine;
        }
    }
}
