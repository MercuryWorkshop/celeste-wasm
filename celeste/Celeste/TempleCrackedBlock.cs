using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class TempleCrackedBlock : Solid
	{
		private EntityID eid;

		private bool persistent;

		private MTexture[,,] tiles;

		private float frame;

		private bool broken;

		private int frames;

		public TempleCrackedBlock(EntityID eid, Vector2 position, float width, float height, bool persistent)
			: base(position, width, height, safe: true)
		{
			this.eid = eid;
			this.persistent = persistent;
			Collidable = (Visible = false);
			int columns = (int)(width / 8f);
			int rows = (int)(height / 8f);
			List<MTexture> tex = GFX.Game.GetAtlasSubtextures("objects/temple/breakBlock");
			tiles = new MTexture[columns, rows, tex.Count];
			frames = tex.Count;
			for (int tx = 0; tx < columns; tx++)
			{
				for (int ty = 0; ty < rows; ty++)
				{
					int subx = ((tx < columns / 2 && tx < 2) ? tx : ((tx < columns / 2 || tx < columns - 2) ? (2 + tx % 2) : (5 - (columns - tx - 1))));
					int suby = ((ty < rows / 2 && ty < 2) ? ty : ((ty < rows / 2 || ty < rows - 2) ? (2 + ty % 2) : (5 - (rows - ty - 1))));
					for (int i = 0; i < tex.Count; i++)
					{
						tiles[tx, ty, i] = tex[i].GetSubtexture(subx * 8, suby * 8, 8, 8);
					}
				}
			}
			Add(new LightOcclude(0.5f));
		}

		public TempleCrackedBlock(EntityID eid, EntityData data, Vector2 offset)
			: this(eid, data.Position + offset, data.Width, data.Height, data.Bool("persistent"))
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			if (CollideCheck<Player>())
			{
				if (persistent)
				{
					SceneAs<Level>().Session.DoNotLoad.Add(eid);
				}
				RemoveSelf();
			}
			else
			{
				Collidable = (Visible = true);
			}
		}

		public override void Update()
		{
			base.Update();
			if (broken)
			{
				frame += Engine.DeltaTime * 15f;
				if (frame >= (float)frames)
				{
					RemoveSelf();
				}
			}
		}

		public override void Render()
		{
			int f = (int)frame;
			if (f >= frames)
			{
				return;
			}
			for (int tx = 0; (float)tx < base.Width / 8f; tx++)
			{
				for (int ty = 0; (float)ty < base.Height / 8f; ty++)
				{
					tiles[tx, ty, f].Draw(Position + new Vector2(tx, ty) * 8f);
				}
			}
		}

		public void Break(Vector2 from)
		{
			if (persistent)
			{
				SceneAs<Level>().Session.DoNotLoad.Add(eid);
			}
			Audio.Play("event:/game/05_mirror_temple/crackedwall_vanish", base.Center);
			broken = true;
			Collidable = false;
			for (int x = 0; (float)x < base.Width / 8f; x++)
			{
				for (int y = 0; (float)y < base.Height / 8f; y++)
				{
					base.Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(x * 8 + 4, y * 8 + 4), '1').BlastFrom(from));
				}
			}
		}
	}
}
