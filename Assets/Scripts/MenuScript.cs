using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System.Xml.Serialization;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Playables;

public class MenuScript : MonoBehaviour
{
    public static MenuScript Instance;
    public static bool isPaused = false;

    public static float gameTime = 0;
    public static int launches = 0;

    public Instructions instructions;
    public GameObject player;
    public Rigidbody2D playerRB;
    public AudioMixer mixer;
    public Animator settingsMenu;
    public PlayableDirector computerDirector;
    public GameObject inGameMenu;
    public GameObject inGameSettings;
    public GameObject windows;
    [Space(10)]
    public Volume postProcessingVolume;
    public float focalLengthLerpSpeed = 10;
    public float menuFocalLength = 67;
    public float gameFocalLength = 28;
    [Space(10)]
    public Cinemachine.CinemachineVirtualCamera menuVCam;
    public Cinemachine.CinemachineVirtualCamera inGameVCam;
    [Space(10)]
    public CurSettings curSettings;

    private bool gameStarted = false;

    private List<string> resolutionSettings;
    public List<string> ResolutionSettings
    {
        get
        {
            if (resolutionSettings == null)
            {
                resolutionSettings = new List<string>();

                int curSelected = 0;
                for (int i = 0; i < resolutions.Length; i++)
                {
                    resolutionSettings.Add($"{resolutions[i].width}x{resolutions[i].height}");

                    if (resolutions[i].width == Screen.currentResolution.width &&
                        resolutions[i].height == Screen.currentResolution.height)
                    {
                        curSelected = i;
                    }
                }
                if (curSettings.Resolution == -1) curSettings.Resolution = curSelected;
            }
            return resolutionSettings;
        }
    }
    private Resolution[] resolutions;

    private DepthOfField dof;
    private string savePath;

    private float lerpTime = 0;
    private float prevFocalLength = 0;
    private float curFocalLength = 0;

    private void Awake()
    {
        Instance = this;

        savePath = Path.Combine(Application.persistentDataPath, "settings");
        curSettings = new CurSettings();

#if !UNITY_EDITOR
        prevFocalLength = menuFocalLength;
        curFocalLength = menuFocalLength;
#else
        prevFocalLength = gameFocalLength;
        curFocalLength = gameFocalLength;
#endif

        postProcessingVolume.sharedProfile.TryGet(out dof);

        //Load settings from file
        if (File.Exists(savePath))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CurSettings));
            using (FileStream stream = new FileStream(savePath, FileMode.Open))
            {
                curSettings = (CurSettings)serializer.Deserialize(stream);
            }
        }

        resolutions = Screen.resolutions;

        CloseInGame();
    }

    public void AddLaunched()
    {
        curSettings.launches++;
    }

    public void StopGame()
    {
        gameStarted = false;
        gameTime = curSettings.gameTime;
        launches = curSettings.launches;
    }

    private void Update()
    {
        if (lerpTime < 1 && dof != null)
        {
            lerpTime += Time.deltaTime * focalLengthLerpSpeed;
            dof.focalLength.value = Mathf.Lerp(prevFocalLength, curFocalLength, lerpTime);
        }

        if (gameStarted && Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;

            inGameMenu.SetActive(isPaused);
            if (!isPaused) inGameSettings.SetActive(false);

            if (isPaused)
            {
                Time.timeScale = 0;
                
                SaveGame();
                SaveSettings();
            } else
            {
                SaveSettings();
                Time.timeScale = 1;
            }
        }

        curSettings.gameTime += Time.deltaTime;
    }

    public void SaveGame()
    {
        UnityEngine.Debug.Log("Saving Game");

        curSettings.PlayerPos = playerRB.transform.position;
        curSettings.PlayerRot = playerRB.transform.rotation;
        curSettings.PlayerVel = playerRB.velocity;
        curSettings.PlayerAngularVel = playerRB.angularVelocity;
        curSettings.hasSave = true;

        SaveSettings();
    }

#region Menu Buttons
    public void IntroFinished()
    {
        menuVCam.m_Priority = 5;
        inGameVCam.m_Priority = 10;
        lerpTime = 0;

        prevFocalLength = curFocalLength;
        curFocalLength = gameFocalLength;

        player.SetActive(true);

        instructions.shouldTutorial = true;

        gameStarted = true;
    }

    public void StartGame()
    {
        SaveSettings();
        settingsMenu.SetBool("open", false);

        InvokeRepeating("SaveGame", 0, 60);

        computerDirector.Play();
    }

    public void GoToMenu()
    {
        CancelInvoke();
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void ContinueGame()
    {
        SaveSettings();

        InvokeRepeating("SaveGame", 60, 60);

        windows.SetActive(false);

        playerRB.transform.position = curSettings.PlayerPos;
        playerRB.transform.rotation = curSettings.PlayerRot;
        playerRB.velocity = curSettings.PlayerVel;
        playerRB.angularVelocity = curSettings.PlayerAngularVel;

        prevFocalLength = gameFocalLength;
        curFocalLength = gameFocalLength;
        lerpTime = 0;

        menuVCam.enabled = false;
        player.SetActive(true);

        gameStarted = true;
    }

    public void MenuSettings()
    {
        settingsMenu.SetBool("open", true);
    }

    public void CloseMenuSettings()
    {
        SaveSettings();
        settingsMenu.SetBool("open", false);
    }

    public void CloseInGame()
    {
        isPaused = false;

        inGameMenu.SetActive(false);
        inGameSettings.SetActive(false);

        Time.timeScale = 1;
    }

    public void InGameSettings()
    {
        inGameSettings.SetActive(true);
    }

    public void InGameCloseSettings()
    {
        SaveSettings();
        inGameSettings.SetActive(false);
    }

    public void QuitGame()
    {
        SaveSettings();
        Application.Quit();
    }
#endregion

#region Settings
    public void SetGraphics(int value)
    {
        QualitySettings.SetQualityLevel(value, true);
        curSettings.Graphics = value;
    }
    public void SetResolution(int value)
    {
        Resolution curRes = Screen.resolutions[value];
        Screen.SetResolution(curRes.width, curRes.height, GetFullScreenMode(curSettings.FullscreenMode));
        curSettings.Resolution = value;
    }
    public void Fullscreen(int value)
    {
        Screen.fullScreenMode = GetFullScreenMode(value);
        curSettings.FullscreenMode = value;
    }
    public void SetMasterVolume(float volume)
    {
        mixer.SetFloat("masterVolume", volume);
        curSettings.Master = volume;
    }
    public void SetPlayerVolume(float volume)
    {
        mixer.SetFloat("playerVolume", volume);
        curSettings.Player = volume;
    }
    public void SetAmbienceVolume(float volume)
    {
        mixer.SetFloat("ambienceVolume", volume);
        curSettings.Ambience = volume;
    }

    public void SaveSettings()
    {
        //Save settings to file
        XmlSerializer serializer = new XmlSerializer(typeof(CurSettings));
        using (FileStream stream = new FileStream(savePath, FileMode.Create))
        {
            serializer.Serialize(stream, curSettings);
        }
    }

    public FullScreenMode GetFullScreenMode(int index)
    {
        switch (index)
        {
            case 0:
                return FullScreenMode.Windowed;
            case 1:
                return FullScreenMode.FullScreenWindow;
            case 2:
                return FullScreenMode.ExclusiveFullScreen;
            default:
                return FullScreenMode.Windowed;
        }
    }

    [System.Serializable]
    public class CurSettings
    {
        //Video
        public int FullscreenMode = 2;
        public int Resolution = -1;
        public int Graphics = 2;

        //Volume
        public float Master = 0;
        public float Player = 0;
        public float Ambience = 0;

        //Game Save
        public bool hasSave = false;
        public Vector3 PlayerPos = Vector3.zero;
        public Quaternion PlayerRot = Quaternion.identity;
        public Vector3 PlayerVel = Vector3.zero;
        public float PlayerAngularVel = 0;

        public float gameTime = 0;
        public int launches = 0;
    }
#endregion
}