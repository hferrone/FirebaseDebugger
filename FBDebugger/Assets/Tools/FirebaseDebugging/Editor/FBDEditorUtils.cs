using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FBDebugger;

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
		/// <summary>
		/// Create an empty GameObject with instance of FBDataService attached.
		/// </summary>
		public static void NewManagerInstance() 
		{
			GameObject ds = new GameObject(Constants.ObjectNames.ds);
			ds.transform.position = Vector3.zero;
			ds.AddComponent<FBDataService>();
		}

		/// <summary>
		/// Dicts the parse.
		/// </summary>
		/// <returns>The parse.</returns>
		/// <param name="dictionary">Dictionary.</param>
		/// <param name="sequenceSeparator">Sequence separator.</param>
		public static string DictParse(Dictionary<string, object> dictionary, string sequenceSeparator)
		{
			StringBuilder sb = new StringBuilder();

			foreach (var kvp in dictionary)
			{
				if (kvp.Value is IDictionary)
				{
					sb.AppendFormat("\t{0}\t{1}", kvp.Key, ParseObjectType(kvp.Value));
					sb.Append(sequenceSeparator);
					sb.AppendFormat("\t{0}", DictParse(kvp.Value as Dictionary<string, object>, sequenceSeparator));
					sb.AppendFormat("{0}\t", sequenceSeparator);
				}
				else
				{
					sb.AppendFormat("\t\t{0,-5}\t{1,-5}\t{2,5}", kvp.Key, kvp.Value, ParseObjectType(kvp.Value));
					sb.AppendFormat("{0}\t", sequenceSeparator);
				}
			}

			return sb.ToString(0, sb.Length - sequenceSeparator.Length);
		}

		public static string DictionaryPrint(Dictionary<string,object> dictionary, string space = "\t")
		{
			string output = "";

			foreach(KeyValuePair<string,object> entry in dictionary)
			{
				output += string.Format("{0}{1}", space, entry.Key);
				if (entry.Value is Dictionary<string, object>)
					output += string.Format("\t{0}\n{1}", ParseObjectType(entry.Value), DictionaryPrint((Dictionary<string, object>)entry.Value, space + "\t"));
				else if (entry.Value is List<object>)
					output += "\n" + ListPrint((List<object>)entry.Value, space + "  ");
				else
					output += string.Format(" :\t{0,-5}\t{1,5}\n", entry.Value, ParseObjectType(entry.Value));
			}

			return output;
		}

		private static string ListPrint(List<object> list, string space = "")
		{
			string output = "";

			foreach (object entry in list)
			{
				if (entry is List<object>)
					output += ListPrint((List<object>)entry, space + "  ");
				else if (entry is Dictionary<string, object>)
					output += DictionaryPrint((Dictionary<string, object>)entry, space + "  ");
				else
					output += entry + "\n";
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

			if (value is Int64)
				type = "(Int)";
			else if (value is bool)
				type = "(Bool)";
			else if (value is Array)
				type = "(Array)";
			else if (value is byte)
				type = "(Byte)";
			else if (value is float)
				type = "(Float)";
			else if (value is string)
				type = "(String)";
			else if (value is IDictionary)
				type = "(Dictionary)";

			string output = "<color=white>" + type + "</color>";
			return output;
		}
	}
}
