using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMapClass : GenericMappable
{
	public string email;
	public int score;
	public int level;

	public override void Map(Dictionary<string, object> withDictionary)
	{
		if (withDictionary.ContainsKey("email"))
		{
			email = withDictionary["email"].ToString();
			score = Convert.ToInt32(withDictionary["score"]);
			level = Convert.ToInt32(withDictionary["level"]);
		}
	}
}
