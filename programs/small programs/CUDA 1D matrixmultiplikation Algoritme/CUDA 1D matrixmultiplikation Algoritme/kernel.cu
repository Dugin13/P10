
#include "cuda_runtime.h"
#include "device_launch_parameters.h"

#include <stdio.h>
#include <iostream>
#include <time.h>
#include <fstream> 

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
int MA(int* A, int* B, int* C, int Size, int Size1d, int max_threadsPerBlock)
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

	if (Size1d< max_threadsPerBlock)
	{
		threadsPerBlock = Size1d;
		blocksPerGrid = 1;
	}
	else
	{
		threadsPerBlock = max_threadsPerBlock;
		blocksPerGrid = (Size1d / max_threadsPerBlock) + 1;
	}

	KernelFunc << <blocksPerGrid, threadsPerBlock >> >(GPU_A, GPU_B, GPU_C, Size);

	cudaMemcpy(C, GPU_C, Size1d * sizeof(int), cudaMemcpyDeviceToHost);

	cudaFree(GPU_A);
	cudaFree(GPU_B);
	cudaFree(GPU_C);
	return 1;
}

#pragma region Mark
double* Mark3(int* A, int* B, int* C, int max_threadsPerBlock, int Size, int Size1d, int n, int count)
{
	double* result = new double[n];
	double dummy = 0.0;
	for (int j = 0; j<n; j++) {
		clock_t t; // not sure if it is in right format
		t = clock();
		for (int i = 0; i<count; i++)
		{
			dummy += MA(A, B, C, Size, Size1d, max_threadsPerBlock);
		}
		t = clock() - t;
		double time = ((double)t / CLOCKS_PER_SEC)*10;
		result[j] = time;
		std::cout << "time: " << time << " ms" << std::endl;
	}
	return result;
}

double* Mark4(int* A, int* B, int* C, int max_threadsPerBlock, int Size, int Size1d, int n, int count)
{

	double dummy = 0.0;
	double st = 0.0, sst = 0.0;
	for (int j = 0; j<n; j++) {
		clock_t t; // not sure if it is in right format
		t = clock();
		for (int i = 0; i<count; i++)
			dummy += MA(A, B, C, Size, Size1d, max_threadsPerBlock);
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


int main()
{
	//const int Size = 3;
	//const int Size1d = Size*Size;
	//int A[Size1d];
	//int B[Size1d];
	//int C[Size1d];
	//int i, x, y, z;
	//cudaDeviceProp GPU_prop;
	//cudaGetDeviceProperties(&GPU_prop, 0);

	//for (int i = 0; i < Size1d; i++)
	//{
	//	A[i] = 2;
	//	B[i] = 3;
	//	C[i] = 0;
	//	std::cout << A[i] << " " << B[i] << std::endl;
	//}
	const int testSize[] { 5, 10, 20, 50, 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000 };
	double result[(sizeof(testSize) / sizeof(*testSize))][3];
	int i, n = 10, count = 100;
	cudaDeviceProp GPU_prop;
	cudaGetDeviceProperties(&GPU_prop, 0);
	int max_threadsPerBlock = GPU_prop.maxThreadsPerBlock;

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
		double* Mark4_time = Mark4(A, B, C, max_threadsPerBlock, Size, Size1d, n, count);
		result[i][0] = Size;
		result[i][1] = Mark4_time[0];
		result[i][2] = Mark4_time[1];
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

	//double* result = Mark3(A, B, C, GPU_prop);
	std::ofstream outfile("CUDA_1D_MA_in_C++.txt");
	outfile << "CUDA 1D MA in C++  mean, sdev" << std::endl;

	for (int i = 0; i < (sizeof(testSize) / sizeof(*testSize)); i++)
	{
		outfile << "size: " << result[i][0] << " time: " << result[i][1] << " , " << result[i][2] << std::endl;
	}


	outfile.close();

	int STOP = 0;
    return 0;
}