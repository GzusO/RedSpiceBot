// Copyright © 2016 phil.o@codeproject.com
//
// This source code is released under the Code Project Open License 1.02.
// Please see http://www.codeproject.com/info/cpol10.aspx

namespace RedSpiceBot.ArtifactGenerator
{
	/// <summary>
	/// Represents the distinct positions any element can be in a collection.
	/// </summary>
	public enum Position : byte
	{
		/// <summary>
		/// The element is the first in the collection.
		/// </summary>
		First,

		/// <summary>
		/// The element is inside the collection.
		/// </summary>
		Inner,

		/// <summary>
		/// The element is the last in the collection.
		/// </summary>
		Last
	}
}