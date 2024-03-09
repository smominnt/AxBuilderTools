#pragma once

#ifdef AXFBXBUILDER_EXPORTS
#define AXFBXBUILDER_API __declspec(dllexport)
#else
#define AXFBXBUILDER_API __declspec(dllimport)
#endif

extern "C" AXFBXBUILDER_API int BuildMapFbx(char** ppData, int* pLength, const char* json, const char* assetPath, char* errorMessage, int errorMessageSize);

extern "C" __declspec(dllexport) void FreeMemory(char* pData)
{
    delete[] pData;
}