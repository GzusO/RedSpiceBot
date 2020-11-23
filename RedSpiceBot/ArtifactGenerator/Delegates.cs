// Copyright © 2016 phil.o@codeproject.com
//
// This source code is released under the Code Project Open License 1.02.
// Please see http://www.codeproject.com/info/cpol10.aspx

using System;
using System.Collections.Generic;

namespace RedSpiceBot.ArtifactGenerator
{
	/// <summary>
	/// Defines a method which is able to join a given number of discrete values into a new one.
	/// </summary>
	/// <typeparam name="T"><see cref="Type"/> of values to join.</typeparam>
	/// <param name="values">Values to use to form a new value.</param>
	/// <param name="start">Index in <paramref name="values"/> to start from.</param>
	/// <param name="count">Number of values to use from <paramref name="start"/>.</param>
	/// <returns>A new value from the <paramref name="count"/> items in <paramref name="values"/>, starting at
	/// <paramref name="start"/>.</returns>
	/// <remarks>Reciprocal of <see cref="PartitionFunc{T}"/>.</remarks>
	public delegate T AggregationFunc<T>(IEnumerable<T> values, int start, int count);

	/// <summary>
	/// Defines a method which is able to split a given value into discrete, unitary values.
	/// </summary>
	/// <typeparam name="T"><see cref="Type"/> of element to partition.</typeparam>
	/// <param name="value">Value to partition.</param>
	/// <returns>An array of <typeparamref name="T"/> values from <paramref name="value"/>.</returns>
	/// <remarks>Reciprocal of <see cref="AggregationFunc{T}"/>.</remarks>
	public delegate T[] PartitionFunc<T>(T value);

	/// <summary>
	/// Defines a method which, given a <typeparamref name="T"/> value and the list of its preceding states in the chain,
	/// will issue the decision whether or not to accept the value.
	/// </summary>
	/// <typeparam name="T"><see cref="Type"/> of chain's elements.</typeparam>
	/// <param name="state">Proposed state.</param>
	/// <param name="precedingStates">List of preceding states.</param>
	/// <returns>True if <paramref name="state"/> is acceptable as a value following <paramref name="precedingStates"/>,
	/// otherwise false.</returns>
	public delegate bool ValidationFunc<T>(T state, IEnumerable<T> precedingStates);
}