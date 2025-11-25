using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTReplayConverter
{
	public class DamageKeyframe : ReplayRecorder.EventKeyframe
	{

		public int targetId;


		protected override void OnSerialize()
		{
		}

		protected override void OnDeserialize()
		{
			base.OnDeserialize();
			this.targetId = ReplaySerializer.ReadInt();
		}


		public DamageKeyframe()
		{
		}
	}
}
