using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class OuiJournalProgress : OuiJournalPage
	{
		private Table table;

		public OuiJournalProgress(OuiJournal journal)
			: base(journal)
		{
			PageTexture = "page";
			table = new Table().AddColumn(new TextCell(Dialog.Clean("journal_progress"), new Vector2(0f, 0.5f), 1f, Color.Black * 0.7f)).AddColumn(new EmptyCell(20f)).AddColumn(new EmptyCell(64f))
				.AddColumn(new EmptyCell(64f))
				.AddColumn(new EmptyCell(100f))
				.AddColumn(new IconCell("strawberry", 150f))
				.AddColumn(new IconCell("skullblue", 100f));
			if (SaveData.Instance.UnlockedModes >= 2)
			{
				table.AddColumn(new IconCell("skullred", 100f));
			}
			if (SaveData.Instance.UnlockedModes >= 3)
			{
				table.AddColumn(new IconCell("skullgold", 100f));
			}
			table.AddColumn(new IconCell("time", 220f));
			foreach (AreaStats data in SaveData.Instance.Areas)
			{
				AreaData area = AreaData.Get(data.ID);
				if (area.Interlude)
				{
					continue;
				}
				if (area.ID > SaveData.Instance.UnlockedAreas)
				{
					break;
				}
				string strawberries = null;
				if (area.Mode[0].TotalStrawberries > 0 || data.TotalStrawberries > 0)
				{
					strawberries = data.TotalStrawberries.ToString();
					if (data.Modes[0].Completed)
					{
						strawberries = strawberries + "/" + area.Mode[0].TotalStrawberries;
					}
				}
				else
				{
					strawberries = "-";
				}
				List<string> heartgems = new List<string>();
				for (int l = 0; l < data.Modes.Length; l++)
				{
					if (data.Modes[l].HeartGem)
					{
						heartgems.Add("heartgem" + l);
					}
				}
				if (heartgems.Count <= 0)
				{
					heartgems.Add("dot");
				}
				IconsCell completeCell;
				Row row2 = table.AddRow().Add(new TextCell(Dialog.Clean(area.Name), new Vector2(1f, 0.5f), 0.6f, TextColor)).Add(null)
					.Add(completeCell = new IconsCell(CompletionIcon(data)));
				if (area.CanFullClear)
				{
					row2.Add(new IconsCell(data.Cassette ? "cassette" : "dot"));
					row2.Add(new IconsCell(-32f, heartgems.ToArray()));
				}
				else
				{
					completeCell.SpreadOverColumns = 3;
					row2.Add(null).Add(null);
				}
				row2.Add(new TextCell(strawberries, TextJustify, 0.5f, TextColor));
				if (area.IsFinal)
				{
					row2.Add(new TextCell(Dialog.Deaths(data.Modes[0].Deaths), TextJustify, 0.5f, TextColor)
					{
						SpreadOverColumns = SaveData.Instance.UnlockedModes
					});
					for (int k = 0; k < SaveData.Instance.UnlockedModes - 1; k++)
					{
						row2.Add(null);
					}
				}
				else
				{
					for (int j = 0; j < SaveData.Instance.UnlockedModes; j++)
					{
						if (area.HasMode((AreaMode)j))
						{
							row2.Add(new TextCell(Dialog.Deaths(data.Modes[j].Deaths), TextJustify, 0.5f, TextColor));
						}
						else
						{
							row2.Add(new TextCell("-", TextJustify, 0.5f, TextColor));
						}
					}
				}
				if (data.TotalTimePlayed > 0)
				{
					row2.Add(new TextCell(Dialog.Time(data.TotalTimePlayed), TextJustify, 0.5f, TextColor));
				}
				else
				{
					row2.Add(new IconCell("dot"));
				}
			}
			if (table.Rows > 1)
			{
				table.AddRow();
				Row row = table.AddRow().Add(new TextCell(Dialog.Clean("journal_totals"), new Vector2(1f, 0.5f), 0.7f, TextColor)).Add(null)
					.Add(null)
					.Add(null)
					.Add(null)
					.Add(new TextCell(SaveData.Instance.TotalStrawberries.ToString(), TextJustify, 0.6f, TextColor));
				row.Add(new TextCell(Dialog.Deaths(SaveData.Instance.TotalDeaths), TextJustify, 0.6f, TextColor)
				{
					SpreadOverColumns = SaveData.Instance.UnlockedModes
				});
				for (int i = 1; i < SaveData.Instance.UnlockedModes; i++)
				{
					row.Add(null);
				}
				row.Add(new TextCell(Dialog.Time(SaveData.Instance.Time), TextJustify, 0.6f, TextColor));
				table.AddRow();
			}
		}

		private string CompletionIcon(AreaStats data)
		{
			if (!AreaData.Get(data.ID).CanFullClear && data.Modes[0].Completed)
			{
				return "beat";
			}
			if (data.Modes[0].FullClear)
			{
				return "fullclear";
			}
			if (data.Modes[0].Completed)
			{
				return "clear";
			}
			return "dot";
		}

		public override void Redraw(VirtualRenderTarget buffer)
		{
			base.Redraw(buffer);
			Draw.SpriteBatch.Begin();
			table.Render(new Vector2(60f, 20f));
			Draw.SpriteBatch.End();
		}

		private void DrawIcon(Vector2 pos, bool obtained, string icon)
		{
			if (obtained)
			{
				MTN.Journal[icon].DrawCentered(pos);
			}
			else
			{
				MTN.Journal["dot"].DrawCentered(pos);
			}
		}
	}
}
