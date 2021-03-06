﻿using System.Collections.Generic;

namespace Atos.iFSA.Objects
{
	public sealed class ActivityTypeCategory
	{
		public long Id { get; }
		public string Name { get; }
		public List<ActivityType> Types { get; } = new List<ActivityType>();
		public ActivityType AutoSelectedActivityType { get; set; }

		public ActivityTypeCategory(long id, string name)
		{
			this.Id = id;
			this.Name = name;
		}
	}
}