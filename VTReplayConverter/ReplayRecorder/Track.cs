using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTReplayConverter
{
	public class Track<T> where T : ReplayRecorder.Keyframe
	{
		public List<T> keyframes { get; private set; } = new List<T>();

		public float startTime
		{
			get
			{
				if (this.keyframes.Count > 0)
				{
					return this.keyframes[0].t;
				}
				return 0f;
			}
		}

		public float endTime
		{
			get
			{
				if (this.keyframes.Count > 0)
				{
					return this.keyframes[this.keyframes.Count - 1].t;
				}
				return 0f;
			}
		}

		public T this[int i]
		{
			get
			{
				return this.keyframes[i];
			}
		}

		public void Add(T kf)
		{
			this.keyframes.Add(kf);
		}

		public int Count
		{
			get
			{
				return this.keyframes.Count;
			}
		}
	}
}
