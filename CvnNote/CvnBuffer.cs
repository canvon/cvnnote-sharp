﻿/*
 *  cvnnote-sharp - process canvon notes files with C#/.NET library, CLI or GUI
 *  Copyright (C) 2016  Fabian Pietsch <fabian@canvon.de>
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CvnNote
{
	public class CvnBuffer
	{
		public class Range
		{
			private CvnBuffer _buf;

			private int _start;
			public int Start {
				get {
					return _start;
				}
			}

			private int _len;
			public int Length {
				get {
					return _len;
				}
			}


			// (Should have been private, but then even the outer class
			// can't instantiate the inner class anymore. So marking internal.)
			internal Range(CvnBuffer buf, int start, int len)
			{
				if (object.ReferenceEquals(buf, null))
					throw new ArgumentNullException("buf");

				if (start < 0 || start >= buf.Length)
					throw new ArgumentOutOfRangeException(
						"start", start, "Start of range has to be inside buffer");

				if (len < 0)
					throw new ArgumentOutOfRangeException(
						"len", len, "Length of range has to be non-negative");

				if (start + len > buf.Length)
					throw new ArgumentOutOfRangeException(
						"len", len, "Length of range must not lead outside buffer");

				_buf = buf;
				_start = start;
				_len = len;
			}


			public char this[int index] {
				get {
					if (index < 0)
						throw new ArgumentOutOfRangeException(
							"index", index, "Index has to be non-negative");

					if (index >= _len)
						throw new ArgumentOutOfRangeException(
							"index", index, "Index must not point outside range");

					return _buf[_start + index];
				}
			}
		}


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

		public Range GetRange(int start, int length)
		{
			return new Range(this, start, length);
		}
	}
}
