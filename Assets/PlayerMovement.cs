using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

	public bool enableGravity = true;

	public float speed;
	public float acceleration;
	public float rotationSpeed = 15;
	public float gravity = 1;
	public float jumpHeight = 0.2f;

	public struct Axes {
		public float vertAxis;
		public float horzAxis;
	};

	CharacterController cc;
	GameObject cam;
	[HideInInspector]
	public Axes axes;
	float yvel = 0;
	bool doubleJumped = false;
	bool canControl = true;
	Vector3 extraVelocity;
	Animator anim;
	float extraVelDecel;

	Vector3 horizVector = Vector3.zero;

	void Start () {
		//Initialize
		cc = PlayerComponents.instance.cc;
		cam = Camera.main.gameObject;
		anim = PlayerComponents.instance.anim;
	}
	
	void Update () {

		//Get forward direction of player relative to camera rotation (direction that the player should go forward)
		Vector3 camPlyForward = cam.transform.forward;
		camPlyForward.y = 0;
		camPlyForward.Normalize();
		Debug.DrawRay(transform.position, camPlyForward, Color.blue);
		//Get right direction relative to camera
		Vector3 camPlyRight = camPlyForward;
		camPlyRight = Quaternion.Euler(0, 90, 0) * camPlyForward;
		Debug.DrawRay(transform.position, camPlyRight, Color.red);

		if (canControl) {
			//If either directional axis is > 0
			if ((IsPlaying("hang") ? 0 : 1) * (Mathf.Abs(Input.GetAxisRaw("Vertical")) + Mathf.Abs(Input.GetAxisRaw("Horizontal"))) > 0) {
				//Set vert and horiz axes
				axes.vertAxis = Mathf.Lerp(axes.vertAxis, Input.GetAxisRaw("Vertical"), Time.deltaTime * acceleration);
				axes.horzAxis = Mathf.Lerp(axes.horzAxis, Input.GetAxisRaw("Horizontal"), Time.deltaTime * acceleration);

				//Set rotation
				transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(camPlyForward * Input.GetAxisRaw("Vertical") + camPlyRight * Input.GetAxisRaw("Horizontal"), Vector3.up), Time.deltaTime * rotationSpeed * Mathf.Clamp01(Mathf.Abs(Input.GetAxisRaw("Vertical")) + Mathf.Abs(Input.GetAxisRaw("Horizontal"))));
			} else {
				//If not pressing directional keys, come to a quick stop
				axes.vertAxis = Mathf.Lerp(axes.vertAxis, Input.GetAxisRaw("Vertical"), Time.deltaTime * acceleration * 4);
				axes.horzAxis = Mathf.Lerp(axes.horzAxis, Input.GetAxisRaw("Horizontal"), Time.deltaTime * acceleration * 4);
			}
		}

		if (enableGravity) {
			if (cc.isGrounded) {
				//If on ground, cling to ground
				yvel = -0.05f;
				doubleJumped = true;
				//Jump if on ground and Jump button is pressed
				if (Input.GetButtonDown("Jump")) {
					doubleJumped = false;
					yvel = jumpHeight;
				}
				//When player lands from a dive
				if(anim.GetBool("diving")) {
					anim.SetBool("diving", false);
					canControl = true;
					Vector3 planeForward = transform.forward;
					planeForward.y = 0;
					planeForward.Normalize();
					transform.rotation = Quaternion.LookRotation(planeForward, Vector3.up);
					extraVelDecel = 5;
					yvel = jumpHeight / 2;
				}
			} else {
				//If not on ground, fall
				yvel -= gravity * Time.deltaTime;
				//If not on ground and already jumped and Jump pressed, double jump
				if (Input.GetButtonDown("Jump") && doubleJumped == false) {
					yvel = jumpHeight * 1.25f;
					doubleJumped = true;
					anim.SetTrigger("doubleJump");
				}
				//Edge detection, hanging, and hopping up
				object[] edge = DetectEdge();
				if ((bool)edge[0]) {
					extraVelDecel = 5;
					RaycastHit hit = (RaycastHit)edge[1];
					Vector3 hitNormal = -hit.normal;
					hitNormal.y = 0;
					hitNormal.Normalize();
					transform.rotation = Quaternion.LookRotation(hitNormal);
					yvel = jumpHeight;
					extraVelocity = transform.forward / 6;
					anim.Play("hang");
					doubleJumped = true;
				}
				//Diving - if action button and directional buttons pressed
				if (Input.GetButtonDown("Action") && Mathf.Abs(Input.GetAxisRaw("Vertical")) + Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0) {
					yvel = jumpHeight;
					extraVelocity = transform.forward / 3;
					doubleJumped = true;
					anim.SetBool("diving", true);
					canControl = false;
					extraVelDecel = 1;
				}
				if (anim.GetBool("diving")) {
					axes.vertAxis = 1;
					axes.horzAxis = 0;
					transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(cc.velocity), Time.deltaTime * 5);
				}
			}
		} else {
			yvel = 0;
		}

		//Set plane (x and z) velocity
		horizVector = (IsPlaying("hang") ? 0 : 1) * (canControl ? 1 : 0) * transform.forward * Mathf.Clamp01(Mathf.Abs(axes.vertAxis) + Mathf.Abs(axes.horzAxis)) * speed;

		//Animation stuff based on movement variables
		if (horizVector.magnitude > 0.01) {
			anim.speed = horizVector.magnitude * 8 + (3 / (horizVector.magnitude*128));
		} else {
			anim.speed = 1;
		}
		anim.SetFloat("planeSpeed", Mathf.Clamp01(horizVector.magnitude * 10));
		anim.SetBool("isGrounded", cc.isGrounded);
		anim.SetFloat("yvel", yvel);

		cc.Move(horizVector + Vector3.up * yvel + extraVelocity);
		extraVelocity = Vector3.Lerp(extraVelocity, Vector3.zero, Time.deltaTime * extraVelDecel);
	}

	//Detects corners of objects with three raycasts
	//Returns an object with hit information
	//return[0] -> if edge is detected
	//return[1] -> hit info
	object[] DetectEdge() {

		float checkHeight = 1.5f;
		object[] output = new object[2];
		output[0] = false;

		Debug.DrawRay(transform.position + Vector3.up * checkHeight, transform.forward * 1, Color.blue);
		Debug.DrawRay(transform.position + Vector3.up * (checkHeight-0.2f), transform.forward * 1, Color.blue);
		Debug.DrawRay(transform.forward * 0.75f + transform.position + Vector3.up * checkHeight, -transform.up * 0.2f, Color.red);
		if (!Physics.Raycast(transform.position + Vector3.up * checkHeight, transform.forward, 1)) {
			RaycastHit hit;
			if (Physics.Raycast(transform.position + Vector3.up * (checkHeight-0.2f), transform.forward, out hit, 1)) {
				if (Physics.Raycast(transform.forward * 0.75f + transform.position + Vector3.up * checkHeight, -transform.up, 0.2f)) {
					output[0] = true;
					output[1] = hit;
				}
			}
		}

		return output;
	}

	//Returns true if animation state is playing, otherwise false
	bool IsPlaying(string name) {
		return anim.GetCurrentAnimatorStateInfo(0).IsName(name);
	}
}
