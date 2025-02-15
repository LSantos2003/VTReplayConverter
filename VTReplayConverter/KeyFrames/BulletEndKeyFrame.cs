using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTReplayConverter;

namespace VTOLVR.ReplaySystem
{
	public class BulletEndKeyframe : ReplayRecorder.EventKeyframe
	{
		public int bId;

		protected override void OnSerialize()
		{
			base.OnSerialize();
			ReplaySerializer.WriteInt(this.bId);
		}

		protected override void OnDeserialize()
		{
			base.OnDeserialize();
			this.bId = ReplaySerializer.ReadInt();
		}
	}
}
