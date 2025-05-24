using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTReplayConverter;

public class LockingRadar
{
	public class RadarLockReplayMetadata : ReplaySerializer.IReplaySerializable
	{
		public int actorId;

		public void ReplayDeserialize()
		{
			this.actorId = ReplaySerializer.ReadInt();
		}

		public void ReplaySerialize()
		{
			ReplaySerializer.WriteInt(this.actorId);
		}

        public int getEntityId() 
		{
			return actorId;
		}
        public void setEntityId(int id) 
		{
			this.actorId = id;
		}
    }

	public class RadarLockKeyframe : ReplayRecorder.Keyframe
	{
		public int targetId;

		protected override void OnSerialize()
		{
			base.OnSerialize();
			ReplaySerializer.WriteInt(this.targetId);
		}

		protected override void OnDeserialize()
		{
			base.OnDeserialize();
			this.targetId = ReplaySerializer.ReadInt();
		}
    }
}

