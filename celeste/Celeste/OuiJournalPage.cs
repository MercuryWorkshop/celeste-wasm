using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public abstract class OuiJournalPage
	{
		public class Table
		{
			private const float headerHeight = 80f;

			private const float headerBottomMargin = 20f;

			private const float rowHeight = 60f;

			private List<Row> rows = new List<Row>();

			public int Rows => rows.Count;

			public Row Header
			{
				get
				{
					if (rows.Count <= 0)
					{
						return null;
					}
					return rows[0];
				}
			}

			public Table AddColumn(Cell label)
			{
				if (rows.Count == 0)
				{
					AddRow();
				}
				rows[0].Add(label);
				return this;
			}

			public Row AddRow()
			{
				Row row = new Row();
				rows.Add(row);
				return row;
			}

			public float Height()
			{
				return 100f + 60f * (float)(rows.Count - 1);
			}

			public void Render(Vector2 position)
			{
				if (Header == null)
				{
					return;
				}
				float left = 0f;
				float fullWidth = 0f;
				for (int x2 = 0; x2 < Header.Count; x2++)
				{
					fullWidth += Header[x2].Width() + 20f;
				}
				for (int x = 0; x < Header.Count; x++)
				{
					float width = Header[x].Width();
					Header[x].Render(position + new Vector2(left + width * 0.5f, 40f), width);
					int bg = 1;
					float yPos = 130f;
					for (int y = 1; y < rows.Count; y++)
					{
						Vector2 pos = position + new Vector2(left + width * 0.5f, yPos);
						if (rows[y].Count > 0)
						{
							if (bg % 2 == 0)
							{
								Draw.Rect(pos.X - width * 0.5f, pos.Y - 27f, width + 20f, 54f, Color.Black * 0.08f);
							}
							if (x < rows[y].Count && rows[y][x] != null)
							{
								Cell entry = rows[y][x];
								if (entry.SpreadOverColumns > 1)
								{
									for (int i = x + 1; i < x + entry.SpreadOverColumns; i++)
									{
										pos.X += Header[i].Width() * 0.5f;
									}
									pos.X += (float)(entry.SpreadOverColumns - 1) * 20f * 0.5f;
								}
								rows[y][x].Render(pos, width);
							}
							bg++;
							yPos += 60f;
						}
						else
						{
							Draw.Rect(pos.X - width * 0.5f, pos.Y - 25.5f, width + 20f, 6f, Color.Black * 0.3f);
							yPos += 15f;
						}
					}
					left += width + 20f;
				}
			}
		}

		public class Row
		{
			public List<Cell> Entries = new List<Cell>();

			public int Count => Entries.Count;

			public Cell this[int index] => Entries[index];

			public Row Add(Cell entry)
			{
				Entries.Add(entry);
				return this;
			}
		}

		public abstract class Cell
		{
			public int SpreadOverColumns = 1;

			public virtual float Width()
			{
				return 0f;
			}

			public virtual void Render(Vector2 center, float columnWidth)
			{
			}
		}

		public class EmptyCell : Cell
		{
			private float width;

			public EmptyCell(float width)
			{
				this.width = width;
			}

			public override float Width()
			{
				return width;
			}
		}

		public class TextCell : Cell
		{
			private string text;

			private Vector2 justify;

			private float scale;

			private Color color;

			private float width;

			private bool forceWidth;

			public TextCell(string text, Vector2 justify, float scale, Color color, float width = 0f, bool forceWidth = false)
			{
				this.text = text;
				this.justify = justify;
				this.scale = scale;
				this.color = color;
				this.width = width;
				this.forceWidth = forceWidth;
			}

			public override float Width()
			{
				if (forceWidth)
				{
					return width;
				}
				return Math.Max(width, ActiveFont.Measure(text).X * scale);
			}

			public override void Render(Vector2 center, float columnWidth)
			{
				float textWidth = ActiveFont.Measure(text).X * scale;
				float s = 1f;
				if (!forceWidth && textWidth > columnWidth)
				{
					s = columnWidth / textWidth;
				}
				ActiveFont.Draw(text, center + new Vector2((0f - columnWidth) / 2f + columnWidth * justify.X, 0f), justify, Vector2.One * scale * s, color);
			}
		}

		public class IconCell : Cell
		{
			private string icon;

			private float width;

			public IconCell(string icon, float width = 0f)
			{
				this.icon = icon;
				this.width = width;
			}

			public override float Width()
			{
				return Math.Max(MTN.Journal[icon].Width, width);
			}

			public override void Render(Vector2 center, float columnWidth)
			{
				MTN.Journal[icon].DrawCentered(center);
			}
		}

		public class IconsCell : Cell
		{
			private float iconSpacing = 4f;

			private string[] icons;

			public IconsCell(float iconSpacing, params string[] icons)
			{
				this.iconSpacing = iconSpacing;
				this.icons = icons;
			}

			public IconsCell(params string[] icons)
			{
				this.icons = icons;
			}

			public override float Width()
			{
				float width = 0f;
				for (int i = 0; i < icons.Length; i++)
				{
					width += (float)MTN.Journal[icons[i]].Width;
				}
				return width + (float)(icons.Length - 1) * iconSpacing;
			}

			public override void Render(Vector2 center, float columnWidth)
			{
				float width = Width();
				Vector2 pos = center + new Vector2((0f - width) * 0.5f, 0f);
				for (int i = 0; i < icons.Length; i++)
				{
					MTexture gfx = MTN.Journal[icons[i]];
					gfx.DrawJustified(pos, new Vector2(0f, 0.5f));
					pos.X += (float)gfx.Width + iconSpacing;
				}
			}
		}

		public const int PageWidth = 1610;

		public const int PageHeight = 1000;

		public readonly Vector2 TextJustify = new Vector2(0.5f, 0.5f);

		public const float TextScale = 0.5f;

		public readonly Color TextColor = Color.Black * 0.6f;

		public int PageIndex;

		public string PageTexture;

		public OuiJournal Journal;

		public OuiJournalPage(OuiJournal journal)
		{
			Journal = journal;
		}

		public virtual void Redraw(VirtualRenderTarget buffer)
		{
			Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
			Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
		}

		public virtual void Update()
		{
		}
	}
}
