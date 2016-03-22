
#include "cuda_runtime.h"
#include "device_launch_parameters.h"

#include <stdio.h>
#include <iostream>
#include <time.h>

const int Size = 3;
const int Size1d = Size*Size;
const int n = 10;
const int count = 10000;
const int MINI_SEC_IN_SEC = 1000;

__global__ void KernelFunc(const int *A, const int *B, int *C, const int Size)
{
    int i = threadIdx.x+ blockIdx.x*blockDim.x;
	int x = i / Size;
	int y = i % Size;

	C[i] = 0;
	//D[i] = (x*Size) + y;
	for (int z = 0; z < Size; z++)
	{
		C[i] += A[(x*Size) + z] * B[(z*Size) + y];
	}
}
int MA(int* A, int* B, int* C, int Size, int Size1d, cudaDeviceProp GPU_prop)
{

	int *GPU_A, *GPU_B, *GPU_C;

	cudaMalloc((void**)&GPU_A, Size1d * sizeof(int));
	cudaMalloc((void**)&GPU_B, Size1d * sizeof(int));
	cudaMalloc((void**)&GPU_C, Size1d * sizeof(int));

	cudaMemcpy(GPU_A, A, Size1d * sizeof(int), cudaMemcpyHostToDevice);
	cudaMemcpy(GPU_B, B, Size1d * sizeof(int), cudaMemcpyHostToDevice);

	// TODO: lab en bedre måde at gøre dette på

	int threadsPerBlock = 0;
	int blocksPerGrid = 0;

	if (Size1d< GPU_prop.maxThreadsPerBlock)
	{
		threadsPerBlock = Size1d;
		blocksPerGrid = 1;
	}
	else
	{
		threadsPerBlock = GPU_prop.maxThreadsPerBlock;
		blocksPerGrid = (Size1d/GPU_prop.maxThreadsPerBlock)+1;
	}

	KernelFunc << <blocksPerGrid, threadsPerBlock >> >(GPU_A, GPU_B, GPU_C, Size);

	cudaMemcpy(C, GPU_C, Size1d * sizeof(int), cudaMemcpyDeviceToHost);

	cudaFree(GPU_A);
	cudaFree(GPU_B);
	cudaFree(GPU_C);
	return 1;
}

#pragma region Mark
double* Mark3(int* A, int* B, int* C, cudaDeviceProp GPU_prop)
{
	double result[n];
	double dummy = 0.0;
	for (int j = 0; j<n; j++) {
		clock_t t; // not sure if it is in right format
		t = clock();
		for (int i = 0; i<count; i++)
		{
			dummy += MA(A, B, C, Size, Size1d, GPU_prop);
		}
		t = clock() - t;
		double time = ((double)t / CLOCKS_PER_SEC)*MINI_SEC_IN_SEC;
		result[j] = time;
		std::cout << "time: " << time << " ms" << std::endl;
	}
	return result;
}
#pragma endregion


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
	//int *GPU_A, *GPU_B, *GPU_C;

	//cudaMalloc((void**)&GPU_A, Size1d * sizeof(int));
	//cudaMalloc((void**)&GPU_B, Size1d * sizeof(int));
	//cudaMalloc((void**)&GPU_C, Size1d * sizeof(int));

	//cudaMemcpy(GPU_A, A, Size1d * sizeof(int), cudaMemcpyHostToDevice);
	//cudaMemcpy(GPU_B, B, Size1d * sizeof(int), cudaMemcpyHostToDevice);

	//// TODO: lab en bedre måde at gøre dette på
	//int threadsPerBlock = Size1d; 
	//int blocksPerGrid = 1;

	//KernelFunc <<<blocksPerGrid, threadsPerBlock >>>(GPU_A, GPU_B, GPU_C, Size);

	//cudaMemcpy(C, GPU_C, Size1d * sizeof(int), cudaMemcpyDeviceToHost);

	//cudaFree(GPU_A);
	//cudaFree(GPU_B);
	//cudaFree(GPU_C);

#pragma endregion
	cudaDeviceProp GPU_prop;
	cudaGetDeviceProperties(&GPU_prop, 0);
	double* result = Mark3(A, B, C, GPU_prop);

	for (int i = 0; i < Size1d; i++)
	{
		std::cout << "A: " << A[i] << "	B: " << B[i] << "	c: " << C[i] << std::endl;
	}


	int STOP = 0;
    return 0;
}