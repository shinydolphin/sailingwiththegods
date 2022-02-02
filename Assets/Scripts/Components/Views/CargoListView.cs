using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class CargoListView : ListView<ICollectionModel<CargoInventoryViewModel>, CargoInventoryViewModel>
{
	UISystem UI => Globals.UI;

	[SerializeField] ButtonView CloseButton = null;

	public override void Bind(ICollectionModel<CargoInventoryViewModel> model) {
		base.Bind(model);

		CloseButton?.Bind(ValueModel.New(new ButtonViewModel {
			OnClick = () => UI.Hide(this)
		}));
	}
}
