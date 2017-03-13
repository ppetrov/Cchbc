using System;
using System.Collections.Generic;

namespace Cchbc.Localization
{
	public sealed class LocalizationManager
	{
		private Dictionary<string, Dictionary<string, string>> ByContextValues { get; } = new Dictionary<string, Dictionary<string, string>>();

		public void Load(IEnumerable<string> lines)
		{
			if (lines == null) throw new ArgumentNullException(nameof(lines));

			this.ByContextValues.Clear();

			//Context.Name:Message
			foreach (var line in lines)
			{
				var index = line.IndexOf('.');
				if (index >= 0)
				{
					index++;
					var separatorIndex = line.IndexOf(':', index) - index;
					var context = line.Substring(0, index - 1);
					var key = line.Substring(index, separatorIndex);
					var message = line.Substring(context.Length + key.Length + 2);

					this.Add(context, key, message);
				}
			}
		}

		public string GetBy(string context, string key)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (key == null) throw new ArgumentNullException(nameof(key));

			var message = default(string);

			Dictionary<string, string> messages;
			if (this.ByContextValues.TryGetValue(context, out messages))
			{
				messages.TryGetValue(key, out message);
			}

			return message ?? @"N/A";
		}

		private void Add(string context, string key, string message)
		{
			Dictionary<string, string> values;
			if (!this.ByContextValues.TryGetValue(context, out values))
			{
				values = new Dictionary<string, string>();
				this.ByContextValues.Add(context, values);
			}
			values.Add(key, message);
		}
	}
}