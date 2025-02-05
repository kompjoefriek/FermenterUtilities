# Fermenter Utilities
A few customisation options for fermenters including percentage progress, not requiring cover and custom fermentation time

## Automatic Installation
R2ModMan: https://r2modman.com/

## Manual Installation
To install this mod, you need to have BepInEx: https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/
After installing BepInEx, extract FermenterUtilities.dll into games install **"\Valheim\BepInEx\plugins"**

## Config

### Global;
| Config Option | Type | Default Value | Description |
|:-------------:|:-----------:|:-----------:|:-----------|
| Enable Mod | bool | true | Enable or disable the mod |
| Enable Logging | bool | true | Enable or disable logging for this mod |

### Progress;
| Config Option | Type | Default Value | Description |
|:-----------:|:-----------:|:-----------:|:-----------|
| Show Percentage | bool | true | Shows the fermentation progress as a percentage when you hover over the fermenter |
| Show Percentage Color | bool | true | Makes it so the percentage changes color depending on the progress |
| Show Percentage Decimal Places | int | 2 | The amount of decimal places to show for the percentage |
| Show Time | bool | false | Show the time when done |

### Time;
| Config Option | Type | Default Value | Description |
|:-----------:|:-----------:|:-----------:|:-----------|
| Custom Time | bool | false | Enables the custom time for fermentation |
| Fermentation Time | int | 5 | The amount of minutes fermentation takes (Default 40) |

### Cover;
| Config Option | Type | Default Value | Description |
|:-----------:|:-----------:|:-----------:|:-----------|
| Work Without Cover | bool | false | Allow the Fermenter to work without any cover |

If you have any suggestions, feel free to let me know!

## Not my code
This is just a new Visual Studio project around the code of https://github.com/smallo92/FermenterUtilities.
New project generated with bepinex5plugin template: https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/index.html
Just changed target from net46 to net462 (fixes missing reference to netstandard)

## Development setup
Installed VS2022 with:
Workloads:
- .NET desktop environment
Individual components:
- .NET Framework 4.6.2
