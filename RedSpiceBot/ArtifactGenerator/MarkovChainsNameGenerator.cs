// Copyright © 2016 phil.o@codeproject.com
//
// This source code is released under the Code Project Open License 1.02.
// Please see http://www.codeproject.com/info/cpol10.aspx

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RedSpiceBot.ArtifactGenerator
{
	/// <summary>
	/// Implements an object which uses <see cref="MarkovChainMap{T}"/> of strings to generate random words or names.
	/// </summary>
	public sealed class MarkovChainsNameGenerator
	{
		#region Fields
		private const int DefaultDepth = MarkovChainMap<string>.DefaultDepth;
		private const int DefaultMinFrequency = Sieve<string>.DefaultMinFrequency;
		private const int DefaultMaxLength = 9;
		private const int DefaultMinLength = 3;
		private const int Maximum = int.MaxValue - 1;

		private readonly HashSet<string> _blacklistedNames;
		private bool _capitalize;
		private MarkovChainMap<string> _map;
		private readonly MarkovChainStringMapBuilder _mapBuilder;
		private int _mapDepth;
		private int _maxLength;
		private int _minFrequency;
		private int _minLength;
		private Random _random;
		private readonly StringBuilder _sb;
		private readonly Sieve<string> _sieve;
		private bool _skipWhitespace;
		#endregion

		#region Properties
		#region Public Properties
		/// <summary>
		/// Gets or sets whether results produced by <see cref="GetName"/> method will be capitalized.
		/// </summary>
		public bool Capitalize
		{
			get { return _capitalize; }
			set { _capitalize = value; }
		}

		/// <summary>
		/// Gets or sets the <see cref="MarkovChainMap{T}"/> used to produce random outputs.
		/// </summary>
		/// <exception cref="ArgumentNullException">The <paramref name="value"/> provided to the setter is null.
		/// </exception>
		public MarkovChainMap<string> Map
		{
			get { return _map; }
			set
			{
				if (value != _map)
				{
					_mapDepth = value.Depth;
					_map = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the depth of the <see cref="MarkovChainMap{T}"/> used to produce outputs.
		/// </summary>
		/// <exception cref="ArgumentException">A <paramref name="value"/> less than one is provided to the setter.
		/// </exception>
		/// <remarks><see cref="TrainMapBuilder(string)"/> method should be called after this property has been changed,
		/// so that the map can be rebuilt with the new depth parameter. Otherwise, if current map is not null, produced
		/// outputs will not be generated against current configured depth.</remarks>
		public int MapDepth
		{
			get { return _mapDepth; }
			set
			{
				if (value != _mapDepth)
				{
					if (value < 1)
					{
						throw new ArgumentException(nameof(value));
					}
					_mapDepth = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the maximum length of produced outputs.
		/// </summary>
		/// <remarks><paramref name="value"/> provided to the setter will eventually be lower-bounded to
		/// <see cref="MinLength"/>.</remarks>
		public int MaxLength
		{
			get { return _maxLength; }
			set
			{
				value = Math.Max(_minLength, value);
				if (value != _maxLength)
				{
					_maxLength = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the minimum frequency considered while generating possible outputs table. Please see
		/// <see cref="Sieve{T}.MinFrequency"/>.
		/// </summary>
		/// <remarks><paramref name="value"/> provided to the setter will eventually be lower-bounded to zero.</remarks>
		public int MinFrequency
		{
			get { return _minFrequency; }
			set
			{
				value = Math.Max(0, value);
				if (value != _minFrequency)
				{
					_minFrequency = value;
					_sieve.MinFrequency = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the minimum length of produced outputs.
		/// </summary>
		/// <remarks><paramref name="value"/> provided to the setter will eventually be lower-bounded to one.</remarks>
		public int MinLength
		{
			get { return _minLength; }
			set
			{
				value = Math.Max(1, value);
				if (value != _minLength)
				{
					_minLength = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="System.Random"/> used to produce outputs.
		/// </summary>
		/// <remarks>When a null <paramref name="value"/> is provided to the setter, a new <see cref="System.Random"/>
		/// will be automatically initialized.</remarks>
		public Random Random
		{
			get { return _random; }
			set
			{
				value = value ?? new Random(Environment.TickCount);
				if (value != _random)
				{
					_random = value;
					_sieve.Random = value;
				}
			}
		}

		/// <summary>
		/// Sets the <see cref="System.Random"/> used to produce outputs to a new one with specified seed. Please see
		/// <see cref="Random.Random(int)"/> constructor.
		/// </summary>
		public int Seed
		{
			set { Random = new Random(value); }
		}

		/// <summary>
		/// Gets or sets whether white spaces should be considered while building maps. Please see
		/// <see cref="MarkovChainStringMapBuilder.SkipWhitespace"/>.
		/// </summary>
		public bool SkipWhitespace
		{
			get { return _skipWhitespace; }
			set
			{
				if (value != _skipWhitespace)
				{
					_skipWhitespace = value;
					_mapBuilder.SkipWhitespace = value;
				}
			}
		}
		#endregion

		#region Private Properties
		private double RangeLength
		{
			get { return _maxLength - _minLength + 1d; }
		}
		#endregion
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new <see cref="MarkovChainsNameGenerator"/> instance with specified map depth,
		/// minimum frequency, minimum and maximum lengths, with specified capitalization and whitespace-handling modes,
		/// with specified <see cref="Random"/> used to generate outputs, and eventually a list of forbidden and/or 
		/// already-occured outputs.
		/// </summary>
		/// <param name="random"><see cref="Random"/> which will be used for generating outputs. If null, a new
		/// <see cref="Random"/> will be created automatically.</param>
		/// <param name="mapDepth">The depth of the <see cref="MarkovChainMap{T}"/> of strings which will be used to
		/// produce outputs.</param>
		/// <param name="minFrequency">The minimum frequency considered while producing random outputs. Please see
		/// <see cref="Sieve{T}.MinFrequency"/>. Will eventually be lower-bounded to zero.</param>
		/// <param name="minLength">The minimum length of produced outputs. Will eventually be lower-bounded to one.
		/// </param>
		/// <param name="maxLength">The maximum length of produced outputs. Will eventually be lower-bounded to
		/// <paramref name="minLength"/>.</param>
		/// <param name="capitalize">Whether or not outputs will be capitalized.</param>
		/// <param name="skipWhitespace">Whether or not whitespaces will be ignored while parsing the sample file into a
		/// <see cref="MarkovChainMap{T}"/>. Please see <see cref="MarkovChainStringMapBuilder.SkipWhitespace"/>.</param>
		/// <param name="blacklistedNames">A list of forbiden and/or already produced outputs. This list will be qualified
		/// further while new outputs are generated.</param>
		/// <exception cref="ArgumentException"><paramref name="mapDepth"/> is less than one.</exception>
		public MarkovChainsNameGenerator(Random random = null, int mapDepth = DefaultDepth,
			int minFrequency = DefaultMinFrequency, int minLength = DefaultMinLength, int maxLength = DefaultMaxLength,
			bool capitalize = true, bool skipWhitespace = true, IEnumerable<string> blacklistedNames = null)
		{
			_random = random ?? new Random(Environment.TickCount);
			_mapDepth = mapDepth;
			_minFrequency = Math.Max(0, minFrequency);
			_minLength = Math.Max(1, minLength);
			_maxLength = Math.Max(_minLength, maxLength);
			_capitalize = capitalize;
			_skipWhitespace = skipWhitespace;
			_mapBuilder = new MarkovChainStringMapBuilder(mapDepth, skipWhitespace);
			_sieve = new Sieve<string>(_minFrequency, _random);
			_sb = new StringBuilder(_maxLength);
			if (blacklistedNames == null)
            {
				_blacklistedNames = new HashSet<string>();
			}
			else
            {
				_blacklistedNames = new HashSet<string>(blacklistedNames);
			}
			
		}

		/// <summary>
		/// Initializes a new <see cref="MarkovChainsNameGenerator"/> instance with specified
		/// <see cref="MarkovChainMap{T}"/>, minimum frequency, minimum and maximum lengths, with specified capitalization
		/// and whitespace-handling modes, with specified <see cref="Random"/> used to generate outputs, and eventually a
		/// list of forbidden and/or already-produced outputs.
		/// </summary>
		/// <param name="map"><see cref="MarkovChainMap{T}"/> of strings used to produce outputs.</param>
		/// <param name="random"><see cref="Random"/> which will be used for generating outputs. If null, a new
		/// <see cref="Random"/> will be created automatically.</param>
		/// <param name="minFrequency">The minimum frequency considered while producing random outputs. Please see
		/// <see cref="Sieve{T}.MinFrequency"/>.</param>
		/// <param name="minLength">The minimum length of produced outputs. Will be lower-bounded to one, eventually.
		/// </param>
		/// <param name="maxLength">The maximum length of produced outputs. Will be lower-bounded to
		/// <paramref name="minLength"/>, eventually.</param>
		/// <param name="capitalize">Whether or not outputs will be capitalized.</param>
		/// <param name="skipWhitespace">Whether or not whitespaces will be ignored while parsing the sample file into a
		/// <see cref="MarkovChainMap{T}"/>. Please see <see cref="MarkovChainStringMapBuilder.SkipWhitespace"/>.</param>
		/// <param name="blacklistedNames">A list of forbiden and/or already produced outputs. This list will be qualified
		/// further while new outputs are generated.</param>
		/// <exception cref="ArgumentNullException"><paramref name="map"/> is null.</exception>
		public MarkovChainsNameGenerator(MarkovChainMap<string> map, Random random = null,
			int minFrequency = DefaultMinFrequency, int minLength = DefaultMinLength, int maxLength = DefaultMaxLength,
			bool capitalize = true, bool skipWhitespace = true, IEnumerable<string> blacklistedNames = null)
			: this(random, map.Depth, minFrequency, minLength, maxLength, capitalize, skipWhitespace, blacklistedNames)
		{
			_map = map;
		}
		#endregion

		#region Methods
		#region Public Methods
		/// <summary>
		/// Outputs a random name using internally created <see cref="MarkovChainMap{T}"/>.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">Internal <see cref="MarkovChainMap{T}"/> is null. Please call
		/// <see cref="TrainMapBuilder(string, string)"/> method before using <see cref="GetName"/> method.</exception>
		public string GetName()
		{
			StringBuilder sb = _sb;
			sb.Clear();
			List<string> startLetters = _map.GetStartStates();
			List<string> nextCharacters = _map.GetNextStates();
			int startLetterCount = startLetters.Count;
			int nextCharacterCount = nextCharacters.Count;
			int totalLength, currentLength, selectionLength;
			string initial, key, result;
			do
			{
				totalLength = (int)(_minLength + RangeLength * _random.Next() / Maximum);
				initial = startLetters[(int)(startLetterCount * (double)_random.Next() / Maximum)];
				sb.Append(initial);
				currentLength = initial.Length;
				while (currentLength < totalLength)
				{
					selectionLength = _random.Next(1, Math.Min(currentLength, _mapDepth) + 1);
					do
					{
						key = sb.ToString().Substring(currentLength - selectionLength, selectionLength);
					} while (!_map.ContainsKey(key) && (--selectionLength > 0));
					if (selectionLength == 0)
					{
						key = nextCharacters[(int)(nextCharacterCount * (double)_random.Next() / Maximum)];
					}
					sb.Append(_sieve.GetValue(_map[key],
						(currentLength == 1) ? Position.First :
						(currentLength == totalLength - 1) ? Position.Last
																	  : Position.Inner));
					currentLength++;
				}
				result = sb.ToString();
				if (_capitalize)
				{
					result = MakeProperCase(result);
				}
			} while (_blacklistedNames.Contains(result));
			_blacklistedNames.Add(result);
			return result;
		}

		/// <summary>
		/// Outputs specified number of random names using internally created <see cref="MarkovChainMap{T}"/>.
		/// </summary>
		/// <param name="count">Number of names to output.</param>
		/// <returns>A list of <paramref name="count"/> random names.</returns>
		/// <exception cref="ArgumentNullException">Internal <see cref="MarkovChainMap{T}"/> is null. Please call
		/// <see cref="TrainMapBuilder(string, string)"/> method before using <see cref="GetNames(int)"/> method.
		/// </exception>
		public IEnumerable<string> GetNames(int count)
		{
			if (count < 1)
			{
				yield break;
			}
			for (int i = 0; i < count; i++)
			{
				yield return GetName();
			}
		}

		/// <summary>
		/// Builds a new <see cref="MarkovChainMap{T}"/> from specified samples file. Please see
		/// <see cref="MarkovChainStringMapBuilder.TrainFromFile(string, string)"/>.
		/// </summary>
		/// <param name="path">Path to the file containing the samples.</param>
		/// <param name="commentSpecifier">An eventual comment specifier. Lines in the file beginning with this value
		/// will not be parsed.</param>
		public void TrainMapBuilder(string path, string commentSpecifier = "%")
		{
			_map = _mapBuilder.TrainFromFile(path, commentSpecifier);
		}
		#endregion

		#region Private Methods
		private static string MakeProperCase(string input)
		{
			string initial = input.Substring(0, 1).ToUpper(Thread.CurrentThread.CurrentCulture);
			return $"{initial}{input.Substring(1, input.Length - 1)}";
		}
		#endregion
		#endregion
	}
}
