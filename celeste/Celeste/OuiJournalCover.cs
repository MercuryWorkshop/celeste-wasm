using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class OuiJournalCover : OuiJournalPage
	{
		public OuiJournalCover(OuiJournal journal)
			: base(journal)
		{
			PageTexture = "cover";
		}

		public override void Redraw(VirtualRenderTarget buffer)
		{
			base.Redraw(buffer);
			Draw.SpriteBatch.Begin();
			string title = Dialog.Clean("journal_of");
			if (title.Length > 0)
			{
				title += "\n";
			}
			title = ((SaveData.Instance == null || !Dialog.Language.CanDisplay(SaveData.Instance.Name)) ? (title + Dialog.Clean("FILE_DEFAULT")) : (title + SaveData.Instance.Name));
			ActiveFont.Draw(title, new Vector2(805f, 400f), new Vector2(0.5f, 0.5f), Vector2.One * 2f, Color.Black * 0.5f);
			Draw.SpriteBatch.End();
		}
	}
}
