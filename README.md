# BONELAB Quest-MP (Fusion)
This mod is a port of FUSION to Quest, although it should support both Quest and PCVR Crossplay upon release

## Progress on The Mod
You can see all progress on the mod at: 
https://trello.com/b/IEXzs0C2/quest-fusion-progress

## Networking
This mod utilizes Riptide Networking in order to be usable on quest and Steam Networking (Which is only available on PCVR).

## Modules
Fusion supports a system called "Modules". This allows other code mods to addon and sync their own events in Fusion.
Fusion also has an SDK for integrating features into Marrow™ SDK items, maps, and more.

## Marrow SDK Integration
NOTICE:
When using the integration, if you have the extended sdk installed, delete the "MarrowSDK" script that comes with the extended sdk.
It can be found in "Scripts/SLZ.Marrow.SDK/SLZ/Marrow/MarrowSDK".
The reason for this is that this is already included in the real sdk, and Fusion uses some values from it, causing it to get confused.

You can download the integration unity package from the Releases tab of this repository.
Alternatively, you can download the files raw at this link:
https://github.com/Lakatrazz/BONELAB-Fusion/tree/main/Core/marrow-integration

## Module Example
The module example can be found here:
https://github.com/Lakatrazz/Fusion-Module-Example

## Credits
- EverythingOnArm: For most of the actual networking (I think he should get like 70% of the credit for this port).
- Lakatrazz: For developing Fusion itself and creating an AWESOME network layer system.

## Setting up the Source Code
1. Clone the git repo into a folder
2. Setup a "managed" folder in the "Core" folder.
3. Drag the dlls from Melonloader/Managed into the managed folder.
4. Drag MelonLoader.dll and 0Harmony.dll into the managed folder.
5. You're done!

## Disclaimer

#### THIS PORT IS NOT OFFICIALLY MAINTAINED BY LAKATRAZZ OR ENDORCED BY HIM, DO NOT BUG HIM ABOUT ANY ISSUES YOU MAY HAVE WITH THIS VERSION.
#### THIS MOD IN GENERAL IS NOT AFFILIATED IN ANY WAY WITH SLZ OR ANY OTHER MOD/GAME DEVELOPERS
