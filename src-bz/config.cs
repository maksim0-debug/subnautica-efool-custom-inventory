using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using Newtonsoft.Json;

using Nautilus.Commands;
using Nautilus.Options;
using Nautilus.Options.Attributes;

namespace org.efool.subnautica_bz.custom_inventory {

public class OptionsMenu : ModOptions
{
	private ConfigPerSave _custom = new ConfigPerSave();
	private ConfigPerSave _current;

	public bool inGame { get => Player.main != null; }

	public OptionsMenu(object inst)
		: base(Info.title)
	{
		foreach ( var field in inst.GetType().GetFields() ) {
			switch ( field.GetCustomAttribute<ModOptionAttribute>() ) {
			case SliderAttribute attr:
				var slider = ModSliderOption.Create(attr.Id, attr.Label, attr.Min, attr.Max, readSliderField(inst, field), defaultValue: attr.DefaultValue, valueFormat: attr.Format, step: attr.Step, tooltip: attr.Tooltip);
				slider.OnChanged += (_, e) => writeSliderField(inst, field, e.Value);
				AddItem(slider);
				break;
			case ToggleAttribute attr:
				var toggle = ModToggleOption.Create(attr.Id, attr.Label, (bool)field.GetValue(inst), tooltip: attr.Tooltip);
				toggle.OnChanged += (_, e) => field.SetValue(inst, e.Value);
				AddItem(toggle);
				break;
			}
		}
	}

	public static SliderLabelMode typeToLabelMode(Type type)
	{
		if ( type == typeof(int) )
			return SliderLabelMode.Int;
		if ( type == typeof(float) )
			return SliderLabelMode.Float;

		return SliderLabelMode.Default;
	}

	public static float readSliderField(object obj, FieldInfo field)
	{
		if ( field.FieldType == typeof(float) )
			return (float)field.GetValue(obj);
		if ( field.FieldType == typeof(int) )
			return (int)field.GetValue(obj);

		return 0;
	}

	public static void writeSliderField(object obj, FieldInfo field, float value)
	{
		if ( field.FieldType == typeof(float) )
			field.SetValue(obj, value);
		else if ( field.FieldType == typeof(int) )
			field.SetValue(obj, (int)value);
	}

	public void loadCurrent()
	{
		if ( _current == null ) {
			_current = new ConfigPerSave();
			_current.copySettings(Plugin.game);
		}
	}

	public override void BuildModOptions(uGUI_TabbedControlsPanel panel, int modsTabIndex, IReadOnlyCollection<OptionItem> options)
	{
		var presetConfigs = new List<ConfigPerSave> {
			ConfigPerSave.default_,
			_custom,
		};
		var presetChoices = new List<string>{
			"Default",
			"Custom",
		};
		var presetDefaultChoices = new List<string>{
			"Default"
		};
		const int customPresetIdx = 1;
		
		ConfigPerSave.loadPresets();
		foreach ( var entry in ConfigPerSave.presets ) {
			presetChoices.Add(entry.Key);
			presetConfigs.Add(entry.Value);
			presetDefaultChoices.Add(entry.Key);
		}

		panel.AddHeading(modsTabIndex, Name);
		panel.AddChoiceOption(
			tabIndex: modsTabIndex,
			label: "Default Preset",
			items: presetDefaultChoices.ToArray(),
			currentIndex: Math.Max(0, presetDefaultChoices.FindIndex(1, x => x == Plugin.config.presetDefault)),
			callback: (int idx) =>
				Plugin.config.presetDefault = idx == 0
					? ""
					: presetDefaultChoices[idx],
			tooltip: "Default preset to use when starting a new game save");
		options.ForEach(option => option.AddToPanel(panel, modsTabIndex));

		if ( !inGame )
			return;

		panel.AddHeading(modsTabIndex, Info.title + " Per Save Options");

		_custom.copySettings(Plugin.game);

		var presetIdx = 0;
		if ( !Plugin.game.equalSettings(ConfigPerSave.default_) ) {
			presetIdx = presetConfigs.FindIndex(customPresetIdx + 1, x => x.equalSettings(Plugin.game));
			if ( presetIdx == -1 )
				presetIdx = customPresetIdx;
		}

		var fields = ConfigPerSave.fieldsSettings;
		var sliders = new uGUI_SnappingSlider[fields.Count];

		bool suppressChangeEvent = false;
		UnityAction<int> onPreset = (int idx) => {
			if ( suppressChangeEvent )
				return;

			loadCurrent();
			suppressChangeEvent = true;
			for ( int i = 0; i < sliders.Length; ++i )
				sliders[i].value = readSliderField(presetConfigs[idx], fields[i]);
			suppressChangeEvent = false;

			_current.copySettings(presetConfigs[idx]);
		};
		var presetObject = panel.AddChoiceOption(modsTabIndex, "Preset", presetChoices.ToArray(), presetIdx, callback: onPreset)
			.GetComponentInChildren<uGUI_Choice>();

		for ( int fieldIdx = 0; fieldIdx < fields.Count; ++fieldIdx ) {
			switch ( fields[fieldIdx].GetCustomAttribute<ModOptionAttribute>() ) {
			case SliderAttribute attr:
				int sliderIdx = fieldIdx;
				UnityAction<float> onChange = (float x) => {
					if ( suppressChangeEvent )
						return;

					loadCurrent();
					if ( presetObject.value != customPresetIdx ) {
						suppressChangeEvent = true;
						presetObject.value = customPresetIdx;
						suppressChangeEvent = false;
						for ( int i = 0; i < sliders.Length; ++i ) {
							writeSliderField(_custom, fields[i], sliders[i].value);
							writeSliderField(_current, fields[i], sliders[i].value);
						}
					}
					writeSliderField(_custom, fields[sliderIdx], x);
					writeSliderField(_current, fields[sliderIdx], x);
				};
				sliders[sliderIdx] = panel.AddSliderOption(modsTabIndex, attr.Label, readSliderField(_custom, fields[sliderIdx]), attr.Min, attr.Max, attr.DefaultValue, attr.Step, onChange, typeToLabelMode(fields[sliderIdx].FieldType), attr.Format, attr.Tooltip)
					.GetComponentInChildren<uGUI_SnappingSlider>();
				break;
			}
		}
	}

	public void applyChanges()
	{
		if ( _current is null )
			return;

		Plugin.game.copySettings(_current);
		_current = null;

		Plugin.debug("Applying new game settings");

		applyAll();
	}

	public static void applyAll()
	{
		applyInventory();
		applyStorageContainer();
		applyBioreactor();
		applyFiltrationMachine();
		applyExosuit();
		applySeaTruck();
	}

	public static void applyInventory()
	{
		foreach ( var inv in Plugin.FindObjectsOfType<Inventory>() )
			inv.container.Resize(Plugin.game.inventory_width, Plugin.game.inventory_height);

		foreach ( var tab in Plugin.FindObjectsOfType<uGUI_InventoryTab>() )
			tab.inventory.Init(Inventory.main.container);
	}

	public static void applyStorageContainer()
	{
		foreach ( var s in Plugin.FindObjectsOfType<StorageContainer>() ) {
			Patch.StorageContainer_CreateContainer(s);
			s.Resize(s.width, s.height);
		}
	}

	public static void applyBioreactor()
	{
		foreach ( var b in Plugin.FindObjectsOfType<BaseBioReactor>() )
			Patch.BaseBioReactor_Start(b);
	}

	public static void applyFiltrationMachine()
	{
		foreach ( var f in Plugin.FindObjectsOfType<FiltrationMachine>() )
			Patch.FiltrationMachine_Start(f);
	}

	public static void applyExosuit()
	{
		foreach ( var e in Plugin.FindObjectsOfType<Exosuit>() ) {
			Plugin.debug($"Applying to {e.name}");
			Patch.Exosuit_UpdateStorageSize(e);
		}
	}

	public static void applySeaTruck()
	{
	}
}

[Menu(Info.title)]
public class ConfigGlobal
{
	public string presetDefault = "";

	[Slider("Inventory Max View Width", 4, 8, Step = 1, DefaultValue = 5), OnChange(nameof(syncScrollPanes))]
	public int inventoryMaxView_width = 5;

	[Slider("Inventory Max View Height", 6, 8, Step = 1, DefaultValue = 8), OnChange(nameof(syncScrollPanes))]
	public int inventoryMaxView_height = 8;

	[Slider("Storage Max View Width", 6, 8, Step = 1, DefaultValue = 8), OnChange(nameof(syncScrollPanes))]
	public int storageMaxView_width = 8;

	[Slider("Storage Max View Height", 6, 8, Step = 1, DefaultValue = 8), OnChange(nameof(syncScrollPanes))]
	public int storageMaxView_height = 8;

	[Slider("Scroll View Margin", 0, 256, DefaultValue = 20), OnChange(nameof(syncScrollPanes))]
	public float viewMargin = 20.0f;

	[Slider("Scroll Bar Size", 0, 256, DefaultValue = 20), OnChange(nameof(syncScrollPanes))]
	public float scrollbarSize = 20.0f;

	[Toggle("Reset Scroll when PDA Opens")]
	public bool resetInventoryScroll = true;

	[Slider("Inventory Scroll Pane Padding Left", -32, 32, DefaultValue = -7), OnChange(nameof(syncScrollPanes))]
	public float inventoryMaskPadding_left = -7;
	[Slider("Inventory Scroll Pane Padding Bottom", -32, 32, DefaultValue = -4), OnChange(nameof(syncScrollPanes))]
	public float inventoryMaskPadding_bottom = -4;
	[Slider("Inventory Scroll Pane Padding Right", -32, 32, DefaultValue = -4), OnChange(nameof(syncScrollPanes))]
	public float inventoryMaskPadding_right = -4;
	[Slider("Inventory Scroll Pane Padding Top", -32, 32, DefaultValue = -8), OnChange(nameof(syncScrollPanes))]
	public float inventoryMaskPadding_top = -8;
	
	[Slider("Storage Scroll Pane Padding Left", -32, 32, DefaultValue = -7), OnChange(nameof(syncScrollPanes))]
	public float storageMaskPadding_left = -7;
	[Slider("Storage Scroll Pane Padding Bottom", -32, 32, DefaultValue = -4), OnChange(nameof(syncScrollPanes))]
	public float storageMaskPadding_bottom = -4;
	[Slider("Storage Scroll Pane Padding Right", -32, 32, DefaultValue = -4), OnChange(nameof(syncScrollPanes))]
	public float storageMaskPadding_right = -4;
	[Slider("Storage Scroll Pane Padding Top", -32, 32, DefaultValue = -8), OnChange(nameof(syncScrollPanes))]
	public float storageMaskPadding_top = -8;

	// Hotkeys configs (for Below Zero)
	public KeyCode keyMoveAllItemType = KeyCode.LeftControl;
	public KeyCode keyMoveAllItems = KeyCode.LeftShift;
	public KeyCode keyPinItem = KeyCode.LeftAlt;

	[JsonIgnore]
	public string path
	{
		get {
			return Path.Combine(BepInEx.Paths.ConfigPath, Info.name, "config.json");
		}
	}

	public void load()
	{
		Util.loadJsonFile(this, path);
	}

	public void save()
	{
		Util.saveJsonFile(this, path);
	}

	private static readonly FieldInfo field_uGUI_ItemsContainer_container = typeof(uGUI_ItemsContainer).GetField("container", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
	public void syncItemsContainer(uGUI_ItemsContainer guiItemsContainer)
	{
		var isInventory = guiItemsContainer == guiItemsContainer.inventory.inventory;
		var m = guiItemsContainer.GetComponentInParent<RectMask2D>(true);
		if ( m ) {
			m.padding = isInventory
				? new Vector4(Plugin.config.inventoryMaskPadding_left, Plugin.config.inventoryMaskPadding_bottom, Plugin.config.inventoryMaskPadding_right, Plugin.config.inventoryMaskPadding_top)
				: new Vector4(Plugin.config.storageMaskPadding_left, Plugin.config.storageMaskPadding_bottom, Plugin.config.storageMaskPadding_right, Plugin.config.storageMaskPadding_top);
		}

		var scrollRect = guiItemsContainer.GetComponentInParent<ScrollRect>(true);
		if ( scrollRect ) {
			var r = scrollRect.verticalScrollbar.transform as RectTransform;
			r.sizeDelta = new Vector2(Plugin.config.scrollbarSize, 0);
			scrollRect.verticalScrollbar.enabled = r.sizeDelta.x != 0;
			scrollRect.verticalScrollbar.targetGraphic.gameObject.SetActive(scrollRect.verticalScrollbar.enabled);
			var container = field_uGUI_ItemsContainer_container.GetValue(guiItemsContainer) as ItemsContainer;
			if ( container != null ) {
				var contentSize = new Vector2int(container.sizeX, container.sizeY);
				var maxViewSize = isInventory
					? new Vector2int(Plugin.config.inventoryMaxView_width, Plugin.config.inventoryMaxView_height)
					: new Vector2int(Plugin.config.storageMaxView_width, Plugin.config.storageMaxView_height);
				ScrollPane.init(guiItemsContainer, scrollRect, contentSize, maxViewSize);
			}
		}
	}

	public void syncScrollPanes()
	{
		foreach ( var tab in Plugin.FindObjectsOfType<uGUI_InventoryTab>() ) {
			syncItemsContainer(tab.inventory);
			syncItemsContainer(tab.storage);
		}
	}

	public void syncScrollPanes(SliderChangedEventArgs evt)
	{
		syncScrollPanes();
	}
}

public class ConfigPerSave
{
	public static readonly List<FieldInfo> fieldsSettings = new List<FieldInfo>(
		from x in typeof(ConfigPerSave).GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).AsEnumerable()
		where x.GetCustomAttribute<SliderAttribute>() != null
		select x);

	public bool equalSettings(ConfigPerSave a)
	{
		foreach ( var field in fieldsSettings ) {
			if ( !field.GetValue(this).Equals(field.GetValue(a)) )
				return false;
		}

		return true;
	}

	public void copySettings(ConfigPerSave a)
	{
		foreach ( var field in fieldsSettings )
			field.SetValue(this, field.GetValue(a));
	}

	[JsonIgnore]
	public string path
	{
		get {
			var saveDir = SaveLoadManager.GetTemporarySavePath();
			if (string.IsNullOrEmpty(saveDir))
				return null;

			return Path.Combine(saveDir, Info.name, Info.name + ".json");
		}
	}

	public void load()
	{
		Util.loadJsonFile(this, path);
	}

	public void save()
	{
		var path = this.path;
		Util.saveJsonFile(this, path);
	}

	public static readonly ConfigPerSave default_ = new ConfigPerSave();
	public static DateTime presetsLastAccess;
	public static Dictionary<string, ConfigPerSave> presets = new Dictionary<string, ConfigPerSave>();
	public static bool loadPresets()
	{
		var presetsPath = Path.Combine(Path.GetDirectoryName(typeof(Plugin).Assembly.Location), "presets.json");
		if ( !File.Exists(presetsPath) ) {
			Plugin.log.LogWarning($"Missing presets: {presetsPath}");
			return false;
		}

		var lastAccess = new FileInfo(presetsPath).LastWriteTime;
		if ( lastAccess != presetsLastAccess ) {
			presetsLastAccess = lastAccess;
			try {
				var newPresets = new Dictionary<string, ConfigPerSave>();
				Util.loadJsonFile(newPresets, presetsPath);

				presets = newPresets;
			}
			catch ( Exception ex ) {
				Plugin.log.LogError($"Failed to load presets: {presetsPath}");
				Plugin.log.LogError(ex);
			}

			return true;
		}

		return false;
	}

	const int max = 256;

	public int version = 1;

	// inventory

	[Slider("Inventory Width" , 1, 8  , Step = 1, DefaultValue = 6  )] public int inventory_width  = 6;
	[Slider("Inventory Height", 1, max, Step = 1, DefaultValue = 8  )] public int inventory_height = 8;

	// lockers

	[Slider("Locker Width"            , 1, 8  , Step = 1, DefaultValue = 6  )] public int locker_width        = 6;
	[Slider("Locker Height"           , 1, max, Step = 1, DefaultValue = 8  )] public int locker_height       = 8;
	[Slider("Wall Locker Width"       , 1, 8  , Step = 1, DefaultValue = 5  )] public int smalllocker_width   = 5;
	[Slider("Wall Locker Height"      , 1, max, Step = 1, DefaultValue = 6  )] public int smalllocker_height  = 6;
	[Slider("Waterproof Locker Width" , 1, 8  , Step = 1, DefaultValue = 4  )] public int smallstorage_width  = 4;
	[Slider("Waterproof Locker Height", 1, max, Step = 1, DefaultValue = 4  )] public int smallstorage_height = 4;
	[Slider("Carry-all Width"         , 1, 8  , Step = 1, DefaultValue = 3  )] public int luggagebag_width    = 3;
	[Slider("Carry-all Height"        , 1, max, Step = 1, DefaultValue = 3  )] public int luggagebag_height   = 3;

	// SeaTruck (Below Zero)

	[Slider("SeaTruck Locker 1 Width" , 1, 8  , Step = 1, DefaultValue = 3  )] public int seatruckLocker1_width  = 3;
	[Slider("SeaTruck Locker 1 Height", 1, max, Step = 1, DefaultValue = 5  )] public int seatruckLocker1_height = 5;
	[Slider("SeaTruck Locker 2 Width" , 1, 8  , Step = 1, DefaultValue = 3  )] public int seatruckLocker2_width  = 3;
	[Slider("SeaTruck Locker 2 Height", 1, max, Step = 1, DefaultValue = 5  )] public int seatruckLocker2_height = 5;
	[Slider("SeaTruck Locker 3 Width" , 1, 8  , Step = 1, DefaultValue = 6  )] public int seatruckLocker3_width  = 6;
	[Slider("SeaTruck Locker 3 Height", 1, max, Step = 1, DefaultValue = 3  )] public int seatruckLocker3_height = 3;

	// vehicles

	[Slider("Prawn Suit Storage Width"             , 1, 8  , Step = 1, DefaultValue = 6  )] public int exosuit_width               = 6;
	[Slider("Prawn Suit Storage Height"            , 1, max, Step = 1, DefaultValue = 4  )] public int exosuit_height              = 4;
	[Slider("Prawn Suit Height Per Storage Module" , 1, max, Step = 1, DefaultValue = 1  )] public int exosuitStorageModule_height = 1;

	// bases

	[Slider("Scanner Room Upgrades Width" , 1, 8  , Step = 1, DefaultValue = 2  )] public int maproomupgrades_width          = 2;
	[Slider("Scanner Room Upgrades Height", 1, max, Step = 1, DefaultValue = 2  )] public int maproomupgrades_height         = 2;
	[Slider("Bioreactor Width"            , 1, 8  , Step = 1, DefaultValue = 4  )] public int basebioreactor_width           = 4;
	[Slider("Bioreactor Height"           , 1, max, Step = 1, DefaultValue = 4  )] public int basebioreactor_height          = 4;
	[Slider("Filtration Machine Max Salt" , 1, max, Step = 1, DefaultValue = 2  )] public int basefiltrationMachine_maxSalt  = 2;
	[Slider("Filtration Machine Max Water", 1, max, Step = 1, DefaultValue = 2  )] public int basefiltrationMachine_maxWater = 2;

	// trash

	[Slider("Nuclear Waste Disposal Width" , 1, 8  , Step = 1, DefaultValue = 3  )] public int labtrashcan_width  = 3;
	[Slider("Nuclear Waste Disposal Height", 1, max, Step = 1, DefaultValue = 4  )] public int labtrashcan_height = 4;
	[Slider("Trash Can Width"              , 1, 8  , Step = 1, DefaultValue = 4  )] public int trashcans_width    = 4;
	[Slider("Trash Can Height"             , 1, max, Step = 1, DefaultValue = 5  )] public int trashcans_height   = 5;

	// planters

	[Slider("Basic Plant Pot Width"           , 1, 8  , Step = 1, DefaultValue = 2  )] public int planterpot_width    = 2;
	[Slider("Basic Plant Pot Height"          , 1, max, Step = 1, DefaultValue = 2  )] public int planterpot_height   = 2;
	[Slider("Composite Plant Pot Width"       , 1, 8  , Step = 1, DefaultValue = 2  )] public int planterpot2_width   = 2;
	[Slider("Composite Plant Pot Height"      , 1, max, Step = 1, DefaultValue = 2  )] public int planterpot2_height  = 2;
	[Slider("Chic Plant Pot Width"            , 1, 8  , Step = 1, DefaultValue = 2  )] public int planterpot3_width   = 2;
	[Slider("Chic Plant Pot Height"           , 1, max, Step = 1, DefaultValue = 2  )] public int planterpot3_height  = 2;
	[Slider("Plant Shelf Width"               , 1, 8  , Step = 1, DefaultValue = 1  )] public int plantershelf_width  = 1;
	[Slider("Plant Shelf Height"              , 1, max, Step = 1, DefaultValue = 1  )] public int plantershelf_height = 1;
	[Slider("Exterior Growbed Width"          , 1, 8  , Step = 1, DefaultValue = 4  )] public int farmingtray_width   = 4;
	[Slider("Exterior Growbed Height"         , 1, max, Step = 1, DefaultValue = 6  )] public int farmingtray_height  = 6;
	[Slider("Indoor Growbed Width"            , 1, 8  , Step = 1, DefaultValue = 4  )] public int planterbox_width    = 4;
	[Slider("Indoor Growbed Height"           , 1, max, Step = 1, DefaultValue = 4  )] public int planterbox_height   = 4;
	[Slider("Alien Containment Planter Width" , 1, 8  , Step = 1, DefaultValue = 4  )] public int planter_width       = 4;
	[Slider("Alien Containment Planter Height", 1, max, Step = 1, DefaultValue = 4  )] public int planter_height      = 4;

	// pinned inventory

	public HashSet<string> inventoryPinnedItems = new HashSet<string>();
}

public static class Commands
{
	[ConsoleCommand("set_inventory_width"               )] public static void set_inventory_width               (int x) { Plugin.game.inventory_width                = x; OptionsMenu.applyInventory(); }
	[ConsoleCommand("set_inventory_height"              )] public static void set_inventory_height              (int x) { Plugin.game.inventory_height               = x; OptionsMenu.applyInventory(); }
	[ConsoleCommand("set_locker_width"                  )] public static void set_locker_width                  (int x) { Plugin.game.locker_width                   = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_locker_height"                 )] public static void set_locker_height                 (int x) { Plugin.game.locker_height                  = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_smalllocker_width"             )] public static void set_smalllocker_width             (int x) { Plugin.game.smalllocker_width              = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_smalllocker_height"            )] public static void set_smalllocker_height            (int x) { Plugin.game.smalllocker_height             = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_smallstorage_width"            )] public static void set_smallstorage_width            (int x) { Plugin.game.smallstorage_width             = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_smallstorage_height"           )] public static void set_smallstorage_height           (int x) { Plugin.game.smallstorage_height            = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_luggagebag_width"              )] public static void set_luggagebag_width              (int x) { Plugin.game.luggagebag_width               = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_luggagebag_height"             )] public static void set_luggagebag_height             (int x) { Plugin.game.luggagebag_height              = x; OptionsMenu.applyStorageContainer(); }

	[ConsoleCommand("set_seatruck_locker1_width"        )] public static void set_seatruck_locker1_width        (int x) { Plugin.game.seatruckLocker1_width          = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_seatruck_locker1_height"       )] public static void set_seatruck_locker1_height       (int x) { Plugin.game.seatruckLocker1_height         = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_seatruck_locker2_width"        )] public static void set_seatruck_locker2_width        (int x) { Plugin.game.seatruckLocker2_width          = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_seatruck_locker2_height"       )] public static void set_seatruck_locker2_height       (int x) { Plugin.game.seatruckLocker2_height         = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_seatruck_locker3_width"        )] public static void set_seatruck_locker3_width        (int x) { Plugin.game.seatruckLocker3_width          = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_seatruck_locker3_height"       )] public static void set_seatruck_locker3_height       (int x) { Plugin.game.seatruckLocker3_height         = x; OptionsMenu.applyStorageContainer(); }

	[ConsoleCommand("set_exosuit_width"                 )] public static void set_exosuit_width                  (int x) { Plugin.game.exosuit_width                  = x; OptionsMenu.applyExosuit(); }
	[ConsoleCommand("set_exosuit_height"                )] public static void set_exosuit_height                 (int x) { Plugin.game.exosuit_height                 = x; OptionsMenu.applyExosuit(); }
	[ConsoleCommand("set_exosuitStorageModule_height"   )] public static void set_exosuitStorageModule_height    (int x) { Plugin.game.exosuitStorageModule_height    = x; OptionsMenu.applyExosuit(); }
	[ConsoleCommand("set_maproomupgrades_width"         )] public static void set_maproomupgrades_width         (int x) { Plugin.game.maproomupgrades_width          = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_maproomupgrades_height"        )] public static void set_maproomupgrades_height        (int x) { Plugin.game.maproomupgrades_height         = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_basebioreactor_width"          )] public static void set_basebioreactor_width          (int x) { Plugin.game.basebioreactor_width           = x; OptionsMenu.applyBioreactor(); }
	[ConsoleCommand("set_basebioreactor_height"         )] public static void set_basebioreactor_height         (int x) { Plugin.game.basebioreactor_height          = x; OptionsMenu.applyBioreactor(); }
	[ConsoleCommand("set_basefiltrationMachine_maxSalt" )] public static void set_basefiltrationMachine_maxSalt (int x) { Plugin.game.basefiltrationMachine_maxSalt  = x; OptionsMenu.applyFiltrationMachine(); }
	[ConsoleCommand("set_basefiltrationMachine_maxWater")] public static void set_basefiltrationMachine_maxWater(int x) { Plugin.game.basefiltrationMachine_maxWater = x; OptionsMenu.applyFiltrationMachine(); }
	[ConsoleCommand("set_labtrashcan_width"             )] public static void set_labtrashcan_width             (int x) { Plugin.game.labtrashcan_width              = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_labtrashcan_height"            )] public static void set_labtrashcan_height            (int x) { Plugin.game.labtrashcan_height             = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_trashcans_width"               )] public static void set_trashcans_width               (int x) { Plugin.game.trashcans_width                = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_trashcans_height"              )] public static void set_trashcans_height              (int x) { Plugin.game.trashcans_height               = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_planterpot_width"              )] public static void set_planterpot_width              (int x) { Plugin.game.planterpot_width               = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_planterpot_height"             )] public static void set_planterpot_height             (int x) { Plugin.game.planterpot_height              = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_planterpot2_width"             )] public static void set_planterpot2_width             (int x) { Plugin.game.planterpot2_width              = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_planterpot2_height"            )] public static void set_planterpot2_height            (int x) { Plugin.game.planterpot2_height             = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_planterpot3_width"             )] public static void set_planterpot3_width             (int x) { Plugin.game.planterpot3_width              = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_planterpot3_height"            )] public static void set_planterpot3_height            (int x) { Plugin.game.planterpot3_height             = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_plantershelf_width"            )] public static void set_plantershelf_width            (int x) { Plugin.game.plantershelf_width             = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_plantershelf_height"           )] public static void set_plantershelf_height           (int x) { Plugin.game.plantershelf_height            = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_farmingtray_width"             )] public static void set_farmingtray_width             (int x) { Plugin.game.farmingtray_width              = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_farmingtray_height"            )] public static void set_farmingtray_height            (int x) { Plugin.game.farmingtray_height             = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_planterbox_width"              )] public static void set_planterbox_width              (int x) { Plugin.game.planterbox_width               = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_planterbox_height"             )] public static void set_planterbox_height             (int x) { Plugin.game.planterbox_height              = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_planter_width"                 )] public static void set_planter_width                 (int x) { Plugin.game.planter_width                  = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_planter_height"                )] public static void set_planter_height                (int x) { Plugin.game.planter_height                 = x; OptionsMenu.applyStorageContainer(); }
}

public static class Util
{
	public static void loadJsonFile<T>(T dst, string path)
	{
		if ( path is null )
			return;

		if ( !File.Exists(path) )
			return;

		try {
			using ( var stream = new FileStream(path, FileMode.Open, FileAccess.Read) )
			using ( var reader = new StreamReader(stream) )
			using ( var jsonReader = new JsonTextReader(reader) ) {
				var serializer = new JsonSerializer();
				serializer.Populate(jsonReader, dst);
			}
		} catch ( Exception ex ) {
			Plugin.log.LogError($"Failed to load json file: {path}");
			Plugin.log.LogError(ex);
		}
	}

	public static void saveJsonFile<T>(T dst, string path)
	{
		if ( path is null )
			return;

		var tmp = path + ".tmp";
		try {
			{
				var dir = Path.GetDirectoryName(tmp);
				if ( !string.IsNullOrEmpty(dir) )
					Directory.CreateDirectory(dir);
			}

			using ( var stream = new FileStream(tmp, FileMode.Create, FileAccess.Write) )
			using ( var writer = new StreamWriter(stream) )
			using ( var jsonWriter = new JsonTextWriter(writer) ) {
				jsonWriter.Formatting = Formatting.Indented;
				jsonWriter.IndentChar = '\t';
				jsonWriter.Indentation = 1;

				var serializer = new JsonSerializer();
				serializer.Serialize(jsonWriter, dst);
			}

			if ( File.Exists(path) )
				File.Replace(tmp, path, null);
			else
				File.Move(tmp, path);
		} catch ( Exception ex ) {
			Plugin.log.LogError($"Failed to save game data: {path}");
			Plugin.log.LogError(ex);
			if ( File.Exists(tmp) )
				File.Delete(tmp);
		}
	}
}

} // namespace org.efool.subnautica_bz.custom_inventory
