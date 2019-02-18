using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetRedHit : MonoBehaviour {

    public Text RedShow;

	void Update () {
		if(GameManager.active != null)
        {
            RedShow.text = GameManager.active.LateRed.ToString() + " Blocks\nOut of " + GameManager.active.totalGen + " Blocks";
        }
        else
        {
            RedShow.text = "Cannot calculate failures";
        }
	}

    public void Retry()
    {
        if (GameManager.active != null)
        {
            GameManager.active.Retry();
        }
    }
    public void MainMenu()
    {
        if (GameManager.active != null)
        {
            GameManager.active.ExitToMenu();
        }
    }
}
