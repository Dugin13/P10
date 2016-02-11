
#include "cuda_runtime.h"
#include "device_launch_parameters.h"

#include <stdio.h>
#include <iostream>

__global__ void KernelFunc(const int **A, const int **B, int **C, const int Size)
{
}

int main()
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
	cudaDeviceProp prop;
	int CountOfDevice;

	cudaGetDeviceCount(&CountOfDevice);
	cudaGetDeviceProperties(&prop, 0);
	if (prop.maxThreadsPerBlock > Size1d)
	{
		dim3 BlockSize(Size, Size);

	}
	else
	{

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

    return 0;
}