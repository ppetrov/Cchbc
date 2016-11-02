using System;

namespace ConsoleClient
{
	public enum ControlType
	{
		Button,
		ComboBox,
		CheckBox,
		DateTimePicker,
		List,
	}

	public sealed class AppUsageContext
	{
		public static readonly AppUsageContext Agenda = new AppUsageContext(@"Agenda");
		public static readonly AppUsageContext AddActivity = new AppUsageContext(@"Add Activity");

		public string Name { get; }

		public AppUsageContext(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Name = name;
		}
	}

	public sealed class AppUsageOperation
	{
		public static readonly AppUsageOperation LoadData = new AppUsageOperation(@"Load Data");
		public static readonly AppUsageOperation Create = new AppUsageOperation(@"Create");
		public static readonly AppUsageOperation Start = new AppUsageOperation(@"Start");
		public static readonly AppUsageOperation SelectOutlet = new AppUsageOperation(@"Select Outlet");
		public static readonly AppUsageOperation SelectCategory = new AppUsageOperation(@"Select Category");
		public static readonly AppUsageOperation SelectType = new AppUsageOperation(@"Select Type");
		public static readonly AppUsageOperation SelectFromDate = new AppUsageOperation(@"Select From Date");
		public static readonly AppUsageOperation SelectToDate = new AppUsageOperation(@"Select To Date");
		public static readonly AppUsageOperation ToggleVisit = new AppUsageOperation(@"Toggle Visit");

		public string Name { get; }

		public AppUsageOperation(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Name = name;
		}
	}

	public sealed class AppUsage
	{
		public string Context { get; }
		public string Operation { get; }
		public ControlType ControlType { get; }
		public string Details { get; }

		public AppUsage(string context, string operation, ControlType controlType, string details)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (operation == null) throw new ArgumentNullException(nameof(operation));
			if (details == null) throw new ArgumentNullException(nameof(details));

			this.Context = context;
			this.Operation = operation;
			this.ControlType = controlType;
			this.Details = details;
		}
	}

	public sealed class AppUsageManager
	{
		private AppUsageAdapter Adapter { get; }

		public AppUsageManager(AppUsageAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Adapter = adapter;
		}

		public void DeleteOldData()
		{
			this.Adapter.DeleteOldData();
		}

		public void RecordButton(AppUsageContext context, AppUsageOperation operation, string details = null)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (operation == null) throw new ArgumentNullException(nameof(operation));

			this.Record(context, operation, ControlType.Button, details);
		}

		public void RecordComboBox(AppUsageContext context, AppUsageOperation operation, string details = null)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (operation == null) throw new ArgumentNullException(nameof(operation));

			this.Record(context, operation, ControlType.ComboBox, details);
		}

		public void RecordCheckBox(AppUsageContext context, AppUsageOperation operation, string details = null)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (operation == null) throw new ArgumentNullException(nameof(operation));

			this.Record(context, operation, ControlType.CheckBox, details);
		}

		public void RecordDateTimePicker(AppUsageContext context, AppUsageOperation operation, string details = null)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (operation == null) throw new ArgumentNullException(nameof(operation));

			this.Record(context, operation, ControlType.DateTimePicker, details);
		}

		public void Record(AppUsageContext context, AppUsageOperation operation, ControlType controlType, string details = null)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (operation == null) throw new ArgumentNullException(nameof(operation));

			// TODO : !!!
			long deviceId = -1;
			var username = @"Username";
			var version = @"Version";

			//var appUsage = new AppUsage(context.Name, operation.Name, controlType, details ?? string.Empty, DateTime.Now, username, version, deviceId);
			//this.Adapter.Insert(appUsage);
		}
	}

	public sealed class AppUsageAdapter
	{
		public void DeleteOldData()
		{
			// TODO : !!!
			// Delete recods that are too old
		}

		public void Insert(AppUsage usage)
		{
			if (usage == null) throw new ArgumentNullException(nameof(usage));

			// TODO : !!!
		}
	}
}