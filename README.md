<p align="center">
  <img src="Resources/logo.png" width="75%" height="75%">
</p>

# Maelstrom Launcher

This is a project for the Maelstrom Launcher's Server, which is used for the corresponding World of Warcraft Roleplay server, written in .NET Core 9.0 with ASP.NET.

Also featuring a [fully custom WPF game launcher](https://github.com/Cenatm/MaelstromLauncher) written in .NET for communication with the server and recieving files via HTTP.


## Features

- Custom .
- Comprehensive file download, serving & validation logic.
- QoL feature set for keeping game up to date and shipped on the client.
- Efficient manifest.json operations for serving files and handling files on the backend.
- In-code documentation with extensive logging.
- Modular API/application design viable for any game.
- Modern feature set using the latest features from .NET Core 9.0
- Thread safe fully asynchronous background operations.
- Memory efficient downloader service with file streaming.

## Building

### Prerequisites

* [.NET Core SDK 9.0.0 or later](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)

### Build Instruction

* Available runtime identifiers/platforms: win-x64/x64 (Any CPU)
* Available release configurations: Release, Debug
* Execute `dotnet build MaelstromLauncher.Server.sln -c Release -p:Platform="Any CPU"`
* Output is placed in `bin\Release\net9.0-windows\`
