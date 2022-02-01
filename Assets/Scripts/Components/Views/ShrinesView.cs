using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ShrinesViewModel : Model
{
	GameSession Session { get; set; }

	int BaseCost {
		get {
			int baseCost = 0;
			//We need to do a clout check as well as a network checks
			int baseModifier = Mathf.CeilToInt(1000 - (200 * Session.GetOverallCloutModifier(Session.currentSettlement.settlementID)));
			if (Session.Network.CheckIfCityIDIsPartOfNetwork(Session.currentSettlement.settlementID)) {
				baseCost = Mathf.CeilToInt(Session.currentSettlement.tax_network * baseModifier * 1);
			}
			else {
				baseCost = Mathf.CeilToInt(Session.currentSettlement.tax_neutral * baseModifier * 1);
			}
			return baseCost;
		}
	}

	public ICollectionModel<ShrineOptionModel> Options { get; private set; }

	public ShrinesViewModel(GameSession session) {
		Session = session;
		Options = ValueModel.Wrap(new ObservableCollection<ShrineOptionModel>(new[]
		{
			new ShrineOptionModel(Session, "Votive", BaseCost / 200, 1, "+1 Clout"),
			new ShrineOptionModel(Session, "Feast", BaseCost / 10, 10, "+10 Clout"),
			new ShrineOptionModel(Session, "Statue", BaseCost / 3, 30, "+30 Clout"),
			new ShrineOptionModel(Session, "Shrine", BaseCost / 3 * 50, 50, "+50 Clout"),
			new ShrineOptionModel(Session, "Temple", BaseCost / 3 * 50 * 20, 100, "+100 Clout")
		}));
	}
}

public class ShrinesView : ViewBehaviour<ShrinesViewModel>
{
	[SerializeField] ShrineListView Options = null;

	public override void Bind(ShrinesViewModel model) {
		base.Bind(model);

		Options.Bind(model.Options);
	}
}
