using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class TownScreen : ViewBehaviour<TradeViewModel>
{
	UISystem UI => Globals.UI;

	[SerializeField] CargoTradeListView Available = null;
	[SerializeField] CargoTradeListView Mine = null;

	[SerializeField] StringView PortName = null;

	[SerializeField] StringView Capacity = null;
	[SerializeField] StringView Money = null;

	[SerializeField] ButtonView Info = null;
	[SerializeField] ButtonView Port = null;
	[SerializeField] ButtonView Sail = null;

	[SerializeField] ButtonView SmallTxn = null;
	[SerializeField] ButtonView LargeTxn = null;
	[SerializeField] ButtonView AllTxn = null;

	[SerializeField] ButtonView Monuments = null;
	[SerializeField] StringView BuiltMonuments = null;


	public override void Bind(TradeViewModel model) {

		base.Bind(model);

		Available?.Bind(model.Available);
		Mine?.Bind(model.Mine);
		
		Port?.Bind(ValueModel.New(new ButtonViewModel {
			Label = "Port",
			OnClick = model.BackToPort
		}));
		Port.Interactable = model.allowPortAccess;

		Monuments?.Bind(ValueModel.New(new ButtonViewModel {
			Label = "Monuments",
			OnClick = () => UI.Show<ShrinesView, ShrinesViewModel>(new ShrinesViewModel(model.Session))
		}));
		Monuments.Interactable = model.monuments;

		Sail?.Bind(ValueModel.New(new ButtonViewModel {
			Label = "Sail",
			OnClick = model.GUI_Button_TryToLeavePort
		}));

		Info?.Bind(ValueModel.New(new ButtonViewModel {
			OnClick = () => UI.Show<InfoScreen, InfoScreenModel>(new InfoScreenModel {
				Icon = model.PortCoin,
				Title = model.PortName,
				Subtitle = model.PortPopulationRank,
				Message = model.PortDescription
			})
		}));

		Money.Bind(ValueModel.Wrap(Model.Money)
				.AsString()
				.Select(s => s + " dr")
		);

		BuiltMonuments.Bind(new BoundModel<string>(Model.Ship, nameof(Model.Ship.builtMonuments)));
	}

	protected override void Refresh(object sender, string propertyChanged) {
		base.Refresh(sender, propertyChanged);

		PortName.Bind(ValueModel.New(Model.PortName));
		Capacity.Bind(ValueModel.New(Model.Capacity));

		SmallTxn.Bind(ValueModel.New(new ButtonViewModel {
			Label = Model.TradeAction == TradeAction.Buy ? ">" : "<",
			OnClick = Model.SmallTxn
		}));

		LargeTxn.Bind(ValueModel.New(new ButtonViewModel {
			Label = Model.TradeAction == TradeAction.Buy ? ">>" : "<<",
			OnClick = Model.LargeTxn
		}));

		AllTxn.Bind(ValueModel.New(new ButtonViewModel {
			Label = Model.TradeAction == TradeAction.Buy ? "All>" : "<All",
			OnClick = Model.AllTxn
		}));

	}
}
