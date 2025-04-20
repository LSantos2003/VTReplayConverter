using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VTReplayConverter;


public class RadarJammer
{
	public enum TransmitModes
	{
		NOISE,
		DRFM,
		SAS
	}

	public class JammerKeyframe : ReplayRecorder.Keyframe
	{
		public RadarJammer.JammerKeyframe.KeyframeType type;

		public Vector3 direction;

		public RadarJammer.TransmitModes transmitMode;

		public EMBands band;

		protected override void OnSerialize()
		{
			base.OnSerialize();
			ReplaySerializer.WriteByte((byte)this.type);
			ReplaySerializer.WriteByte((byte)this.transmitMode);
			ReplaySerializer.WriteByte((byte)this.band);
			ReplaySerializer.WriteVector3(this.direction);
		}

		protected override void OnDeserialize()
		{
			base.OnDeserialize();
			this.type = (RadarJammer.JammerKeyframe.KeyframeType)ReplaySerializer.ReadByte();
			this.transmitMode = (RadarJammer.TransmitModes)ReplaySerializer.ReadByte();
			this.band = (EMBands)ReplaySerializer.ReadByte();
			this.direction = ReplaySerializer.ReadVector3();
		}

		public enum KeyframeType
		{
			Start,
			Stop
		}
	}

	public class ReplayMetadata : ReplaySerializer.IReplaySerializable
	{
		public int actorReplayId;

		public void ReplayDeserialize()
		{
			this.actorReplayId = ReplaySerializer.ReadInt();
		}

		public void ReplaySerialize()
		{
			ReplaySerializer.WriteInt(this.actorReplayId);
		}
	}
}

