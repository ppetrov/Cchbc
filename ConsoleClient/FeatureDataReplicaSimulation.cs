﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cchbc.ConsoleClient;
using Cchbc.Features.Data;
using Cchbc.Features.Replication;

namespace ConsoleClient
{
	public static class FeatureDataReplicaSimulation
	{
		public static void Replicate()
		{
			try
			{
				var contextCreator = new TransactionContextCreator(@"Data Source = C:\Users\PetarPetrov\Desktop\server.sqlite; Version = 3;");

				var clientData = new ClientData();
				clientData.Add(new Dictionary<string, FeatureContextRow>
				{
					{@"Agenda", new FeatureContextRow(1, @"Agenda") }
				});
				clientData.Add(new[] { new FeatureExceptionRow(1, @"new exception : stack trace => ..."), });
				clientData.Add(new Dictionary<long, List<FeatureRow>>()
				{
					{1, new List<FeatureRow>()
					{
						new FeatureRow(1, @"Add Activity", 1),
						new FeatureRow(2, @"Close Activity", 1),
						new FeatureRow(3, @"Cancel Activity", 1),
					} },
				});
				clientData.Add(new[]
				{
					new FeatureEntryRow(2, "N/A", DateTime.Now, 17.293),
					new FeatureEntryRow(3, "Sick", DateTime.Now, 23.811)
				});
				clientData.Add(new[]
				{
					new FeatureExceptionEntryRow(1, DateTime.Today.AddDays(-1), 1),

				});

				//ResetSchema(contextCreator);				

				var s = Stopwatch.StartNew();
				using (var ctx = contextCreator.Create())
				{
					for (var i = 0; i < 1; i++)
					{
						FeatureServerManager.Replicate(ctx, @"BG900343", @"1.0.0.0", clientData.GetBytes());
					}
					ctx.Complete();
				}
				s.Stop();
				Console.WriteLine(s.ElapsedMilliseconds);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

		private static void ResetSchema(TransactionContextCreator contextCreator)
		{
			try
			{
				using (var ctx = contextCreator.Create())
				{
					FeatureServerManager.DropSchema(ctx);
					ctx.Complete();
				}
			}
			catch (Exception _)
			{
			}
			try
			{
				using (var ctx = contextCreator.Create())
				{
					FeatureServerManager.CreateSchema(ctx);
					ctx.Complete();
				}
			}
			catch (Exception _)
			{
			}
		}
	}
}