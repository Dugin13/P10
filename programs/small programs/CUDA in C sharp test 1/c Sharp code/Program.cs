using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace c_Sharp_code
{
    class Program
    {
        // form the site https://algoslaves.wordpress.com/2013/08/25/nvidia-cuda-hello-world-in-managed-c-and-f-with-use-of-managedcuda/
        static CudaKernel addWithCuda;

        static void InitKernels()
        {
            CudaContext cntxt = new CudaContext();
            CUmodule cumodule = cntxt.LoadModule(@"C:\Users\Niels\Documents\uni ting\P10\P10\programs\small programs\CUDA 1D MA in C Sharp\CUDA 1D MA in C Sharp\Debug\kernel.ptx");
            addWithCuda = new CudaKernel("_Z6kerneliiPi", cumodule, cntxt);
        }

        static Func<int, int, int> cudaAdd = (a, b) =>
        {
            // init output parameters
            CudaDeviceVariable result_dev = 0;
            int result_host = 0;
            // run CUDA method
            addWithCuda.Run(a, b, result_dev.DevicePointer);
            // copy return to host
            result_dev.CopyToHost(ref result_host);
            return result_host;
        };

        static void Main(string[] args)
        {
            InitKernels();
            Console.WriteLine(cudaAdd(3, 10));
            Console.ReadKey();
        }
    }
}
