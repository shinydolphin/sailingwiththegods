using UnityEngine;
using UnityEngine.UI;

public class ShrineOptionModel : Model
{
	GameSession Session => Globals.Game.Session;
	Notifications Notifications => Globals.Notifications;

	public string Name;
	public int Cost;
	public int CloutGain;
	public string BenefitHint;

	public ShrineOptionModel(string name, int cost, int cloutGain, string benefitHint) {
		Name = name;
		Cost = cost;
		CloutGain = cloutGain;
		BenefitHint = benefitHint;
	}

	public void Buy() {

		if (Session.playerShipVariables.ship.currency > Cost) {
			Session.playerShipVariables.ship.currency -= Cost;
			Notifications.ShowANotificationMessage("You built a " + Name + " for " + Session.currentSettlement.name + "! " + BenefitHint);
			Session.AdjustPlayerClout(1);
			Session.playerShipVariables.ship.builtMonuments += Session.currentSettlement.name + " -- " + Name + "\n";

		}
		else {
			Notifications.ShowANotificationMessage("You don't have enough money to build a " + Name + " for " + Session.currentSettlement.name);
		}

	}
}

public class ShrineOptionView : ViewBehaviour<ShrineOptionModel>
{
	[SerializeField] StringView Name = null;
	[SerializeField] StringView BenefitHint = null;
	[SerializeField] ButtonView Buy = null;

	public override void Bind(ShrineOptionModel model) {
		base.Bind(model);

		Name.Bind(ValueModel.New(model.Name));
		BenefitHint.Bind(ValueModel.New(model.BenefitHint));
		Buy.Bind(ValueModel.New(new ButtonViewModel {
			Label = model.Cost + " dr",
			OnClick = model.Buy
		}));
	}
}
