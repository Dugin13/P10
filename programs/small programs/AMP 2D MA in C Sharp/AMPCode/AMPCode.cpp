// AMPCode.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include <amp.h>

extern "C" __declspec (dllexport) void _stdcall GPU_part_for_1D(int* A, int* B, int* C, int Size, int Size1d)
{

	concurrency::array_view<int, 2> GPU_A(Size, Size, A);
	concurrency::array_view<int, 2> GPU_B(Size, Size, B);
	concurrency::array_view<int, 2> GPU_C(Size, Size, C);
	GPU_C.discard_data();

	concurrency::parallel_for_each(GPU_C.extent, [=](concurrency::index<2> i) restrict(amp)
	{
		int x = i[0];
		int y = i[1];
		GPU_C[y][x] = 0;
		for (int z = 0; z < Size; z++)
		{
			GPU_C[y][x] += GPU_A[y][z] * GPU_B[z][x];
		}
	});
	GPU_C.synchronize();
}