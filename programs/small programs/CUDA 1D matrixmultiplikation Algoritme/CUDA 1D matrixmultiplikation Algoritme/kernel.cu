
#include "cuda_runtime.h"
#include "device_launch_parameters.h"

#include <stdio.h>
#include <iostream>

__global__ void KernelFunc(const int *A, const int *B, int *C, const int Size)
{
    int i = threadIdx.x;
	int x = i / Size;
	int y = i % Size;
	//D[i] = (x*Size) + y;
	for (int z = 0; z < Size; z++)
	{
		C[i] += A[(x*Size) + z] * B[(z*Size) + y];
	}
}



int main()
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
#pragma region D1 version
	int *GPU_A, *GPU_B, *GPU_C;

	cudaMalloc((void**)&GPU_A, Size1d * sizeof(int));
	cudaMalloc((void**)&GPU_B, Size1d * sizeof(int));
	cudaMalloc((void**)&GPU_C, Size1d * sizeof(int));

	cudaMemcpy(GPU_A, A, Size1d * sizeof(int), cudaMemcpyHostToDevice);
	cudaMemcpy(GPU_B, B, Size1d * sizeof(int), cudaMemcpyHostToDevice);

	// TODO: lab en bedre måde at gøre dette på
	int threadsPerBlock = Size1d; 
	int blocksPerGrid = 1;

	KernelFunc <<<blocksPerGrid, threadsPerBlock >>>(GPU_A, GPU_B, GPU_C, Size);

	cudaMemcpy(C, GPU_C, Size1d * sizeof(int), cudaMemcpyDeviceToHost);

	cudaFree(GPU_A);
	cudaFree(GPU_B);
	cudaFree(GPU_C);

#pragma endregion
	for (int i = 0; i < Size1d; i++)
	{
		std::cout << "A: " << A[i] << "	B: " << B[i] << "	c: " << C[i] << std::endl;
	}


	int STOP = 0;
    return 0;
}