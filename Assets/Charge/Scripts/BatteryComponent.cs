using UnityEngine;

public class BatteryComponent : MonoBehaviour {
	public Renderer[] Borders;
	public TextMesh ChargeTextMesh;

	private bool _active; public bool active { get { return _active; } set { if (_active == value) return; _active = value; UpdateColor(); } }
	private int _charge; public int charge { get { return _charge; } set { if (_charge == value) return; _charge = value; UpdateText(); UpdateColor(); } }
	private int _required; public int required { get { return _required; } set { if (_required == value) return; _required = value; UpdateText(); } }

	private void Start() {
		UpdateColor();
	}

	private void UpdateText() {
		ChargeTextMesh.text = charge > 0 ? charge.ToString().PadLeft(4, ' ') + '/' + required.ToString().PadRight(4, ' ') : "";
	}

	private void UpdateColor() {
		Color color = charge > 0 ? Color.green : (Color)(new Color32(0x88, 0x88, 0x88, 0xff));
		foreach (Renderer renderer in Borders) renderer.material.color = color;
	}
}
