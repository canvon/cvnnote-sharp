using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CvnNote
{
	public class CvnBuffer
	{
		private readonly int _ChunkSize;
		public int ChunkSize {
			get {
				return _ChunkSize;
			}
		}

		protected List<char[]> _Chunks;

		private int _Length;
		public int Length {
			get {
				return _Length;
			}
		}


		public CvnBuffer(int chunkSize)
		{
			if (chunkSize < 1)
				throw new ArgumentOutOfRangeException(
					"chunkSize", chunkSize,
					string.Format("Chunk size is too small: {0}", chunkSize));

			_ChunkSize = chunkSize;

			_Chunks = new List<char[]>();
			_Length = 0;
		}

		public CvnBuffer() : this(Environment.SystemPageSize / Buffer.ByteLength(new char[1]))
		{
		}


		public char this[int index] {
			get {
				if (index < 0)
					throw new ArgumentOutOfRangeException(
						"index", index,
						string.Format("CvnBuffer index can't be negative: {0}", index));

				if (index >= _Length)
					throw new ArgumentOutOfRangeException(
						"index", index,
						string.Format("Invalid CvnBuffer index {0}: Length is {1}", index, _Length));

				return _Chunks[index / _ChunkSize][index % _ChunkSize];
			}
		}

		public void Append(char[] data)
		{
			if (object.ReferenceEquals(data, null))
				throw new ArgumentNullException("data");

			int i = 0;

			// Special case: Left-over space in last chunk
			if (_Length % _ChunkSize > 0) {
				char[] chunk = _Chunks[_Length / _ChunkSize];

				for (int j = _Length % _ChunkSize; j < chunk.Length; j++) {
					if (i >= data.Length)
						return;

					chunk[j] = data[i++];
					_Length++;
				}
			}

			// Loop: Chunk-by-chunk
			while (i + _ChunkSize <= data.Length) {
				char[] chunk = new char[_ChunkSize];

				Array.ConstrainedCopy(data, i, chunk, 0, chunk.Length);
				i += chunk.Length;

				_Chunks.Add(chunk);
				_Length += chunk.Length;
			}

			// Special case: More data that will only fill a partial chunk
			if (i < data.Length) {
				char[] chunk = new char[_ChunkSize];
				int endLen = data.Length - i;

				Array.ConstrainedCopy(data, i, chunk, 0, endLen);
				i += endLen;

				_Chunks.Add(chunk);
				_Length += endLen;
			}

			// Sanity-check.
			Debug.Assert(i == data.Length, "CvnBuffer.Append() didn't copy all the data.",
				"Index into source data ({0}) should have matched data.Length ({1}).",
				i, data.Length);
		}
	}
}
