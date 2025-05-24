using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTReplayConverter
{
    public class ACMIHex
    {
        //Note that the random "B"'s are "buffers" to segment certain data
        public ACMIHex(int conversionId)
        {
            this.offset = conversionId >= 0;
            this.conversionId = conversionId;
        }

        private bool offset = false;
        private int conversionId;

        public string GetEntityHex(int trackId)
        {
            if (this.offset)
            {
                return $"{this.conversionId.ToString("X")}B{trackId+1}";
            }
            return (trackId + 1).ToString();
        }

        public string GetJammerHex(int jammerId)
        {
            if (this.offset)
            {
                return $"{this.conversionId.ToString("X")}B{jammerId + 1}C";
            }
            return (jammerId + 1).ToString() + "C";
        }

        public string GetBulletHex(int bulletId)
        {
            if (this.offset)
            {
                return $"{this.conversionId.ToString("X")}B{bulletId + 1}B";
            }
            return (bulletId + 1).ToString() + "B";
        }

        public string GetProjectileHex(int projectileId, int instance)
        {
            if (this.offset)
            {
                return $"{this.conversionId.ToString("X")}B{projectileId + 1}A{instance}";
            }
            return (projectileId + 1).ToString() + "A" + instance.ToString();
        }

    }
}
