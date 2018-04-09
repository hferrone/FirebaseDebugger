using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FBDebugger;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

namespace FBDebugger
{
	/////-------------------------------------------------------------------------------------------------------/////
	//////////////////////////////////<-----------Editor Utilities----------->///////////////////////////////////////
	/////-------------------------------------------------------------------------------------------------------/////

	/// <summary>
	/// Utility functions used with FBDebuggerWindow (Editor Window).
	/// </summary>
	public static class FBDEditorUtils 
	{
		#region Editor window creation
		/// <summary>
		/// Create an empty GameObject with instance of FBDataService attached.
		/// </summary>
		public static void NewManagerInstance() 
		{
			GameObject ds = new GameObject(Constants.ObjectNames.ds);
			ds.transform.position = Vector3.zero;
			ds.AddComponent<FBDataService>();
		}
		#endregion

		#region Scriptable Object management
		public static void SaveConfigurationAsset(string[] nodes, MonoScript mapClass)
		{
			string path = EditorUtility.SaveFilePanelInProject(
				"New Configuration",
				"FBDebuggerConfigurations",
				"asset",
				"Set the name of the new FBDConfiguration asset");

			if (path != "")
				FBDConfiguration.CreateSOAsset(nodes, mapClass, path);
		}
		#endregion

		#region Snapshot parsing
		/// <summary>
		/// Parses a data snapshot dictionary and returns a custom formatted string.
		/// </summary>
		/// <returns>A formatted string.</returns>
		/// <param name="dictionary">Data snapshot dictionary.</param>
		/// <param name="space">Preferred tab spacing.</param>
		public static string DictionaryPrint(Dictionary<string,object> dictionary, string space = "\t")
		{
			string output = "";

			foreach(KeyValuePair<string,object> entry in dictionary)
			{
				output += string.Format("{0}{1,-15}", space, entry.Key);

				if (entry.Value is Dictionary<string, object>)
					output += string.Format("\t{0}\n{1}\n", ParseObjectType(entry.Value), DictionaryPrint((Dictionary<string, object>)entry.Value, space + "\t"));
				else if (entry.Value is List<object>)
					output += string.Format("\t{0}\n{1}", ParseObjectType(entry.Value), ListPrint((List<object>)entry.Value, space + "\t"));
				else
					output += string.Format("{0,-1}\t{1,-5}\n", "", entry.Value);
			}

			return output;
		}

		/// <summary>
		/// Parses a list or array and returns a custom formatted string.
		/// </summary>
		/// <returns>A formatted string.</returns>
		/// <param name="list">List or array.</param>
		/// <param name="space">Preferred tab spacing.</param>
		private static string ListPrint(List<object> list, string space = "\t")
		{
			string output = "";

			foreach (object entry in list)
			{
				output += string.Format("{0}\t", space);

				if (entry is List<object>)
					output += ListPrint((List<object>)entry, space + "\t");
				else if (entry is Dictionary<string, object>)
					output += DictionaryPrint((Dictionary<string, object>)entry, space + "\t");
				else
					output += string.Format("{0,-5}\n", entry);
			}

			return output;
		}

		/// <summary>
		/// Parses the type of the object.
		/// </summary>
		/// <returns>The object type.</returns>
		/// <param name="value">Value.</param>
		public static string ParseObjectType(object value)
		{
			string type = "";

			if (value is List<object>)
				type = "(Array)";
			else if (value is IDictionary)
				type = "(Dictionary)";

			string output = "<color=white>" + type + "</color>";
			return output;
		}
		#endregion

		public static void NewTestData()
		{
			List<string> equipment = new List<string>() { "Diamond Helm", "Gold Tiara", "Ocarina"};
			Player player1 = new Player("hello@gmail.com", "player1", 201, 112.4f, true, equipment);
			string jsonString = JsonUtility.ToJson(player1);
			var db = FirebaseDatabase.DefaultInstance.RootReference;
			var key = db.Child("players").Push().Key;

			db.Child("players").Child(key).SetRawJsonValueAsync(jsonString);
		}
	}
}
