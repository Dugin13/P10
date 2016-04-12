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
    class GPU_func
    {
        private CudafyModule km;
        private GPGPU gpu;
        private GPGPUProperties GPU_prop;

        public GPU_func()
        {
            km = CudafyTranslator.Cudafy();

            gpu = CudafyHost.GetDevice(CudafyModes.Target, CudafyModes.DeviceId);
            gpu.LoadModule(km);

            GPGPUProperties GPU_prop = gpu.GetDeviceProperties();
        }

        private static double[,] makeEmtyTempResult(int AmountOfNumbers, int numberOfTempResult)
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

        private static int findnumberOfTempResult(int[,] func)
        {
            int number = 0;
            for (int i = 0; i < func.GetLength(0); i++)
            {
                if (number > func[i, 3])
                {
                    number = func[i, 3];
                }
            }
            if (number == 0)
            {
                number--;
            }

            number = number * -1;
            return number;
        }

        public double[] calculate(double[,] input,int[,] func)
        {
            int numberOfTempResult = findnumberOfTempResult(func);
            int SizeOfInput = input.GetLength(0);
            int AmountOfNumbers = input.GetLength(1);
            int numberOfFunctions = func.GetLength(0);
            
            
            double[] output = new double[AmountOfNumbers];
            double[,] tempResult = makeEmtyTempResult(AmountOfNumbers, numberOfTempResult);

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



            return output;
        }

        [Cudafy]
        private static void GPU_func(GThread thread, double[,] GPU_input, double[] GPU_output, int[,] GPU_func, double[,] GPU_TempResult, int AmountOfNumbers, int number_Of_Functions)
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

    }
}
