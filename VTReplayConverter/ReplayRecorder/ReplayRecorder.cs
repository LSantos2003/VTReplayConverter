using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VTReplayConverter
{
	public class ReplayRecorder
	{
		public Dictionary<int, MotionTrack> motionTracks = new Dictionary<int, MotionTrack>();

		public EventTrack eventTrack = new EventTrack();

		private List<ReplayRecorder.ReplayEntity> trackedEntities = new List<ReplayRecorder.ReplayEntity>();

		public Dictionary<int, ReplayRecorder.ReplayEntity> entityDict = new Dictionary<int, ReplayRecorder.ReplayEntity>();

		private int nextId;

		public Dictionary<int, CustomTrack> customTracks = new Dictionary<int, CustomTrack>();

		private float startTime;

		private bool acceptFinalKeys;

		public float keyframeInterval = 0.2f;

		private int framesSkippedForConstantV;

		private int framesSkippedForNonMovement;

		public static ReplayRecorder Instance { get; private set; }

		public void Awake()
		{
			ReplayRecorder.Instance = this;
		}

		public void Reset()
        {
			ReplaySerializer.ClearSerializedReplay();
			ReplayRecorder.Instance = null;
		}
		public int keyframeCount { get; private set; }

		public void RecountKeys()
		{
			this.keyframeCount = 0;
			this.totalDuration = 0f;
			foreach (MotionTrack motionTrack in this.motionTracks.Values)
			{
				this.keyframeCount += motionTrack.Count;
				this.totalDuration = Mathf.Max(this.totalDuration, motionTrack.endTime);
			}
			foreach (CustomTrack customTrack in this.customTracks.Values)
			{
				this.keyframeCount += customTrack.Count;
				this.totalDuration = Mathf.Max(this.totalDuration, customTrack.endTime);
			}
			this.keyframeCount += this.eventTrack.Count;
			this.totalDuration = Mathf.Max(this.totalDuration, this.eventTrack.endTime);
		}

		public float totalDuration { get; private set; }

		public ReplayRecorder.ReplayEntity GetEntity(int entityId)
		{
			ReplayRecorder.ReplayEntity result;
			if (this.entityDict.TryGetValue(entityId, out result))
			{
				return result;
			}
			return null;
		}

		public List<ReplayRecorder.ReplayEntity> GetAllEntities()
		{
			List<ReplayRecorder.ReplayEntity> list = new List<ReplayRecorder.ReplayEntity>();
			foreach (ReplayRecorder.ReplayEntity item in this.entityDict.Values)
			{
				list.Add(item);
			}
			return list;
		}

		public IEnumerable<ReplayRecorder.ReplayEntity> EnumerateEntities()
		{
			return this.entityDict.Values;
		}

		public void SetEntities(List<ReplayRecorder.ReplayEntity> entities)
		{
			this.entityDict.Clear();
			foreach (ReplayRecorder.ReplayEntity replayEntity in entities)
			{
				this.entityDict.Add(replayEntity.id, replayEntity);
			}
		}


		public static bool IsRecording { get; private set; }

		public static event Action OnBeginRecording;

		public static event Action OnEndRecording;


		private float CurrentT
		{
			get
			{
				return UnityEngine.Time.time - this.startTime;
			}
		}


		public class ReplayEntity
		{
			public int id;

			public ReplayRecorder.IReplayTrackable trackable;

			public int entityType;

			public ReplayRecorder.TrackMetadata metaData;

			//VTReplayConvert Stuff
			public bool initalized = false;
		}

		public class Keyframe
		{
			public float t;

			public void Serialize()
			{
				this.OnSerialize();
			}

			protected virtual void OnSerialize()
			{
				ReplaySerializer.WriteFloat(this.t);
			}

			public void Deserialize()
			{
				this.OnDeserialize();
			}

			protected virtual void OnDeserialize()
			{
				this.t = ReplaySerializer.ReadFloat();
			}
		}

		public class InterpolatedKeyframe : ReplayRecorder.Keyframe
		{
			public virtual void Interpolate(ReplayRecorder.InterpolatedKeyframe otherFrame, float lerpT, float simTime)
			{
			}
		}

		public class MotionKeyframe : ReplayRecorder.InterpolatedKeyframe
		{
			public FixedPoint fp;

			public Vector3 velocity;

			public Quaternion rotation;

			protected override void OnSerialize()
			{
				base.OnSerialize();
				ReplaySerializer.WriteFixedPoint(this.fp);
				ReplaySerializer.WriteVector3(this.velocity);
				ReplaySerializer.WriteInt(VTNetUtils.QuaternionToInt(this.rotation));
			}

			protected override void OnDeserialize()
			{
				base.OnDeserialize();
				this.fp = ReplaySerializer.ReadFixedPoint();
				this.velocity = ReplaySerializer.ReadVector3();
				this.rotation = VTNetUtils.IntToQuaternion(ReplaySerializer.ReadInt());
			}

			public void SerializeDelta(ReplayRecorder.MotionKeyframe prevKf)
			{
				ReplaySerializer.WriteFloat(this.t - prevKf.t);
				Vector3 toVector = (this.fp.globalPoint - prevKf.fp.globalPoint).toVector3;
				Vector3 v = this.velocity - prevKf.velocity;
				int num = VTNetUtils.QuaternionToInt(this.rotation) - VTNetUtils.QuaternionToInt(prevKf.rotation);
				bool flag = toVector.sqrMagnitude > 1f;
				bool flag2 = v.sqrMagnitude > 1f;
				bool flag3 = num != 0;
				byte b = 0;
				if (flag)
				{
					b |= 1;
				}
				if (flag2)
				{
					b |= 2;
				}
				if (flag3)
				{
					b |= 4;
				}
				ReplaySerializer.WriteByte(b);
				if (flag)
				{
					ReplaySerializer.WriteVector3(toVector);
				}
				if (flag2)
				{
					ReplaySerializer.WriteVector3(v);
				}
				if (flag3)
				{
					ReplaySerializer.WriteInt(num);
				}
			}

			public void DeserializeDelta(ReplayRecorder.MotionKeyframe prevKf)
			{
				this.t = ReplaySerializer.ReadFloat() + prevKf.t;
				byte b = ReplaySerializer.ReadByte();
				if ((b & 1) == 1)
				{
					Vector3 vector = ReplaySerializer.ReadVector3();
					this.fp = new FixedPoint(prevKf.fp.globalPoint + new Vector3D(vector));
				}
				else
				{
					this.fp = prevKf.fp;
				}
				if ((b & 2) == 2)
				{
					Vector3 b2 = ReplaySerializer.ReadVector3();
					this.velocity = prevKf.velocity + b2;
				}
				else
				{
					this.velocity = prevKf.velocity;
				}
				if ((b & 4) == 4)
				{
					int num = ReplaySerializer.ReadInt();
					this.rotation = VTNetUtils.IntToQuaternion(VTNetUtils.QuaternionToInt(prevKf.rotation) + num);
					return;
				}
				this.rotation = prevKf.rotation;
			}

			public override void Interpolate(ReplayRecorder.InterpolatedKeyframe otherFrame, float lerpT, float simTime)
			{
				ReplayRecorder.MotionKeyframe motionKeyframe = (ReplayRecorder.MotionKeyframe)otherFrame;
				Vector3 a = this.fp.globalPoint.toVector3 + this.velocity * simTime;
				Vector3 b = motionKeyframe.fp.globalPoint.toVector3 - motionKeyframe.velocity * (motionKeyframe.t - this.t - simTime);
				Vector3 a2 = Vector3.Lerp(a, b, lerpT);
				Quaternion quaternion = Quaternion.Lerp(this.rotation, motionKeyframe.rotation, lerpT);
			}
		}

		public class EventKeyframe : ReplayRecorder.Keyframe
		{
			public int eventType;

			protected override void OnSerialize()
			{
				base.OnSerialize();
				ReplaySerializer.WriteByte((byte)this.eventType);
			}

			protected override void OnDeserialize()
			{
				base.OnDeserialize();
				this.eventType = (int)ReplaySerializer.ReadByte();
			}
		}

		public class TrackMetadata
		{
			public int identity;

			public string label;

			private static byte[] metaUtfBuffer = new byte[1024];

			public void Serialize()
			{
				ReplaySerializer.WriteInt(this.identity);
				int bytes = Encoding.UTF8.GetBytes(this.label, 0, this.label.Length, ReplayRecorder.TrackMetadata.metaUtfBuffer, 0);
				ReplaySerializer.WriteInt(bytes);
				for (int i = 0; i < bytes; i++)
				{
					ReplaySerializer.WriteByte(ReplayRecorder.TrackMetadata.metaUtfBuffer[i]);
				}
			}

			public void Deserialize()
			{
				this.identity = ReplaySerializer.ReadInt();
				int num = ReplaySerializer.ReadInt();
				for (int i = 0; i < num; i++)
				{
					ReplayRecorder.TrackMetadata.metaUtfBuffer[i] = ReplaySerializer.ReadByte();
				}
				this.label = Encoding.UTF8.GetString(ReplayRecorder.TrackMetadata.metaUtfBuffer, 0, num);
			}
		}

		public interface IReplayTrackable
		{
			void RecordReplayData(out FixedPoint fp, out Vector3 velocity, out Quaternion rotation);
		}

		public class WorldEventKeyframe : ReplayRecorder.EventKeyframe
		{
			public FixedPoint fp;

			public Quaternion rotation;

			protected override void OnSerialize()
			{
				base.OnSerialize();
				ReplaySerializer.WriteFixedPoint(this.fp);
				ReplaySerializer.WriteInt(VTNetUtils.QuaternionToInt(this.rotation));
			}

			protected override void OnDeserialize()
			{
				base.OnDeserialize();
				this.fp = ReplaySerializer.ReadFixedPoint();
				this.rotation = VTNetUtils.IntToQuaternion(ReplaySerializer.ReadInt());
			}
		}
	}
}
