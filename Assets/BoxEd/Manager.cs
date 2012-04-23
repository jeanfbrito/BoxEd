using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;

namespace BoxEd
{
	/// <summary>
	/// Handles serialisation and general level functionality.
	/// </summary>
	public static class LevelManager
	{
		public static string LevelName { get; set; }
		public static string AuthorName { get; set; }

		/// <summary>
		/// Serialises the currently loaded level into an .xml file in the levels folder.
		/// </summary>
		/// <param name="levelName">The name of the level to load.</param>
		public static void SaveLevel(string levelName, bool useFullpath = false)
		{
			//Construct a new document and create the root node
			var document = new XmlDocument();
			var root = document.CreateElement("BoxLevel");
			document.AppendChild(root);

			//In the meta node, we store data relating to the level, such as the author's details and the version of BoxEd with which it was created
			var meta = document.CreateElement("Meta");
			root.AppendChild(meta);
			{
				var level = document.CreateElement("Level");
				meta.AppendChild(level);

				level.SetAttribute("Name", LevelManager.LevelName);

				var author = document.CreateElement("Author");
				meta.AppendChild(author);

				author.SetAttribute("Author", LevelManager.AuthorName);

				var version = document.CreateElement("Version");
				meta.AppendChild(version);

				version.SetAttribute("App", Editor.Version.ToString(2));
			}

			//Data houses all the entities from the game world
			var data = document.CreateElement("Data");
			root.AppendChild(data);

			//This loop is where we serialise all the entities to XML
			foreach(var entity in LevelManager.Find<Entity>())
			{
				var type = entity.GetType();
				var element = document.CreateElement(type.Name);

				//This scary piece of Linq code just gets us all properties that use the BoxEdProperty attribute
				var properties = type.GetProperties().Where(prop => prop.GetCustomAttributes(typeof(EntityPropertyAttribute), true).Length > 0);

				//Unique names will be a future feature for referencing entities
				if(entity.UniqueName != null)
					element.SetAttribute("Name", entity.UniqueName);

				element.SetAttribute("Position", ConvertVector(entity.transform.position));

				foreach(var property in properties)
					element.SetAttribute(property.PropertyType + "." + property.Name, property.GetValue(entity, null).ToString());

				data.AppendChild(element);
			}

			if(!useFullpath)
			{
				if(!Directory.Exists(LevelManager.LevelFolder))
					Directory.CreateDirectory(LevelManager.LevelFolder);

				Editor.Log("Saving to {0}. Content is: {1}", LevelManager.GetFilepath(levelName), document.InnerXml);
				document.Save(LevelManager.GetFilepath(levelName));
			}
#if UNITY_EDITOR
			else
			{
				Editor.Log("Updating the webplayer demo. Content is: {0}", document.InnerXml);
				document.Save(Path.Combine(Application.dataPath, @"Resources\Level 1.box.xml"));
			}
#endif
		}

		/// <summary>
		/// Deserialises a level saved in .xml form.
		/// </summary>
		/// <param name="levelName"></param>
		/// <returns>Returns false if the level does not exist, or a parsing issue occurred.</returns>
		public static bool LoadLevel(string levelName, bool webplayer = false)
		{
			foreach(var ent in LevelManager.Find<Entity>())
				GameObject.Destroy(ent.gameObject);

			var document = new XmlDocument();

			//Usually we load from file but if not, we can load a string directly embedded in Resourcess
			if(!webplayer)
				document.Load(GetFilepath(levelName));
			else
				document.LoadXml(levelName);

			var root = document.FirstChild;

			var meta = root.SelectSingleNode(BoxFormat.MetaNode);
			{
				LevelManager.LevelName = (meta.SelectSingleNode("Level") as XmlElement).GetAttribute("Name");
			}

			var data = root.SelectSingleNode(BoxFormat.DataNode);

			//Traverse the doc for all XmlElements, in which the entities are stored
			foreach(var child in data.ChildNodes.OfType<XmlElement>())
			{
				//Instantiate the entity using the stored name
				var type = Type.GetType(child.Name);
				var entity = Entity.Create<Entity>(ParseVector(child.GetAttribute("Position")), type: type);

				//Set property values
				foreach(var property in type.GetProperties().Where(property => property.GetCustomAttributes(typeof(EntityPropertyAttribute), true).Length > 0))
				{
					var name = child.GetAttribute(property.PropertyType + "." + property.Name);

					if(!String.IsNullOrEmpty(name))
						property.SetValue(entity, Convert.ChangeType(name, property.PropertyType), null);
				}
			}

			return true;
		}

		/// <summary>
		/// Retrieves the level folder.
		/// </summary>
		public static string LevelFolder
		{
			get
			{
				var path = Path.Combine(Application.persistentDataPath, "Levels");

				if(!Directory.Exists(path))
					Directory.CreateDirectory(path);

				return path;
			}
		}

		/// <summary>
		/// Retrieves the full filepath to the given level.
		/// </summary>
		/// <param name="levelName"></param>
		/// <returns></returns>
		public static string GetFilepath(string levelName)
		{
			return Path.Combine(LevelFolder, levelName + ".box");
		}

		/// <summary>
		/// Finds a list of MonoBehaviours of type T.
		/// </summary>
		/// <typeparam name="T">The type of script to search for.</typeparam>
		/// <returns>A collection containing all the located instances.</returns>
		public static IEnumerable<T> Find<T>() where T : MonoBehaviour
		{
			return GameObject.FindObjectsOfType(typeof(T)) as IEnumerable<T>;
		}

		/// <summary>
		/// Converts a vector to a lightweight string representation.
		/// </summary>
		/// <param name="vec">The vector to convert.</param>
		/// <returns>The string representation of the vector in the form "x,y,z".</returns>
		private static string ConvertVector(Vector3 vec)
		{
			return String.Format("{0},{1},{2}", vec.x, vec.y, vec.z);
		}

		/// <summary>
		/// Parses a vector from a given string.
		/// </summary>
		/// <param name="value">The vector as a string, in the form "x,y,z".</param>
		/// <returns>The parsed vector.</returns>
		private static Vector3 ParseVector(string value)
		{
			var split = value.Split(',');
			return new Vector3(Single.Parse(split[0]), Single.Parse(split[1]), Single.Parse(split[2]));
		}
	}
}