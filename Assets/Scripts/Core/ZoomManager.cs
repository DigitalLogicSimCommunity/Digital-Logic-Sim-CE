using System.Collections.Generic;
using UnityEngine;

public class ZoomManager : MonoBehaviour {
	public static ZoomManager instance;

	const float maxCamOrthoSize = 4.7f;
	const float minCamOrthoSize = 0.7f;

	public static float zoom = 0f;

	[Header("Main Settings")]
	[Range(0, 1)] public float targetZoom = 0f;
	public bool showZoomHelper = true;

	[Header("Mouse Zoom Settings")]
	public float mouseWheelSensitivity = 0.1f;
	public float mouseWheelDeadzone = 0.01f;
	public float mouseZoomSpeed = 12f;
	public float camMoveSpeed = 12f;

	[Header("References")]
	public Camera cam;
	public GameObject zoomHelpPanel;
	public RectTransform zoomHelpViewport;
	public Camera zoomHelpCam;


	Vector2 maxZoomViewportSize = new Vector2(320, 180);
	Vector2 minZoomViewportSize = new Vector2(64, 36);

	Vector2 zoomMoveRange = new Vector2(7f, 3.4f);
	Vector3 targetCamPosition = Vector3.zero;

	Vector3 focusOffset = new Vector3(0, -0.2f, 0);
	Vector2 zoomViewportMoveRange = new Vector2(132, 68);


	void Awake() {
		instance = this;
	}

	void Update() {
		if (!InputHelper.MouseOverUIObject()) {
			if (Input.GetKey(KeyCode.F)) {
				if (ChipInteraction.selectedChips.Count > 0) {
					List<Vector3> chipPositions = new List<Vector3>();
					foreach (Chip chip in ChipInteraction.selectedChips) {
						chipPositions.Add(chip.transform.position);
					}
					targetCamPosition = MathUtility.Center(chipPositions) + focusOffset;

					// TODO: set target zoom based on selection world size
					targetZoom = 1;
				} else {
					targetCamPosition = InputHelper.MouseWorldPos;
				}
				
			} else {
				Vector3 moveVec = new Vector3();
				moveVec.x += InputHelper.AnyOfTheseKeysHeld(KeyCode.RightArrow, KeyCode.D) ? 1 : 0;
				moveVec.x -= InputHelper.AnyOfTheseKeysHeld(KeyCode.LeftArrow, KeyCode.A) ? 1 : 0;
				moveVec.y += InputHelper.AnyOfTheseKeysHeld(KeyCode.UpArrow, KeyCode.W) ? 1 : 0;
				moveVec.y -= InputHelper.AnyOfTheseKeysHeld(KeyCode.DownArrow, KeyCode.S) ? 1 : 0;
				targetCamPosition = targetCamPosition + (moveVec * camMoveSpeed) * 0.01f;
			}

			if (Input.GetKeyDown(KeyCode.G)) {
				targetZoom = 0;
				targetCamPosition = Vector3.zero;
			}

			float scrollAmount = Input.GetAxis("Mouse ScrollWheel");
			if ((scrollAmount > mouseWheelDeadzone || scrollAmount < -mouseWheelDeadzone) && !InputHelper.MouseOverUIObject()) {
				//targetCamPosition = InputHelper.MouseWorldPos;
				targetZoom = Mathf.Clamp01(zoom + scrollAmount * mouseWheelSensitivity);
				if (ChipInteraction.selectedChips.Count > 0) {
					List<Vector3> chipPositions = new List<Vector3>();
					foreach (Chip chip in ChipInteraction.selectedChips) {
						chipPositions.Add(chip.transform.position);
					}
					targetCamPosition = MathUtility.Center(chipPositions) + focusOffset;
				}
			}
			zoom = Mathf.Lerp(zoom, targetZoom, mouseZoomSpeed * Time.deltaTime);
		}
	}

	void LateUpdate() {
		cam.orthographicSize = CalcCameraOrthoSize();

		targetCamPosition = new Vector3(
			Mathf.Clamp(targetCamPosition.x, -zoomMoveRange.x * zoom, zoomMoveRange.x * zoom),
			Mathf.Clamp(targetCamPosition.y, -zoomMoveRange.y * zoom, zoomMoveRange.y * zoom),
			0
		);
		transform.position = Vector3.Lerp(transform.position, targetCamPosition, camMoveSpeed * Time.deltaTime);
		
		UpdateZoomHelper();
	}

	void UpdateZoomHelper() {
		if (showZoomHelper) {
			if (zoom >= 0.1 && !zoomHelpPanel.activeInHierarchy) {
				zoomHelpCam.gameObject.SetActive(true);
				zoomHelpPanel.SetActive(true);
			} else if (zoom < 0.1 && zoomHelpPanel.activeInHierarchy) {
				zoomHelpCam.gameObject.SetActive(false);
				zoomHelpPanel.SetActive(false);
			}

			Vector2 viewportSize = new Vector2 (
				Mathf.Lerp(maxZoomViewportSize.x, minZoomViewportSize.x, zoom),
				Mathf.Lerp(maxZoomViewportSize.y, minZoomViewportSize.y, zoom)
			);
			zoomHelpViewport.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, viewportSize.x);
			zoomHelpViewport.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, viewportSize.y);

			zoomHelpViewport.anchoredPosition = CalcViewPortSize();

		} else if (zoomHelpPanel.activeInHierarchy) {
			zoomHelpCam.gameObject.SetActive(false);
			zoomHelpPanel.SetActive(false);
		}
		
	}

	float CalcCameraOrthoSize() {
		zoom = Mathf.Clamp01(zoom);
		return Mathf.Lerp(maxCamOrthoSize, minCamOrthoSize, zoom);
	}

	Vector2 CalcViewPortSize() {
		AnimationCurve curve = new AnimationCurve(
			new Keyframe(-zoomMoveRange.x, -zoomViewportMoveRange.x),
			new Keyframe(zoomMoveRange.x, zoomViewportMoveRange.x)
		);
		curve.SmoothTangents(0, 0);
		curve.SmoothTangents(1, 0);

		float xPos = curve.Evaluate(transform.position.x);

		curve.MoveKey(0, new Keyframe(-zoomMoveRange.y, -zoomViewportMoveRange.y));
		curve.MoveKey(1, new Keyframe(zoomMoveRange.y, zoomViewportMoveRange.y));

		float yPos = curve.Evaluate(transform.position.y);

		return new Vector2(xPos, yPos);
	}
}
