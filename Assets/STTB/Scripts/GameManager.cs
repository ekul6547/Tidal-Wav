using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour {

    #region Varaibles

    public static GameManager active; //Static instance of the script - easy access
    [Header("Main")]
    public string GameScene; //InGame Scene Name
    public bool MenuDebug = false;
    public static string GameMode = "AudioFile";
    public bool IsInGame = false;
    public Dropdown down;
    public controller control;
    public GeneratePlatform gen;
    GameObject hook;
    public EventSystem e;
    public IDBManager DataBaseManager;
    public PointSystem Points;
    public GameObject popupObj;
    public MenuModes menuModes;

    [Space]
    [Header("Audio")]
    public AudioData activeData; //Current activeData
    public AudioSource audio_Music
    {
        get
        {
            return GetComponents<AudioSource>()[0];
        }
    } //Get the audiosource
    public AudioSource audio_SFX
    {
        get
        {
            return GetComponents<AudioSource>()[1];
        }
    }
    public AudioClip activeClip
    {
        get
        {
            return audio_Music.clip;
        }
        private set
        {
            if(audio_Music.clip != null)
                audio_Music.clip.UnloadAudioData(); //Unload previous clip
            audio_Music.clip = value;
            if (value == null) return;

            DebugMobile("Clip set to " + value.name);
            audio_Music.clip.LoadAudioData(); //Load new clip
            audioData.clear();
        }
    }  //Currently playing clip
    public List<AudioData> availableClips = new List<AudioData>();
    public GraphData audioData = new GraphData(4);

    [Space]
    [Header("Control Variables")]
    public int LateRed;
    public int totalGen = 0;
    //Variables for when the song finishes, and a small delay before exit
    private float exitCountDown = 0.0f;
    private float preExit; 
    public float PitchThreshold = 0.1f;//Minimum pitch
    private bool canExit;
    [Range(0, 4)]
    public float MenuYChange = 0.5f;
    public bool DoRainbowColors = true;
    public string Mode = "normal";

    [Space]
    public List<GameOption> options = new List<GameOption>();
    [Space]
    public List<Color> blockColours = new List<Color>() { Color.red, Color.magenta, Color.blue, Color.cyan, Color.green, Color.yellow }; //List of colours

    public UnityEvent OnPickupCollectable;

    public GameObject playerSkinPrefab;
    public Texture2D platformSkinTexture;
    public Material backgroundSkinTexture;
    protected string _MenuMode = "";
    public string MenuMode
    {
        get
        {
            return _MenuMode;
        }
        set
        {
            if(menuModes != null)
                menuModes.SetMenuByName(value);
            _MenuMode = value;
        }
    }
    #endregion

    #region Start and Exit
    //Start the game from menu
    //Used for button reference
    public void StartGame()
    {
        active.StartGameActive();
    }
    //Main Start
    public void StartGameActive()
    {
        if (exitCountDown != 0)
        {
            return;
        }
        SceneManager.LoadSceneAsync(GameScene, LoadSceneMode.Single);
        DebugMobile("Starting Game");
        CancelInvoke("StartGameActive");
        canExit = true;
        totalGen = 0;
    }
    //Exit to EndGame Menu after cooldown
    public void ExitToOutGame(int red, float cooldown)
    {
        LateRed = red;
        if (exitCountDown == 0.0f)
        {
            exitCountDown = cooldown + 0.001f;
        }
        DebugMobile("Starting Exit countdown");
    }
    //Exit to End Game
    public void ExitToOutGame()
    {
        if (canExit)
        {
            if (gen != null)
                gen.ResetPlatforms();
            SceneManager.LoadScene("OutGame");
            DebugMobile("Exiting to OutGame");
            canExit = false;
        }
    }

    //Exit to Menu
    public void ExitToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
    //Directly Quit the Application
    public void QuitGame()
    {
        DebugMobile("Exit by Button");
        Application.Quit();
    }

    public void Retry()
    {
        InvokeRepeating("StartGameActive", 0.1f, 0.1f);
    }
    public void OnMenu()
    {
        GetHooks();
        GameObject skinObj = GameObject.Find("Skins_Menu");
        DebugMobile("Loaded Menu");
        if (skinObj != null)
            skinObj.GetComponent<SetSkin>().SetByPrefabName(playerSkinPrefab.name);

        DownList();
        RandomClip();
        menuModes.Invoke("Setup",0.1f);
    }
    #endregion

    [System.Serializable]
    public class GameOption
    {
        public string name = "Option";
        public float value = 0f;
        public GameOption (string N, float V)
        {
            name = N;
            value = V;
        }
    }

    //Struct to store Data for an audio clip without actually loading or creating one
    [System.Serializable]
    public struct AudioData
    {
        public string name; //Name to display
        public string path; //Path to the clip file

        public AudioData(string InputPath)
        {
            path = InputPath;
            name = Path.GetFileNameWithoutExtension(InputPath);
        }
        public AudioData(string N, string InputPath)
        {
            path = InputPath;
            name = N;
        }
        //Get, but not load the audio clip
        public AudioClip GetAudioClip()
        {
            WWW www = new WWW("file://" + path);
            AudioClip newClip = www.GetAudioClip(false, false);
            newClip.name = name;
            return newClip;
        }
    }

    void Awake()
    {
        DataBaseManager = gameObject.AddComponent<DBManager>();
        Points = gameObject.GetComponent<PointSystem>() ?? gameObject.AddComponent<PointSystem>();
        GetHooks();
        if (MenuDebug)
            debugText.gameObject.SetActive(true);
        if (active == null)
        {
            DontDestroyOnLoad(gameObject);
            active = this;
        }
        else if (active != this)
        {
            Destroy(gameObject);
        }
        DownList();
    }

    public void Update()
    {
        audio_Music.loop = !IsInGame && (WWWData.url == "");
        if (activeClip != null && WWWData.url == "")
        {
            if (canActivePlay())
            {
                if (!audio_Music.isPlaying)
                {
                    DebugMobile("Set to Play");
                    audio_Music.Play();
                }
            }
            else
            {
                activeClip.LoadAudioData();
            }
        }
        else
        {
            if (WWWData.isActive)
            {
                UpdateWWW();
            }
        }

        //Countdown to Exit
        if (exitCountDown > 0.0f)
        {
            exitCountDown -= 1 * Time.deltaTime;
            if (exitCountDown == 0.0f)
            {
                exitCountDown -= 0.1f;
            }
        } 
        if(exitCountDown < 0.0f)
        {
            ExitToOutGame();
            exitCountDown = 0.0f;
        }
        //If the dropdown is still selected after picking an item, deselect it
        if (GameObject.Find("Dropdown List") == null && e != null)
        {
            if(e.currentSelectedGameObject == down.gameObject)
            {
                e.SetSelectedGameObject(null);
            }
        }
        //checkDown = checkDown > -1 ? checkDown - 1 : -1; //Countdown - small delay
    }

    #region WWW methods

    [System.Serializable]
    public class held_data
    {
        public bool isActive = false;
        public WWW www;
        public string url = "";
        public AudioClip clip;
        public float timer;
        public float interval = 10;
        public bool played = false;
        public float[] audioTime = new float[2];
        public float playTime;
    }

    public held_data WWWData = new held_data();


    public void TestURL()
    {
        GetComponent<ImportTrack>().TestURL();
    }

    private void StartWWW()
    {
        WWWData.isActive = true;
    }
    private void ForceWWWClip()
    {
        WWWData.clip = WWWData.www.GetAudioClip(false, true,AudioType.MPEG);
        WWWData.clip.name = Time.time.ToString();
    }
    private void ForceWWW()
    {
        WWWData.www = new WWW(WWWData.url);
    }
    private void UpdateWWW()
    {
        WWWData.timer = WWWData.timer + 1 * Time.deltaTime; //Mathf.FloorToInt(Time.timeSinceLevelLoad*10); 
                                            //Time.frameCount; 

        if (WWWData.timer >= WWWData.interval)
        {             //if(timer%interval == 0){
            if (WWWData.www != null)
            {
                WWWData.www.Dispose();
                WWWData.www = null;
                WWWData.played = false;
                WWWData.timer = 0;
            }
        }
        else
        {
            if (WWWData.www == null)
            {
                WWWData.www = new WWW(WWWData.url);
            }
        }
        if (WWWData.clip == null)
        {
            if (WWWData.www != null)
            {
                WWWData.clip = WWWData.www.GetAudioClip(false, true,AudioType.MPEG);
                WWWData.clip.name = Time.time.ToString();
                WWWData.playTime = Time.time;
            }
        }

        if (WWWData.clip != null)
        {
            if (Time.time - 0.05f > WWWData.playTime && !WWWData.played)
            {
                DebugMobile("Played " + WWWData.clip.name + " at " + Time.time);
                audio_Music.clip = WWWData.clip;
                audio_Music.Play();
                WWWData.played = true;
                WWWData.clip = null;
            }
        }
        WWWData.audioTime[0] = audio_Music.time;
        WWWData.audioTime[1] = audio_Music.clip.length;
    }

    #endregion

    #region Misc
    //Set activeClip as random
    public void RandomClip()
    {
        int n = Random.Range(0, availableClips.Count);
        SetClip(n); 
        down.value = n;
    }

    void GetHooks()
    {
        hook = GameObject.Find("MenuHook");
        if(hook != null)
        {
            var hookList = hook.GetComponent<MenuHook>().hooks;

            e = hookList[1].GetComponent<EventSystem>();
            
            down = hookList[0].GetComponent<Dropdown>();
            
            debugText = hookList[2].GetComponent<Text>();

            popupObj = hookList[3]; 

            GameObject MenuModeObj = hookList[4];
            if(MenuModeObj != null)
            {
                menuModes = MenuModeObj.GetComponent<MenuModes>();
            }
        }
    }


    //Set and Get custom options
    public void SetOption(string optionName, float value)
    {
        foreach (GameOption O in options)
        {
            if (O.name == optionName)
            {
                O.value = value;
                return;
            }
        }
        options.Add(new GameOption(optionName, value));
    }
    public float GetOption(string optionName)
    {
        foreach (GameOption O in options)
        {
            if (O.name == optionName)
            {
                return O.value;
            }
        }
        DebugMobile("Option not found: " + optionName + ". Returning 0",1);
        return 0f;
    }

    public void CallFunction(string functionName)
    {
        Invoke(functionName, 0);
    }

    #endregion

    #region Audio

    //Does the list contain a clip already?
    bool contains(AudioData clip)
    {
        if (availableClips.Contains(clip))
        {
            return true;
        }
        foreach(AudioData c in availableClips)
        {
            if (clip.name == c.name)
                return true;
        }
        return false;
    }

    //Add clip to list
    public bool AddClip(AudioData clip)
    {
        if (!contains(clip))
        {
            availableClips.Add(clip);
            DownList();
            return true;
        }
        else
        {
            return false;
        }
    }
    public void ClearTracks()
    {
        availableClips.Clear();
    }

    //Update the DropDown list with the list of clips available
    void DownList()
    {
        if (down != null)
        {
            down.options.Clear();
            foreach (AudioData clip in availableClips)
            {
                down.options.Add(new Dropdown.OptionData(clip.name));
            }
        }
    }

    //Set the active clip
    public void SetClip(AudioClip clip)
    {
        activeClip = clip;
        if(WWWData.www != null)
        {
            WWWData.www.Dispose();
            WWWData.www = null;
        }
        WWWData.url = "";
        WWWData.isActive = false;
    }
    public void SetClip(AudioData dt)
    {
        SetClip(dt.GetAudioClip());
        activeData = dt;
    }
    public void SetClip(int I)
    {
        if (I < availableClips.Count)
        {
            SetClip(availableClips[I]);
        }
    }

    public void SetClip(string URL)
    {
        DebugMobile("Clip streaming from: " + URL);
        WWWData.url = URL;
        WWWData.timer = 0;
        activeClip = null;
        ForceWWW();
        Invoke("ForceWWWClip", 0.1f);
        Invoke("StartWWW", 10);
        
    }

    public bool canActivePlay()
    {
        return activeClip.loadState == AudioDataLoadState.Loaded;
    }

    [System.Serializable]
    public struct GraphData
    {
        public AnimationCurve[] left;
        public AnimationCurve[] right;
        public GraphData(int amount)
        {
            left = new AnimationCurve[amount];
            right = new AnimationCurve[amount];
        }
        public void clear()
        {
            for (int i = 0; i < left.Length; i++)
                left[i].keys = new Keyframe[1] { new Keyframe(0, 0) };
            for (int i = 0; i < right.Length; i++)
                right[i].keys = new Keyframe[1] { new Keyframe(0, 0) };
        }
        public void Set(int channel, int c, Keyframe value)
        {
            if (channel==0)
                left[c].AddKey(value);
            else
                right[c].AddKey(value);
        }
        public float Get(int channel, int c, float time)
        {
            if (channel == 0)
                return left[c].Evaluate(time);
            else
                return right[c].Evaluate(time);
        }

        public float[] GetFromTime(int channel, int c, int key, int amount, bool getFromBeforeTime = false)
        {
            Keyframe[] temp;
            if (channel == 0)
                temp = left[c].keys;
            else
                temp = right[c].keys;

            if(!(key >= 0 && key < temp.Length))
                return new float[0];

            int length = Mathf.Min(temp.Length, amount);
            float[] ret = new float[length];

            for (int i = 0; i < length; i ++)
            {
                if (getFromBeforeTime) {
                    ret[i] = temp[key - i].value;
                }
                else
                {
                    ret[i] = temp[key + i].value;
                }
            }

            return ret;
        }

        public int GetKeyAtTime(int channel, int c, float timeInput)
        {
            Keyframe[] temp;
            if (channel == 0)
                temp = left[c].keys;
            else
                temp = right[c].keys;

            for(int i = 0; i < temp.Length; i++)
            {
                if(temp[i].time > timeInput)
                {
                    return Mathf.Max(i - 1,0);
                }
            }
            return temp.Length-1;
        }

        public bool hasGreaterTime(int channel, int c, float timeInput)
        {
            Keyframe[] temp;
            if (channel == 0) temp = left[c].keys;
            else temp = right[c].keys;
            if (temp.Length <= 1)
                return false;

            bool ret = temp[temp.Length - 1].time > timeInput;

            return ret;

        }

    }

    public void GetSpeakerAudio()
    {

    }

    #endregion

    #region Debug and Messages

    public Text debugText;
    public List<string> debugLines = new List<string>();
    public int debugLimit = 40;
    public string debug_var
    {
        set
        {
            if (debugLines.Count+1 > debugLimit)
                debugLines.RemoveAt(0);
            debugLines.Add(value);
            Text_Debug(debugLines.ToArray());
        }
    }

    void Text_Debug(string[] text)
    {
        if (debugText == null) return;
        string output = "";
        if (text.Length > 0) {
            output = text[0];
            if(text.Length > 1)
                for (int i = 1; i < text.Length; i++)
                {
                    output += "\n" + text[i];
                }
        }
        debugText.text = output;
    }

    /// <summary>
    /// Display in both log and text screen
    /// </summary>
    /// <param name="text">text to log</param>
    /// <param name="mode">0 = normal, 1 = warning, 2 = error</param>
    public void DebugMobile(string text, int mode = 0)
    {
        switch (mode)
        {
            default:
                Debug.Log(text);
                break;
            case 1:
                Debug.LogWarning(text);
                break;
            case 2:
                Debug.LogError(text);
                break;
        }
        debug_var = text;
    }

    public void MenuPopup(string message)
    {
        popupObj.SetActive(true);
        popupObj.transform.Find("Text").GetComponent<Text>().text = message;
        DebugMobile("Popup shown: " + message,1);
    }

    #endregion
}
