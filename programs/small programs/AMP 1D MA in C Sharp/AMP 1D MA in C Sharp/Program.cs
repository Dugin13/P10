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
            const int Size = 3;
            const int Size1d = Size * Size;
            int[] A = new int[Size1d];
            int[] B = new int[Size1d];
            int[] C = new int[Size1d];
            int i, n = 10, count = 10000;

            double[] time;


            for (i = 0; i < Size1d; i++)
            {
                A[i] = 2;
                B[i] = 3;
                C[i] = 0;
                Console.WriteLine("i: " + i + "  A: " + A[i] + "   B: " + B[i] + "   C: " + C[i]);
            }


            //fixed (int* APt = &A[0], BPt = &B[0], CPt = &C[0])
            //{
            //    GPU_part_for_1D(APt, BPt, CPt, Size, Size1d);
            //}
            time = Mark3(A, B, C, Size, Size1d, n, count);

            Console.WriteLine("--------------------------------------------------------------------::" + 1e9);
            for (i = 0; i < Size1d; i++)
            {
                Console.WriteLine("i: " + i + "  A: " + A[i] + "   B: " + B[i] + "   C: " + C[i]);
            }

            Console.ReadLine();
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
                Console.WriteLine("time: "+ time + " ms");
                }
            return result;
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
