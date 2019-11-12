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
            int dim = Nodes.GetLength(0);
            var a_matrix = new double[dim, dim];
            var x_vector = new double[dim];
            var b_vector = new double[dim];

            for (int i = 0; i < dim; ++i)
            {
                b_vector[i] = 3*(((Nodes[i + 1, 1] - Nodes[i, 1])/(Nodes[i + 1, 0] - Nodes[i, 0])) - ((Nodes[i, 1] - Nodes[i - 1, 1]) / (Nodes[i, 0] - Nodes[i - 1, 0])));
            }

            double a, b, c, d;
            for(int i = 0; i < dim; ++i)
            {
                // h[k] = x[k] - x[k-1]
                /*a = Nodes[i, 0] - Nodes[i - 1, 0];
                b = Nodes[i + 1, 0] - Nodes[i, 0];
                c = 2 * ((Nodes[i, 0] - Nodes[i - 1, 0]) + (Nodes[i + 1, 0] - Nodes[i, 0]));*/

                a_matrix[i, i] = (i==0 || i==dim-1) ? 0 : (2 * ((Nodes[i, 0] - Nodes[i - 1, 0]) + (Nodes[i + 1, 0] - Nodes[i, 0]))); // C
                if (i >= 0 && i + 1 < dim) { a_matrix[i + 1, i] = Nodes[i + 1, 0] - Nodes[i, 0]; } // A
                if (i + 1 < dim) { a_matrix[i, i + 1] = Nodes[i + 1, 0] - Nodes[i, 0]; } // B
            }

            Console.WriteLine("Linear System: ");
            for (int i = 0; i < dim; ++i)
            {
                for (int j = 0; j < dim; ++j)
                {
                    Console.Write("{0:f3}\t", a_matrix[i, j]);
                }
                Console.Write("| {0:f3}", b_vector[i]);
                Console.WriteLine();
            }

            var system = new LinearSystem(a_matrix, b_vector);

            Console.WriteLine("X Vector: ");
            for (int i = 0; i < dim; ++i)
            {
                Console.Write("{0:f3}\t", system.XVector[i]);
            }
            Console.WriteLine();
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
