using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace AMP_2D_MA_in_C_Sharp
{
    class Program
    {
        [DllImport("AMPCode", CallingConvention = CallingConvention.StdCall)]
        extern unsafe static void GPU_part_for_1D(int* A, int* B, int* C, int Size, int Size1d);
        static unsafe void Main(string[] args)
        {
            int[] testSize = new int[] { 5, 10, 20, 50, 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000 };
            double[,] result = new double[testSize.Length, 3];
            int i, n = 10, count = 100;

            //int[][] A = new int[Size][];
            //int[][] B = new int[Size][];
            //int[][] C = new int[Size][];

            //for(int x = 0; x < Size; x++)
            //{
            //    A[x] = new int[Size];
            //    B[x] = new int[Size];
            //    C[x] = new int[Size];
            //    for(int y = 0; y < Size; y++)
            //    {
            //        //A[x,y] = 2;
            //        //B[x,y] = 3;
            //        A[x][y] = 2;
            //        B[x][y] = 3;
            //        //Console.WriteLine("(x,y): (" + x + "," + y + ")" + "  A: " + A[x, y] + "   B: " + B[x, y] + "   C: " + C[x, y]);
            //    }
            //}
            Console.WriteLine("-----------------------------------------");
            for (i = 0; i < testSize.Length; i++)
            {
                int Size = testSize[i];
                int Size1d = Size * Size;
                int[][] A = new int[Size][];
                int[][] B = new int[Size][];
                int[][] C = new int[Size][];
                for (int x = 0; x < Size; x++)
                {
                    A[x] = new int[Size];
                    B[x] = new int[Size];
                    C[x] = new int[Size];
                    for (int y = 0; y < Size; y++)
                    {
                        //A[x,y] = 2;
                        //B[x,y] = 3;
                        A[x][y] = 2;
                        B[x][y] = 3;
                        //Console.WriteLine("(x,y): (" + x + "," + y + ")" + "  A: " + A[x, y] + "   B: " + B[x, y] + "   C: " + C[x, y]);
                    }
                }
                Console.WriteLine(testSize[i] + " starting");
                double[] Mark4_time = Mark4(A, B, C, Size, Size1d, n, count);
                result[i, 0] = testSize[i];
                result[i, 1] = Mark4_time[0];
                result[i, 2] = Mark4_time[1];
            }
            //double[] time = Mark3(A, B, C, Size, Size1d, n, count);
            string lines = "AMP 2D MA in C Sharp  mean  , sdev \r\n";
            for (i = 0; i < testSize.Length; i++)
            {
                lines = lines + "size: " + result[i, 0] + " time: " + result[i, 1] + " " + result[i, 2] + "\r\n";
            }

            // Write the string to a file.
            string path = @"c:\result\AMP_2D_MA_in_C_Sharp.txt";
            System.IO.StreamWriter file;
            if (!System.IO.File.Exists(path))
            {
                file = System.IO.File.CreateText(path);

            }
            else
            {
                file = new System.IO.StreamWriter(path);
            }
            file.WriteLine(lines);
            file.Close();
        }
        public static double[] Mark3(int[][] A, int[][] B, int[][] C, int Size, int Size1d, int n, int count)
        {
            double[] result = new double[n];
            double dummy = 0.0;
            for (int j = 0; j < n; j++)
            {
                Timer t = new Timer();
                for (int i = 0; i < count; i++)
                {
                    dummy += MA(A, B, C, Size, Size1d);
                }
                double time = t.Check() / count;
                result[j] = time;
                Console.WriteLine("time: " + time + " ms");
            }
            return result;
        }

        public static double[] Mark4(int[][] A, int[][] B, int[][] C, int Size, int Size1d, int n, int count)
        {
            double dummy = 0.0;
            double st = 0.0, sst = 0.0;
            for (int j = 0; j < n; j++)
            {
                Timer t = new Timer();
                for (int i = 0; i < count; i++)
                    dummy += MA(A, B, C, Size, Size1d);
                double time = t.Check() / count;
                st += time;
                sst += time * time;
            }
            double mean = st / n, sdev = Math.Sqrt((sst - mean * mean * n) / (n - 1));
            return new double[2] { mean, sdev };
        }

        public static unsafe int MA(int[][] A, int[][] B, int[][] C, int Size, int Size1d)
        {
            int[] flat_A = A.SelectMany(x => x).ToArray();
            int[] flat_B = B.SelectMany(x => x).ToArray();
            int[] flat_C = C.SelectMany(x => x).ToArray();
            fixed (int* APt = &flat_A[0], BPt = &flat_B[0], CPt = &flat_C[0])
            {
                GPU_part_for_1D(APt, BPt, CPt, Size, Size1d);
            }
            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    C[x][y] = flat_C[(x * Size) + y];
                }
            }
            return 1;
        }
    }
}
