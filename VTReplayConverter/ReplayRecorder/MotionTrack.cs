using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VTReplayConverter
{
	public class MotionTrack : Track<ReplayRecorder.MotionKeyframe>
	{
		public int entityId;

		//VTACMI Variable. Stores the current position of a motion track
		public Vector3D currentGPSPosition;

		public Vector3D nextGPSPosition;
	}
}
