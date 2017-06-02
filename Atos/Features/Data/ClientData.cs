using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Atos.Client.Features.Data
{
	public sealed class ClientData
	{
		private readonly BinaryWriter _binaryWriter;
		private readonly MemoryStream _memoryStream = new MemoryStream(8 * 1024);

		public ClientData()
		{
			_binaryWriter = new BinaryWriter(_memoryStream, Encoding.ASCII);
		}

		public byte[] GetBytes()
		{
			return _memoryStream.ToArray();
		}

		public void Add(Dictionary<string, FeatureContextRow> contexts)
		{
			if (contexts == null) throw new ArgumentNullException(nameof(contexts));

			_binaryWriter.Write(contexts.Count);

			foreach (var contextRow in contexts.Values)
			{
				_binaryWriter.Write(contextRow.Id);
				_binaryWriter.Write(contextRow.Name);
			}
		}

		public void Add(Dictionary<long, List<FeatureRow>> features)
		{
			if (features == null) throw new ArgumentNullException(nameof(features));

			var initialPosition = (int)_memoryStream.Position;
			_binaryWriter.Write(-1);

			var count = 0;
			foreach (var byContext in features.Values)
			{
				foreach (var feature in byContext)
				{
					_binaryWriter.Write(feature.Id);
					_binaryWriter.Write(feature.Name);
					_binaryWriter.Write(feature.ContextId);
				}
				count += byContext.Count;
			}

			this.WriteRecordsCount(initialPosition, count);
		}

		public void Add(IEnumerable<FeatureEntryRow> featureEntries)
		{
			if (featureEntries == null) throw new ArgumentNullException(nameof(featureEntries));

			var initialPosition = (int)_memoryStream.Position;
			_binaryWriter.Write(-1);

			var count = 0;
			foreach (var entry in featureEntries)
			{
				_binaryWriter.Write(entry.FeatureId);
				_binaryWriter.Write(entry.Details);
				_binaryWriter.Write(entry.CreatedAt.ToBinary());
				count++;
			}

			this.WriteRecordsCount(initialPosition, count);
		}

		public void Add(IEnumerable<FeatureExceptionEntryRow> featureExceptionEntries)
		{
			if (featureExceptionEntries == null) throw new ArgumentNullException(nameof(featureExceptionEntries));

			var initialPosition = (int)_memoryStream.Position;
			_binaryWriter.Write(-1);

			var count = 0;
			foreach (var entry in featureExceptionEntries)
			{
				_binaryWriter.Write(entry.ExceptionId);
				_binaryWriter.Write(entry.CreatedAt.ToBinary());
				_binaryWriter.Write(entry.FeatureId);
				count++;
			}

			this.WriteRecordsCount(initialPosition, count);
		}

		public void Add(IEnumerable<FeatureExceptionRow> featureExceptions)
		{
			if (featureExceptions == null) throw new ArgumentNullException(nameof(featureExceptions));

			var initialPosition = (int)_memoryStream.Position;
			_binaryWriter.Write(-1);

			var count = 0;
			foreach (var e in featureExceptions)
			{
				_binaryWriter.Write(e.Id);
				_binaryWriter.Write(e.Contents);
				count++;
			}

			this.WriteRecordsCount(initialPosition, count);
		}

		private void WriteRecordsCount(int initialPosition, int count)
		{
			var currentPosition = (int)_memoryStream.Position;
			_binaryWriter.Seek(initialPosition, SeekOrigin.Begin);
			_binaryWriter.Write(count);
			_binaryWriter.Seek(currentPosition, SeekOrigin.Begin);
		}
	}
}