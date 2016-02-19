// AMP Lambda Expression Test.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <amp.h>
#include <iostream>
#include <vector>

int _tmain(int argc, _TCHAR* argv[])
{
	const int Size = 3;
	const int Size1d = Size*Size;
	const int Size_Of_input = 2;
	const int number_Of_Functions = 1;
	const int number_Of_Temp_result = 1; // must not be zero even know there is no temp result!!!
	int input[Size_Of_input][Size1d];
	int A[Size1d];
	int B[Size1d];
	int C[Size1d];
	int i, x, y, z;

	for (i = 0; i < Size1d; i++)
	{
		A[i] = 2;
		B[i] = 3;
		input[0][i] = A[i];
		input[1][i] = B[i];
		C[i] = 0;
	}
	std::vector<int> func; 
	//for (i = 0; i < 3; i++)
	//{
	//	for (x = 0; x < 4; x++)
	//	{
	//		func.push_back((i * 3) + x);
	//	}
	//}
	// a+b -> output
	func.push_back(1); // a
	func.push_back(1); // +
	func.push_back(2); // b
	func.push_back(0); // -> output





	int *p_input = &input[0][0];
	concurrency::array_view<int, 2> GPU_input(Size_Of_input, Size1d, p_input);
	concurrency::array_view<int, 1> GPU_output(Size1d, C);
	GPU_output.discard_data();
	
	concurrency::array_view<int, 2> GPU_Func(number_Of_Functions, 4, func);
	
	std::vector<int> Temp_result;
	for (i = 0; i < Size1d; i++)
	{
		for (x = 0; x < number_Of_Temp_result; x++)
		{
			Temp_result.push_back(0);
		}
	}
	concurrency::array_view<int, 2> GPU_Temp_result(Size1d, number_Of_Temp_result, Temp_result);
	GPU_Temp_result.discard_data();
	
	//concurrency::array_view<int, 2> GPU_Func(3,4, A)



	auto GPU_code = [=](concurrency::index<1> idx) restrict(amp)
	{
		int i = idx[0];
		for (int x = 0; x < number_Of_Functions; x++)
		{
			// finding which imput to use
			int input_1, input_2;
			// for input 1
			if (GPU_Func[x][0] > 0)
			{
				int temp_index = GPU_Func[x][0] - 1;
				input_1 = GPU_input[temp_index][i];
			}
			else
			{
				int temp_index = (GPU_Func[x][0] * -1) - 1;
				input_1 = GPU_Temp_result[i][temp_index];
			}
			// for input 2
			if (GPU_Func[x][2] > 0)
			{
				int temp_index = GPU_Func[x][2] - 1;
				input_2 = GPU_input[temp_index][i];
			}
			else
			{
				int temp_index = (GPU_Func[x][2] * -1) - 1;
				input_2 = GPU_Temp_result[i][temp_index];
			}
			int temp_result=0;

			// finding which operator to used
			if (GPU_Func[x][1] == 1 ) // (+)
			{
				temp_result = input_1 + input_2;
			}
			else if (GPU_Func[x][1] == 2) // (-)
			{
				temp_result = input_1 - input_2;
			}
			else if (GPU_Func[x][1] == 3) // (*)
			{
				temp_result = input_1 * input_2;
			}
			else if (GPU_Func[x][1] == 4) // (/)
			{
				temp_result = input_1 / input_2;
			}
			else
			{
				// some kind of error
			}
			// place temp_result in in place
			if (GPU_Func[x][3] == 0) // return the result
			{
				GPU_output[i] = temp_result;
			}
			else // place the result in temp_result_holder
			{
				int temp_index = (GPU_Func[x][3] * -1) - 1;
				GPU_Temp_result[i][temp_index] = temp_result;
			}
		}
	};

	concurrency::parallel_for_each(GPU_output.extent, GPU_code);
	GPU_output.synchronize();
	for (int i = 0; i < Size1d; i++)
	{
		std::cout << "A: " << A[i] << "	B: " << B[i] << "	c: " << C[i] << std::endl;
	}
	std::string str;
	std::getline(std::cin, str);

	return 0;
}

