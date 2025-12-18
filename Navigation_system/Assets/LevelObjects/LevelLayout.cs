using System;
using UnityEngine;

namespace LevelObjects
{
	[Serializable]
	public class LevelLayout
	{
		public Vector2IntSerializable[] obstaclePositions;
		public Vector3Serializable carPosition;
		public Vector4Serializable carRotation;
		public Vector3Serializable targetPosition;
	}

	[Serializable]
	public struct Vector2IntSerializable
	{
		public int x;
		public int y;

		public Vector2IntSerializable(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public static implicit operator Vector2Int(Vector2IntSerializable v)
			=> new Vector2Int(v.x, v.y);

		public static implicit operator Vector2IntSerializable(Vector2Int v)
			=> new Vector2IntSerializable(v.x, v.y);
	}

	[Serializable]
	public struct Vector3Serializable
	{
		public float x;
		public float y;
		public float z;

		public Vector3Serializable(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public static implicit operator Vector3(Vector3Serializable v)
			=> new Vector3(v.x, v.y, v.z);

		public static implicit operator Vector3Serializable(Vector3 v)
			=> new Vector3Serializable(v.x, v.y, v.z);
	}

	[Serializable]
	public struct Vector4Serializable
	{
		public float x;
		public float y;
		public float z;
		public float w;

		public Vector4Serializable(float x, float y, float z, float w)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}

		public static implicit operator Quaternion(Vector4Serializable v)
			=> new Quaternion(v.x, v.y, v.z, v.w);

		public static implicit operator Vector4Serializable(Quaternion q)
			=> new Vector4Serializable(q.x, q.y, q.z, q.w);
	}
}