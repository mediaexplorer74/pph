# PPH (Pocket Palm Heroes) v1.0-pre-pre-pre-alpha - master branch

![Logo](Images/logo.png)

My fork of PPH-Native project for some micro-RnD

## Screenshots
![Logo](Images/sshot01.png)
![Logo](Images/sshot02.png)

## Design
![Logo](Images/design01.png)
![Logo](Images/design02.png)


## Status
- Init phase of Windows Mobile - Windows Phone (UWP) porting



## Where are we now

Let's check the status according to your general plan for porting PPH to C# (UWP/MonoGame). Below is a clear link to the stages and current results:

- 1. Basic framework — completed:
- There is a ViewManager, IView interface, Update/Draw cycle.
  - Input routing via InputRouter .
  - MenuView, OverlandView, BattleView stubs with minimal rendering.
- 2. Content and resources — in progress:
- Deployment of runtime resources to LocalFolder via RuntimeAssets.DeployAsync (creates Data/Maps/Save/Cache).
  - Fonts.dat , game are being copied.gfx , game.pix , game.sfx , objects.dat ; maps Tutorial_English.phm , Two_by_the_river_English.phm ; PalmHeroes.cfg .
  - HmmReader.ReadHeaderAsync reads first from the LocalFolder, then the fallback to the ms-appx package:/// .
  - The package unpacking/streaming mechanism is documented (Docs/resource_unpacking.md): UnpackAsync plan , index readers, GFX/SFX/Fonts/Objects providers, cache manager.
- 3. Game logic - partially:
- There is a GameProcess blank, but iGame transfer.MainMenu/StartNewGame/ExitGame/Process and the event state machine are still to come.
- 4. UI and interactions — not started:
  - iPopupView, dialogs, and tooltips have not been ported; the font stack is minimal so far (SpriteFont is present, bitmap fonts are not integrated).
- 5. Combat and AI — not started:
  - The battle screen, initiative, actions/effects, and basic AI have not been ported yet.
- 6. Saves and compatibility — the database is ready:
- The Save folder and PalmHeroes.cfg are deployed in LocalFolder. Next is the definition of the save format and compatibility.
— 7. Testing and polishing — not started:
  - Modular input/rendering/sound checks, basic benchmarks, and logic correctness checks are needed.
Recent key changes

- Added explicit Content Include="Data\\objects.dat" in PPH.csproj — fixed the problem of missing objects.dat in LocalFolder.
- In RuntimeAssets.DeployAsync added the creation of a Cache for future physical decompression/caching.
- Updated Docs/project_context.md and added Docs/resource_unpacking.md with an unpacking implementation plan.
- We are using TRAE AI & GPT-5. The assembly was carried out strictly through MSBuild:
- MSBuild.exe "PPH.sln" /t:Restore
  - msbuild "PPH.sln" /t:Build /p:Configuration=Debug /p:Platform=x64 /p:AppxBundle=Never /p:GenerateAppxPackageOnBuild=false
Next steps

- Content and resources:
  - Add RuntimeAssets.UnpackAsync and the cache version marker ( Cache/.version ).
  - Implement the PackIndexReader prototype and IGfxProvider/ISfxProvider/IFontProvider/IObjectsProvider for streaming access.
- Game logic:
- Transfer iGame.* A state machine in GameProcess: a cycle of states, input/timer handlers, a basic overland without a fight.
- UI:
- Start porting iPopupView, simple dialogs and tooltips; coordinate the font layer (SpriteFont/bitmap fonts).
- Diagnostics:
- Enable logging of successes/failures during copying/unpacking; add summaries to LocalFolder.

## Credits

This work contains the following code:

* Pocket Heroes Game, Apache 2.0 License
* libxml2, MIT License

Also, there are the following dependencies in the `externals` dir:

* Windows Template Library, Microsoft Public License
* LZ👌 (lzokay), MIT License
* libpng and zlib, zlib License

## References
- https://github.com/SerVB/pph-native PPH by SerVB
- https://www.palmheroes.com/ Old Palm Heroes site

## .

As is. No support. RnD only.

## ..

[m][e] Nov, 2 2025

