using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FBDebugger 
{
	[Serializable]
	public class FBDConfiguration: ScriptableObject
	{
		public string[] childNodes = new string[0];
		public MonoScript destinationClass;

		public void Init(string[] nodes, MonoScript mapClass)
		{
			this.childNodes = nodes;
			this.destinationClass = mapClass;
		}

		public static FBDConfiguration CreateSOAsset(string[] nodes, MonoScript mapClass, string path)
		{
			var dataAsset = ScriptableObject.CreateInstance<FBDConfiguration>();
			dataAsset.Init(nodes, mapClass);

			AssetDatabase.CreateAsset(dataAsset, path);
			AssetDatabase.Refresh();
			AssetDatabase.SaveAssets();

			return dataAsset;
		}
	}
}
