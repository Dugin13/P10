// CPU 2D matrixmultiplikation Algoritme.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <iostream>
#include <time.h>
const int Size = 3;
const int Size1d = Size*Size;
const int n = 10;
const int count = 10000;
const int MINI_SEC_IN_SEC = 1000;

int MA(int A[][Size], int B[][Size], int C[][Size], int Size, int Size1d)
{
	for (int x = 0; x < Size; x++)
	{
		for (int y = 0; y < Size; y++)
		{
			C[x][y] = 0;
			//C[(x*Size) + y] = (x*Size) + y;
			for (int z = 0; z < Size; z++)
			{
				C[x][y] += A[x][z] * B[z][y];
			}
		}
	}
	return 1;
}

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

double* Mark4(int A[][Size], int B[][Size], int C[][Size])
{

	double dummy = 0.0;
	double st = 0.0, sst = 0.0;
	for (int j = 0; j<n; j++) {
		clock_t t; // not sure if it is in right format
		t = clock();
		for (int i = 0; i<count; i++)
			dummy += MA(A, B, C, Size, Size1d);
		t = clock() - t;
		double time = ((double)t / CLOCKS_PER_SEC)*MINI_SEC_IN_SEC;
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
	const int Size = 3;
	const int Size1d = Size*Size;
	int A[Size][Size];
	int B[Size][Size];
	int C[Size][Size];
	int i, x, y, z;

	for (int i = 0; i < Size; i++)
	{
		for (int x = 0; x < Size; x++)
		{
			A[i][x] = 2;
			B[i][x] = 3;
			C[i][x] = 0;
		}
	}
#pragma region normal version
	//for (x = 0; x < Size; x++)
	//{
	//	for (y = 0; y < Size; y++)
	//	{
	//		C[x][y] = 0;
	//		//C[(x*Size) + y] = (x*Size) + y;
	//		for (z = 0; z < Size; z++)
	//		{
	//			C[x][y] += A[x][z] * B[z][y];
	//		}
	//	}
	//}
#pragma endregion

	double* result = Mark3(A, B, C);

	for (int i = 0; i < Size; i++)
	{
		for (int x = 0; x < Size; x++)
		{
			std::cout << "A: " << A[i][x] << "	B: " << B[i][x] << "	c: " << C[i][x] << std::endl;
		}
	}
	int STOP = 0;
	return 0;
}

