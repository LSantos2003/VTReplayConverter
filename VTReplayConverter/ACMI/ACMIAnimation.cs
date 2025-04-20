using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VTReplayConverter
{
    public class ACMIAnimation
    {

        public static void ExplosionAnimation(StreamWriter writer, ReplayRecorder.WorldEventKeyframe explosion, int explosionId)
        {
            string entityHex = (explosionId + 1).ToString() + "E";
            Vector3 position = explosion.fp.point;
            Vector3D gpsPosition = ACMIUtils.WorldPositionToGPSCoords(position);
            string gpsString = $"{gpsPosition.x}|{gpsPosition.y}|{gpsPosition.z}";
            string typeString = "Projectile + Explosion";

            string explosionString = $"{entityHex},Type={typeString},T={gpsString},Radius=0.1";
            writer.WriteLine(explosionString);


            /*writer.WriteLine($"#{bullet.currentT + 1f}");
            string despawnExplosion = $"{entityHex},Visible=0";
            writer.WriteLine(despawnExplosion);*/

        }
    }
}
