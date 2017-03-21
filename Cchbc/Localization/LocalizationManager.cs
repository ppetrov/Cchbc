using System;
using System.Collections.Generic;

namespace Cchbc.Localization
{
	public sealed class LocalizationManager : ILocalizationManager
	{
		private Dictionary<string, Dictionary<string, string>> ByContextValues { get; } = new Dictionary<string, Dictionary<string, string>>();

		public void Load(IEnumerable<string> lines)
		{
			if (lines == null) throw new ArgumentNullException(nameof(lines));

			this.ByContextValues.Clear();

			//Context.Name:Message
			var previousContext = default(string);

			foreach (var line in lines)
			{
				var index = line.IndexOf('.');
				if (index >= 0)
				{
					index++;
					var separatorIndex = line.IndexOf(':', index) - index;

					var context = previousContext;
					if (context == null || IsContextChanged(line, index, previousContext))
					{
						context = line.Substring(0, index - 1);
					}
					previousContext = context;

					var key = line.Substring(index, separatorIndex);
					var message = line.Substring(context.Length + key.Length + 2);

					Dictionary<string, string> values;
					if (!this.ByContextValues.TryGetValue(context, out values))
					{
						values = new Dictionary<string, string>();
						this.ByContextValues.Add(context, values);
					}

					// Guard against duplicated entries
					string current;
					if (!values.TryGetValue(key, out current))
					{
						values.Add(key, message);
					}
				}
			}
		}

		public string Get(LocalizationKey key)
		{
			if (key == null) throw new ArgumentNullException(nameof(key));

			var defaultValue = key.Name;
			var message = default(string);

			Dictionary<string, string> messages;
			if (this.ByContextValues.TryGetValue(key.Context, out messages))
			{
				messages.TryGetValue(key.Name, out message);
			}

			return message ?? defaultValue;
		}

		private static bool IsContextChanged(string line, int index, string previousContext)
		{
			if (index == previousContext.Length)
			{
				// Compare every symbol
				for (var i = 0; i < index; i++)
				{
					var x = line[i];
					var y = previousContext[i];
					if (!char.ToUpperInvariant(x).Equals(char.ToUpperInvariant(y)))
					{
						// symbols don't match - context is changed
						return true;
					}
				}

				// No early return - same context
				return false;
			}

			// Different lengths - context is changed
			return true;
		}
	}
}