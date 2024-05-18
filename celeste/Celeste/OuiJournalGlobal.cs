using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class OuiJournalGlobal : OuiJournalPage
	{
		private Table table;

		public OuiJournalGlobal(OuiJournal journal)
			: base(journal)
		{
			PageTexture = "page";
			table = new Table().AddColumn(new TextCell("", new Vector2(1f, 0.5f), 1f, TextColor, 700f)).AddColumn(new TextCell(Dialog.Clean("STATS_TITLE"), new Vector2(0.5f, 0.5f), 1f, TextColor, 48f, forceWidth: true)).AddColumn(new TextCell("", new Vector2(1f, 0.5f), 0.7f, TextColor, 700f));
			foreach (Stat stat in Enum.GetValues(typeof(Stat)))
			{
				if (SaveData.Instance.CheatMode || SaveData.Instance.DebugMode || ((stat != Stat.GOLDBERRIES || SaveData.Instance.TotalHeartGems >= 16) && ((stat != Stat.PICO_BERRIES && stat != Stat.PICO_COMPLETES && stat != Stat.PICO_DEATHS) || Settings.Instance.Pico8OnMainMenu)))
				{
					string value = Stats.Global(stat).ToString();
					string name = Stats.Name(stat);
					string str = "";
					int i = value.Length - 1;
					int j = 0;
					while (i >= 0)
					{
						str = value[i] + ((j > 0 && j % 3 == 0) ? "," : "") + str;
						i--;
						j++;
					}
					Row row = table.AddRow();
					row.Add(new TextCell(name, new Vector2(1f, 0.5f), 0.7f, TextColor));
					row.Add(null);
					row.Add(new TextCell(str, new Vector2(0f, 0.5f), 0.8f, TextColor));
				}
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
