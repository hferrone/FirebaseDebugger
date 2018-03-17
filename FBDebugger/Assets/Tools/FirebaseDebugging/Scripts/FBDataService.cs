using System;
using System.Linq;
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
	///////////////////////////////<-----------Firebase Data Service----------->/////////////////////////////////////
	/////-------------------------------------------------------------------------------------------------------/////

	/// <summary>
	/// Data service class that encapsulates all functionality and interactions dealing with Google Firebase.
	///  - Singleton => uses DontDestroyOnLoad to keep Firebase instance alive through scene changes
	///  - Event driven => all data is emitted through events, other scripts are responsible for event subscriptions
	/// </summary>
	public class FBDataService : MonoBehaviour 
	{
		// Singleton instance 
		public static FBDataService instance = null;
			
		// Property list variables
		public UnityEngine.Object plistObject;
		public static Hashtable plist 
		{
			get
			{
				// Parses GoogleService-Info.plist file if possible.
				// Script Asset => http://www.chrisdanielson.com/2011/05/09/using-apple-property-list-files-with-unity3d/
				Hashtable parsedData = new Hashtable();
				string xmlFile = AssetDatabase.GetAssetPath(instance.plistObject);;
				PListManager.ParsePListFile(xmlFile, ref parsedData);

				return parsedData;
			}
		}

		// Event types and variables
		public delegate void NewDataAvailable(string formattedString, string action);
		public event NewDataAvailable ValueDataChanged;

		/// <summary>
		/// Initializes the singleton pattern and fires off Firebase init.
		/// </summary>
		void Awake()
		{
			if (instance && instance.GetInstanceID() != GetInstanceID())
				DestroyImmediate(gameObject);
			else
			{
				instance = this;
				DontDestroyOnLoad(gameObject);
			}

			InitializeFirebase();
		}

		/// <summary>
		/// Responsible for all Firebase setup.
		/// Not accessible from other scripts since it should only happen on Awake.
		/// </summary>
		private void InitializeFirebase() 
		{
			FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(plist[Constants.PlistKeys.databaseURL].ToString());
		}

		/// <summary>
		/// Public entry point for requesting new Firebase data.
		/// </summary>
		/// <param name="childNodes">Array of database child nodes. If empty, reference is defaulted to root Firebase project reference.</param>
		public void LoadData(string[] childNodes) 
		{
			DatabaseReference constructedRef = ConstructReference(childNodes);
			constructedRef.ValueChanged += HandleValueChange;

			Debug.Log("Database queried at " + constructedRef);
		}

		/// <summary>
		/// Subscribes to Firebase value change event on current reference.
		/// Every value change emits a new event with formatted data string to all subscribers.
		/// </summary>
		/// <param name="sender">Sender object.</param>
		/// <param name="eventArgs">Event argument that holds Firebase DataSnapshot.</param>
		private void HandleValueChange(object sender, ValueChangedEventArgs eventArgs) 
		{
			var snapDict = (Dictionary<string, object>)eventArgs.Snapshot.Value;
			string dataString = snapDict != null ? ProcessData(snapDict) : "No endpoint found in the database...";

			if (ValueDataChanged != null)
				ValueDataChanged(dataString, "Value Changed");
		}

		#region Utilities
		/// <summary>
		/// Constructs a Firebase DatabaseReference from child nodes parameters.
		/// </summary>
		/// <returns>A reference constructed from input child nodes from FBDebuggerWindow (Editor Window).</returns>
		/// <param name="childNodes">Child nodes.</param>
		private DatabaseReference ConstructReference(string[] childNodes) 
		{
			string childRef = string.Join("/", childNodes);
			return FirebaseDatabase.DefaultInstance.RootReference.Child(childRef);
		}

		/// <summary>
		/// Parses snapshot dictionary into formatted string.
		/// </summary>
		/// <returns>Formatted data string to display in FBDebuggerWindow (Editor Window).</returns>
		/// <param name="dict">Snapshot dictionary from Firebase.</param>
		private string ProcessData(Dictionary<string, object> dict) 
		{
			return dict.Select(x => x.Key + " : " + x.Value).Aggregate((s1, s2) => s1 + "\n" + s2);
		}
		#endregion
	}
}
