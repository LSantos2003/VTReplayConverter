using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VTReplayConverter;

namespace VTOLVR.ReplaySystem
{
	class VTRPooledProjectile
	{
		public class PooledProjectileMetadata : ReplaySerializer.IReplaySerializable
		{
			public void ReplayDeserialize()
			{
			}

			public void ReplaySerialize()
			{
			}

			public int getEntityId() 
			{ 
				return -1; 
			}
            public void setEntityId(int id)
			{
			}
        }

		public class PooledProjectileKeyframe : ReplayRecorder.InterpolatedKeyframe
		{
			public bool active;

			public Vector3 globalPos;

			public Vector3 velocity;

			public override void Interpolate(ReplayRecorder.InterpolatedKeyframe otherFrame, float lerpT, float simTime)
			{
				if (!this.active)
				{
					return;
				}
				VTRPooledProjectile.PooledProjectileKeyframe pooledProjectileKeyframe = (VTRPooledProjectile.PooledProjectileKeyframe)otherFrame;
				Vector3 a = this.globalPos + this.velocity * simTime;
				float d = pooledProjectileKeyframe.t - this.t - simTime;
				Vector3 b = pooledProjectileKeyframe.globalPos - pooledProjectileKeyframe.velocity * d;
				Vector3 globalPoint = Vector3.Lerp(a, b, lerpT);
				Vector3 forward = Vector3.Lerp(this.velocity, pooledProjectileKeyframe.velocity, lerpT);
			}

			protected override void OnSerialize()
			{
				base.OnSerialize();
				ReplaySerializer.WriteBool(this.active);
				ReplaySerializer.WriteVector3(this.globalPos);
				ReplaySerializer.WriteVector3(this.velocity);
			}

			protected override void OnDeserialize()
			{
				base.OnDeserialize();
				this.active = ReplaySerializer.ReadBool();
				this.globalPos = ReplaySerializer.ReadVector3();
				this.velocity = ReplaySerializer.ReadVector3();
			}
		}
	}

}
