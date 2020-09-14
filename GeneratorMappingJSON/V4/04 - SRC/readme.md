
# Structure of the folder

This folder contains all the needed element to cusomize the generator.

In most situation you **don't need to customize** this generator. Just use the ready to use executable jar file and selected you **custom metamodel**.

### Caution :
Customization done in the generator may not be properly interpreted by the graphQL runtime API.


## Folder eclipse-project

This contains a jar file to import in eclipse. In eclipse import a new project and select the Jar file.

Make sure you configure the JRE to be the same one as HOPEX

Default location : `C:\Program Files (x86)\MEGA\HOPEX V4\java\jre`

Version number maybe V3, V4 or more


## Folder jar-ressources

This contains the jar that are needed by eclipe to build the source.


## Folder java-graphQL

This contains the java source file. You don't need it has they are contain in the jar file project. Except if you want to use another tool than eclipse to build your project.
