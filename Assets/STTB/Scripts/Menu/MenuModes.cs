using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MenuModes : MonoBehaviour {

    public Transform MainCanvas;
    public Image background;

    [System.Serializable]
    public class SliderTextures
    {
        public Sprite Knob;
        public Color KnobColor = Color.white;
        public Sprite Background;
        public Color BackgroundColor = Color.white;
        public Sprite Fill;
        public Color FillColor = Color.white;
    }

    [System.Serializable]
    public class MenuData
    {
        [Header("Main")]
        public string OptionName;
        public Font font;
        public Color MiscTextColor = Color.black;
        public string BlockMode = "normal";
        [Header("Button Style")]
        public Sprite ButtonSprite;
        public ColorBlock ButtonColour = ColorBlock.defaultColorBlock;
        public Color ButtonTextColour = Color.white;
        [Header("Background Style")]
        public Sprite PanelSprite;
        public Color BackgroundColour = Color.white;
        public Sprite BackgorundSprite;
        public bool doRainbowBlocks = true;
        [Header("Slider")]
        public SliderTextures sliderData;

        public UnityEvent OnDeselected;
        public UnityEvent OnSelected;
    }
    public List<MenuData> menuOptions = new List<MenuData>();

    public void SetMenuByName(string MenuName)
    {
        MenuData newData = GetMenuByName(MenuName);
        if(newData != null)
        {
            MenuData oldData = GetMenuByName(GameManager.active.Mode);
            if(oldData != null)
            {
                oldData.OnDeselected.Invoke();
            }
            SetMenu(newData);
        }
    }
    public MenuData GetMenuByName(string MenuName)
    {
        foreach (MenuData m in menuOptions)
        {
            if (m.OptionName == MenuName)
            {
                return m;
            }
        }
        return null;
    }

    public void SetMenu(MenuData data)
    {
        SetFontAs(data.font);
        SetButtonSprite(data.ButtonSprite,data.ButtonColour,data.ButtonTextColour,data.MiscTextColor);
        SetSliderButton(data.sliderData);
        SetBackground(data.BackgorundSprite,data.BackgroundColour);
        GameManager.active.DoRainbowColors = data.doRainbowBlocks;
        GameManager.active.Mode = data.BlockMode;
        data.OnSelected.Invoke();
    }

    public void CallMethod(string methodName)
    {
        Invoke(methodName, 0);
    }

    [Header("Item Lists")]
    public List<Text> toSetFont = new List<Text>();
    public List<Slider> sliders = new List<Slider>();
    public Transform SkinParent;

    public void Setup()
    {
        toSetFont.AddRange(MainCanvas.GetComponentsInChildren<Text>(true));

        List<object> toRemove = new List<object>();
        foreach (Text t in toSetFont)
        {
            if (t.tag == "NoFontChange")
            {
                toRemove.Add(t);
            }
        }

        foreach (object t in toRemove)
        {
            toSetFont.Remove(t as Text);
        }

        sliders.AddRange(MainCanvas.GetComponentsInChildren<Slider>(true));

        toRemove.Clear();
        foreach (Slider s in sliders)
        {
            if (s.tag == "NoSliderChange")
            {
                toRemove.Add(s);
            }
        }

        foreach (object t in toRemove)
        {
            sliders.Remove(t as Slider);
        }

        for(int i = 0; i < SkinParent.childCount; i++)
        {
            ButtonImages.Add(SkinParent.GetChild(i).GetComponent<Image>());
        }
    }

    public void SetFontAs(Font newFont)
    {
        foreach (Text t in toSetFont)
        {
            t.font = newFont;
        }
    }

    public Font defaultFont;
    public Font GetFont(string menuName = "Default")
    {
        foreach(MenuData m in menuOptions)
        {
            if(m.OptionName == menuName)
            {
                return m.font;
            }
        }
        return defaultFont;
    }

    public MenuData GetData(string menuName)
    {
        foreach(MenuData m in menuOptions)
        {
            if(m.OptionName == menuName)
            {
                return m;
            }
        }
        return null;
    }

    public List<Image> ButtonImages = new List<Image>();
    public void SetButtonSprite(Sprite spr,ColorBlock butCol, Color textCol, Color miscText)
    {
        foreach (Image I in ButtonImages)
        {
            I.sprite = spr;
            if (I.tag != "NoButtonChange")
            {
                var button = I.GetComponent<Button>();
                if(button != null)
                {
                    button.colors = butCol;
                }
                else
                {
                    I.color = butCol.normalColor;
                }
            }
            var t = I.transform.Find("Text");
            if (t != null && t.tag != "NoFontChange")
            {
                t.GetComponent<Text>().color = textCol;
            }
            var skin = I.GetComponent<SkinInstance>();
            if(skin != null)
            {
                skin.transform.Find("Text").GetComponent<Text>().color = miscText;
            }
        }
    }
    public void SetSliderButton(SliderTextures sliderData)
    {
        foreach(Slider s in sliders)
        {
            Image background = s.transform.Find("Background").GetComponent<Image>();
            background.sprite = sliderData.Background;
            background.color = sliderData.BackgroundColor;

            Image fill = s.transform.Find("Fill Area").GetChild(0).GetComponent<Image>();
            fill.sprite = sliderData.Fill;
            fill.color = sliderData.FillColor;

            Image knob = s.transform.Find("Handle Slide Area").GetChild(0).GetComponent<Image>();
            knob.sprite = sliderData.Knob;
            knob.color = sliderData.KnobColor;
        }
    }
    public void ClearButtonSprite()
    {
        SetButtonSprite(null,ColorBlock.defaultColorBlock,Color.black,Color.black);
    }
    public void SetBackground(Sprite spr,Color col)
    {
        background.sprite = spr;
        background.color = col;
    }
}
