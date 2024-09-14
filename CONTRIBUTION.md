# Contribution Guideline

## Installation

In case you already installed this plugin via the package manager you need to uninstall it in the package manager before you get started with setting up the development environment.

##### Prerequisites
- Windows 10
- [Git](https://git-scm.com/downloads)
- [Visual Studio 2022](https://visualstudio.microsoft.com/de/downloads/)
- [Rhinoceros 7](https://www.rhino3d.com/download/)

##### Setup
1. Clone this repository (or a forked repository) to your machine
2. Open the Visual Studio solution ```D2PComponentsCSharp.sln```

##### Debug Configuration
1. Build the solution with the ```Debug```  ```Any CPU``` configuration
2. Start Rhino and run ```GrasshopperDeveloperSettings```
3. Add ```**YOUR_REPOSITORY_DIRECTORY**\D2P-Components\D2P_GrasshopperTools\bin\Debug\net48\``` to the library folders
4. Set the path to your Rhino installation in the the D2P_GrasshopperTools project settings under ```Debugging > General```
By default it will try to start Rhino at ```C:\Program Files\Rhino 7\System\Rhino.exe```
5. Start the Debugger for ```D2P_GrasshopperTools``` in Visual Studio (with ```Debug```  ```Any CPU```)
this will start Rhino and attach the debugging session to the Rhino process).
6. After Rhino has been launched start Grasshopper and checkout the plugin tab ```D2P``` 