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
using System.Windows;

namespace SplineInterpolation
{
    public partial class Form1 : Form
    {
        private int nodesNum;

        public Form1()
        {
            InitializeComponent();
        }

        // Метод прогонки для решения трёхдиагональной матрицы
        private double[] SolveTriDiag(double[][] TriDiagMatrix, double[] F, int N)
        {
            // Рассчётные коэффициенты
            double[] alpha = new double[N - 1];
            double[] beta = new double[N - 1];
            double[] b = new double[N];

            // Начальные значения (по формулам)
            alpha[0] = -TriDiagMatrix[2][0] / TriDiagMatrix[1][0];
            beta[0] = F[0] / TriDiagMatrix[1][0];

            // Прямая прогонка
            for (int i = 1; i < N - 1; i++)
            {
                alpha[i] = -TriDiagMatrix[2][i] / (TriDiagMatrix[1][i] + TriDiagMatrix[0][i] * alpha[i - 1]);
                beta[i] = (F[i] - TriDiagMatrix[0][i] * beta[i - 1]) / (TriDiagMatrix[1][i] + TriDiagMatrix[0][i] * alpha[i - 1]);
            }
            b[N - 1] = (F[N - 1] - TriDiagMatrix[0][N - 1] * beta[N - 2]) / (TriDiagMatrix[1][N - 1] + TriDiagMatrix[0][N - 1] * alpha[N - 2]);

            // Обратная прогонка
            for (int i = N - 2; i > -1; i--)
            {
                b[i] = b[i + 1] * alpha[i] + beta[i];
            }

            return b;
        }

        // Интерполяция в точке
        double Interpolate(double[,] Nodes, double[,] Coef, double x)
        {
            // Определяем, между какими узлами лежит точка
            int i = 0;
            while (Nodes[i, 0] <= x)
                i++;
            i--;

            // Вычисляем значение полинома в точке
            return Coef[i, 0] + Coef[i, 1] * (x - Nodes[i, 0]) + Coef[i, 2] * Math.Pow((x - Nodes[i, 0]), 2) + Coef[i, 3] * Math.Pow((x - Nodes[i, 0]), 3);

        }

        private void SplineInterpolation(double [,] Nodes)
        {
            int N = Nodes.GetLength(0); // Узлы
            var Coef = new double[N - 1, 4]; // Итоговые коэффициенты

            // Рассчётные коэффициенты
            var a = new double[N - 1];
            var b = new double[N];
            var c = new double[N - 1];
            var d = new double[N - 1];
            var delta = new double[N - 1];
            var h = new double[N - 1];
            var f = new double[N];
            double x3;
            double xn;

            // Тридиагональная матрица
            var TriDiagMatrix = new double[3][];
            TriDiagMatrix[0] = new double[N];
            TriDiagMatrix[1] = new double[N];
            TriDiagMatrix[2] = new double[N];

            // Начальное и конечное значения
            x3 = Nodes[2, 0] - Nodes[0, 0];
            xn = Nodes[N - 1, 0] - Nodes[N - 3, 0];

            /* Заполнение трёхдиагональной матрицы */
            for (int i = 0; i < N - 1; i++)
            {
                a[i] = Nodes[i, 1];
                h[i] = Nodes[i + 1, 0] - Nodes[i, 0];
                delta[i] = (Nodes[i + 1, 1] - Nodes[i, 1]) / h[i];
                TriDiagMatrix[0][i] = i > 0 ? h[i] : x3;
                f[i] = i > 0 ? 3 * (h[i] * delta[i - 1] + h[i - 1] * delta[i]) : 0;
            }

            TriDiagMatrix[1][0] = h[0];
            TriDiagMatrix[2][0] = h[0];
            for (int i = 1; i < N - 1; i++)
            {
                TriDiagMatrix[1][i] = 2 * (h[i] + h[i - 1]);
                TriDiagMatrix[2][i] = h[i];
            }
            TriDiagMatrix[1][N - 1] = h[N - 2];
            TriDiagMatrix[2][N - 1] = xn;
            TriDiagMatrix[0][N - 1] = h[N - 2];


            int k = N - 1;
            f[0] = ((h[0] + 2 * x3) * h[1] * delta[0] + Math.Pow(h[0], 2) * delta[1]) / x3;
            f[N - 1] = (Math.Pow(h[k - 1], 2) * delta[k - 2] + (2 * xn + h[k - 1]) * h[k - 2] * delta[k - 1]) / xn;

            /* Отладочный вывод трёхдиагональной */
            for(int i = 0; i < 3; ++i)
            {
                for(int j = 0; j < N - 1; ++j)
                {
                    Console.Write(TriDiagMatrix[i][j] + "\t");
                }
                Console.WriteLine();
            }

            b = SolveTriDiag(TriDiagMatrix, f, N); // Вычисление коэффициента b

            /* Вычисление остальных коэффициентов через коэф. b */
            for (int j = 0; j < N - 1; j++)
            {
                d[j] = (b[j + 1] + b[j] - 2 * delta[j]) / (h[j] * h[j]);
                c[j] = 2 * (delta[j] - b[j]) / h[j] - (b[j + 1] - delta[j]) / h[j];

                Coef[j, 0] = a[j];
                Coef[j, 1] = b[j];
                Coef[j, 2] = c[j];
                Coef[j, 3] = d[j];
            }

            /* Вывод: Итоговые коэффициенты */
            Console.WriteLine();
            for (int i = 0; i < N - 1; ++i)
            {
                Console.WriteLine("{0:f3}\t{1:f3}\t{2:f3}\t{3:f3}\n", Coef[i, 0], Coef[i, 1], Coef[i, 2], Coef[i, 3]);
            }

            /* Вывод: Рисование узлов в чарте */
            for (int i = 0; i < N; ++i)
            {
                chart1.Series[0].Points.AddXY(Nodes[i, 0], Nodes[i, 1]);
            }
            /* Вывод: Рисование сплайна в чарте */
            for (double x = Nodes[0, 0]; x <= Nodes[N - 1, 0]; x += 0.01) // От первого до последнего x в nodes
            {
                chart1.Series[1].Points.AddXY(x, Interpolate(Nodes, Coef, x));
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            // Массив узлов
            double[,] Nodes;

            /* Ввод из файла */
            using (StreamReader sstream = new StreamReader("nodes.in"))
            {
                string line;
                nodesNum = int.Parse(sstream.ReadLine());
                Nodes = new double[nodesNum, 2];

                for (int i = 0; i < nodesNum; ++i)
                {
                    line = sstream.ReadLine();
                    string[] tokens = line.Split();


                    Nodes[i, 0] = double.Parse(tokens[0]);
                    Nodes[i, 1] = double.Parse(tokens[1]);
                }

            }

            SplineInterpolation(Nodes);

        }
    }
}
