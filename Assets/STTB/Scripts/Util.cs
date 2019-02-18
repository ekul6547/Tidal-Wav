using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Util : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    [ContextMenu("Screenshot")]
    public void ScreenShot()
    {
        string FileName = Application.productName + "_" + SceneManager.GetActiveScene().name + "_" + Time.time + ".png";
        ScreenCapture.CaptureScreenshot(FileName);
        Debug.Log("Screenshot Taken: " + FileName);
    }


    public Texture2D Image;
    [ContextMenu("Save Image")]
    public void SaveImage()
    {
        string path = Path.Combine(Application.dataPath,"UtilSave");
        var file = Image.EncodeToPNG();
        File.WriteAllBytes(path, file);
    }
}
