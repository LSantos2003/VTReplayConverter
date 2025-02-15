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

		public float mass;

		public int bId;

		public float lifeTime;

		private static int nextId;

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
			this.mass = ReplaySerializer.ReadFloat();
			this.bId = ReplaySerializer.ReadInt();
			this.lifeTime = (float)ReplaySerializer.ReadByte();
		}

		public static void RecordStopEvent(int bId)
		{
			BulletEndKeyframe bulletEndKeyframe = new BulletEndKeyframe();
			bulletEndKeyframe.eventType = 1;
			bulletEndKeyframe.bId = bId;
		}
	}
}
