using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VTReplayConverter
{
    public class ACMILoadingBar
    {
        public static int maxKeyFrameCount { get; private set; }
        public static int currentKeyFrameProgress { get; private set; }

        public static int GetKeyFrameProgress()
        {
            if(maxKeyFrameCount == 0)
                return 0;

            int roundedPercentage = Mathf.FloorToInt((currentKeyFrameProgress * 100) / maxKeyFrameCount);
            roundedPercentage = Mathf.Clamp(roundedPercentage, 0, 100);
            return roundedPercentage;
        }

        public static void AddTotalKeyFrameCount(int totalKeyFrames)
        {
            maxKeyFrameCount += totalKeyFrames;
        }

        public static void AdvanceFrameProgress()
        {
            currentKeyFrameProgress++;

        }
        public static void ResetBar()
        {
            maxKeyFrameCount = 0;
            currentKeyFrameProgress = 0;
        }
    }
}
