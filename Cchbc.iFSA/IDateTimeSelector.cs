﻿using System;
using System.Threading.Tasks;
using Cchbc.Validation;

namespace iFSA
{
	public interface IDateTimeSelector
	{
		Task ShowAsync(Func<DateTime, PermissionResult> validator, Action<DateTime> operation);
	}
}