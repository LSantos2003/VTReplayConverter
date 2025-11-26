using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VTReplayConverter;

namespace VTOLVR.ReplaySystem
{
	public class BulletEventKeyframe : ReplayRecorder.EventKeyframe
	{
		public FixedPoint fp;

		public Vector3 velocity;

		//Not in VFM
		public float mass;

		public int bId;

		//In VFM
		public FixedPoint endPt;
		public Vector3 endVel;
		public float endT;

		//Not in VFM
		public float lifeTime;

		private static int nextId;

		private List<int> bulletIds = new List<int>();
		protected override void OnSerialize()
		{
			base.OnSerialize();
			ReplaySerializer.WriteFixedPoint(this.fp);
			ReplaySerializer.WriteVector3(this.velocity);
			ReplaySerializer.WriteFloat(this.mass);
			ReplaySerializer.WriteInt(this.bId);
			ReplaySerializer.WriteByte((byte)Mathf.CeilToInt(this.lifeTime));
		}

		protected override void OnDeserialize()
		{
			base.OnDeserialize();
			this.fp = ReplaySerializer.ReadFixedPoint();
			this.velocity = ReplaySerializer.ReadVector3();
			if (!ReplaySerializer.ConvertingVFM) { this.mass = ReplaySerializer.ReadFloat(); }
			this.bId = ReplaySerializer.ReadInt();

			if (ReplaySerializer.ConvertingVFM)
            {
				this.endPt = ReplaySerializer.ReadFixedPoint();
				this.endVel = ReplaySerializer.ReadVector3();
				this.endT = ReplaySerializer.ReadFloat();
			}

			if (!ReplaySerializer.ConvertingVFM){this.lifeTime = (float)ReplaySerializer.ReadByte();}
		}

		private string VectorToString(Vector3 vector)
        {
			return $"{vector.x},{vector.y},{vector.z}";
        }
		public static void RecordStopEvent(int bId)
		{
			BulletEndKeyframe bulletEndKeyframe = new BulletEndKeyframe();
			bulletEndKeyframe.eventType = 1;
			bulletEndKeyframe.bId = bId;
		}
	}
}
