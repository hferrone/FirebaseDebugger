using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMapClass : GenericMappable
{
	public string email;
	public int score;
	public int exp;

	public override void Map(Dictionary<string, object> withDictionary)
	{
		if (withDictionary.ContainsKey("email"))
		{
			email = withDictionary["email"].ToString();
			score = Convert.ToInt32(withDictionary["score"]);
			exp = Convert.ToInt32(withDictionary["exp"]);
		}
	}
}
