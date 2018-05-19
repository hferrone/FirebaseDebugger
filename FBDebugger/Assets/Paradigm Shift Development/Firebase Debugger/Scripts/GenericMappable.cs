using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GenericMappable : ScriptableObject
{
	public List<string> Keys = new List<string>();
	public abstract void Map(Dictionary<string, object> withDictionary);

	public virtual void ParseKeys(Dictionary<string, object> withDictionary)
	{
		foreach(KeyValuePair<string, object> entry in withDictionary)
			Keys.Add(entry.Key);
	}
}

