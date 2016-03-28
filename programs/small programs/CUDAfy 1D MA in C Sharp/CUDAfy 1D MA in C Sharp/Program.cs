using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cudafy;
using Cudafy.Host;
using Cudafy.Translator;

namespace CUDAfy_1D_MA_in_C_Sharp
{
    class Program
    {
        static void Main(string[] args)
        {
            const int Size = 1000;
            const int Size1d = Size * Size;
            int[] A = new int[Size1d];
            int[] B = new int[Size1d];
            int[] C = new int[Size1d];
            int i, n = 5, count = 100;

            double[] time;


            for (i = 0; i < Size1d; i++)
            {
                A[i] = 2;
                B[i] = 3;
                C[i] = 0;
                //Console.WriteLine("i: " + i + "  A: " + A[i] + "   B: " + B[i] + "   C: " + C[i]);
            }
            Console.WriteLine("--------------------------------------------------------------------::");
            time = Mark3(A, B, C, Size, Size1d, n, count);

            Console.WriteLine("--------------------------------------------------------------------::");
            for (i = 0; i < Size1d; i++)
            {
                //Console.WriteLine("i: " + i + "  A: " + A[i] + "   B: " + B[i] + "   C: " + C[i]);
            }

            Console.ReadLine();


        }
        public static double[] Mark3(int[] A, int[] B, int[] C, int Size, int Size1d, int n, int count)
        {
            double[] result = new double[n];
            double dummy = 0.0;
            CudafyModule km = CudafyTranslator.Cudafy();

            GPGPU gpu = CudafyHost.GetDevice(CudafyModes.Target, CudafyModes.DeviceId);
            gpu.LoadModule(km);

            GPGPUProperties GPU_prop = gpu.GetDeviceProperties();
            int threadsPerBlock = 0;
	        int blocksPerGrid = 0;

	        if (Size1d< GPU_prop.MaxThreadsPerBlock)
	        {
		        threadsPerBlock = Size1d;
		        blocksPerGrid = 1;
	        }
	        else
	        {
		        threadsPerBlock = GPU_prop.MaxThreadsPerBlock;
		        blocksPerGrid = (Size1d/GPU_prop.MaxThreadsPerBlock)+1;
	        }


            for (int j = 0; j < n; j++)
            {
                Timer t = new Timer();
                for (int i = 0; i < count; i++)
                {
                    dummy += MA(A, B, C, Size, Size1d, gpu, threadsPerBlock, blocksPerGrid);
                }
                double time = t.Check() / count;
                result[j] = time;
                Console.WriteLine("time: " + time + " ms");
            }
            return result;
        }

        public static int MA(int[] A, int[] B, int[] C, int Size, int Size1d, GPGPU gpu, int threadsPerBlock, int blocksPerGrid)
        {
            // allocate the memory on the GPU
            int[] GPU_A = gpu.Allocate<int>(A);
            int[] GPU_B = gpu.Allocate<int>(B);
            int[] GPU_C = gpu.Allocate<int>(C);

            // copy the arrays 'a' and 'b' to the GPU
            gpu.CopyToDevice(A, GPU_A);
            gpu.CopyToDevice(B, GPU_B);

            // launch add on N threads
            gpu.Launch(threadsPerBlock, blocksPerGrid).GPU_MA(GPU_A, GPU_B, GPU_C, Size, Size1d);

            // copy the array 'c' back from the GPU to the CPU
            gpu.CopyFromDevice(GPU_C, C);

            gpu.Free(GPU_A);
            gpu.Free(GPU_B);
            gpu.Free(GPU_C);
            return 1;
        }
        [Cudafy]
        public static void GPU_MA(GThread thread, int[] GPU_A, int[] GPU_B, int[] GPU_C, int Size, int Size1d)
        {
            int i = thread.threadIdx.x + thread.blockDim.x * thread.blockIdx.x;

            if (i < Size1d)
            {
                GPU_C[i] = 0;
                int x = i / Size;
                int y = i % Size;
                //D[i] = (x*Size) + y;
                for (int z = 0; z < Size; z++)
                {
                    GPU_C[i] += GPU_A[(x * Size) + z] * GPU_B[(z * Size) + y];
                }
            }
        }
    }
}
