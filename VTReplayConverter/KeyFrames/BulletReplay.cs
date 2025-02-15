using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VTReplayConverter
{
	public class BulletReplay
	{

		public int bId;

		public Vector3 velocity;

		public float mass;

		public float dragArea;

		public float startT;

		public float currentT;

		public float endT;

		public float lastRT;

		public Vector3 globalPt;

		public float lifeTime;

		public Vector3 GetBulletPos(float t)
		{

			float d = t - this.startT;
			Vector3 a = new Vector3(0f, -9.81f, 0f);
			Vector3 globalPoint = this.globalPt + this.velocity * d + 0.5f * a * d * d;
			Vector3 forward = this.velocity + a * d;

			return globalPoint;
		}
	}
}
