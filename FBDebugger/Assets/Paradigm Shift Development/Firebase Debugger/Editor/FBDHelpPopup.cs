using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace  FBDebugger 
{
	public class FBDHelpPopup : PopupWindowContent 
	{
		public override Vector2 GetWindowSize()
		{
			return new Vector2(400, 200);
		}

		public override void OnGUI(Rect rect)
		{
			GUILayout.Label("Test", EditorStyles.boldLabel);
			Rect yourLabelRect = new Rect();
			if (Event.current.type == EventType.MouseUp && yourLabelRect.Contains(Event.current.mousePosition))
				Application.OpenURL("https://github.com/hferrone/FirebaseDebugger");
			
			GUI.Label(yourLabelRect, "GitHub");
		}
	}
}
