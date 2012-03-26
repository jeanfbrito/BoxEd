using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

namespace BoxEd
{
	public static class Localisation
	{
		public static LocaleDictionary BoxEd { get; private set; }

		public static void LoadLanguage(string languageCode)
		{
			BoxEd = LocaleDictionary.Load(Resources.Load("Editor") as TextAsset, languageCode);
		}
	}

	/// <summary>
	/// Specialised dictionary used for localisation.
	/// </summary>
	public class LocaleDictionary : Dictionary<string, string>
	{
		/// <summary>
		/// Accesses the currently loaded string if it has been set.
		/// </summary>
		/// <param name="key">The localisation string to search for.</param>
		/// <returns>The </returns>
		public new string this[string key]
		{
			get
			{
				return base.ContainsKey(key) && !string.IsNullOrEmpty(base[key]) ? base[key] : key;
			}
		}

		public static LocaleDictionary Load(string xmlString, string languageCode)
		{
			var dictionary = new LocaleDictionary();
			var document = new XmlDocument();
			document.LoadXml(xmlString);

			foreach(var phrase in document.FirstChild.ChildNodes.OfType<XmlElement>().Where(phrase => phrase.HasAttribute(languageCode)))
				dictionary.Add(phrase.Name, phrase.GetAttribute(languageCode));

			return dictionary;
		}

		public static LocaleDictionary Load(TextAsset textAsset, string languageCode)
		{
			return Load(textAsset.ToString(), languageCode);
		}
	}
}