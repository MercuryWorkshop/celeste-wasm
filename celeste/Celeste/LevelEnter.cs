using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class LevelEnter : Scene
	{
		private class BSideTitle : Entity
		{
			private string title;

			private string musicBy;

			private string artist;

			private MTexture artistImage;

			private string album;

			private float musicByWidth;

			private float[] fade = new float[3];

			private float[] offsets = new float[3];

			private float offset;

			public BSideTitle(Session session)
			{
				base.Tag = Tags.HUD;
				switch (session.Area.ID)
				{
				case 1:
					artist = Credits.Remixers[0];
					break;
				case 2:
					artist = Credits.Remixers[1];
					break;
				case 3:
					artist = Credits.Remixers[2];
					break;
				case 4:
					artist = Credits.Remixers[3];
					break;
				case 5:
					artist = Credits.Remixers[4];
					break;
				case 6:
					artist = Credits.Remixers[5];
					break;
				case 7:
					artist = Credits.Remixers[6];
					break;
				case 9:
					artist = Credits.Remixers[7];
					break;
				}
				if (artist.StartsWith("image:"))
				{
					artistImage = GFX.Gui[artist.Substring(6)];
				}
				title = Dialog.Get(AreaData.Get(session).Name) + " " + Dialog.Get(AreaData.Get(session).Name + "_remix");
				musicBy = Dialog.Get("remix_by") + " ";
				musicByWidth = ActiveFont.Measure(musicBy).X;
				album = Dialog.Get("remix_album");
			}

			public IEnumerator EaseIn()
			{
				Add(new Coroutine(FadeTo(0, 1f, 1f)));
				yield return 0.2f;
				Add(new Coroutine(FadeTo(1, 1f, 1f)));
				yield return 0.2f;
				Add(new Coroutine(FadeTo(2, 1f, 1f)));
				yield return 1.8f;
			}

			public IEnumerator EaseOut()
			{
				Add(new Coroutine(FadeTo(0, 0f, 1f)));
				yield return 0.2f;
				Add(new Coroutine(FadeTo(1, 0f, 1f)));
				yield return 0.2f;
				Add(new Coroutine(FadeTo(2, 0f, 1f)));
				yield return 1f;
			}

			private IEnumerator FadeTo(int index, float target, float duration)
			{
				while ((fade[index] = Calc.Approach(fade[index], target, Engine.DeltaTime / duration)) != target)
				{
					if (target == 0f)
					{
						offsets[index] = Ease.CubeIn(1f - fade[index]) * 32f;
					}
					else
					{
						offsets[index] = (0f - Ease.CubeIn(1f - fade[index])) * 32f;
					}
					yield return null;
				}
			}

			public override void Update()
			{
				base.Update();
				offset += Engine.DeltaTime * 32f;
			}

			public override void Render()
			{
				Vector2 position = new Vector2(60f + offset, 800f);
				ActiveFont.Draw(title, position + new Vector2(offsets[0], 0f), Color.White * fade[0]);
				ActiveFont.Draw(musicBy, position + new Vector2(offsets[1], 60f), Color.White * fade[1]);
				if (artistImage != null)
				{
					artistImage.Draw(position + new Vector2(musicByWidth + offsets[1], 68f), Vector2.Zero, Color.White * fade[1]);
				}
				else
				{
					ActiveFont.Draw(artist, position + new Vector2(musicByWidth + offsets[1], 60f), Color.White * fade[1]);
				}
				ActiveFont.Draw(album, position + new Vector2(offsets[2], 120f), Color.White * fade[2]);
			}
		}

		private Session session;

		private Postcard postcard;

		private bool fromSaveData;

		public static void Go(Session session, bool fromSaveData)
		{
			HiresSnow snow = null;
			if (Engine.Scene is Overworld)
			{
				snow = (Engine.Scene as Overworld).Snow;
			}
			bool entry = !fromSaveData && session.StartedFromBeginning;
			if (entry && session.Area.ID == 0)
			{
				Engine.Scene = new IntroVignette(session, snow);
			}
			else if (entry && session.Area.ID == 7 && session.Area.Mode == AreaMode.Normal)
			{
				Engine.Scene = new SummitVignette(session);
			}
			else if (entry && session.Area.ID == 9 && session.Area.Mode == AreaMode.Normal)
			{
				Engine.Scene = new CoreVignette(session, snow);
			}
			else
			{
				Engine.Scene = new LevelEnter(session, fromSaveData);
			}
		}

		private LevelEnter(Session session, bool fromSaveData)
		{
			this.session = session;
			this.fromSaveData = fromSaveData;
			Add(new Entity
			{
				new Coroutine(Routine())
			});
			Add(new HudRenderer());
		}

		private IEnumerator Routine()
		{
			int area = -1;
			if (session.StartedFromBeginning && !fromSaveData && session.Area.Mode == AreaMode.Normal && (!SaveData.Instance.Areas[session.Area.ID].Modes[0].Completed || SaveData.Instance.DebugMode) && session.Area.ID >= 1 && session.Area.ID <= 6)
			{
				area = session.Area.ID;
			}
			if (area >= 0)
			{
				yield return 1f;
				Add(postcard = new Postcard(Dialog.Get("postcard_area_" + area), area));
				yield return postcard.DisplayRoutine();
			}
			if (session.StartedFromBeginning && !fromSaveData && session.Area.Mode == AreaMode.BSide)
			{
				BSideTitle title = new BSideTitle(session);
				Add(title);
				Audio.Play("event:/ui/main/bside_intro_text");
				yield return title.EaseIn();
				yield return 0.25f;
				yield return title.EaseOut();
				yield return 0.25f;
			}
			Input.SetLightbarColor(AreaData.Get(session.Area).TitleBaseColor);
			Engine.Scene = new LevelLoader(session);
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
