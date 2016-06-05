// CPU 1D matrixmultiplikation Algoritme.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <iostream>
#include <time.h>
#include <fstream>

const int MINI_SEC_IN_SEC = 1000;

int MA_Normal(int* A, int* B, int* C, int Size, int Size1d)
{
	for (int x = 0; x < Size; x++)
	{
		for (int y = 0; y < Size; y++)
		{
			//C[(x*Size) + y] = (x*Size) + y;
			C[(x *Size) + y] = 0;
			for (int z = 0; z < Size; z++)
			{
				C[(x *Size) + y] += A[(x*Size) + z] * B[(z*Size) + y];
			}
		}
	}
	return 1;
}
int MA_1D_Version(int* A, int* B, int* D, int Size, int Size1d)
{
	for (int i = 0; i < Size1d; i++)
	{
		int x = i / Size;
		int y = i % Size;
		//D[i] = (x*Size) + y;
		D[i] = 0;
		for (int z = 0; z < Size; z++)
		{
			D[i] += A[(x*Size) + z] * B[(z*Size) + y];
		}

	}
	return 1;
}
#pragma region Mark
double* Mark3_Normal(int* A, int* B, int* C, int Size, int Size1d, int n, int count)
{
	double* result = new double[n];
	double dummy = 0.0;
	for (int j = 0; j<n; j++) {
		clock_t t; // not sure if it is in right format
		t = clock();
		for (int i = 0; i<count; i++)
		{
			dummy += MA_Normal(A, B, C, Size, Size1d);
		}
		t = clock() - t;
		double time = ((double)t / CLOCKS_PER_SEC) * 10;
		result[j] = time;
		std::cout << "time: " << time << " ms" << std::endl;
	}
	return result;
}

double* Mark4_Normal(int* A, int* B, int* C, int Size, int Size1d, int n, int count)
{

	double dummy = 0.0;
	double st = 0.0, sst = 0.0;
	for (int j = 0; j<n; j++) {
		clock_t t; // not sure if it is in right format
		t = clock();
		for (int i = 0; i<count; i++)
			dummy += MA_Normal(A, B, C, Size, Size1d);
		t = clock() - t;
		double time = ((double)t / CLOCKS_PER_SEC)*10;
		st += time;
		sst += time * time;
	}
	double mean = st / n, sdev = sqrt((sst - mean*mean*n) / (n - 1));
	double result[2] = { mean, sdev };
	return result;
}

double* Mark3_1D(int* A, int* B, int* C, int Size, int Size1d, int n, int count)
{
	double* result = new double[n];
	double dummy = 0.0;
	for (int j = 0; j<n; j++) {
		clock_t t; // not sure if it is in right format
		t = clock();
		for (int i = 0; i<count; i++)
		{
			dummy += MA_1D_Version(A, B, C, Size, Size1d);
		}
		t = clock() - t;
		double time = ((double)t / CLOCKS_PER_SEC)*10;
		result[j] = time;
		std::cout << "time: " << time << " ms" << std::endl;
	}
	return result;
}

double* Mark4_1D(int* A, int* B, int* C, int Size, int Size1d, int n, int count)
{

	double dummy = 0.0;
	double st = 0.0, sst = 0.0;
	for (int j = 0; j<n; j++) {
		clock_t t; // not sure if it is in right format
		t = clock();
		for (int i = 0; i<count; i++)
			dummy += MA_1D_Version(A, B, C, Size, Size1d);
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
#pragma endregion


int _tmain(int argc, _TCHAR* argv[])
{
	//int A[Size1d];
	//int B[Size1d];
	//int C[Size1d];
	//int D[Size1d];
	//int i, x, y, z;
	//for (int i = 0; i < Size1d; i++)
	//{
	//	A[i] = 2;
	//	B[i] = 3;
	//	C[i] = 0;
	//	D[i] = 0;
	//	std::cout << A[i] << " " << B[i] << std::endl;
	//}
#pragma region normal version
	//for (x = 0; x < Size; x++)
	//{
	//	for (y = 0; y < Size; y++)
	//	{
	//		//C[(x*Size) + y] = (x*Size) + y;
	//		for (z = 0; z < Size; z++)
	//		{
	//			C[(x *Size) + y] += A[(x*Size) + z] * B[(z*Size) + y];
	//		}
	//	}
	//}
#pragma endregion

#pragma region D1 version
	//for (i = 0; i < Size1d; i++)
	//{
	//	x = i / Size;
	//	y = i % Size;
	//	//D[i] = (x*Size) + y;
	//	for (z = 0; z < Size; z++)
	//	{
	//		D[i] += A[(x*Size) + z] * B[(z*Size) + y];
	//	}

	//}
#pragma endregion

	/*double* result_Normal = Mark4_Normal(A, B, C);
	double* result_1D = Mark4_1D(A, B, D);
	double normal1 = result_Normal[0];
	double normal2 = result_Normal[1];

	double d11 = result_1D[0];
	double d12 = result_1D[1];

	for (int i = 0; i < Size1d; i++)
	{
		std::cout << "A: " << A[i] << "	B: " << B[i] << "	c: " << C[i] << "	D: " << D[i] << std::endl;
	}*/

	const int testSize[] { 5, 10, 20, 50, 100, 200, 300, 400, 500 };
	double result_normal[(sizeof(testSize) / sizeof(*testSize))][3];
	double result_1D[(sizeof(testSize) / sizeof(*testSize))][3];
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
		std::cout << testSize[i] << " starting" << std::endl;
		double* Mark4_time_normal = Mark4_Normal(A, B, C, Size, Size1d, n, count);
		result_normal[i][0] = Size;
		result_normal[i][1] = Mark4_time_normal[0];
		result_normal[i][2] = Mark4_time_normal[1];

		double* Mark4_time_1D = Mark4_1D(A, B, C, Size, Size1d, n, count);
		result_1D[i][0] = Size;
		result_1D[i][1] = Mark4_time_1D[0];
		result_1D[i][2] = Mark4_time_1D[1];

	}

	std::ofstream outfile("C++_CPU_1D_MA.txt");
	outfile << "C++ CPU normal 1D MA  mean, sdev" << std::endl;

	for (int i = 0; i < (sizeof(testSize) / sizeof(*testSize)); i++)
	{
		outfile << "size: " << result_normal[i][0] << " time: " << result_normal[i][1] << " , " << result_normal[i][2] << std::endl;
	}

	outfile << std::endl << std::endl << "C++ CPU 1D 1D MA  mean, sdev" << std::endl;

	for (int i = 0; i < (sizeof(testSize) / sizeof(*testSize)); i++)
	{
		outfile << "size: " << result_1D[i][0] << " time: " << result_1D[i][1] << " , " << result_1D[i][2] << std::endl;
	}


	outfile.close();

	int STOP = 0;
	return 0;

}