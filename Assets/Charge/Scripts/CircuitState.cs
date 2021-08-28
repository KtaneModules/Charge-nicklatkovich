using System.Collections.Generic;

public class CircuitState {
	public enum CrossState {
		NONE,
		CHARGE,
		STATUS,
		SUBMIT,
	}

	public bool discharge = false;
	public int powergridStart = 4;
	public int powergridEnd = 5;
	public bool powergridActive;
	public CrossState crossState = CrossState.STATUS;

	public void UpdateActivity(ChargePuzzle puzzle) {
		if (powergridStart > powergridEnd || (crossState != CrossState.CHARGE && crossState != CrossState.STATUS)) powergridActive = false;
		else powergridActive = puzzle.GetConnectionPower(new KeyValuePair<int, int>(powergridStart - 1, powergridEnd - 1)) > 0;
	}
}
