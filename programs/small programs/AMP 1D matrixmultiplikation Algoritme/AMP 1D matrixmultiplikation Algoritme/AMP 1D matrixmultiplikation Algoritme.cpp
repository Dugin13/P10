// AMP 1D matrixmultiplikation Algoritme.cpp : Defines the entry point for the console application.
//
#include "stdafx.h"
#include <iostream>
#include <amp.h>


int _tmain(int argc, _TCHAR* argv[])
{
	const int Size = 3;
	const int Size1d = Size*Size;
	int A[Size1d];
	int B[Size1d];
	int C[Size1d];
	int i, x, y, z;

	for (int i = 0; i < Size1d; i++)
	{
		A[i] = 2;
		B[i] = 3;
		C[i] = 0;
		std::cout << A[i] << " " << B[i] << std::endl;
	}

#pragma region parallel_for restrict cpu

	//concurrency::array_view<int, 1> GPU_A(Size1d, A);
	//concurrency::array_view<int, 1> GPU_B(Size1d, B);
	//concurrency::array_view<int, 1> GPU_C(Size1d, C);

	//concurrency::parallel_for(0, Size1d, [=](int i) restrict(cpu)
	//{
	//	int x = i / Size;
	//	int y = i % Size;
	//	//D[i] = (x*Size) + y;
	//	for (int z = 0; z < Size; z++)
	//	{
	//		GPU_C[i] += GPU_A[(x*Size) + z] * GPU_B[(z*Size) + y];
	//	}
	//});
	//GPU_C.synchronize();
	//int *result = GPU_C.data();

#pragma endregion

#pragma region parallel_for_each restrict amp

	concurrency::array_view<int, 1> GPU_A(Size1d, A);
	concurrency::array_view<int, 1> GPU_B(Size1d, B);
	concurrency::array_view<int, 1> GPU_C(Size1d, C);
	GPU_C.discard_data();

	concurrency::parallel_for_each(GPU_C.extent, [=](concurrency::index<1> i) restrict(amp)
	{
		int x = i[0] / Size;
		int y = i[0] % Size;
		//D[i] = (x*Size) + y;
		for (int z = 0; z < Size; z++)
		{
			GPU_C[i] += GPU_A[(x*Size) + z] * GPU_B[(z*Size) + y];
		}
	});
	GPU_C.synchronize();
	int *result = GPU_C.data();

#pragma endregion
	for (int i = 0; i < Size1d; i++)
	{
		std::cout << "A: " << A[i] << "	B: " << B[i] << "	c: " << C[i] << "	result: " << result[i] << std::endl;
	}


	int STOP = 0;
	return 0;

}

