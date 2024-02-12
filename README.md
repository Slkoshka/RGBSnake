# RGB Snake

Simple **Snake** game implementation using RGB RAM (via **Corsair iCUE**) as a screen. Supports only x64 versions of Windows (should work on Windows 11 and Windows 10 1607+).

Example video and more context: [here](https://twitter.com/Slkoshka/status/1757033574723727798).

## How to run

Download the latest version from the [Releases](https://github.com/Slkoshka/RGBSnake/releases/latest) page or build it yourself (requires **.NET 8.0 SDK**):

```cmd
dotnet publish RGBSnake.csproj -c Release -o publish
```

This command will save binaries to the `publish` folder.

## Controls

Use WASD or the arrow keys for movement.

## License

This project is licensed under the MIT license with an exception of Corsair iCUE SDK binaries that are included as a part of this repository inside the `x64` folder.

## Credits

**Inspiration**: [adamjezek98/ubnt-etherlighting](https://github.com/adamjezek98/ubnt-etherlighting) project by [Adam Ježek](https://twitter.com/adamjezek98)
