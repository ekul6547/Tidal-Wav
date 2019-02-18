using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RetroBars : MonoBehaviour {

    public GameObject Template;
    public int barAmount;

    public List<GameObject> bars = new List<GameObject>();
    RectTransform rTransform;

    public float barWidth;
    public float height;

    void Awake()
    {
        Init();
    }

    void Init()
    {
        bars.Clear();
        rTransform = gameObject.GetComponent<RectTransform>();
        barWidth = rTransform.rect.width / (barAmount*2);
        height = rTransform.rect.height;
        for(int i = 0; i < barAmount*2; i++)
        {
            GameObject newBar = Instantiate(Template, transform);
            newBar.SetActive(true);
            RectTransform barRectT = newBar.GetComponent<RectTransform>();
            barRectT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, barWidth);
            barRectT.SetPositionAndRotation(new Vector3(barRectT.rect.xMin + ((i+1) * barWidth), 0, 0), new Quaternion());
            newBar.name = i.ToString();
            bars.Add(newBar);
        }
    }
	void Start () {
		
	}
	void Update () {
		
	}
}
