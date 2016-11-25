﻿using UnityEngine;
using System.Collections;

public class CastleLife : MonoBehaviour {

	public float initialLife=100;

	float currentLife;


	// Use this for initialization
	void Start () {
		currentLife = initialLife;
	}
	
	// Update is called once per frame
	void Update () {
		

	}

	public bool decreaseLife(int attack){
		currentLife = currentLife - attack;
		if (currentLife <= 0) {
			//death procedure
			death ();
			return true;
		} else {
			return false;
		}
	}

	public void death (){

		this.gameObject.SetActive (false);

	}

}
