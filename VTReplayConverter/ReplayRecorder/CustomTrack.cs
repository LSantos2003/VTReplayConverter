using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTReplayConverter
{
	public class CustomTrack : Track<ReplayRecorder.Keyframe>
	{
		public int trackId;

		public Type keyframeType;

		public ReplaySerializer.IReplaySerializable metadata;

		//VTReplayConvert Stuff
		public bool initalized = false;

		public int reinitalizedCount = 0;
	}
}
