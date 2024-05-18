using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class OuiJournalSpeedrun : OuiJournalPage
	{
		private Table table;

		public OuiJournalSpeedrun(OuiJournal journal)
			: base(journal)
		{
			PageTexture = "page";
			Vector2 justify = new Vector2(0.5f, 0.5f);
			float scale = 0.5f;
			Color color = Color.Black * 0.6f;
			table = new Table().AddColumn(new TextCell(Dialog.Clean("journal_speedruns"), new Vector2(1f, 0.5f), 0.7f, Color.Black * 0.7f)).AddColumn(new TextCell(Dialog.Clean("journal_mode_normal"), justify, scale + 0.1f, color, 240f)).AddColumn(new TextCell(Dialog.Clean("journal_mode_normal_fullclear"), justify, scale + 0.1f, color, 240f));
			if (SaveData.Instance.UnlockedModes >= 2)
			{
				table.AddColumn(new TextCell(Dialog.Clean("journal_mode_bside"), justify, scale + 0.1f, color, 240f));
			}
			if (SaveData.Instance.UnlockedModes >= 3)
			{
				table.AddColumn(new TextCell(Dialog.Clean("journal_mode_cside"), justify, scale + 0.1f, color, 240f));
			}
			foreach (AreaStats data3 in SaveData.Instance.Areas)
			{
				AreaData area3 = AreaData.Get(data3.ID);
				if (area3.Interlude || area3.IsFinal)
				{
					continue;
				}
				if (area3.ID > SaveData.Instance.UnlockedAreas)
				{
					break;
				}
				Row row3 = table.AddRow().Add(new TextCell(Dialog.Clean(area3.Name), new Vector2(1f, 0.5f), scale + 0.1f, color));
				if (data3.Modes[0].BestTime > 0)
				{
					row3.Add(new TextCell(Dialog.Time(data3.Modes[0].BestTime), justify, scale, color));
				}
				else
				{
					row3.Add(new IconCell("dot"));
				}
				if (area3.CanFullClear)
				{
					if (data3.Modes[0].BestFullClearTime > 0)
					{
						row3.Add(new TextCell(Dialog.Time(data3.Modes[0].BestFullClearTime), justify, scale, color));
					}
					else
					{
						row3.Add(new IconCell("dot"));
					}
				}
				else
				{
					row3.Add(new TextCell("-", TextJustify, 0.5f, TextColor));
				}
				if (SaveData.Instance.UnlockedModes >= 2)
				{
					if (area3.HasMode(AreaMode.BSide))
					{
						if (data3.Modes[1].BestTime > 0)
						{
							row3.Add(new TextCell(Dialog.Time(data3.Modes[1].BestTime), justify, scale, color));
						}
						else
						{
							row3.Add(new IconCell("dot"));
						}
					}
					else
					{
						row3.Add(new TextCell("-", TextJustify, 0.5f, TextColor));
					}
				}
				if (SaveData.Instance.UnlockedModes < 3)
				{
					continue;
				}
				if (area3.HasMode(AreaMode.CSide))
				{
					if (data3.Modes[2].BestTime > 0)
					{
						row3.Add(new TextCell(Dialog.Time(data3.Modes[2].BestTime), justify, scale, color));
					}
					else
					{
						row3.Add(new IconCell("dot"));
					}
				}
				else
				{
					row3.Add(new TextCell("-", TextJustify, 0.5f, TextColor));
				}
			}
			bool hasAllClears = true;
			bool hasAllFulls = true;
			bool hasAllRemix = true;
			bool hasAllRemix2 = true;
			long allClearTime = 0L;
			long allFullTime = 0L;
			long allRemixTime = 0L;
			long allRemix2Time = 0L;
			foreach (AreaStats data2 in SaveData.Instance.Areas)
			{
				AreaData area2 = AreaData.Get(data2.ID);
				if (!area2.Interlude && !area2.IsFinal)
				{
					if (data2.ID > SaveData.Instance.UnlockedAreas)
					{
						hasAllClears = (hasAllFulls = (hasAllRemix = (hasAllRemix2 = false)));
						break;
					}
					allClearTime += data2.Modes[0].BestTime;
					allFullTime += data2.Modes[0].BestFullClearTime;
					allRemixTime += data2.Modes[1].BestTime;
					allRemix2Time += data2.Modes[2].BestTime;
					if (data2.Modes[0].BestTime <= 0)
					{
						hasAllClears = false;
					}
					if (area2.CanFullClear && data2.Modes[0].BestFullClearTime <= 0)
					{
						hasAllFulls = false;
					}
					if (area2.HasMode(AreaMode.BSide) && data2.Modes[1].BestTime <= 0)
					{
						hasAllRemix = false;
					}
					if (area2.HasMode(AreaMode.CSide) && data2.Modes[2].BestTime <= 0)
					{
						hasAllRemix2 = false;
					}
				}
			}
			if (hasAllClears || hasAllFulls || hasAllRemix || hasAllRemix2)
			{
				table.AddRow();
				Row row2 = table.AddRow().Add(new TextCell(Dialog.Clean("journal_totals"), new Vector2(1f, 0.5f), scale + 0.2f, color));
				if (hasAllClears)
				{
					row2.Add(new TextCell(Dialog.Time(allClearTime), justify, scale + 0.1f, color));
				}
				else
				{
					row2.Add(new IconCell("dot"));
				}
				if (hasAllFulls)
				{
					row2.Add(new TextCell(Dialog.Time(allFullTime), justify, scale + 0.1f, color));
				}
				else
				{
					row2.Add(new IconCell("dot"));
				}
				if (SaveData.Instance.UnlockedModes >= 2)
				{
					if (hasAllRemix)
					{
						row2.Add(new TextCell(Dialog.Time(allRemixTime), justify, scale + 0.1f, color));
					}
					else
					{
						row2.Add(new IconCell("dot"));
					}
				}
				if (SaveData.Instance.UnlockedModes >= 3)
				{
					if (hasAllRemix2)
					{
						row2.Add(new TextCell(Dialog.Time(allRemix2Time), justify, scale + 0.1f, color));
					}
					else
					{
						row2.Add(new IconCell("dot"));
					}
				}
				table.AddRow();
			}
			long allFinalClearTimes = 0L;
			foreach (AreaStats data in SaveData.Instance.Areas)
			{
				AreaData area = AreaData.Get(data.ID);
				if (area.IsFinal)
				{
					if (area.ID > SaveData.Instance.UnlockedAreas)
					{
						break;
					}
					allFinalClearTimes += data.Modes[0].BestTime;
					Row row = table.AddRow().Add(new TextCell(Dialog.Clean(area.Name), new Vector2(1f, 0.5f), scale + 0.1f, color));
					row.Add(null);
					if (data.Modes[0].BestTime > 0)
					{
						Cell time2;
						row.Add(time2 = new TextCell(Dialog.Time(data.Modes[0].BestTime), justify, scale, color));
					}
					else
					{
						Cell time2;
						row.Add(time2 = new IconCell("dot"));
					}
					table.AddRow();
				}
			}
			if (hasAllClears && hasAllFulls && hasAllRemix && hasAllRemix2)
			{
				TextCell time = new TextCell(Dialog.Time(allClearTime + allFullTime + allRemixTime + allRemix2Time + allFinalClearTimes), justify, scale + 0.2f, color)
				{
					SpreadOverColumns = 1 + SaveData.Instance.UnlockedModes
				};
				table.AddRow().Add(new TextCell(Dialog.Clean("journal_grandtotal"), new Vector2(1f, 0.5f), scale + 0.3f, color)).Add(time);
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
