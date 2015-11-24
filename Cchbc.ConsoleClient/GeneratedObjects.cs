using System;
using System.Collections.Generic;
using Cchbc.Objects;

namespace Cchbc.ConsoleClient
{























	public sealed class Outlet
	{
		public long Id { get; }
		public string Name { get; }

		public Outlet(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class Visit : IDbObject
	{
		public long Id { get; set; }
		public Outlet Outlet { get; set; }
		public DateTime Date { get; set; }
		public List<Activity> Activities { get; set; }

		public Visit(long id, Outlet outlet, DateTime date, List<Activity> activities)
		{
			if (outlet == null) throw new ArgumentNullException(nameof(outlet));
			if (activities == null) throw new ArgumentNullException(nameof(activities));

			this.Id = id;
			this.Outlet = outlet;
			this.Date = date;
			this.Activities = activities;
		}
	}

	public sealed class ActivityType
	{
		public long Id { get; }
		public string Name { get; }

		public ActivityType(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class Activity : IDbObject
	{
		public long Id { get; set; }
		public DateTime Date { get; set; }
		public ActivityType ActivityType { get; set; }
		public Visit Visit { get; set; }

		public Activity(long id, DateTime date, ActivityType activityType, Visit visit)
		{
			if (activityType == null) throw new ArgumentNullException(nameof(activityType));
			if (visit == null) throw new ArgumentNullException(nameof(visit));

			this.Id = id;
			this.Date = date;
			this.ActivityType = activityType;
			this.Visit = visit;
		}
	}

	public sealed class Brand
	{
		public long Id { get; }
		public string Name { get; }

		public Brand(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class Flavor
	{
		public long Id { get; }
		public string Name { get; }

		public Flavor(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class Article
	{
		public long Id { get; }
		public string Name { get; }
		public Brand Brand { get; }
		public Flavor Flavor { get; }

		public Article(long id, string name, Brand brand, Flavor flavor)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (brand == null) throw new ArgumentNullException(nameof(brand));
			if (flavor == null) throw new ArgumentNullException(nameof(flavor));

			this.Id = id;
			this.Name = name;
			this.Brand = brand;
			this.Flavor = flavor;
		}
	}

	public sealed class ActivityNoteType
	{
		public long Id { get; }
		public string Name { get; }

		public ActivityNoteType(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class ActivityNote : IDbObject
	{
		public long Id { get; set; }
		public string Contents { get; set; }
		public ActivityNoteType ActivityNoteType { get; set; }
		public Activity Activity { get; set; }

		public ActivityNote(long id, string contents, ActivityNoteType activityNoteType, Activity activity)
		{
			if (contents == null) throw new ArgumentNullException(nameof(contents));
			if (activityNoteType == null) throw new ArgumentNullException(nameof(activityNoteType));
			if (activity == null) throw new ArgumentNullException(nameof(activity));

			this.Id = id;
			this.Contents = contents;
			this.ActivityNoteType = activityNoteType;
			this.Activity = activity;
		}
	}












}