namespace Celeste
{
	public static class Achievements
	{
		public static string ID(Achievement achievement)
		{
			return achievement.ToString();
		}

		public static bool Has(Achievement achievement)
		{
			return false;
		}

		public static void Register(Achievement achievement)
		{
			Has(achievement);
		}
	}
}
