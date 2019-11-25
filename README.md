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