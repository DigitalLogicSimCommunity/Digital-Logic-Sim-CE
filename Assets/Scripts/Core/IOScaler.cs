using UnityEngine;

public class IOScaler : MonoBehaviour {
	public enum Mode {Input, Output}
	public Mode mode;
	public Pin pin;
	public Transform wire;
	public Transform indicator;

	CircleCollider2D col;

	void Awake(){
		col = GetComponent<CircleCollider2D>();
	}

	public void UpdateScale() {
		wire.transform.localScale = new Vector3 (ScalingManager.pinSize, ScalingManager.wireThickness / 10, 1);
		float xPos = mode == Mode.Input ? ScalingManager.pinSize : ScalingManager.pinSize * -1;
		pin.transform.localPosition = new Vector3 (xPos, 0, -0.1f);
		indicator.transform.localScale = new Vector3 (ScalingManager.pinSize, ScalingManager.pinSize, 1);
		col.radius = ScalingManager.pinSize / 2 * 1.25f;
		pin.SetScale();
	}
}
