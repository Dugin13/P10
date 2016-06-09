// AMP 1D matrixmultiplikation Algoritme.cpp : Defines the entry point for the console application.
//
#include "stdafx.h"
#include <iostream>
#include <amp.h>
#include <time.h>
#include <math.h>
#include <fstream> 

const int Millisecond_IN_SEC = 1000;
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
double* Mark3(int* A, int* B, int* C, int Size, int Size1d, int n, int count)
{
	double* result = new double[n];
	double dummy = 0.0;
	for (int j = 0; j<n; j++) {
		clock_t t; // not sure if it is in right format
		t = clock();
		for (int i = 0; i<count; i++)
		{
			dummy += MA(A, B, C, Size, Size1d);
		}
		t = clock() - t;
		double time = ((double)t / CLOCKS_PER_SEC) * 10;
		result[j] = time;
		std::cout << "time: " << time << " ms" << std::endl;
	}
	return result;
}

double* Mark4(int* A, int* B, int* C,int Size,int Size1d,int n,int count)
{
	
	double dummy = 0.0;
	double st = 0.0, sst = 0.0;
	for (int j = 0; j<n; j++) {
		clock_t t; // not sure if it is in right format
		t = clock();
		for (int i = 0; i<count; i++)
			dummy += MA(A, B, C, Size, Size1d);
		t = clock() - t;
		double time = ((double)t / CLOCKS_PER_SEC)*10;
		st += time;
		sst += time * time;
	}
	double mean = st / n, sdev = sqrt((sst - mean*mean*n) / (n - 1));
	double result[2] = { mean, sdev };
	return result;
}


#pragma endregion


int _tmain(int argc, _TCHAR* argv[])
{
	const int testSize[] { 5, 10, 20, 50, 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000 };
	double result[(sizeof(testSize) / sizeof(*testSize))][3];
	int i, n = 10, count = 100;

	for (int i = 0; i < (sizeof(testSize) / sizeof(*testSize)); i++)
	{
		int Size = testSize[i];
		int Size1d = Size * Size;
		int* A = new int[Size1d];
		int* B = new int[Size1d];
		int* C = new int[Size1d];
		for (int x = 0; x < Size1d; x++)
		{
			A[x] = 2;
			B[x] = 3;
		}
		std::cout << testSize[i]<< " starting" << std::endl;
		double* Mark4_time = Mark4(A, B, C, Size, Size1d, n, count);
		result[i][0] = Size;
		result[i][1] = Mark4_time[0];
		result[i][2] = Mark4_time[1];
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

	std::ofstream outfile("AMP_1D_MA_in_C++.txt");
	outfile << "AMP 1D MA in C++  mean, sdev" << std::endl;

	for (int i = 0; i < (sizeof(testSize) / sizeof(*testSize)); i++)
	{
		outfile << "size: " << result[i][0] << " time: " << result[i][1] << " , " << result[i][2] << std::endl;
	}


	outfile.close();



	int STOP = 0;
	return 0;

}

