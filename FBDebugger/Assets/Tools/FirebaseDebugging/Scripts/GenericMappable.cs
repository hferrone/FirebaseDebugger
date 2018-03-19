using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GenericMappable : ScriptableObject
{
	public abstract void Map(Dictionary<string, object> withDictionary);
}
