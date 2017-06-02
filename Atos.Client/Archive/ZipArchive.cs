using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace Atos.Client.Archive
{
	public sealed class ZipArchive
	{
		internal ZipHeader Header { get; }
		internal ZipData Data { get; }

		public ZipArchive()
		{
			this.Header = new ZipHeader();
			this.Data = new ZipData();
		}

		public async Task AddFileAsync(string name, Stream input, CancellationToken cancellationToken)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (input == null) throw new ArgumentNullException(nameof(input));

			using (var data = new MemoryStream())
			{
				using (var zip = new GZipStream(data, CompressionMode.Compress, true))
				{
					var buffer = new byte[64 * 1024];

					int readBytes;
					while ((readBytes = await input.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) != 0)
					{
						await zip.WriteAsync(buffer, 0, readBytes, cancellationToken);
					}
				}
				this.Data.Add(data.ToArray());
				this.Header.Add(name, (int)data.Length);
			}
		}

		public async Task SaveAsync(Stream output, CancellationToken cancellationToken)
		{
			if (output == null) throw new ArgumentNullException(nameof(output));

			await this.Header.SaveAsync(output, cancellationToken);
			await this.Data.SaveAsync(output, cancellationToken);
		}

		public async Task LoadAsync(Stream input, CancellationToken cancellationToken)
		{
			if (input == null) throw new ArgumentNullException(nameof(input));

			await this.Header.LoadAsync(input, cancellationToken);
			await this.Data.LoadAsync(input, this.Header, cancellationToken);
		}
	}
}