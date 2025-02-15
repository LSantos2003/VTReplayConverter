using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTReplayConverter
{
	public class MotionTrack : Track<ReplayRecorder.MotionKeyframe>
	{
		public int entityId;
	}
}
