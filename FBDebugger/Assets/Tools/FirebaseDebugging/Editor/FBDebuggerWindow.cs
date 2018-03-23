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

		// Firebase reference nodes
		public string[] _childNodes = new string[0];
		#endregion

		#region Private variables
        // Window size constants
        private const float leftMinWidth = 300;
        private const float leftMaxWidth = 350;
        private const float rightMinWidth = 200;
        private const float rightMaxWidth = 300;
        private const float maxHeight = 400;

        // GUI styles
        private GUIStyle _mainStyle;
		private GUIStyle _subStyle;
		private GUIStyle _textArea;

		// Firebase formatted string
		private string _dataString = "No Data...";
		private Dictionary<string, object> _dataSnapshot;

		// Filtering and sorting options
		private bool _isSortingEnabled;
		private bool _isFilteringEnabled;
		private string _orderByChild;
		private string _orderByKey;
		private string _orderByValue;
		private int _limitToFirst;
		private int _limitToLast;
		private int _startAt;
		private int _endAt;
		private int _equalTo;

		// Data mapping variables
		private MonoScript _destinationClass;

		// Configurations
		//private FBDConfiguration _currentConfig;

        // Serialized objects
		private SerializedObject _serializedTarget;
		private SerializedProperty _serializedChildNodes;
		#endregion

        /// <summary>
		/// Editor instance constructor fired from FBDMenuItems.
        /// </summary>
        public static void ShowDebugger() 
		{
            instance = (FBDebuggerWindow)EditorWindow.GetWindow(typeof(FBDebuggerWindow));
            instance.titleContent = new GUIContent("FBDebugger");
            instance.minSize = new Vector2(200, 200);
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

			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.TextArea(_dataString, _textArea, GUILayout.MaxHeight(500));
			EditorGUI.EndDisabledGroup();

			// Start Query/Clear button group
            EditorGUILayout.BeginHorizontal();

			// Set Query button action
            bool buttonQuery = GUILayout.Button("Query", GUILayout.Height(2 * EditorGUIUtility.singleLineHeight));
            if (buttonQuery)
            {
				// Construct reference with child nodes and fetch new data
				FBDataService.instance.LoadData(_childNodes);
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

			EditorGUIUtility.labelWidth = 75;
			EditorGUILayout.LabelField("References", _subStyle);
			EditorGUILayout.LabelField("Base", FBDataService.plist["DATABASE_URL"].ToString());
			EditorGUIUtility.labelWidth = 0;

			GUILayout.Space(5);

			EditorGUILayout.LabelField("Nodes", _subStyle);

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(_serializedChildNodes, true);
			if (EditorGUI.EndChangeCheck())
				ClearData();

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

			// Expanding sorting section
			_isSortingEnabled = EditorGUILayout.Toggle("Show Sorting", _isSortingEnabled);
			if (_isSortingEnabled == true)
			{
				_orderByChild = EditorGUILayout.TextField("Order by child", _orderByChild);
				_orderByKey = EditorGUILayout.TextField("Order by key", _orderByKey);
				_orderByValue = EditorGUILayout.TextField("Order by value", _orderByValue);
			}

			// Expanding filtering section
			_isFilteringEnabled = EditorGUILayout.Toggle("Show Filtering", _isFilteringEnabled);
			if (_isFilteringEnabled == true)
			{
				_limitToFirst = EditorGUILayout.IntField("Limit to first", _limitToFirst);
			}
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
			ScriptableObject target = this;
			_serializedTarget = new SerializedObject(target);
			_serializedChildNodes = _serializedTarget.FindProperty("_childNodes");
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
			FBDataService.instance.LoadData(_childNodes);
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
			
			_childNodes = new string[0];
			_destinationClass = null;
			//_currentConfig = null;
			SerializedObjects();
			ClearData();
			FBDataService.instance.LoadData(_childNodes);
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
		#endregion
    }
}
