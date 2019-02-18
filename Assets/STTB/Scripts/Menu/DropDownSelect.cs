using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropDownSelect : MonoBehaviour {

    public Dropdown down;
    public Button Play;

    public void Change()
    {
        GameManager.active.SetClip(down.value);
        if (GameManager.active.activeClip != null)
        {
            Play.interactable = true;
        }
        else
        {
            Play.interactable = false;
        }
    }
}
