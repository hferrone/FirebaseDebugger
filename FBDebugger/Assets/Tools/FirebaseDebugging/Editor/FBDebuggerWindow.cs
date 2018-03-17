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

		// Firebase formatted string
		private string _dataString = "No Data...";

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
        private Object _destinationClass;

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
			EditorGUILayout.TextArea(_dataString, GUILayout.MaxHeight(500));
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
				// Reset data string
				_dataString = "No Data...";
				Debug.Log("Log cleared..." + _dataString);
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
			EditorGUILayout.PropertyField(_serializedChildNodes, true);
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

            _destinationClass = EditorGUILayout.ObjectField(_destinationClass, typeof(Object), true);
            if (_destinationClass == null)
                EditorGUILayout.HelpBox("Please drag a class script that you would like the FBDataSnapshot mapped to.", MessageType.Info);
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
        #endregion

		#region Initializations
		/// <summary>
		/// Loads and assigns all GUISkins from editor Resources folder
		/// </summary>
		private void InitStyles() 
		{
			GUISkin mainSkin = (GUISkin)Resources.Load("Debugger_Main");
			GUISkin subSkin = (GUISkin)Resources.Load("Debugger_Sub1");

			_mainStyle = mainSkin.label;
			_subStyle = subSkin.label;
		}

		/// <summary>
		/// Serializes the editor window and its properties for display
		/// </summary>
		private void InitSerializedObjects() 
		{
			ScriptableObject target = this;
			_serializedTarget = new SerializedObject(target);
			_serializedChildNodes = _serializedTarget.FindProperty("_childNodes");
		}
		#endregion

        #region EditorWindow message methods
		/// <summary>
		/// Called when editor window is enabled.
		///  - Sets up styles, serialized objects, subscription events, and initial Firebase query
		/// </summary>
        private void OnEnable() 
		{
            InitStyles();
			InitSerializedObjects();
			SubscribeEvents();
			FBDataService.instance.LoadData(_childNodes);

			Debug.Log("OnEnable was called...");
        }

		/// <summary>
		/// Called when editor window is disabled.
		///  - Handles unsubscribing events
		/// </summary>
        private void OnDisable() 
		{
			UnsubscribeEvents();
            Debug.Log("OnDisable was called...");
        }

		/// <summary>
		/// Called when editor window is destroyed.
		///  - Handles unsubscribing events
		/// </summary>
        private void OnDestroy() 
		{
			UnsubscribeEvents();
            Debug.Log("OnDestroy was called...");
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
		private void UpdateCurrentData(string formattedString, string action) 
		{
			_dataString = formattedString;
			Repaint();
		}
		#endregion
    }
}
