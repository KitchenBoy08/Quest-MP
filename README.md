# BONELAB TideFusion Release
A multiplayer mod for BONELAB featuring support for all platforms.
[You can view a basic installation guide here.](INSTALLATION.md)

## Networking
Fusion is networked and built around Steam, but the networking system can be swapped out using a Networking Layer. This fork adds its own Networking layer using a networking solution called Riptide.

# Check Fusion's Repo for Fusion Information

## Credits
- Lakatrazz: Made Fusion and all the base things
- Trev: Made the base port of Fusion to quest, allowing things to move along much smoother
- EverythingOnArm: Helped me through the base networking solution, and guided me through this whole project

## Licensing
- The source code of [RiptideNetworking](https://github.com/RiptideNetworking/Riptide) is included under the MIT License. The full license can be found [here](https://github.com/RiptideNetworking/Riptide/blob/main/LICENSE.md).
- The source code of [Open.NAT](https://github.com/lontivero/Open.NAT) is included under the MIT License. The full license can be found [here](https://github.com/lontivero/Open.NAT/blob/master/LICENSE).
- The source code of [Mono.Nat](https://github.com/alanmcgovern/Mono.Nat) is included under the MIT License. The full license can be found [here](https://github.com/alanmcgovern/Mono.Nat/blob/master/LICENSE.md).

## Setting up the Source Code
1. Clone the git repo into a folder
2. Setup a "managed" folder in the "Core" folder.
3. Drag the dlls from Melonloader/Managed into the managed folder.
4. Drag MelonLoader.dll and 0Harmony.dll into the managed folder.
5. Manually reference RiptideNetworking.dll and Open.NAT.dll from the plugins folder (included with mod release).
6. You're done!

## Disclaimer

#### THIS PROJECT IS NOT AFFILIATED WITH LAKATRAZZ AND BASE FUSION, AS WELL AS SLZ OR ANY OTHER DEVELOPMENT PARTIES.
