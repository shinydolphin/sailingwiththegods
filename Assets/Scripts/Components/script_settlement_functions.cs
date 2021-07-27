using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using NaughtyAttributes;
using System.Collections;


// TODO: Rename to SettlementComponent or something
public class script_settlement_functions : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	bool IsTooltipPendingHide;

	public Transform anchor;
	public Settlement thisSettlement;
	World World;

	// debug display of settlement id so you can easily find them
	[ShowNativeProperty] int SettlementId => thisSettlement != null ? thisSettlement.settlementID : -1;

	
	void Start() {
		World = Globals.World;
	}

	// TODO: Remove this once we're sure we don't want any of this selection_ring code
	/*
	public void ActivateHighlightOnMouseOver() {
		//child selection ring to current settlement
		World.selection_ring.transform.SetParent(transform);
		//set the ring to the origin coordinates of the settlement
		World.selection_ring.transform.localPosition = new Vector3(0, 2, 0);
		//turn the ring on
		World.selection_ring.SetActive(true);

		Session.currentSettlementGameObject = gameObject;
		Session.currentSettlement = thisSettlement;

	}
	*/

	public void OnPointerEnter(PointerEventData eventData) {
		TryShowTooltip();
	}

	public void OnPointerExit(PointerEventData eventData) {
		IsTooltipPendingHide = !TryHideTooltip(false);
	}

	void TryShowTooltip() {
		if (!Globals.UI.IsShown<CityView>() && !Globals.Game.Session.IsCutsceneMode) {
			var ui = Globals.UI.Show<CityView, CityViewModel>(new CityDetailsViewModel(thisSettlement, null));
			ui.transform.position = Globals.UI.WorldToUI(World.FPVCamera.GetComponent<Camera>(), transform.position);
		}
	}

	bool TryHideTooltip(bool requirePendingHide) {

		// allow he pointer to hover over the tooltip without closing it so we can make it clickable and not flicker on the edge
		if (Globals.UI.IsShown<CityView>() && UISystem.IsMouseOverUI(Globals.UI.Get<CityView>().GetComponent<Graphic>())) {
			return false;
		}
		else if(IsTooltipPendingHide || !requirePendingHide) {
			Globals.UI.Hide<CityView>();
			IsTooltipPendingHide = false;
			return true;
		}
		else {
			return false;
		}

	}

	void Update() {

		// hide the tooltip after mousing over it and mousing off (so we missed settlement.OnPointerExit)
		TryHideTooltip(true);

	}
}
