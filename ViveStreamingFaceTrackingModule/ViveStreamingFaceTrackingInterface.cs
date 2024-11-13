using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using VRCFaceTracking;
using VRCFaceTracking.Core.Library;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Params.Expressions;
using VRCFaceTracking.Core.Types;
using Microsoft.VisualBasic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using static ViveStreamingFaceTrackingModule.FaceData;
using System.Numerics;
using System.Xml.Linq;

namespace ViveStreamingFaceTrackingModule
{
    public class ViveStreamingFaceTrackingModule : ExtTrackingModule
    {
        private const string VERSION_STRING = "v1.7";
        private const string HMD_NAME = "2004";
        private const string VS_SERVER_VERSION = "2007";
        private const string VS_SERVER_STATE = "2105";
        private const string EYE_DATA = "5001";
        private const string LIP_DATA = "5002";

        private static uint MAX_RETRY_COUNT = 5;
        private static string EyeData = "";
        private static string LipData = "";
        private static Boolean NewEyeData = false;
        private static Boolean NewLipData = false;

        private static Boolean HasClientConnection = false;
        private static Boolean EyeTrackerInited = false;
        private static Boolean LipTrackerInited = false;
        private static int FACE_TRACKER_DATA_TIMEOUT_MS = 3000; // For init

        private static List<string> mVSMessageQueue = new List<string>();
        private static void OnVSStatusUpdate(string status, string value)
        {
            string strMsg = "";
            switch (status)
            {
                case "2498":
                case EYE_DATA:
                    if (value.Length > 0)
                    {
                        EyeData = value;
                        NewEyeData = true;
                        EyeTrackerInited = true;
                        //mVSMessageQueue.Add($" EYE_DATA {EyeData}");
                    }
                    break;
                case "2499":
                case LIP_DATA:
                    if (value.Length > 0)
                    {
                        LipData = value;
                        NewLipData = true;
                        LipTrackerInited = true;
                        //mVSMessageQueue.Add($" LIP_DATA {LipData}");
                    }
                    break;
                case VS_SERVER_VERSION:
                    mVSMessageQueue.Add($"Vive Streaming Server v{value} connected.");
                    break;
                case HMD_NAME: 
                    if (value.Length > 0)
                    {
                        mVSMessageQueue.Add($"VS_PC_SDK: HMD: {value}");
                    }
                    break;
                case VS_SERVER_STATE:
                    //mVSMessageQueue.Add($"VS_PC_SDK: VS_SERVER_STATE : {value}");
                    //HasClientConnection = true; //debug
                    // 0: Streaming
                    // 1: No frame received from SteamVR
                    // 2: Standby (waiting for VBS client connect)
                    // 3: VBS Driver not found
                    // 4: SteamVR not running
                    if (value == "0")
                    {
                        if (!HasClientConnection)
                        {
                            strMsg = "HMD connected.";
                            HasClientConnection = true;
                            mVSMessageQueue.Add($"VS_PC_SDK: {strMsg}");
                        } 
                        // else { // ignore }
                    } 
                    else if (value == "2")
                    {
                        if (HasClientConnection)
                        {
                            strMsg = "HMD disconnected.";
                            HasClientConnection = false;
                            mVSMessageQueue.Add($"VS_PC_SDK: {strMsg}");
                        }
                        // else { // ignore }
                    } 
                    else
                    {
                        //mVSMessageQueue.Add($"VS_PC_SDK: {status} : {value}");
                    }
                    break;
                default:                    
                    //mVSMessageQueue.Add($"VS_PC_SDK: {status} : {value}");
                    break;
            }
        }

        private static void OnVSSettingChange(string setting)
        {
            // mVSMessageQueue.Add($"VS_PC_SDK: {setting}");
        }
        private static void OnVSWriteLog(string logMsg)
        {
            mVSMessageQueue.Add($"VS_PC_SDK: {logMsg}");
        }

        private static VS_PC_SDK.VS_StatusUpdateCallback mVSStatusUpdateCallback = OnVSStatusUpdate;
        private static VS_PC_SDK.VS_SettingChangeCallback mVSSettingChangeCallback = OnVSSettingChange;
        private static VS_PC_SDK.VS_LoggerCallback mVSLogger = OnVSWriteLog;

        // Kernel32 SetDllDirectory
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern bool SetDllDirectory(string lpPathName);

        // What your interface is able to send as tracking data.
        public override (bool SupportsEye, bool SupportsExpression) Supported => (true, true);

        // This is the first function ran by VRCFaceTracking. Make sure to completely initialize 
        // your tracking interface or the data to be accepted by VRCFaceTracking here. This will let 
        // VRCFaceTracking know what data is available to be sent from your tracking interface at initialization.
        public override (bool eyeSuccess, bool expressionSuccess) Initialize(bool eyeAvailable, bool expressionAvailable)
        {
            var state = (eyeAvailable, expressionAvailable);

            ModuleInformation.Name = "ViveStreamingFaceTracking";

            // Display image
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream("ViveStreamingFaceTrackingModule.resource.VBS_XRE.png");
            ModuleInformation.StaticImages =
                stream != null ? new List<Stream> { stream } : ModuleInformation.StaticImages;

            // DLL directory
            string? dllDirectory = Path.GetDirectoryName(assembly.Location);
            if (dllDirectory == null)
            {
                Logger.LogInformation($"Could not found DLL directory.");
                return state;
            }
            // Logger.LogInformation($"Loading VS_PC_SDK DLL from: {dllDirectory}");
            SetDllDirectory(Path.Combine(dllDirectory, "Libs"));

            // Set callback before VS_Init() to collect init fail log
            VS_PC_SDK.VS_SetCallbackFunction(mVSStatusUpdateCallback, mVSSettingChangeCallback, mVSLogger);
            var intResult = VS_PC_SDK.VS_Init();
            if (intResult == 0)
            {
                Logger.LogInformation($"Init VRCFT ViveStreaming Module {VERSION_STRING}");
                Logger.LogInformation($"Init ViveStreamingSDK {VS_PC_SDK.VS_Version()} success.");
                Logger.LogInformation($"Waiting for server.");
                // Logger.LogInformation($"ViveStreaming SDK v{VS_PC_SDK.VS_SDKVersion()}"); // crash
            } 
            else 
            {
                Logger.LogInformation($"Init ViveStreamingSDK failed({intResult}).");
                state.expressionAvailable = false;
                state.eyeAvailable = false;
                // VS_PC_SDK.VS_Release();
            }

            Logger.LogInformation($"Log throttle period: 60 seconds.");
            return state;
        }

        // Polls data from the tracking interface.
        // VRCFaceTracking will run this function in a separate thread;
        public override void Update()
        {
            try
            {
                List<string> clone;
                lock (mVSMessageQueue)
                {
                    clone = mVSMessageQueue.ToList();
                    mVSMessageQueue.Clear();
                }
                foreach (var log in clone)
                {
                    Logger.LogInformation(log);
                }
            }
            catch (Exception ex)
            {
                ThrottleLogger($"Update() Log exception {ex.Message}");
            }

            if (Status == ModuleState.Active)
            {
                StartFaceTracking();

                try
                {
                    /*
                     * Eye gaze + exp, data length = 37 
                     *  GazePos, Dir, Openness(0~19)
                     *  EyeExp(20~33)
                     *  Timestamp(34)
                     *  PupilDiameter(35, 36)
                     *  
                     * Eye gaze only, data length = 23
                     *  GazePos, Dir, Openness(0~19)
                     *  Timestamp(20)
                     *  PupilDiameter(21, 22)
                     */
                    if (NewEyeData)
                    {
                        string[] eyeDataComponent = EyeData.Split(',');
                        UpdateEyeGaze(ref UnifiedTracking.Data.Eye, eyeDataComponent);
                        if (eyeDataComponent.Length == 37)
                        {
                            ThrottleLogger($"Current tracking: Eye gaze, eye expression, pupils");
                            UpdateEyeExpression(ref UnifiedTracking.Data.Shapes, eyeDataComponent);
                        }
                        if (eyeDataComponent.Length == 23)
                        {
                            ThrottleLogger($"Current tracking: Eye gaze, pupils");
                        }
                        if (eyeDataComponent.Length == 21) // older server
                        {
                            ThrottleLogger($"Current tracking: Eye gaze");
                        }
                        NewEyeData = false;
                    }

                    if (NewLipData)
                    {
                        string[] lipDataComponent = LipData.Split(',');
                        UpdateLipExpression(ref UnifiedTracking.Data.Shapes, lipDataComponent);
                        NewLipData = false;
                        ThrottleLogger($"Current tracking: Lip expression");
                    }
                }
                catch (Exception ex)
                {
                    ThrottleLogger($"Update() Data exception {ex.Message}");
                }

            }
             
            // User toggled "ToggleTracking" or HMD disconnected
            if (Status == ModuleState.Idle || !HasClientConnection)
            {
               StopFaceTracking();                
            }

            // PC SDK init failed probably due to failed to load DLL, no way to recover this ... ?
            if (Status == ModuleState.Uninitialized)
            {
                // Logger.LogInformation($"Update() ModuleState.Uninitialized");
                // Retry and set Supported to (true, true)
                // StartFaceTracking();
            }

            // Add a delay or halt for the next update cycle for performance. eg: 
            Thread.Sleep(11);
        }

        private static int retryCount = 0;
        public static Thread? InitFaceTrackerWorker = null;
        private void StartFaceTracking()
        {
            if (!HasClientConnection)
            {
                return;
            }

            if (retryCount > MAX_RETRY_COUNT)
            {
                return;
            }

            // Init (re-init) only when both tracker don't work
            if (!EyeTrackerInited && !LipTrackerInited)
            {
                if (InitFaceTrackerWorker == null || !InitFaceTrackerWorker.IsAlive)
                {
                    try
                    {
                        InitFaceTrackerWorker = new Thread(InitFaceTracker);
                        InitFaceTrackerWorker.Start();
                        retryCount++;
                    }
                    catch (Exception ex)
                    {
                        ThrottleLogger($"InitFaceTrackerWorker exception {ex.Message}");
                    }

                }
            }
        }
        
        private static void StopFaceTracking()
        {
            if (EyeTrackerInited || LipTrackerInited)
            {
                VS_PC_SDK.VS_StopFaceTracking();
                EyeTrackerInited = false;
                NewEyeData = false;
                EyeData = "";

                LipTrackerInited = false;
                NewLipData = false;
                LipData = "";

                mVSMessageQueue.Add($"Face tracking stopped.");
            }
            retryCount = 0;
        }

        public static void InitFaceTracker()
        {
            mVSMessageQueue.Add($"Starting face tracking.");
            VS_PC_SDK.VS_StartFaceTracking();

            int totalTime = 0;
            int sleepMs = 100;

            // Exit loop when both tracker success or timeout
            while (totalTime < FACE_TRACKER_DATA_TIMEOUT_MS)
            {
                if (!EyeTrackerInited || !LipTrackerInited) 
                {
                    // mVSMessageQueue.Add($"InitFaceTracker() Waiting for face tracker initialization. {totalTime}");
                    totalTime += sleepMs;
                    Thread.Sleep(sleepMs);
                } 

                if (EyeTrackerInited && LipTrackerInited) {
                    break;
                }
            }

            string result = (EyeTrackerInited || LipTrackerInited)? "started" : "failed to start";
            mVSMessageQueue.Add($"Face tracking {result}");

            // Both tracker failed, re-establish pipe
            if (!EyeTrackerInited && !LipTrackerInited)
            {
                if (retryCount < MAX_RETRY_COUNT)
                {
                    mVSMessageQueue.Add($"Initialize face trackers timeout. Retrying ... ({retryCount})");
                    /*VS_PC_SDK.VS_Release();
                    Thread.Sleep(10);
                    VS_PC_SDK.VS_SetCallbackFunction(mVSStatusUpdateCallback, mVSSettingChangeCallback, mVSLogger);
                    VS_PC_SDK.VS_Init();*/
                } 
                else
                {
                    mVSMessageQueue.Add($"Failed to initialize face trackers after ({retryCount}) retries. You can force retry by 'Toggle Tracking'");
                }
            }
        }

        // Called when the module is unloaded or VRCFaceTracking itself tears down.
        public override void Teardown()
        {
            VS_PC_SDK.VS_StopFaceTracking();
            VS_PC_SDK.VS_Release();

            EyeTrackerInited = false;
            NewEyeData = false;
            EyeData = "";

            LipTrackerInited = false;
            NewLipData = false;
            LipData = "";

            retryCount = 0;
        }

        private void ThrottleLogger(String msg)
        {
            if (LogThrottler.ShouldLog(msg))
            {
                Logger.LogInformation($"{msg}");
            } 
        }
    }
}
