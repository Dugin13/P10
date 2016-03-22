// CPU 1D matrixmultiplikation Algoritme.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <iostream>
#include <time.h>

const int Size = 3;
const int Size1d = Size*Size;
const int n = 10;
const int count = 10000;
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
double* Mark3_Normal(int* A, int* B, int* C)
{
	double result[n];
	double dummy = 0.0;
	for (int j = 0; j<n; j++) {
		clock_t t; // not sure if it is in right format
		t = clock();
		for (int i = 0; i<count; i++)
		{
			dummy += MA_Normal(A, B, C, Size, Size1d);
		}
		t = clock() - t;
		double time = (double)t;
		result[j] = time;
		std::cout << "time: " << time << " ms" << std::endl;
	}
	return result;
}
#pragma region Mark
double* Mark3_1D(int* A, int* B, int* C)
{
	double result[n];
	double dummy = 0.0;
	for (int j = 0; j<n; j++) {
		clock_t t; // not sure if it is in right format
		t = clock();
		for (int i = 0; i<count; i++)
		{
			dummy += MA_1D_Version(A, B, C, Size, Size1d);
		}
		t = clock() - t;
		double time = ((double)t / CLOCKS_PER_SEC)*MINI_SEC_IN_SEC;
		result[j] = time;
		std::cout << "time: " << time << " ms" << std::endl;
	}
	return result;
}
#pragma endregion
#pragma endregion


int _tmain(int argc, _TCHAR* argv[])
{
	int A[Size1d];
	int B[Size1d];
	int C[Size1d];
	int D[Size1d];
	int i, x, y, z;

	for (int i = 0; i < Size1d; i++)
	{
		A[i] = 2;
		B[i] = 3;
		C[i] = 0;
		D[i] = 0;
		std::cout << A[i] << " " << B[i] << std::endl;
	}
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

	double* result_Normal = Mark3_Normal(A, B, C);
	double* result_1D = Mark3_1D(A, B, D);


	for (int i = 0; i < Size1d; i++)
	{
		std::cout << "A: " << A[i] << "	B: " << B[i] << "	c: " << C[i] << "	D: " << D[i] << std::endl;
	}


	int STOP = 0;
	return 0;

}