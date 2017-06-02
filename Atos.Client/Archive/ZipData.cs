using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace Atos.Client.Archive
{
	internal sealed class ZipData
	{
		public List<byte[]> Files { get; } = new List<byte[]>();

		public void Add(byte[] data)
		{
			if (data == null) throw new ArgumentNullException(nameof(data));

			Files.Add(data);
		}

		public async Task SaveAsync(Stream output, CancellationToken cancellationToken)
		{
			if (output == null) throw new ArgumentNullException(nameof(output));

			var buffer = new byte[64 * 1024];

			foreach (var file in Files)
			{
				using (var input = new MemoryStream(file))
				{
					int readBytes;
					while ((readBytes = await input.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) != 0)
					{
						await output.WriteAsync(buffer, 0, readBytes, cancellationToken);
					}
				}
			}
		}

		public async Task LoadAsync(Stream input, ZipHeader header, CancellationToken cancellationToken)
		{
			if (input == null) throw new ArgumentNullException(nameof(input));
			if (header == null) throw new ArgumentNullException(nameof(header));

			// Clear the files
			Files.Clear();

			var buffer = new byte[64 * 1024];

			foreach (var file in header.Files)
			{
				var fileSize = file.Item2;
				using (var output = new MemoryStream(fileSize))
				{
					int readBytes;
					while ((readBytes = await input.ReadAsync(buffer, 0, Math.Min(fileSize, buffer.Length), cancellationToken)) != 0)
					{
						await output.WriteAsync(buffer, 0, readBytes, cancellationToken);
						fileSize -= readBytes;
					}

					output.Position = 0;

					using (var data = new MemoryStream(fileSize))
					{
						using (var zip = new GZipStream(output, CompressionMode.Decompress, true))
						{
							while ((readBytes = await zip.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) != 0)
							{
								await data.WriteAsync(buffer, 0, readBytes, cancellationToken);
							}
							Files.Add(data.ToArray());
						}
					}
				}
			}
		}
	}
}