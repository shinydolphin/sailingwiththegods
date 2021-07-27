using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MainState
{
	// TODO: Is this a bug? These never change.
	public const bool IS_NEW_GAME = true;
	public const bool IS_NOT_NEW_GAME = false;

	public bool isTitleScreen { get; set; } = true;
	public bool isStartScreen { get; set; } = false;
	public bool isLoadedGame { get; set; } = false;
	public bool runningMainGameGUI { get; set; } = false;

	public bool isGameOver { get; set; } = false;
	public bool menuControlsLock { get; set; } = false;
	public bool gameIsFinished { get; set; } = false;
}
