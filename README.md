# SLP-Sharp
This is an ASP.Net Core version of the Switch Lan Play Relay server, originally written by SpaceMeowX2 in NodeJs/Typescript which can be found here: https://github.com/spacemeowx2/switch-lan-play/blob/master/server

## Command Line
There are 2 available command line options allowing you to specify a port and a listen address.

### Listen Address
You can specify an IP to listen on with the `-ip` option.

Example running on IP 0.0.0.0:
```
dotnet SwitchLanNet.dll --ip 0.0.0.0
```

### Listen Port
You can specify which port to listen on by with the `-p` option.

Example running on port 2150:
```
dotnet run SwitchLanNet.dll -p 2150
```

## API
At the moment, only a client count and bytes/s current upload/download rate are provided by the API, returned as JSON.

To call the API, simply `GET http://ip:port/`

## Building
Download .net core 2.2 for your desired platform here: https://dotnet.microsoft.com/download/dotnet-core/2.2

### Visual Studio 2019
Install Visual Studio 2019, the community edition is fine.
Clone this repo and open the .sln file in Visual Studio, wait for it to restore the packages and it should build and run fine.

### Command Line
If you're developing with VS Code or another IDE, or setting the project up on a linux server, etc, you can build and run the project via the command line.

Clone the repo, cd into the project folder(SwitchLanNet, with the .csproj file) and run `dotnet restore` to restore the packages then you can run the project with `dotnet run .`

You can build the project with `dotnet build`. If you want to build it as release, run `dotnet build --configuration Release`.

You can publish the project to create a release you can distribute and run on any platform(`dotnet SwitchLanNet.dll`) with `dotnet publish`. 
