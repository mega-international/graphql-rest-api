#Has Web Site

## TODO

Check has-manifest.json and update settings (Description, name, author)

## Publish your module

Ensure the latest has global tool is installed on your machine, use the following command to install (or update) it.

```
dotnet tool update has --global
```

and then publish your module with :

```
has module create [-p <module-folder>] -s <HAS server address> -t <HAS token> --version 1.0.0
``` 

module-folder is optional and use the current directory by default
version is required (major.minor.build) or any valid semantic version

if you want just create a package without publication, omit -s and -t options.

Use ```has module -?``` to display available options.

