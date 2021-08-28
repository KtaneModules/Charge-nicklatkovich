using UnityEngine;

public class LockComponent : MonoBehaviour {
	public Renderer[] Borders;
	public Renderer Logo;
	public Material InactiveLock;
	public Material ActiveLock;

	private bool _active = true; public bool active { get { return _active; } set { if (_active == value) return; _active = value; UpdateColors(); } }

	private void Start() {
		UpdateColors();
	}

	private void UpdateColors() {
		Color color = active ? Color.red : (Color)(new Color32(0x88, 0x88, 0x88, 0xff));
		foreach (Renderer border in Borders) border.material.color = color;
		Logo.material = active ? ActiveLock : InactiveLock;
	}
}
