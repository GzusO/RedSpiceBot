// Copyright © 2016 phil.o@codeproject.com
//
// This source code is released under the Code Project Open License 1.02.
// Please see http://www.codeproject.com/info/cpol10.aspx

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RedSpiceBot.ArtifactGenerator
{
	/// <summary>
	/// Implements	the <see cref="Map{T, StateFrequency}"/> which is used for handling Markov Chain maps.
	/// </summary>
	/// <typeparam name="T"><see cref="Type"/> of Markov Chain's states.</typeparam>
	[Serializable]
	public sealed class FrequencyMap<T> : Map<T, StateFrequency>
	{
		#region Constructors
		/// <summary>
		/// Initializes a new <see cref="FrequencyMap{T}"/> instance which is empty, has the default initial capacity,
		/// and uses the default <see cref="IEqualityComparer{T}"/>.
		/// </summary>
		public FrequencyMap()
			: base() { }

		/// <summary>
		/// Initializes a new <see cref="FrequencyMap{T}"/> instance which is empty, has the specified initial capacity,
		/// and uses the default <see cref="IEqualityComparer{T}"/>.
		/// </summary>
		public FrequencyMap(int capacity)
			: base(capacity) { }

		/// <summary>
		/// Initializes a new <see cref="FrequencyMap{T}"/> instance which contains elements copied from specified
		/// <see cref="IDictionary{T, StateFrequency}"/> and uses the default <see cref="IEqualityComparer{T}"/>.
		/// </summary>
		public FrequencyMap(IDictionary<T, StateFrequency> dictionary)
			: base(dictionary) { }

		private FrequencyMap(SerializationInfo info, StreamingContext context)
			: base(info, context) { }
		#endregion

		#region Methods
		#region Public Methods
		/// <summary>
		/// <para>Merges the contents of this <see cref="FrequencyMap{T}"/> instance with those of specified
		/// <see cref="IDictionary{T, StateFrequency}"/>, and returns the number of
		/// <see cref="KeyValuePair{T, StateFrequency}"/> that have been added.</para>
		/// <para>When a key in <paramref name="dictionary"/> is not present in this instance's collection, then the
		/// foreign pair is added to the collection.</para>
		/// <para>When a key is already present, <see cref="StateFrequency"/> value of foreign pair is added to current
		/// value associated to the key.</para>
		/// </summary>
		/// <param name="dictionary"><see cref="IDictionary{TKey, TValue}"/> to merge with this
		/// <see cref="FrequencyMap{T}"/>.</param>
		/// <returns>Number of <see cref="KeyValuePair{T, StateFrequency}"/> which have been added to the collection.
		/// Already present keys whose values have been added to foreign ones are not counted.</returns>
		/// <exception cref="NullReferenceException"><paramref name="dictionary"/> is null.</exception>
		public int Merge(IDictionary<T, StateFrequency> dictionary)
		{
			int count = 0;
			T key;
			StateFrequency value;
			foreach (var pair in dictionary)
			{
				key = pair.Key;
				value = pair.Value;
				if (!EventuallyAdd(key, value))
				{
					this[key].Add(value);
				}
				else
				{
					count++;
				}
			}
			return count;
		}
		#endregion

		#region Overrides
		/// <summary>
		/// Returns a string representation of this <see cref="FrequencyMap{T}"/>.
		/// </summary>
		/// <returns>String representation of this <see cref="FrequencyMap{T}"/>.</returns>
		public override string ToString()
		{
			return $"{Count} state frequencies";
		}
		#endregion
		#endregion
	}
}
