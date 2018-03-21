# FirebaseDebugger
*Official Copy:* 

A user-friendly Unity Editor Window for debugging Firebase Realtime Database functionality in Unity projects.

*The Story:* 

The idea for this tool started after my third or fourth Unity + Firebase project. I'd been getting frustrated with having to write long debug logs every time I wanted to check interactions with Firebase's Realtime Database. So in short, I decided to learn about Unity Editor Extensions, which I had wanted to do for a long time, and scratch my own usability itch in the process.

## Getting Started

Alright, let's get you up and running.

### Prerequisites

First off, you'll need to have Firebase integrated into your project. I'm assuming that since you're using this tool that's already the case, but [here is the general setup link](https://firebase.google.com/docs/unity/setup) and the [Realtime Database setup](https://firebase.google.com/docs/database/unity/start). 

Don't forget to put your GoogleService-Info.plist file into the project, as that's how the Editor Window establishes an automatic connection with your project.

### Installing

You can download FirebaseDebugger from the [Unity Asset Store](https://assetstore.unity.com/) (preferred method), or you can download the whole project from the repo and drag/drop the entire Tools folder into your project.

### Setup

All the setup for this tool is done programmatically, so the only thing you're responsible for is going to Tools > FBDebugger and clicking Setup. 


![Basic Setup](https://user-images.githubusercontent.com/8218795/37569699-6214c5be-2ae6-11e8-829e-99980a40eae7.png)


The setup menu item will only be active if Unity is NOT in Play Mode, and the setup hasn't already been completed. I've done my best to create a sort of setup 'happy-path', so if you try and do anything out of order or in the wrong editor mode don't worry, there will be message dialogs to guide you back on track.


![Editor Display Messages](https://user-images.githubusercontent.com/8218795/37569700-6860128e-2ae6-11e8-8c2f-42846a138c79.png)


After the setup is done, select the Firebase Manager game object and assign your GoogleService-Info.plist in the Inspector.


![Setting GoogleService-Info.plist](https://user-images.githubusercontent.com/8218795/37569738-e6417508-2ae6-11e8-87f2-3b411d657372.png)


I wanted to make this as intuitive and easy as possible, so that's really all the setup there is.

## Using the FBDebuggerWindow

After the setup is complete, you can open the debug window in Play Mode with the F key, or through Tools > FBDebugger > Show Debugger. The editor window will display your data root automatically when you open the window, and closes itself when you exit Play Mode because all our setup code happens on Awake.

### Drilling into your data

Explain what these tests test and why

```
Give an example
```

### Sorting and filtering

Explain what these tests test and why

```
Give an example
```

### Data snapshot mapping

Explain what these tests test and why

```
Give an example
```

## Built With

* [Unity 2017](https://unity3d.com/)
* [Google Firebase -> Realtime Database](https://firebase.google.com/docs/database/unity/start)

## TODO

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
* Property list parser by [Chris Danielson](http://www.chrisdanielson.com/2011/05/09/using-apple-property-list-files-with-unity3d/)
* [simple-firebase-unity](https://github.com/dkrprasetya/simple-firebase-unity)
