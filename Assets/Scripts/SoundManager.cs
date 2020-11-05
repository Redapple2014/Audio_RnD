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

//[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{

    public static SoundManager Instance;
    DirectoryInfo MusicFolder;

    string myPath;
    private GameObject[] listings;
    private int index = 0;
    public GameObject listingPrefab;
    public Transform listingsParent;
    public int songNumber = 0;
    public TextMeshProUGUI btn;

    public GameObject Play_Button;
    public GameObject Pause_Button;
    public GameObject Next_Button;
    public GameObject Previous_Button;
    public GameObject Mute_Button;
    public GameObject Unmute_Button;
    public GameObject Loop_Button;
    public GameObject Unloop_Button;
    public GameObject playMusic_Button;
    public GameObject Middle_Control_Panel;
    public GameObject NoMiddle_panel;
    public GameObject Panel;
    public GameObject leftPanel;
    public List<AudioClip> audioClips = new List<AudioClip>();
    public int currentTrack = 0;
    public Text songName;
    public Text timeText;
    int fullLength, playTime, sec, min;
    public Slider progressBar, Volume_Slider;
    public bool isPaused = false;
    private List<string> newFiles = new List<string>();

    [SerializeField] private GameObject folderItemPrefab;
    [SerializeField] private Transform folderItemHolder;
    [SerializeField] private Button ok_Button;
    [SerializeField] private Button exitBtn_SelectionPopup;
    [SerializeField] private GameObject folderSelectionPopup;
    [SerializeField] private GameObject loadingPopup;
    [SerializeField] private RectTransform equalizerMenu;

    private AudioSource audioSource;
    [SerializeField] private AudioSource surroundSoundAC;
    private ResonanceAudioSource resonanceAudio;
    private bool isEqualizerOn = false;
    public bool isSurroundOn = false;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioReverbZone reverbZone;


    // Use this for initialization
    void Awake()
    {
        
        Instance = this;
        // audioSource = GetComponent<AudioSource>();
#if UNITY_ANDROID
        string[] dataPath = (Application.persistentDataPath.Replace("Android", "")).Split(new string[] { "//" }, System.StringSplitOptions.None);   //"/storage/sdcard0/Music"
        Debug.Log(dataPath + "persistant data path : " + dataPath[0]);
        //  string t_Path = Path.Combine(dataPath[0], "Songs"); Its working no need to try in spesific folder
        myPath = dataPath[0]; //  storage/emulated/0
        Debug.Log("Source Root Path: " + dataPath[0]);

#elif UNITY_STANDALONE
        myPath = "D:/PersonalProjects/AudioTest/Assets/Music";  //C:/Users/HUNNY/Music/ogg
        Debug.Log("in awake");

#elif Unity_Editor
		    myPath = "Builds/Music";
#endif
        MusicFolder = new DirectoryInfo(myPath);
        ok_Button.onClick.AddListener(() => { CreateMusicLibrary(); loadingPopup.SetActive(true); folderSelectionPopup.SetActive(false); });
        exitBtn_SelectionPopup.onClick.AddListener(() => { folderSelectionPopup.SetActive(false); });
        resonanceAudio = surroundSoundAC.gameObject.GetComponent<ResonanceAudioSource>();
        SetUpAudioPlayeForNomal();

    }
    private void OnEnable()
    {
        ButtonSelectionSound.onAudioSelected += SetUpAudioPlayerAsModeSelected;


    }
    private void OnDisable()
    {
        ButtonSelectionSound.onAudioSelected -= SetUpAudioPlayerAsModeSelected;
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
              //  resonanceAudio.bypassRoomEffects = true;
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
        //  UpdateMusicLibrary();


        Volume_Slider.value = 0.5f;

    }
    private IEnumerator StartLoading()
    {
        yield return new WaitForSeconds(2.0f);
        loadingPopup.SetActive(false);
        folderSelectionPopup.SetActive(true);
        InitialiseMusicFolders();
    }

    /* public MusicType e_MusicType;
     public void ChangeAudioEffect(AudioMixerGroup _audioMixerGroup, MusicType _musicType)
     {
         switch (_musicType)
         {
             case MusicType.Normal:
                 {
                     SetUpAudioPlayeForNomal();
                     audioSource.outputAudioMixerGroup = _audioMixerGroup;
                     break;
                 }
             case MusicType.Surround:
             {
                     SetUpAudioPlayeForSurround();
                     audioSource.outputAudioMixerGroup = _audioMixerGroup;
                     break;
             }
         }

     }*/
    private void MoveAnimation(RectTransform rect, float lastPos, float time)
    {
        rect.DOAnchorPosX(lastPos, time).SetEase(Ease.OutBounce);
    }
    public void changeEqualizerState()
    {

        if (!isEqualizerOn)
        {
            //  equalizerMenu.SetActive(true);
            MoveAnimation(equalizerMenu, 0, 1);
            isEqualizerOn = true;

        }
        else
        {
            //  equalizerMenu.SetActive(false);
            MoveAnimation(equalizerMenu, 100, 1);
            isEqualizerOn = false;
        }
    }


    async Task<AudioClip> LoadClip(string path)
    {
        AudioClip clip = null;
        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.MPEG))
        {
            uwr.SendWebRequest();

            // wrap tasks in try/catch, otherwise it'll fail silently
            try
            {
                while (!uwr.isDone) await Task.Delay(1);

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

    private List<string> _excludedDirectories = new List<string>() { "Android", "." };
    List<string> all_Directories = new List<string>();
    private List<FolderItemSelection> folders = new List<FolderItemSelection>();
    static bool isExcluded(List<string> exludedDirList, string target)
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
    private void CreateMusicLibrary()
    {
        folders.ForEach(x =>
        {
            if (!string.IsNullOrEmpty(x.GetFolderName()))
            {
                all_Directories.Add(x.FullDirectoryName);
            }
        });
        UpdateMusicLibrary();
    }
    private async void UpdateMusicLibrary()
    {
        // string[] root_Directories = Directory.GetDirectories(myPath);
        //     List<string> filteredDirs = Directory.GetDirectories(myPath).Where(d => !isExcluded(_excludedDirectories, d)).ToList();
        //  Debug.Log("directory name filtered" + filteredDirs);
        //  all_Directories = filteredDirs;
        Debug.Log("all directories: " + all_Directories.Count);
        for (int i = 0; i < all_Directories.Count; i++)
        {
            //   DirectoryInfo directory = new DirectoryInfo(all_Directories[i]);
            //   Debug.Log("Directory Properties : " + directory.Name + "----------" + directory.FullName);
            newFiles.AddRange(Directory.GetFiles(all_Directories[i], "*.mp3", SearchOption.AllDirectories).ToList());
            //  newFiles.AddRange(Directory.GetFiles(directory.FullName, "*.wav", SearchOption.AllDirectories).ToList()); if we need wav file to play as well
        }

        Debug.Log("files count:" + newFiles.Count);
        int length = newFiles.Count;
        Debug.Log("length" + length);
        if (length > 0)
        {
            audioClips = new List<AudioClip>(length);
            listings = new GameObject[length];

            for (int i = 0; i < length; i++)
            {
                FileInfo file = new FileInfo(newFiles[i]);
                Debug.Log("all files name " + file.Name);
                if (!(file.FullName.Contains("wav") || file.FullName.Contains("mp3")))
                    continue;
                Debug.Log("main file name: " + newFiles[i]);
                AudioClip clip = await LoadClip("file:///" + file.FullName);
                audioClips.Add(clip);
                audioClips[i].name = file.Name;
                Debug.Log("download complete" + audioClips[i].name.ToString());
                songNumber++;
                GameObject obj = Instantiate(listingPrefab);
                obj.GetComponent<ListObject_Main>().songName.text = file.Name;
                obj.transform.SetParent(listingsParent);
                obj.name = file.Name;
                obj.GetComponent<RectTransform>().localScale = Vector3.one;
                listings[i] = obj;
                Debug.Log("song Name: " + audioClips[i].name);

            }
            audioSource.clip = audioClips[index];
            songName.text = audioClips[index].name;
        }
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
        StartCoroutine("waitformusic");
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

        StartCoroutine("waitformusic");
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

        StartCoroutine("waitformusic");
    }

    public void pause()
    {

        Play_Button.SetActive(true);
        Pause_Button.SetActive(false);

        audioSource.Pause();
        isPaused = true;
        StopCoroutine("waitformusic");
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

