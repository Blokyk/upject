# upject
A small C# utility to fetch and format Github Projects from a given repo

## Building

This project can be built on any platform supporting one of the following frameworks :
- .NET 5
- .NET Core 3.1
- .NET Core 3
- .NET Framework 4.6 (only on windows or using mono)

To build for your current platform, you can simply run `dotnet build -c Release`. If you wish to build for other platforms, you can either use the `build_all.sh` script (bash, tipically for Linux & Mac) or use `dotnet publish -f FRAMEWORK -r PLATFORM -c Release`.

## Running

To run this tool, you need to set at least two variables :
- `UPJECT_USERNAME`, which will contain your github username/email
- `UPJECT_API_KEY` OR `UPJECT_PASSWORD`, which will contain your github API key/password (API Key might be required if you activated 2FA)

If you'd prefer to not store those as variables, you can instead inline them in Program.cs @ line 16 and 17 and then rebuild the project.

#### Why not use a secret manager

A secret manager, IMO, adds too much bloat and is too complicated for such a simple tool. I might add a build flag to allow the user to specify them on a command-to-command basis, but an app can just look at your command history and check them.
