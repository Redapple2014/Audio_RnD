using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.IO;
using TMPro;
using UnityEngine.Networking;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine.Audio;
using System.Runtime.Serialization.Formatters.Binary;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    private DirectoryInfo MusicFolder;
    private string myPath;
    private GameObject[] listings;
    private int index = 0;   
    private int songNumber = 0;
    private int currentTrack = 0;
    private AudioSource audioSource;
    private List<string> newFiles = new List<string>();
    private List<AudioClip> audioClips = new List<AudioClip>();
    private int fullLength, playTime, sec, min;
    private ResonanceAudioSource resonanceAudio;
    private bool isEqualizerOn = false;
    private bool isSurroundOn = false;
    private List<string> _excludedDirectories = new List<string>() { "Android", "." };
    private List<string> all_Directories = new List<string>();
    private List<FolderItemSelection> folders = new List<FolderItemSelection>();

    [SerializeField] private GameObject listingPrefab;
    [SerializeField] private Transform listingsParent;
    [SerializeField] private GameObject Play_Button;
    [SerializeField] private GameObject Pause_Button;
    [SerializeField] private GameObject Next_Button;
    [SerializeField] private GameObject Previous_Button;
    [SerializeField] private GameObject Mute_Button;
    [SerializeField] private GameObject Unmute_Button;
    [SerializeField] private GameObject Loop_Button;
    [SerializeField] private GameObject Unloop_Button;
    [SerializeField] private GameObject playMusic_Button;
    [SerializeField] private GameObject Middle_Control_Panel;
    [SerializeField] private GameObject NoMiddle_panel;
    [SerializeField] private GameObject Panel;
    [SerializeField] private GameObject leftPanel;
    [SerializeField] private Text songName;
    [SerializeField] private Text timeText;
    [SerializeField] private Slider progressBar, Volume_Slider;
    [SerializeField] private bool isPaused = false;  
    [SerializeField] private GameObject folderItemPrefab;
    [SerializeField] private Transform folderItemHolder;
    [SerializeField] private Button ok_Button;
    [SerializeField] private Button exitBtn_SelectionPopup;
    [SerializeField] private GameObject folderSelectionPopup;
    [SerializeField] private GameObject loadingPopup;
    [SerializeField] private RectTransform equalizerMenu;   
    [SerializeField] private AudioSource surroundSoundAC;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioReverbZone reverbZone;
    [SerializeField] private PooledListView pooledListView;
    [SerializeField] private ListViewItemPool listViewItemPool;

    public static SoundManager Instance => instance;
    public bool IsSurroundOn => isSurroundOn;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }       
#if UNITY_ANDROID
        string[] dataPath = (Application.persistentDataPath.Replace("Android", "")).Split(new string[] { "//" }, System.StringSplitOptions.None);   //"/storage/sdcard0/Music"
        Debug.Log(dataPath + "persistant data path : " + dataPath[0]);    
        myPath = dataPath[0]; //  storage/emulated/0
        Debug.Log("Source Root Path: " + dataPath[0]);

#elif UNITY_STANDALONE
        myPath = "D:/PersonalProjects/AudioTest/Assets/Music";  //C:/Users/HUNNY/Music/ogg
        Debug.Log("in awake");

#elif Unity_Editor
		    myPath = "Builds/Music";
#endif       
        resonanceAudio = surroundSoundAC.gameObject.GetComponent<ResonanceAudioSource>();
        SetUpAudioPlayeForNomal();
    }

    private void OnEnable()
    {
        ok_Button.onClick.AddListener(OnClickOkButton);
        exitBtn_SelectionPopup.onClick.AddListener(OnClickExitToFolder);
        ButtonSelectionSound.onAudioSelected += SetUpAudioPlayerAsModeSelected;
    }

    private void OnDisable()
    {
        ok_Button.onClick.RemoveListener(OnClickOkButton);
        exitBtn_SelectionPopup.onClick.RemoveListener(OnClickExitToFolder);
        ButtonSelectionSound.onAudioSelected -= SetUpAudioPlayerAsModeSelected;
    }

    private void OnClickOkButton()
    {
        loadingPopup.SetActive(true);
        folderSelectionPopup.SetActive(false);
        CreateMusicLibrary();      
    }

    private void OnClickExitToFolder()
    {
        folderSelectionPopup.SetActive(false);
    }

    private void SetUpAudioPlayerAsModeSelected(AudioMixerGroup _audioMixerGroup, MusicType type)
    {
        switch (type)
        {
            case MusicType.Normal:
                {
                    SetUpAudioPlayeForNomal();
                    break;
                }
            case MusicType.Surround:
                {
                    SetUpAudioForSurround();
                    break;
                }
        }
        audioSource.outputAudioMixerGroup = _audioMixerGroup;
    }

    private void SetUpAudioForSurround()
    {
        isSurroundOn = true;
        audioSource.spatialize = true;
        audioSource.spatializePostEffects = true;
        audioSource.bypassReverbZones = false;
        audioSource.dopplerLevel = 1;
        audioSource.minDistance = 15;
        reverbZone.enabled = true;
     //   resonanceAudio.bypassRoomEffects = false;
        animator.SetBool("SurroundOn", true);
        //audioSource.Play();

    }
    private void SetUpAudioPlayeForNomal()
    {
        isSurroundOn = false;
        if (audioSource == null)
        {
            audioSource = surroundSoundAC;
        }
        else
        {
            if (audioSource.isPlaying)
            {

                audioSource.spatialize = false;
                audioSource.spatializePostEffects = false;
                audioSource.bypassReverbZones = false;
                //resonanceAudio.bypassRoomEffects = true;
                audioSource.dopplerLevel = 0;
                audioSource.minDistance = 1;
                reverbZone.enabled = false;
                animator.SetBool("SurroundOn", false);
            }
        }

    }

    public void Start()
    {
        Pause_Button.SetActive(false);
        Play_Button.SetActive(true);

        Mute_Button.SetActive(false);
        Unmute_Button.SetActive(true);

        Next_Button.SetActive(true);
        Previous_Button.SetActive(true);

        Loop_Button.SetActive(true);
        Unloop_Button.SetActive(false);


        Middle_Control_Panel.SetActive(false);
        NoMiddle_panel.SetActive(true);

        loadingPopup.SetActive(true);
        StartCoroutine(StartLoading());
        Volume_Slider.value = 0.5f;
    }
    private IEnumerator StartLoading()
    {
        yield return new WaitForSeconds(2.0f);
        loadingPopup.SetActive(false);
        if (string.IsNullOrEmpty(PlayerPrefs.GetString("MusicFolderPath")))
        {
            Debug.Log("FirstTime");
            folderSelectionPopup.SetActive(true);
            InitialiseMusicFolders();           
        }
        else
        {
            loadingPopup.SetActive(true);
            string musicFolderFilePath = PlayerPrefs.GetString("MusicFolderPath");
            if(File.Exists(musicFolderFilePath))
            {
                FileStream fileStream = new FileStream(musicFolderFilePath, FileMode.Open, FileAccess.Read);
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                List<string> musicFiles = (List<string>)binaryFormatter.Deserialize(fileStream);
                UpdateMusicLibrary(musicFiles);
            }
        }
    }
   
    private void MoveAnimation(RectTransform rect, float lastPos, float time)
    {
        rect.DOAnchorPosX(lastPos, time).SetEase(Ease.OutBounce);
    }

    public void changeEqualizerState()
    {
        if (!isEqualizerOn)
        {            
            MoveAnimation(equalizerMenu, 0, 1);
            isEqualizerOn = true;
        }
        else
        {
            MoveAnimation(equalizerMenu, 100, 1);
            isEqualizerOn = false;
        }
    }

    private async Task<AudioClip> LoadClip(string path)
    {
        AudioClip clip = null;
        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.MPEG))
        {
            uwr.SendWebRequest();
            try
            {
                while (!uwr.isDone) await Task.Yield();

                if (uwr.isNetworkError || uwr.isHttpError) Debug.Log($"{uwr.error}");
                else
                {
                    clip = DownloadHandlerAudioClip.GetContent(uwr);
                }
            }
            catch (Exception err)
            {
                Debug.Log($"{err.Message}, {err.StackTrace}");
            }
        }
        return clip;
    }

    private bool isExcluded(List<string> exludedDirList, string target)
    {
        return exludedDirList.Any(d => new DirectoryInfo(target).Name.Contains(d));
    }

    private void InitialiseMusicFolders()
    {
        DirectoryInfo rootDirectoryInfo = new DirectoryInfo(myPath);
        List<DirectoryInfo> rootDirectories = rootDirectoryInfo.GetDirectories().Where(d => !isExcluded(_excludedDirectories, d.FullName)).ToList();
        rootDirectories.ForEach(x =>
        {
            GameObject go = Instantiate(folderItemPrefab, folderItemHolder);
            go.SetActive(true);
            FolderItemSelection folderSelection = go.GetComponent<FolderItemSelection>();
            folders.Add(folderSelection);
            folderSelection.FullDirectoryName = x.FullName;
            folderSelection.FolderName.text = x.Name;
        });
    }

    public void AddMusicDirectory(string directory)
    {
        all_Directories.Add(directory);
    }

    public void RemoveMusicDirectory(string directory)
    {
        if (all_Directories.Contains(directory))
        {
            all_Directories.Remove(directory);
        }
    }

    private void CreateMusicLibrary()
    {
        for (int i = 0; i < all_Directories.Count; i++)
        {
            newFiles.AddRange(Directory.GetFiles(all_Directories[i], "*.mp3", SearchOption.AllDirectories).ToList());
        }
        string fileName = "MusicNames.dat";
        string filePath = Path.Combine(myPath, fileName);
        if(!File.Exists(filePath))
        {
            File.Create(filePath);
        }
        FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate,FileAccess.Write);
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        binaryFormatter.Serialize(fileStream, newFiles);
        fileStream.Close();
        PlayerPrefs.SetString("MusicFolderPath", filePath);
        UpdateMusicLibrary(newFiles);
    }

    private async void UpdateMusicLibrary(List<string> musicDirectories)
    {              
        ListViewItemModel[] demoData = new ListViewItemModel[musicDirectories.Count];
        listViewItemPool.SetUpPool(musicDirectories.Count);
        //Debug.Log("length" + musicDirectories.Count+ "  "+ demoData.Length);
        if (musicDirectories.Count > 0)
        {
            audioClips = new List<AudioClip>(musicDirectories.Count);           
            for (int i = 0; i < musicDirectories.Count; i++)
            {
                FileInfo file = new FileInfo(musicDirectories[i]);
                AudioClip clip = await LoadClip("file:///" + file.FullName);
                audioClips.Add(clip);
                audioClips[i].name = file.Name;               
                demoData[i] = new ListViewItemModel(file.Name,i+1);                            
                Debug.Log("song Name: "+i+"  " + audioClips[i].name);
            }
            audioSource.clip = audioClips[index];
            songName.text = audioClips[index].name;
            pooledListView.Setup(demoData);
        }
        Debug.Log("length");
        loadingPopup.SetActive(false);
    }

    public void SelectSongByName(string s)
    {
        string seting;
        for (int i = 0; i < audioClips.Count; i++)
        {
            seting = audioClips[i].name;

            if (s.Equals(seting))
            {
                playMine(i);
                break;
            }
        }
    }

    public void playMusicBtn()
    {
        play();
    }

    public void play()
    {
        Play_Button.SetActive(false);
        Pause_Button.SetActive(true);

        if (isPaused == true)
        {
            audioSource.UnPause();
            isPaused = false;
        }
        if (audioSource.isPlaying)
            return;

        currentTrack--;
        if (currentTrack < 0)
            currentTrack = audioClips.Count - 1;
        //  audioSource.Play();
        StartCoroutine(waitformusic());
    }

    IEnumerator waitformusic()
    {
        while (audioSource.isPlaying)
        {
            songName.text = audioSource.clip.name;
            playTime = (int)audioSource.time;
            yield return null;
        }
        next();
    }


    // Update is called once per frame
    void Update()
    {

        if (audioSource.isPlaying)
        {
            showTitle();
            setVol();
            showPlayTime();
            progressBar.maxValue = audioClips[currentTrack].length;
            progressBar.value = audioSource.time;

            float[] spectrum = new float[256];
            AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);
            float highestBass = 0;
            for (int i = 1; i < spectrum.Length - 1; i++)
            {
                if (spectrum[i] > highestBass)
                {
                    highestBass = spectrum[i];
                }
            }
            //  gameObject.GetComponent<BloomOptimized>().threshold = highestBass;
        }

    }


    public void playMine(int value)
    {
        Play_Button.SetActive(false);
        Pause_Button.SetActive(true);
        audioSource.Stop();
        audioSource.clip = audioClips[value];
        audioSource.Play();
    }

    public void next()
    {
        audioSource.Stop();
        currentTrack++;
        if (currentTrack > audioClips.Count - 1)
            currentTrack = 0;
        audioSource.clip = audioClips[currentTrack];
        audioSource.Play();

        // show title

        StartCoroutine(waitformusic());
    }

    public void previous()
    {
        audioSource.Stop();
        currentTrack--;
        if (currentTrack < 0)
            currentTrack = audioClips.Count - 1;
        audioSource.clip = audioClips[currentTrack];
        audioSource.Play();

        // show title

        StartCoroutine(waitformusic());
    }

    public void pause()
    {
        Play_Button.SetActive(true);
        Pause_Button.SetActive(false);

        audioSource.Pause();
        isPaused = true;
        StopCoroutine(waitformusic());
    }

    public void mute()
    {
        Mute_Button.SetActive(false);
        Unmute_Button.SetActive(true);
        audioSource.mute = false;
    }

    public void unmute()
    {
        Mute_Button.SetActive(true);
        Unmute_Button.SetActive(false);
        audioSource.mute = true;
    }

    public void equalizer()
    {
        Middle_Control_Panel.SetActive(true);
        NoMiddle_panel.SetActive(false);
        HidePanel();

    }

    public void unequalizer()
    {
        Middle_Control_Panel.SetActive(false);
        NoMiddle_panel.SetActive(true);
        showPanel();
    }

    void HidePanel()
    {
        Panel.gameObject.SetActive(false);
    }

    void showPanel()
    {
        Panel.gameObject.SetActive(true);
    }

    void showTitle()
    {
        songName.text = audioSource.clip.name;
        fullLength = (int)audioSource.clip.length;
    }

    void showPlayTime()
    {
        sec = playTime % 60;
        min = (playTime / 60) % 60;

        timeText.text = min + ":" + sec.ToString("D2") + "/" + ((fullLength / 60) % 60) + ":" + (fullLength % 60).ToString("D2");
    }

    void setVol()
    {
        audioSource.volume = Volume_Slider.value;
    }
    //   public void HideLeftSide


    public void quit()
    {
        Debug.Log("has quit");
        Application.Quit();
    }

    public void loopMusic()
    {
        Loop_Button.SetActive(false);
        Unloop_Button.SetActive(true);

        audioSource.loop = false;
    }

    public void unloopMusic()
    {
        Loop_Button.SetActive(true);
        Unloop_Button.SetActive(false);
        audioSource.loop = true;
    }
}

