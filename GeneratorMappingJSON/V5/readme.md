# Structure of the folder

This folder contains the needed ressource to create your custom endpoint for the graphQL REST API

## 01 - JSON

This folder contains the default JSON generated out-of-the-box by the JSON generator. There are installed by default by the setup.

2 files are required to generate the JSON of the schema :

### File : 00_OverrideName_Global.JSON

This file contains renaiming of the metamodel when generating the JSON. The renaiming can be applied to Metaclass, MetaAttribute, MetaAssociationEnd...

The provided file contains the defaukt renaiming. You need this file to applied the default renaiming when generating your JSON.

### File : 00_SchemaToGenerate.json

This file is used by the generator to know which schema to generate. It contains the absolute identifier of the metamodel in HOPEX that we want to generate.

## 02 - MetaModel (PNG)

This contains a picture of the selected metamodel contains in the JSON. This metamodel represent a subset of the standard HOPEX metamodel

## 03 - MGL

This contains an MGL file that can be imported in HOPEX V3 and above with Metamodel and Metamodel diagram used to generated the above JSON

## 04 - SRC

Contains all the needed resource to create you own JSON generator based on the standard one. The project run with eclipe and Java 8

## 05 - EXE

This contains, a ready to use, executable jar file to generate your own JSON


# How to use your custom JSON schema

The custom JSON file needs to be placed in the has.custom module in the graphQL folder. 
In this example we have following conditions:
- the new schema is named "mynewschame"
- V5 instance uses the "5000" port.
- has.custom version is 15.2.0+17.

### Step 1 :

Take your generated file "mynewschame.json" and put it in the following folder :\
**`C:\ProgramData\MEGA\Hopex Application Server\5000\.shadowFiles\has.custom\15.2.0+17\hopex.graphql\CONFIG\V3\Custom`**\
Location may vary based on your HAS instances locations.

### Step 2 (optional):
If you use the GraphQL IDE module: add the new schema in the options like this:
- Login into HAS console
- In **Cluster** tab (default), search for the **GraphQL IDE** module and click on the settings button (with the "gear" icon)
- Add the new schema in the **GraphQL schemas** list with a previously added comma as separator.
- If needed, restart HAS instance.
