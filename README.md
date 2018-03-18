# FirebaseDebugger
The idea for this project started 
This project is an Editor Window for debugging Firebase Realtime Database projects in Unity.

## Getting Started

All the setup for this tool is done programmatically, so the only thing you're responsible for is going to Tools > FBDebugger and clicking Setup. 


![Basic Setup](https://user-images.githubusercontent.com/8218795/37569699-6214c5be-2ae6-11e8-829e-99980a40eae7.png)


The setup menu item will only be active if Unity is NOT in Play Mode, and the setup hasn't already been completed. I've done my best to create a sort of setup 'happy-path', so if you try and do anything out of order or in the wrong editor mode don't worry, there will be message dialogs to guide you back on track.


![Editor Display Messages](https://user-images.githubusercontent.com/8218795/37569700-6860128e-2ae6-11e8-8c2f-42846a138c79.png)


After the setup is done, select the Firebase Manager game object and assign your GoogleService-Info.plist in the Inspector.


![Setting GoogleService-Info.plist](https://user-images.githubusercontent.com/8218795/37569738-e6417508-2ae6-11e8-87f2-3b411d657372.png)

### Prerequisites

What things you need to install the software and how to install them

```
Give examples
```

### Installing

A step by step series of examples that tell you have to get a development env running

Say what the step will be

```
Give the example
```

And repeat

```
until finished
```

End with an example of getting some data out of the system or using it for a little demo

## Running the tests

Explain how to run the automated tests for this system

### Break down into end to end tests

Explain what these tests test and why

```
Give an example
```

### And coding style tests

Explain what these tests test and why

```
Give an example
```

## Deployment

Add additional notes about how to deploy this on a live system

## Built With

* [Dropwizard](http://www.dropwizard.io/1.0.2/docs/) - The web framework used
* [Maven](https://maven.apache.org/) - Dependency Management
* [ROME](https://rometools.github.io/rome/) - Used to generate RSS Feeds

## Contributing

Please read [CONTRIBUTING.md](https://gist.github.com/PurpleBooth/b24679402957c63ec426) for details on our code of conduct, and the process for submitting pull requests to us.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/your/project/tags). 

## Authors

* **Billie Thompson** - *Initial work* - [PurpleBooth](https://github.com/PurpleBooth)

See also the list of [contributors](https://github.com/your/project/contributors) who participated in this project.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* Hat tip to anyone who's code was used
* Inspiration
* etc
