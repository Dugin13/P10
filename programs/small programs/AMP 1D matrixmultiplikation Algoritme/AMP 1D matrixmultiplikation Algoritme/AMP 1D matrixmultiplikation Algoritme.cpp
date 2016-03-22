// AMP 1D matrixmultiplikation Algoritme.cpp : Defines the entry point for the console application.
//
#include "stdafx.h"
#include <iostream>
#include <amp.h>
#include <time.h>
const int Size = 3;
const int Size1d = Size*Size;
const int n = 10;
const int count = 10000;
const int MINI_SEC_IN_SEC = 1000;
#pragma region parallel_for_each restrict amp
int MA(int* A, int* B, int* C, int Size, int Size1d)
{
	int GPU_Size = Size;

	concurrency::array_view<int, 1> GPU_A(Size1d, A);
	concurrency::array_view<int, 1> GPU_B(Size1d, B);
	concurrency::array_view<int, 1> GPU_C(Size1d, C);
	GPU_C.discard_data();

	concurrency::parallel_for_each(GPU_C.extent, [=](concurrency::index<1> i) restrict(amp)
	{
		GPU_C[i] = 0;
		int x = i[0] / GPU_Size;
		int y = i[0] % GPU_Size;
		//D[i] = (x*Size) + y;
		for (int z = 0; z < GPU_Size; z++)
		{
			GPU_C[i] += GPU_A[(x*GPU_Size) + z] * GPU_B[(z*GPU_Size) + y];
		}
	});
	GPU_C.synchronize();
	//int *result = GPU_C.data();
	return 1;
}
#pragma endregion

#pragma region Mark
double* Mark3(int* A, int* B, int* C)
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

	//concurrency::array_view<int, 1> GPU_A(Size1d, A);
	//concurrency::array_view<int, 1> GPU_B(Size1d, B);
	//concurrency::array_view<int, 1> GPU_C(Size1d, C);
	//GPU_C.discard_data();

	//concurrency::parallel_for_each(GPU_C.extent, [=](concurrency::index<1> i) restrict(amp)
	//{
	//	int x = i[0] / Size;
	//	int y = i[0] % Size;
	//	//D[i] = (x*Size) + y;
	//	for (int z = 0; z < Size; z++)
	//	{
	//		GPU_C[i] += GPU_A[(x*Size) + z] * GPU_B[(z*Size) + y];
	//	}
	//});
	//GPU_C.synchronize();
	//int *result = GPU_C.data();

#pragma endregion


	double* result = Mark3(A, B, C);

	for (int i = 0; i < Size1d; i++)
	{
		std::cout << "A: " << A[i] << "	B: " << B[i] << "	c: " << C[i] << std::endl;
	}


	int STOP = 0;
	return 0;

}

