using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeselectThis : MonoBehaviour {

    EventSystem e
    {
        get
        {
            return GetComponent<EventSystem>();
        }
    }

	public void Deselect(GameObject obj)
    {
        if (e.currentSelectedGameObject == obj)
        {
            e.SetSelectedGameObject(null);
        }
    }
}
