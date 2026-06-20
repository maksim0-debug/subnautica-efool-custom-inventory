# efool's Custom Inventory Mod for Subnautica & Below Zero

This is a fork of the original Custom Inventory mod by efool, updated to support the Subnautica 1.0 (Living Large Update) and ported to Subnautica: Below Zero.

- Configure size of inventories:
  - Player inventory (both games)
  - Locker inventories (waterproof locker, wall locker, locker, lifepod locker, Cyclops/SeaTruck lockers)
  - Seamoth storage (Subnautica 1 only)
  - SeaTruck storage (Below Zero only, including Locker 1/2 and Locker 3)
  - Prawn suit storage (both games, including additional size per storage module)
  - Scanner room upgrades (both games)
  - Bioreactor (both games)
  - Filtration machine (both games, including salt/water ratio)
  - Carry-all bags (both games)
  - Trash can (both games)
  - Nuclear waste can (both games)
  - Planters (it don't make sense, but you can do it)
- Inventory sizes are per-save
- Preset inventory sizes
- Bulk inventory operations (switch, drop, eat, etc) modifier keys
  - `left shift` all items
  - `left ctrl` all items of selected tech type
  - Modifier keys are configurable
- Pin inventory items to prevent move, drop, eat, etc
  - `left alt` click item inside an inventory to toggle pin
  - Modifier key is configurable
  - Complements bulk operations
  - Bulk pin all items with `left alt` + `left shift`
  - Bulk pin items by tech type with `left alt` + `left ctrl`

# Requirements

### For Subnautica 1 (Living Large)

- Subnautica patch Oct-2025 83031
- BepInEx Pack v5
  - [Nexus Mods](https://www.nexusmods.com/subnautica/mods/1108)
  - [Github](https://github.com/toebeann/BepInEx.Subnautica)
- Nautilus v1.0.0-pre.46 (or later)
  - [Nexus Mods](https://www.nexusmods.com/subnautica/mods/1262)
  - [Github](https://github.com/SubnauticaModding/Nautilus)

### For Subnautica: Below Zero

- Subnautica: Below Zero
- BepInEx Pack v5 (or compatible pack)
  - [Nexus Mods](https://www.nexusmods.com/subnauticabelowzero/mods/344)
- Nautilus v1.0.0 (or later)
  - [Nexus Mods](https://www.nexusmods.com/subnauticabelowzero/mods/373)
  - [Github](https://github.com/SubnauticaModding/Nautilus)

# Installation

### For Subnautica 1

- Install BepInEx Pack v5 and Nautilus.
- Extract `efool-custom-inventory_#.#.#.zip` to your game directory so that the files are placed as:
  - `[game]/BepInEx/plugins/efool-custom-inventory/efool-custom-inventory.dll`
  - `[game]/BepInEx/plugins/efool-custom-inventory/presets.json`

### For Subnautica: Below Zero

- Install BepInEx Pack v5 and Nautilus.
- Extract `efool-bz-custom-inventory_#.#.#.zip` to your game directory so that the files are placed as:
  - `[game]/BepInEx/plugins/efool-bz-custom-inventory/efool-bz-custom-inventory.dll`
  - `[game]/BepInEx/plugins/efool-bz-custom-inventory/presets.json`

Note: `[game]` is the directory containing your game executable (`Subnautica.exe` or `SubnauticaZero.exe`).

# Custom Presets

There is no UI option to modify presets. Manually modify `presets.json` at the installed location (`[game]/BepInEx/plugins/efool-custom-inventory/presets.json` or `[game]/BepInEx/plugins/efool-bz-custom-inventory/presets.json`) to add your own entry. See `efool's Hoarder Rules` as a reference.

# Console Commands

### Universal Commands (Both Games)

| Description                          | Command                              |
| ------------------------------------ | ------------------------------------ |
| Player inventory width               | `set_inventory_width`                |
| Player inventory height              | `set_inventory_height`               |
| Locker width                         | `set_locker_width`                   |
| Locker height                        | `set_locker_height`                  |
| Wall locker width                    | `set_smalllocker_width`              |
| Wall locker height                   | `set_smalllocker_height`             |
| Waterproof locker width              | `set_smallstorage_width`             |
| Waterproof locker height             | `set_smallstorage_height`            |
| Carry-all width                      | `set_luggagebag_width`               |
| Carry-all height                     | `set_luggagebag_height`              |
| Prawn suit storage width             | `set_exosuit_width`                  |
| Prawn suit storage height            | `set_exosuit_height`                 |
| Prawn suit height per storage module | `set_exosuitStorageModule_height`    |
| Scanner room upgrades width          | `set_maproomupgrades_width`          |
| Scanner room upgrades height         | `set_maproomupgrades_height`         |
| Bioreactor width                     | `set_basebioreactor_width`           |
| Bioreactor height                    | `set_basebioreactor_height`          |
| Filtration machine max salt          | `set_basefiltrationMachine_maxSalt`  |
| Filtration machine max water         | `set_basefiltrationMachine_maxWater` |
| Nuclear waste disposal width         | `set_labtrashcan_width`              |
| Nuclear waste disposal height        | `set_labtrashcan_height`             |
| Trash can width                      | `set_trashcans_width`                |
| Trash can height                     | `set_trashcans_height`               |
| Basic plant pot width                | `set_planterpot_width`               |
| Basic plant pot height               | `set_planterpot_height`              |
| Composite plant pot width            | `set_planterpot2_width`              |
| Composite plant pot height           | `set_planterpot2_height`             |
| Chic plant pot width                 | `set_planterpot3_width`              |
| Chic plant pot height                | `set_planterpot3_height`             |
| Plant shelf width                    | `set_plantershelf_width`             |
| Plant shelf height                   | `set_plantershelf_height`            |
| Exterior growbed width               | `set_farmingtray_width`              |
| Exterior growbed height              | `set_farmingtray_height`             |
| Indoor growbed width                 | `set_planterbox_width`               |
| Indoor growbed height                | `set_planterbox_height`              |
| Alien containment planter width      | `set_planter_width`                  |
| Alien containment planter height     | `set_planter_height`                 |

### Subnautica 1 Only Commands

| Description                           | Command                     |
| ------------------------------------- | --------------------------- |
| Lifepod 5 storage width               | `set_lifepodLocker_width`   |
| Lifepod 5 storage height              | `set_lifepodLocker_height`  |
| Seamoth storage width                 | `set_seamoth_width`         |
| Seamoth storage height                | `set_seamoth_height`        |
| Cyclops storage locker default width  | `set_cyclopsLocker_width`   |
| Cyclops storage locker default height | `set_cyclopsLocker_height`  |
| Cyclops storage locker 1 width        | `set_cyclopsLocker1_width`  |
| Cyclops storage locker 1 height       | `set_cyclopsLocker1_height` |
| Cyclops storage locker 2 width        | `set_cyclopsLocker2_width`  |
| Cyclops storage locker 2 height       | `set_cyclopsLocker2_height` |
| Cyclops storage locker 3 width        | `set_cyclopsLocker3_width`  |
| Cyclops storage locker 3 height       | `set_cyclopsLocker3_height` |
| Cyclops storage locker 4 width        | `set_cyclopsLocker4_width`  |
| Cyclops storage locker 4 height       | `set_cyclopsLocker4_height` |
| Cyclops storage locker 5 width        | `set_cyclopsLocker5_width`  |
| Cyclops storage locker 5 height       | `set_cyclopsLocker5_height` |

### Below Zero Only Commands

| Description              | Command                       |
| ------------------------ | ----------------------------- |
| SeaTruck Locker 1 width  | `set_seatruck_locker1_width`  |
| SeaTruck Locker 1 height | `set_seatruck_locker1_height` |
| SeaTruck Locker 2 width  | `set_seatruck_locker2_width`  |
| SeaTruck Locker 2 height | `set_seatruck_locker2_height` |
| SeaTruck Locker 3 width  | `set_seatruck_locker3_width`  |
| SeaTruck Locker 3 height | `set_seatruck_locker3_height` |

# Known Issues

- Decreasing inventory size can lose stored items
  - Workaround: choose inventory sizes at the beginning of a save
- Planter inventory sizes have no effect
- Bulk item drop occludes PDA
  - Workaround: close PDA to allow items to move away or manually look elsewhere before opening PDA again

# Other Mods

- This custom inventory mod replaces these popular mods:
  - [Advanced inventory](https://www.nexusmods.com/subnautica/mods/490) (Subnautica 1)
  - [Inventory size](https://www.nexusmods.com/subnautica/mods/1206) (Subnautica 1)
    - Note: "Inventory size" supports dynamically recognized storages that this mod does not.

- This custom inventory mod goes well with these mods:
  - [EasyCraft](https://www.nexusmods.com/subnautica/mods/24) (Subnautica 1) / [EasyCraft for Below Zero](https://www.nexusmods.com/subnauticabelowzero/mods/10) (Below Zero)
  - [More Modified Items (BepInEx)](https://www.nexusmods.com/subnautica/mods/398) (Subnautica 1)
    - Scuba Manifold connects to all the tanks in your giant inventory
