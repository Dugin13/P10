using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cudafy;
using Cudafy.Host;
using Cudafy.Translator;

namespace CUDAfy_2D_MA_in_C_Sharp
{
    class Program
    {

        static void Main(string[] args)
        {
            int[] testSize = new int[] { 5, 10, 20, 50, 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000 };
            double[,] result = new double[testSize.Length, 3];
            int i, n = 10, count = 100;

            for (i = 0; i < testSize.Length; i++)
            {
                int Size = testSize[i];
                int Size1d = Size * Size;
                int[,] A = new int[Size, Size];
                int[,] B = new int[Size, Size];
                int[,] C = new int[Size, Size];

                for (int x = 0; x < Size; x++)
                {
                    for (int y = 0; y < Size; y++)
                    {
                        A[x, y] = 2;
                        B[x, y] = 3;
                        C[x, y] = 0;
                        //Console.WriteLine("(x,y): (" + x + "," + y + ")" + "  A: " + A[x, y] + "   B: " + B[x, y] + "   C: " + C[x, y]);
                    }
                }
                Console.WriteLine(testSize[i] + " starting");
                double[] Mark4_time = Mark4(A, B, C, Size, Size1d, n, count);
                result[i, 0] = testSize[i];
                result[i, 1] = Mark4_time[0];
                result[i, 2] = Mark4_time[1];
            }
            Console.WriteLine("finis testing");
            string lines = "CUDAfy 2D MA in C Sharp  mean  , sdev \r\n";
            for (i = 0; i < testSize.Length; i++)
            {
                lines = lines + "size: " + result[i, 0] + " time: " + result[i, 1] + " " + result[i, 2] + "\r\n";
            }

            // Write the string to a file.
            string path = @"c:\result\CUDAfy_2D_MA_in_C_Sharp.txt";
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

        public static double[] Mark3(int[,] A, int[,] B, int[,] C, int Size, int Size1d, int n, int count)
        {
            double[] result = new double[n];
            double dummy = 0.0;
            CudafyModule km = CudafyTranslator.Cudafy();
                
            GPGPU gpu = CudafyHost.GetDevice(CudafyModes.Target, CudafyModes.DeviceId);
            gpu.LoadModule(km);

            GPGPUProperties GPU_prop = gpu.GetDeviceProperties();
            int maxTheadBlockSize = (int)Math.Sqrt(GPU_prop.MaxThreadsPerBlock);

            for (int j = 0; j < n; j++)
            {
                Timer t = new Timer();
                for (int i = 0; i < count; i++)
                {
                    dummy += MA(A, B, C, gpu, maxTheadBlockSize,Size);
                }
                double time = t.Check() / count;
                result[j] = time;
                Console.WriteLine("time: " + time + " ms");
            }
            return result;
        }

        public static double[] Mark4(int[,] A, int[,] B, int[,] C, int Size, int Size1d, int n, int count)
        {
            double dummy = 0.0;
            double st = 0.0, sst = 0.0;

            CudafyModule km = CudafyTranslator.Cudafy();

            GPGPU gpu = CudafyHost.GetDevice(CudafyModes.Target, CudafyModes.DeviceId);
            gpu.LoadModule(km);

            GPGPUProperties GPU_prop = gpu.GetDeviceProperties();
            int maxTheadBlockSize = (int)Math.Sqrt(GPU_prop.MaxThreadsPerBlock);

            for (int j = 0; j < n; j++)
            {
                Timer t = new Timer();
                for (int i = 0; i < count; i++)
                    dummy += MA(A, B, C, gpu, maxTheadBlockSize, Size);
                double time = t.Check() / count;
                st += time;
                sst += time * time;
            }
            double mean = st / n, sdev = Math.Sqrt((sst - mean * mean * n) / (n - 1));
            return new double[2] { mean, sdev };
        }


        public static int MA(int[,] A, int[,] B, int[,] C, GPGPU gpu, int maxTheadBlockSize, int Size)
        {
            // allocate the memory on the GPU
            int[,] GPU_A = gpu.Allocate<int>(A);
            int[,] GPU_B = gpu.Allocate<int>(B);
            int[,] GPU_C = gpu.Allocate<int>(C);

            // copy the arrays 'a' and 'b' to the GPU
            gpu.CopyToDevice(A, GPU_A);
            gpu.CopyToDevice(B, GPU_B);
            dim3 threadsPerBlock;
            // find the number of threads and blocks
            if (Size < maxTheadBlockSize)
            {
                threadsPerBlock = new dim3(Size, Size);
            }
            else
            {
                threadsPerBlock = new dim3(maxTheadBlockSize, maxTheadBlockSize);
            }
            dim3 block = new dim3(Size, Size);

            // launch GPU_MA
            gpu.Launch(block, threadsPerBlock, "GPU_MA", GPU_A, GPU_B, GPU_C, Size);

            // copy the array 'c' back from the GPU to the CPU
            gpu.CopyFromDevice(GPU_C, C);

            gpu.Free(GPU_A);
            gpu.Free(GPU_B);
            gpu.Free(GPU_C);
            return 1;
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
