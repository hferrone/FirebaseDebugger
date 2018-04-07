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
		#region Public variables
        // Instance accessor
        public static FBDebuggerWindow instance;
		#endregion

		#region Private variables
        // Window size constants
        private const float leftMinWidth = 300;
        private const float leftMaxWidth = 350;
        private const float rightMinWidth = 200;
        private const float rightMaxWidth = 300;
        private const float maxHeight = 300;

		// Scroll view position
		private Vector2 scrollPos;

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
            instance.minSize = new Vector2(800, 400);
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

            // Options and mapping area
            EditorGUILayout.BeginVertical("box", GUILayout.MinWidth(rightMinWidth), GUILayout.MaxWidth(rightMaxWidth));
            DrawFirebaseInfoGUI();
            GUILayout.Space(10);

			DrawOptionsGUI();
			GUILayout.Space(10);
			DrawDataMappingGUI();
			GUILayout.Space(10);

			DrawSavingGUI();
				
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

			scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.MaxHeight(instance.maxSize.y - (2 * EditorGUIUtility.singleLineHeight)));
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.TextArea(_dataString, _textArea);
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndScrollView();

			// Start Query/Clear button group
            EditorGUILayout.BeginHorizontal();

			// Set Query button action
            bool buttonQuery = GUILayout.Button("Query", GUILayout.Height(2 * EditorGUIUtility.singleLineHeight));
            if (buttonQuery)
            {
				// Construct reference with child nodes and fetch new data
				FBDataService.instance.LoadData();
            }

			// Set Clear button action
            bool buttonClear = GUILayout.Button("Clear", GUILayout.Height(2 * EditorGUIUtility.singleLineHeight));
            if (buttonClear)
            {
				ClearData();
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
			EditorGUILayout.LabelField("References", _subStyle);
			EditorGUILayout.LabelField("Base", FBDataService.plist["DATABASE_URL"].ToString());
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
				GenericMappable customClass = (GenericMappable)ScriptableObject.CreateInstance(_destinationClass.GetClass());
				customClass.ParseKeys(_dataSnapshot);
				customClass.Map(_dataSnapshot);
				Editor.CreateEditor(customClass).OnInspectorGUI();
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
			EditorGUILayout.PropertyField(_serializedSortOption);

			EditorGUI.BeginDisabledGroup(_serializedSortOption.enumValueIndex != (int)SortOption.Child);
			EditorGUILayout.PropertyField(_serializedSortValue);
			EditorGUI.EndDisabledGroup();

			_serializedTarget.ApplyModifiedProperties();
			EditorGUILayout.EndVertical();

			// Filtering section
			EditorGUILayout.BeginHorizontal("box");
			EditorGUILayout.PropertyField(_serializedFilterOption);
			EditorGUILayout.PropertyField(_serializedFilterValue);
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
			bool buttonReset = GUILayout.Button("Hard Reset", GUILayout.Height(2 * EditorGUIUtility.singleLineHeight));
			if (buttonReset)
			{
				bool reset = EditorUtility.DisplayDialog("Wait!", "Are you sure you want to reset all the editor window data?", "Go for it!", "Nope");
				if (reset)
					HardReset();
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
		/// Validates sorting and filtering options to ensure max 1 is selected from each group.
		/// </summary>
		/// <param name="options">String array of option fields to validate.</param>
		private void ValidateOptions(string[] options)
		{

		}

		/// <summary>
		/// Updates serialized objects in the Editor Window.
		/// </summary>
		private void UpdateSerializedObjects()
		{
//			_childNodes = _currentConfig.childNodes;
//			_destinationClass = _currentConfig.destinationClass;
//			SerializedObjects();
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


		void ShowArrayGUI (SerializedProperty property) {
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
