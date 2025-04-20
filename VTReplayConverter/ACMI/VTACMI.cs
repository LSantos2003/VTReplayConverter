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
            var watch = System.Diagnostics.Stopwatch.StartNew();

            ReplayRecorder recorder = new ReplayRecorder();
            recorder.Awake();

            string folderName = Path.GetDirectoryName(vtrPath);
            folderName = Path.GetFileName(folderName);
            Console.WriteLine($"Converting {folderName} to acmi");

            ReplaySerializer.LoadFromFile(vtrPath, ReplayRecorder.Instance);

            List<ReplayRecorder.ReplayEntity> entitites = ReplayRecorder.Instance.GetAllEntities();
            SortedDictionary<float, List<ReplayRecorder.ReplayEntity>> entityKeyFrameDict = new SortedDictionary<float, List<ReplayRecorder.ReplayEntity>>();

            //Console.WriteLine("Creating keyframe dictionary");
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
            // Queue<ReplayRecorder.WorldEventKeyframe> explosions = GetExplosionKeyFrames();


            Console.WriteLine($"Bullet Count: {bullets.Count}");

            int totalBulletLineCount = 0; 
            using (var streamWriter = new StreamWriter(tacviewSavePath, false, Encoding.UTF8))
            {
                streamWriter.WriteLine("FileType=text/acmi/tacview");
                streamWriter.WriteLine("FileVersion=2.2");

                int currentProgress = 0;
                int maxProgress = entityKeyFrameDict.Count;
                foreach (KeyValuePair<float, List<ReplayRecorder.ReplayEntity>> entityKeyframe in entityKeyFrameDict)
                {
                    float t = entityKeyframe.Key;
                    int bulletLineCount;
                    bool bulletSameFrame = ConvertBullets(streamWriter, bullets, t, out bulletLineCount);
                    totalBulletLineCount += bulletLineCount;
                    if (!bulletSameFrame)
                        streamWriter.WriteLine($"#{t}");
                        

                    foreach (ReplayRecorder.ReplayEntity entity in entityKeyframe.Value)
                    {
                        string tacviewString = !entity.initalized ? BuildInitString(entity, t) : BuildUpdateString(entity, t);

                        streamWriter.WriteLine(tacviewString);

                    }

                    currentProgress++;

                    if(currentProgress % 2500 == 0)
                    {
                        ACMIUtils.ClearCurrentConsoleLine();
                        Console.WriteLine($"Progress: {currentProgress}/{maxProgress}");
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                    }
                }

            }


            ReplayRecorder.Instance.Reset();
           /* Console.WriteLine($"Motion Tracks: {ReplayRecorder.Instance.motionTracks.Count}");
            Console.WriteLine($"Event Tracks: {ReplayRecorder.Instance.eventTrack.Count}");
            Console.WriteLine($"Custom Tracks: {ReplayRecorder.Instance.customTracks.Count}");
            Console.WriteLine($"Keyframe count: {entityKeyFrameDict.Count}");
            Console.WriteLine($"Bullet Events: {bullets.Count} ");
            Console.WriteLine($"Bullet Line Count: {totalBulletLineCount} ");*/

            float fileSize = new FileInfo(tacviewSavePath).Length;
            fileSize /= 1024*1024;

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"Final File Size: {fileSize.ToString("0.00")} MB");
            Console.WriteLine($"Finished conversion: {elapsedMs/1000f} seconds\n");
        }

        private static bool ConvertBullets(StreamWriter streamWriter, List<BulletReplay> bullets, float t, out int lineCount)
        {
            int whileLoopRan = 0;
            lineCount = 0;
            bool frameSetTrigger = false;
            bool bulletSameFrame = false;

           
            whileLoopRan++;
            frameSetTrigger = false;

            while (!frameSetTrigger)
            {
                List<float> previousFrames = new List<float>();

                frameSetTrigger = true;
                foreach (BulletReplay bullet in bullets)
                {
                    if (bullet.currentT == t)
                        bulletSameFrame = true;

                    if (bullet.currentT <= t && bullet.currentT <= bullet.endT)
                    {
                        if (!previousFrames.Contains(bullet.currentT))
                        {
                            previousFrames.Add(bullet.currentT);
                            streamWriter.WriteLine($"#{bullet.currentT}");
                            lineCount++;
                        }

                        frameSetTrigger = false;
                        string tacviewUpdateString = BuildBulletUpdateString(bullet, bullet.currentT);
                        streamWriter.WriteLine(tacviewUpdateString);
                        lineCount++;


                        if (bullet.currentT == bullet.endT)
                        {
                            /*if (bullet.endT - bullet.startT < bullet.lifeTime)
                            {
                                //ACMIAnimation.ExplosionAnimation(streamWriter, bullet);
                            }*/
                            bullet.currentT += 5f;
                            continue;
                        }

                        bullet.currentT += Program.BulletPollRate;
                        bullet.currentT = Math.Min(bullet.currentT, bullet.endT);
                    }
                }
            }
            

            if(whileLoopRan > 1)
                Console.WriteLine($"While Loop Ran: {whileLoopRan}");

            return bulletSameFrame;
        }

        private static List<BulletReplay> GetBullets()
        {
            List<BulletReplay> bullets = new List<BulletReplay>();

            foreach (var eventTrack in ReplayRecorder.Instance.eventTrack.keyframes)
            {
                if (eventTrack.eventType == (int)EventTypes.Bullet)
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
                        bullet.endT = bullet.startT + bullet.lifeTime;

                        bullets.Add(bullet);
                        continue;
                    }

                    if (eventTrack is BulletEndKeyframe bulletEndKeyframe && bulletEndKeyframe.bId < bullets.Count)
                    {
                        bullets[bulletEndKeyframe.bId].endT = bulletEndKeyframe.t;
                    }
                }

            }

            List<BulletReplay> sortedList = bullets.OrderBy(b => b.startT).ToList();

            return sortedList;
            
        }

        private static bool ConvertExplosions(StreamWriter streamWriter, Queue<ReplayRecorder.WorldEventKeyframe> explosions, float t)
        {
            bool explosionSameFrame = false;
            if(explosions.Peek().t <= t)
            {
                explosionSameFrame = explosions.Peek().t == t;
                streamWriter.WriteLine($"#{t}");
                ACMIAnimation.ExplosionAnimation(streamWriter, explosions.Dequeue(), explosions.Count);
            }

            return explosionSameFrame;
        }

        private static Queue<ReplayRecorder.WorldEventKeyframe> GetExplosionKeyFrames()
        {
            Queue<ReplayRecorder.WorldEventKeyframe> explosions = new Queue<ReplayRecorder.WorldEventKeyframe>();

            foreach (var eventTrack in ReplayRecorder.Instance.eventTrack.keyframes)
            {
                if (eventTrack.eventType == (int)EventTypes.Explosion && eventTrack is ReplayRecorder.WorldEventKeyframe explosionEvent)
                {
                   
                    explosions.Enqueue(eventTrack as ReplayRecorder.WorldEventKeyframe);
                    
                }

            }

            return explosions;
        }


        public static void DebugVTR(string vtrPath)
        {
            ReplaySerializer.LoadFromFile(vtrPath, ReplayRecorder.Instance);

            List<ReplayRecorder.ReplayEntity> entitites = ReplayRecorder.Instance.GetAllEntities();

            Console.WriteLine($"Motion Tracks: {ReplayRecorder.Instance.motionTracks.Count}");
            Console.WriteLine($"Event Tracks: {ReplayRecorder.Instance.eventTrack.Count}");
            Console.WriteLine($"Custom Tracks: {ReplayRecorder.Instance.customTracks.Count}");

            foreach(var motionTrack in entitites)
            {
                Console.WriteLine($"{motionTrack.metaData.label}");
            }

            foreach (var track in ReplayRecorder.Instance.eventTrack.keyframes)
            {

                //Console.WriteLine($"{(EventTypes)track.eventType} : {track.t}");
            }

            foreach (var track in ReplayRecorder.Instance.customTracks)
            {
               // Console.WriteLine($"{track.Value.keyframeType}");
            }

           
        } 
        
        private static string BuildBulletUpdateString(BulletReplay bullet, float t)
        {
            
            string entityHex = (bullet.bId + 1).ToString() + "B";
            Vector3 position = bullet.GetBulletPos(t);
            Vector3D gpsPosition = ACMIUtils.WorldPositionToGPSCoords(position);
            string gpsString = $"{gpsPosition.y}|{gpsPosition.x}|{gpsPosition.z}";
            string typeString = "Projectile + Bullet";
            
            string builtString = $"{entityHex},T={gpsString}";
            if (!bullet.initiliazed)
            {
                builtString += $",Type={typeString}";
                bullet.initiliazed = true;
            }

            if(t >= bullet.endT && bullet.endT <= bullet.startT + bullet.lifeTime)
            {
                builtString += $"\n-{entityHex}";
            }

            return builtString;
        }  

        private static string BuildInitString(ReplayRecorder.ReplayEntity entity, float time)
        {
            entity.initalized = true;

            string updateString = BuildUpdateString(entity, time);
            string typeString = GetType((ReplayActorEntityTypes)entity.entityType);
            string shapeString = GetShape((ReplayActorEntityTypes)entity.entityType);
            string colorString = GetColor((ReplayActorEntityTypes)entity.entityType);


            string builtString = $"{updateString},Name={entity.metaData.label},Type={typeString},LongName={entity.metaData.label},ShortName={entity.metaData.label},Color={colorString},Shape={shapeString}";

            return builtString;
        }
        private static string BuildUpdateString(ReplayRecorder.ReplayEntity entity, float time)
        {
            string entityHex = (entity.id + 1).ToString();
            Vector3 position;
            Vector3 rotation;
            bool lastFrame;
            ACMIUtils.GetPosition(ReplayRecorder.Instance.motionTracks[entity.id], time, out position, out rotation, out lastFrame);
            Vector3D gpsPosition = ACMIUtils.WorldPositionToGPSCoords(position);
            string gpsString = $"{gpsPosition.y}|{gpsPosition.x}|{gpsPosition.z}|{-rotation.z}|{-rotation.x}|{rotation.y}";

            string builtString = $"{entityHex},T={gpsString}" + (lastFrame ? ",Visible=0" : "");
            builtString += lastFrame ? $"\n-{entityHex}" : "";
            
            return builtString;
        }

        private static string GetType(ReplayActorEntityTypes entityType)
        {
            string typeString;
            switch (entityType)
            {
                case ReplayActorEntityTypes.AirA:
                case ReplayActorEntityTypes.AirB:
                    typeString = "FixedWing";
                    break;
                case ReplayActorEntityTypes.GroundA:
                case ReplayActorEntityTypes.GroundB:
                    typeString = "Tank";
                    break;
                case ReplayActorEntityTypes.ChopperA:
                case ReplayActorEntityTypes.ChopperB:
                    typeString = "Rotorcraft";
                    break;
                case ReplayActorEntityTypes.SeaA:
                case ReplayActorEntityTypes.SeaB:
                    typeString = "Watercraft";
                    break;
                case ReplayActorEntityTypes.Missile:
                    typeString = "Missile";
                    break;
                default:
                    typeString = "FixedWing";
                    break;
            }

            return typeString;
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
