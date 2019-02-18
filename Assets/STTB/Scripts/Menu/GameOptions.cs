using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOptions : MonoBehaviour {

    [System.Serializable]
    public struct Option
    {
        public string name;
        public float min;
        public float max;
        public float def;
        public bool reversed;
        public float extraGap;
    }

    bool Lock = true;
    public GameObject template;
    RectTransform templateTransform;
    public RectTransform content;
    public Option[] newOptions = new Option[0];
    private List<Slider> slides = new List<Slider>();
    float[] def = new float[0];

    void Start()
    {
        templateTransform = template.GetComponent<RectTransform>();
        foreach (Option o in newOptions)
            AddOption(o);
        SetDifficulty(0);
        def = new float[slides.Count];
        for (int i = 0; i < slides.Count; i++)
        {
            def[i] = slides[i].value;
        }
        foreach (GameManager.GameOption op in GameManager.active.options)
            SetOption(op.name, op.value);
    }

    float padding = 7;
    void AddOption(Option o)
    {
        float h = (padding * 2) + templateTransform.rect.height + o.extraGap;
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, content.rect.height + h);
        Vector3 newPos = templateTransform.anchoredPosition;
        newPos.y = content.rect.yMin + (templateTransform.rect.height/2);

        GameObject newOption = Instantiate(template, newPos, new Quaternion(), content);
        newOption.SetActive(true);
        newOption.name = o.name;
        //Debug.Log(newPos + " : " + TemplateTransform.localPosition);
        newOption.GetComponent<RectTransform>().anchoredPosition = newPos;

        Slider optionSlider = newOption.transform.Find("Slider").GetComponent<Slider>();
        optionSlider.maxValue = o.max;
        optionSlider.minValue = o.min;
        optionSlider.value = o.def;
        if(o.reversed)
            optionSlider.direction = Slider.Direction.RightToLeft;

        slides.Add(optionSlider);

        newOption.transform.Find("Text").GetComponent<Text>().text = o.name;
    }

    void Update () {
	    foreach(Slider S in slides)
        {
            string N = S.transform.parent.name;
            float V = S.value;
            if(N == "CC Speed")
            {
                V = (S.maxValue-S.minValue)-(S.value-S.minValue-1);
            }
            GameManager.active.SetOption(N, V);
        }
        if (Lock)
        {
            transform.Find("Options_Menu").gameObject.SetActive(false);
            Lock = false;
        }
    }

    public void ResetOptions()
    {
        for (int i = 0; i < slides.Count; i++)
        {
            slides[i].value = def[i];
        }
    }

    public void SetOption(string Name, float value)
    {
        foreach (Slider S in slides)
        {
            if(S.transform.parent.name == Name)
            {
                S.value = value;
            }
        }
    }

    public CustomDifficulty[] difficulties;

    public void SetDifficulty(int difficulty)
    {
        CustomDifficulty dif;
        if(!(difficulty >= 0 && difficulty < difficulties.Length))
        {
            dif = difficulties[0];
        }
        else
        {
            dif = difficulties[difficulty];
        }
        SetOption("Game Speed", dif.GameSpeed);
        SetOption("Exaggeration", dif.Exaggeration);
        SetOption("Cave Height", dif.CaveHeight);
        SetOption("CH Variation", dif.CaveHeightVariation);
        SetOption("CH Speed", dif.CaveHeightSpeed);
        SetOption("Cave Climb", dif.CaveClimb);
        SetOption("CC Speed", dif.CaveClimbSpeed);
    }

    [System.Serializable]
    public struct CustomDifficulty
    {
        public float GameSpeed;
        public float Exaggeration;
        public float CaveHeight;
        public float CaveHeightVariation;
        public float CaveHeightSpeed;
        public float CaveClimb;
        public float CaveClimbSpeed;
    }
}
