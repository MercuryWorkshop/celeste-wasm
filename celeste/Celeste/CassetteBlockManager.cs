using FMOD.Studio;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class CassetteBlockManager : Entity
	{
		private int currentIndex;

		private float beatTimer;

		private int beatIndex;

		private float tempoMult;

		private int leadBeats;

		private int maxBeat;

		private bool isLevelMusic;

		private int beatIndexOffset;

		private EventInstance sfx;

		private EventInstance snapshot;

		public CassetteBlockManager()
		{
			base.Tag = Tags.Global;
			Add(new TransitionListener
			{
				OnOutBegin = delegate
				{
					if (!SceneAs<Level>().HasCassetteBlocks)
					{
						RemoveSelf();
					}
					else
					{
						maxBeat = SceneAs<Level>().CassetteBlockBeats;
						tempoMult = SceneAs<Level>().CassetteBlockTempo;
					}
				}
			});
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			isLevelMusic = AreaData.Areas[SceneAs<Level>().Session.Area.ID].CassetteSong == null;
			if (isLevelMusic)
			{
				leadBeats = 0;
				beatIndexOffset = 5;
			}
			else
			{
				beatIndexOffset = 0;
				leadBeats = 16;
				snapshot = Audio.CreateSnapshot("snapshot:/music_mains_mute");
			}
			maxBeat = SceneAs<Level>().CassetteBlockBeats;
			tempoMult = SceneAs<Level>().CassetteBlockTempo;
		}

		public override void Removed(Scene scene)
		{
			base.Removed(scene);
			if (!isLevelMusic)
			{
				Audio.Stop(snapshot);
				Audio.Stop(sfx);
			}
		}

		public override void SceneEnd(Scene scene)
		{
			base.SceneEnd(scene);
			if (!isLevelMusic)
			{
				Audio.Stop(snapshot);
				Audio.Stop(sfx);
			}
		}

		public override void Update()
		{
			base.Update();
			if (isLevelMusic)
			{
				sfx = Audio.CurrentMusicEventInstance;
			}
			if (sfx == null && !isLevelMusic)
			{
				string sfxs = AreaData.Areas[SceneAs<Level>().Session.Area.ID].CassetteSong;
				sfx = Audio.CreateInstance(sfxs);
				Audio.Play("event:/game/general/cassette_block_switch_2");
			}
			else
			{
				AdvanceMusic(Engine.DeltaTime * tempoMult);
			}
		}

		public void AdvanceMusic(float time)
		{
			_ = beatTimer;
			beatTimer += time;
			if (!(beatTimer >= 1f / 6f))
			{
				return;
			}
			beatTimer -= 1f / 6f;
			beatIndex++;
			beatIndex %= 256;
			if (beatIndex % 8 == 0)
			{
				currentIndex++;
				currentIndex %= maxBeat;
				SetActiveIndex(currentIndex);
				if (!isLevelMusic)
				{
					Audio.Play("event:/game/general/cassette_block_switch_2");
				}
				Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
			}
			else if ((beatIndex + 1) % 8 == 0)
			{
				SetWillActivate((currentIndex + 1) % maxBeat);
			}
			else if ((beatIndex + 4) % 8 == 0 && !isLevelMusic)
			{
				Audio.Play("event:/game/general/cassette_block_switch_1");
			}
			if (leadBeats > 0)
			{
				leadBeats--;
				if (leadBeats == 0)
				{
					beatIndex = 0;
					if (!isLevelMusic)
					{
						sfx.start();
					}
				}
			}
			if (leadBeats <= 0)
			{
				sfx.setParameterByName("sixteenth_note", GetSixteenthNote());
			}
		}

		public int GetSixteenthNote()
		{
			return (beatIndex + beatIndexOffset) % 256 + 1;
		}

		public void StopBlocks()
		{
			foreach (CassetteBlock entity in base.Scene.Tracker.GetEntities<CassetteBlock>())
			{
				entity.Finish();
			}
			if (!isLevelMusic)
			{
				Audio.Stop(sfx);
			}
		}

		public void Finish()
		{
			if (!isLevelMusic)
			{
				Audio.Stop(snapshot);
			}
			RemoveSelf();
		}

		public void OnLevelStart()
		{
			maxBeat = SceneAs<Level>().CassetteBlockBeats;
			tempoMult = SceneAs<Level>().CassetteBlockTempo;
			if (beatIndex % 8 >= 5)
			{
				currentIndex = maxBeat - 2;
			}
			else
			{
				currentIndex = maxBeat - 1;
			}
			SilentUpdateBlocks();
		}

		private void SilentUpdateBlocks()
		{
			foreach (CassetteBlock block in base.Scene.Tracker.GetEntities<CassetteBlock>())
			{
				if (block.ID.Level == SceneAs<Level>().Session.Level)
				{
					block.SetActivatedSilently(block.Index == currentIndex);
				}
			}
		}

		public void SetActiveIndex(int index)
		{
			foreach (CassetteBlock entity in base.Scene.Tracker.GetEntities<CassetteBlock>())
			{
				entity.Activated = entity.Index == index;
			}
		}

		public void SetWillActivate(int index)
		{
			foreach (CassetteBlock block in base.Scene.Tracker.GetEntities<CassetteBlock>())
			{
				if (block.Index == index || block.Activated)
				{
					block.WillToggle();
				}
			}
		}
	}
}
