using System;
using System.Runtime.InteropServices;
using System.Text;

namespace AxBuilder
{
    public static class AxFbxBuilderDll
    {
        [DllImport("AxFBXBuilder.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int BuildMapFbx(out IntPtr ppData, out int pLength, string json, string assetPath, StringBuilder errorMessage, int errorMessageSize);

        [DllImport("AxFBXBuilder.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void FreeMemory(IntPtr pData);
    }
}
