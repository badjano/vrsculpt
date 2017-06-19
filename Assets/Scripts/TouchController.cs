using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchController : MonoBehaviour {

    public OVRInput.Controller controller;

    public bool dragger;

    private GameObject chunkHolder;

    private bool holding;

    // Use this for initialization
    void Start () {
        if ( dragger )
        {
            chunkHolder = GameObject.Find("ChunkHolder");
        }
	}
	
	// Update is called once per frame
	void Update () {
        this.transform.localPosition = OVRInput.GetLocalControllerPosition(controller);
        this.transform.localRotation = OVRInput.GetLocalControllerRotation(controller);

        if (dragger )
        {
            if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, controller) > 0.1f)
            {
                if (!holding)
                {
                    holding = true;
                    chunkHolder.transform.parent = this.transform;
                }
            } else if (holding)
            {
                holding = false;
                if (chunkHolder.transform.parent == this.transform)
                {
                    chunkHolder.transform.parent = null;
                }
            }
        }
    }
}
