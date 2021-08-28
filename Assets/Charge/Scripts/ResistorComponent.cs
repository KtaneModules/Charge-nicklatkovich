using UnityEngine;

public class ResistorComponent : MonoBehaviour {
	public Renderer[] Borders;

	private bool _active; public bool active { get { return _active; } set { if (_active == value) return; _active = value; UpdateBorders(); } }

	private void Start() {
		UpdateBorders();
	}

	private void UpdateBorders() {
		foreach (Renderer border in Borders) border.material.color = active ? Color.green : (Color)(new Color32(0x88, 0x88, 0x88, 0xff));
	}
}
