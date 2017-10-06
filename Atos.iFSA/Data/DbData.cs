using Atos.Client.Data;

namespace Atos.iFSA.Data
{
	public static class DbData
	{
		public static int GetInt(IFieldDataReader r, int index, int defaultValue = 0)
		{
			return r.IsDbNull(index) ? defaultValue : r.GetInt32(index);
		}

		public static long GetLong(IFieldDataReader r, int index, long defaultValue = 0)
		{
			return r.IsDbNull(index) ? defaultValue : r.GetInt64(index);
		}

		public static string GetString(IFieldDataReader r, int index, string defaultValue = "")
		{
			return r.IsDbNull(index) ? defaultValue : r.GetString(index);
		}
	}
}