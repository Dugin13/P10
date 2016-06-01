using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AMP_1D_MA_in_C_Sharp
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
            //const int Size = 1000;
            //const int Size1d = Size * Size;
            
            

            //for (i = 0; i < Size1d; i++)
            //{
            //    A[i] = 2;
            //    B[i] = 3;
            //    Console.WriteLine("i: " + i + "  A: " + A[i] + "   B: " + B[i] + "   C: " + C[i]);
            //}

            Console.WriteLine("starting --------------------------------------------------------------------::");
            //fixed (int* APt = &A[0], BPt = &B[0], CPt = &C[0])
            //{
            //    GPU_part_for_1D(APt, BPt, CPt, Size, Size1d);
            //}
            //double[] Mark3_time = Mark3(A, B, C, Size, Size1d, n, count);
            //double[] Mark4_time = Mark4(A, B, C, Size, Size1d, n, count);
            for (i = 0; i < testSize.Length; i++)
            {
                int Size = testSize[i];
                int Size1d = Size * Size;
                int[] A = new int[Size1d];
                int[] B = new int[Size1d];
                int[] C = new int[Size1d];
                for (int x = 0; x < (Size1d); x++)
                {
                    A[x] = 2;
                    B[x] = 3;
                }
                Console.WriteLine(testSize[i] + " starting");
                double[] Mark4_time = Mark4(A, B, C, Size, Size1d, n, count);
                result[i, 0] = testSize[i];
                result[i, 1] = Mark4_time[0];
                result[i, 2] = Mark4_time[1];
            }

            Console.WriteLine("finis --------------------------------------------------------------------::" + 1e9);
            // Compose a string that consists of three lines.
            string lines = "AMP 1D MA in C Sharp  mean  , sdev \r\n";
            for(i=0;i< testSize.Length;i++)
            {
                lines = lines + "size: " + result[i, 0] + " time: " + result[i, 1] + " " + result[i, 2] + "\r\n";
            }

            // Write the string to a file.
            string path = @"c:\result\AMP_1D_MA_in_C_Sharp.txt";
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
        // taken from paper Microbenchmarks in Java and C#
        public static double[] Mark3(int[] A, int[] B, int[] C, int Size, int Size1d, int n, int count) 
        {
            double[] result = new double[n];
            double dummy = 0.0;
            for (int j=0; j<n; j++) {
                Timer t = new Timer();
                for (int i=0; i<count; i++)
                {
                    dummy += MA(A,B,C,Size, Size1d);
                }
                double time = t.Check() / count;
                result[j] = time;
                }
            return result;
        }

        public static double[] Mark4(int[] A, int[] B, int[] C, int Size, int Size1d, int n, int count)
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


        public static unsafe int MA(int[] A, int[] B, int[] C, int Size, int Size1d)
        {
            fixed (int* APt = &A[0], BPt = &B[0], CPt = &C[0])
            {
                GPU_part_for_1D(APt, BPt, CPt, Size, Size1d);
            }
            return 1;
        }
 

    }
}
