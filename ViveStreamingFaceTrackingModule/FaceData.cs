using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRCFaceTracking;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Params.Expressions;
using VRCFaceTracking.Core.Types;

namespace ViveStreamingFaceTrackingModule
{
    public static class FaceData
    {
        public enum EyeDataIndex : int
        {
            COMBINE_EYE_ORIGIN_X = 0,
            COMBINE_EYE_ORIGIN_Y,
            COMBINE_EYE_ORIGIN_Z,
            COMBINE_EYE_DIRECTION_X,
            COMBINE_EYE_DIRECTION_Y,
            COMBINE_EYE_DIRECTION_Z,
            LEFT_EYE_ORIGIN_X,
            LEFT_EYE_ORIGIN_Y,
            LEFT_EYE_ORIGIN_Z,
            LEFT_EYE_DIRECTION_X,
            LEFT_EYE_DIRECTION_Y,
            LEFT_EYE_DIRECTION_Z,
            RIGHT_EYE_ORIGIN_X,
            RIGHT_EYE_ORIGIN_Y,
            RIGHT_EYE_ORIGIN_Z,
            RIGHT_EYE_DIRECTION_X,
            RIGHT_EYE_DIRECTION_Y,
            RIGHT_EYE_DIRECTION_Z,
            LEFT_EYE_OPENNESS,
            RIGHT_EYE_OPENNESS,
            LEFT_BLINK,
            LEFT_WIDE,
            RIGHT_BLINK,
            RIGHT_WIDE,
            LEFT_SQUEEZE,
            RIGHT_SQUEEZE,
            LEFT_DOWN,
            RIGHT_DOWN,
            LEFT_OUT,
            RIGHT_IN,
            LEFT_IN,
            RIGHT_OUT,
            LEFT_UP,
            RIGHT_UP,
            TIMESTAMP, // unused
            LEFT_PUPIL_DIAMETER,
            RIGHT_PUPIL_DIAMETER,
            MAX
        }
        public enum LipDataIndex : int
        {
            Jaw_Right = 0,
            Jaw_Left,
            Jaw_Forward,
            Jaw_Open,
            Mouth_Ape_Shape,
            Mouth_Upper_Right,
            Mouth_Upper_Left,
            Mouth_Lower_Right,
            Mouth_Lower_Left ,
            Mouth_Upper_Overturn ,
            Mouth_Lower_Overturn ,
            Mouth_Pout,
            Mouth_Smile_Right ,
            Mouth_Smile_Left,
            Mouth_Sad_Right,
            Mouth_Sad_Left,
            Cheek_Puff_Right,
            Cheek_Puff_Left,
            Cheek_Suck,
            Mouth_Upper_Upright,
            Mouth_Upper_Upleft,
            Mouth_Lower_Downright,
            Mouth_Lower_Downleft,
            Mouth_Upper_Inside,
            Mouth_Lower_Inside,
            Mouth_Lower_Overlay,
            Tongue_Longstep1,
            Tongue_Left,
            Tongue_Right,
            Tongue_Up,
            Tongue_Down,
            Tongue_Roll,
            Tongue_Longstep2,
            Tongue_Upright_Morph,
            Tongue_Upleft_Morph,
            Tongue_Downright_Morph,
            Tongue_Downleft_Morph,
            Max
        }

        public static void UpdateEyeGaze(ref UnifiedEyeData data, string[] eyeDataComponent)
        {
            if (eyeDataComponent.Length >= (int)EyeDataIndex.RIGHT_EYE_OPENNESS)
            {
                float x, y;
                if (SafeParse(eyeDataComponent[(int)EyeDataIndex.LEFT_EYE_DIRECTION_X], out x) &&
                    SafeParse(eyeDataComponent[(int)EyeDataIndex.LEFT_EYE_DIRECTION_Y], out y))
                {
                    data.Left.Gaze.x = x;
                    data.Left.Gaze.y = y;
                }

                if (SafeParse(eyeDataComponent[(int)EyeDataIndex.RIGHT_EYE_DIRECTION_X], out x) &&
                    SafeParse(eyeDataComponent[(int)EyeDataIndex.RIGHT_EYE_DIRECTION_Y], out y))
                {
                    data.Right.Gaze.x = x;
                    data.Right.Gaze.y = y;
                }

                float leftOpenness;
                if (SafeParse(eyeDataComponent[(int)EyeDataIndex.LEFT_EYE_OPENNESS], out leftOpenness))
                {
                    data.Left.Openness = leftOpenness;
                }

                float rightOpenness;
                if (SafeParse(eyeDataComponent[(int)EyeDataIndex.RIGHT_EYE_OPENNESS], out rightOpenness))
                {
                    data.Right.Openness = rightOpenness;
                }
            }
            // workaround when server doesn't send eye exp
            if (eyeDataComponent.Length == 23)
            {
                float pd;
                if (SafeParse(eyeDataComponent[21], out pd))
                {
                    data.Left.PupilDiameter_MM = pd;
                }

                if (SafeParse(eyeDataComponent[22], out pd))
                {
                    data.Right.PupilDiameter_MM = pd;
                }
            } 
            else if (eyeDataComponent.Length >= (int)EyeDataIndex.RIGHT_PUPIL_DIAMETER)
            {
                float pd;
                if (SafeParse(eyeDataComponent[(int)EyeDataIndex.LEFT_PUPIL_DIAMETER], out pd))
                {
                    data.Left.PupilDiameter_MM = pd;
                }

                if (SafeParse(eyeDataComponent[(int)EyeDataIndex.RIGHT_PUPIL_DIAMETER], out pd))
                {
                    data.Right.PupilDiameter_MM = pd;
                }
            }

        }
        public static void UpdateEyeExpression(ref UnifiedExpressionShape[] data, string[] eyeDataComponent)
        {
            if (eyeDataComponent.Length >= (int)EyeDataIndex.RIGHT_SQUEEZE)
            {
                float v;
                if (SafeParse(eyeDataComponent[(int)EyeDataIndex.LEFT_WIDE], out v))
                {
                    data[(int)UnifiedExpressions.EyeWideLeft].Weight = v;
                }

                if (SafeParse(eyeDataComponent[(int)EyeDataIndex.RIGHT_WIDE], out v))
                {
                    data[(int)UnifiedExpressions.EyeWideRight].Weight = v;
                }

                if (SafeParse(eyeDataComponent[(int)EyeDataIndex.LEFT_SQUEEZE], out v))
                {
                    data[(int)UnifiedExpressions.EyeSquintLeft].Weight = v;
                }

                if (SafeParse(eyeDataComponent[(int)EyeDataIndex.RIGHT_SQUEEZE], out v))
                {
                    data[(int)UnifiedExpressions.EyeSquintRight].Weight = v;
                }

                if (SafeParse(eyeDataComponent[(int)EyeDataIndex.LEFT_WIDE], out v))
                {
                    data[(int)UnifiedExpressions.BrowInnerUpLeft].Weight = v;
                }

                if (SafeParse(eyeDataComponent[(int)EyeDataIndex.LEFT_WIDE], out v))
                {
                    data[(int)UnifiedExpressions.BrowOuterUpLeft].Weight = v;
                }

                if (SafeParse(eyeDataComponent[(int)EyeDataIndex.RIGHT_WIDE], out v))
                {
                    data[(int)UnifiedExpressions.BrowInnerUpRight].Weight = v;
                }

                if (SafeParse(eyeDataComponent[(int)EyeDataIndex.RIGHT_WIDE], out v))
                {
                    data[(int)UnifiedExpressions.BrowOuterUpRight].Weight = v;
                }

                if (SafeParse(eyeDataComponent[(int)EyeDataIndex.LEFT_SQUEEZE], out v))
                {
                    data[(int)UnifiedExpressions.BrowPinchLeft].Weight = v;
                }

                if (SafeParse(eyeDataComponent[(int)EyeDataIndex.LEFT_SQUEEZE], out v))
                {
                    data[(int)UnifiedExpressions.BrowLowererLeft].Weight = v;
                }

                if (SafeParse(eyeDataComponent[(int)EyeDataIndex.RIGHT_SQUEEZE], out v))
                {
                    data[(int)UnifiedExpressions.BrowPinchRight].Weight = v;
                }

                if (SafeParse(eyeDataComponent[(int)EyeDataIndex.RIGHT_SQUEEZE], out v))
                {
                    data[(int)UnifiedExpressions.BrowLowererRight].Weight = v;
                }
            }
        }
        public static void UpdateLipExpression(ref UnifiedExpressionShape[] data, string[] lipDataComponent)
        {
            if (lipDataComponent.Length >= (int)LipDataIndex.Tongue_Downleft_Morph)
            {
                float v, v2;
                if (SafeParse(lipDataComponent[(int)LipDataIndex.Jaw_Open], out v) &&
                    SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Ape_Shape], out v2))
                {
                    data[(int)UnifiedExpressions.JawOpen].Weight = v + v2;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Jaw_Left], out v))
                {
                    data[(int)UnifiedExpressions.JawLeft].Weight = v;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Jaw_Right], out v))
                {
                    data[(int)UnifiedExpressions.JawRight].Weight = v;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Jaw_Forward], out v))
                {
                    data[(int)UnifiedExpressions.JawForward].Weight = v;
                }
                if (SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Ape_Shape], out v))
                {
                    data[(int)UnifiedExpressions.MouthClosed].Weight = v;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Upper_Upright], out v) &&
                    SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Upper_Overturn], out v2))
                {
                    data[(int)UnifiedExpressions.MouthUpperUpRight].Weight = v - v2;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Upper_Upright], out v) &&
                    SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Upper_Overturn], out v2))
                {
                    data[(int)UnifiedExpressions.MouthUpperDeepenRight].Weight = v - v2;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Upper_Upleft], out v) &&
                    SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Upper_Overturn], out v2))
                {
                    data[(int)UnifiedExpressions.MouthUpperUpLeft].Weight = v - v2;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Upper_Upleft], out v) &&
                    SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Upper_Overturn], out v2))
                {
                    data[(int)UnifiedExpressions.MouthUpperDeepenLeft].Weight = v - v2;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Lower_Downleft], out v) &&
                    SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Lower_Overturn], out v2))
                {
                    data[(int)UnifiedExpressions.MouthLowerDownLeft].Weight = v - v2;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Lower_Downright], out v) &&
                    SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Lower_Overturn], out v2))
                {
                    data[(int)UnifiedExpressions.MouthLowerDownRight].Weight = v - v2;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Pout], out v))
                {
                    data[(int)UnifiedExpressions.LipPuckerUpperLeft].Weight = v;
                    data[(int)UnifiedExpressions.LipPuckerLowerLeft].Weight = v;
                    data[(int)UnifiedExpressions.LipPuckerUpperRight].Weight = v;
                    data[(int)UnifiedExpressions.LipPuckerLowerRight].Weight = v;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Upper_Overturn], out v))
                {
                    data[(int)UnifiedExpressions.LipFunnelUpperLeft].Weight = v;
                    data[(int)UnifiedExpressions.LipFunnelUpperRight].Weight = v;
                    data[(int)UnifiedExpressions.LipFunnelLowerLeft].Weight = v;
                    data[(int)UnifiedExpressions.LipFunnelLowerRight].Weight = v;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Upper_Inside], out v))
                {
                    data[(int)UnifiedExpressions.LipSuckUpperLeft].Weight = v;
                    data[(int)UnifiedExpressions.LipSuckUpperRight].Weight = v;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Lower_Inside], out v))
                {
                    data[(int)UnifiedExpressions.LipSuckLowerLeft].Weight = v;
                    data[(int)UnifiedExpressions.LipSuckLowerRight].Weight = v;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Upper_Left], out v))
                {
                    data[(int)UnifiedExpressions.MouthUpperLeft].Weight = v;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Upper_Right], out v))
                {
                    data[(int)UnifiedExpressions.MouthUpperRight].Weight = v;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Lower_Left], out v))
                {
                    data[(int)UnifiedExpressions.MouthLowerLeft].Weight = v;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Lower_Right], out v))
                {
                    data[(int)UnifiedExpressions.MouthLowerRight].Weight = v;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Smile_Left], out v))
                {
                    data[(int)UnifiedExpressions.MouthCornerPullLeft].Weight = v;
                    data[(int)UnifiedExpressions.MouthCornerSlantLeft].Weight = v;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Smile_Right], out v))
                {
                    data[(int)UnifiedExpressions.MouthCornerPullRight].Weight = v;
                    data[(int)UnifiedExpressions.MouthCornerSlantRight].Weight = v;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Sad_Left], out v))
                {
                    data[(int)UnifiedExpressions.MouthFrownLeft].Weight = v;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Sad_Right], out v))
                {
                    data[(int)UnifiedExpressions.MouthFrownRight].Weight = v;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Lower_Overlay], out v))
                {
                    data[(int)UnifiedExpressions.MouthRaiserLower].Weight = v;
                    if (SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Upper_Inside], out v2))
                    {
                        data[(int)UnifiedExpressions.MouthRaiserUpper].Weight = v - v2;
                    }
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Cheek_Puff_Left], out v))
                {
                    data[(int)UnifiedExpressions.CheekPuffLeft].Weight = v;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Cheek_Puff_Right], out v))
                {
                    data[(int)UnifiedExpressions.CheekPuffRight].Weight = v;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Cheek_Suck], out v))
                {
                    data[(int)UnifiedExpressions.CheekSuckLeft].Weight = v;
                    data[(int)UnifiedExpressions.CheekSuckRight].Weight = v;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Tongue_Longstep1], out v) &&
                    SafeParse(lipDataComponent[(int)LipDataIndex.Tongue_Longstep2], out v2))
                {
                    data[(int)UnifiedExpressions.TongueOut].Weight = (v + v2) / 2.0f;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Tongue_Up], out v))
                {
                    data[(int)UnifiedExpressions.TongueUp].Weight = v;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Tongue_Down], out v))
                {
                    data[(int)UnifiedExpressions.TongueDown].Weight = v;
                }
                if (SafeParse(lipDataComponent[(int)LipDataIndex.Tongue_Left], out v))
                {
                    data[(int)UnifiedExpressions.TongueLeft].Weight = v;
                }
                if (SafeParse(lipDataComponent[(int)LipDataIndex.Tongue_Right], out v))
                {
                    data[(int)UnifiedExpressions.TongueRight].Weight = v;
                }
                if (SafeParse(lipDataComponent[(int)LipDataIndex.Tongue_Roll], out v))
                {
                    data[(int)UnifiedExpressions.TongueRoll].Weight = v;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Smile_Left], out v))
                {
                    data[(int)UnifiedExpressions.CheekSquintLeft].Weight = v;
                    data[(int)UnifiedExpressions.MouthDimpleLeft].Weight = v;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Smile_Right], out v))
                {
                    data[(int)UnifiedExpressions.CheekSquintRight].Weight = v;
                    data[(int)UnifiedExpressions.MouthDimpleRight].Weight = v;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Sad_Left], out v))
                {
                    data[(int)UnifiedExpressions.MouthStretchLeft].Weight = v;
                }

                if (SafeParse(lipDataComponent[(int)LipDataIndex.Mouth_Sad_Right], out v))
                {
                    data[(int)UnifiedExpressions.MouthStretchRight].Weight = v;
                }
            }
        }
        // Handle null, whitespace and NaN
        // Return false to skip update (keep previous value) instead of sending 0
        public static bool SafeParse(string value, out float result)
        {
            result = 0f;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            // Some countries uses comma as float number separator. Use CultureInfo.InvariantCulture to always use dot as separator
            if (float.TryParse(value, CultureInfo.InvariantCulture, out result) && !float.IsNaN(result))
                return true;

            return false;
        }
    }
}
