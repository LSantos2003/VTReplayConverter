using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using K4os.Compression.LZ4;
using UnityEngine;
using VTBitConverter;
using VTOLVR.ReplaySystem;

namespace VTReplayConverter
{
	public class ReplaySerializer
	{
		private static byte[] kfBuffer = new byte[1024];

		private static byte[] lz4Buffer = new byte[1];

		private static ReplaySerializer.SerializedReplay serializedReplay;

		private static int kfBufferPos = 0;

		private static Dictionary<int, Type> keyframeTypes;

		private static Dictionary<Type, int> keyframeTypeIndices;

		private static byte[] decompressBuffer = new byte[1];

		private static Stream dsStream = null;


		public static void ClearSerializedReplay()
		{
			kfBuffer = new byte[1024];
			lz4Buffer = new byte[1];
			decompressBuffer = new byte[1];
			
			ReplaySerializer.serializedReplay = null;
		}

		public static bool isSerializing { get; private set; }

		public static void WriteFloat(float f)
		{
			VTBitConverter.BitConverter.GetBytes(f, ReplaySerializer.kfBuffer, ReplaySerializer.kfBufferPos);
			ReplaySerializer.kfBufferPos += 4;
		}

		public static void WriteBool(bool b)
		{
			ReplaySerializer.kfBuffer[ReplaySerializer.kfBufferPos] = ((byte)(b ? 1 : 0));
			ReplaySerializer.kfBufferPos++;
		}

		public static void WriteByte(byte b)
		{
			ReplaySerializer.kfBuffer[ReplaySerializer.kfBufferPos] = b;
			ReplaySerializer.kfBufferPos++;
		}

		public static void WriteInt(int i)
		{
			VTBitConverter.BitConverter.GetBytes(i, ReplaySerializer.kfBuffer, ReplaySerializer.kfBufferPos);
			ReplaySerializer.kfBufferPos += 4;
		}

		public static void WriteVector3(Vector3 v)
		{
			ReplaySerializer.WriteFloat(v.x);
			ReplaySerializer.WriteFloat(v.y);
			ReplaySerializer.WriteFloat(v.z);
		}

		public static void WriteFixedPoint(FixedPoint p)
		{
			ReplaySerializer.WriteVector3(p.globalPoint.toVector3);
		}

		public static void WriteString(string s)
		{
			int bytes = Encoding.UTF8.GetBytes(s, 0, s.Length, ReplaySerializer.kfBuffer, ReplaySerializer.kfBufferPos + 4);
			VTBitConverter.BitConverter.GetBytes(bytes, ReplaySerializer.kfBuffer, ReplaySerializer.kfBufferPos);
			ReplaySerializer.kfBufferPos += 4 + bytes;
		}

		private static void CreateTypeDictionary()
		{
			if (ReplaySerializer.keyframeTypes != null)
			{
				return;
			}
			ReplaySerializer.keyframeTypes = new Dictionary<int, Type>();
			ReplaySerializer.keyframeTypes.Add(0, typeof(ReplayRecorder.MotionKeyframe));
			ReplaySerializer.keyframeTypes.Add(1, typeof(ReplayRecorder.EventKeyframe));
			ReplaySerializer.keyframeTypes.Add(2, typeof(ReplayRecorder.WorldEventKeyframe));
			ReplaySerializer.keyframeTypes.Add(3, typeof(BulletEventKeyframe));
			ReplaySerializer.keyframeTypes.Add(4, typeof(BulletEndKeyframe));
			ReplaySerializer.keyframeTypeIndices = new Dictionary<Type, int>();
			foreach (KeyValuePair<int, Type> keyValuePair in ReplaySerializer.keyframeTypes)
			{
				ReplaySerializer.keyframeTypeIndices.Add(keyValuePair.Value, keyValuePair.Key);
			}
		}

		public static void LoadFromFile(string filepath, ReplayRecorder recorder)
		{
			ReplaySerializer.Deserialize(filepath, recorder);
		}

		public static void Deserialize(string filepath, ReplayRecorder recorder)
		{
			if (File.Exists(filepath))
			{
				ReplaySerializer.lz4Buffer = File.ReadAllBytes(filepath);
				ReplaySerializer.Deserialize(ReplaySerializer.lz4Buffer, ReplaySerializer.lz4Buffer.Length, recorder);
				return;
			}
			Console.WriteLine("Tried to load replay but file doees not exist: " + filepath);
		}

		public static void Deserialize(byte[] buffer, int count, ReplayRecorder recorder)
		{
			try
			{
				ReplaySerializer.serializedReplay = new ReplaySerializer.SerializedReplay();
				ReplaySerializer.serializedReplay.data = buffer;
				int num = count * 255;
				if (ReplaySerializer.decompressBuffer.Length < num)
				{
					ReplaySerializer.decompressBuffer = new byte[num];
				}
				int num2 = LZ4Codec.Decode(buffer, 0, count, ReplaySerializer.decompressBuffer, 0, ReplaySerializer.decompressBuffer.Length);
				Console.WriteLine(string.Format("Replay Disk Size = {0}", num2));
				using (MemoryStream memoryStream = new MemoryStream(ReplaySerializer.decompressBuffer, 0, num2))
				{
					ReplaySerializer.Deserialize(memoryStream, recorder);
				}
			}
			catch (Exception arg)
			{
				Console.WriteLine(string.Format("Exception thrown when trying to deserialize replay:\n{0}", arg));
				ReplaySerializer.serializedReplay = null;
				recorder.motionTracks.Clear();
				recorder.eventTrack.keyframes.Clear();
				recorder.RecountKeys();
			}
		}

		private static void Deserialize(Stream fs, ReplayRecorder recorder)
		{
			ReplaySerializer.CreateTypeDictionary();
			ReplaySerializer.dsStream = fs;
			int num = ReplaySerializer.ReadInt();
			if (num == 1)
			{
				ReplaySerializer.DeserializeV1(fs, recorder);
				ReplaySerializer.dsStream = null;
				return;
			}
			ReplaySerializer.dsStream = null;
			throw new Exception(string.Format("ReplaySerializer Error: Unsupported format version: {0}", num));
		}

		private static void DeserializeV1(Stream fs, ReplayRecorder recorder)
		{
			List<ReplayRecorder.ReplayEntity> list = new List<ReplayRecorder.ReplayEntity>();
			recorder.motionTracks.Clear();
			int num = ReplaySerializer.ReadInt();
			for (int i = 0; i < num; i++)
			{
				MotionTrack motionTrack = new MotionTrack();
				motionTrack.entityId = ReplaySerializer.ReadInt();
				ReplayRecorder.ReplayEntity replayEntity = new ReplayRecorder.ReplayEntity();
				replayEntity.id = motionTrack.entityId;
				replayEntity.entityType = ReplaySerializer.ReadInt();
				list.Add(replayEntity);
				if (ReplaySerializer.ReadByte() > 0)
				{
					ReplayRecorder.TrackMetadata trackMetadata = new ReplayRecorder.TrackMetadata();
					trackMetadata.Deserialize();
					replayEntity.metaData = trackMetadata;
				}
				int num2 = ReplaySerializer.ReadInt();
				ReplayRecorder.MotionKeyframe prevKf = null;
				for (int j = 0; j < num2; j++)
				{
					ReplayRecorder.MotionKeyframe motionKeyframe = new ReplayRecorder.MotionKeyframe();
					if (j == 0)
					{
						motionKeyframe.Deserialize();
					}
					else
					{
						motionKeyframe.DeserializeDelta(prevKf);
					}
					prevKf = motionKeyframe;
					motionTrack.Add(motionKeyframe);
				}
				recorder.motionTracks.Add(motionTrack.entityId, motionTrack);
			}
			recorder.SetEntities(list);
			recorder.customTracks.Clear();
			int num3 = ReplaySerializer.ReadInt();
			for (int k = 0; k < num3; k++)
			{
				CustomTrack customTrack = new CustomTrack();
				customTrack.trackId = ReplaySerializer.ReadInt();
				string text = ReplaySerializer.ReadString();
				Type type = Type.GetType(text);
				customTrack.keyframeType = type;
				string text2 = ReplaySerializer.ReadString();
				Type type2 = Type.GetType(text2);
				if (type2 == null)
				{
					Console.WriteLine("Failed to parse metadata type: " + text2 + " for keyframeType: " + text);
				}
				object obj = Activator.CreateInstance(type2);
				customTrack.metadata = (ReplaySerializer.IReplaySerializable)obj;
				((ReplaySerializer.IReplaySerializable)obj).ReplayDeserialize();
				int num4 = ReplaySerializer.ReadInt();
				for (int l = 0; l < num4; l++)
				{
					ReplayRecorder.Keyframe keyframe = (ReplayRecorder.Keyframe)Activator.CreateInstance(type);
					keyframe.Deserialize();
					customTrack.Add(keyframe);
				}
				recorder.customTracks.Add(customTrack.trackId, customTrack);
			}
			recorder.eventTrack.keyframes.Clear();
			int num5 = ReplaySerializer.ReadInt();
			for (int m = 0; m < num5; m++)
			{
				int key = (int)ReplaySerializer.ReadByte();
				ReplayRecorder.EventKeyframe eventKeyframe = (ReplayRecorder.EventKeyframe)Activator.CreateInstance(ReplaySerializer.keyframeTypes[key]);
				eventKeyframe.Deserialize();
				recorder.eventTrack.Add(eventKeyframe);
			}
			recorder.RecountKeys();
		}

		public static int ReadInt()
		{
			ReplaySerializer.dsStream.Read(ReplaySerializer.kfBuffer, 0, 4);
			return System.BitConverter.ToInt32(ReplaySerializer.kfBuffer, 0);
		}

		public static bool ReadBool()
		{
			ReplaySerializer.dsStream.Read(ReplaySerializer.kfBuffer, 0, 1);
			return ReplaySerializer.kfBuffer[0] > 0;
		}

		public static byte ReadByte()
		{
			ReplaySerializer.dsStream.Read(ReplaySerializer.kfBuffer, 0, 1);
			return ReplaySerializer.kfBuffer[0];
		}

		public static float ReadFloat()
		{
			ReplaySerializer.dsStream.Read(ReplaySerializer.kfBuffer, 0, 4);
			return System.BitConverter.ToSingle(ReplaySerializer.kfBuffer, 0);
		}

		public static Vector3 ReadVector3()
		{
			return new Vector3
			{
				x = ReplaySerializer.ReadFloat(),
				y = ReplaySerializer.ReadFloat(),
				z = ReplaySerializer.ReadFloat()
			};
		}

		public static FixedPoint ReadFixedPoint()
		{
			Vector3 vector = ReplaySerializer.ReadVector3();
			return new FixedPoint(new Vector3D(vector));
		}

		public static string ReadString()
		{
			int count = ReplaySerializer.ReadInt();
			ReplaySerializer.dsStream.Read(ReplaySerializer.kfBuffer, 0, count);
			return Encoding.UTF8.GetString(ReplaySerializer.kfBuffer, 0, count);
		}

		public interface IReplaySerializable
		{
			void ReplaySerialize();

			void ReplayDeserialize();
		}

		public class SerializedReplay
		{
			public byte[] data;
		}
	}
}
