# FirebaseDebugger
A user-friendly Unity Editor Window for debugging a Firebase Realtime Database project.


The idea for this tool started after my third or fourth Unity + Firebase project. I'd been getting frustrated with having to write long debug logs every time I wanted to check interactions with Firebase's Realtime Database. So in short, I decided to learn about Unity Editor Extensions, which I had wanted to do for a long time, and scratch my own usability itch in the process.

## Getting Started

Alright, let's get up and running.

### Prerequisites

* <b>FirebaseDatabase.unitypackage</b> from the <b>Firebase SDK</b>
* <b>GoogleService-Info.plist</b> file somewhere in the project
* A working internet connection

If you haven't already done this, here is a link to the general Unity [setup](https://firebase.google.com/docs/unity/setup) and the Realtime Database [instructions](https://firebase.google.com/docs/database/unity/start). 

### Installing

You can download <b>FirebaseDebugger</b> from the <b>Unity Asset Store</b> [here](https://assetstore.unity.com/).

### Setup

Almost all of the setup for this tool is done programmatically, so the only thing you're responsible for is going to <b>Tools > FBDebugger > Setup</b>

![Basic Setup](https://user-images.githubusercontent.com/8218795/37569699-6214c5be-2ae6-11e8-829e-99980a40eae7.png)

And assigning your <b>GoogleService-Info.plist</b> to the <b>Firebase Manager</b> GameObject that is created for you in the Inspector. 

![Setting GoogleService-Info.plist](https://user-images.githubusercontent.com/8218795/37569738-e6417508-2ae6-11e8-87f2-3b411d657372.png)

The setup menu item will only be active if Unity is <b>NOT</b> in Play Mode, and the setup hasn't already been completed. I've done my best to create a sort of setup 'happy-path', so if you try and do anything out of order or in the wrong editor mode don't worry, there will be message dialogs to guide you back on track.

![Editor Display Messages](https://user-images.githubusercontent.com/8218795/37569700-6860128e-2ae6-11e8-8c2f-42846a138c79.png)

I wanted to make this as intuitive and easy as possible, so that's really all the setup there is.

### Help

If you need some tips, can't figure something out, or just want to send us some feedback about the tool, just use the big <b>Help</b> button in the bottom right corner of the Editor Window.

You can send any constructive criticism, requests, or really anything not filled with vitriol to <b>hferrone@paradigmshiftdev.com</b>

## Using the FBDebuggerWindow

After the setup is complete, you can open the debug window in Play Mode by hitting the <b>F key</b> OR <b>Tools > FBDebugger > Show Debugger</b>

The <b>Firebase Manager</b> GameObject is already set up with a Singleton script that handles all the Firebase functionality, so you don't have to worry about initializing the debugger if you switch scenes.

On startup the debugger will automatically display all data at the project root, and will close itself when you exit Play Mode to keep the Firebase SDK happy and your console error-free.

![root data](https://user-images.githubusercontent.com/8218795/40130378-22ebd8a8-5937-11e8-9fc0-0167917bacf0.png)

### Drilling into your data

Getting into your data is pretty simple. Enter the key, or nested keys, in the <b>Child Nodes</b> array and hit Query. This will construct a Database Reference and return all its data up to single key-value pairs. 

In the example screenshots below, I can access a single players data by entering my parent node <i>players</i> and a user key. 

![child data](https://user-images.githubusercontent.com/8218795/40130479-6d30d904-5937-11e8-9b9c-e6fa379242ee.png)

### Sorting and filtering

For this first release only one sorting and filtering option can be applied to a given reference. In the next releases this will be expanded to incorporate multiple filtering options to mirror what Firebase supports.

By default there are no options selected, but you can use the dropdowns to target specific data, and where applicable enter sorting/filtering values. In the following example, I've sorted by <i>exp</i> and limited the query to the first three entries.

![sort and filter](https://user-images.githubusercontent.com/8218795/40130564-bba47c6c-5937-11e8-982a-ea68c33652e1.png)

### Data snapshot mapping

The data mapping option will let you test if you're unpacking your snapshots correctly, and will also give you a handy list of all the snapshot keys by default. 

Any class that you want to map Firebase snapshots to will need to be derived from <b>GenericMappable</b>. 
<i>GenericMappable</i> is a simple abstract class that inherits from <b>ScriptableObject</b>.

```
public abstract class GenericMappable : ScriptableObject
{
	public List<string> Keys = new List<string>();
	public abstract void Map(Dictionary<string, object> withDictionary);

	public virtual void ParseKeys(Dictionary<string, object> withDictionary)
	{
		foreach(KeyValuePair<string, object> entry in withDictionary)
			Keys.Add(entry.Key);
	}
}
```

You will need to implement the <b>Map</b> method to comply with <b>GenericMappable</b>, which is where you'd put in any snapshot unpacking logic you're working with.

For the example below I've unpacked the <i>email</i>, <i>score</i>, and <i>exp</i> fields with the following code: 

```
public class TestMapClass : GenericMappable
{
	public string email;
	public int score;
	public int level;

	public override void Map(Dictionary<string, object> withDictionary)
	{
		if (withDictionary.ContainsKey("email"))
		{
			email = withDictionary["email"].ToString();
			score = Convert.ToInt32(withDictionary["score"]);
			level = Convert.ToInt32(withDictionary["exp"]);
		}
	}
}
```

With the result:

![data mapping](https://user-images.githubusercontent.com/8218795/40270131-317d5d08-5b88-11e8-9f7c-f08cdb1a37eb.png)

## Built With

* [Unity 2017](https://unity3d.com/)
* [Google Firebase -> Realtime Database](https://firebase.google.com/docs/database/unity/start)

## TODO

* Add in multiple filtering option functionality
* Add save/load/edit settings feature using Scriptable Objects
* Create custom editor for mapping class display
* Refactor debug area into [TreeView](https://docs.unity3d.com/Manual/TreeViewAPI.html)

## Contributing

Please read [CONTRIBUTING.md](https://gist.github.com/PurpleBooth/b24679402957c63ec426) for details on our code of conduct, and the process for submitting pull requests.

## Versioning

This project uses [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/your/project/tags). 

## Authors

* **Harrison Ferrone** ([hferrone](https://github.com/hferrone)) - *Initial planning, design, and implementation* 

You can also see the complete list of [contributors](https://github.com/your/project/contributors) who participated in this project.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* [Extending Unity with Editor Scripting](https://www.packtpub.com/game-development/extending-unity-editor-scripting) by Angelo Tadres
* [Property list parser](http://www.chrisdanielson.com/2011/05/09/using-apple-property-list-files-with-unity3d/) by Chris Danielson
