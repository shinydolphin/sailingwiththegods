using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel; 
using UnityEngine;
using UnityEngine.UI;

public class LoanViewModel : Model
{
	World World => Globals.World;
	Database Database => Globals.Database;
	Notifications Notifications => Globals.Notifications;

	GameSession Session { get; set; }

	public LoanViewModel(GameSession session)
    {
		Session = session;
    }

	public Loan Loan => Session.playerShipVariables.ship.currentLoan;
	public Loan NewLoan {
		get {
			//Setup the initial term to repay the loan
			float numOfDaysToPayOffLoan = 10;
			//Determine the base loan amount off the city's population
			float baseLoanAmount = 500 * (Session.currentSettlement.population / 1000);
			//If base loan amount is less than 200 then make it 200 as the smallest amount available
			if (baseLoanAmount < 200f) baseLoanAmount = 200f;
			//Determine the actual loan amount off the player's clout
			int loanAmount = (int)(baseLoanAmount + (baseLoanAmount * Session.GetOverallCloutModifier(Session.currentSettlement.settlementID)));
			//Determmine the base interest rate of the loan off the city's population
			float baseInterestRate = 10 + (Session.currentSettlement.population / 1000);
			//Determine finalized interest rate after determining player's clout
			float finalInterestRate = (float)System.Math.Round(baseInterestRate - (baseInterestRate * Session.GetOverallCloutModifier(Session.currentSettlement.settlementID)), 3);

			//Create the Loan object for our button to process		
			return new Loan(loanAmount, finalInterestRate, numOfDaysToPayOffLoan, Session.currentSettlement.settlementID);
		}
	}

	public bool IsAtOriginPort => Session.CheckIfShipBackAtLoanOriginPort();
	public Settlement OriginPort => Loan != null ? Database.GetSettlementFromID(Loan.settlementOfOrigin) : null;

	public void GUI_PayBackLoan() {
		var amountDue = Loan.GetTotalAmountDueWithInterest();

		//Pay the loan back if the player has the currency to do it
		if (Session.playerShipVariables.ship.currency > amountDue) {
			Session.playerShipVariables.ship.currency -= amountDue;
			Session.playerShipVariables.ship.currentLoan = null;
			Notifications.ShowANotificationMessage("You paid back your loan and earned a little respect!");
			//give a boost to the players clout for paying back loan
			Session.AdjustPlayerClout(3);

			NotifyAny();
		}
		else {
			Notifications.ShowANotificationMessage("You currently can't afford to pay your loan back! Better make some more money!");
		}

		NotifyAny();
	}

	public void GUI_TakeOutLoan() {
		var loanAmount = NewLoan.amount;
		var loan = NewLoan;

		Session.playerShipVariables.ship.currentLoan = loan;
		Session.playerShipVariables.ship.currency += loanAmount;
		Notifications.ShowANotificationMessage("You took out a loan of " + loanAmount + " drachma! Remember to pay it back in due time!");

		NotifyAny();
	}
}

public class LoanView : PopupView<LoanViewModel>
{
	// subscreens
	[SerializeField] CurrentLoanView CurrentLoanView = null;
	[SerializeField] NewLoanView NewLoanView = null;
	[SerializeField] LoanIsElsewhereView LoanIsElsewhereView = null;

	public override void Bind(LoanViewModel model) {
		base.Bind(model);

		CurrentLoanView.Bind(model);
		NewLoanView.Bind(model);
		LoanIsElsewhereView.Bind(model);
	}

	protected override void Refresh(object sender, string propertyChanged) {
		base.Refresh(sender, propertyChanged);

		if(Model.Loan == null) {
			CurrentLoanView.gameObject.SetActive(false);
			NewLoanView.gameObject.SetActive(true);
			LoanIsElsewhereView.gameObject.SetActive(false);
		}
		else if (Model.IsAtOriginPort) {
			CurrentLoanView.gameObject.SetActive(true);
			NewLoanView.gameObject.SetActive(false);
			LoanIsElsewhereView.gameObject.SetActive(false);
		}
		else {
			CurrentLoanView.gameObject.SetActive(false);
			NewLoanView.gameObject.SetActive(false);
			LoanIsElsewhereView.gameObject.SetActive(true);
		}
	}
}
