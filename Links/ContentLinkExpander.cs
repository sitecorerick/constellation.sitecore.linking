namespace Constellation.Sitecore.Links
{
	using global::Sitecore.Data.Items;
	using global::Sitecore.Diagnostics;
	using global::Sitecore.Globalization;
	using global::Sitecore.Links;
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Text;

	/// <summary>
	/// Converts Dynamic Links to "friendly URLs" within a block of text.
	/// </summary>
	public class ContentLinkExpander
	{
		/// <summary>
		/// Converts dynamic links to friendly urls within the supplied text.
		/// </summary>
		/// <param name="text">
		/// The supplied text.
		/// </param>
		/// <param name="provider">
		/// The provider.
		/// </param>
		/// <param name="options">
		/// The url options.
		/// </param>
		[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
		public void ExpandLinks(ref string text, LinkProvider provider, UrlOptions options)
		{
			Assert.ArgumentNotNull(text, "text");
			Assert.ArgumentNotNull(options, "urlOptions");
			var linkStart = text.IndexOf("~/link.aspx?", StringComparison.InvariantCulture);
			if (linkStart == -1)
			{
				return;
			}

			var builder = new StringBuilder(text.Length);
			var linkEnd = 0;

			for (; linkStart >= 0; linkStart = text.IndexOf("~/link.aspx?", linkEnd, StringComparison.InvariantCulture))
			{
				var num = text.IndexOf("_z=z", linkStart, StringComparison.InvariantCulture);
				if (num < 0)
				{
					text = builder.ToString();
					return;
				}

				var dynamicLink = DynamicLink.Parse(text.Substring(linkStart, num - linkStart));

				var item = GetItem(dynamicLink);
				var url = "#";
				if (item != null)
				{
					url = provider.GetItemUrl(GetItem(dynamicLink), options);
				}
				var str = text.Substring(linkEnd, linkStart - linkEnd);
				builder.Append(str);
				builder.Append(url);
				linkEnd = num + "_z=z".Length;
			}

			builder.Append(text.Substring(linkEnd));
			text = builder.ToString();
		}

		/// <summary>
		/// Gets the item from a dynamic link.
		/// </summary>
		/// <param name="link">
		/// The given link.
		/// </param>
		/// <returns>
		/// The Item<see cref="Item"/>.
		/// </returns>
		private static Item GetItem(DynamicLink link)
		{
			var site = link.Site ?? global::Sitecore.Context.Site;
			var database = site.Database;

			Language language;
			if (link.Language != null)
			{
				language = link.Language;
			}
			else
			{
				Language.TryParse(site.Language, out language);
			}

			return database.GetItem(link.ItemId, language);
		}
	}
}