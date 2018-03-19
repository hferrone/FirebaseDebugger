using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Interface for any class that wants to map data snapshot info to its values.
/// </summary>
public interface IMappable
{
	void Map(Dictionary<string, object> withDictionary);
}
