using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class Hahaha : Entity
	{
		private class Ha
		{
			public Sprite Sprite;

			public float Percent;

			public float Duration;

			public Ha()
			{
				Sprite = new Sprite(GFX.Game, "characters/oldlady/");
				Sprite.Add("normal", "ha", 0.15f, 0, 1, 0, 1, 0, 1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9);
				Sprite.Play("normal");
				Sprite.JustifyOrigin(0.5f, 0.5f);
				Duration = (float)Sprite.CurrentAnimationTotalFrames * 0.15f;
			}
		}

		private bool enabled;

		private string ifSet;

		private float timer;

		private int counter;

		private List<Ha> has = new List<Ha>();

		private bool autoTriggerLaughSfx;

		private Vector2 autoTriggerLaughOrigin;

		public bool Enabled
		{
			get
			{
				return enabled;
			}
			set
			{
				if (!enabled && value)
				{
					timer = 0f;
					counter = 0;
				}
				enabled = value;
			}
		}

		public Hahaha(Vector2 position, string ifSet = "", bool triggerLaughSfx = false, Vector2? triggerLaughSfxOrigin = null)
		{
			base.Depth = -10001;
			Position = position;
			this.ifSet = ifSet;
			if (triggerLaughSfx)
			{
				autoTriggerLaughSfx = triggerLaughSfx;
				autoTriggerLaughOrigin = triggerLaughSfxOrigin.Value;
			}
		}

		public Hahaha(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Attr("ifset"), data.Bool("triggerLaughSfx"), (data.Nodes.Length != 0) ? (offset + data.Nodes[0]) : Vector2.Zero)
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			if (!string.IsNullOrEmpty(ifSet) && !(base.Scene as Level).Session.GetFlag(ifSet))
			{
				Enabled = false;
			}
		}

		public override void Update()
		{
			if (Enabled)
			{
				timer -= Engine.DeltaTime;
				if (timer <= 0f)
				{
					has.Add(new Ha());
					counter++;
					if (counter >= 3)
					{
						counter = 0;
						timer = 1.5f;
					}
					else
					{
						timer = 0.6f;
					}
				}
				if (autoTriggerLaughSfx && base.Scene.OnInterval(0.4f))
				{
					Audio.Play("event:/char/granny/laugh_oneha", autoTriggerLaughOrigin);
				}
			}
			for (int i = has.Count - 1; i >= 0; i--)
			{
				if (has[i].Percent > 1f)
				{
					has.RemoveAt(i);
				}
				else
				{
					has[i].Sprite.Update();
					has[i].Percent += Engine.DeltaTime / has[i].Duration;
				}
			}
			if (!Enabled && !string.IsNullOrEmpty(ifSet) && (base.Scene as Level).Session.GetFlag(ifSet))
			{
				Enabled = true;
			}
			base.Update();
		}

		public override void Render()
		{
			foreach (Ha ha in has)
			{
				ha.Sprite.Position = Position + new Vector2(ha.Percent * 60f, -10f + (float)(0.0 - Math.Sin(ha.Percent * 13f)) * 4f + ha.Percent * -16f);
				ha.Sprite.Render();
			}
		}
	}
}
