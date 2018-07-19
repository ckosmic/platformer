using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerComponents : MonoBehaviour {

	public static PlayerComponents instance = null;

	[HideInInspector]
	public GameObject ply;
	[HideInInspector]
	public CharacterController cc;
	[HideInInspector]
	public PlayerMovement movement;
	public Animator anim;

	void Awake() {
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);

		ply = gameObject;
		cc = GetComponent<CharacterController>();
		movement = GetComponent<PlayerMovement>();
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
