using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime;
using System.Threading.Tasks;
using UnityEngine;
using VTOLVR.ReplaySystem;
using System.IO.Compression;

namespace VTReplayConverter
{
    public class VTACMI
    {
        public static void ConvertToACMI(string vtrPath, string tacviewSavePath)
        {
            Console.WriteLine($"Converting {Path.GetFileNameWithoutExtension(vtrPath)} to acmi");

            ReplaySerializer.LoadFromFile(vtrPath, ReplayRecorder.Instance);

            List<ReplayRecorder.ReplayEntity> entitites = ReplayRecorder.Instance.GetAllEntities();
            SortedDictionary<float, List<ReplayRecorder.ReplayEntity>> entityKeyFrameDict = new SortedDictionary<float, List<ReplayRecorder.ReplayEntity>>();

            //Console.WriteLine("ID | Label | KeyframeCount | Lifetime");
            foreach (var entity in entitites)
            {
                var keyFrames = ReplayRecorder.Instance.motionTracks[entity.id].keyframes;
                //Console.WriteLine($"{entity.id} | {entity.metaData.label} | {keyFrames.Count} | {keyFrames.Last().t - keyFrames[0].t }");
                foreach (ReplayRecorder.Keyframe keyFrame in keyFrames)
                {
                    if (!entityKeyFrameDict.ContainsKey(keyFrame.t))
                    {
                        entityKeyFrameDict.Add(keyFrame.t, new List<ReplayRecorder.ReplayEntity>());
                    }

                    entityKeyFrameDict[keyFrame.t].Add(entity);
                }
            }

            List<BulletReplay> bullets = GetBullets();
            int lineCount = 0;
            using (var streamWriter = new StreamWriter(tacviewSavePath, false, Encoding.UTF8))
            {
                streamWriter.WriteLine("FileType=text/acmi/tacview");
                streamWriter.WriteLine("FileVersion=2.2");


                foreach (KeyValuePair<float, List<ReplayRecorder.ReplayEntity>> entityKeyframe in entityKeyFrameDict)
                {

                    bool frameSetTrigger = false;
                    bool bulletSameFrame = false;
                    List<float> previousFrames = new List<float>();
                    while (!frameSetTrigger)
                    {
                        frameSetTrigger = true;

                        foreach (BulletReplay bullet in bullets)
                        {
                            if(bullet.currentT == entityKeyframe.Key)
                                bulletSameFrame = true;

                            if (bullet.currentT <= entityKeyframe.Key && bullet.currentT <= bullet.endT)
                            {
                                if (!previousFrames.Contains(bullet.currentT))
                                {
                                    previousFrames.Add(bullet.currentT);
                                    streamWriter.WriteLine($"#{bullet.currentT}");
                                }

                                frameSetTrigger = true;
                                string tacviewUpdateString = BuildBulletUpdateString(bullet, bullet.currentT);
                                streamWriter.WriteLine(tacviewUpdateString);

                                lineCount++;

                                if(bullet.currentT == bullet.endT)
                                {
                                    if(bullet.endT - bullet.startT < bullet.lifeTime)
                                    {
                                        ExplosionAnimation(streamWriter, bullet.endT, bullet);
                                    }
                                    bullet.currentT += 1f;
                                    continue;
                                }

                                bullet.currentT += 0.05f;
                                bullet.currentT = Math.Min(bullet.currentT, bullet.endT);
                            }
                        }
                    }

                    if(!bulletSameFrame)
                        streamWriter.WriteLine($"#{entityKeyframe.Key}");

                    foreach (ReplayRecorder.ReplayEntity entity in entityKeyframe.Value)
                    {
                        string tacviewString = !entity.initalized ? BuildInitString(entity, entityKeyframe.Key) : BuildUpdateString(entity, entityKeyframe.Key);

                        streamWriter.WriteLine(tacviewString);

                    }

                }

            }

            Console.WriteLine($"Motion Tracks: {ReplayRecorder.Instance.motionTracks.Count}");
            Console.WriteLine($"Event Tracks: {ReplayRecorder.Instance.eventTrack.Count}");
            Console.WriteLine($"Custom Tracks: {ReplayRecorder.Instance.customTracks.Count}");
            Console.WriteLine($"Keyframe count: {entityKeyFrameDict.Count}");
            Console.WriteLine($"Bullet Events: {bullets.Count} ");
            Console.WriteLine($"Bullet Line Count: {lineCount} ");
            Console.WriteLine("Finished conversion\n");
        }

        private static List<BulletReplay> GetBullets()
        {
            List<BulletReplay> bullets = new List<BulletReplay>();

            foreach (var eventTrack in ReplayRecorder.Instance.eventTrack.keyframes)
            {
                if (eventTrack.eventType == 1)
                {
                    if (eventTrack is BulletEventKeyframe bulletEvent)
                    {
                        
                        BulletReplay bullet = new BulletReplay();
                        Vector3 vector = bulletEvent.fp.globalPoint.toVector3 + (0.05f) * bulletEvent.velocity;
                        bullet.globalPt = vector;
                        bullet.velocity = bulletEvent.velocity;
                        bullet.mass = bulletEvent.mass;
                        bullet.startT = bulletEvent.t;
                        bullet.currentT = bulletEvent.t;
                        bullet.bId = bulletEvent.bId;
                        bullet.lifeTime = bulletEvent.lifeTime;

                        bullets.Add(bullet);
                        continue;
                    }

                    if (eventTrack is BulletEndKeyframe bulletEndKeyframe)
                    {
                        bullets[bulletEndKeyframe.bId].endT = bulletEndKeyframe.t;
                    }
                }

            }

            List<BulletReplay> sortedList = bullets.OrderBy(b => b.startT).ToList();

            return sortedList;
            
        }
        public static void DebugVTR(string vtrPath)
        {
            ReplaySerializer.LoadFromFile(vtrPath, ReplayRecorder.Instance);

            List<ReplayRecorder.ReplayEntity> entitites = ReplayRecorder.Instance.GetAllEntities();

            Console.WriteLine($"Motion Tracks: {ReplayRecorder.Instance.motionTracks.Count}");
            Console.WriteLine($"Event Tracks: {ReplayRecorder.Instance.eventTrack.Count}");
            Console.WriteLine($"Custom Tracks: {ReplayRecorder.Instance.customTracks.Count}");

           
        }

        private static bool GetPosition(MotionTrack track, float t, out Vector3 pos, out Vector3 eulerRotation, out bool lastFrame)
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
                    lastFrame = i == track.keyframes.Count - 1;
                    return true;
                }
            }

            pos = default(Vector3);
            eulerRotation = default(Vector3);
            lastFrame = false;
            return false;
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
            v.y = NormalizeAngle(v.y-90f);
            return NormalizeAngles(v);
        }

        static Vector3 NormalizeAngles(Vector3 angles)
        {
            angles.x = NormalizeAngle(angles.x);
            angles.y = NormalizeAngle(angles.y);
            angles.z = NormalizeAngle(angles.z);
            return angles;
        }

        static float NormalizeAngle(float angle)
        {
            return angle % 360;
        }
        
        private static string BuildBulletUpdateString(BulletReplay bullet, float t)
        {
            string entityHex = (bullet.bId + 1).ToString()+ "B";
            Vector3 position = bullet.GetBulletPos(t);
            Vector3D gpsPosition = WorldPositionToGPSCoords(position);
            string gpsString = $"{gpsPosition.x}|{gpsPosition.y}|{gpsPosition.z}";
            string typeString = "Projectile + Bullet";
            
            string builtString = $"{entityHex},Type={typeString},T={gpsString}";
            
            return builtString;
        }

        private static void ExplosionAnimation(StreamWriter writer, float t, BulletReplay bullet)
        {
            string entityHex = (bullet.bId + 1).ToString() + "E";
            Vector3 position = bullet.GetBulletPos(t);
            Vector3D gpsPosition = WorldPositionToGPSCoords(position);
            string gpsString = $"{gpsPosition.x}|{gpsPosition.y}|{gpsPosition.z}";
            string typeString = "Projectile + Explosion";
           
            string explosionString = $"{entityHex},Type={typeString},T={gpsString},Radius=0.1";
            writer.WriteLine(explosionString);

            writer.WriteLine($"#{t + 0.5f}");
            string despawnExplosion = $"{entityHex},Visible=0";
            writer.WriteLine(despawnExplosion);

        }

        private static string BuildInitString(ReplayRecorder.ReplayEntity entity, float time)
        {
            entity.initalized = true;

            string updateString = BuildUpdateString(entity, time);
            string shapeString = GetShape((ReplayActorEntityTypes)entity.entityType);
            string colorString = GetColor((ReplayActorEntityTypes)entity.entityType);


            string builtString = $"{updateString},Name={entity.metaData.label},LongName={entity.metaData.label},ShortName={entity.metaData.label},Color={colorString},Shape={shapeString}";

            return builtString;
        }
        private static string BuildUpdateString(ReplayRecorder.ReplayEntity entity, float time)
        {
            string entityHex = (entity.id + 1).ToString();
            Vector3 position;
            Vector3 rotation;
            bool lastFrame;
            GetPosition(ReplayRecorder.Instance.motionTracks[entity.id], time, out position, out rotation, out lastFrame);
            Vector3D gpsPosition = WorldPositionToGPSCoords(position);
            string gpsString = $"{gpsPosition.x}|{gpsPosition.y}|{gpsPosition.z}|{rotation.z}|{-rotation.x}|{-rotation.y}";

            string builtString = $"{entityHex},T={gpsString}" + (lastFrame ? ",Visible=0" : "");

            return builtString;
        }
        private static string GetShape(ReplayActorEntityTypes entityType)
        {
            string shapeString;
            switch (entityType)
            {
                case ReplayActorEntityTypes.AirA:
                case ReplayActorEntityTypes.AirB:
                    shapeString = "FixedWing.F-15.obj";
                    break;
                case ReplayActorEntityTypes.GroundA:
                case ReplayActorEntityTypes.GroundB:
                    shapeString = "Vehicle.Tank.M1.obj";
                    break;
                case ReplayActorEntityTypes.ChopperA:
                case ReplayActorEntityTypes.ChopperB:
                    shapeString = "Rotorcraft.AH-64.obj";
                    break;
                case ReplayActorEntityTypes.SeaA:
                case ReplayActorEntityTypes.SeaB:
                    shapeString = "Watercraft.Warship.obj";
                    break;
                case ReplayActorEntityTypes.Missile:
                    shapeString = "Missile.AIM-120C.obj";
                    break;
                default:
                    shapeString = "FixedWing.F-15.obj";
                    break;
            }

            return shapeString;
        }
        private static string GetColor(ReplayActorEntityTypes entityType)
        {
            string colorString;
            switch (entityType)
            {
                case ReplayActorEntityTypes.AirA:
                case ReplayActorEntityTypes.GroundA:
                case ReplayActorEntityTypes.SeaA:
                case ReplayActorEntityTypes.ChopperA:
                    colorString = "Blue";
                    break;
                case ReplayActorEntityTypes.AirB:
                case ReplayActorEntityTypes.GroundB:
                case ReplayActorEntityTypes.SeaB:
                case ReplayActorEntityTypes.ChopperB:
                    colorString = "Red";
                    break;
                case ReplayActorEntityTypes.Missile:
                    colorString = "Orange";
                    break;
                default:
                    colorString = "Yellow";
                    break;
            }

            return colorString;
        }
        private static Vector3D WorldPositionToGPSCoords(Vector3 worldPoint)
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

        private enum ReplayActorEntityTypes
        {
            AirA,
            AirB,
            GroundA,
            GroundB,
            SeaA,
            SeaB,
            Missile,
            ChopperA,
            ChopperB
        }
    }
}
