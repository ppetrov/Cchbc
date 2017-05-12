using System;

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

			Write(buffer, ref offset, (short)clientData.Contexts.Length);
			foreach (var row in clientData.Contexts)
			{
				Write(buffer, ref offset, row.Id);
				Write(buffer, ref offset, row.Name);
			}

			Write(buffer, ref offset, (short)clientData.Exceptions.Length);
			foreach (var row in clientData.Exceptions)
			{
				Write(buffer, ref offset, row.Id);
				Write(buffer, ref offset, row.Contents);
			}

			Write(buffer, ref offset, (short)clientData.Features.Length);
			foreach (var row in clientData.Features)
			{
				Write(buffer, ref offset, row.Id);
				Write(buffer, ref offset, row.Name);
				Write(buffer, ref offset, row.ContextId);
			}

			Write(buffer, ref offset, (short)clientData.FeatureEntries.Length);
			foreach (var row in clientData.FeatureEntries)
			{
				Write(buffer, ref offset, row.Details);
				Write(buffer, ref offset, row.CreatedAt.ToBinary());
				Write(buffer, ref offset, row.FeatureId);
			}

			Write(buffer, ref offset, (short)clientData.ExceptionEntries.Length);
			foreach (var row in clientData.ExceptionEntries)
			{
				Write(buffer, ref offset, row.ExceptionId);
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
			var dbFeatureContextRows = new FeatureContextRow[count];
			for (var i = 0; i < count; i++)
			{
				var id = ReadLong(buffer, ref offset);
				var name = ReadString(buffer, ref offset, symbolBuffer);
				dbFeatureContextRows[i] = new FeatureContextRow(id, name);
			}

			count = ReadShort(buffer, ref offset);
			var dbFeatureExceptionRows = new FeatureExceptionRow[count];
			for (var i = 0; i < count; i++)
			{
				var id = ReadLong(buffer, ref offset);
				var contents = ReadString(buffer, ref offset, symbolBuffer);
				dbFeatureExceptionRows[i] = new FeatureExceptionRow(id, contents);
			}

			count = ReadShort(buffer, ref offset);
			var dbFeatureRows = new FeatureRow[count];
			for (var i = 0; i < count; i++)
			{
				var id = ReadLong(buffer, ref offset);
				var name = ReadString(buffer, ref offset, symbolBuffer);
				var contextId = ReadLong(buffer, ref offset);
				dbFeatureRows[i] = new FeatureRow(id, name, contextId);
			}

			count = ReadShort(buffer, ref offset);
			var dbFeatureEntryRows = new FeatureEntryRow[count];
			for (var i = 0; i < count; i++)
			{
				var details = ReadString(buffer, ref offset, symbolBuffer);
				var createdAt = DateTime.FromBinary(ReadLong(buffer, ref offset));
				var featureId = ReadLong(buffer, ref offset);
				dbFeatureEntryRows[i] = new FeatureEntryRow(featureId, createdAt, details);
			}

			count = ReadShort(buffer, ref offset);
			var dbFeatureExceptionEntryRows = new FeatureExceptionEntryRow[count];
			for (var i = 0; i < count; i++)
			{
				var exceptionRowId = ReadLong(buffer, ref offset);
				var createdAt = DateTime.FromBinary(ReadLong(buffer, ref offset));
				var featureId = ReadLong(buffer, ref offset);
				dbFeatureExceptionEntryRows[i] = new FeatureExceptionEntryRow(exceptionRowId, createdAt, featureId);
			}

			return new ClientData(dbFeatureContextRows, dbFeatureRows, dbFeatureEntryRows, dbFeatureExceptionEntryRows, dbFeatureExceptionRows);
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
			var value = BitConverter.ToInt16(buffer, offset);
			offset += ShortSize;
			return value;
		}

		private static long ReadLong(byte[] buffer, ref int offset)
		{
			var value = BitConverter.ToInt64(buffer, offset);
			offset += LongSize;
			return value;
		}

		private static int GetBufferSize(ClientData clientData)
		{
			// For the number of elements in every list
			var totalLists = 5;
			var size = totalLists * ShortSize;

			// Id 
			size += LongSize * clientData.Contexts.Length;
			foreach (var row in clientData.Contexts)
			{
				// Name
				size += GetBufferSize(row.Name);
			}

			// Id
			size += LongSize * clientData.Exceptions.Length;
			foreach (var row in clientData.Exceptions)
			{
				// Contents
				size += GetBufferSize(row.Contents);
			}

			// Id & Context Id
			size += 2 * LongSize * clientData.Features.Length;
			foreach (var row in clientData.Features)
			{
				// Name
				size += GetBufferSize(row.Name);
			}

			// Id, CreatedAt
			var totalFeatureEntries = clientData.FeatureEntries.Length;
			size += LongSize * totalFeatureEntries;
			// FeatureId
			size += LongSize * totalFeatureEntries;
			// Details
			foreach (var row in clientData.FeatureEntries)
			{
				size += GetBufferSize(row.Details);
			}

			// Exception Id & Feature Id
			var totalExceptionentries = clientData.ExceptionEntries.Length;
			size += 2 * LongSize * totalExceptionentries;
			// Created At
			size += LongSize * totalExceptionentries;

			return size;
		}

		private static int GetBufferSize(string value)
		{
			// the length(encoded in short) and a char for every symbol
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