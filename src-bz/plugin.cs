using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

using Nautilus.Handlers;
using Nautilus.Utility;

namespace org.efool.subnautica_bz.custom_inventory {

[BepInPlugin(
	org.efool.subnautica_bz.custom_inventory.Info.FQN,
	org.efool.subnautica_bz.custom_inventory.Info.title,
	org.efool.subnautica_bz.custom_inventory.Info.version)]
[BepInDependency("com.snmodding.nautilus")]
public class Plugin : BaseUnityPlugin
{
	public static ConfigGlobal config { get; private set;}
	public static OptionsMenu optionsMenu { get; private set; }
	public static ConfigPerSave game { get; private set; }

	public static ManualLogSource log;
	public static void debug(string txt)
	{
#if DEBUG
		log.LogDebug(txt);
#endif
	}

	private void Awake()
	{
		log = Logger;

		config = new ConfigGlobal();
		config.load();

		optionsMenu = new OptionsMenu(config);
		OptionsPanelHandler.RegisterModOptions(optionsMenu);

		WaitScreenHandler.RegisterEarlyLoadTask(org.efool.subnautica_bz.custom_inventory.Info.title, (_) => {
			game = new ConfigPerSave();
			game.load();
		});
		WaitScreenHandler.RegisterLateLoadTask(
			org.efool.subnautica_bz.custom_inventory.Info.title,
			(_) => {
				if ( SaveLoadManager.main.timePlayedTotal == 0 && !config.presetDefault.IsNullOrWhiteSpace() ) {
					ConfigPerSave.loadPresets();
					game.copySettings(ConfigPerSave.presets.GetOrDefault(config.presetDefault, ConfigPerSave.default_));
					Plugin.config.syncScrollPanes();
					OptionsMenu.applyAll();
					debug("Applied presets to new save");
				}
			});
		SaveUtils.RegisterOnSaveEvent(() => game.save());

		ConsoleCommandsHandler.RegisterConsoleCommands(typeof(Commands));

		SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(PinItem.evtSceneLoaded);

		new Harmony(org.efool.subnautica_bz.custom_inventory.Info.FQN).PatchAll();
	}
}

[HarmonyPatch]
static class Patch
{
	[HarmonyPostfix]
	[HarmonyPatch(typeof(uGUI_OptionsPanel), nameof(uGUI_OptionsPanel.OnSave))]
	public static void uGUI_OptionsPanel_OnSave(uGUI_OptionsPanel __instance, UserStorageUtils.SaveOperation saveOperation)
	{
		Plugin.config.save();
		Plugin.config.syncScrollPanes();
		Plugin.optionsMenu.applyChanges();
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Inventory), "Awake")]
	public static void Inventory_Awake(Inventory __instance)
	{
		__instance.container.Resize(Plugin.game.inventory_width, Plugin.game.inventory_height);
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(StorageContainer), "CreateContainer")]
	public static void StorageContainer_CreateContainer(StorageContainer __instance)
	{
		string name = __instance.name;
		string prefabName = __instance.prefabRoot != null ? __instance.prefabRoot.name : "";

		if ( name.StartsWith("SmallLocker") ) {
			// wall locker
			__instance.width = Plugin.game.smalllocker_width;
			__instance.height = Plugin.game.smalllocker_height;
		}
		else if ( name.StartsWith("Locker") ) {
			// locker
			__instance.width = Plugin.game.locker_width;
			__instance.height = Plugin.game.locker_height;
		}
		else if ( prefabName.StartsWith("SmallStorage") ) {
			// waterproof locker
			__instance.width = Plugin.game.smallstorage_width;
			__instance.height = Plugin.game.smallstorage_height;
		}
		else if ( name.StartsWith("storage") ) {
			// carry-all
			__instance.width = Plugin.game.luggagebag_width;
			__instance.height = Plugin.game.luggagebag_height;
		}
		else if ( name.Contains("seatruck") || name.Contains("SeaTruck") || prefabName.Contains("seatruck") || prefabName.Contains("SeaTruck") ) {
			// SeaTruck lockers (Below Zero)
			if ( __instance.width == 3 && __instance.height == 5 ) {
				// Locker 1 or Locker 2 (default 3x5)
				__instance.width = Plugin.game.seatruckLocker1_width;
				__instance.height = Plugin.game.seatruckLocker1_height;
			}
			else if ( __instance.width == 6 && __instance.height == 3 ) {
				// Locker 3 (default 6x3)
				__instance.width = Plugin.game.seatruckLocker3_width;
				__instance.height = Plugin.game.seatruckLocker3_height;
			}
			else {
				// Fallback/unknown seatruck storage
				__instance.width = Plugin.game.seatruckLocker1_width;
				__instance.height = Plugin.game.seatruckLocker1_height;
			}
		}
		else if ( name.StartsWith("Trashcans") ) {
			// trash can
			__instance.width = Plugin.game.trashcans_width;
			__instance.height = Plugin.game.trashcans_height;
		}
		else if ( name.StartsWith("LabTrashcan") ) {
			// nuclear waste can
			__instance.width = Plugin.game.labtrashcan_width;
			__instance.height = Plugin.game.labtrashcan_height;
		}
		else if ( name.StartsWith("FiltrationMachine") ) {
			// filtration machine
		}
		else if ( prefabName.StartsWith("Exosuit") ) {
			// prawn storage
			__instance.width = Plugin.game.exosuit_width;
			__instance.height = Plugin.game.exosuit_height;
		}
		else if ( name.StartsWith("MapRoomUpgrades") ) {
			// scanner room upgrades
			__instance.width = Plugin.game.maproomupgrades_width;
			__instance.height = Plugin.game.maproomupgrades_height;
		}
		else if ( name.StartsWith("PlanterPot3") ) {
			// planter 3
			__instance.width = Plugin.game.planterpot3_width;
			__instance.height = Plugin.game.planterpot3_height;
		}
		else if ( name.StartsWith("PlanterPot2") ) {
			// planter 2
			__instance.width = Plugin.game.planterpot2_width;
			__instance.height = Plugin.game.planterpot2_height;
		}
		else if ( name.StartsWith("PlanterPot") ) {
			// planter
			__instance.width = Plugin.game.planterpot_width;
			__instance.height = Plugin.game.planterpot_height;
		}
		else if ( name.StartsWith("PlanterShelf") ) {
			// shelf planter
			__instance.width = Plugin.game.plantershelf_width;
			__instance.height = Plugin.game.plantershelf_height;
		}
		else if ( name.StartsWith("FarmingTray") ) {
			// farming tray
			__instance.width = Plugin.game.farmingtray_width;
			__instance.height = Plugin.game.farmingtray_height;
		}
		else if ( name.StartsWith("PlanterBox") ) {
			// planter box
			__instance.width = Plugin.game.planterbox_width;
			__instance.height = Plugin.game.planterbox_height;
		}
		else if ( name.StartsWith("planter") ) {
			// alien containment planter
			__instance.width = Plugin.game.planter_width;
			__instance.height = Plugin.game.planter_height;
		}
#if DEBUG
		else {
			Plugin.debug("StorageContainer_CreateContainer Unknown container: " + name + " (prefabRoot: " + prefabName + ") default size: " + __instance.width + "x" + __instance.height);
		}
#endif
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(BaseBioReactor), "Start")]
	public static void BaseBioReactor_Start(BaseBioReactor __instance)
	{
		(AccessTools.PropertyGetter(typeof(BaseBioReactor), "container").Invoke(__instance, null) as ItemsContainer).Resize(Plugin.game.basebioreactor_width, Plugin.game.basebioreactor_height);
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(FiltrationMachine), "Start")]
	public static void FiltrationMachine_Start(FiltrationMachine __instance)
	{
		__instance.maxSalt = Plugin.game.basefiltrationMachine_maxSalt;
		__instance.maxWater = Plugin.game.basefiltrationMachine_maxWater;

		int maxWidth = Plugin.config.storageMaxView_width;
		int total = __instance.maxSalt + __instance.maxWater;
		int sqroot = (int)Math.Ceiling(Math.Sqrt(total));
		if ( sqroot <= maxWidth ) {
			__instance.storageContainer.width = sqroot;
			__instance.storageContainer.height = sqroot;
		}
		else {
			__instance.storageContainer.width = maxWidth;
			__instance.storageContainer.height = (total + maxWidth - 1) / maxWidth;
		}
		__instance.storageContainer.Resize(__instance.storageContainer.width, __instance.storageContainer.height);
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Exosuit), "UpdateStorageSize")]
	public static void Exosuit_UpdateStorageSize(Exosuit __instance)
	{
		__instance.storageContainer.Resize(
			Plugin.game.exosuit_width,
			Plugin.game.exosuit_height
				+ (Plugin.game.exosuitStorageModule_height * __instance.modules.GetCount(TechType.VehicleStorageModule)));
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(uGUI_InventoryTab), nameof(uGUI_InventoryTab.OnOpenPDA))]
	public static void uGUI_InventoryTab_OnOpenPDA(uGUI_InventoryTab __instance)
	{
		if ( __instance.inventory != null )
			Plugin.config.syncItemsContainer(__instance.inventory);
		if ( __instance.storage != null )
			Plugin.config.syncItemsContainer(__instance.storage);

		if ( Plugin.config.resetInventoryScroll ) {
			var scrollRect = __instance.inventory.GetComponentInParent<ScrollRect>(true);
			if ( scrollRect ) {
				scrollRect.verticalNormalizedPosition = 1f;
				scrollRect.horizontalNormalizedPosition = 0f;
			}
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(uGUI_ItemsContainer), nameof(uGUI_ItemsContainer.Init))]
	public static void uGUI_ItemsContainer_Init(uGUI_ItemsContainer __instance, ItemsContainer ___container)
	{
		var isInventory = __instance == __instance.inventory.inventory;
		var isStorage = __instance == __instance.inventory.storage;
		if ( isInventory || isStorage ) {
			var contentSize = new Vector2int(___container.sizeX, ___container.sizeY);
			var maxViewSize = isInventory
				? new Vector2int(Plugin.config.inventoryMaxView_width, Plugin.config.inventoryMaxView_height)
				: new Vector2int(Plugin.config.storageMaxView_width, Plugin.config.storageMaxView_height);

			var scrollRect = __instance.GetComponentInParent<ScrollRect>(true);
			if ( scrollRect is null )
				scrollRect = ScrollPane.inject(__instance, "ScrollPane");
			else
				scrollRect.gameObject.SetActive(true);

			ScrollPane.init(__instance, scrollRect, contentSize, maxViewSize);
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(uGUI_ItemsContainer), nameof(uGUI_ItemsContainer.Uninit))]
	public static void uGUI_ItemsContainer_Uninit(uGUI_ItemsContainer __instance, ItemsContainer ___container)
	{
		__instance.GetComponentInParent<ScrollRect>(true)?.gameObject.SetActive(false);
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(uGUI_ItemsContainer), "OnAddItem")]
	public static void uGUI_ItemsContainer_OnAddItem(uGUI_ItemsContainer __instance, InventoryItem item)
	{
		PinItem.applyEffectPin(__instance, item, PinItem.isPinned(item));
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(uGUI_InventoryTab), nameof(uGUI_InventoryTab.OnPointerClick))]
	public static bool uGUI_InventoryTab_OnPointerClick(uGUI_InventoryTab __instance, InventoryItem item, int button)
	{
		if ( ItemDragManager.isDragging )
			return false;

		var itemAction = Inventory.main.GetItemAction(item, button);
		if ( UnityEngine.Input.GetKey(Plugin.config.keyPinItem) ) {
			if ( button == 0 )
				PinItem.handlePinItemAction(item);
		}
		else if ( itemAction != ItemAction.None && !PinItem.isPinned(item) ) {
			PinItem.handleItemAction(item, itemAction);
		}

		__instance.equipment.ExtinguishSlots();

		if ( itemAction != ItemAction.None || GameInput.GetPrimaryDevice() != GameInput.Device.Controller || button != 1 )
			return false;

		Player.main.GetPDA().Close();
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(InventoryItem), nameof(InventoryItem.CanDrag))]
	public static bool InventoryItem_CanDrag(ref bool __result, InventoryItem __instance, bool verbose)
	{
		if ( PinItem.isPinned(__instance) ) {
			__result = false;
			return false;
		}

		return true;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Inventory), nameof(Inventory.GetAllItemActions))]
	public static void Inventory_GetAllItemActions(ref ItemAction __result, Inventory __instance, InventoryItem item)
	{
		const ItemAction disallowed = ItemAction.Use
			| ItemAction.Eat
			| ItemAction.Equip
			| ItemAction.Unequip
			| ItemAction.Switch
			| ItemAction.Swap
			| ItemAction.Drop;
		if ( PinItem.isPinned(item) )
			__result = __result & ~disallowed;
	}
}

public static class ScrollPane
{
	public static Vector2 viewportSize(Vector2int contentSize, Vector2int maxViewSize)
	{
		var w = Math.Min(contentSize.x, maxViewSize.x);
		var h = Math.Min(contentSize.y, maxViewSize.y);
		return new Vector2(w * uGUI_ItemsContainer.CellWidth + Plugin.config.viewMargin, h * uGUI_ItemsContainer.CellHeight + Plugin.config.viewMargin);
	}

	public static ScrollRect inject(uGUI_ItemsContainer guiItemsContainer, string name)
	{
		GameObject scrollObject = new GameObject() { name = name };
		scrollObject.transform.SetParent(guiItemsContainer.transform.parent);
		var viewport = scrollObject.AddComponent<RectTransform>();
		viewport.CopyLocals(guiItemsContainer.rectTransform);
		guiItemsContainer.transform.SetParent(scrollObject.transform);

		{
			var m = scrollObject.AddComponent<RectMask2D>();
			m.padding = guiItemsContainer == guiItemsContainer.inventory.inventory
				? new Vector4(Plugin.config.inventoryMaskPadding_left, Plugin.config.inventoryMaskPadding_bottom, Plugin.config.inventoryMaskPadding_right, Plugin.config.inventoryMaskPadding_top)
				: new Vector4(Plugin.config.storageMaskPadding_left, Plugin.config.storageMaskPadding_bottom, Plugin.config.storageMaskPadding_right, Plugin.config.storageMaskPadding_top);
		}

		var scrollRect = scrollObject.AddComponent<ScrollRect>();
		scrollRect.movementType = ScrollRect.MovementType.Clamped;
		scrollRect.horizontal = true;
		scrollRect.viewport = viewport;
		scrollRect.content = guiItemsContainer.rectTransform;
		scrollRect.scrollSensitivity = uGUI_ItemsContainer.CellHeight;

		scrollRect.verticalScrollbarSpacing = 0;
		scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
		scrollRect.verticalScrollbar = UnityEngine.Object.Instantiate<GameObject>((Player.main.GetPDA().ui.tabEncyclopedia as uGUI_EncyclopediaTab).contentScrollRect.verticalScrollbar.gameObject, scrollRect.viewport).GetComponent<Scrollbar>();
		{
			var r = scrollRect.verticalScrollbar.transform as RectTransform;
			r.anchorMin        = new Vector2(1, 0);
			r.anchorMax        = new Vector2(1, 1);
			r.pivot            = new Vector2(1, 1);
			r.sizeDelta        = new Vector2(Plugin.config.scrollbarSize, 0);
			r.anchoredPosition = new Vector2(0, 0);

			scrollRect.verticalScrollbar.enabled = r.sizeDelta.x != 0;
			scrollRect.verticalScrollbar.targetGraphic.gameObject.SetActive(scrollRect.verticalScrollbar.enabled);
		}

		return scrollRect;
	}

	public static void init(uGUI_ItemsContainer guiItemsContainer, ScrollRect scrollRect, Vector2int contentSize, Vector2int maxViewSize)
	{
		scrollRect.viewport.sizeDelta = viewportSize(contentSize, maxViewSize);
		
		// In Below Zero, forcing pivot changes to (0.5f, 1) breaks icon positioning,
		// because the game calculates their coordinates relative to the original pivot (typically 0.5f, 0.5f).
		// Therefore, we keep the original anchors and pivot of the content.
		// We only reset anchoredPosition to Vector2.zero to let ScrollRect correctly initialize scrolling.
		guiItemsContainer.rectTransform.anchoredPosition = Vector2.zero;

		scrollRect.verticalNormalizedPosition = 1f;
		scrollRect.horizontalNormalizedPosition = 0f;
	}
}

public static class PinItem
{
	public static GameObject _pinReference;

	public static string getId(InventoryItem item)
	{
		return item.item.GetComponent<UniqueIdentifier>().Id;
	}

	public static bool isPinned(InventoryItem item)
	{
		return Plugin.game.inventoryPinnedItems.Contains(getId(item));
	}

	public static void pin(InventoryItem item)
	{
		Plugin.game.inventoryPinnedItems.Add(getId(item));
	}

	public static void unpin(InventoryItem item)
	{
		Plugin.game.inventoryPinnedItems.Remove(getId(item));
	}

	public static void evtSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if ( _pinReference == null && Player.main != null ) {
			var tab = Player.main.GetPDA().ui.tabJournal as uGUI_BlueprintsTab;
			var pin = tab.prefabPin.gameObject.FindChild("Pin");
			_pinReference = UnityEngine.Object.Instantiate<GameObject>(pin);
			_pinReference.name = "Pin";
			var x = _pinReference.transform as RectTransform;
			x.anchorMin     = new Vector2(0.1f, 1);
			x.anchorMax     = new Vector2(0.1f, 1);
			x.pivot         = new Vector2(0.1f, 1);
			x.offsetMin     = new Vector2(0, -30);
			x.offsetMax     = new Vector2(35, 0);
			x.localPosition = new Vector3(0, 0, 0);
			_pinReference.SetActive(false);
		}
	}

	public static void applyEffectPin(uGUI_ItemsContainer container, InventoryItem item, bool on = true)
	{
		if ( _pinReference == null )
			return;

		var icon = container.GetIcon(item);
		if ( icon == null )
			return;

		if ( on ) {
			RectTransform rectTransform = UnityEngine.Object.Instantiate<GameObject>(_pinReference, icon.rectTransform).GetComponent<RectTransform>();
			rectTransform.gameObject.name = "Pin";
			rectTransform.gameObject.SetActive(true);
		}
		else {
			var pin = icon.gameObject.FindChild("Pin");
			if ( pin != null )
				GameObject.Destroy(pin);
		}
	}

	public static void handlePinItemAction(InventoryItem item)
	{
		if ( !item.isEnabled )
			return;

		var container = item.container as ItemsContainer;
		if ( item.container is Equipment )
			return;

		var itemPinned = isPinned(item);
		var targetItems = new List<InventoryItem>();
		if ( UnityEngine.Input.GetKey(Plugin.config.keyMoveAllItems) ) {
			foreach ( var e in container )
				if ( isPinned(e) == itemPinned )
					targetItems.Add(e);
		}
		else if ( UnityEngine.Input.GetKey(Plugin.config.keyMoveAllItemType) ) {
			foreach ( var e in container.GetItems(item.item.GetTechType()) )
				if ( isPinned(e) == itemPinned )
					targetItems.Add(e);
		}
		else {
			targetItems.Add(item);
		}

		if ( itemPinned ) {
			foreach ( var e in targetItems ) {
				unpin(e);
				foreach ( var c in Plugin.FindObjectsOfType<uGUI_ItemsContainer>() )
					applyEffectPin(c, e, false);
			}
		}
		else {
			foreach ( var e in targetItems ) {
				pin(e);
				foreach ( var c in Plugin.FindObjectsOfType<uGUI_ItemsContainer>() )
					applyEffectPin(c, e, true);
			}
		}
	}

	public static void handleItemAction(InventoryItem item, ItemAction itemAction)
	{
		var targetItems = new List<InventoryItem>();
		if ( UnityEngine.Input.GetKey(Plugin.config.keyMoveAllItems) ) {
			foreach ( var e in item.container )
				if ( !isPinned(e) )
					targetItems.Add(e);
		}
		else {
			var container = item.container as ItemsContainer;
			if ( container != null && UnityEngine.Input.GetKey(Plugin.config.keyMoveAllItemType) ) {
				foreach ( var e in container.GetItems(item.item.GetTechType()) )
					if ( !isPinned(e) )
						targetItems.Add(e);
			}
			else {
				targetItems.Add(item);
			}
		}

		foreach ( var e in targetItems )
			Inventory.main.ExecuteItemAction(itemAction, e);
	}
}

} // namespace org.efool.subnautica_bz.custom_inventory
