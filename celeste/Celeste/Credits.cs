using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class Credits
	{
		private abstract class CreditNode
		{
			public abstract void Render(Vector2 position, float alignment = 0.5f, float scale = 1f);

			public abstract float Height(float scale = 1f);
		}

		private class Role : CreditNode
		{
			public const float NameScale = 1.8f;

			public const float RolesScale = 1f;

			public const float Spacing = 8f;

			public const float BottomSpacing = 64f;

			public static readonly Color NameColor = Color.White;

			public static readonly Color RolesColor = Color.White * 0.8f;

			public string Name;

			public string Roles;

			public Role(string name, params string[] roles)
			{
				Name = name;
				Roles = string.Join(", ", roles);
			}

			public override void Render(Vector2 position, float alignment = 0.5f, float scale = 1f)
			{
				Font.DrawOutline(FontSize, Name, position.Floor(), new Vector2(alignment, 0f), Vector2.One * 1.8f * scale, NameColor, 2f, BorderColor);
				position.Y += (LineHeight * 1.8f + 8f) * scale;
				Font.DrawOutline(FontSize, Roles, position.Floor(), new Vector2(alignment, 0f), Vector2.One * 1f * scale, RolesColor, 2f, BorderColor);
			}

			public override float Height(float scale = 1f)
			{
				return (LineHeight * 2.8f + 8f + 64f) * scale;
			}
		}

		private class Team : CreditNode
		{
			public const float TeamScale = 1.4f;

			public static readonly Color TeamColor = Color.White;

			public string Name;

			public string[] Members;

			public string Roles;

			public Team(string name, string[] members, params string[] roles)
			{
				Name = name;
				Members = members;
				Roles = string.Join(", ", roles);
			}

			public override void Render(Vector2 position, float alignment = 0.5f, float scale = 1f)
			{
				Font.DrawOutline(FontSize, Name, position.Floor(), new Vector2(alignment, 0f), Vector2.One * 1.8f * scale, Role.NameColor, 2f, BorderColor);
				position.Y += (LineHeight * 1.8f + 8f) * scale;
				for (int i = 0; i < Members.Length; i++)
				{
					Font.DrawOutline(FontSize, Members[i], position.Floor(), new Vector2(alignment, 0f), Vector2.One * 1.4f * scale, TeamColor, 2f, BorderColor);
					position.Y += LineHeight * 1.4f * scale;
				}
				Font.DrawOutline(FontSize, Roles, position.Floor(), new Vector2(alignment, 0f), Vector2.One * 1f * scale, Role.RolesColor, 2f, BorderColor);
			}

			public override float Height(float scale = 1f)
			{
				return (LineHeight * (1.8f + (float)Members.Length * 1.4f + 1f) + 8f + 64f) * scale;
			}
		}

		private class Thanks : CreditNode
		{
			public const float TitleScale = 1.4f;

			public const float CreditsScale = 1.15f;

			public const float Spacing = 8f;

			public readonly Color TitleColor = Color.White;

			public readonly Color CreditsColor = Color.White * 0.8f;

			public int TopPadding;

			public string Title;

			public string[] Credits;

			private string[] linkedImages;

			public Thanks(string title, params string[] to)
				: this(0, title, to)
			{
			}

			public Thanks(int topPadding, string title, params string[] to)
			{
				TopPadding = topPadding;
				Title = title;
				Credits = to;
				linkedImages = new string[Credits.Length];
				for (int i = 0; i < linkedImages.Length; i++)
				{
					linkedImages[i] = null;
					if (Credits[i].StartsWith("image:"))
					{
						linkedImages[i] = Credits[i].Substring(6);
					}
				}
			}

			public override void Render(Vector2 position, float alignment = 0.5f, float scale = 1f)
			{
				position.Y += (float)TopPadding * scale;
				Font.DrawOutline(FontSize, Title, position.Floor(), new Vector2(alignment, 0f), Vector2.One * 1.4f * scale, TitleColor, 2f, BorderColor);
				position.Y += (LineHeight * 1.4f + 8f) * scale;
				for (int i = 0; i < Credits.Length; i++)
				{
					if (linkedImages[i] != null)
					{
						MTexture tex = GFX.Gui[linkedImages[i]];
						tex.DrawJustified(position.Floor() + new Vector2(0f, -2f), new Vector2(alignment, 0f), BorderColor, 1.15f * scale, 0f);
						tex.DrawJustified(position.Floor() + new Vector2(0f, 2f), new Vector2(alignment, 0f), BorderColor, 1.15f * scale, 0f);
						tex.DrawJustified(position.Floor() + new Vector2(-2f, 0f), new Vector2(alignment, 0f), BorderColor, 1.15f * scale, 0f);
						tex.DrawJustified(position.Floor() + new Vector2(2f, 0f), new Vector2(alignment, 0f), BorderColor, 1.15f * scale, 0f);
						tex.DrawJustified(position.Floor(), new Vector2(alignment, 0f), CreditsColor, 1.15f * scale, 0f);
						position.Y += (float)tex.Height * 1.15f * scale;
					}
					else
					{
						Font.DrawOutline(FontSize, Credits[i], position.Floor(), new Vector2(alignment, 0f), Vector2.One * 1.15f * scale, CreditsColor, 2f, BorderColor);
						position.Y += LineHeight * 1.15f * scale;
					}
				}
			}

			public override float Height(float scale = 1f)
			{
				return (LineHeight * (1.4f + (float)Credits.Length * 1.15f) + ((Credits.Length != 0) ? 8f : 0f) + (float)TopPadding) * scale;
			}
		}

		private class MultiCredit : CreditNode
		{
			public class Section
			{
				public string Subtitle;

				public int SubtitleLines;

				public string[] Credits;

				public Section(string subtitle, params string[] credits)
				{
					Subtitle = subtitle.ToUpper();
					SubtitleLines = subtitle.Split('\n').Length;
					Credits = credits;
				}
			}

			public const float TitleScale = 1.4f;

			public const float SubtitleScale = 0.7f;

			public const float CreditsScale = 1.15f;

			public const float Spacing = 8f;

			public const float SectionSpacing = 32f;

			public readonly Color TitleColor = Color.White;

			public readonly Color SubtitleColor = Calc.HexToColor("a8a694");

			public readonly Color CreditsColor = Color.White * 0.8f;

			public int TopPadding;

			public string Title;

			public Section[] Sections;

			public MultiCredit(string title, params Section[] to)
				: this(0, title, to)
			{
			}

			public MultiCredit(int topPadding, string title, Section[] to)
			{
				TopPadding = topPadding;
				Title = title;
				Sections = to;
			}

			public override void Render(Vector2 position, float alignment = 0.5f, float scale = 1f)
			{
				position.Y += (float)TopPadding * scale;
				Font.DrawOutline(FontSize, Title, position.Floor(), new Vector2(alignment, 0f), Vector2.One * 1.4f * scale, TitleColor, 2f, BorderColor);
				position.Y += (LineHeight * 1.4f + 8f) * scale;
				for (int i = 0; i < Sections.Length; i++)
				{
					Section section = Sections[i];
					_ = section.Subtitle;
					Font.DrawOutline(FontSize, section.Subtitle, position.Floor(), new Vector2(alignment, 0f), Vector2.One * 0.7f * scale, SubtitleColor, 2f, BorderColor);
					position.Y += (float)section.SubtitleLines * LineHeight * 0.7f * scale;
					for (int j = 0; j < section.Credits.Length; j++)
					{
						Font.DrawOutline(FontSize, section.Credits[j], position.Floor(), new Vector2(alignment, 0f), Vector2.One * 1.15f * scale, CreditsColor, 2f, BorderColor);
						position.Y += LineHeight * 1.15f * scale;
					}
					position.Y += 32f * scale;
				}
			}

			public override float Height(float scale = 1f)
			{
				float height = 0f;
				height += (float)TopPadding;
				height += LineHeight * 1.4f + 8f;
				for (int i = 0; i < Sections.Length; i++)
				{
					height += (float)Sections[i].SubtitleLines * LineHeight * 0.7f;
					height += LineHeight * 1.15f * (float)Sections[i].Credits.Length;
				}
				height += 32f * (float)(Sections.Length - 1);
				return height * scale;
			}
		}

		private class Ending : CreditNode
		{
			public string Text;

			public bool Spacing;

			public Ending(string text, bool spacing)
			{
				Text = text;
				Spacing = spacing;
			}

			public override void Render(Vector2 position, float alignment = 0.5f, float scale = 1f)
			{
				if (Spacing)
				{
					position.Y += 540f;
				}
				else
				{
					position.Y += ActiveFont.LineHeight * 1.5f * scale * 0.5f;
				}
				ActiveFont.DrawOutline(Text, new Vector2(960f, position.Y), new Vector2(0.5f, 0.5f), Vector2.One * 1.5f * scale, Color.White, 2f, BorderColor);
			}

			public override float Height(float scale = 1f)
			{
				if (Spacing)
				{
					return 540f;
				}
				return ActiveFont.LineHeight * 1.5f * scale;
			}
		}

		private class Image : CreditNode
		{
			public Atlas Atlas;

			public string ImagePath;

			public float BottomPadding;

			public float Rotation;

			public bool ScreenCenter;

			public Image(string path, float bottomPadding = 0f)
				: this(GFX.Gui, path, bottomPadding)
			{
			}

			public Image(Atlas atlas, string path, float bottomPadding = 0f, float rotation = 0f, bool screenCenter = false)
			{
				Atlas = atlas;
				ImagePath = path;
				BottomPadding = bottomPadding;
				Rotation = rotation;
				ScreenCenter = screenCenter;
			}

			public override void Render(Vector2 position, float alignment = 0.5f, float scale = 1f)
			{
				MTexture img = Atlas[ImagePath];
				Vector2 pos = position + new Vector2((float)img.Width * (0.5f - alignment), (float)img.Height * 0.5f) * scale;
				if (ScreenCenter)
				{
					pos.X = 960f;
				}
				img.DrawCentered(pos, Color.White, scale, Rotation);
			}

			public override float Height(float scale = 1f)
			{
				return ((float)Atlas[ImagePath].Height + BottomPadding) * scale;
			}
		}

		private class ImageRow : CreditNode
		{
			private Image[] images;

			public ImageRow(params Image[] images)
			{
				this.images = images;
			}

			public override void Render(Vector2 position, float alignment = 0.5f, float scale = 1f)
			{
				float height = Height(scale);
				float width = 0f;
				Image[] array = images;
				foreach (Image img in array)
				{
					width += (float)(img.Atlas[img.ImagePath].Width + 32) * scale;
				}
				width -= 32f * scale;
				Vector2 pos = position - new Vector2(alignment * width, 0f);
				array = images;
				foreach (Image img2 in array)
				{
					img2.Render(pos + new Vector2(0f, (height - img2.Height(scale)) / 2f), 0f, scale);
					pos.X += (float)(img2.Atlas[img2.ImagePath].Width + 32) * scale;
				}
			}

			public override float Height(float scale = 1f)
			{
				float highest = 0f;
				Image[] array = images;
				foreach (Image img in array)
				{
					if (img.Height(scale) > highest)
					{
						highest = img.Height(scale);
					}
				}
				return highest;
			}
		}

		private class Break : CreditNode
		{
			public float Size;

			public Break(float size = 64f)
			{
				Size = size;
			}

			public override void Render(Vector2 position, float alignment = 0.5f, float scale = 1f)
			{
			}

			public override float Height(float scale = 1f)
			{
				return Size * scale;
			}
		}

		public static string[] Remixers = new string[8] { "Maxo", "Ben Prunty", "Christa Lee", "in love with a ghost", "2 Mello", "Jukio Kallio", "Kuraine", "image:matthewseiji" };

		public static Color BorderColor = Color.Black;

		public const float CreditSpacing = 64f;

		public const float AutoScrollSpeed = 100f;

		public const float InputScrollSpeed = 600f;

		public const float ScrollResumeDelay = 1f;

		public const float ScrollAcceleration = 1800f;

		private List<CreditNode> credits;

		public float AutoScrollSpeedMultiplier = 1f;

		private float scrollSpeed = 100f;

		private float scroll;

		private float height;

		private float scrollDelay;

		private float scrollbarAlpha;

		private float alignment;

		private float scale;

		public float BottomTimer;

		public bool Enabled = true;

		public bool AllowInput = true;

		public static PixelFont Font;

		public static float FontSize;

		public static float LineHeight;

		private static List<CreditNode> CreateCredits(bool title, bool polaroids)
		{
			List<CreditNode> list = new List<CreditNode>();
			if (title)
			{
				list.Add(new Image("title", 320f));
			}
			list.AddRange(new List<CreditNode>
			{
				new Role("Maddy Thorson", "Director", "Designer", "Writer", "Gameplay Coder"),
				new Role("Noel Berry", "Co-Creator", "Programmer", "Artist"),
				new Role("Amora B.", "Concept Artist", "High Res Artist"),
				new Role("Pedro Medeiros", "Pixel Artist", "UI Artist"),
				new Role("Lena Raine", "Composer"),
				new Team("Power Up Audio", new string[4] { "Kevin Regamey", "Jeff Tangsoc", "Joey Godard", "Cole Verderber" }, "Sound Designers")
			});
			if (polaroids)
			{
				list.Add(new Image(GFX.Portraits, "credits/a", 64f, -0.05f));
			}
			list.AddRange(new List<CreditNode>
			{
				new Role("Gabby DaRienzo", "3D Artist"),
				new Role("Sven Bergström", "3D Lighting Artist")
			});
			list.AddRange(new List<CreditNode>
			{
				new Thanks("Writing Assistance", "Noel Berry", "Amora B.", "Greg Lobanov", "Lena Raine", "Nick Suttner"),
				new Thanks("Script Editor", "Nick Suttner"),
				new Thanks("Narrative Consulting", "Silverstring Media", "Claris Cyarron", "with Lucas JW Johnson", "and Tanya Kan")
			});
			list.Add(new Thanks("Remixers", Remixers));
			list.Add(new MultiCredit("Musical Performances", new MultiCredit.Section("Violin, Viola", "Michaela Nachtigall"), new MultiCredit.Section("Cello", "SungHa Hong"), new MultiCredit.Section("Drums", "Doug Perry")));
			if (polaroids)
			{
				list.Add(new Image(GFX.Portraits, "credits/b", 64f, 0.05f));
			}
			list.Add(new Thanks("Operations Manager", "Heidy Motta"));
			list.Add(new Thanks("Porting", "Sickhead Games, LLC", "Ethan Lee"));
			list.Add(new MultiCredit("Localization", new MultiCredit.Section("French, Italian, Korean, Russian,\nSimplified Chinese, Spanish,\nBrazilian Portuguese, German", "EDS Wordland, Ltd."), new MultiCredit.Section("Japanese", "8-4, Ltd", "Keiko Fukuichi", "Graeme Howard", "John Ricciardi"), new MultiCredit.Section("German, Additional Brazilian Portuguese", "Shloc, Ltd.", "Oli Chance", "Isabel Sterner", "Nadine Leonhardt"), new MultiCredit.Section("Additional Brazilian Portuguese", "Amora B.")));
			list.Add(new Thanks("Contributors & Playtesters", "Nels Anderson", "Liam & Graeme Berry", "Tamara Bruketta", "Allan Defensor", "Grayson Evans", "Jada Gibbs", "Em Halberstadt", "Justin Jaffray", "Chevy Ray Johnston", "Will Lacerda", "Myriame Lachapelle", "Greg Lobanov", "Rafinha Martinelli", "Shane Neville", "Kyle Pulver", "Murphy Pyan", "Garret Randell", "Kevin Regamey", "Atlas Regaudie", "Stefano Strapazzon", "Nick Suttner", "Ryan Thorson", "Greg Wohlwend", "Justin Yngelmo", "baldjared", "zep", "DevilSquirrel", "Covert_Muffin", "buhbai", "Chaikitty", "Msushi", "TGH"));
			list.Add(new Thanks("Community", "Speedrunners & Tool Assisted Speedrunners", "Everest Modding Community", "The Celeste Discord"));
			list.Add(new Thanks("Special Thanks", "Fe Angelo", "Bruce Berry & Marilyn Firth", "Josephine Baird", "Liliane Carpinski", "Yvonne Hanson", "Katherine Elaine Jones", "Clint 'halfcoordinated' Lexa", "Greg Lobanov", "Gabi 'Platy' Madureira", "Rodrigo Monteiro", "Fernando Piovesan", "Paulo Szyszko Pita", "Zoe Si", "Julie, Richard, & Ryan Thorson", "Davey Wreden"));
			if (polaroids)
			{
				list.Add(new Image(GFX.Portraits, "credits/c", 64f, -0.05f));
			}
			list.Add(new Thanks("Production Kitties", "Jiji, Mr Satan, Peridot", "Azzy, Phil, Furiosa", "Fred, Bastion, Meredith", "Bobbin, Finn"));
			list.AddRange(new List<CreditNode>
			{
				new Image(GFX.Misc, "fmod"),
				new Image(GFX.Misc, "monogame"),
				new ImageRow(new Image(GFX.Misc, "fna"), new Image(GFX.Misc, "xna"))
			});
			list.Add(new Break(540f));
			if (polaroids)
			{
				list.Add(new Image(GFX.Portraits, "credits/d", 0f, 0.05f, screenCenter: true));
			}
			list.Add(new Ending(Dialog.Clean("CREDITS_THANKYOU"), !polaroids));
			return list;
		}

		public Credits(float alignment = 0.5f, float scale = 1f, bool haveTitle = true, bool havePolaroids = false)
		{
			this.alignment = alignment;
			this.scale = scale;
			credits = CreateCredits(haveTitle, havePolaroids);
			Font = Dialog.Languages["english"].Font;
			FontSize = Dialog.Languages["english"].FontFaceSize;
			LineHeight = Font.Get(FontSize).LineHeight;
			height = 0f;
			foreach (CreditNode credit in credits)
			{
				height += credit.Height(scale) + 64f * scale;
			}
			height += 476f;
			if (havePolaroids)
			{
				height -= 280f;
			}
		}

		public void Update()
		{
			if (Enabled)
			{
				scroll += scrollSpeed * Engine.DeltaTime * scale;
				if (scrollDelay <= 0f)
				{
					scrollSpeed = Calc.Approach(scrollSpeed, 100f * AutoScrollSpeedMultiplier, 1800f * Engine.DeltaTime);
				}
				else
				{
					scrollDelay -= Engine.DeltaTime;
				}
				if (AllowInput)
				{
					if (Input.MenuDown.Check)
					{
						scrollDelay = 1f;
						scrollSpeed = Calc.Approach(scrollSpeed, 600f, 1800f * Engine.DeltaTime);
					}
					else if (Input.MenuUp.Check)
					{
						scrollDelay = 1f;
						scrollSpeed = Calc.Approach(scrollSpeed, -600f, 1800f * Engine.DeltaTime);
					}
					else if (scrollDelay > 0f)
					{
						scrollSpeed = Calc.Approach(scrollSpeed, 0f, 1800f * Engine.DeltaTime);
					}
				}
				if (scroll < 0f || scroll > height)
				{
					scrollSpeed = 0f;
				}
				scroll = Calc.Clamp(scroll, 0f, height);
				if (scroll >= height)
				{
					BottomTimer += Engine.DeltaTime;
				}
				else
				{
					BottomTimer = 0f;
				}
			}
			scrollbarAlpha = Calc.Approach(scrollbarAlpha, (Enabled && scrollDelay > 0f) ? 1f : 0f, Engine.DeltaTime * 2f);
		}

		public void Render(Vector2 position)
		{
			Vector2 pos = position + new Vector2(0f, 1080f - scroll).Floor();
			foreach (CreditNode credit in credits)
			{
				float height = credit.Height(scale);
				if (pos.Y > 0f - height && pos.Y < 1080f)
				{
					credit.Render(pos, alignment, scale);
				}
				pos.Y += height + 64f * scale;
			}
			if (scrollbarAlpha > 0f)
			{
				int padding = 64;
				int winHeight = 1080 - padding * 2;
				float barHeight = (float)winHeight * ((float)winHeight / this.height);
				float barY = scroll / this.height * ((float)winHeight - barHeight);
				Draw.Rect(1844f, padding, 12f, winHeight, Color.White * 0.2f * scrollbarAlpha);
				Draw.Rect(1844f, (float)padding + barY, 12f, barHeight, Color.White * 0.5f * scrollbarAlpha);
			}
		}
	}
}
