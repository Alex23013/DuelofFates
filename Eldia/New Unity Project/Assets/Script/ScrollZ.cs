using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollZ : MonoBehaviour {

	public float scrollSpeed = 20; 

	float startTime;

	void Start(){
		startTime = Time.time;
	}

	// Update is called once per frame
	void Update () {
		Vector3 pos = transform.position;

		Vector3 localVectorUp = transform.TransformDirection (0,1,0);

		pos += localVectorUp * scrollSpeed * Time.deltaTime;
		transform.position = pos;

		//Debug.Log ("Hola");
		Debug.Log (Time.time - startTime);

		if(Time.time - startTime >= 20.0f){

			Application.LoadLevel ("prueba1");

		}
	}
}
