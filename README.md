### Scene Loader
A custom scene loader component that makes it really easy to load unity scenes

![Scene Loader](/Promotional/inspector_component.jpg)

## Installation
You can install or import Scene Loader in your project through the following wayss:

<!--
### Install via OpenUPM
The package is available on the [openupm registry](https://openupm.com/). It's recommended to install it via [openupm-cli](https://github.com/openupm/openupm-cli).

```
openupm add com.ng.homealonestudios.papae.tools.sceneloader
```
-->

### Install via Git URL
Using the native Unity Package Manager introduced in 2017.2, you can add this library as a package by modifying your `manifest.json` file found at `/ProjectName/Packages/manifest.json` to include it as a dependency. See the example below on how to reference it.

```
{
	"dependencies": {
		...
		"com.ng.homealonestudios.papae.tools.sceneloader" : "https://github.com/Home-Alone-Studios/scene-loader.git?path=/Assets",
		...
	}
}
```

Or you can simply add the URL https://github.com/Home-Alone-Studios/scene-loader.git?path=/Assets in package manager as shown below

![Import Instruction](/Promotional/import_instruction.jpg)

<!--
### Install via classic `.UnityPackage`
The latest release can be found [here](https://github.com/home-alone-studios/scene-loader/releases) as a UnityPackage file that can be downloaded and imported directly into your project's Assets folder.


### Install via zip extract
-->

## Samples: Todo
- ~~Synchronous scene demo~~
- Asynchronous scene load
- ~~Preloaded asynchronous scene demo~~
- [Asynchronous scene load with user inteface and userfeedback progress](https://www.patrykgalach.com/2021/02/15/smooth-scene-loading)
- Asynchronous load and unload of different portions (scenes) of a full 3D level or map
- Asynchronous scene load with user inteface and userfeedback progress via [SceneLoadManager](/Assets/Scripts/SceneLoaderManager.cs)
- Asynchronous load of multiple dependent scenes with user inteface and userfeedback progress via [SceneLoadManager](/Assets/Scripts/SceneLoaderManager.cs)  


## Usage
Coming soon.

<!--
To learn more about how to use JCMG Package Tools, see [here](./usage.md) for more information.
-->

## Special Thanks
- [Scene Reference](https://github.com/roboryantron/UnityEditorJunkie#scenereference)

<!--
## Contributors
If you are interested in contributing, found a bug, or want to request a new feature, please see [here](./contributors.md) for more information.



## Support
If this is useful to you and/or you’d like to see future development and more tools in the future, please consider supporting it either by contributing to the Github projects (submitting bug reports or features and/or creating pull requests) or by buying me coffee using any of the links below. Every little bit helps!

[![ko-fi](https://www.ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/I3I2W7GX)

## License
-->


