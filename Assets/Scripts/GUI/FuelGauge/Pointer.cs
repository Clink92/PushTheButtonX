﻿using UnityEngine;

public class Pointer : MonoBehaviour {
    
    private RectTransform img;
    private DropPod pod;
    private int speed = 30;

	// Use this for initialization
	void Start () {
        img = GetComponent<RectTransform>();
        pod = transform.root.GetComponent<GUIHandler>().pod;
    }
	
	// Update is called once per frame
	void Update () {
        float value = ExtensionMethods.Remap(pod.Fuel(), 0, 100, 90, -90);    //Remove magic numbers...
        Debug.Log(pod.Fuel());
        img.transform.rotation = Quaternion.Euler(0, 0, value);
	}
}
