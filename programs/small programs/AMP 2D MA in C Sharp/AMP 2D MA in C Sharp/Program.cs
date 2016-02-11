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
        static void Main(string[] args)
        {
            int Size = 3;
            int Size1d = Size * Size;
            int[,] A = new int[Size , Size];
            int[,] B = new int[Size, Size];
            int[,] C = new int[Size, Size];

            for(int x = 0; x < Size; x++)
            {
                for(int y = 0; y < Size; y++)
                {
                    A[x,y] = 2;
                    B[x,y] = 3;
                    C[x,y] = 0;
                    Console.WriteLine("(x,y): (" + x + "," + y + ")" + "  A: " + A[x, y] + "   B: " + B[x, y] + "   C: " + C[x, y]);
                }
            }
            Console.ReadLine();
        }
    }
}
