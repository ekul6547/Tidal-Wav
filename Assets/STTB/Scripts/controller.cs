using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class controller : MonoBehaviour {

    #region Variables
    new AudioSource audio
    {
        get
        {
            return GameManager.active.audio_Music;
        }
    }
    public bool isInGame;
    [Range(0.0f,150f)]
    public float speedMulti = 3F;

    [Header("Cave Height")]
    public float disMin;
    private float disMax;
    [Range(0.0f,10f)]
    public float disSize = 3;
    public float dis;
    public float toDis;
    public float disSpeed = 0.01f;

    [Header("Bar Scale")]
    public float scale;
    public float randScale = 1f;

    [Header("Cave Climb")]
	public float yEdit;
	public float yETo;
    [Range(0.0f,5.0f)]
	public float yEMax = 8f;
    public float yAdd = 0f;
    [Range(50f,200f)]
    public float yErate = 50f;

    [Range(-3,3),Header("Music Options")]
    public float musicPitch = 1f;
    public Slider ProgressBar;

    private bool finished = false;
    #endregion

    void Awake()
    {
        LoadOptions();
    }

    // Use this for initialization
    void Start () {
        isInGame = SceneManager.GetActiveScene().name == "InGame";
        GameManager.active.IsInGame = isInGame;
        GameManager.active.control = this;
        if (audio != null)
        {
            if (GameManager.active != null)
            {
                if(!isInGame) //If Menu, not Game
                    GameManager.active.OnMenu();
                audio.clip = GameManager.active.activeClip;
            }
            audio.Play();
        }
        disMax = disMin + disSize;
        dis = disMax;
        newScale();
    }

    // Update is called once per frame
    void Update () {
        LoadOptions();

        //Cave height variation
	    if(dis == toDis)
        {
            newScale();
        }else{
            if(dis < toDis)
            {
                dis += disSpeed;
            }else
            {
                dis -= disSpeed;
            }
            dis = Mathf.Round(dis * 100f) / 100f;
        }
        
			newYEdit(3);
            yAdd = (yETo - yEdit) / yErate;
            yAdd = Mathf.Ceil(yAdd * 10000f) / 10000f;
            yEdit += yAdd;
            yEdit = Mathf.Round(yEdit * 10000f) / 10000f;

        if (audio != null)
        {
            audio.pitch = musicPitch;
            if (finished && GameManager.active != null && isInGame) //If finished in game - Exit after delay
            {
                GameManager.active.ExitToOutGame(redAmount,1f);
            }
            if(!audio.isPlaying && !isInGame) //If not playing, and if menu
            {
                audio.Play();
            }
            //Progress bar
            if(ProgressBar != null && audio.clip != null)
            {
                progress = (audio.time) / audio.clip.length; //Decimal percentage of time in audio / total length - between 0 and 1
                if(finished) //If finished playing, make the cave height increase masively to make it seem like a cliff
                    dis += 0.5f;
            }
        }
	}

    #region Misc
    void LoadOptions()
    {
        if (GameManager.active != null)
        {
            speedMulti = GameManager.active.GetOption("Game Speed");
            disMin = GameManager.active.GetOption("Cave Height");
            disSize = GameManager.active.GetOption("CH Variation");
            disSpeed = GameManager.active.GetOption("CH Speed");
            yEMax = GameManager.active.GetOption("Cave Climb");
            yErate = GameManager.active.GetOption("CC Speed");
            musicPitch = GameManager.active.GetOption("Pitch");
        }
    }
    public void ForceExitGame()
    {
        GameManager.active.ExitToOutGame(redAmount, 0);
    }
    
    void newScale()
    {
        disMax = disMin + disSize;
        toDis = Random.Range(disMin,disMax);
		toDis = Mathf.Round(toDis * 100f) / 100f;
    }

	void newYEdit(int graph)
	{
        yETo = GameManager.active.audioData.Get(0, graph, audio.time);
        if (graph < 2)
            yETo /= 250;
        else
            yETo *= 2;
        yETo += Random.Range(-yEMax,yEMax);
        //Debug.Log(yETo);
        yETo = Mathf.Round(yETo * 100f) / 100f;
	}
    public float progress {
        get
        {
            return ProgressBar.value;
        }
        private set
        {
            ProgressBar.value = Mathf.Max(ProgressBar.value, value);
            if (value == 0 && ProgressBar.value > 0) finished = true;
        }
    }
    #endregion
    
    #region Red Control
    [Header("Red Options")]
    public int redAmount;
    public Text redText;

    public void addRed()
    {
        redAmount++;
        redText.text = redAmount.ToString();
    }

    #endregion
}
