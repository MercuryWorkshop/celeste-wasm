using System.Collections;
using Monocle;

namespace Celeste
{
	public class PreviewPostcard : Scene
	{
		private Postcard postcard;

		public PreviewPostcard(Postcard postcard)
		{
			Audio.SetMusic(null);
			Audio.SetAmbience(null);
			this.postcard = postcard;
			Add(new Entity
			{
				new Coroutine(Routine(postcard))
			});
			Add(new HudRenderer());
		}

		private IEnumerator Routine(Postcard postcard)
		{
			yield return 0.25f;
			Add(postcard);
			yield return postcard.DisplayRoutine();
			Engine.Scene = new OverworldLoader(Overworld.StartMode.MainMenu);
		}

		public override void BeforeRender()
		{
			base.BeforeRender();
			if (postcard != null)
			{
				postcard.BeforeRender();
			}
		}
	}
}
