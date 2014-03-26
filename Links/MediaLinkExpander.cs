namespace Constellation.Sitecore.Links
{
	using global::Sitecore;
	using global::Sitecore.Data;
	using global::Sitecore.Data.Items;
	using global::Sitecore.Diagnostics;
	using global::Sitecore.Links;
	using global::Sitecore.Resources.Media;
	using System;
	using System.Text;

	/// <summary>
	/// Verbatim from the Sitecore Media Link Expander, because to implement your own LinkProvider,
	/// you need this class, but there's no way to access their internal class, so you're forced to
	/// copy it.
	/// </summary>
	public class MediaLinkExpander : LinkExpander
	{
		/// <summary>
		/// The expand.
		/// </summary>
		/// <param name="text">The text.</param><param name="urlOptions">The url options.</param>
		public override void Expand(ref string text, UrlOptions urlOptions)
		{
			Assert.ArgumentNotNull(text, "text");
			Assert.ArgumentNotNull(urlOptions, "urlOptions");

			var builder = new StringBuilder();
			var startIndex = 0;
			string prefix;
			var mediaPrefix = this.FindMediaPrefix(text, 0, out prefix);
			while (mediaPrefix >= 0 && text.Length >= mediaPrefix + prefix.Length + 32)
			{
				string id = text.Substring(mediaPrefix + prefix.Length, 32);
				if (!ShortID.IsShortID(id))
				{
					mediaPrefix = this.FindMediaPrefix(text, mediaPrefix + 1, out prefix);
				}
				else
				{
					MediaItem mediaItem = null;
					if (Context.Database != null)
					{
						mediaItem = Context.Database.GetItem(new ID(id));
					}

					if (mediaItem == null)
					{
						mediaPrefix = this.FindMediaPrefix(text, mediaPrefix + 1, out prefix);
					}
					else
					{
						MediaUrlOptions empty = MediaUrlOptions.Empty;
						empty.IncludeExtension = text.Substring(mediaPrefix + prefix.Length + 32 + 1, "ashx".Length) == "ashx";
						string mediaUrl = MediaManager.GetMediaUrl(mediaItem, empty);
						builder.Append(text.Substring(startIndex, mediaPrefix - startIndex));
						builder.Append(mediaUrl);
						startIndex = mediaPrefix + prefix.Length + 32;
						if (empty.IncludeExtension)
						{
							startIndex += "ashx".Length + 1;
						}

						mediaPrefix = this.FindMediaPrefix(text, startIndex, out prefix);
					}
				}
			}

			builder.Append(text.Substring(startIndex));
			text = builder.ToString();
		}

		/// <summary>
		/// The find media prefix.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <param name="startIndex">The start index.</param>
		/// <param name="prefix">The prefix.</param>
		/// <returns>The media prefix.</returns>
		protected virtual int FindMediaPrefix(string text, int startIndex, out string prefix)
		{
			Assert.ArgumentNotNullOrEmpty(text, "text");
			int num1 = int.MaxValue;
			prefix = null;
			foreach (string str in MediaManager.Provider.Config.MediaPrefixes)
			{
				var num2 = text.IndexOf(str, startIndex, StringComparison.OrdinalIgnoreCase);
				if (num2 < 0 || num2 >= num1)
				{
					continue;
				}

				num1 = num2;
				prefix = str;
			}

			return prefix == null ? -1 : num1;
		}
	}
}
