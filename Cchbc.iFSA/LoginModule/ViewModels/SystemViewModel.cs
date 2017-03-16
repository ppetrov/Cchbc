﻿using System;
using iFSA.LoginModule.Objects;

namespace iFSA.LoginModule.ViewModels
{
	public sealed class SystemViewModel
	{
		public AppSystem AppSystem { get; }
		public string Name => this.AppSystem.Name;
		public SystemSource Source => this.AppSystem.Source;

		public SystemViewModel(AppSystem appSystem)
		{
			if (appSystem == null) throw new ArgumentNullException(nameof(appSystem));

			this.AppSystem = appSystem;
		}
	}
}