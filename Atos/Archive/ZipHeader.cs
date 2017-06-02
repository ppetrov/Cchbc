using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Atos.Client.Archive
{
	internal sealed class ZipHeader
	{
		public List<Tuple<string, int>> Files { get; } = new List<Tuple<string, int>>();

		public void Add(string name, int length)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Files.Add(Tuple.Create(name, length));
		}

		public async Task LoadAsync(Stream input, CancellationToken cancellationToken)
		{
			if (input == null) throw new ArgumentNullException(nameof(input));

			var headerSizeBuffer = BitConverter.GetBytes(default(int));
			await input.ReadAsync(headerSizeBuffer, 0, headerSizeBuffer.Length, cancellationToken);

			var headerSize = BitConverter.ToInt32(headerSizeBuffer, 0);
			var headerBuffer = new byte[headerSize];
			await input.ReadAsync(headerBuffer, 0, headerBuffer.Length, cancellationToken);

			// Clear the files
			this.Files.Clear();

			using (var sr = new StringReader(Encoding.Unicode.GetString(headerBuffer, 0, headerBuffer.Length)))
			{
				var separator = new[] { '|' };

				string line;
				while ((line = await sr.ReadLineAsync()) != null)
				{
					var values = line.Split(separator);
					this.Add(values[0], int.Parse(values[1]));
				}
			}
		}

		public async Task SaveAsync(Stream output, CancellationToken cancellationToken)
		{
			if (output == null) throw new ArgumentNullException(nameof(output));

			var buffer = new StringBuilder(this.Files.Count * 16);

			foreach (var file in this.Files)
			{
				if (buffer.Length > 0)
				{
					buffer.AppendLine();
				}
				buffer.Append(file.Item1);
				buffer.Append('|');
				buffer.Append(file.Item2);
			}

			var bytes = Encoding.Unicode.GetBytes(buffer.ToString());
			var headerSize = BitConverter.GetBytes(bytes.Length);
			await output.WriteAsync(headerSize, 0, headerSize.Length, cancellationToken);
			await output.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
		}
	}
}