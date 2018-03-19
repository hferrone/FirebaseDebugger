using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TestMapClass : GenericMappable
{
	public List<string> uids = new List<string>();
	public string test = "TEST";

	public override void Map(Dictionary<string, object> withDictionary)
	{
		foreach(KeyValuePair<string, object> entry in withDictionary)
		{
			uids.Add(entry.Key);
		}
	}
}
