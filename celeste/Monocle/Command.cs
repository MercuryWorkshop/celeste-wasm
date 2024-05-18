using System;

namespace Monocle
{
	public class Command : Attribute
	{
		public string Name;

		public string Help;

		public Command(string name, string help)
		{
			Name = name;
			Help = help;
		}
	}
}
