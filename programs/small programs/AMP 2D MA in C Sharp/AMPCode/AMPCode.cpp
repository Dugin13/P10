// AMPCode.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include <amp.h>

extern "C" __declspec (dllexport) void _stdcall GPU_part_for_1D(int* A, int* B, int* C, int Size, int Size1d)
{
	// Create a view over the data on the CPU
	concurrency::array_view<int, 1> GPU_A(Size1d, &A[0]);
	concurrency::array_view<int, 1> GPU_B(Size1d, &B[0]);
	concurrency::array_view<int, 1> GPU_C(Size1d, &C[0]);
	GPU_C.discard_data();
	int GPU_Size = Size;
	int GPU_Size1d = Size1d;

	concurrency::parallel_for_each(GPU_C.extent, [=](concurrency::index<1> i) restrict(amp)
	{
		int x = i[0] / GPU_Size;
		int y = i[0] % GPU_Size;
		//D[i] = (x*Size) + y;
		for (int z = 0; z < GPU_Size; z++)
		{
			GPU_C[i] += GPU_A[(x*GPU_Size) + z] * GPU_B[(z*GPU_Size) + y];
		}
	});
	GPU_C.synchronize();
}