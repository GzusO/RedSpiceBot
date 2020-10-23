// Copyright © 2016 phil.o@codeproject.com
//
// This source code is released under the Code Project Open License 1.02.
// Please see http://www.codeproject.com/info/cpol10.aspx

using System;

namespace RedSpiceBot.ArtifactGenerator
{
	/// <summary>
	/// Implements an object for handling positional (<see cref="Position"/>) frequencies.
	/// </summary>
	[Serializable]
	public class StateFrequency : IEquatable<StateFrequency>
	{
		#region Fields
		private int _firstCount;
		private int _innerCount;
		private int _lastCount;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the frequency related to <see cref="Position.First"/>.
		/// </summary>
		public int FirstCount
		{
			get { return _firstCount; }
		}

		/// <summary>
		/// Gets the frequency related to <see cref="Position.Inner"/>.
		/// </summary>
		public int InnerCount
		{
			get { return _innerCount; }
		}

		/// <summary>
		/// Gets the frequency related to <see cref="Position.Last"/>.
		/// </summary>
		public int LastCount
		{
			get { return _lastCount; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Creates a new instance of <see cref="StateFrequency"/> with all frequencies initialized to zero.
		/// </summary>
		public StateFrequency()
			: this(0, 0, 0) { }

		private StateFrequency(int firstCount, int innerCount, int lastCount)
		{
			_firstCount = firstCount;
			_innerCount = innerCount;
			_lastCount = lastCount;
		}
		#endregion

		#region Methods
		#region Public Methods
		/// <summary>
		/// Adds the values of specified <see cref="StateFrequency"/> instance to those of this instance.
		/// </summary>
		/// <param name="other"><see cref="StateFrequency"/> to add to this instance.</param>
		/// <exception cref="NullReferenceException"><paramref name="other"/> is null.</exception>
		public void Add(StateFrequency other)
		{
			IncrementFirst(other._firstCount);
			IncrementInner(other._innerCount);
			IncrementLast(other._lastCount);
		}

		/// <summary>
		/// Returns the value associated to specified <see cref="Position"/>.
		/// </summary>
		/// <param name="position"><see cref="Position"/> to consider.</param>
		/// <returns>
		/// <para>If <paramref name="position"/> is <see cref="Position.First"/>, then <see cref="FirstCount"/>.</para>
		/// <para>If <paramref name="position"/> is <see cref="Position.Inner"/>, then <see cref="InnerCount"/>.</para>
		/// <para>If <paramref name="position"/> is <see cref="Position.Last"/>, then <see cref="LastCount"/>.</para>
		/// </returns>
		public int GetValue(Position position)
		{
			switch (position)
			{
				case Position.First:
					return _firstCount;
				case Position.Last:
					return _lastCount;
				case Position.Inner:
				default:
					return _innerCount;
			}
		}

		/// <summary>
		/// Increments this <see cref="StateFrequency"/> instance's values by a given amount.
		/// </summary>
		/// <param name="count">Value to add to those of this <see cref="StateFrequency"/> instance.</param>
		public void Increment(int count = 1)
		{
			IncrementFirst(count);
			IncrementInner(count);
			IncrementLast(count);
		}

		/// <summary>
		/// Increments the value associated to specified <see cref="Position"/> by specified amount.
		/// </summary>
		/// <param name="position"><see cref="Position"/> to consider.</param>
		/// <param name="count">Amount by which incrementing value relative to <paramref name="position"/>.</param>
		public void Increment(Position position, int count = 1)
		{
			switch (position)
			{
				case Position.First:
					IncrementFirst(count);
					break;
				case Position.Last:
					IncrementLast(count);
					break;
				case Position.Inner:
				default:
					IncrementInner(count);
					break;
			}
		}

		/// <summary>
		/// Increments the <see cref="FirstCount"/> value by a given amount.
		/// </summary>
		/// <param name="count">Value to increment <see cref="FirstCount"/> by.</param>
		public void IncrementFirst(int count = 1)
		{
			_firstCount += count;
		}

		/// <summary>
		/// Increments the <see cref="InnerCount"/> value by a given amount.
		/// </summary>
		/// <param name="count">Value to increment <see cref="InnerCount"/> by.</param>
		public void IncrementInner(int count = 1)
		{
			_innerCount += count;
		}

		/// <summary>
		/// Increments the <see cref="LastCount"/> value by a given amount.
		/// </summary>
		/// <param name="count">Value to increment <see cref="LastCount"/> by.</param>
		public void IncrementLast(int count = 1)
		{
			_lastCount += count;
		}
		#endregion

		#region IEquatable<StateFrequency> Implementation
		/// <summary>
		/// Returns whether a given <see cref="StateFrequency"/> equals this instance.
		/// </summary>
		/// <param name="other"><see cref="StateFrequency"/> to compare this instance to.</param>
		/// <returns>True if this <see cref="StateFrequency"/> and <paramref name="other"/> are same instance, or if
		/// <paramref name="other"/> is not null and its respective values match those of this instance. Otherwise, false.
		/// </returns>
		public bool Equals(StateFrequency other)
		{
			if (other == null)
			{
				return false;
			}
			if (ReferenceEquals(this, other))
			{
				return true;
			}
			return ((other.FirstCount == _firstCount) && (other._innerCount == _innerCount)
				&& (other._lastCount == _lastCount));
		}
		#endregion

		#region Overrides
		/// <summary>
		/// Returns whether a given <see cref="object"/> equals this <see cref="StateFrequency"/> instance.
		/// </summary>
		/// <param name="obj"><see cref="object"/> to compare this <see cref="StateFrequency"/> instance to.</param>
		/// <returns>True if <see cref="obj"/> is a <see cref="StateFrequency"/> instance which equals this instance.
		///  Otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return (obj is StateFrequency) ? Equals((StateFrequency)obj) : false;
		}

		/// <summary>
		/// Returns a hash value for this <see cref="StateFrequency"/> instance.
		/// </summary>
		/// <returns>A hash value for this <see cref="StateFrequency"/> instance.</returns>
		public override int GetHashCode()
		{
			int hash = 351;
			hash += _firstCount.GetHashCode();
			hash *= 13;
			hash += _innerCount.GetHashCode();
			hash *= 13;
			return hash + _lastCount.GetHashCode();
		}

		/// <summary>
		/// Returns a string representation of this <see cref="StateFrequency"/>.
		/// </summary>
		/// <returns>String representation of this <see cref="StateFrequency"/>.</returns>
		public override string ToString()
		{
			return $"First: {_firstCount}; Inner: {_innerCount}; Last: {_lastCount}";
		}
		#endregion
		#endregion
	}
}