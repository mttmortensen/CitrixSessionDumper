using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CitrixSessionDumper
{
    public static class CitrixInfoDump
    {
        // ==== WTS API IMPORTS ====
        [DllImport("Wtsapi32.dll")]
        static extern bool WTSQuerySessionInformation
        (
            IntPtr hServer,
            int sessionId,
            WTS_INFO_CLASS wtsInfoClass,
            out IntPtr ppBuffer,
            out int bytesReturned
        );

        [DllImport("Wtsapi32.dll")]
        static extern void WTSFreeMemory(IntPtr pMemory);

        // === Session Information Types ===
        enum WTS_INFO_CLASS
        {
            WTSUserName = 5,
            WTSDomainName = 7,
            WTSClientAddress = 14,
            WTSClientName = 10,
        }

        // === WTS Structure for IP Address ===
        [StructLayout(LayoutKind.Sequential)]
        struct WTS_CLIENT_ADDRESS
        {
            public int AddressFamily;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] Address;
        }
    }
}
