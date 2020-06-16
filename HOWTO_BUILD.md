**To Build this solution you will have to respect the following folder hierarchy for your GraphQL solution.**

The solution file and the projects folders must be in the following Hierarchy:
- [ParentFolder]\[AddonsFolder]\HOPEXGraphQL

Then you have to copy:
- [C:\inetpub\wwwroot]\HOPEXAPI\bin\Mega.Bridge.dll
- [C:\Program Files (x86)]\MEGA\[HOPEX]\System\Hopex.ApplicationServer.WebServices.dll
- [C:\Program Files (x86)]\MEGA\[HOPEX]\System\Mega.Macro.Executor.dll
- [C:\Program Files (x86)]\MEGA\[HOPEX]\System\Mega.Macro.Wrapper.dll
INTO
- [ParentFolder]\ExeRelease\Assemblies\

*If you want to build the Setup project, you will need to install WiX Toolset (https://wixtoolset.org) and it's Visual Studio extension.*
*If you don't want to build the setup, Visual Studio will fail to load it and wil disable it.*
*It's is not mandatory to build this project to start working on your addon.*

Finally you can open your solution in VisualStudio 2019 and build it.
