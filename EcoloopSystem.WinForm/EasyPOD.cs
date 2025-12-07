// EasyPOD DLL P/Invoke 封裝
using System;
using System.Runtime.InteropServices;

namespace EcoloopSystem.WinForm
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MW_EasyPOD
    {
        public uint VID;
        public uint PID;
        public uint ReadTimeOut;
        public uint WriteTimeOut;
        public uint Handle;
        public uint FeatureReportSize;
        public uint InputReportSize;
        public uint OutputReportSize;
    }

    public class PODfuncs
    {
        [DllImport("EasyPOD.dll", CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern uint ConnectPOD(MW_EasyPOD* pEasyPOD, uint Index);

        [DllImport("EasyPOD.dll", CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern uint WriteData(MW_EasyPOD* pEasyPOD, byte[] lpBuffer, uint nNumberOfBytesToWrite, uint* lpNumberOfBytesWritten);

        [DllImport("EasyPOD.dll", CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern uint ReadData(MW_EasyPOD* pEasyPOD, byte[] lpBuffer, uint nNumberOfBytesToRead, uint* lpNumberOfBytesRead);

        [DllImport("EasyPOD.dll", CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern uint DisconnectPOD(MW_EasyPOD* pEasyPOD);

        [DllImport("EasyPOD.dll", CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern uint ClearPODBuffer(MW_EasyPOD* pEasyPOD);
    }
}
