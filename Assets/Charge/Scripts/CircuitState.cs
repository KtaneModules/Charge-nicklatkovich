public class CircuitState {
	public bool powergridActive { get { return powergridStart <= powergridEnd; } }
	public int powergridStart = 4;
	public int powergridEnd = 5;
}
