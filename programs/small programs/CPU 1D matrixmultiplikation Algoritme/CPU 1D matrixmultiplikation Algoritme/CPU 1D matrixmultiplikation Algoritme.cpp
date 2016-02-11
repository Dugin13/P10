// CPU 1D matrixmultiplikation Algoritme.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <iostream>

int _tmain(int argc, _TCHAR* argv[])
{
	const int Size = 3;
	const int Size1d = Size*Size;
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
	for (x = 0; x < Size; x++)
	{
		for (y = 0; y < Size; y++)
		{
			//C[(x*Size) + y] = (x*Size) + y;
			for (z = 0; z < Size; z++)
			{
				C[(x *Size) + y] += A[(x*Size) + z] * B[(z*Size) + y];
			}
		}
	}
#pragma endregion

#pragma region D1 version
	for (i = 0; i < Size1d; i++)
	{
		x = i / Size;
		y = i % Size;
		//D[i] = (x*Size) + y;
		for (z = 0; z < Size; z++)
		{
			D[i] += A[(x*Size) + z] * B[(z*Size) + y];
		}

	}
#pragma endregion
	for (int i = 0; i < Size1d; i++)
	{
		std::cout << "A: " << A[i] << "	B: " << B[i] << "	c: " << C[i] << "	D: " << D[i] << std::endl;
	}


	int STOP = 0;
	return 0;

}