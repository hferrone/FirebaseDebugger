using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

namespace FBDebugger 
{
	/////-------------------------------------------------------------------------------------------------------/////
	////////////////////////////////<-----------Debug Editor Window----------->//////////////////////////////////////
	/////-------------------------------------------------------------------------------------------------------/////

	/// <summary>
	/// Custom debug editor window.
	///  - Handles user input
	///  - Coordinates with the Firebase Manager (data service)
	///  - Main data service subscriber
	/// </summary>
    public class FBDebuggerWindow : EditorWindow 
	{
        // Instance accessor
        public static FBDebuggerWindow instance;

		#region Private variables
		// Scroll view positions
		private Vector2 debugScrollPos;
		private Vector2 infoScrollPos;

        // GUI styles
        private GUIStyle _mainStyle;
		private GUIStyle _subStyle;
		private GUIStyle _textArea;

		// Firebase data variables
		private string _dataString = "No Data...";
		private Dictionary<string, object> _dataSnapshot;

		// Data mapping variables
		private MonoScript _destinationClass;

		// Configurations
		//private FBDConfiguration _currentConfig;

        // Serialized objects
		private SerializedObject _serializedTarget;
		private SerializedProperty _serializedChildNodes;
		private SerializedProperty _serializedSortOption;
		private SerializedProperty _serializedSortValue;
		private SerializedProperty _serializedFilterOption;
		private SerializedProperty _serializedFilterValue;
		#endregion

        /// <summary>
		/// Editor instance constructor fired from FBDMenuItems.
        /// </summary>
        public static void ShowDebugger() 
		{
            instance = (FBDebuggerWindow)EditorWindow.GetWindow(typeof(FBDebuggerWindow));
            instance.titleContent = new GUIContent("FBDebugger");
            instance.minSize = new Vector2(800, 390);
        }

		void Update()
		{
			if (!Application.isPlaying)
				instance.Close();
		}

        #region GUI area drawing
		private void OnGUI() 
		{
			DrawSplitViewLayoutGUI();
		}

		/// <summary>
		/// Draws the main window GUI.
		///  - Each section has its own drawing function
		/// </summary>
        private void DrawSplitViewLayoutGUI() {

            // Main layout area
            EditorGUILayout.BeginHorizontal();

            // Debug area
            EditorGUILayout.BeginVertical("box");
            DrawDebugGUI();
            EditorGUILayout.EndVertical();

            // Scroll view for entire right-hand side of editor window
			EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(Constants.Window.rightMaxWidth));
			infoScrollPos = EditorGUILayout.BeginScrollView(infoScrollPos, GUILayout.MaxHeight(instance.maxSize.y - (2 * EditorGUIUtility.singleLineHeight)));

			// Firebase info area
            DrawFirebaseInfoGUI();
            GUILayout.Space(10);

			// Sorting/filtering area
			DrawOptionsGUI();
			GUILayout.Space(10);
			DrawDataMappingGUI();
			GUILayout.Space(10);

			EditorGUILayout.EndScrollView();

			// Saving/reset/help area
			EditorGUILayout.BeginHorizontal();
			DrawSavingGUI();
			EditorGUILayout.EndVertical();
				
            EditorGUILayout.EndVertical();

			// End main area layout
            EditorGUILayout.EndHorizontal();

            Repaint();
        }

		/// <summary>
		/// Draws the debug area GUI.
		///  - Displays Firebase data
		///  - Handles query and clear button actions
		/// </summary>
        private void DrawDebugGUI() 
		{
            EditorGUILayout.LabelField("Debug Console", _mainStyle);

			debugScrollPos = EditorGUILayout.BeginScrollView(debugScrollPos, GUILayout.MaxHeight(instance.maxSize.y - (2 * EditorGUIUtility.singleLineHeight)));
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.TextArea(_dataString, _textArea);
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndScrollView();

			// Start Query button group
            EditorGUILayout.BeginHorizontal();

			// Set Query button action
            bool buttonQuery = GUILayout.Button("Query", GUILayout.Height(2 * EditorGUIUtility.singleLineHeight));
            if (buttonQuery)
            {
				// Construct reference with child nodes and fetch new data
				FBDataService.instance.LoadData();
            }
		                
			// End Query/Clear button group
            EditorGUILayout.EndHorizontal();
        }

		/// <summary>
		/// Draws the Firebase info GUI.
		///  - Displays property list attributes and serialized node array
		/// </summary>
        private void DrawFirebaseInfoGUI() 
		{
            EditorGUILayout.LabelField("Firebase Info", _mainStyle);

			EditorGUIUtility.labelWidth = 70;
			EditorGUILayout.LabelField("Base", _subStyle);
			EditorGUILayout.LabelField(FBDataService.plist["DATABASE_URL"].ToString());
			EditorGUIUtility.labelWidth = 0;

			GUILayout.Space(5);

			EditorGUILayout.LabelField("Child Nodes", _subStyle);

			EditorGUI.BeginChangeCheck();
			EditorGUIUtility.labelWidth = 90;
			ShowArrayGUI(_serializedChildNodes);
			if (EditorGUI.EndChangeCheck())
				ClearData();

			EditorGUIUtility.labelWidth = 0;

			_serializedTarget.ApplyModifiedProperties();
        }

		/// <summary>
		/// Draws data mapping GUI.
		///  - Allows a user to drag/drop applicable class script to parse data into
		/// </summary>
        private void DrawDataMappingGUI() 
		{
            EditorGUILayout.LabelField("Mapping", _mainStyle);
            EditorGUILayout.LabelField("Destination Class");

			_destinationClass = (MonoScript)EditorGUILayout.ObjectField(_destinationClass, typeof(MonoScript), true);
			if (_destinationClass == null)
				EditorGUILayout.HelpBox("Please drag a class script that you would like the FBDataSnapshot mapped to.", MessageType.Info);

			if (_destinationClass != null)
			{
				Type classType = _destinationClass.GetClass();
				if (classType.IsSubclassOf(typeof(GenericMappable)))
				{
					GenericMappable customClass = (GenericMappable)ScriptableObject.CreateInstance(_destinationClass.GetClass());
					customClass.ParseKeys(_dataSnapshot);
					customClass.Map(_dataSnapshot);
					Editor.CreateEditor(customClass).OnInspectorGUI();
				}
				else
					EditorGUILayout.HelpBox("Please make sure that your destination class inherits from GenericMappable and implements the Map() function", MessageType.Warning);
			}
		}

		/// <summary>
		/// Draws the Firebase query options GUI.
		///  - Sorting and filtering sections expand separately to reduce layout clutter
		///  - Options are applied to data queries where applicable
		/// </summary>
        private void DrawOptionsGUI() 
		{
            EditorGUILayout.LabelField("Options", _mainStyle);

			GUILayout.Space(5);
			EditorGUIUtility.labelWidth = 70;

			// Sorting section
			EditorGUILayout.BeginHorizontal("box");
			EditorGUILayout.PropertyField(_serializedSortOption, GUILayout.Width(155));
			GUILayout.Space(10);

			EditorGUI.BeginDisabledGroup(_serializedSortOption.enumValueIndex != (int)SortOption.Child);
			if (_serializedSortOption.enumValueIndex != (int)SortOption.Child)
				_serializedSortValue.stringValue = "0";

			EditorGUILayout.PropertyField(_serializedSortValue, GUIContent.none);
			EditorGUI.EndDisabledGroup();

			_serializedTarget.ApplyModifiedProperties();
			EditorGUILayout.EndVertical();

			// Filtering section
			EditorGUILayout.BeginHorizontal("box");
			EditorGUILayout.PropertyField(_serializedFilterOption, GUILayout.Width(155));
			GUILayout.Space(10);

			EditorGUI.BeginDisabledGroup(_serializedFilterOption.enumValueIndex == (int)FilterOption.None);
			if (_serializedFilterOption.enumValueIndex == (int)FilterOption.None)
				_serializedFilterValue.intValue = 0;

			EditorGUILayout.PropertyField(_serializedFilterValue, GUIContent.none);
			EditorGUI.EndDisabledGroup();

			_serializedTarget.ApplyModifiedProperties();
			EditorGUILayout.EndVertical();

			EditorGUIUtility.labelWidth = 0;
        }

		/// <summary>
		/// Draws the configuration saving GUI.
		///  - Allows users to create and load configuration settings (as ScriptableObjects)
		/// </summary>
		private void DrawSavingGUI()
		{
//			EditorGUILayout.LabelField("Save/Load", _mainStyle);
//
//			EditorGUIUtility.labelWidth = 100;
//			_currentConfig = (FBDConfiguration)EditorGUILayout.ObjectField("Configuration", _currentConfig, typeof(FBDConfiguration), false);
//			EditorGUIUtility.labelWidth = 0;
//
//			if (_currentConfig != null)
//			{
//				Editor.CreateEditor(_currentConfig).OnInspectorGUI();
//				UpdateSerializedObjects();
//			}
//			else
//				EditorGUILayout.HelpBox("You can load any previously saved configurations.", MessageType.Info);
//
//			// Set Save button action
//			bool buttonSave = GUILayout.Button("Save current settings", GUILayout.Height(2 * EditorGUIUtility.singleLineHeight));
//			if (buttonSave)
//				FBDEditorUtils.SaveConfigurationAsset(_childNodes, _destinationClass);

			// Set Reset button action
			bool buttonReset = GUILayout.Button("Hard Reset", GUILayout.Height(2 * EditorGUIUtility.singleLineHeight), GUILayout.Width(200));
			if (buttonReset)
			{
				bool reset = EditorUtility.DisplayDialog("Wait!", "Are you sure you want to reset all the editor window data?", "Go for it!", "Nope");
				if (reset)
					HardReset();
			}

			// Set Help button action
			bool buttonHelp = GUILayout.Button("Help", GUILayout.Height(2 * EditorGUIUtility.singleLineHeight));
			if (buttonHelp)
			{
				bool github = EditorUtility.DisplayDialog("Need Some Help?", "All the information about this tool can be found on the project GitHub page.\n\nIf you want to get in touch or send us feedback, shoot an email to paradigmshiftdev@gmail.com.\n", "Go to GitHub", "Back");
				if (github)
					Application.OpenURL("https://github.com/hferrone/FirebaseDebugger");
			}
		}
        #endregion

		#region Initializations
		/// <summary>
		/// Loads and assigns all GUISkins from editor Resources folder
		/// </summary>
		private void InitStyles() 
		{
			GUISkin mainSkin = (GUISkin)Resources.Load("Debugger_Main");
			GUISkin subSkin = (GUISkin)Resources.Load("Debugger_Sub1");
			GUISkin textArea = (GUISkin)Resources.Load("Debugger_TextArea");

			_mainStyle = mainSkin.label;
			_subStyle = subSkin.label;
			_textArea = textArea.label;
		}

		/// <summary>
		/// Serializes the editor window and its properties for display
		/// </summary>
		private void SerializedObjects() 
		{
			_serializedTarget = new SerializedObject(FBDataService.instance);

			_serializedChildNodes = _serializedTarget.FindProperty("childNodes");
			_serializedSortOption = _serializedTarget.FindProperty("sortBy");
			_serializedSortValue = _serializedTarget.FindProperty("sortValue");
			_serializedFilterOption = _serializedTarget.FindProperty("filterBy");
			_serializedFilterValue = _serializedTarget.FindProperty("filterValue");

			_serializedTarget.ApplyModifiedProperties();
		}
		#endregion

        #region EditorWindow message methods
		/// <summary>
		/// Called when editor window is enabled.
		///  - Sets up styles, serialized objects, subscription events, and initial Firebase query
		/// </summary>
        private void OnEnable() 
		{
			Debug.Log("OnEnable was called...");

            InitStyles();
			SerializedObjects();
			SubscribeEvents();
			FBDataService.instance.LoadData();
        }

		/// <summary>
		/// Called when editor window is disabled.
		///  - Handles unsubscribing events
		/// </summary>
        private void OnDisable() 
		{
			Debug.Log("OnDisable was called...");
			UnsubscribeEvents();
        }

		/// <summary>
		/// Called when editor window is destroyed.
		///  - Handles unsubscribing events
		/// </summary>
        private void OnDestroy() 
		{
            Debug.Log("OnDestroy was called...");
			UnsubscribeEvents();
        }
        #endregion

		#region Data Service Delegates
		/// <summary>
		/// Subscribes to all available events from FBDataService
		///  - Maps received events to UpdateCurrentData() 
		/// </summary>
		private void SubscribeEvents() 
		{
			FBDataService.instance.ValueDataChanged += UpdateCurrentData;
		}

		/// <summary>
		/// Unsubscribes to all current event subscriptions.
		/// </summary>
		private void UnsubscribeEvents() 
		{
			FBDataService.instance.ValueDataChanged -= UpdateCurrentData;
		}

		/// <summary>
		/// Updates dataString text variable to display in debug area.
		/// </summary>
		/// <param name="formattedString">Formatted string passed through FBDataService event.</param>
		/// <param name="action">Type of event from Firebase (eg Value Changed, Child Added etc).</param>
		private void UpdateCurrentData(Dictionary<string, object> snapshotDict, string action) 
		{
			string dataString = snapshotDict != null ? ProcessData(snapshotDict) : "No endpoint found in the database...";
			_dataString = dataString;
			_dataSnapshot = snapshotDict;
			Repaint();
		}
		#endregion

		#region Utilities
		/// <summary>
		/// Clears the debug area data.
		/// </summary>
		private void ClearData()
		{
			// Reset data string
			_dataString = "No Data...";
			Debug.Log("Log cleared..." + _dataString);
		}

		/// <summary>
		/// Resets the entire Editor to initial state.
		/// </summary>
		private void HardReset()
		{
			
			FBDataService.instance.Reset();
			_destinationClass = null;
			ClearData();
		}

		/// <summary>
		/// Parses snapshot dictionary into formatted string.
		/// </summary>
		/// <returns>Formatted data string to display in FBDebuggerWindow (Editor Window).</returns>
		/// <param name="dict">Snapshot dictionary from Firebase.</param>
		private string ProcessData(Dictionary<string, object> dict)
		{
			return FBDEditorUtils.DictionaryPrint(dict);
		}
			
		/// <summary>
		/// Displays serialized array children to mimic editor array.
		/// </summary>
		/// <param name="property">Serialized property.</param>
		void ShowArrayGUI (SerializedProperty property) 
		{
			SerializedProperty arraySizeProp = property.FindPropertyRelative("Array.size");
			EditorGUILayout.PropertyField(arraySizeProp);

			EditorGUI.indentLevel ++;

			for (int i = 0; i < arraySizeProp.intValue; i++) {
				EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(i));
			}

			EditorGUI.indentLevel --;
		}
		#endregion
    }
}