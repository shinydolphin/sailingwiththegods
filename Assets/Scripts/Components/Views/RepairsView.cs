using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class RepairsViewModel : Model
{
	World World => Globals.World;
	GameSession Session => Globals.Game.Session;
	Notifications Notifications => Globals.Notifications;

	public int costToRepair { get; private set; }
	public int costToBuyUpgrade => 10000;			// TODO: Drive with something.

	public BoundModel<float> shipHealth { get; private set; }
	public BoundModel<int> shipLevel { get; private set; }

	public RepairsViewModel() {

		//We need to do a clout check as well as a network checks
		int baseModifier = Mathf.CeilToInt(2 - Session.GetOverallCloutModifier(Session.currentSettlement.settlementID));
		if (Session.Network.CheckIfCityIDIsPartOfNetwork(Session.currentSettlement.settlementID)) {
			costToRepair = Mathf.CeilToInt(Session.currentSettlement.tax_network * baseModifier * 1);
		}
		else {
			costToRepair = Mathf.CeilToInt(Session.currentSettlement.tax_neutral * baseModifier * 1);
		}

		shipHealth = new BoundModel<float>(Session.playerShipVariables.ship, nameof(Session.playerShipVariables.ship.health));
		shipLevel = new BoundModel<int>(Session.playerShipVariables.ship, nameof(Session.playerShipVariables.ship.upgradeLevel));

	}

	public void GUI_RepairShipByOneHP() {
		Session.playerShipVariables.ship.health += 1f;
		//make sure the hp can't go above 100
		if (Session.playerShipVariables.ship.health > 100) {
			Session.playerShipVariables.ship.health = 100;
			Notifications.ShowANotificationMessage("Your ship is already fully repaired");
		}
		else {
			Session.playerShipVariables.ship.currency -= costToRepair;
		}

		NotifyAny();
	}

	public void GUI_RepairShipByAllHP() {
		if (Mathf.CeilToInt(Session.playerShipVariables.ship.health) >= 100) {
			Notifications.ShowANotificationMessage("Your ship is already fully repaired");
		}
		else {
			Session.playerShipVariables.ship.currency -= (int)(costToRepair * Mathf.CeilToInt(100 - Session.playerShipVariables.ship.health));
			Session.playerShipVariables.ship.health = 100f;
		}

		NotifyAny();
	}

	public void GUI_BuyNewShip() {
		if(Session.playerShipVariables.ship.currency > costToBuyUpgrade) {
			Session.UpgradeShip(costToBuyUpgrade);
			Notifications.ShowANotificationMessage("We have a larger ship! More benches, more oars, more men and more bellies… We need more food and ample water to keep men at oars and mutiny at bay!");
		}
		else { 
			Notifications.ShowANotificationMessage("Earn more drachma through trade to upgrade your ship!");
		}
	}

}

public class RepairsView : ViewBehaviour<RepairsViewModel>
{
	[SerializeField] StringView ShipHealth = null;
	[SerializeField] StringView CostOneHp = null;
	[SerializeField] StringView CostAllHp = null;
	[SerializeField] ButtonView RepairOneButton = null;
	[SerializeField] ButtonView RepairAllButton = null;
	[SerializeField] ButtonView UpgradeButton = null;

	public override void Bind(RepairsViewModel model) {
		base.Bind(model);

		ShipHealth.Bind(ValueModel.Wrap(model.shipHealth)
			.Select(h => Mathf.CeilToInt(Globals.Game.Session.playerShipVariables.ship.health))
			.AsString()
		);

		RepairOneButton.Bind(ValueModel.New(new ButtonViewModel {
			Label = "Repair",
			OnClick = Model.GUI_RepairShipByOneHP
		}));

		RepairAllButton.Bind(ValueModel.New(new ButtonViewModel {
			Label = "Repair",
			OnClick = Model.GUI_RepairShipByAllHP
		}));

		UpgradeButton.Bind(ValueModel.New(new ButtonViewModel {
			Label = "Buy",
			OnClick = model.GUI_BuyNewShip
		}));

	}

	protected override void Refresh(object sender, string propertyChanged) {
		base.Refresh(sender, propertyChanged);

		//If the ship is at 100HP already, then let's not worry about giving the player the costs--we'll replace the costs by an X
		//	--and disable the repair buttons
		if (Mathf.CeilToInt(Model.shipHealth.Value) ==  100) {
			CostOneHp.Bind(ValueModel.New("X"));
			CostAllHp.Bind(ValueModel.New("X"));

			RepairOneButton.GetComponent<Button>().interactable = false;
			RepairAllButton.GetComponent<Button>().interactable = false;
		}
		else {

			CostOneHp.Bind(ValueModel.New(Model.costToRepair)
				.Select(cost => Mathf.CeilToInt(cost))
				.AsString());

			CostAllHp.Bind(ValueModel.New(Model.costToRepair)
				.Select(cost => (Mathf.CeilToInt(100 - Mathf.CeilToInt(Globals.Game.Session.playerShipVariables.ship.health)) * cost))
				.AsString());

			RepairOneButton.GetComponent<Button>().interactable = true;
			RepairAllButton.GetComponent<Button>().interactable = true;
		}

		// TODO: Flesh out upgrade system? For now, you can only upgrade once and it just gives you the main ship. You start out with a smaller one.
		UpgradeButton.GetComponent<Button>().interactable = Model.shipLevel.Value == 0;
	}
}
