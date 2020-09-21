# How to use this generator

This generator is an executable jar file that expect some parameter to run. It works for HOPEX V3 and above. It is recommended to use the default HOPEX JRE to run it.

## Step 1 : Create a metamodel in HOPEX

### Custom Metamodel

1. Open HOPEX windows desktop client
2. In MetaStudio tab create a new Metamodel
3. Add a new diagram for this metamodel.
4. Put the piece of the metamodel you want.
5. Copy and keep for later the absolute identifier of the metamodel object

### Complete default metamodel

Should you want to complete the default schema with additional MetaClass, MetaAttribute... follow the steps below :

1. Import the MGL file located in "03 - MGL" folder above
2. Duplicate the standard Metamodel and diagram you want to complete
3. Put the pieve of metamodel you want.
4. Copy and keep for later the absolute identifier of the metamodel object

### Important rules

This generator apply the following rules :
* Only concrete metamodel is generated
* MetaAssociation must be in the diagram to be generated, including the abstract version of the metaassociation.


## Step 2 : Configure the generator

In this example all the files will be moved to the destination folder `C:\temp\java`. You can adjust according to your case.

+ From folder "05 - EXE" copy paste the files to the destination folder :
	+ `generatorJSONMapping.jar` 
	+ `run.bat` 
+ Open the `00_SchemaToGenerate.json` to add your schema. Past the absolute identifier you have save from previous step. 
+ Edit the included section to false to avoid to generate the other schema.

Example of JSON :
```json
{
	"included": "true",
	"schemaName": "ITPM",
	"metaModelAbsoluteIdentifier": "TeEKeRMmSPYK",
	"login": "Tibere",
	"password": "Hopex",
	"profile": "ITPM Functional Administrator"
},
```

+ Login, password, and profile should reflect the rescrition of metamodel you want to apply to the API. If you don't know which profile put 'HOPEX Customizer'. Access right will be read at runtime.

## Step 3 : Run the generator

Before to run the generator edit the file 'run.bat' to adjust : 
+ folders location : HOPEX, log, config file
+ environment path
+ repository name
+ debug option (-d)

To run the program execute the run.bat file. In case of success the message appear in the console.

Processing the request may take a while : between 30 to 45 min depending on the complexicity of th metamodel.

Example of console message in debug :

```
[2020-09-14 08:35:21] [CONFIG ] debug                   = true
[2020-09-14 08:35:21] [CONFIG ] verbose                 = false
[2020-09-14 08:35:21] [CONFIG ] Folder                  = c:\temp\java\
[2020-09-14 08:35:21] [CONFIG ] Log Folder              = c:\temp\java\
[2020-09-14 08:35:21] [CONFIG ] File Name Override      = 00_OverrideName_Global.JSON
[2020-09-14 08:35:21] [CONFIG ] File Name Schema        = 00_SchemaToGenerate.JSON
[2020-09-14 08:35:21] [CONFIG ] Environment             = C:\Users\Public\Documents\HOPEX V4\PRESALESV4
[2020-09-14 08:35:21] [CONFIG ] Repository              = SOHO
[2020-09-14 08:35:21] [INFOS  ] Read schema name
[2020-09-14 08:35:22] [INFOS  ] ########### Starting ###########
[2020-09-14 08:35:22] [INFOS  ] ########## Starting : Custom
[2020-09-14 08:35:22] [INFOS  ] Open HOPEX
[2020-09-14 08:35:29] [INFOS  ] Open Session
[2020-09-14 08:35:29] [INFOS  ] sAdministrator: Mega - sPassword: *****
[2020-09-14 08:35:30] [INFOS  ] Read overRideName JSON
[2020-09-14 08:35:30] [INFOS  ] Creating JSON
[2020-09-14 08:35:30] [INFOS  ] Start Metaclass
[2020-09-14 08:35:32] [INFOS  ] Size = 1
[2020-09-14 08:35:33] [INFOS  ] MetaClass = Application
[2020-09-14 08:36:01] [INFOS  ] Starting Reverse Id
[2020-09-14 08:36:01] [INFOS  ] Start Interfaces
[2020-09-14 08:36:01] [INFOS  ] Wrting filec:\temp\java\SBB.JSON
[2020-09-14 08:36:01] [INFOS  ] Write overRideName JSON
[2020-09-14 08:36:01] [INFOS  ] HOPEX Closed
[2020-09-14 08:36:01] [INFOS  ] ########### All done ###########
```
