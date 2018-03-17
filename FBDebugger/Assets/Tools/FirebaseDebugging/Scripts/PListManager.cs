/**
 * 
 * Converted By: Chris Danielson of http://MonkeyPrism.com
 * Converted Date: May 8, 2011
 * Original JavaScript Auther: capnbishop
 * 
 * Conversion of a Unity 3D Community script from JavaScript to type controlled C# Mono.Net.
 * 
 * Please note, that the original object was named PropertyListSerializer.  I renamed it to prevent any conflicts with the original script.
 * 
 * The PropertyListSerializer.js (CD: renamed now to PListManager) script is used to load and save an XML property list file to and from a hierarchical hashtable 
 * (in which the root hashtable can contain child hashtables and arrays). This can provide a convenient and dynamic means of serializing 
 * a complex hierarchy of game data into XML files.
 * When loading, the resulting hashtable can include 8 different types of values: string, integer, real, date, data, boolean, dictionary, and array. 
 * Data elements are loaded as strings. Dictionaries are loaded as hashtables. Arrays are loaded as arrays. Each value is loaded with an 
 * associating key, except for elements of an array. Thus, each child hashtable and array also have associating keys, and can be combined to 
 * create a complex hierarchy of key value pairs and arrays.
 * When saving, the resulting XML file will contain the same hierarchy of data. All data will end up being stored as a string, but with an 
 * associated value type. Strings, integers, and decimals values are stored as such. Dates are stored in ISO 8601 format. Hashtables are stored 
 * as a plist key/value dictionary, and arrays as a series of keyless values.
 * The loader passes a lot of values by reference, and performs a considerable amount of recursion. Primitive values had to be passed by reference. 
 * Unity's JavaScript only passes objects by reference, and cannot explicitly pass a primitive by reference. As such, we've had to create a 
 * special ValueObject, which is just an abstract object that holds a single value. This object is then passed by reference, and the primitive 
 * value is set to its val property.
 * This plist loader conforms to Apple's plist DOCTYPE definition: http://www.apple.com/DTDs/PropertyList-1.0.dtd
 * 
 * Original JavaScript URL:  http://www.unifycommunity.com/wiki/index.php?title=PropertyListSerializer
 * 
 * Example Saving:
 
 		Hashtable playerData = new Hashtable();
		playerData.Add("Health",100);
		playerData.Add("TestObject", 1.5f);
		ArrayList guns = new ArrayList();
		guns.Add("AK-47");
		guns.Add("Pistol");
	    playerData.Add("Guns", guns);
		Hashtable grenades = new Hashtable();
		grenades.Add("FragmentationCount", 1);
		grenades.Add("IncendiaryCount", 1);
		playerData.Add("Grenades", grenades);
 
		//save outside the current project (same folder as Assets and Library)
		String xmlFile = Application.dataPath + "/../ExampleSaveFile.plist"; 
		PListManager.SavePlistToFile(xmlFile, playerData);
 * 
 */
using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Text;
using System.Globalization;
using UnityEngine;

public class PListManager {

	public PListManager() { }

	private const string SUPPORTED_VERSION = "1.0";

	public static bool ParsePListFile(string xmlFile, ref Hashtable plist) {                       
		if (!File.Exists(xmlFile)) { 
			Debug.LogError("File doesn't exist: " + xmlFile); 
			return false; 
		}

		StreamReader sr = new StreamReader(xmlFile);
		string txt = sr.ReadToEnd();
		sr.Close();

		XmlDocument xml = new XmlDocument();
		xml.XmlResolver = null; //Disable schema/DTD validation, it's not implemented for Unity.
		xml.LoadXml(txt);

		XmlNode plistNode = xml.LastChild;
		if (!plistNode.Name.Equals("plist")) { 
			Debug.LogError("plist file missing <plist> nodes." + xmlFile);
			return false;
		}

		string plistVers = plistNode.Attributes["version"].Value;
		if (plistVers == null || !plistVers.Equals(SUPPORTED_VERSION)) { 
			Debug.LogError("This is an unsupported plist version: " + plistVers + ". Required version:a " + SUPPORTED_VERSION); 
			return false;
		}

		XmlNode dictNode = plistNode.FirstChild;
		if (!dictNode.Name.Equals("dict")) { 
			Debug.LogError("Missing root dict from plist file: " + xmlFile); 
			return false; 
		}

		return LoadDictFromPlistNode(dictNode, ref plist);
	}


	#region LOAD_PLIST_PRIVATE_METHODS
	private static bool LoadDictFromPlistNode(XmlNode node, ref Hashtable dict) {
		if (node == null) { 
			Debug.LogError("Attempted to load a null plist dict node.");
			return false;
		}
		if (!node.Name.Equals("dict")) { 
			Debug.LogError("Attempted to load an dict from a non-array node type: " + node + ", " + node.Name); 
			return false;
		}
		if (dict == null) { 
			dict = new Hashtable();
		}

		int cnodeCount = node.ChildNodes.Count;
		for (int i = 0; i+1 < cnodeCount; i+=2) {
			// Select the key and value child nodes
			XmlNode keynode = node.ChildNodes.Item(i);
			XmlNode valuenode = node.ChildNodes.Item(i+1);

			// If this node isn't a 'key'
			if (keynode.Name.Equals("key")) {
				// Establish our variables to hold the key and value.
				string key = keynode.InnerText;
				ValueObject value = new ValueObject();

				// Load the value node.
				// If the value node loaded successfully, add the key/value pair to the dict hashtable.
				if (LoadValueFromPlistNode(valuenode, ref value)) {
					// This could be one of several different possible data types, including another dict.
					// AddKeyValueToDict() handles this by replacing existing key values that overlap, and doing so recursively for dict values.
					// If this not successful, post a message stating so and return false.
					if (!AddKeyValueToDict(ref dict, key, value)) {
						Debug.LogError("Failed to add key value to dict when loading plist from dict"); 
						return false;
					}
				} else { 
					Debug.LogError("Did not load plist value correctly for key in node: " + key + ", " + node);
					return false;
				}
			} else { 
				Debug.LogError("The plist being loaded may be corrupt.");
				return false;
			}

		} //end for

		return true;
	}

	private static bool LoadValueFromPlistNode(XmlNode node, ref ValueObject value) {
		if (node == null) { 
			Debug.LogError("Attempted to load a null plist value node."); 
			return false;
		}
		if (node.Name.Equals("string")) { value.val = node.InnerText; }
		else if (node.Name.Equals("integer")) { value.val = int.Parse(node.InnerText); }
		else if (node.Name.Equals("real")) { value.val = float.Parse(node.InnerText); }
		else if (node.Name.Equals("date")) { value.val = DateTime.Parse(node.InnerText, null, DateTimeStyles.None); } // Date objects are in ISO 8601 format
		else if (node.Name.Equals("data")) { value.val = node.InnerText; } // Data objects are just loaded as a string
		else if (node.Name.Equals("true")) { value.val = true; } // Boollean values are empty objects, simply identified with a name being "true" or "false"
		else if (node.Name.Equals("false")) { value.val = false; }
		// The value can be an array or dict type.  In this case, we need to recursively call the appropriate loader functions for dict and arrays.
		// These functions will in turn return a boolean value for their success, so we can just return that.
		// The val value also has to be instantiated, since it's being passed by reference.
		else if (node.Name.Equals("dict")) { 
			value.val = new Hashtable();
			Hashtable htRef = (Hashtable)value.val;
			return LoadDictFromPlistNode(node, ref htRef);
		}
		else if (node.Name.Equals("array")) {
			value.val = new ArrayList();
			ArrayList alRef = (ArrayList)value.val;
			return LoadArrayFromPlistNode(node, ref alRef);
		} else { 
			Debug.LogError("Attempted to load a value from a non value type node: " + node + ", " + node.Name);
			return false;
		}

		return true;
	}

	private static bool LoadArrayFromPlistNode(XmlNode node, ref ArrayList array ) {
		// If we were passed a null node object, then post an error stating so and return false
		if (node == null) { 
			Debug.LogError("Attempted to load a null plist array node.");
			return false;
		}
		// If we were passed a non array node, then post an error stating so and return false
		if (!node.Name.Equals("array")) { 
			Debug.LogError("Attempted to load an array from a non-array node type: " + node + ", " + node.Name); 
			return false;
		}

		// We can be passed an empty array object.  If so, initialize it
		if (array == null) { array = new ArrayList(); }

		// Itterate through the child nodes for this array object
		int nodeCount = node.ChildNodes.Count;
		for (int i = 0; i < nodeCount; i++) {
			// Establish variables to hold the child node of the array, and it's value
			XmlNode cnode = node.ChildNodes.Item(i);
			ValueObject element = new ValueObject();
			// Attempt to load the value from the current array node.
			// If successful, add it as an element of the array.  If not, post and error stating so and return false.
			if (LoadValueFromPlistNode(cnode, ref element)) { 
				array.Add(element.val); 
			} else { 
				return false; 
			}
		}

		// If we made it through the array without errors, return true
		return true;
	}

	private static bool AddKeyValueToDict(ref Hashtable dict, string key, ValueObject value) {
		// Make sure that we have values that we can work with.
		if (dict == null || key == null || key.Length < 1 || value == null) { 
			Debug.LogError("Attempted to AddKeyValueToDict() with null objects.");
			return false;
		}
		// If the hashtable doesn't already contain the key, they we can just go ahead and add it.
		if (!dict.ContainsKey(key)) { 
			dict.Add(key, value.val);
			return true;
		}
		// At this point, the dict contains already contains the key we're trying to add.
		// If the value for this key is of a different type between the dict and the new value, then we have a type mismatch.
		// Post an error stating so, but go ahead and overwrite the existing key value.
		if (value.val.GetType() != dict[key].GetType()) {
			Debug.LogWarning("Value type mismatch for overlapping key (will replace old value with new one): " + value.val + ", " + dict[key] + ", " + key);
			dict[key] = value.val;
		}
		// If the value for this key is a hashtable, then we need to recursively add the key values of each hashtable.
		else if (value.val.GetType() == typeof(Hashtable)) {
			// Itterate through the elements of the value's hashtable.
			Hashtable htTmp = (Hashtable)value.val;
			foreach (object element in htTmp) {
				// Recursively attempt to add/repalce the elements of the value hashtable to the dict's value hashtable.
				// If this fails, post a message stating so and return false.
				Hashtable htRef = (Hashtable)dict[key];
				if (!AddKeyValueToDict(ref htRef, (string)element, new ValueObject(htTmp[element]))) {
					Debug.LogError("Failed to add key value to dict: " + element + ", " + htTmp[element] + ", " + dict[key]);
					return false;
				}
			}
		}
		// If the value is an array, then there's really no way we can tell which elements to overwrite, because this is done based on the congruent keys.
		// Thus, we'll just add the elements of the array to the existing array.
		else if (value.val.GetType() == typeof(ArrayList)) {
			ArrayList alTmp = (ArrayList)value.val;
			ArrayList alAddTmp = (ArrayList)dict[key];
			foreach (object element in alTmp) {
				alAddTmp.Add(element);
			}
		}
		// If the key value is not an array or a hashtable, then it's a primitive value that we can easily write over.
		else { 
			dict[key] = value.val;
		}

		return true;
	}
	#endregion


	public static bool SavePlistToFile (String xmlFile, Hashtable plist) {
		// If the hashtable is null, then there's apparently an issue; fail out.
		if (plist == null) { 
			Debug.LogError("Passed a null plist hashtable to SavePlistToFile.");
			return false;
		}

		// Create the base xml document that we will use to write the data
		XmlDocument xml = new XmlDocument();
		xml.XmlResolver = null; //Disable schema/DTD validation, it's not implemented for Unity.
		// Create the root XML declaration
		// This, and the DOCTYPE, below, are standard parts of a XML property list file
		XmlDeclaration xmldecl = xml.CreateXmlDeclaration("1.0", "UTF-8", null);
		xml.PrependChild(xmldecl);

		// Create the DOCTYPE
		XmlDocumentType doctype = xml.CreateDocumentType("plist", "-//Apple//DTD PLIST 1.0//EN", "http://www.apple.com/DTDs/PropertyList-1.0.dtd", null);
		xml.AppendChild(doctype);

		// Create the root plist node, with a version number attribute.
		// Every plist file has this as the root element.  We're using version 1.0 of the plist scheme
		XmlNode plistNode = xml.CreateNode(XmlNodeType.Element, "plist", null);
		XmlAttribute plistVers = (XmlAttribute)xml.CreateNode(XmlNodeType.Attribute, "version", null);
		plistVers.Value = "1.0";
		plistNode.Attributes.Append(plistVers);
		xml.AppendChild(plistNode);

		// Now that we've created the base for the XML file, we can add all of our information to it.
		// Pass the plist data and the root dict node to SaveDictToPlistNode, which will write the plist data to the dict node.
		// This function will itterate through the hashtable hierarchy and call itself recursively for child hashtables.
		if (!SaveDictToPlistNode(plistNode, plist)) {
			// If for some reason we failed, post an error and return false.
			Debug.LogError("Failed to save plist data to root dict node: " + plist);
			return false;
		} else { // We were successful
			// Create a StreamWriter and write the XML file to disk.
			// (do not append and UTF-8 are default, but we're defining it explicitly just in case)
			StreamWriter sw = new StreamWriter(xmlFile, false, System.Text.Encoding.UTF8);
			xml.Save(sw);
			sw.Close();
		}

		// We're done here.  If there were any failures, they would have returned false.
		// Return true to indicate success.
		return true;
	}

	#region SAVE_PLIST_PRIVATE_METHODS

	private static bool SaveDictToPlistNode(XmlNode node, Hashtable dict) {
		// If we were passed a null object, return false
		if (node == null) {
			Debug.LogError("Attempted to save a null plist dict node.");
			return false;
		}

		XmlNode dictNode = node.OwnerDocument.CreateNode(XmlNodeType.Element, "dict", null);
		node.AppendChild(dictNode);

		// We could be passed an null hashtable.  This isn't necessarily an error.
		if (dict == null) { 
			Debug.LogWarning("Attemped to save a null dict: " + dict); 
			return true;
		}

		// Iterate through the keys in the hashtable
		//for (var key in dict.Keys) {
		foreach (object key in dict.Keys) {
			// Since plists are key value pairs, save the key to the plist as a new XML element
			XmlElement keyNode = node.OwnerDocument.CreateElement("key");
			keyNode.InnerText = (string)key;
			dictNode.AppendChild(keyNode);

			// The name of the value element is based on the datatype of the value.  We need to serialize it accordingly.  Pass the XML node and the hash value to SaveValueToPlistNode to handle this.
			if (!SaveValueToPlistNode(dictNode, dict[key])) {
				// If SaveValueToPlistNode() returns false, that means there was an error.  Return false to indicate this up the line.
				Debug.LogError("Failed to save value to plist node: " + key);
				return false;
			}
		}

		// If we got this far then all is well.  Return true to indicate success.
		return true;
	}

	private static bool SaveValueToPlistNode(XmlNode node, object value) {
		// The node passed will be the parent node to the new value node.
		XmlNode valNode;
		System.Type type = value.GetType();
		// Identify the data type for the value and serialize it accordingly
		if (type == typeof(String)) { 
			valNode = node.OwnerDocument.CreateElement("string"); 
		}
		else if (type == typeof(Int16) || 
			type == typeof(Int32) ||
			type == typeof(Int64)) { valNode = node.OwnerDocument.CreateElement("integer"); }
		else if (type == typeof(Single) || 
			type == typeof(Double) ||
			type == typeof(Decimal)) { valNode = node.OwnerDocument.CreateElement("real"); }
		else if (type == typeof(DateTime)) {
			// Dates need to be stored in ISO 8601 format
			valNode = node.OwnerDocument.CreateElement("date");
			DateTime dt = (DateTime)value;
			valNode.InnerText = dt.ToUniversalTime().ToString("o");
			node.AppendChild(valNode);
			return true;
		}
		else if (type == typeof(bool)) {
			// Boolean values are empty elements, simply being stored as an elemement with a name of true or false
			if ((bool)value == true) { valNode = node.OwnerDocument.CreateElement("true"); }
			else { valNode = node.OwnerDocument.CreateElement("false"); }
			node.AppendChild(valNode);
			return true;
		}
		// Hashtables and arrays require special functions to save their values in an itterative and recursive manner.
		// The functions will return true/false to indicate success/failure, so pass those on.
		else if (type == typeof(Hashtable))    { 
			return SaveDictToPlistNode(node, (Hashtable)value); 
		}
		else if (type == typeof(ArrayList)) { return SaveArrayToPlistNode(node, (ArrayList)value); }
		// Anything that doesn't fit the defined data types will just be stored as "data", which is effectively a string.
		else { 
			valNode = node.OwnerDocument.CreateElement("data");
		}

		// Some of the values (strings, numbers, data) basically get stored as a string.  The rest will store their values in their special format and return true for success.  If we made it this far, then the value in valNode must be stored as a string.
		if (valNode != null) valNode.InnerText = value.ToString();
		node.AppendChild(valNode);

		// We're done.  Return true for success.
		return true;
	}

	private static bool SaveArrayToPlistNode (XmlNode node, ArrayList array) {
		// Create the value node as an "array" element.
		XmlElement arrayNode = node.OwnerDocument.CreateElement("array");
		node.AppendChild(arrayNode);

		// Each element in the array can be any data type.  Itterate through the array and send each element to SaveValueToPlistNode(), where it can be stored accordingly based on its data type.
		foreach (object element in array) {
			// If SaveValueToPlistNode() returns false, then there was a problem.  Return false in that case.
			if (!SaveValueToPlistNode(arrayNode, element)) { return false; }
		}
		return true;
	}

	#endregion

} //end PListManager class

class ValueObject { 
	public object val;
	public ValueObject() {}
	public ValueObject(object aVal) { val = aVal; }
}
