using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButtonInvoke : MonoBehaviour {

	public void CallFunction(string funcName)
    {
        GameManager.active.CallFunction(funcName);
    }
}
