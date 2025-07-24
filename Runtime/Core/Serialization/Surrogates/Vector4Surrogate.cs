using System.Runtime.Serialization;
using UnityEngine;

namespace UnitySimplified.Serialization
{
	public sealed class Vector4Surrogate : ISerializationSurrogate
	{
		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			Vector4 vector4 = (Vector4)obj;
			info.AddValue("x", vector4.x);
			info.AddValue("y", vector4.y);
			info.AddValue("z", vector4.z);
			info.AddValue("w", vector4.w);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			Vector4 vector4 = (Vector4)obj;
            vector4.x = (float)info.GetValue("x", typeof(float));
            vector4.y = (float)info.GetValue("y", typeof(float));
            vector4.z = (float)info.GetValue("z", typeof(float));
            vector4.w = (float)info.GetValue("w", typeof(float));
			return vector4;
		}
	}
}