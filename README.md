# Fermenter Utilities
A few customisation options for fermenters including percentage progress, not requiring cover and custom fermentation time

## Manual Installation
To install this mod, you need to have BepInEx. After installing BepInEx, extract FermenterUtilities.dll into games install **"\Valheim\BepInEx\plugins"**

#### There are serveral config options available;

## Main Toggles;
| Config Option | Type | Default Value | Description |
|:-------------:|:-----------:|:-----------:|:-----------|
| Enable Mod | bool | true | Enable or disable the mod |

## Percentage;
| Config Option | Type | Default Value | Description |
|:-----------:|:-----------:|:-----------:|:-----------|
| Show Percentage | bool | true | Shows the fermentation progress as a percentage when you hover over the fermenter |
| Show Colour Percentage | bool | true | Makes it so the percentage changes colour depending on the progress |
| Decimal Places | int | 0 | Amount of decimal places to display on the percentage if you want more detail |

## Time;
| Config Option | Type | Default Value | Description |
|:-----------:|:-----------:|:-----------:|:-----------|
| Custom Time | bool | false | Enables the custom time for fermentation |
| Fermentation Time | int | 5 | The amount of minutes fermentation takes (Default 40) |

## Cover;
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
