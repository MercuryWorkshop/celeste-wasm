using System.Collections;
using Monocle;

namespace Celeste
{
	public abstract class Oui : Entity
	{
		public bool Focused;

		public Overworld Overworld => SceneAs<Overworld>();

		public bool Selected
		{
			get
			{
				if (Overworld != null)
				{
					return Overworld.Current == this;
				}
				return false;
			}
		}

		public Oui()
		{
			AddTag(Tags.HUD);
		}

		public virtual bool IsStart(Overworld overworld, Overworld.StartMode start)
		{
			return false;
		}

		public abstract IEnumerator Enter(Oui from);

		public abstract IEnumerator Leave(Oui next);
	}
}
