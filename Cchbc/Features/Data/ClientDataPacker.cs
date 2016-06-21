using System;
using System.Collections.Generic;

namespace Cchbc.Features.Data
{
	public static class ClientDataPacker
	{
		private const int ShortSize = 2;
		private const int LongSize = 8;

		public static byte[] Pack(ClientData clientData)
		{
			if (clientData == null) throw new ArgumentNullException(nameof(clientData));

			var offset = 0;
			var buffer = new byte[GetBufferSize(clientData)];

			Write(buffer, ref offset, (short)clientData.ContextRows.Count);
			foreach (var row in clientData.ContextRows)
			{
				Write(buffer, ref offset, row.Id);
				Write(buffer, ref offset, row.Name);
			}

			Write(buffer, ref offset, (short)clientData.StepRows.Count);
			foreach (var row in clientData.StepRows)
			{
				Write(buffer, ref offset, row.Id);
				Write(buffer, ref offset, row.Name);
			}

			Write(buffer, ref offset, (short)clientData.ExceptionRows.Count);
			foreach (var row in clientData.ExceptionRows)
			{
				Write(buffer, ref offset, row.Id);
				Write(buffer, ref offset, row.Contents);
			}

			Write(buffer, ref offset, (short)clientData.FeatureRows.Count);
			foreach (var row in clientData.FeatureRows)
			{
				Write(buffer, ref offset, row.Id);
				Write(buffer, ref offset, row.Name);
				Write(buffer, ref offset, row.ContextId);
			}

			Write(buffer, ref offset, (short)clientData.FeatureEntryRows.Count);
			foreach (var row in clientData.FeatureEntryRows)
			{
				Write(buffer, ref offset, row.Id);
				Write(buffer, ref offset, BitConverter.DoubleToInt64Bits(row.TimeSpent));
				Write(buffer, ref offset, row.Details);
				Write(buffer, ref offset, row.CreatedAt.ToBinary());
				Write(buffer, ref offset, row.FeatureId);
			}

			Write(buffer, ref offset, (short)clientData.EntryStepRows.Count);
			foreach (var row in clientData.EntryStepRows)
			{
				Write(buffer, ref offset, BitConverter.DoubleToInt64Bits(row.TimeSpent));
				Write(buffer, ref offset, row.FeatureEntryId);
				Write(buffer, ref offset, row.FeatureStepId);
			}

			Write(buffer, ref offset, (short)clientData.ExceptionEntryRows.Count);
			foreach (var row in clientData.ExceptionEntryRows)
			{
				Write(buffer, ref offset, row.ExceptionRowId);
				Write(buffer, ref offset, row.CreatedAt.ToBinary());
				Write(buffer, ref offset, row.FeatureId);
			}

			return buffer;
		}

		public static ClientData Unpack(byte[] buffer)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));

			var offset = 0;
			var symbolBuffer = new char[short.MaxValue];

			var count = ReadShort(buffer, ref offset);
			var dbFeatureContextRows = new List<DbFeatureContextRow>(count);
			for (var i = 0; i < count; i++)
			{
				var id = ReadLong(buffer, ref offset);
				var name = ReadString(buffer, ref offset, symbolBuffer);
				dbFeatureContextRows.Add(new DbFeatureContextRow(id, name));
			}

			count = ReadShort(buffer, ref offset);
			var dbFeatureStepRows = new List<DbFeatureStepRow>(count);
			for (var i = 0; i < count; i++)
			{
				var id = ReadLong(buffer, ref offset);
				var name = ReadString(buffer, ref offset, symbolBuffer);
				dbFeatureStepRows.Add(new DbFeatureStepRow(id, name));
			}

			count = ReadShort(buffer, ref offset);
			var dbFeatureExceptionRows = new List<DbFeatureExceptionRow>(count);
			for (var i = 0; i < count; i++)
			{
				var id = ReadLong(buffer, ref offset);
				var contents = ReadString(buffer, ref offset, symbolBuffer);
				dbFeatureExceptionRows.Add(new DbFeatureExceptionRow(id, contents));
			}

			count = ReadShort(buffer, ref offset);
			var dbFeatureRows = new List<DbFeatureRow>(count);
			for (var i = 0; i < count; i++)
			{
				var id = ReadLong(buffer, ref offset);
				var name = ReadString(buffer, ref offset, symbolBuffer);
				var contextId = ReadLong(buffer, ref offset);
				dbFeatureRows.Add(new DbFeatureRow(id, name, contextId));
			}

			count = ReadShort(buffer, ref offset);
			var dbFeatureEntryRows = new List<DbFeatureEntryRow>(count);
			for (var i = 0; i < count; i++)
			{
				var id = ReadLong(buffer, ref offset);
				var timeSpent = BitConverter.Int64BitsToDouble(ReadLong(buffer, ref offset));
				var details = ReadString(buffer, ref offset, symbolBuffer);
				var createdAt = DateTime.FromBinary(ReadLong(buffer, ref offset));
				var featureId = ReadLong(buffer, ref offset);
				dbFeatureEntryRows.Add(new DbFeatureEntryRow(id, timeSpent, details, createdAt, featureId));
			}

			count = ReadShort(buffer, ref offset);
			var dbFeatureEntryStepRows = new List<DbFeatureEntryStepRow>(count);
			for (var i = 0; i < count; i++)
			{
				var timeSpent = BitConverter.Int64BitsToDouble(ReadLong(buffer, ref offset));
				var featureEntryId = ReadLong(buffer, ref offset);
				var featureStepId = ReadLong(buffer, ref offset);
				dbFeatureEntryStepRows.Add(new DbFeatureEntryStepRow(timeSpent, featureEntryId, featureStepId));
			}

			count = ReadShort(buffer, ref offset);
			var dbFeatureExceptionEntryRows = new List<DbFeatureExceptionEntryRow>(count);
			for (var i = 0; i < count; i++)
			{
				var exceptionRowId = ReadLong(buffer, ref offset);
				var createdAt = DateTime.FromBinary(ReadLong(buffer, ref offset));
				var featureId = ReadLong(buffer, ref offset);
				dbFeatureExceptionEntryRows.Add(new DbFeatureExceptionEntryRow(exceptionRowId, createdAt, featureId));
			}

			return new ClientData(dbFeatureContextRows, dbFeatureStepRows, dbFeatureExceptionRows, dbFeatureRows, dbFeatureEntryRows, dbFeatureEntryStepRows, dbFeatureExceptionEntryRows);
		}

		private static string ReadString(byte[] buffer, ref int offset, char[] symbolBuffer)
		{
			var length = ReadShort(buffer, ref offset);

			for (var i = 0; i < length; i++)
			{
				symbolBuffer[i] = (char)buffer[offset++];
			}

			return new string(symbolBuffer, 0, length);
		}

		private static short ReadShort(byte[] buffer, ref int offset)
		{
			offset += ShortSize;
			return BitConverter.ToInt16(buffer, offset - ShortSize);
		}

		private static long ReadLong(byte[] buffer, ref int offset)
		{
			offset += LongSize;
			return BitConverter.ToInt64(buffer, offset - LongSize);
		}

		private static int GetBufferSize(ClientData clientData)
		{
			const int longSize = 8;

			var lists = 7;
			var bufferSize = lists * ShortSize;

			foreach (var row in clientData.ContextRows)
			{
				bufferSize += longSize;
				bufferSize += GetBufferSize(row.Name);
			}

			foreach (var row in clientData.StepRows)
			{
				bufferSize += longSize;
				bufferSize += GetBufferSize(row.Name);
			}

			foreach (var row in clientData.ExceptionRows)
			{
				bufferSize += longSize;
				bufferSize += GetBufferSize(row.Contents);
			}

			foreach (var row in clientData.FeatureRows)
			{
				bufferSize += (2 * longSize);
				bufferSize += GetBufferSize(row.Name);
			}

			foreach (var row in clientData.FeatureEntryRows)
			{
				bufferSize += (4 * longSize);
				bufferSize += GetBufferSize(row.Details);
			}

            bufferSize += (3 * longSize) * clientData.EntryStepRows.Count;

            bufferSize += (clientData.ExceptionEntryRows.Count * (3 * longSize));

			return bufferSize;
		}

		private static int GetBufferSize(string value)
		{
			return ShortSize + value.Length;
		}

		private static void Write(byte[] buffer, ref int offset, short value)
		{
			buffer[offset++] = (byte)(value);
			buffer[offset++] = (byte)(value >> 8);
		}

		private static void Write(byte[] buffer, ref int offset, long value)
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

		private static void Write(byte[] buffer, ref int offset, string value)
		{
			Write(buffer, ref offset, (short)value.Length);

			foreach (var symbol in value)
			{
				buffer[offset++] = (byte)(symbol);
			}
		}
	}
}