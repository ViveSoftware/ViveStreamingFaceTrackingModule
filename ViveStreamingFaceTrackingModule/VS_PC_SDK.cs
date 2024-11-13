using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ViveStreamingFaceTrackingModule
{
    public static class VS_PC_SDK
    {
        [DllImport("VS_PC_SDK.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int VS_Version();

        [DllImport("VS_PC_SDK.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern string VS_SDKVersion();

        [DllImport("VS_PC_SDK.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int VS_Init();

        [DllImport("VS_PC_SDK.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int VS_Release();

        [DllImport("VS_PC_SDK.dll", CallingConvention = CallingConvention.Cdecl)]        
        public static extern void VS_SetCallbackFunction(VS_StatusUpdateCallback pStatusUpdateCallback, VS_SettingChangeCallback pSettingChangeCallback, VS_LoggerCallback pDebugLogCallback);

        [DllImport("VS_PC_SDK.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool VS_StartFaceTracking();
        
        [DllImport("VS_PC_SDK.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool VS_StopFaceTracking();

        public delegate void VS_StatusUpdateCallback([MarshalAs(UnmanagedType.LPWStr)] string oldStatus, [MarshalAs(UnmanagedType.LPWStr)] string newStatus);
        public delegate void VS_SettingChangeCallback([MarshalAs(UnmanagedType.LPWStr)] string setting);
        public delegate void VS_LoggerCallback([MarshalAs(UnmanagedType.LPWStr)] string logMsg);
    }
}
