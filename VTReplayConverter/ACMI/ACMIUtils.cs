using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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

        public static bool IsReplayConverted(string replayPath)
        {
            string folderName = Path.GetFileName(replayPath);
            string tacviewSavePath = Path.Combine(Program.AcmiSavePath, $"{folderName}.acmi");

            return File.Exists(tacviewSavePath);
        }

        public static void ReplaceWithZippedVersion(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            string directory = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileName(filePath);
            string tempZipPath = Path.Combine(directory, fileName + ".ziptmp");

            try
            {
                // Create a zip file next to the original file
                using (FileStream zipToOpen = new FileStream(tempZipPath, FileMode.Create))
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                {
                    archive.CreateEntryFromFile(filePath, fileName, CompressionLevel.Optimal);
                }

                // Replace the original file with the zipped version (but keep the original name)
                File.Delete(filePath);
                File.Move(tempZipPath, filePath);
            }
            catch
            {
                if (File.Exists(tempZipPath))
                    File.Delete(tempZipPath);
                throw;
            }
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

        public static Teams GetEntityTeam(ReplayRecorder.ReplayEntity entity)
        {
            switch ((VTACMI.ReplayActorEntityTypes)entity.entityType)
            {
                case VTACMI.ReplayActorEntityTypes.AirA:
                case VTACMI.ReplayActorEntityTypes.GroundA:
                case VTACMI.ReplayActorEntityTypes.SeaA:
                case VTACMI.ReplayActorEntityTypes.ChopperA:
                    return Teams.Allied;
                case VTACMI.ReplayActorEntityTypes.AirB:
                case VTACMI.ReplayActorEntityTypes.GroundB:
                case VTACMI.ReplayActorEntityTypes.SeaB:
                case VTACMI.ReplayActorEntityTypes.ChopperB:
                    return Teams.Enemy;
                case VTACMI.ReplayActorEntityTypes.Missile:
                    return Teams.Allied;
                default:
                    return Teams.Allied;
 
            }

        }
        public static bool GetPositionAndRotation(MotionTrack track, float t, out Vector3 pos, out Vector3 eulerRotation, out Quaternion rotation, out bool lastFrame)
        {
            if (t < track.startTime || t > track.endTime)
            {
                pos = default(Vector3);
                eulerRotation = default(Vector3);
                rotation = default(Quaternion);
                lastFrame = false;
                return false;
            }

            int segmentIndex;
            FindSegment<ReplayRecorder.MotionKeyframe>(track, out segmentIndex, t);
      
            if (segmentIndex >= 0 && track[segmentIndex].t == t)
            {
                pos = track[segmentIndex].fp.globalPoint.toVector3;
                eulerRotation = ToEulerAngles(track[segmentIndex].rotation);
                rotation = track[segmentIndex].rotation;
                lastFrame = t >= track.endTime;
                return true;
            }
            

            pos = default(Vector3);
            eulerRotation = default(Vector3);
            rotation = default(Quaternion);
            lastFrame = false;
            return false;
        }

        public static bool GetPosition(MotionTrack track, float t, out Vector3 pos, out bool lastFrame)
        {
            if (t < track.startTime || t > track.endTime)
            {
                pos = default(Vector3);
                lastFrame = false;
                return false;
            }

            int segmentIndex;
            FindSegment<ReplayRecorder.MotionKeyframe>(track, out segmentIndex, t);

            if (segmentIndex >= 0)
            {
                pos = track[segmentIndex].fp.globalPoint.toVector3;
                lastFrame = t >= track.endTime;
                return true;
            }


            pos = default(Vector3);
            lastFrame = false;
            return false;
        }

        public static void FindSegment<T>(Track<T> track, out int segmentIndex, float targetTime) where T : ReplayRecorder.Keyframe
        {
            int low = 0;
            int high = track.keyframes.Count - 1;

            while (low <= high)
            {
                int mid = low + (high - low) / 2;
                float midTime = track[mid].t;

                if (midTime < targetTime)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }

            // After the loop, 'low' is the index of the first keyframe >= targetTime
            if (low < track.keyframes.Count)
            {
                segmentIndex = low;
                //If there's a duplicate keyframe timestamp, return the later keyframe
                if(low < track.keyframes.Count-1 && track.keyframes[segmentIndex].t == track.keyframes[segmentIndex + 1].t)
                {
                    segmentIndex++;
                }
            }
            else
            {
                segmentIndex = -1; // No segment found with time >= targetTime
            }
        }

        public static Vector3 ToEulerAngles(Quaternion q1)
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

        public static Vector3 Slerp(Vector3 from, Vector3 to, float t)
        {
            t = Mathf.Clamp01(t);

            float dot = Vector3.Dot(from.normalized, to.normalized);
            dot = Mathf.Clamp(dot, -1.0f, 1.0f);

            float theta = Mathf.Acos(dot) * t;
            Vector3 relativeVec = (to - from * dot).normalized;

            return from * Mathf.Cos(theta) + relativeVec * Mathf.Sin(theta);
        }

    }
}
