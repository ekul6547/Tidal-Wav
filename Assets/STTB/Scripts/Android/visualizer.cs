using UnityEngine;
using System.Collections;

public class visualizer : MonoBehaviour {
    AndroidJavaClass inst;
    byte[] ret;

	// Use this for initialization
	void Start () {
        inst = new AndroidJavaClass("com.relapis.unityvisualizerplugin.PluginClass");
        GameManager.active.DebugMobile(inst.CallStatic<bool>("TestEnabled").ToString());
    }
	
	// Update is called once per frame
	void Update ()
    {
        //AndroidJavaObject jo = inst.Call<AndroidJavaObject>("getCaptureSize");

    }
}
