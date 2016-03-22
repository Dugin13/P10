// AMP 2D matrixmultiplikation Algoritme.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <iostream>
#include <amp.h>

const int Size = 3;
const int Size1d = Size*Size;
const int n = 10;
const int count = 10000;
const int MINI_SEC_IN_SEC = 1000;


#pragma region parallel_for_each restrict amp
int MA(int A[][Size], int B[][Size], int C[][Size], int Size, int Size1d)
{
	// fandt løsning på hvordan man lavede 2d array i amp her:
	// https://stackoverflow.com/questions/8548016/how-to-use-a-2d-array-to-declare-an-array-view-or-array-object-in-c-amp

	int *p_A = &A[0][0];
	int *p_B = &B[0][0];
	int *p_C = &C[0][0];

	concurrency::array_view<int, 2> GPU_A(Size, Size, p_A);
	concurrency::array_view<int, 2> GPU_B(Size, Size, p_B);
	concurrency::array_view<int, 2> GPU_C(Size, Size, p_C);
	GPU_C.discard_data();

	concurrency::parallel_for_each(GPU_C.extent, [=](concurrency::index<2> i) restrict(amp)
	{
		int x = i[0];
		int y = i[1];
		GPU_C[y][x] = 0;
		for (int z = 0; z < Size; z++)
		{
			GPU_C[y][x] += GPU_A[y][z] * GPU_B[z][x];
		}
	});
	GPU_C.synchronize();
	return 1;
}
#pragma endregion

#pragma region Mark
double* Mark3(int A[][Size], int B[][Size], int C[][Size])
{
	double result[n];
	double dummy = 0.0;
	for (int j = 0; j<n; j++) {
		clock_t t; // not sure if it is in right format
		t = clock();
		for (int i = 0; i<count; i++)
		{
			dummy += MA(A, B, C, Size, Size1d);
		}
		t = clock() - t;
		double time = ((double)t / CLOCKS_PER_SEC)*MINI_SEC_IN_SEC;
		result[j] = time;
		std::cout << "time: " << time << " ms" << std::endl;
	}
	return result;
}
#pragma endregion



int _tmain(int argc, _TCHAR* argv[])
{
	int A[Size][Size];
	int B[Size][Size];
	int C[Size][Size];
	int D[Size][Size];
	int i, x, y, z;

	for (int i = 0; i < Size; i++)
	{
		for (int x = 0; x < Size; x++)
		{
			A[i][x] = 2;
			B[i][x] = 3;
			C[i][x] = 0;
			std::cout << "A: " << A[i][x] << "	B: " << B[i][x] << "	c: " << C[i][x] << std::endl;
		}
	}

	std::cout << "---------------------------" << std::endl;
#pragma region parallel_for_each restrict amp
	//// fandt løsning på hvordan man lavede 2d array i amp her:
	//// https://stackoverflow.com/questions/8548016/how-to-use-a-2d-array-to-declare-an-array-view-or-array-object-in-c-amp

	//int *p_A = &A[0][0];
	//int *p_B = &B[0][0];
	//int *p_C = &C[0][0];

	//concurrency::array_view<int, 2> GPU_A(Size, Size, p_A);
	//concurrency::array_view<int, 2> GPU_B(Size, Size, p_B);
	//concurrency::array_view<int, 2> GPU_C(Size, Size, p_C);
	//GPU_C.discard_data();

	//concurrency::parallel_for_each(GPU_C.extent, [=](concurrency::index<2> i) restrict(amp)
	//{
	//	int x = i[0];
	//	int y = i[1];
	//	for (int z = 0; z < Size; z++)
	//	{
	//		GPU_C[y][x] += GPU_A[y][z] * GPU_B[z][x];
	//	}
	//});
	//GPU_C.synchronize();

#pragma endregion


	double* result = Mark3(A, B, C);


	for (unsigned int i = 0; i < Size; i++)
	{
		for (unsigned int x = 0; x < Size; x++)
		{
			std::cout << "A: " << A[i][x] << "	B: " << B[i][x] << "	c: " << C[i][x] << std::endl;
		}
	}
	int STOP = 0;
	return 0;
}

