[TOC]

# Project Title

This add-on expose a catalog of REST API that can be queried following the GraphQL specification framework. The call to the API are made using JSON that contains the query to execute (query or mutation). The result of the call to the REST API is a JSON containing the data requested. With this add-on you can read and write data in the HOPEX repository.

When properly authenticated (UAS bearer) you will be able to get, for instance, the list of applications, business process, capabilities...and their fields and relationships. To ease navigation, different schemas are provided by solutions : ITPM, GDPR, Audit, BPA...

[![GraphQL REST API Overview](http://img.youtube.com/vi/uI6ZRsOQdlk/0.jpg)](http://www.youtube.com/watch?v=uI6ZRsOQdlk "GraphQL REST API Overview")

## Getting Started

We recommend you to use this add-on "out of the box" without changes the source code. Just deploy using the latest install MSI in the release section.

See installation instruction on how to install the project on a live system.

### Prerequisites

This add-on runs on the HOPEX Platform. Minimum platform level is V3 CP2.

###Installation

Follow the step of the installation wizard : [Video of the installation](https://youtu.be/80BNrmm64gc "Video of the installation")

#### Mains steps included in the automatic wizard
1. Deployement of the hopexGraphQL folder in IIS:
> C:\inetpub\wwwroot\HOPEXGraphQL
2. Deployement the source in the default HOPEX Platform installation folder :
> C:\Program Files (x86)\MEGA\HOPEX V3\DotNet\hopex.graphql

#### Manual steps to perform

1. Convert the folder hopexGraphQL into an application in IIS:
> Convert to application
> Select application pool **HOPEXAPI**

2. Configure web.config in folder hopexGraphQL
>Adapt the URL to match your server installation
```
<add key="AuthenticationUrl" value="" />
<add key="MegaSiteProvider" value="" />
```
>Define the security key in the secureAppSettings
```
<secureAppSettings>
<add key="SecurityKey" value="" />
</secureAppSettings>
```
>Define the default environment, repository and user for GraphiQL
```
    <add key="EnvironmentId" value="" />
    <add key="RepositoryId" value="" />
    <add key="ProfileId" value="" />
    <add key="Login" value="" />
    <add key="Password" value="" />
```
## Use the GraphQL REST API

When calling the API you need to send an GraphQL query in a JSON format. The response you will get is a JSON with either the result or an error message. 

| JSON GraphQL query | JSON GraphQL response |
| :------------ | :------------- |
| query { </br>&emsp; application { </br> &emsp;&emsp; id </br>&emsp;&emsp; name  </br> &emsp; } </br>}  | { </br> "data": { </br> &emsp; "application": [ </br>&emsp;&emsp;&emsp; { </br>&emsp;&emsp;&emsp; "id": "rnRV)xNbQXB6", </br>&emsp;&emsp;&emsp; "name": "Web portal" </br>&emsp;&emsp;&emsp; } </br> &emsp;&emsp; ] </br>&emsp;} </br>} |

Find more information on the [MEGA Community](https://community.mega.com/ "MEGA Community")


### Required information

Get this information from your adminsitrator in order to be able to make call to the API
- Environment absolute identifier
- Repository absolute identifier
- Profil absolute identifier
- HOPEX user login
- HOPEX user password

### Building query with GraphiQL

You can use GraphiQL to build your query. Example of how to use it here : [How to use GraphiQL](https://youtu.be/oBGdII-sCuw "How to use GraphiQL")

### Testing the API with Postman

You can use tool like postman to make test call to the API. [How to build Postman HTTP request](https://youtu.be/3xgesyYCXsw "How to build Postman HTTP request")


## Built With

* [Microsoft Visual Studio 2019](https://visualstudio.microsoft.com/ "Microsoft Visual Studio 2019")


## Versioning

The master branch contains the latest available version of the source code. Each version source code is available in the zip of each release.

## Authors

* **Olivier GUIMARD** - *Product Owner* - [OGD](https://github.com/oguimardmega "OGD")
* **Laurent GIBELIN** - *Lead Dev*
* **Sebastien CRONIER** - *Lead Dev* 

## License

This project is licensed under the HOPEX License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* GraphQL Framwork
* GraphCool specification for filter and sort


