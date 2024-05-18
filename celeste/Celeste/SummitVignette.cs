using System.IO;
using System.Xml;
using Monocle;

namespace Celeste
{
	public class SummitVignette : Scene
	{
		private CompleteRenderer complete;

		private bool slideFinished;

		private Session session;

		private bool ending;

		private bool ready;

		private bool addedRenderer;

		public SummitVignette(Session session)
		{
			this.session = session;
			session.Audio.Apply();
            LoadCompleteThread();
			//RunThread.Start(LoadCompleteThread, "SUMMIT_VIGNETTE");
		}

		private void LoadCompleteThread()
		{
			Atlas atlas = null;
			XmlElement xml = GFX.CompleteScreensXml["Screens"]["SummitIntro"];
			if (xml != null)
			{
				atlas = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", xml.Attr("atlas")), Atlas.AtlasDataFormat.PackerNoAtlas);
			}
			complete = new CompleteRenderer(xml, atlas, 0f, delegate
			{
				slideFinished = true;
			});
			complete.SlideDuration = 7.5f;
			ready = true;
		}

		public override void Update()
		{
			if (ready && !addedRenderer)
			{
				Add(complete);
				addedRenderer = true;
			}
			base.Update();
			if ((Input.MenuConfirm.Pressed || slideFinished) && !ending && ready)
			{
				ending = true;
				new MountainWipe(this, wipeIn: false, delegate
				{
					Engine.Scene = new LevelLoader(session);
				});
			}
		}

		public override void End()
		{
			base.End();
			if (complete != null)
			{
				complete.Dispose();
			}
		}
	}
}
