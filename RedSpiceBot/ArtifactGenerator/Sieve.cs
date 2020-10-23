// Copyright © 2016 phil.o@codeproject.com
//
// This source code is released under the Code Project Open License 1.02.
// Please see http://www.codeproject.com/info/cpol10.aspx

using System;
using System.Linq;
using System.Text;

namespace RedSpiceBot.ArtifactGenerator
{
	/// <summary>
	/// Implements an object which, given specified <see cref="FrequencyMap{T}"/> and <see cref="Position"/>, is able to
	/// issue a random <typeparamref name="T"/> value based on the values of frequencies in the map at given position.
	/// </summary>
	/// <typeparam name="T"><see cref="Type"/> of <see cref="FrequencyMap{T}"/>'s keys.</typeparam>
	public sealed class Sieve<T>
	{
		#region Fields
		/// <summary>
		/// Default minimum frequency for a <see cref="Sieve{T}"/>.
		/// </summary>
		public const int DefaultMinFrequency = 1;

		private int _minFrequency;
		private Random _random;
		#endregion

		#region Properties
		/// <summary>
		/// <para>Gets or sets this <see cref="Sieve{T}"/> instance's minimum frequency while computing cumulative sums of
		/// frequencies.</para>
		/// <para>During random generation process, frequencies are lower-bounded to <see cref="MinFrequency"/>, to give a
		/// chance for inexistent chains in samples to appear. Increasing <see cref="MinFrequency"/> will make more likely
		/// for inexistant chains to occur, while setting it to zero will prevent them from appearing.</para>
		/// </summary>
		/// <remarks>Provided <pararef name="value"/> to setter is itself lower-bounded to zero.</remarks>
		public int MinFrequency
		{
			get { return _minFrequency; }
			set { _minFrequency = Math.Max(0, value); }
		}

		/// <summary>
		/// Gets or sets the <see cref="Random"/> used in <see cref="GetValue(FrequencyMap{T}, Position)"/>.
		/// </summary>
		/// <remarks>When set to null, this prperty is automatically initialized to a new default
		/// <see cref="System.Random"/>.</remarks>
		public Random Random
		{
			get { return _random; }
			set { _random = value ?? new Random(); }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new <see cref="Sieve{T}"/> instance with specified <see cref="Random"/> and minimum frequency.
		/// </summary>
		/// <param name="minFrequency">Value to which all frequencies will be bounded when constructing the table used for
		/// issuing values. A minimum frequency greater than zero will give a chance to inexistent chains in the samples
		/// to still have a chance to appear in the random choice process. A minimum frequency of zero will make it
		/// impossible to happen. A value lower than zero will be bounded to zero.</param>
		/// <param name="random"><see cref="Random"/> which will be used in
		/// <see cref="GetValue(FrequencyMap{T}, Position)"/> method.</param>
		public Sieve(int minFrequency = DefaultMinFrequency, Random random = null)
		{
			Random = random;
			MinFrequency = minFrequency;
		}
		#endregion

		#region Methods
		/// <summary>
		/// Returns an array containing the cumulative sum of frequencies at specified <see cref="Position"/> in specified
		/// <see cref="FrequencyMap{T}"/> value collection.
		/// </summary>
		/// <param name="map"><see cref="FrequencyMap{T}"/> whose cumulative sum of frequencies to get for
		/// <paramref name="position"/>.</param>
		/// <param name="position"><see cref="Position"/> in <paramref name="map"/>'s value collection to
		/// consider.</param>
		/// <returns>An array containing the cumulative sum of frequencies at <paramref name="position"/> in
		/// <paramref name="map"/>.</returns>
		public int[] GetBuckets(FrequencyMap<T> map, Position position)
		{
			int[] buckets = new int[map.Count];
			int sum = 0;
			int index = 0;
			StateFrequency frequency;
			int value;
			foreach (var pair in map)
			{
				frequency = pair.Value;
				value = Math.Max(_minFrequency, frequency.GetValue(position));
				sum += value;
				buckets[index++] = sum;
			}
			return buckets;
		}

		/// <summary>
		/// Gets a random <typeparamref name="T"/> key from specified <see cref="FrequencyMap{T}"/> based on all values at
		/// specified <see cref="Position"/> in map's frequencies.
		/// </summary>
		/// <param name="map"><see cref="FrequencyMap{T}"/> used for issuing values.</param>
		/// <param name="position"><see cref="Position"/> in <paramref name="map"/>'s frequencies to consider issuing
		/// values for.</param>
		/// <returns>A random <see cref="T"/> value from <paramref name="map"/>'s keys based on frequencies at
		/// <paramref name="position"/> in the map.</returns>
		/// <exception cref="NullReferenceException"><paramref name="map"/> is null.</exception>
		public T GetValue(FrequencyMap<T> map, Position position)
		{
			int[] buckets = GetBuckets(map, position);
			int length = buckets.Length;
			int total = buckets[length - 1];
			int value = _random.Next(0, total);
			int index = 0;
			for (; index < length; index++)
			{
				if (value < buckets[index])
				{
					break;
				}
			}
			return map.Keys.ToList()[index];
		}
		#endregion
	}
}
