using System;

namespace Atos.iFSA.LoginModule2
{
	public sealed class DayStatus
	{
		public long Id { get; }
		public string Name { get; }
		public bool IsOpen => this.Id == 0;
		public bool IsWorking => this.Id == 1;
		public bool IsCancel => this.Id == 2;
		public bool IsClose => this.Id == 3;
		public bool IsActive => this.IsOpen || this.IsWorking;

		public DayStatus(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}
}