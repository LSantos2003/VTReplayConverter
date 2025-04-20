using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VTReplayConverter
{
    public class ACMIUtils
    {

        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        public static Vector3D WorldPositionToGPSCoords(Vector3 worldPoint)
        {
            Vector3D vector3D = VTMapManager.WorldToGlobalPoint(worldPoint);
            double z = (double)(worldPoint.y);
            double num = vector3D.z / 111319.9;
            double num2 = Math.Abs(Math.Cos(num * 0.01745329238474369) * 111319.9);
            double num3 = 0.0;
            if (num2 > 0.0)
            {
                num3 = vector3D.x / num2;
            }
            double num4 = num3;

            return new Vector3D(num, num4, z);
        }

        public static bool GetPosition(MotionTrack track, float t, out Vector3 pos, out Vector3 eulerRotation, out bool lastFrame)
        {
            if (t < track.startTime || t > track.endTime)
            {
                pos = default(Vector3);
                eulerRotation = default(Vector3);
                lastFrame = false;
                return false;
            }

            for (int i = 0; i < track.keyframes.Count; i++)
            {
                if (track[i].t == t)
                {
                    pos = track[i].fp.globalPoint.toVector3;
                    eulerRotation = ToEulerAngles(track[i].rotation);
                    lastFrame = t >= track.endTime;
                    return true;
                }
            }

            pos = default(Vector3);
            eulerRotation = default(Vector3);
            lastFrame = false;
            return false;
        }

        private static Vector3 ToEulerAngles(Quaternion q1)
        {
            float sqw = q1.w * q1.w;
            float sqx = q1.x * q1.x;
            float sqy = q1.y * q1.y;
            float sqz = q1.z * q1.z;
            float unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
            float test = q1.x * q1.w - q1.y * q1.z;
            Vector3 v;

            if (test > 0.4995f * unit)
            { // singularity at north pole
                v.y = 2f * Mathf.Atan2(q1.y, q1.x);
                v.x = Mathf.PI / 2;
                v.z = 0;
                return NormalizeAngles(v * Mathf.Rad2Deg);
            }
            if (test < -0.4995f * unit)
            { // singularity at south pole
                v.y = -2f * Mathf.Atan2(q1.y, q1.x);
                v.x = -Mathf.PI / 2;
                v.z = 0;
                return NormalizeAngles(v * Mathf.Rad2Deg);
            }
            Quaternion q = new Quaternion(q1.w, q1.z, q1.x, q1.y);
            v.y = (float)Math.Atan2(2f * q.x * q.w + 2f * q.y * q.z, 1 - 2f * (q.z * q.z + q.w * q.w));     // Yaw
            v.x = (float)Math.Asin(2f * (q.x * q.z - q.w * q.y));                             // Pitch
            v.z = (float)Math.Atan2(2f * q.x * q.y + 2f * q.z * q.w, 1 - 2f * (q.y * q.y + q.z * q.z));      // Roll

            //Subtracts 90 degrees from yaw for tacview
            v = v * Mathf.Rad2Deg;
            v.y = NormalizeAngle(v.y);
            return NormalizeAngles(v);
        }

        private static Vector3 NormalizeAngles(Vector3 angles)
        {
            angles.x = NormalizeAngle(angles.x);
            angles.y = NormalizeAngle(angles.y);
            angles.z = NormalizeAngle(angles.z);
            return angles;
        }

        private static float NormalizeAngle(float angle)
        {
            return angle % 360;
        }

    }
}
