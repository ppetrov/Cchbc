using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Cchbc.Web
{
	public static class WebHelper
	{
		public static async Task<byte[]> DownloadDataAsync(string url)
		{
			if (url == null) throw new ArgumentNullException(nameof(url));

			var request = WebRequest.Create(url);

			using (var response = await request.GetResponseAsync())
			{
				using (var dataStream = response.GetResponseStream())
				{
					using (var ms = new MemoryStream())
					{
						var buffer = new byte[1024 * 8];

						int readedBytes;
						while ((readedBytes = dataStream.Read(buffer, 0, buffer.Length)) != 0)
						{
							ms.Write(buffer, 0, readedBytes);
						}

						return ms.ToArray();
					}
				}
			}
		}
	}
}