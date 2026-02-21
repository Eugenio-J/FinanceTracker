namespace FinanceTracker.Domain.Helpers
{
	public static class PhilippineDateTime
	{
		private static readonly TimeZoneInfo PhTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
		public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PhTimeZone);
	}
}
