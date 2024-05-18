using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class MirrorReflection : Component
	{
		public bool IgnoreEntityVisible;

		public bool IsRendering;

		public MirrorReflection()
			: base(active: false, visible: true)
		{
		}
	}
}
