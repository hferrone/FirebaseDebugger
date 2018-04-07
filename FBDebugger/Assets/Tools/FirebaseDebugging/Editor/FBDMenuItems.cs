using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FBDebugger 
{
	/////-------------------------------------------------------------------------------------------------------/////
	/////////////////////////////////<-----------Custom Menu Items----------->///////////////////////////////////////
	/////-------------------------------------------------------------------------------------------------------/////
    
	/// <summary>
	/// Manages all custom menu items.
	/// </summary>
    public class FBDMenuItems 
	{
		// Tracks setup state by existence of Firebase Manager GameObject in scene
		private static bool isSetupComplete
		{
			get
			{
				var debugger = GameObject.Find(Constants.ObjectNames.ds);
				if (debugger == null)
					return false;
				else
					return true;
			}
		}

		#region Menu items
		/// <summary>
		/// Sets up the debugger parent GameObject.
		///  - Active depending on ValidateSetup()
		/// </summary>
		[MenuItem("Tools/FBDebugger/Setup")]
		public static void SetupDebugger()
		{
			FBDEditorUtils.NewManagerInstance();
		}

		[MenuItem("Tools/FBDebugger/TestData")]
		public static void SetupTestData()
		{
			for (int i = 0; i < 5; i++)
				FBDEditorUtils.NewTestData();
		}

        /// <summary>
		/// Shows the editor window.
		///  - Works with F keyboard shortcut
		///  - Displays dialog if setup isn't complete or not in Play Mode
		///  - Active depending on ValidateShowDebugger()
        /// </summary>
        [MenuItem("Tools/FBDebugger/Show Debugger _f")]
        public static void ShowDebugger() 
		{     
			if (Application.isPlaying && !isSetupComplete)
				EditorUtility.DisplayDialog("FBDebugger", "You haven't completed the initial setup. Exit Play Mode in the editor and go to Tools > FBDebugger > Setup.", "Ok");
			else if (!Application.isPlaying && !isSetupComplete)
				EditorUtility.DisplayDialog("FBDebugger", "You haven't set up the debugger yet. Go to Tools > FBDebugger > Setup.", "Ok");
			else if (!Application.isPlaying && isSetupComplete)
				EditorUtility.DisplayDialog("FBDebugger", "You need to be in Play mode to use the FBDebugger editor", "Ok");
			else
				FBDebuggerWindow.ShowDebugger();
        }

        /// <summary>
        /// Shows the help window.
		///  - Works with H keyboard shortcut
		///  - Active depending on ValidateHelp()
        /// </summary>
        [MenuItem("Tools/FBDebugger/Help _h")]
        public static void ShowHelp() 
		{
			//TODO: Implement help page
        }

		#endregion 

		#region Menu validation
		/// <summary>
		/// Manages Setup item state in Tools > FBDebugger menu
		///  - Application can't be in Play Mode for setup
		/// </summary>
		/// <returns><c>true</c>, if setup was validated, <c>false</c> otherwise.</returns>
		[MenuItem("Tools/FBDebugger/Setup", true)]
		static bool ValidateSetup()
		{
			if (!Application.isPlaying && !isSetupComplete)
				return true;
			else
				return false;
		}
		#endregion
    }

}
