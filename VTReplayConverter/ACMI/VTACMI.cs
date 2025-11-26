using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VTOLVR.ReplaySystem;
using System.Threading;


namespace VTReplayConverter
{
    public class VTACMI
    {
        public static bool IncludeEW = true;

        public static bool IncludeBullets = true;

        public const float BulletPollRate = 0.35f;

        private bool isVFM = false;

        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        private ReplayRecorder recorder;

        private ACMIHex acmiHex;

        private static int ConversionId = 0;

        private int flareCount = 0;

        private List<float> flareDespawnTimes = new List<float>();

        private const float FlareLifeTime = 4f;

        public VTACMI(string vtrPath, bool isVFM)
        {
            this.recorder = new ReplayRecorder();
            this.recorder.Awake();
            this.acmiHex = new ACMIHex(ConversionId);
            this.isVFM = isVFM;

            ConversionId++;
            ReplaySerializer.LoadFromFile(vtrPath, this.recorder, this.isVFM);
            ReplaySerializer.ClearSerializedReplay();

        }

        public static async void ConvertToACMI(string vtrPath, string tacviewSavePath, bool isVFM = false)
        {
            VTACMI converter = new VTACMI(vtrPath, isVFM);
            converter.ConvertFromPath(vtrPath, tacviewSavePath);
            converter.recorder.Reset();
            ACMILoadingBar.ResetBar();
        }


        public static async Task ConvertToACMIAsync(string vtrPath, string tacviewSavePath, bool isVFM = false)
        {
            await semaphore.WaitAsync();
         
            VTACMI converter = new VTACMI(vtrPath, isVFM);
            semaphore.Release(); // Release the Replay Serializer

            converter.ConvertFromPath(vtrPath, tacviewSavePath);
            converter.recorder.Reset();
        }

        private void ConvertFromPath(string vtrPath, string tacviewSavePath)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();


            string folderName = Path.GetDirectoryName(vtrPath);
            folderName = Path.GetFileName(folderName);
            Console.WriteLine($"Converting {folderName} to acmi");

            List<ReplayRecorder.ReplayEntity> entitites = this.recorder.GetAllEntities();

            //Master dictionary of all tracks sorted by keyframe
            SortedDictionary<float, List<object>> tracksKeyFrameDict = new SortedDictionary<float, List<object>>();

            //Console.WriteLine("Creating keyframe dictionary");
            //Console.WriteLine("ID | Label | KeyframeCount | Lifetime");
            foreach (var entity in entitites)
            {
                var keyFrames = this.recorder.motionTracks[entity.id].keyframes;
                //Console.WriteLine($"{entity.id} | {entity.metaData.label} | {keyFrames.Count} | {keyFrames.Last().t - keyFrames[0].t }");
                foreach (ReplayRecorder.Keyframe keyFrame in keyFrames)
                {
                    if (!tracksKeyFrameDict.ContainsKey(keyFrame.t))
                    {
                        tracksKeyFrameDict.Add(keyFrame.t, new List<object>());
                    }

                    tracksKeyFrameDict[keyFrame.t].Add(entity);
                }
            }

            foreach(var customTrack in this.recorder.customTracks)
            {
                if (!VTACMI.IncludeEW && customTrack.Value.keyframeType == typeof(RadarJammer.JammerKeyframe))
                    continue;

                var keyFrames = customTrack.Value.keyframes;

                foreach (ReplayRecorder.Keyframe keyFrame in keyFrames)
                {
                    if (!tracksKeyFrameDict.ContainsKey(keyFrame.t))
                    {
                        tracksKeyFrameDict.Add(keyFrame.t, new List<object>());
                    }

                    tracksKeyFrameDict[keyFrame.t].Add(customTrack.Value);
                }
            }

            foreach (ReplayRecorder.Keyframe keyFrame in this.recorder.eventTrack.keyframes)
            {
                if (!tracksKeyFrameDict.ContainsKey(keyFrame.t))
                {
                    tracksKeyFrameDict.Add(keyFrame.t, new List<object>());
                }

                tracksKeyFrameDict[keyFrame.t].Add(this.recorder.eventTrack);
            }

            List<BulletReplay> bullets = GetBullets();

            Console.WriteLine($"Bullet Count: {bullets.Count}");
            Console.WriteLine($"Keyframe Count: {tracksKeyFrameDict.Count}");

            using (var streamWriter = new StreamWriter(tacviewSavePath, false, Encoding.UTF8))
            {
                streamWriter.WriteLine("FileType=text/acmi/tacview");
                streamWriter.WriteLine("FileVersion=2.2");

                int currentProgress = 0;
                int maxProgress = tracksKeyFrameDict.Count;
                ACMILoadingBar.AddTotalKeyFrameCount(maxProgress);

                int prevPercentage = 0;
                foreach (KeyValuePair<float, List<object>> trackKeyframe in tracksKeyFrameDict)
                {
                    float t = trackKeyframe.Key;

                    bool bulletSameFrame = VTACMI.IncludeBullets ? ConvertBullets(streamWriter, bullets, t) : false;
                    //bool bulletSameFrame = false;

                    if (!bulletSameFrame)
                        streamWriter.WriteLine($"#{t}");
                        

                    foreach (var track in trackKeyframe.Value)
                    {
                        switch (track)
                        {
                            case ReplayRecorder.ReplayEntity entity:
                                //Handles VFM Missiles
                                if(entity.metaData == null)
                                {
                                    Console.WriteLine($"Null meta data, entity {entity.id}");
                                    entity.metaData = new ReplayRecorder.TrackMetadata();
                                    entity.metaData.label = "AIM-9";
                                    entity.metaData.identity = -1;
                                    entity.metaData.label = "AIM-9";
                                    entity.entityType = (int)ReplayActorEntityTypes.Missile;
                                }
                                string tacviewString = !entity.initalized ? BuildInitString(entity, t) : BuildUpdateString(entity, t);
                                streamWriter.WriteLine(tacviewString);
                                break;
                            case CustomTrack customTrack:
                                HandleCustomTrack(streamWriter, customTrack, t);
                                break;
                            case EventTrack eventTrack:
                                HandleEventTrack(streamWriter, eventTrack, t);
                                break;
                            default:
                                break;
                        }

                    }

                    currentProgress++;
                    ACMILoadingBar.AdvanceFrameProgress();

                    int roundedPercentage = Mathf.FloorToInt((currentProgress*100)/maxProgress);
                    if(Program.ConsoleMode && roundedPercentage % 1 == 0 && prevPercentage != roundedPercentage)
                    {
                        ACMIUtils.ClearCurrentConsoleLine();
                        Console.WriteLine($"Progress: {roundedPercentage}%");
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                        prevPercentage = roundedPercentage;
                    }
                }

                //Hacky way to despawn flares
                for(int i = 0; i < this.flareCount; i++)
                {
                    streamWriter.WriteLine($"#{this.flareDespawnTimes[i]}");
                    streamWriter.WriteLine($"-{acmiHex.GetFlareHex(i)}");
                }
            }


            ACMIUtils.ReplaceWithZippedVersion(tacviewSavePath);

            float fileSize = new FileInfo(tacviewSavePath).Length;
            fileSize /= 1024*1024;

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            Console.WriteLine($"Final File Size: {fileSize.ToString("0.00")} MB");
            Console.WriteLine($"Finished converting {folderName}: {elapsedMs/1000f} seconds");
            
        }

        public static void DebugVTR(string vtrPath)
        {
            ReplayRecorder recorder = new ReplayRecorder();
            recorder.Awake();
            ReplaySerializer.LoadFromFile(vtrPath, recorder);

            List<ReplayRecorder.ReplayEntity> entitites = recorder.GetAllEntities();

            Console.WriteLine($"Motion Tracks: {recorder.motionTracks.Count}");
            Console.WriteLine($"Event Tracks: {recorder.eventTrack.Count}");
            Console.WriteLine($"Custom Tracks: {recorder.customTracks.Count}");

            foreach (var motionTrack in entitites)
            {
                //Console.WriteLine($"{motionTrack.metaData.label}");
            }

            foreach (var track in recorder.eventTrack.keyframes)
            {

                //Console.WriteLine($"{(EventTypes)track.eventType} : {track.t}");
            }

            foreach (var track in recorder.customTracks)
            {

                if (track.Value.keyframeType == typeof(VTRPooledProjectile.PooledProjectileKeyframe))
                {
                    var customTrack = track.Value;
                    var metaData = track.Value.metadata;
                    Console.WriteLine($"{track.Value.keyframeType}");
                    Console.WriteLine($"Keyframe Count: {track.Value.keyframes.Count}");
                    Console.WriteLine();


                }
            }


        }

        //TODO: Implement
        public static void DebugVFM(string vfmPath)
        {

        }

        private string BuildInitString(ReplayRecorder.ReplayEntity entity, float time)
        {
            entity.initalized = true;

            string updateString = BuildUpdateString(entity, time);
            string typeString = GetType((ReplayActorEntityTypes)entity.entityType);
            if (entity.metaData.label.ToLower().Contains("carrier"))
            {
                typeString = "AircraftCarrier";
            }

            string shapeString = this.isVFM ? GetShapeVFM(entity) : GetShape(entity);
            string colorString = GetColor((ReplayActorEntityTypes)entity.entityType);
            string coalitionString = GetCoalition((ReplayActorEntityTypes)entity.entityType);

            string builtString = $"{updateString},Name={entity.metaData.label},Type={typeString},LongName={entity.metaData.label},ShortName={entity.metaData.label}{colorString},Shape={shapeString}{coalitionString}";

            return builtString;
        }

        private string BuildUpdateString(ReplayRecorder.ReplayEntity entity, float time)
        {
            string entityHex = this.acmiHex.GetEntityHex(entity.id);
            Vector3 position;
            Vector3 eulerRotation;
            Quaternion rotation;
            bool lastFrame;

            ACMIUtils.GetPositionAndRotation(this.recorder.motionTracks[entity.id], time, out position, out eulerRotation, out rotation, out lastFrame);

            if (entity.metaData.label.Contains("Carrier"))
            {
                Vector3 temp = position;
                temp.y = position.y - 16f;
                position = temp;

                position += rotation * Vector3.forward * +94f;
            }

            Vector3D gpsPosition = ACMIUtils.WorldPositionToGPSCoords(position);
            this.recorder.motionTracks[entity.id].currentGPSPosition = gpsPosition;

            string gpsString = $"{gpsPosition.y.Invariant()}|{gpsPosition.x.Invariant()}|{gpsPosition.z.Invariant()}|{(-eulerRotation.z).Invariant()}|{(-eulerRotation.x).Invariant()}|{eulerRotation.y.Invariant()}";

            string builtString = $"{entityHex},T={gpsString}";
            builtString += lastFrame ? $"\n-{entityHex}" : "";

            return builtString;
        }

        private void HandleCustomTrack(StreamWriter streamWriter, CustomTrack customTrack, float t)
        {
            if (IncludeEW && customTrack.keyframeType == typeof(RadarJammer.JammerKeyframe))
            {
                var metaData = (RadarJammer.ReplayMetadata)customTrack.metadata;
                var entity = this.recorder.GetEntity(metaData.actorReplayId);

                string tacviewString = !customTrack.initalized ? BuildJammerInitString(customTrack, entity, t) : BuildJammerUpdateString(customTrack, entity, t);
                streamWriter.WriteLine(tacviewString);
            }
            else if (customTrack.keyframeType == typeof(LockingRadar.RadarLockKeyframe))
            {
                var metaData = (LockingRadar.RadarLockReplayMetadata)customTrack.metadata;
                var entity = this.recorder.GetEntity(metaData.actorId);

                string tacviewString = BuildRadarLockString(customTrack, entity, t);
                streamWriter.WriteLine(tacviewString);
            }
            else if (customTrack.keyframeType == typeof(VTRPooledProjectile.PooledProjectileKeyframe))
            {
                var metaData = (VTRPooledProjectile.PooledProjectileMetadata)customTrack.metadata;

                string tacviewString = BuildPooledUpdateString(customTrack, t);
                streamWriter.WriteLine(tacviewString);
            }
        }

        private void HandleEventTrack(StreamWriter streamWriter, EventTrack eventTrack, float t)
        {
            int segmentIndex;
            ACMIUtils.FindSegment<ReplayRecorder.EventKeyframe>(eventTrack, out segmentIndex, t);

            if(segmentIndex >= 0 && eventTrack[segmentIndex].eventType == (int)EventTypes.Flare)
            {
                string flareHex = this.acmiHex.GetFlareHex(this.flareCount);
                this.flareCount++;

                ReplayRecorder.WorldEventKeyframe flareEvent = (ReplayRecorder.WorldEventKeyframe)eventTrack[segmentIndex];
                FixedPoint fp = flareEvent.fp;
                Quaternion rotation = flareEvent.rotation;


                Vector3 pos = fp.globalPoint.toVector3;
                Vector3 eulerRotation = ACMIUtils.ToEulerAngles(rotation);

                Vector3D gpsPosition = ACMIUtils.WorldPositionToGPSCoords(pos);
                string gpsString = $"{gpsPosition.y.Invariant()}|{gpsPosition.x.Invariant()}|{gpsPosition.z.Invariant()}|{(-eulerRotation.z).Invariant()}|{(-eulerRotation.x).Invariant()}|{eulerRotation.y.Invariant()}";
                string flareString = $"{flareHex},T={gpsString},Type=Flare";

                this.flareDespawnTimes.Add(t + VTACMI.FlareLifeTime);
                streamWriter.WriteLine(flareString);
            }else if (segmentIndex >= 0 && eventTrack[segmentIndex].eventType == (int)EventTypes.Damage)
            {

                DamageKeyframe damageFrame = (DamageKeyframe)eventTrack[segmentIndex];
                string entityHex = acmiHex.GetEntityHex(damageFrame.targetId);

                ReplayRecorder.ReplayEntity entity = this.recorder.GetEntity(damageFrame.targetId);

                float damage = 0.16f;
                float minHealth = 1 - (damage * 4f);

                entity.pseudoHealth -= entity.pseudoHealth <= minHealth ? 0f : damage;
                string colorChangeString = $"{entityHex},Visible={entity.pseudoHealth}";
                streamWriter.WriteLine(colorChangeString);
            }
        }

        private string BuildJammerInitString(CustomTrack customTrack, ReplayRecorder.ReplayEntity entity, float t)
        {
            customTrack.initalized = true;
            string updateString = BuildJammerUpdateString(customTrack, entity, t);
            string typeString = "Missile";
            string shapeString = "vtolvr_ConePog.obj";
            string parentHex = this.acmiHex.GetEntityHex(entity.id);

            string builtString = $"{updateString},Name=Jam,Type={typeString},Shape={shapeString},Parent={parentHex}";
            return builtString;
        }

        private string BuildJammerUpdateString(CustomTrack customTrack, ReplayRecorder.ReplayEntity entity, float t)
        {
            int segmentIndex;
            ACMIUtils.FindSegment<ReplayRecorder.Keyframe>(customTrack, out segmentIndex, t);

            if (segmentIndex >= 0)
            {
                string jammerHex = this.acmiHex.GetJammerHex(customTrack.trackId);

                RadarJammer.JammerKeyframe jammerKeyFrame1 = (RadarJammer.JammerKeyframe)customTrack.keyframes[segmentIndex];
                int nextIndex = segmentIndex >= customTrack.keyframes.Count - 1 ? segmentIndex : segmentIndex + 1;

                RadarJammer.JammerKeyframe jammerKeyFrame2 = (RadarJammer.JammerKeyframe)customTrack.keyframes[nextIndex];
                float lerpT = Mathf.InverseLerp(jammerKeyFrame1.t, jammerKeyFrame2.t, t);


                Vector3 lerpedDirection = ACMIUtils.Slerp(jammerKeyFrame1.direction, jammerKeyFrame2.direction, lerpT);

                Vector3D gpsPosition = this.recorder.motionTracks[entity.id].currentGPSPosition;

                float jammerYaw = Vector3.SignedAngle(Vector3.forward, lerpedDirection, Vector3.up);

                float jammerPitch = Mathf.Asin(-lerpedDirection.y) * Mathf.Rad2Deg;
                string gpsString = $"{gpsPosition.y.Invariant()}|{gpsPosition.x.Invariant()}|{gpsPosition.z.Invariant()}|{0}|{(-jammerPitch).Invariant()}|{jammerYaw.Invariant()}";

                string color = GetJammerColor(jammerKeyFrame1.transmitMode, jammerKeyFrame1.band);


                bool isVisible = jammerKeyFrame1.type == RadarJammer.JammerKeyframe.KeyframeType.Stop || jammerKeyFrame2.type == RadarJammer.JammerKeyframe.KeyframeType.Stop || segmentIndex >= customTrack.keyframes.Count - 1;

                string visibleText = isVisible ? "0" : "0.25";
                string builtString = $"{jammerHex},T={gpsString},Color={color},Visible={visibleText}";
                //builtString += segmentIndex >= customTrack.keyframes.Count-2 ? $"\n-{jammerHex}" : "";


                return builtString;
            }

            return string.Empty;
        }

        private string BuildPooledUpdateString(CustomTrack customTrack, float t)
        {
            int segmentIndex;
            ACMIUtils.FindSegment<ReplayRecorder.Keyframe>(customTrack, out segmentIndex, t);

            if (segmentIndex >= 0)
            {
                string pooledHex = this.acmiHex.GetProjectileHex(customTrack.trackId, customTrack.reinitalizedCount);

                VTRPooledProjectile.PooledProjectileKeyframe pooledProjectileKeyframe = (VTRPooledProjectile.PooledProjectileKeyframe)customTrack.keyframes[segmentIndex];

                Vector3 globalPos = pooledProjectileKeyframe.globalPos;
                Vector3D gpsPosition = ACMIUtils.WorldPositionToGPSCoords(globalPos);
                int active = pooledProjectileKeyframe.active ? 1 : 0;
                string typeString = "Rocket";
                string shapeString = "Rocket.Hydra 70.obj";

                string tacviewString = $"{pooledHex},T={gpsPosition.y.Invariant()}|{gpsPosition.x.Invariant()}|{gpsPosition.z.Invariant()},Visible={active}";
                if (!customTrack.initalized)
                {
                    tacviewString += $",Name=Rocket,Shape={shapeString},Type={typeString},Color=Orange";
                    customTrack.initalized = true;
                }

                if (!pooledProjectileKeyframe.active)
                {
                    customTrack.initalized = false;
                    customTrack.reinitalizedCount++;
                }
               
                return tacviewString;

            }

            return string.Empty;
        }

        private string BuildRadarLockString(CustomTrack customTrack, ReplayRecorder.ReplayEntity entity, float t)
        {
            int segmentIndex;
            ACMIUtils.FindSegment<ReplayRecorder.Keyframe>(customTrack, out segmentIndex, t);

            if (segmentIndex >= 0)
            {
                string entityHex = this.acmiHex.GetEntityHex(entity.id);
                LockingRadar.RadarLockKeyframe lockKeyFrame = (LockingRadar.RadarLockKeyframe)customTrack.keyframes[segmentIndex];

                if (lockKeyFrame.targetId >= 0)
                    return $"{entityHex},LockedTargetMode=1,LockedTarget={this.acmiHex.GetEntityHex(lockKeyFrame.targetId)}";


                return $"{entityHex},LockedTargetMode=0,LockedTarget=-1";
            }

            return string.Empty;
        }

        private bool ConvertBullets(StreamWriter streamWriter, List<BulletReplay> bullets, float t)
        {
            int whileLoopRan = 0;
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

                        }

                        frameSetTrigger = false;
                        string tacviewUpdateString = BuildBulletUpdateString(bullet, bullet.currentT);
                        streamWriter.WriteLine(tacviewUpdateString);



                        if (bullet.currentT == bullet.endT)
                        {
                            bullet.currentT += 5f;
                            continue;
                        }

                        bullet.currentT += VTACMI.BulletPollRate;
                        bullet.currentT = Math.Min(bullet.currentT, bullet.endT);
                    }
                }
            }
            

            if(whileLoopRan > 1)
                Console.WriteLine($"While Loop Ran: {whileLoopRan}");

            return bulletSameFrame;
        }

        private List<BulletReplay> GetBullets()
        {
            List<BulletReplay> bullets = new List<BulletReplay>();

            foreach (var eventTrack in this.recorder.eventTrack.keyframes)
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
                        bullet.lifeTime = this.isVFM ? bulletEvent.endT - bullet.startT : bulletEvent.lifeTime;
                        bullet.endT = this.isVFM ? bulletEvent.endT : bullet.startT + bullet.lifeTime;

                        bullets.Add(bullet);
                        continue;
                    }

                    if (eventTrack is BulletEndKeyframe bulletEndKeyframe && bulletEndKeyframe.bId < bullets.Count && bulletEndKeyframe.bId >= 0)
                    {
                        bullets[bulletEndKeyframe.bId].endT = bulletEndKeyframe.t;
                    }
                }

            }

            List<BulletReplay> sortedList = bullets.OrderBy(b => b.startT).ToList();

            return sortedList;
            
        }

        private string BuildBulletUpdateString(BulletReplay bullet, float t)
        {
            string entityHex = this.acmiHex.GetBulletHex(bullet.bId);
            if (t >= bullet.endT)
            {
                return $"-{entityHex}";
            }

            Vector3 position = bullet.GetBulletPos(t);
            Vector3D gpsPosition = ACMIUtils.WorldPositionToGPSCoords(position);
            string gpsString = $"{gpsPosition.y.Invariant()}|{gpsPosition.x.Invariant()}|{gpsPosition.z.Invariant()}";
            string typeString = "Bullet";

            string builtString = $"{entityHex},T={gpsString}";
            if (!bullet.initiliazed)
            {
                builtString += $",Type={typeString}";
                bullet.initiliazed = true;
            }


            return builtString;
        }

        private string GetType(ReplayActorEntityTypes entityType)
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

        private string GetShape(ReplayRecorder.ReplayEntity entity)
        {
            ReplayActorEntityTypes entityType = (ReplayActorEntityTypes)entity.entityType;
            string name = entity.metaData.label;
           

            foreach(ACMIObjects.TacviewObject unit in ACMIObjects.UnitList)
            {
                if (name.Contains(unit.vtolName))
                {
                    return unit.tacviewObjName;
                }
                
            }
            
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

        private string GetShapeVFM(ReplayRecorder.ReplayEntity entity)
        {
            int type = entity.metaData.identity;
            string shapeString;
            switch (type)
            {
                case 0:
                    shapeString = "FixedWing.F-15.obj";
                    break;
                case 1:
                    shapeString = "FixedWing.F-16.obj";
                    break;
                case 2:
                    shapeString = "FixedWing.F-18E.obj";
                    break;
                case -1:
                    shapeString = "Missile.AIM-9M.obj";
                    break;
                default:
                    shapeString = "FixedWing.F-15.obj";
                    break;
            }

            return shapeString;
        }

        private string GetColor(ReplayActorEntityTypes entityType)
        {
            string colorString;
            switch (entityType)
            {
                case ReplayActorEntityTypes.AirA:
                case ReplayActorEntityTypes.GroundA:
                case ReplayActorEntityTypes.SeaA:
                case ReplayActorEntityTypes.ChopperA:
                    colorString = ",Color=Blue";
                    break;
                case ReplayActorEntityTypes.AirB:
                case ReplayActorEntityTypes.GroundB:
                case ReplayActorEntityTypes.SeaB:
                case ReplayActorEntityTypes.ChopperB:
                    colorString = ",Color=Red";
                    break;
                case ReplayActorEntityTypes.Missile:
                    colorString = ",Color=Orange";
                    break;
                default:
                    colorString = "Orange";
                    break;
            }

            return colorString;
        }

        private string GetJammerColor(RadarJammer.TransmitModes transmitMode, EMBands band)
        {
            switch (transmitMode)
            {
                case RadarJammer.TransmitModes.NOISE:
                    switch (band) 
                    {
                        case EMBands.High:
                            return "Cyan";
                        case EMBands.Mid:
                            return "Orange";
                        case EMBands.Low:
                            return "Violet";
                    }

                    return "Orange";
                case RadarJammer.TransmitModes.DRFM:
                    return "Green";
                case RadarJammer.TransmitModes.SAS:
                    return "Blue";
                default:
                    return "Cyan";
            }
        }

        private string GetCoalition(ReplayActorEntityTypes entityType)
        {
            string coalitionString;
            switch (entityType)
            {
                case ReplayActorEntityTypes.AirA:
                case ReplayActorEntityTypes.GroundA:
                case ReplayActorEntityTypes.SeaA:
                case ReplayActorEntityTypes.ChopperA:
                    coalitionString = ",Coalition=Allies";
                    return coalitionString;
                case ReplayActorEntityTypes.AirB:
                case ReplayActorEntityTypes.GroundB:
                case ReplayActorEntityTypes.SeaB:
                case ReplayActorEntityTypes.ChopperB:
                    coalitionString = ",Coalition=Enemies";
                    return coalitionString;
                default:
                    coalitionString = "";
                    return coalitionString;
            }

        }

        public enum ReplayActorEntityTypes
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
