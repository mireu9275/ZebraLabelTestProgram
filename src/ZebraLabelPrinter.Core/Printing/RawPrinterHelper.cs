using System;
using System.Runtime.InteropServices;

namespace ZebraLabelPrinter.Core.Printing
{
    internal static class RawPrinterHelper
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public class DOCINFOW
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string pDocName;
            [MarshalAs(UnmanagedType.LPWStr)] public string pOutputFile;
            [MarshalAs(UnmanagedType.LPWStr)] public string pDataType;
        }

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterW", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPWStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterW", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOW di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        public static bool SendBytes(string printerName, byte[] data, out string error)
        {
            error = null;
            IntPtr hPrinter;
            if (!OpenPrinter(printerName, out hPrinter, IntPtr.Zero))
            {
                error = "OpenPrinter failed: " + Marshal.GetLastWin32Error();
                return false;
            }

            try
            {
                var di = new DOCINFOW
                {
                    pDocName = "ZPL Label",
                    pDataType = "RAW"
                };

                if (!StartDocPrinter(hPrinter, 1, di))
                {
                    error = "StartDocPrinter failed: " + Marshal.GetLastWin32Error();
                    return false;
                }

                try
                {
                    if (!StartPagePrinter(hPrinter))
                    {
                        error = "StartPagePrinter failed: " + Marshal.GetLastWin32Error();
                        return false;
                    }

                    try
                    {
                        var pBytes = Marshal.AllocCoTaskMem(data.Length);
                        try
                        {
                            Marshal.Copy(data, 0, pBytes, data.Length);
                            int written;
                            if (!WritePrinter(hPrinter, pBytes, data.Length, out written))
                            {
                                error = "WritePrinter failed: " + Marshal.GetLastWin32Error();
                                return false;
                            }
                            return written == data.Length;
                        }
                        finally
                        {
                            Marshal.FreeCoTaskMem(pBytes);
                        }
                    }
                    finally
                    {
                        EndPagePrinter(hPrinter);
                    }
                }
                finally
                {
                    EndDocPrinter(hPrinter);
                }
            }
            finally
            {
                ClosePrinter(hPrinter);
            }
        }
    }
}
