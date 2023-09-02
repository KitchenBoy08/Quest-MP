# BONELAB TideFusion Release
A multiplayer mod for BONELAB featuring support for all platforms.
[You can view a basic installation guide here.](INSTALLATION.md)

![](https://i.imgur.com/1ZpMfei.png)

## Networking
Fusion is networked and built around Steam, but the networking system can be swapped out using a Networking Layer. This fork adds its own Networking layer using P2P called Riptide.

## Modules
Fusion supports a system called "Modules". This allows other code mods to add on and sync their own events in Fusion.
Fusion also has an SDK for integrating features into Marrow™ SDK items, maps, and more.

## Marrow SDK Integration
NOTICE:
When using the integration, if you have the extended sdk installed, delete the "MarrowSDK" script that comes with the extended sdk.
It can be found in "Scripts/SLZ.Marrow.SDK/SLZ/Marrow/MarrowSDK".
The reason for this is that this is already included in the real sdk, and Fusion uses some values from it, causing it to get confused.

You can download the integration unity package from [the Releases tab of this repository](https://github.com/Lakatrazz/BONELAB-Fusion/releases/latest).
Alternatively, you can download the files raw [here](https://github.com/Lakatrazz/BONELAB-Fusion/tree/main/Core/marrow-integration).

## Module Example
The module example can be found here:
https://github.com/Lakatrazz/Fusion-Module-Example

## Credits
- Lakatrazz: Made Fusion and all the base things
- Trev: Made the base port of Fusion to quest, allowing things to move along much smoother
- EverythingOnArm: Helped me through the base networking solution, and guided me through this whole project

## Licensing
- The source code of [RiptideNetworking](https://github.com/RiptideNetworking/Riptide) is included under the MIT License. The full license can be found [here](https://github.com/RiptideNetworking/Riptide/blob/main/LICENSE.md).
- The source code of [Open.NAT](https://github.com/lontivero/Open.NAT) is included under the MIT License. The full license can be found [here](https://github.com/lontivero/Open.NAT/blob/master/LICENSE).

## Setting up the Source Code
1. Clone the git repo into a folder
2. Setup a "managed" folder in the "Core" folder.
3. Drag the dlls from Melonloader/Managed into the managed folder.
4. Drag MelonLoader.dll and 0Harmony.dll into the managed folder.
5. Manually reference RiptideNetworking.dll and Open.NAT.dll from the plugins folder (included with mod release).
6. You're done!

## Disclaimer

#### THIS PROJECT IS NOT AFFILIATED WITH LAKATRAZZ AND BASE FUSION, AS WELL AS SLZ OR ANY OTHER DEVELOPMENT PARTIES.
