// CPU 2D matrixmultiplikation Algoritme.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <iostream>

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
	for (x = 0; x < Size; x++)
	{
		for (y = 0; y < Size; y++)
		{
			//C[(x*Size) + y] = (x*Size) + y;
			for (z = 0; z < Size; z++)
			{
				C[x][y] += A[x][z] * B[z][y];
			}
		}
	}
#pragma endregion
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

