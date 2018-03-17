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
	}
}
