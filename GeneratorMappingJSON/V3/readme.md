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

The custom JSON file need to be placed in the deployment of the graphQL component. In this example we assume the new schema is named "mynewschame".

- Step 1 :

Take your generated file "mynewschame.json" and put it in the following folder : `C:\Program Files (x86)\MEGA\HOPEX V4\DotNet\hopex.graphql\1.0.0.0\CONFIG\V3\Custom`
Location may vary based on your HOPEX version.

- Step 2 :
Edit the web.config of graphQL located in `C:\inetpub\wwwroot\HOPEXGraphQL` to add your schema name. location of the folder in IIS may depend on your installation settings.

Edit the key GraphQLSchemas and append with mynewschame. Caution the name in the must be the same as the name of the file (without the extension)

```XML
    <add key="GraphQLSchemas" value="ITPM, Assessment, Audit, BPA, Data, DataPrivacy, MetaModel, Reporting, Risk, Workflow,mynewschame"/>
    <!--      GraphQLSchema-->
```