using System;
using UnityEngine;

[Serializable]
public struct LeafDNA2
{
	[Header("Body")]
	[Range(0, 10)] public float height;
	[Range(0, 10)] public float width;

	[Header("Petiole")]
	[Range(0, 10)] public int petioleHeight;
	[Range(0, 10)] public int petioleWidth;

	[Header("Midrib")]
	[Range(0, 10)] public int midribHeight;
	[Range(0, 10)] public int midribWidth;

	public LeafBlade firstBlade;
	public LeafBlade secondBlade;
	public LeafBlade thirdBlade;

	public int TotalWidthWeight
	{
		get
		{
			int sum = 0;
			sum += petioleWidth;
			sum += midribWidth;

			return sum;
		}
	}

	public int TotalHeightWeight
	{
		get
		{
			int sum = 0;
			sum += petioleHeight;
			sum += midribHeight;

			return sum;
		}
	}


	[Serializable]
	public struct LeafBlade
	{
		public int width;
		public int height;

		public int firstSectionWidth;
		public int secondSectionWidth;
		public int thirdSectionWidth;

		public int TotalWeight => firstSectionWidth + secondSectionWidth + thirdSectionWidth;
	}


}