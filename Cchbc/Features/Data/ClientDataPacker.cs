using System;
using System.Collections.Generic;

namespace Cchbc.Features.Data
{
	public static class ClientDataPacker
	{
		private const int ShortSize = 2;
		private const int IntSize = 4;
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
				Write(buffer, ref offset, row.Details);
				Write(buffer, ref offset, row.CreatedAt.ToBinary());
				Write(buffer, ref offset, row.FeatureId);
			}

			Write(buffer, ref offset, (short)clientData.ExceptionEntryRows.Count);
			foreach (var row in clientData.ExceptionEntryRows)
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
			var dbFeatureContextRows = new List<DbFeatureContextRow>(count);
			for (var i = 0; i < count; i++)
			{
				var id = ReadInt(buffer, ref offset);
				var name = ReadString(buffer, ref offset, symbolBuffer);
				dbFeatureContextRows.Add(new DbFeatureContextRow(id, name));
			}

			count = ReadShort(buffer, ref offset);
			var dbFeatureExceptionRows = new List<DbFeatureExceptionRow>(count);
			for (var i = 0; i < count; i++)
			{
				var id = ReadInt(buffer, ref offset);
				var contents = ReadString(buffer, ref offset, symbolBuffer);
				dbFeatureExceptionRows.Add(new DbFeatureExceptionRow(id, contents));
			}

			count = ReadShort(buffer, ref offset);
			var dbFeatureRows = new List<DbFeatureRow>(count);
			for (var i = 0; i < count; i++)
			{
				var id = ReadInt(buffer, ref offset);
				var name = ReadString(buffer, ref offset, symbolBuffer);
				var contextId = ReadInt(buffer, ref offset);
				dbFeatureRows.Add(new DbFeatureRow(id, name, contextId));
			}

			count = ReadShort(buffer, ref offset);
			var dbFeatureEntryRows = new List<DbFeatureEntryRow>(count);
			for (var i = 0; i < count; i++)
			{
				var details = ReadString(buffer, ref offset, symbolBuffer);
				var createdAt = DateTime.FromBinary(ReadLong(buffer, ref offset));
				var featureId = ReadInt(buffer, ref offset);
				dbFeatureEntryRows.Add(new DbFeatureEntryRow(details, createdAt, featureId));
			}

			count = ReadShort(buffer, ref offset);
			var dbFeatureExceptionEntryRows = new List<DbFeatureExceptionEntryRow>(count);
			for (var i = 0; i < count; i++)
			{
				var exceptionRowId = ReadInt(buffer, ref offset);
				var createdAt = DateTime.FromBinary(ReadLong(buffer, ref offset));
				var featureId = ReadInt(buffer, ref offset);
				dbFeatureExceptionEntryRows.Add(new DbFeatureExceptionEntryRow(exceptionRowId, createdAt, featureId));
			}

			return new ClientData(dbFeatureContextRows, dbFeatureExceptionRows, dbFeatureRows, dbFeatureEntryRows, dbFeatureExceptionEntryRows);
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

		private static int ReadInt(byte[] buffer, ref int offset)
		{
			var value = BitConverter.ToInt32(buffer, offset);
			offset += IntSize;
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
			size += IntSize * clientData.ContextRows.Count;
			foreach (var row in clientData.ContextRows)
			{
				// Name
				size += GetBufferSize(row.Name);
			}

			// Id
			size += IntSize * clientData.ExceptionRows.Count;
			foreach (var row in clientData.ExceptionRows)
			{
				// Contents
				size += GetBufferSize(row.Contents);
			}

			// Id & Context Id
			size += 2 * IntSize * clientData.FeatureRows.Count;
			foreach (var row in clientData.FeatureRows)
			{
				// Name
				size += GetBufferSize(row.Name);
			}

			// Id, CreatedAt
			size += LongSize * clientData.FeatureEntryRows.Count;
			// FeatureId
			size += IntSize * clientData.FeatureEntryRows.Count;
			// Details
			foreach (var row in clientData.FeatureEntryRows)
			{
				size += GetBufferSize(row.Details);
			}

			// Exception Id & Feature Id
			size += 2 * IntSize * clientData.ExceptionEntryRows.Count;
			// Created At
			size += LongSize * clientData.ExceptionEntryRows.Count;

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

		private static void Write(byte[] buffer, ref int offset, int value)
		{
			buffer[offset++] = (byte)(value);
			buffer[offset++] = (byte)(value >> 8);
			buffer[offset++] = (byte)(value >> 16);
			buffer[offset++] = (byte)(value >> 24);
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