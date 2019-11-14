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

        private void SplineInterpolation(double [,] Nodes)
        {
            int N = Nodes.GetLength(0);
            var b_vector = new double[N];
            var c_vector = new double[N];
            var d_vector = new double[N];

            var x_vector = new double[N];
            var delta_x = new double[N];

            var alpha_vector = new double[N];
            var beta_vector = new double[N];
            var gamma_vector = new double[N];

            var a_matrix = new double[N, N]; // Тридиагональная матрица
            var coefficient_matrix = new double[4][];

            for (int i = 0; i < 4; i++)
            {
                coefficient_matrix[i] = new double[N - 1];
            }

            for (int i = 0; i + 1 <= N - 1; i++)
            {
                delta_x[i] = Nodes[i + 1, 0] - Nodes[i, 0];
                if (delta_x[i] == 0.0)
                {
                    //return null;
                }
            }

            // b[i] = 3 * ( (f[i] - f[i - 1])*delta_x[i]/delta_x[i - 1] + (f[i + 1] - f[i])*delta_x[i - 1]/delta_x[i] ),  i = 1, ... , N - 2
            for (long i = 1; i + 1 <= N - 1; i++)
            {
                b_vector[i] = 3.0 * (delta_x[i] * ((Nodes[i, 1] - Nodes[i - 1, 1]) / delta_x[i - 1]) + delta_x[i - 1] * ((Nodes[i + 1, 1] - Nodes[i, 1]) / delta_x[i]));
            }
            b_vector[0] = ((delta_x[0] + 2.0 * (Nodes[2, 0] - Nodes[0, 0])) * delta_x[1] * ((Nodes[1, 1] - Nodes[0, 1]) / delta_x[0]) + Math.Pow(delta_x[0], 2.0) * ((Nodes[2, 1] - Nodes[1, 1]) / delta_x[1])) / (Nodes[2, 0] - Nodes[0, 0]);
            b_vector[N - 1] = (Math.Pow(delta_x[N - 1 - 1], 2.0) * ((Nodes[N - 2, 1] - Nodes[N - 3, 1]) / delta_x[N - 1 - 2]) + (2.0 * (Nodes[N - 1, 0] - Nodes[N - 3, 0]) + delta_x[N - 1 - 1]) * delta_x[N - 1 - 2] * ((Nodes[N - 1, 1] - Nodes[N - 2, 1]) / delta_x[N - 1 - 1])) / (Nodes[N - 1, 0] - Nodes[N - 3, 0]);

            // Коэффициенты матрицы
            beta_vector[0] = delta_x[1];
            gamma_vector[0] = Nodes[2, 0] - Nodes[0, 0];

            beta_vector[N - 1] = delta_x[N - 1 - 1];
            alpha_vector[N - 1] = (Nodes[N - 1, 0] - Nodes[N - 3, 0]);

            for (int i = 1; i < N - 1; i++)
            {
                beta_vector[i] = 2.0 * (delta_x[i] + delta_x[i - 1]);
                gamma_vector[i] = delta_x[i];
                alpha_vector[i] = delta_x[i - 1];
            }

            // Формирование матрицы
            for (int i = 0; i < N; ++i)
            {
                a_matrix[i, i] = beta_vector[i];
                if (i >= 0 && i < N-1) { a_matrix[i + 1, i] = alpha_vector[i + 1]; }
                if (i < N - 1) { a_matrix[i, i + 1] = gamma_vector[i]; }
            }

            for (int i = 0; i < N; i++)
            {
                Console.WriteLine(alpha_vector[i] + "  " + beta_vector[i] + " " + gamma_vector[i] + " | " + b_vector[i]);
            }

            Console.WriteLine("Linear System: ");
            for (int i = 0; i < N; ++i)
            {
                for (int j = 0; j < N; ++j)
                {
                    Console.Write("{0:f3}\t", a_matrix[i, j]);
                }
                Console.Write("| {0:f3}", b_vector[i]);
                Console.WriteLine();
            }

            // Решение методом Гаусса
            var xf_vector = new double[N];
            var system = new LinearSystem(a_matrix, b_vector);
            xf_vector = system.XVector;
            Console.WriteLine("x_vector: ");
            for (int k = 0; k < N; ++k)
            {
                Console.WriteLine(xf_vector[k]);
            }

            // Нахождение других коэффициентов
            for (int i = 0; i < N; ++i)
            {
                d_vector[i] = (b_vector[i + 1] + b_vector[i] - 2 * delta[i]) / (h[i] * h[i]);
                c_vector[i] = 2 * (delta[i] - b_vector[i]) / h[i] - (b_vector[i + 1] - delta[j]) / h[i];
            }
            /*
            double c = 0.0;
            for (long i = 0; i < N - 1; i++)
            {
                c = beta_vector[i];
                b[i] /= c;
                beta_vector[i] /= c;
                gamma_vector[i] /= c;

                c = alpha_vector[i + 1];
                b[i + 1] -= c * b[i];
                alpha_vector[i + 1] -= c * beta_vector[i];
                beta_vector[i + 1] -= c * gamma_vector[i];
            }

            b[N - 1] /= beta_vector[N - 1];
            beta_vector[N - 1] = 1.0;
            for (long i = N - 2; i >= 0; i--)
            {
                c = gamma_vector[i];
                b[i] -= c * b[i + 1];
                gamma_vector[i] -= c * beta_vector[i];
            }*/
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
