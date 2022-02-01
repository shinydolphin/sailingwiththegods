using UnityEngine;
using UnityEngine.UI;

public class ShrineOptionModel : Model
{
	Notifications Notifications => Globals.Notifications;

	GameSession Session { get; set; }

	public string Name;
	public int Cost;
	public int CloutGain;
	public string BenefitHint;

	public ShrineOptionModel(GameSession session, string name, int cost, int cloutGain, string benefitHint) {
		Session = session;
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
