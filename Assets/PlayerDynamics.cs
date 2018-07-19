using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDynamics : MonoBehaviour {

	public float turningIntensity = 20;
	public float accelIntensity = 200;

	public Transform boneBack;
	public Transform boneHead;
	public Transform boneHips;
	public AnimationCurve accelAnimation;

	Vector3 velocity;
	Vector3 velocity_prev;

	float lerpedInput;
	Vector3 lastRot;
	float rotDelta;
	float lerpedHoriz;
	Animator anim;
	CharacterController cc;

	// Use this for initialization
	void Start () {
		anim = PlayerComponents.instance.anim;
		cc = PlayerComponents.instance.cc;
	}
	
	// Update is called once per frame
	void Update () {
		velocity = velocity_prev - transform.position;
		velocity_prev = transform.position;

		float inp = Mathf.Clamp01(Mathf.Abs(Input.GetAxisRaw("Horizontal")) + Mathf.Abs(Input.GetAxisRaw("Vertical")));
		if (inp > 0) {
			lerpedInput = Mathf.Lerp(lerpedInput, inp, Time.deltaTime);
		} else {
			lerpedInput = Mathf.Lerp(lerpedInput, 0, Time.deltaTime * 10);
		}

		rotDelta = transform.eulerAngles.y - lastRot.y;
		if (rotDelta > 180) rotDelta -= 360;
		if (rotDelta < -180) rotDelta += 360;
		lerpedHoriz = Mathf.Lerp(lerpedHoriz, rotDelta, Time.deltaTime * 3);
		lerpedHoriz = Mathf.Clamp(lerpedHoriz, -8, 8);

		lastRot = transform.eulerAngles;
	}

	void LateUpdate() {
		if (cc.isGrounded) {
			//Acceleration
			boneBack.Rotate(Vector3.right, accelIntensity * velocity.magnitude * -accelAnimation.Evaluate(lerpedInput) + (20 * Mathf.PerlinNoise(Time.time, 4) - 10));
			boneHead.Rotate(Vector3.right, accelIntensity * velocity.magnitude * accelAnimation.Evaluate(lerpedInput) / 2 + (20 * Mathf.PerlinNoise(Time.time, 6) - 10));
		}

		//Rotating
		boneBack.Rotate(Vector3.up, turningIntensity * 2 * lerpedHoriz * Mathf.PerlinNoise(Time.time, 0));
		boneBack.Rotate(Vector3.forward, turningIntensity * -lerpedHoriz * Mathf.PerlinNoise(Time.time, 2));
		boneHead.Rotate(Vector3.up, turningIntensity * 2 * lerpedHoriz * Mathf.PerlinNoise(Time.time, 8));
		boneHips.Rotate(Vector3.forward, turningIntensity * -lerpedHoriz * Mathf.PerlinNoise(Time.time, 10));

		//Rotate spine by y velocity (mostly for diving)
		boneBack.Rotate(Vector3.right, cc.velocity.y * 2);
	}
}
