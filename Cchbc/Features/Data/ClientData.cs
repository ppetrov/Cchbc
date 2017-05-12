using System;

namespace Cchbc.Features.Data
{
	public sealed class ClientData
	{
		private const int ShortSize = 2;
		private const int LongSize = 8;

		public readonly FeatureContextRow[] Contexts;
		public readonly FeatureRow[] Features;
		public readonly FeatureEntryRow[] FeatureEntries;
		public readonly FeatureExceptionEntryRow[] ExceptionEntries;
		public readonly FeatureExceptionRow[] Exceptions;

		public ClientData(FeatureContextRow[] contexts, FeatureRow[] features, FeatureEntryRow[] featureEntries, FeatureExceptionEntryRow[] exceptionEntries, FeatureExceptionRow[] exceptions)
		{
			if (contexts == null) throw new ArgumentNullException(nameof(contexts));
			if (features == null) throw new ArgumentNullException(nameof(features));
			if (featureEntries == null) throw new ArgumentNullException(nameof(featureEntries));
			if (exceptionEntries == null) throw new ArgumentNullException(nameof(exceptionEntries));
			if (exceptions == null) throw new ArgumentNullException(nameof(exceptions));

			this.Contexts = contexts;
			this.Features = features;
			this.FeatureEntries = featureEntries;
			this.ExceptionEntries = exceptionEntries;
			this.Exceptions = exceptions;
		}

		public byte[] Pack()
		{
			var offset = 0;
			var buffer = new byte[GetBufferSize()];

			Write(buffer, ref offset, this.Contexts.Length);
			foreach (var row in this.Contexts)
			{
				Write(buffer, ref offset, row.Id);
				Write(buffer, ref offset, row.Name);
			}

			Write(buffer, ref offset, this.Features.Length);
			foreach (var row in this.Features)
			{
				Write(buffer, ref offset, row.Id);
				Write(buffer, ref offset, row.Name);
				Write(buffer, ref offset, row.ContextId);
			}

			Write(buffer, ref offset, this.FeatureEntries.Length);
			foreach (var row in this.FeatureEntries)
			{
				Write(buffer, ref offset, row.FeatureId);
				Write(buffer, ref offset, row.Details);
				Write(buffer, ref offset, row.CreatedAt.ToBinary());
			}

			Write(buffer, ref offset, this.ExceptionEntries.Length);
			foreach (var row in this.ExceptionEntries)
			{
				Write(buffer, ref offset, row.ExceptionId);
				Write(buffer, ref offset, row.CreatedAt.ToBinary());
				Write(buffer, ref offset, row.FeatureId);
			}

			Write(buffer, ref offset, this.Exceptions.Length);
			foreach (var row in this.Exceptions)
			{
				Write(buffer, ref offset, row.Id);
				Write(buffer, ref offset, row.Contents);
			}

			return buffer;
		}

		public static ClientData Unpack(byte[] bytes)
		{
			if (bytes == null) throw new ArgumentNullException(nameof(bytes));

			var offset = 0;
			var buffer = new char[short.MaxValue];

			var featureContextRows = new FeatureContextRow[ReadLong(bytes, ref offset)];
			for (var i = 0; i < featureContextRows.Length; i++)
			{
				var id = ReadLong(bytes, ref offset);
				var name = ReadString(bytes, ref offset, buffer);
				featureContextRows[i] = new FeatureContextRow(id, name);
			}

			var featureExceptionRows = new FeatureExceptionRow[ReadLong(bytes, ref offset)];
			for (var i = 0; i < featureExceptionRows.Length; i++)
			{
				var id = ReadLong(bytes, ref offset);
				var contents = ReadString(bytes, ref offset, buffer);
				featureExceptionRows[i] = new FeatureExceptionRow(id, contents);
			}

			var featureRows = new FeatureRow[ReadLong(bytes, ref offset)];
			for (var i = 0; i < featureRows.Length; i++)
			{
				var id = ReadLong(bytes, ref offset);
				var name = ReadString(bytes, ref offset, buffer);
				var contextId = ReadLong(bytes, ref offset);
				featureRows[i] = new FeatureRow(id, name, contextId);
			}

			var featureEntryRows = new FeatureEntryRow[ReadLong(bytes, ref offset)];
			for (var i = 0; i < featureEntryRows.Length; i++)
			{
				var featureId = ReadLong(bytes, ref offset);
				var details = ReadString(bytes, ref offset, buffer);
				var createdAt = DateTime.FromBinary(ReadLong(bytes, ref offset));
				featureEntryRows[i] = new FeatureEntryRow(featureId, details, createdAt);
			}

			var featureExceptionEntryRows = new FeatureExceptionEntryRow[ReadLong(bytes, ref offset)];
			for (var i = 0; i < featureExceptionEntryRows.Length; i++)
			{
				var exceptionRowId = ReadLong(bytes, ref offset);
				var createdAt = DateTime.FromBinary(ReadLong(bytes, ref offset));
				var featureId = ReadLong(bytes, ref offset);
				featureExceptionEntryRows[i] = new FeatureExceptionEntryRow(exceptionRowId, createdAt, featureId);
			}

			return new ClientData(featureContextRows, featureRows, featureEntryRows, featureExceptionEntryRows, featureExceptionRows);
		}

		private static string ReadString(byte[] buffer, ref int offset, char[] symbolBuffer)
		{
			var length = BitConverter.ToInt16(buffer, offset);
			offset += ShortSize;

			for (var i = 0; i < length; i++)
			{
				symbolBuffer[i] = (char)buffer[offset++];
			}

			return new string(symbolBuffer, 0, length);
		}

		private static long ReadLong(byte[] buffer, ref int offset)
		{
			var value = BitConverter.ToInt64(buffer, offset);
			offset += LongSize;
			return value;
		}

		private int GetBufferSize()
		{
			// For the number of elements in every list
			var size = 5 * LongSize;

			//public readonly long Id;
			//public readonly string Name;
			size += LongSize * this.Contexts.Length;
			foreach (var row in this.Contexts)
			{
				size += GetBufferSize(row.Name);
			}

			//public readonly long Id;
			//public readonly string Name;
			//public readonly long ContextId;
			size += 2 * LongSize * this.Features.Length;
			foreach (var row in this.Features)
			{
				size += GetBufferSize(row.Name);
			}

			//public readonly long FeatureId;
			//public readonly string Details;
			//public readonly DateTime CreatedAt;
			size += 2 * LongSize * this.FeatureEntries.Length;
			foreach (var row in this.FeatureEntries)
			{
				size += GetBufferSize(row.Details);
			}

			//public readonly long ExceptionId;
			//public readonly DateTime CreatedAt;
			//public readonly long FeatureId;
			size += 3 * LongSize * this.ExceptionEntries.Length;

			//public readonly long Id;
			//public readonly string Contents;
			size += LongSize * this.Exceptions.Length;
			foreach (var row in this.Exceptions)
			{
				size += GetBufferSize(row.Contents);
			}

			return size;
		}

		private int GetBufferSize(string value)
		{
			// the length(encoded in short) and a char for every symbol
			return ShortSize + value.Length;
		}

		private void Write(byte[] buffer, ref int offset, long value)
		{
			buffer[offset++] = (byte)(value);
			buffer[offset++] = (byte)(value >> 8);
			buffer[offset++] = (byte)(value >> 16);
			buffer[offset++] = (byte)(value >> 24);
			buffer[offset++] = (byte)(value >> 32);
			buffer[offset++] = (byte)(value >> 40);
			buffer[offset++] = (byte)(value >> 48);
			buffer[offset++] = (byte)(value >> 56);
		}

		private void Write(byte[] buffer, ref int offset, string value)
		{
			var length = (short)value.Length;
			buffer[offset++] = (byte)(length);
			buffer[offset++] = (byte)(length >> 8);

			foreach (var symbol in value)
			{
				buffer[offset++] = (byte)(symbol);
			}
		}
	}
}