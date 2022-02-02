using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class CrewListScreen : ViewBehaviour<ICollectionModel<CrewManagementMemberViewModel>>
{
	UISystem UI => Globals.UI;
	QuestSystem Quests => Globals.Quests;

	[SerializeField] CrewManagementListView List = null;
	[SerializeField] ButtonView Close = null;

	public override void Bind(ICollectionModel<CrewManagementMemberViewModel> model) {
		base.Bind(model);

		// HACK: collectionwrappermodel has limited ordering features for now, so the *9999 simulates a orderby.thenby, and use a 1/(value) to simulate a desc sort
		List?.Bind(model
			.OrderBy(c => 1f / (c.CitiesInNetwork.Count(city => city.City.settlementID == Quests.CurrDestinationId) * 9999 + c.CitiesInNetwork.Count()))
		);
		Close?.Bind(ValueModel.New(new ButtonViewModel { OnClick = () => UI.Hide(this) }));
	}
}
