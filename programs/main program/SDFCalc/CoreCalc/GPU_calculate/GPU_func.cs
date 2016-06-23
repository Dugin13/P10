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
        private List<double> ConstantsList;
        private int[,] array_points;
        private int col, row;
        public GPU_func()
        {
            km = CudafyTranslator.Cudafy();

            gpu = CudafyHost.GetDevice(CudafyModes.Target, CudafyModes.DeviceId);
            gpu.LoadModule(km);

            GPU_prop = gpu.GetDeviceProperties();
        }


        public double[] Get_ConstantsList()
        {
            if (ConstantsList.Count ==0)
            {
                return new double[1];
            }
            else
            {
                return ConstantsList.ToArray();
            }
        }

        public int[,] makeFunc(FunCall input, Sheet sheet, int _col, int _row, int[,] _array_points)
        {
            array_points = _array_points;
            col = _col;
            row = _row;
            tempResult = new List<int>();
            ConstantsList = new List<double>();
            List<List<int>> temp = makeFuncHelper(input, true, sheet);


            int[,] result = new int[temp.Count(), 5];
            for (int i = 0; i < temp.Count(); i++)
            {
                for (int x = 0; x < 5; x++)
                {
                    result[i, x] = temp[i][x];
                }
            }


            return result;
        }

        private List<List<int>> makeFuncHelper(FunCall input, bool root, Sheet sheet)
        {
            List<List<int>> temp = new List<List<int>>();
            if (input.function.name == "IF")
            {
                int locationIf=0, locationOne = 0, locationTwo = 0;

                bool ifIsFunc = (input.es[0] is FunCall);
                bool ifIsNumber = (input.es[0] is NumberConst);
                bool ifIsRef = (input.es[0] is CellRef);

                bool oneIsFunc = (input.es[1] is FunCall);
                bool oneIsNumber = (input.es[1] is NumberConst);
                bool oneIsRef = (input.es[1] is CellRef);

                bool twoIsFunc = (input.es[2] is FunCall);
                bool twoIsNumber = (input.es[2] is NumberConst);
                bool twoIsRef = (input.es[2] is CellRef);

                // encoding for the bool that handle the if
                if (ifIsFunc)
                {
                    temp.AddRange(makeFuncHelper(input.es[0] as FunCall, false, sheet));
                    locationIf = temp[temp.Count - 1][3];
                }
                else if (ifIsNumber)
                {
                    locationIf = (int)Value.ToDoubleOrNan((input.es[0] as NumberConst).value); //TODO: ved ikke om der en nemmerer måde at gøre det her på
                }
                else if (ifIsRef)
                {
                    CellRef celltemp = input.es[0] as CellRef;
                    int cell_col = celltemp.raref.colRef + col;
                    int cell_row = celltemp.raref.rowRef + row;
                    if (cell_col >= array_points[0, 0] && cell_col <= array_points[1, 0] &&
                        cell_row >= array_points[0, 1] && cell_row <= array_points[1, 1])
                    {
                        locationIf = cell_col - array_points[0, 0] + 1;
                    }
                    else
                    {
                        // handling of const variables
                        double constValue = Value.ToDoubleOrNan(input.es[0].Eval(sheet, col, row));
                        if (!ConstantsList.Contains(constValue))
                        {
                            ConstantsList.Add(constValue);
                            locationIf = col + ConstantsList.IndexOf(constValue) + 1;
                        }
                        else
                        {
                            locationIf = col + ConstantsList.IndexOf(constValue) + 1;
                        }
                    }
                }
                else
                {
                    // some kind of error...
                }
                //---------------------
                // encoding in the if
                //---------------------

                if (oneIsFunc)
                {
                    temp.AddRange(makeFuncHelper(input.es[1] as FunCall, false, sheet));
                    locationOne = temp[temp.Count - 1][3];
                }
                else if (oneIsNumber)
                {
                    locationOne = (int)Value.ToDoubleOrNan((input.es[1] as NumberConst).value); //TODO: ved ikke om der en nemmerer måde at gøre det her på
                }
                else if (oneIsRef)
                {
                    CellRef celltemp = input.es[1] as CellRef;
                    int cell_col = celltemp.raref.colRef + col;
                    int cell_row = celltemp.raref.rowRef + row;
                    if (cell_col >= array_points[0, 0] && cell_col <= array_points[1, 0] &&
                        cell_row >= array_points[0, 1] && cell_row <= array_points[1, 1])
                    {
                        locationOne = cell_col - array_points[0, 0] + 1;
                    }
                    else
                    {
                        // handling of const variables
                        double constValue = Value.ToDoubleOrNan(input.es[0].Eval(sheet, col, row));
                        if (!ConstantsList.Contains(constValue))
                        {
                            ConstantsList.Add(constValue);
                            locationOne = col + ConstantsList.IndexOf(constValue) + 1;
                        }
                        else
                        {
                            locationOne = col + ConstantsList.IndexOf(constValue) + 1;
                        }
                    }
                }
                else
                {
                    // some kind of error...
                }


                if (twoIsFunc)
                {
                    temp.AddRange(makeFuncHelper(input.es[2] as FunCall, false, sheet));
                    locationTwo = temp[temp.Count - 1][3];

                }
                else if (twoIsNumber)
                {
                    locationTwo = (int)Value.ToDoubleOrNan((input.es[2] as NumberConst).value); //TODO: ved ikke om der en nemmerer måde at gøre det her på
                }
                else if (twoIsRef)
                {
                    CellRef celltemp = input.es[2] as CellRef;
                    int cell_col = celltemp.raref.colRef + col;
                    int cell_row = celltemp.raref.rowRef + row;
                    if (cell_col >= array_points[0, 0] && cell_col <= array_points[1, 0] &&
                        cell_row >= array_points[0, 1] && cell_row <= array_points[1, 1])
                    {
                        locationTwo = cell_col - array_points[0, 0] + 1;
                    }
                    else
                    {
                        // handling of const variables
                        double constValue = Value.ToDoubleOrNan(input.es[2].Eval(sheet, col, row));
                        if (!ConstantsList.Contains(constValue))
                        {
                            ConstantsList.Add(constValue);
                            locationTwo = col + ConstantsList.IndexOf(constValue) + 1;
                        }
                        else
                        {
                            locationTwo = col + ConstantsList.IndexOf(constValue) + 1;
                        }
                    }
                }
                else
                {
                    // some kind of error...
                }
                int outputPlace = 0;
                if (!root)
                {
                    int x = -1;
                    while (outputPlace == 0)
                    {
                        if (!tempResult.Contains(x))
                        {
                            outputPlace = x;
                            tempResult.Add(outputPlace);
                        }
                        else
                        {
                            x--;
                        }
                    }
                }
                if (locationIf < 0)
                {
                    tempResult.RemoveAt(tempResult.FindLastIndex(x => x == locationIf));
                }
                if (locationOne < 0)
                {
                    tempResult.RemoveAt(tempResult.FindLastIndex(x => x == locationOne));
                }
                if (locationTwo < 0)
                {
                    tempResult.RemoveAt(tempResult.FindLastIndex(x => x == locationOne));
                }
                List<int> result = new List<int>();
                result.Add(locationIf);
                result.Add(-1);
                result.Add(locationOne);
                result.Add(outputPlace);
                result.Add(locationTwo);
                temp.Add(result);
            }
            else
            {
                int locationOne = 0, locationTwo = 0; // used to hold the temp locating of the result
                // find where to place output

                bool oneIsFunc = (input.es[0] is FunCall);
                bool oneIsNumber = (input.es[0] is NumberConst);
                bool oneIsRef = (input.es[0] is CellRef);
                bool twoIsFunc = (input.es[1] is FunCall);
                bool twoIsNumber = (input.es[1] is NumberConst);
                bool twoIsRef = (input.es[1] is CellRef);

                if (oneIsFunc)
                {
                    temp.AddRange(makeFuncHelper(input.es[0] as FunCall, false, sheet));
                    locationOne = temp[temp.Count - 1][3];
                }
                else if (oneIsNumber)
                {
                    locationOne = (int)Value.ToDoubleOrNan((input.es[0] as NumberConst).value); //TODO: ved ikke om der en nemmerer måde at gøre det her på
                }
                else if (oneIsRef)
                {
                    CellRef celltemp = input.es[0] as CellRef;
                    int cell_col = celltemp.raref.colRef + col;
                    int cell_row = celltemp.raref.rowRef + row;
                    if (cell_col >= array_points[0, 0] && cell_col <= array_points[1, 0] &&
                        cell_row >= array_points[0, 1] && cell_row <= array_points[1, 1])
                    {
                        locationOne = cell_col - array_points[0, 0] + 1;
                    }
                    else
                    {
                        // handling of const variables
                        double constValue = Value.ToDoubleOrNan(input.es[0].Eval(sheet, col, row));
                        if (!ConstantsList.Contains(constValue))
                        {
                            ConstantsList.Add(constValue);
                            locationOne = col + ConstantsList.IndexOf(constValue) + 1;
                        }
                        else
                        {
                            locationOne = col + ConstantsList.IndexOf(constValue) + 1;
                        }
                    }
                }
                else
                {
                    // some kind of error...
                }

                if (twoIsFunc)
                {
                    temp.AddRange(makeFuncHelper(input.es[1] as FunCall, false, sheet));
                    locationTwo = temp[temp.Count - 1][3];

                }
                else if (twoIsNumber)
                {
                    locationTwo = (int)Value.ToDoubleOrNan((input.es[1] as NumberConst).value); //TODO: ved ikke om der en nemmerer måde at gøre det her på
                }
                else if (twoIsRef)
                {
                    CellRef celltemp = input.es[1] as CellRef;
                    int cell_col = celltemp.raref.colRef + col;
                    int cell_row = celltemp.raref.rowRef + row;
                    if (cell_col >= array_points[0, 0] && cell_col <= array_points[1, 0] &&
                        cell_row >= array_points[0, 1] && cell_row <= array_points[1, 1])
                    {
                        locationTwo = cell_col - array_points[0, 0] + 1;
                    }
                    else
                    {
                        // handling of const variables
                        double constValue = Value.ToDoubleOrNan(input.es[1].Eval(sheet, col, row));
                        if (!ConstantsList.Contains(constValue))
                        {
                            ConstantsList.Add(constValue);
                            locationTwo = col + ConstantsList.IndexOf(constValue) + 1;
                        }
                        else
                        {
                            locationTwo = col + ConstantsList.IndexOf(constValue) + 1;
                        }
                    }
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
                        break; // bool expression <, <=, >=, >, =, <>
                    case "<":
                        functionValue = 5;
                        break;
                    case "<=":
                        functionValue = 6;
                        break;
                    case ">=":
                        functionValue = 7;
                        break;
                    case ">":
                        functionValue = 8;
                        break;
                    case "=":
                        functionValue = 9;
                        break;
                    case "<>":
                        functionValue = 10;
                        break;

                    default:
                        // some kind of error...
                        break;
                }

                List<int> result = new List<int>();

                if (locationOne < 0)
                {
                    tempResult.RemoveAt(tempResult.FindLastIndex(x => x == locationOne));
                }
                if (locationTwo < 0)
                {
                    tempResult.RemoveAt(tempResult.FindLastIndex(x => x == locationTwo));
                }

                int outputPlace = 0;
                if (!root)
                {
                    int x = -1;
                    while (outputPlace == 0)
                    {
                        if (!tempResult.Contains(x))
                        {
                            outputPlace = x;
                            tempResult.Add(outputPlace);
                        }
                        else
                        {
                            x--;
                        }
                    }
                }

                result.Add(locationOne);
                result.Add(functionValue);
                result.Add(locationTwo);
                result.Add(outputPlace);
                result.Add(0);

                temp.Add(result);
            }
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

        public double[] calculate(double[,] input, int[,] func, double[] constValues)
        {
            int numberOfTempResult = findnumberOfTempResult(func);
            int SizeOfInput = input.GetLength(1);
            int AmountOfNumbers = input.GetLength(0);
            int numberOfFunctions = func.GetLength(0);
            int NumberOfImputNumbers = input.GetLength(1);

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
            double[] GPU_constValues = gpu.Allocate<double>(constValues);
            double[] GPU_output = gpu.Allocate<double>(output);
            int[,] GPU_func = gpu.Allocate<int>(func);

            // copy the arrays to GPU
            gpu.CopyToDevice(input, GPU_input);
            gpu.CopyToDevice(func, GPU_func);
            gpu.CopyToDevice(constValues, GPU_constValues);
            //gpu.CopyToDevice(tempResult, GPU_tempResult);

            // had to add Microsoft.CSharp.dll references for this to work ?
            gpu.Launch(threadsPerBlock, blocksPerGrid).GPUFunc(
                GPU_input,
                GPU_output,
                GPU_func,
                GPU_tempResult,
                GPU_constValues,
                AmountOfNumbers,
                numberOfFunctions,
                NumberOfImputNumbers);
            
            // copy the array 'c' back from the GPU to the CPU
            gpu.CopyFromDevice(GPU_output, output);

            gpu.Free(GPU_input);
            gpu.Free(GPU_tempResult);
            gpu.Free(GPU_output);
            gpu.Free(GPU_constValues);
            gpu.Free(GPU_func);



            return output;
        }

        [Cudafy]
        private static void GPUFunc(GThread thread,
            double[,] GPU_input,
            double[] GPU_output,
            int[,] GPU_func,
            double[,] GPU_TempResult,
            double[] GPU_constValues,
            int AmountOfNumbers,
            int number_Of_Functions,
            int NumberOfImputNumbers)
        {
            int i = thread.threadIdx.x + thread.blockDim.x * thread.blockIdx.x;
            if (i < AmountOfNumbers)
            {
                int x = 0;
                bool done = false;
                while(!done)
                {
                    if (GPU_func[x, 1] == -1) //handle if relly bad
                    {
                        double input_if, input_1, input_2;
                        if (GPU_func[x, 0] > 0)
                        {
                            int temp_index = GPU_func[x, 0] - 1;
                            if (temp_index < NumberOfImputNumbers)
                            {
                                input_if = GPU_input[i, temp_index];
                            }
                            else
                            {
                                input_if = GPU_constValues[temp_index - NumberOfImputNumbers];
                            }
                        }
                        else
                        {
                            int temp_index = (GPU_func[x, 0] * -1) - 1;
                            input_if = GPU_TempResult[i, temp_index];
                        }

                        // input_1 ------------
                        if (GPU_func[x, 2] > 0)
                        {
                            int temp_index = GPU_func[x, 2] - 1;
                            if (temp_index < NumberOfImputNumbers)
                            {
                                input_1 = GPU_input[i, temp_index];
                            }
                            else
                            {
                                input_1 = GPU_constValues[temp_index - NumberOfImputNumbers];
                            }
                        }
                        else
                        {
                            int temp_index = (GPU_func[x, 2] * -1) - 1;
                            input_1 = GPU_TempResult[i, temp_index];
                        }
                        // input_2------------
                        if (GPU_func[x, 4] > 0)
                        {
                            int temp_index = GPU_func[x, 4] - 1;
                            if (temp_index < NumberOfImputNumbers)
                            {
                                input_2 = GPU_input[i, temp_index];
                            }
                            else
                            {
                                input_2 = GPU_constValues[temp_index - NumberOfImputNumbers];
                            }
                        }
                        else
                        {
                            int temp_index = (GPU_func[x, 4] * -1) - 1;
                            input_2 = GPU_TempResult[i, temp_index];
                        }
                        double temp_result = 0;
                        
                        if(input_if == 1 )
                        {
                            temp_result = input_1;
                        }
                        else
                        {
                            temp_result = input_2;
                        }

                        // place temp_result in output
                        if (GPU_func[x, 3] == 0) // return the result
                        {
                            GPU_output[i] = temp_result;
                            done = true;
                        }
                        else // place the result in temp_result_holder
                        {
                            int temp_index = (GPU_func[x, 3] * -1) - 1;
                            GPU_TempResult[i, temp_index] = temp_result;
                            x = GPU_func[x, 4]; // fix for the 
                        }




                    } // -----------------------------------------------------------------------------
                    else // handle the other from of encoding
                    {
                        // finding which imput to use
                        double input_1, input_2;
                        // for input 1
                        if (GPU_func[x, 0] > 0)
                        {
                            int temp_index = GPU_func[x, 0] - 1;
                            if (temp_index < NumberOfImputNumbers)
                            {
                                input_1 = GPU_input[i, temp_index];
                            }
                            else
                            {
                                input_1 = GPU_constValues[temp_index - NumberOfImputNumbers];
                            }
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
                            if (temp_index < NumberOfImputNumbers)
                            {
                                input_2 = GPU_input[i, temp_index];
                            }
                            else
                            {
                                input_2 = GPU_constValues[temp_index - NumberOfImputNumbers];
                            }
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

                        // bool expression
                        else if (GPU_func[x, 1] == 5) // (<)
                        {
                            if (input_1 < input_2)
                                temp_result = 1;
                            else
                                temp_result = 0;
                        }
                        else if (GPU_func[x, 1] == 6) // (<=)
                        {
                            if (input_1 <= input_2)
                                temp_result = 1;
                            else
                                temp_result = 0;
                        }
                        else if (GPU_func[x, 1] == 7) // (>=)
                        {
                            if (input_1 >= input_2)
                                temp_result = 1;
                            else
                                temp_result = 0;
                        }
                        else if (GPU_func[x, 1] == 8) // (>)
                        {
                            if (input_1 > input_2)
                                temp_result = 1;
                            else
                                temp_result = 0;
                        }
                        else if (GPU_func[x, 1] == 9) // (=)
                        {
                            if (input_1 == input_2)
                                temp_result = 1;
                            else
                                temp_result = 0;
                        }
                        else if (GPU_func[x, 1] == 10) // (<>)
                        {
                            if (input_1 != input_2)
                                temp_result = 1;
                            else
                                temp_result = 0;
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
                            done = true;
                        }
                        else // place the result in temp_result_holder
                        {
                            int temp_index = (GPU_func[x, 3] * -1) - 1;
                            GPU_TempResult[i, temp_index] = temp_result;
                            x++;
                            //x = GPU_func[x, 4]; // fix for the 
                        }
                    }
                }
            }
        }
    }
}
