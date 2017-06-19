using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressureScale : MonoBehaviour {

    public OVRInput.Controller controller;

    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        float scale = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controller);
        this.transform.localScale = Vector3.one * scale * 0.2f;
    }
}
