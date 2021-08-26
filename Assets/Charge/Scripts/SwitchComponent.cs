using UnityEngine;

public class SwitchComponent : MonoBehaviour {
	public GameObject Connector;
	public KMSelectable Selectable;

	private bool _state; public bool state { get { return _state; } set { if (_state == value) return; _state = value; UpdateConnector(); } }

	private void Start() {
		UpdateConnector();
	}

	private void UpdateConnector() {
		Connector.transform.localPosition = new Vector3(8, 0, state ? 4 : -4);
		Connector.transform.localRotation = Quaternion.LookRotation(Connector.transform.localPosition, Vector3.up);
	}
}
