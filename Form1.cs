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

        private double[] SolveTriDiag(double[][] TDM, double[] F, int N)
        {
            double[] alph = new double[N - 1];
            double[] beta = new double[N - 1];
            double[] b = new double[N];
            int i;

            alph[0] = -TDM[2][0] / TDM[1][0];
            beta[0] = F[0] / TDM[1][0];

            for (i = 1; i < N - 1; i++)
            {
                alph[i] = -TDM[2][i] / (TDM[1][i] + TDM[0][i] * alph[i - 1]);
                beta[i] = (F[i] - TDM[0][i] * beta[i - 1]) / (TDM[1][i] + TDM[0][i] * alph[i - 1]);
            }
            b[N - 1] = (F[N - 1] - TDM[0][N - 1] * beta[N - 2]) / (TDM[1][N - 1] + TDM[0][N - 1] * alph[N - 2]);

            for (i = N - 2; i > -1; i--)
            {
                b[i] = b[i + 1] * alph[i] + beta[i];
            }

            return b;
        }

        double Interpolate(double[,] Nodes, double[,] Coef, double x)
        {
            int i = 0;
            while (Nodes[i, 0] <= x)
            {
                i++;
            }
            i--;
            Console.WriteLine(i);
            return Coef[i, 0] + Coef[i, 1] * (x - Nodes[i, 0]) + Coef[i, 2] * Math.Pow((x - Nodes[i, 0]), 2) + Coef[i, 3] * Math.Pow((x - Nodes[i, 0]), 3);

        }

        private void SplineInterpolation(double [,] Nodes)
        {
            int N = Nodes.GetLength(0);
            var b = new double[N];
            var Coef = new double[N - 1, 4];

            double[] a = new double[N - 1];
            double[] c = new double[N - 1];
            double[] d = new double[N - 1];
            double[] delta = new double[N - 1];
            double[] h = new double[N - 1];
            //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //ORIGINAL LINE: double** TriDiagMatrix = new double*[3];
            double[][] TriDiagMatrix = new double[3][];

            b = new double[N];

            TriDiagMatrix[0] = new double[N];
            TriDiagMatrix[1] = new double[N];
            TriDiagMatrix[2] = new double[N];

            double[] f = new double[N];
            double x3;
            double xn;
            int i;

            if (N < 3)
            {
                //return -1;
            }

            x3 = Nodes[2, 0] - Nodes[0, 0];
            xn = Nodes[N - 1, 0] - Nodes[N - 3, 0];

            for (i = 0; i < N - 1; i++)
            {
                a[i] = Nodes[i, 1];
                h[i] = Nodes[i + 1, 0] - Nodes[i, 0];
                delta[i] = (Nodes[i + 1, 1] - Nodes[i, 1]) / h[i];
                TriDiagMatrix[0][i] = i > 0 ? h[i] : x3;
                f[i] = i > 0 ? 3 * (h[i] * delta[i - 1] + h[i - 1] * delta[i]) : 0;
            }

            TriDiagMatrix[1][0] = h[0];
            TriDiagMatrix[2][0] = h[0];
            for (i = 1; i < N - 1; i++)
            {
                TriDiagMatrix[1][i] = 2 * (h[i] + h[i - 1]);
                TriDiagMatrix[2][i] = h[i];
            }
            TriDiagMatrix[1][N - 1] = h[N - 2];
            TriDiagMatrix[2][N - 1] = xn;
            TriDiagMatrix[0][N - 1] = h[N - 2];


            i = N - 1;
            f[0] = ((h[0] + 2 * x3) * h[1] * delta[0] + Math.Pow(h[0], 2) * delta[1]) / x3;
            f[N - 1] = (Math.Pow(h[i - 1], 2) * delta[i - 2] + (2 * xn + h[i - 1]) * h[i - 2] * delta[i - 1]) / xn;

            for(i = 0; i < 3; ++i)
            {
                for(int j = 0; j < N - 1; ++j)
                {
                    Console.Write(TriDiagMatrix[i][j] + "\t");
                }
                Console.WriteLine();
            }

            b = SolveTriDiag(TriDiagMatrix, f, N);

            for (int j = 0; j < N - 1; j++)
            {
                d[j] = (b[j + 1] + b[j] - 2 * delta[j]) / (h[j] * h[j]);
                c[j] = 2 * (delta[j] - b[j]) / h[j] - (b[j + 1] - delta[j]) / h[j];

                Coef[j, 0] = a[j];
                Coef[j, 1] = b[j];
                Coef[j, 2] = c[j];
                Coef[j, 3] = d[j];
            }

            Console.WriteLine();
            for (i = 0; i < N - 1; ++i)
            {
                Console.WriteLine("{0:f3}\t{1:f3}\t{2:f3}\t{3:f3}\n", Coef[i, 0], Coef[i, 1], Coef[i, 2], Coef[i, 3]);
            }

            /* Рисование узлов в чарте */
            for (i = 0; i < N; ++i)
            {
                chart1.Series[0].Points.AddXY(Nodes[i, 0], Nodes[i, 1]);
            }

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
