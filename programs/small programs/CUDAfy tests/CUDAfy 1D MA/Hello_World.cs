﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cudafy;
using Cudafy.Host;
using Cudafy.Translator;

namespace CUDAfy_1D_MA
{
    class Hello_World
    {
        private static int Size = 3;
        private static int Size1d = Size * Size;
        private static int n = 10;
        private static int count = 10000;
        private static dim3 blockSize = new dim3(Size, Size);


        // from https://gist.github.com/Banane9/b1aa823535eafa3fd6d1
        private static dim3 getGridSize(int x, int y)
        {
            return new dim3(((x - (x % Size)) / Size) + 1, ((y - (y % Size)) / Size) + 1);
        }
        public static void Execute()
        {
            CudafyModule km = CudafyTranslator.Cudafy();

            GPGPU gpu = CudafyHost.GetDevice(CudafyModes.Target, CudafyModes.DeviceId);
            gpu.LoadModule(km);

            int[,] A = new int[Size, Size];
            int[,] B = new int[Size, Size];
            int[,] C = new int[Size, Size];

            //int[][] A = new int[Size][];
            //int[][] B = new int[Size][];
            //int[][] C = new int[Size][];

            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    //A[x,y] = 2;
                    //B[x,y] = 3;
                    //C[x,y] = 0;
                    A[x,y] = 2;
                    B[x,y] = 3;
                    C[x,y] = 0;
                    //Console.WriteLine("(x,y): (" + x + "," + y + ")" + "  A: " + A[x, y] + "   B: " + B[x, y] + "   C: " + C[x, y]);
                    Console.WriteLine("(x,y): (" + x + "," + y + ")" + "  A: " + A[x,y] + "   B: " + B[x,y] + "   C: " + C[x,y]);
                }
            }
            // allocate the memory on the GPU
            int[,] GPU_A = gpu.Allocate<int>(A);
            int[,] GPU_B = gpu.Allocate<int>(B);
            int[,] GPU_C = gpu.Allocate<int>(C);

            // copy the arrays 'a' and 'b' to the GPU
            gpu.CopyToDevice(A, GPU_A);
            gpu.CopyToDevice(B, GPU_B);
            GPGPUProperties GPU_prop = gpu.GetDeviceProperties();



            // run GPU CODE
            gpu.Launch(getGridSize(Size, Size), blockSize, "GPU_MA", GPU_A, GPU_B, GPU_C, Size);

            //gpu.Launch(threadsPerBlock, blocksPerGrid).GPU_MA(GPU_A, GPU_B, GPU_C, Size);

            // copy the array 'c' back from the GPU to the CPU
            gpu.CopyFromDevice(GPU_C, C);

            gpu.Free(GPU_A);
            gpu.Free(GPU_B);
            gpu.Free(GPU_C);
            Console.WriteLine("-------------------------------------------");
            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    Console.WriteLine("(x,y): (" + x + "," + y + ")" + "  A: " + A[x,y] + "   B: " + B[x,y] + "   C: " + C[x,y]);
                }
            }
            Console.ReadLine();
        }

        [Cudafy]
        public static void GPU_MA(GThread thread, int[,] GPU_A, int[,] GPU_B, int[,] GPU_C, int Size)
        {
            int x = thread.threadIdx.x + thread.blockDim.x * thread.blockIdx.x;
            int y = thread.threadIdx.y + thread.blockDim.y * thread.blockIdx.y;

            if (x < Size && y < Size)
            {
                //GPU_C[y, x] = y;
                GPU_C[y, x] = 0;
                for (int z = 0; z < Size; z++)
                {
                    GPU_C[y, x] += GPU_A[y, z] * GPU_B[z, x];
                }
            }
        }
    }
}
