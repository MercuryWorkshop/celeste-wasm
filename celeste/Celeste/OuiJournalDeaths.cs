using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class OuiJournalDeaths : OuiJournalPage
	{
		private Table table;

		public OuiJournalDeaths(OuiJournal journal)
			: base(journal)
		{
			PageTexture = "page";
			table = new Table().AddColumn(new TextCell(Dialog.Clean("journal_deaths"), new Vector2(1f, 0.5f), 0.7f, TextColor, 300f));
			for (int k = 0; k < SaveData.Instance.UnlockedModes; k++)
			{
				table.AddColumn(new TextCell(Dialog.Clean("journal_mode_" + (AreaMode)k), TextJustify, 0.6f, TextColor, 240f));
			}
			bool[] has = new bool[3]
			{
				true,
				SaveData.Instance.UnlockedModes >= 2,
				SaveData.Instance.UnlockedModes >= 3
			};
			int[] all = new int[3];
			foreach (AreaStats data2 in SaveData.Instance.Areas)
			{
				AreaData area2 = AreaData.Get(data2.ID);
				if (area2.Interlude || area2.IsFinal)
				{
					continue;
				}
				if (area2.ID > SaveData.Instance.UnlockedAreas)
				{
					bool flag;
					has[1] = (flag = (has[2] = false));
					has[0] = flag;
					break;
				}
				Row row2 = table.AddRow();
				row2.Add(new TextCell(Dialog.Clean(area2.Name), new Vector2(1f, 0.5f), 0.6f, TextColor));
				for (int j = 0; j < SaveData.Instance.UnlockedModes; j++)
				{
					if (area2.HasMode((AreaMode)j))
					{
						if (data2.Modes[j].SingleRunCompleted)
						{
							int deaths2 = data2.Modes[j].BestDeaths;
							if (deaths2 > 0)
							{
								foreach (EntityData strawb2 in AreaData.Areas[data2.ID].Mode[j].MapData.Goldenberries)
								{
									EntityID entityID2 = new EntityID(strawb2.Level.Name, strawb2.ID);
									if (data2.Modes[j].Strawberries.Contains(entityID2))
									{
										deaths2 = 0;
									}
								}
							}
							row2.Add(new TextCell(Dialog.Deaths(deaths2), TextJustify, 0.5f, TextColor));
							all[j] += deaths2;
						}
						else
						{
							row2.Add(new IconCell("dot"));
							has[j] = false;
						}
					}
					else
					{
						row2.Add(new TextCell("-", TextJustify, 0.5f, TextColor));
					}
				}
			}
			if (has[0] || has[1] || has[2])
			{
				table.AddRow();
				Row totals = table.AddRow();
				totals.Add(new TextCell(Dialog.Clean("journal_totals"), new Vector2(1f, 0.5f), 0.7f, TextColor));
				for (int i = 0; i < SaveData.Instance.UnlockedModes; i++)
				{
					totals.Add(new TextCell(Dialog.Deaths(all[i]), TextJustify, 0.6f, TextColor));
				}
				table.AddRow();
			}
			int finalChaptersDeaths = 0;
			foreach (AreaStats data in SaveData.Instance.Areas)
			{
				AreaData area = AreaData.Get(data.ID);
				if (!area.IsFinal)
				{
					continue;
				}
				if (area.ID > SaveData.Instance.UnlockedAreas)
				{
					break;
				}
				Row row = table.AddRow();
				row.Add(new TextCell(Dialog.Clean(area.Name), new Vector2(1f, 0.5f), 0.6f, TextColor));
				if (data.Modes[0].SingleRunCompleted)
				{
					int deaths = data.Modes[0].BestDeaths;
					if (deaths > 0)
					{
						foreach (EntityData strawb in AreaData.Areas[data.ID].Mode[0].MapData.Goldenberries)
						{
							EntityID entityID = new EntityID(strawb.Level.Name, strawb.ID);
							if (data.Modes[0].Strawberries.Contains(entityID))
							{
								deaths = 0;
							}
						}
					}
					TextCell cell2 = new TextCell(Dialog.Deaths(deaths), TextJustify, 0.5f, TextColor);
					row.Add(cell2);
					finalChaptersDeaths += deaths;
				}
				else
				{
					row.Add(new IconCell("dot"));
				}
				table.AddRow();
			}
			if (has[0] && has[1] && has[2])
			{
				TextCell cell = new TextCell(Dialog.Deaths(all[0] + all[1] + all[2] + finalChaptersDeaths), TextJustify, 0.6f, TextColor)
				{
					SpreadOverColumns = 3
				};
				table.AddRow().Add(new TextCell(Dialog.Clean("journal_grandtotal"), new Vector2(1f, 0.5f), 0.7f, TextColor)).Add(cell);
			}
		}

		public override void Redraw(VirtualRenderTarget buffer)
		{
			base.Redraw(buffer);
			Draw.SpriteBatch.Begin();
			table.Render(new Vector2(60f, 20f));
			Draw.SpriteBatch.End();
		}
	}
}
