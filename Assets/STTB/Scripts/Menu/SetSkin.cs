using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SetSkin : MonoBehaviour {

    public MenuModes menuModes;
    public RectTransform content;
    public GameObject Template;
    RectTransform TemplateTransform;
    public int padding = 15;
    [Header("Locked Colour")]
    public ColorBlock LockedColour;
    [Header("Deselected Colour")]
    public ColorBlock DeSelectedColour;
    [Header("Selected Colour")]
    public ColorBlock SelectedColour;
    public GameObject BuyOption;

    [System.Serializable]
    public class SkinData
    {
        public string name;
        public Texture2D icon;
        public Vector2 StretchSize = new Vector2(1,1);
        public GameObject prefabObject;
        public int cost;
        public string MenuModeName = "Default";
        public UnityEvent OnSetToActive;
    }

    public List<SkinData> skins = new List<SkinData>();
    public List<SkinInstance> skin_instances = new List<SkinInstance>();
    public int currentSkinIndex;
    
    void Awake()
    {
        menuModes = gameObject.GetComponent<MenuModes>() ?? gameObject.AddComponent<MenuModes>();
        TemplateTransform = Template.GetComponent<RectTransform>();
        Template.SetActive(true);
        for (int i = 0; i < skins.Count; i++)
            AddOption(i);
        Template.SetActive(false);
        if (skins.Count > 0)
            SetByString(skins[0].name);

        gameObject.SetActive(false);
    }

    void AddOption(int index)
    {
        float w = (padding * 2) + TemplateTransform.rect.width;
        int adj = (1-(index % 2)); //0 or 1
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, content.rect.width + (w*adj));
        Vector3 newPos = Vector3.zero;
        newPos.x = content.rect.xMin + (w * (Mathf.FloorToInt(index/2))) + (w/2);
        newPos.y = index % 2 == 0 ? 110 : -110;
        GameObject newOption = Instantiate(Template, newPos, new Quaternion(), content);
        //Debug.Log(newPos + " : " + TemplateTransform.localPosition);
        newOption.GetComponent<RectTransform>().anchoredPosition = newPos;
        SkinInstance inst = newOption.GetComponent<SkinInstance>();
        inst.SetName(skins[index].name);
        inst.SetTexture2D(skins[index].icon, skins[index].StretchSize);
        inst.SetCost(skins[index].cost);
        inst.SetColour(DeSelectedColour,LockedColour);
        inst.SetFont(menuModes.GetFont(skins[index].MenuModeName));
        skin_instances.Add(inst);
    }

    public void SetByString(string input)
    {
        for (int i = 0; i < skins.Count; i++)
        {
            if (skins[i].name == input)
            {
                currentSkinIndex = i;
                SetByIndex(currentSkinIndex);
                ColourSkins(skins[i].name);
                return;
            }
        }
    }
    public void SetByPrefabName(string input)
    {
        for (int i = 0; i < skins.Count; i++)
        {
            if (skins[i].prefabObject.name == input)
            {
                currentSkinIndex = i;
                SetByIndex(currentSkinIndex);
                ColourSkins(skins[i].name);
                return;
            }
        }
    }
    public void SetByIndex(int index)
    {
        if(index >= 0 && index < skins.Count)
        {
            GameManager.active.playerSkinPrefab = skins[currentSkinIndex].prefabObject;

            skins[currentSkinIndex].OnSetToActive.Invoke();
            GameManager.active.MenuMode = skins[index].MenuModeName;
        }
    }
    public void ColourSkins(string exclude)
    {
        foreach(SkinInstance inst in skin_instances)
        {
            if(inst.name == exclude)
            {
                inst.SetColour(SelectedColour,LockedColour);
                continue;
            }
            inst.SetColour(DeSelectedColour,LockedColour);
        }
    }
    public SkinData GetDataByName(string input)
    {
        foreach(SkinData dt in skins)
        {
            if(dt.name == input)
            {
                return dt;
            }
        }
        return null;
    }
    public int GetIndexFromName(string input)
    {
        for(int i = 0; i < skins.Count; i++)
        {
            if (skins[i].name == input)
                return i;
        }
        return -1;
    }

    int buyIndex = -1;
    public void BuySkin(string skinName)
    {
        buyIndex = GetIndexFromName(skinName);
        SkinData buyData = GetDataByName(skinName);
        if(buyData == null)
        {
            GameManager.active.MenuPopup("Invalid Selection");
            GameManager.active.DebugMobile("Invalid skin buy: " + skinName,2);
        }
        if (GameManager.active.Points.CanSpendPoints(buyData.cost))
        {
            BuyOption.SetActive(true);
            BuyOption.transform.Find("RawImage").GetComponent<RawImage>().texture = buyData.icon;
            BuyOption.transform.Find("RawImage").GetComponent<RawImage>().uvRect = skin_instances[buyIndex].stretchRect;
            BuyOption.transform.Find("RawImage").Find("NameText").GetComponent<Text>().text = buyData.name;
            SetBUYFont(menuModes.GetFont(buyData.MenuModeName));
        }
        else
        {
            GameManager.active.MenuPopup("You do not have enough points to buy that");
        }
    }

    public void ConfirmBUY()
    {
        //TODO make sure this saves that the skin will be available next time
        if (buyIndex >= 0 && GameManager.active.Points.CanSpendPoints(skins[buyIndex].cost)) {
            GameManager.active.Points.SpendPoints(skins[buyIndex].cost);
            skin_instances[buyIndex].isAvailable = true;
            SetByString(skins[buyIndex].name);
        }
        else
        {
            GameManager.active.MenuPopup("Invalid transaction");
        }
        CloseBUY();
    }
    public void CloseBUY()
    {
        BuyOption.transform.Find("RawImage").GetComponent<RawImage>().texture = null;
        BuyOption.transform.Find("RawImage").Find("NameText").GetComponent<Text>().text = "[INSERT NAME]";
        BuyOption.SetActive(false);
        buyIndex = -1;
    }

    public void SetFont(int index)
    {
        menuModes.SetFontAs(menuModes.GetFont(skins[index].MenuModeName));
    }

    public Text[] BUYTEXT = new Text[0];

    public void SetBUYFont(Font f)
    {
        foreach(Text t in BUYTEXT)
        {
            t.font = f;
        }
    }
}
