using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundScroll : MonoBehaviour {

    Image cg;
    public controller cont;
    
	void Awake () {
        cg = gameObject.GetComponent<Image>();
	}

    void Start()
    {
        if(GameManager.active.backgroundSkinTexture != null)
        {
            cg.material = GameManager.active.backgroundSkinTexture;
        }
    }
	
	void Update () {
        cg.material.mainTextureOffset = new Vector2(Time.time,0);
	}
}
