using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cudafy;
using Cudafy.Host;
using Cudafy.Translator;

namespace Corecalc
{
    class GPU_func
    {
        private CudafyModule km;
        private GPGPU gpu;
        private GPGPUProperties GPU_prop;
        private List<int> tempResult;
        
        public GPU_func()
        {
            km = CudafyTranslator.Cudafy();

            gpu = CudafyHost.GetDevice(CudafyModes.Target, CudafyModes.DeviceId);
            gpu.LoadModule(km);

            GPU_prop = gpu.GetDeviceProperties();
        }
        
        public int[,] makeFunc(FunCall input)
        {
            tempResult = new List<int>();
            List<List<int>> temp = makeFuncHelper(input, true);

            int[][] tempArray = temp.Select(l => l.ToArray()).ToArray();



            int[,] result = new int[tempArray.Count(), 4];
            for (int i = 0; i < tempArray.Count(); i++)
            {
                for (int x = 0; x < 4; x++)
                {
                    result[i, x] = tempArray[i][x];
                }
            }


            return result;
        }

        private List<List<int>> makeFuncHelper(FunCall input, bool root)
        {
            List<List<int>> temp = new List<List<int>>();
            int locationOne = 0, locationTwo = 0; // used to hold the temp locating of the result
            // find where to place output

            bool oneIsFunc = (input.es[0] is FunCall);
            bool oneIsNumber = (input.es[0] is NumberConst);
            bool twoIsFunc = (input.es[1] is FunCall);
            bool twoIsNumber = (input.es[1] is NumberConst);
           
            if (oneIsFunc)
            {
                temp.AddRange(makeFuncHelper(input.es[0] as FunCall,false));
                locationOne = temp[temp.Count - 1][3];
            }
            else if(oneIsNumber)
            {
                locationOne = (int)Value.ToDoubleOrNan((input.es[0] as NumberConst).value); //TODO: ved ikke om der en nemmerer måde at gøre det her på
            }
            else
            {
                // some kind of error...
            }
            if (twoIsFunc)
            {
                temp.AddRange(makeFuncHelper(input.es[1] as FunCall, false));
                locationTwo = temp[temp.Count - 1][3];
               
            }
            else if (twoIsNumber)
            {
                locationTwo = (int)Value.ToDoubleOrNan((input.es[1] as NumberConst).value); //TODO: ved ikke om der en nemmerer måde at gøre det her på
            }
            else
            {
                // some kind of error...
            }

            int functionValue = 0; ;
            string function = input.function.name.ToString();
            switch (function)
            {
                case "+":
                    functionValue = 1;
                    break;
                case "-":
                    functionValue = 2;
                    break;
                case "*":
                    functionValue = 3;
                    break;
                case "/":
                    functionValue = 4;
                    break;
                default:
                    // some kind of error...
                    break;
            }

            List<int> result = new List<int>();

            if (oneIsFunc)
            {
                tempResult.RemoveAt(tempResult.FindLastIndex(x => x == locationOne));
            }
            if(twoIsFunc)
            {
                tempResult.RemoveAt(tempResult.FindLastIndex(x => x == locationTwo));
            }

            int outputPlace = 0;
            if(!root)
            { 
                int x = -1;
                while (outputPlace == 0)
                {
                    if(!tempResult.Contains(x))
                    {
                        outputPlace = x;
                    }
                    else
                    {
                        x--;
                    }
                }
            }
            tempResult.Add(outputPlace);

            result.Add(locationOne);
            result.Add(functionValue);
            result.Add(locationTwo);
            result.Add(outputPlace);

            temp.Add(result);

            return temp;
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

        public double[] calculate(double[,] input, int[,] func)
        {
            int numberOfTempResult = findnumberOfTempResult(func);
            int SizeOfInput = input.GetLength(1);
            int AmountOfNumbers = input.GetLength(0);
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
            //gpu.CopyToDevice(tempResult, GPU_tempResult);

            // had to add Microsoft.CSharp.dll references for this to work ?
            gpu.Launch(threadsPerBlock, blocksPerGrid).GPUFunc(GPU_input, GPU_output, GPU_func, GPU_tempResult, AmountOfNumbers, numberOfFunctions);
            
            // copy the array 'c' back from the GPU to the CPU
            gpu.CopyFromDevice(GPU_output, output);

            gpu.Free(GPU_input);
            gpu.Free(GPU_tempResult);
            gpu.Free(GPU_output);
            gpu.Free(GPU_func);



            return output;
        }

        [Cudafy]
        private static void GPUFunc(GThread thread, double[,] GPU_input, double[] GPU_output, int[,] GPU_func, double[,] GPU_TempResult, int AmountOfNumbers, int number_Of_Functions)
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
                        input_1 = GPU_input[i, temp_index];
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
                        input_2 = GPU_input[i, temp_index];
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
