using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cudafy;
using Cudafy.Host;
using Cudafy.Translator;

namespace CUDAfy_Progamming_Test
{
    class Program
    {

        static int[,] makeTestFunc()
        {
            //// (a+b)*(c+d)
            int[,] func = new int[3, 4];
            func[0, 0] = 1; // a
            func[0, 1] = 1; // +
            func[0, 2] = 2; // b
            func[0, 3] = -1; // -> temp 1

            func[1, 0] = 3; // c
            func[1, 1] = 1; // +
            func[1, 2] = 4; // d
            func[1, 3] = -2; // -> temp 2

            func[2, 0] = -1; // temp 1
            func[2, 1] = 3; // *
            func[2, 2] = -2; // temp 2
            func[2, 3] = 0; // -> output

            return func;
        }

        static double[,] makeEmtyTempResult(int AmountOfNumbers, int numberOfTempResult)
        {
            double[,] temp = new double[AmountOfNumbers, numberOfTempResult];
            for (int i = 0; i < AmountOfNumbers; i++)
            {
                for (int x = 0; x < numberOfTempResult; x++)
                {
                    temp[i, x] = 0;
                }
            }
            return temp;

        }
        static void Main(string[] args)
        {
            bacisrun();

            classrun();
        }
        static void bacisrun()
        {
            int SizeOfInput = 4;
            int AmountOfNumbers = 10;
            int numberOfTempResult = 2; // must not be zero even know there is no temp result!!! 

            double[,] input = new double[SizeOfInput, AmountOfNumbers];
            double[] output = new double[AmountOfNumbers];
            double[,] tempResult = makeEmtyTempResult(AmountOfNumbers, numberOfTempResult);

            int[,] func = makeTestFunc();

            int numberOfFunctions = func.GetLength(0);

            for (int i = 0; i < SizeOfInput; i++)
            {
                for (int x = 0; x < AmountOfNumbers; x++)
                {
                    input[i, x] = i + 2;
                }
            }

            CudafyModule km = CudafyTranslator.Cudafy();

            GPGPU gpu = CudafyHost.GetDevice(CudafyModes.Target, CudafyModes.DeviceId);
            gpu.LoadModule(km);

            GPGPUProperties GPU_prop = gpu.GetDeviceProperties();
            int threadsPerBlock = 0;
            int blocksPerGrid = 0;


            if (AmountOfNumbers < GPU_prop.MaxThreadsPerBlock)
            {
                threadsPerBlock = AmountOfNumbers;
                blocksPerGrid = 1;
            }
            else
            {
                threadsPerBlock = GPU_prop.MaxThreadsPerBlock;
                blocksPerGrid = (AmountOfNumbers / GPU_prop.MaxThreadsPerBlock) + 1;
            }


            //CPU_func(input, output, func, tempResult, AmountOfNumbers, numberOfFunctions);


            // allocate the memory on the GPU
            double[,] GPU_input = gpu.Allocate<double>(input);
            double[,] GPU_tempResult = gpu.Allocate<double>(tempResult);
            double[] GPU_output = gpu.Allocate<double>(output);
            int[,] GPU_func = gpu.Allocate<int>(func);

            // copy the arrays to GPU
            gpu.CopyToDevice(input, GPU_input);
            gpu.CopyToDevice(func, GPU_func);
            gpu.CopyToDevice(tempResult, GPU_tempResult);

            // launch add on N threads
            gpu.Launch(threadsPerBlock, blocksPerGrid).GPU_func(GPU_input, GPU_output, GPU_func, GPU_tempResult, AmountOfNumbers, numberOfFunctions);

            // copy the array 'c' back from the GPU to the CPU
            gpu.CopyFromDevice(GPU_output, output);

            gpu.Free(GPU_input);
            gpu.Free(GPU_tempResult);
            gpu.Free(GPU_output);
            gpu.Free(GPU_func);
        }

        static void classrun()
        {
            int SizeOfInput = 4;
            int AmountOfNumbers = 10;
            int numberOfTempResult = 2; // must not be zero even know there is no temp result!!! 

            double[,] input = new double[SizeOfInput, AmountOfNumbers];
            double[] output = new double[AmountOfNumbers];
            double[,] tempResult = makeEmtyTempResult(AmountOfNumbers, numberOfTempResult);

            int[,] func = makeTestFunc();

            int numberOfFunctions = func.GetLength(0);

            for (int i = 0; i < SizeOfInput; i++)
            {
                for (int x = 0; x < AmountOfNumbers; x++)
                {
                    input[i, x] = i + 2;
                }
            }

            GPU_func GPU = new GPU_func();

            GPU.calculate(input, func);
        }



        [Cudafy]
        public static void GPU_test(GThread thread, double[,] GPU_input, double[] GPU_output, int[,] GPU_func, double[,] GPU_TempResult, int AmountOfNumbers, int number_Of_Functions)
        {
            int i = thread.threadIdx.x + thread.blockDim.x * thread.blockIdx.x;
            if (i < AmountOfNumbers)
            {
                GPU_output[i] = i;
            }
        }



        [Cudafy]
        public static void GPU_func(GThread thread, double[,] GPU_input, double[] GPU_output, int[,] GPU_func, double[,] GPU_TempResult, int AmountOfNumbers, int number_Of_Functions)
        {
            int i = thread.threadIdx.x + thread.blockDim.x * thread.blockIdx.x;
            if (i < AmountOfNumbers)
            {
                for (int x = 0; x < number_Of_Functions; x++)
                {
                    // finding which imput to use
                    double input_1, input_2;
                    // for input 1
                    if (GPU_func[x, 0] > 0)
                    {
                        int temp_index = GPU_func[x, 0] - 1;
                        input_1 = GPU_input[temp_index,i];
                    }
                    else
                    {
                        int temp_index = (GPU_func[x, 0] * -1) - 1;
                        input_1 = GPU_TempResult[i, temp_index];
                    }
                    // for input 2
                    if (GPU_func[x, 2] > 0)
                    {
                        int temp_index = GPU_func[x, 2] - 1;
                        input_2 = GPU_input[temp_index,i];
                    }
                    else
                    {
                        int temp_index = (GPU_func[x, 2] * -1) - 1;
                        input_2 = GPU_TempResult[i, temp_index];
                    }
                    double temp_result = 0;
                    // -----------------------------------------------------------------------------
                    // finding which operator to used
                    if (GPU_func[x, 1] == 1) // (+)
                    {
                        temp_result = input_1 + input_2;
                    }
                    else if (GPU_func[x, 1] == 2) // (-)
                    {
                        temp_result = input_1 - input_2;
                    }
                    else if (GPU_func[x, 1] == 3) // (*)
                    {
                        temp_result = input_1 * input_2;
                    }
                    else if (GPU_func[x, 1] == 4) // (/)
                    {
                        temp_result = input_1 / input_2;
                    }
                    else
                    {
                        // some kind of error
                    }
                    // -------------------------------------------------------------------------------
                    // place temp_result in output
                    if (GPU_func[x, 3] == 0) // return the result
                    {
                        GPU_output[i] = temp_result;
                    }
                    else // place the result in temp_result_holder
                    {
                        int temp_index = (GPU_func[x, 3] * -1) - 1;
                        GPU_TempResult[i, temp_index] = temp_result;
                    }
                }
            }

        }

        public static void CPU_func(double[,] GPU_input, double[] GPU_output, int[,] GPU_func, double[,] GPU_TempResult, int AmountOfNumbers, int number_Of_Functions)
        {
            for(int i=0; i < AmountOfNumbers; i++)
            {
                CPU_func_helper( i, GPU_input, GPU_output, GPU_func, GPU_TempResult, AmountOfNumbers, number_Of_Functions);
            }
        }

        public static void CPU_func_helper(int i, double[,] GPU_input, double[] GPU_output, int[,] GPU_func, double[,] GPU_TempResult, int AmountOfNumbers, int number_Of_Functions)
        {
            if (i < AmountOfNumbers)
            {
                for (int x = 0; x < number_Of_Functions; x++)
                {
                    // finding which imput to use
                    double input_1, input_2;
                    // for input 1
                    if (GPU_func[x, 0] > 0)
                    {
                        int temp_index = GPU_func[x, 0] - 1;
                        input_1 = GPU_input[temp_index, i];
                    }
                    else
                    {
                        int temp_index = (GPU_func[x, 0] * -1) - 1;
                        input_1 = GPU_TempResult[i, temp_index];
                    }
                    // for input 2
                    if (GPU_func[x, 2] > 0)
                    {
                        int temp_index = GPU_func[x, 2] - 1;
                        input_2 = GPU_input[temp_index, i];
                    }
                    else
                    {
                        int temp_index = (GPU_func[x, 2] * -1) - 1;
                        input_2 = GPU_TempResult[i, temp_index];
                    }
                    double temp_result = 0;
                    // -----------------------------------------------------------------------------
                    // finding which operator to used
                    if (GPU_func[x, 1] == 1) // (+)
                    {
                        temp_result = input_1 + input_2;
                    }
                    else if (GPU_func[x, 1] == 2) // (-)
                    {
                        temp_result = input_1 - input_2;
                    }
                    else if (GPU_func[x, 1] == 3) // (*)
                    {
                        temp_result = input_1 * input_2;
                    }
                    else if (GPU_func[x, 1] == 4) // (/)
                    {
                        temp_result = input_1 / input_2;
                    }
                    else
                    {
                        // some kind of error
                    }
                    // -------------------------------------------------------------------------------
                    // place temp_result in in place
                    if (GPU_func[x, 3] == 0) // return the result
                    {
                        GPU_output[i] = temp_result;
                    }
                    else // place the result in temp_result_holder
                    {
                        int temp_index = (GPU_func[x, 3] * -1) - 1;
                        GPU_TempResult[i, temp_index] = temp_result;
                    }
                }
            }

        }
    }
}
