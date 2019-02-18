using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinInstance : MonoBehaviour {
    
    public SetSkin skinControl;
    public bool isAvailable = true;
    public int cost;
    public void SetCost(int newCost)
    {
        if (newCost > 0)
        {
            cost = newCost;
            isAvailable = false;
            return;
        }
        isAvailable = true;
    }
    public Rect stretchRect;
    public void SetTexture2D(Texture2D tex, Vector2 stretch)
    {
        RawImage LocalImage = transform.GetChild(0).GetComponent<RawImage>();
        LocalImage.texture = tex;
        Rect rect = LocalImage.uvRect;
        rect.size = stretch;
        rect.position = new Vector2(-1 - (-1 / stretch.x), -1 - (-1 / stretch.y));
        LocalImage.uvRect = rect;
        stretchRect = rect;
    }
    public void SetName(string n)
    {
        name = n;
        transform.Find("Text").GetComponent<Text>().text = n;
    }
    public void SetFont(Font f)
    {
        if (f != null)
        {
            transform.Find("Text").GetComponent<Text>().font = f;
        }
    }
    public void SetColour(ColorBlock col, ColorBlock invalidCol)
    {
        if (isAvailable)
        {
            gameObject.GetComponent<Button>().colors = col;
        }
        else
        {
            //transform.GetChild(0).GetComponent<RawImage>().color = invalidCol;
            gameObject.GetComponent<Button>().colors = invalidCol;
        }
    }
    /// <summary>
    /// Eithers sets as the selected skin, or opts to buy that skin
    /// </summary>
    public void ActivateThis()
    {
        if(isAvailable)
            skinControl.SetByString(name);
        else
        {
            skinControl.BuySkin(name);
        }
    }
    
}
