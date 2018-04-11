using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FBDebugger 
{
	/////-------------------------------------------------------------------------------------------------------/////
	//////////////////////////////////<-----------Global Constants----------->///////////////////////////////////////
	/////-------------------------------------------------------------------------------------------------------/////
	
	/// <summary>
	/// Holds nested structures for each constant variable type.
	/// </summary>
	public struct Constants 
	{
		// iOS property list keys
		public struct PlistKeys
		{
			public const string databaseURL = "DATABASE_URL";
		}

		// GameObject names
		public struct ObjectNames
		{
			public const string ds = "Firebase Manager";
		}

		// Editor Window size constraints
		public struct Window 
		{
			public const float rightMaxWidth = 300;
		}
	}
}
