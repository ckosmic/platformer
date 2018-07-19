using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	public float cameraAngle = 45;
	public float rotateSpeed = 90;
	public float collisionRadius = 0.5f;
	public bool mouseControl = true;
	public float mouseSensitivity = 10;
	public LayerMask interactionMask;
	public float startRotation;

	private float yRot = 0;
	private Vector3 camRot;
	private Quaternion lookAtPlayer;
	private float lerpAmnt = 1;
	private float mouseX;
	private float lerpedMouseX;
	private float mouseY;
	private float lerpedMouseY;
	private Vector3 lerpedPosition;
	private Vector2 stickInput;
	private Vector3 camOffset;
	private Vector3 lerpedVel;

	private float initAngle = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		lerpedVel = Vector3.Lerp(lerpedVel, Vector3.ClampMagnitude(PlayerComponents.instance.cc.velocity, 100), Time.smoothDeltaTime * 4);
		lerpedPosition = Vector3.Lerp(lerpedPosition, PlayerComponents.instance.transform.position, lerpAmnt * (Vector3.Distance(PlayerComponents.instance.transform.position, lerpedPosition) / 2));
		if (Mathf.Abs(Input.GetAxis("Mouse X")) + Mathf.Abs(Input.GetAxis("Mouse Y")) == 0 || !mouseControl)
			yRot += Input.GetAxis("Horizontal") * rotateSpeed * Time.deltaTime;

		lerpedMouseX = Mathf.Lerp(lerpedMouseX, Input.GetAxis("Mouse X") * mouseSensitivity, Time.deltaTime * 6);
		mouseX += lerpedMouseX * Time.timeScale;
		lerpedMouseY = Mathf.Lerp(lerpedMouseY, Input.GetAxis("Mouse Y") * mouseSensitivity, Time.deltaTime * 6);
		mouseY += lerpedMouseY * Time.timeScale;
		mouseY = Mathf.Clamp(mouseY, -89 + cameraAngle, 60 + cameraAngle);

		transform.rotation = Quaternion.identity;
		camRot = Vector3.Lerp(camRot, new Vector3(cameraAngle, 0, 0) + (Vector3.up * yRot), Time.smoothDeltaTime * 10);
		transform.Rotate(camRot);
		transform.RotateAround(PlayerComponents.instance.transform.position, Vector3.up, mouseX);
		transform.RotateAround(PlayerComponents.instance.transform.position, transform.right, Mathf.Clamp(-mouseY - lerpedVel.y / 3, -60 - cameraAngle, 89 - cameraAngle));
		transform.position = lerpedPosition + -transform.forward * 6 + PlayerComponents.instance.cc.center + camOffset;
		camOffset = Vector3.zero;

		cameraAngle = initAngle;

		Vector3 heading = transform.position - (PlayerComponents.instance.transform.position + PlayerComponents.instance.cc.center);
		float distance = heading.magnitude;
		Vector3 direction = heading / distance;
		RaycastHit hit = new RaycastHit();
		if (Physics.SphereCast(PlayerComponents.instance.transform.position + PlayerComponents.instance.cc.center, collisionRadius, direction, out hit, distance, interactionMask)) {
			Vector3 newCamPos = new Vector3(hit.point.x + hit.normal.x * collisionRadius, hit.point.y + hit.normal.y * collisionRadius, hit.point.z + hit.normal.z * collisionRadius);
			lerpAmnt = 1;
			transform.position = newCamPos;
		} else {
			lerpAmnt = Time.smoothDeltaTime * 12.5f;
		}
	}
}
