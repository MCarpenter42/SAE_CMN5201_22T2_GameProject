using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class AudioController : Core
{
    #region [ PROPERTIES ]

    #region < General >

    [Header("General Settings")]
    [SerializeField] Vector3 levelCentre;
    private List<GameObject> reverbZones = new List<GameObject>();

    #endregion
    
    #region < Sources >

    [Header("Sources")]
    [SerializeField] SFXSource sfxSourcePrefab;
    [HideInInspector] public List<SFXSource> sfxSources = new List<SFXSource>();
    [HideInInspector] public SFXSource playerSFX;
    [HideInInspector] public List<SFXSource> beamSFX = new List<SFXSource>();
    [HideInInspector] public SFXSource doorSFX;
    [HideInInspector] public SFXSource uiSFX { get { return GameManager.UIController.sfx; } }

    #endregion

    #region < Music >

    [Header("Music")]
    [SerializeField] AudioClip musicIntro;
    [SerializeField] AudioClip musicMenu;
    [SerializeField] AudioClip musicSceneLoad;
    [SerializeField] AudioClip musicLevelBright;
    [SerializeField] AudioClip musicLevelDim;
    [SerializeField] AudioClip musicLevelDark;
    [SerializeField] bool useAltMusic = false;
    [SerializeField] AudioClip musicOther;
    [HideInInspector] public AudioSource musicPlayer;
    private float defaultMusicVolume = 0.2f;
    private bool musicPlaying = false;
    public enum MusicTrack { None, Intro, Menu, Loading, Bright, Dim, Dark };

    #endregion

    #region < Player >

    [Header("Player SFX")]
    public List<AudioClip> walkStone = new List<AudioClip>();
    public List<AudioClip> walkWood = new List<AudioClip>();
    public List<AudioClip> walkGrass = new List<AudioClip>();
    public List<AudioClip> walkFoliage = new List<AudioClip>();
    public List<AudioClip> walkWater = new List<AudioClip>();
    private Coroutine playerWalkCycle = null;
    public List<AudioClip> jump = new List<AudioClip>();
    public List<AudioClip> land = new List<AudioClip>();
    private Coroutine playerJumpCycle = null;

    #endregion

    #region < World >

    [Header("World SFX")]
    public List<AudioClip> moveObject = new List<AudioClip>();
    public List<AudioClip> doorOpen = new List<AudioClip>();
    public List<AudioClip> doorClose = new List<AudioClip>();
    public List<AudioClip> doorStop = new List<AudioClip>();
    public AudioClip beamEmitterActive;
    public AudioClip beamEmitterDeactivate;
    public AudioClip beamTriggerActive;
    public AudioClip beamTriggerDeactivate;
    public AudioClip beamHitNormal;
    public AudioClip beamHitReflect;

    #endregion

    #region < UI >

    [Header("UI SFX")]
    public List<AudioClip> buttonStandard = new List<AudioClip>();
    public List<AudioClip> buttonHeavy = new List<AudioClip>();
    public List<AudioClip> slider = new List<AudioClip>();
    [Range(0.0f, 1.0f)]
    [SerializeField] float buttonVolumeScale = 1.0f;

    #endregion

    #region < Ambient >

    [Header("Ambient SFX")]
    public List<AudioClip> stingersBright;
    public List<AudioClip> stingersDim;
    public List<AudioClip> stingersDark;
    public List<AudioClip> breeze;
    public List<AudioClip> fauna;
    public List<AudioClip> foliage;

    public AmbientSource atmosphericSource;
    public AmbientSource breezeSource;
    public AmbientSource faunaSource;
    public AmbientSource foliageSource;

    #endregion

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    void Awake()
    {
        transform.position = Vector3.zero;
        GetComponents();
        foreach(GameObject zone in reverbZones)
        {
            zone.transform.position = levelCentre;
        }
        GetExistingSFX();
        SetupMusic();

    }

    void Start()
    {
        if (musicPlayer != null && musicPlayer.clip != null && GameManager.LevelController.useAudioMusic)
        {
            musicPlayer.volume = 0.0f;
            musicPlayer.Play();
            StartCoroutine(MusicVolumeCheck());
        }

        SetupAmbient();
    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private void GetComponents()
    {
        musicPlayer = gameObject.GetComponent<AudioSource>();
        defaultMusicVolume = musicPlayer.volume;

        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = gameObject.transform.GetChild(i).gameObject;
            if (child.GetComponent<AudioReverbZone>() != null)
            {
                reverbZones.Add(child);
            }
        }

        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            playerSFX = GetChildrenWithComponent<SFXSource>(GameManager.Player.gameObject)[0].GetComponent<SFXSource>();
        }
        if (GameObject.FindGameObjectWithTag("ExitDoor") != null)
        {
            doorSFX = GameObject.FindGameObjectWithTag("ExitDoor").GetComponent<SFXSource>();
        }
    }

    private void GetExistingSFX()
    {
        List<SFXSource> sources = ArrayToList(FindObjectsOfType<SFXSource>());
        CopyListData(sources, sfxSources);
    }

    private void SetupMusic()
    {
        if (!useAltMusic)
        {
            switch (GameManager.LevelController.sceneType)
            {
                case SceneType.LevelGeneric:
                default:
                    musicPlayer.clip = musicOther;
                    break;

                case SceneType.MenuAndUI:
                    musicPlayer.clip = musicMenu;
                    break;

                case SceneType.CutsceneAndTransition:
                    musicPlayer.clip = musicSceneLoad;
                    break;

                case SceneType.LevelBright:
                    musicPlayer.clip = musicLevelBright;
                    break;

                case SceneType.LevelDim:
                    musicPlayer.clip = musicLevelDim;
                    break;

                case SceneType.LevelDark:
                    musicPlayer.clip = musicLevelDark;
                    break;

                case SceneType.LevelSpecial:
                    musicPlayer.clip = musicOther;
                    break;
            }
        }
        else
        {
            musicPlayer.clip = musicOther;
        }
    }

    public void ChangeMusic(MusicTrack track)
    {
        musicPlayer.Stop();
        switch (track)
        {
            default:
            case MusicTrack.None:
                musicPlayer.clip = null;
                break;

            case MusicTrack.Intro:
                musicPlayer.clip = musicIntro;
                break;

            case MusicTrack.Menu:
                musicPlayer.clip = musicMenu;
                break;

            case MusicTrack.Loading:
                musicPlayer.clip = musicSceneLoad;
                break;

            case MusicTrack.Bright:
                musicPlayer.clip = musicLevelBright;
                break;

            case MusicTrack.Dim:
                musicPlayer.clip = musicLevelDim;
                break;

            case MusicTrack.Dark:
                musicPlayer.clip = musicLevelDark;
                break;
        }
        musicPlayer.Play();
        musicPlayer.loop = true;
    }

    public void DelayedPlayMusic(float delay)
    {
        StartCoroutine(IDelayedPlayMusic(delay));
    }
    
    private IEnumerator IDelayedPlayMusic(float delay)
    {
        yield return new WaitForSeconds(delay);
        musicPlayer.Play();
    }

    private void SetupAmbient()
    {
        if (GameManager.LevelController.isGameplayLevel)
        {
            if (atmosphericSource != null && GameManager.LevelController.useAudioAtmospheric)
            {
                int n = 0;
                switch (GameManager.LevelController.sceneType)
                {
                    default:
                    case SceneType.LevelGeneric:
                        break;

                    case SceneType.LevelBright:
                        atmosphericSource.SetClipsList(stingersBright);
                        n = stingersBright.Count;
                        break;

                    case SceneType.LevelDim:
                        atmosphericSource.SetClipsList(stingersDim);
                        n = stingersDim.Count;
                        break;

                    case SceneType.LevelDark:
                        atmosphericSource.SetClipsList(stingersDark);
                        n = stingersDark.Count;
                        break;

                    case SceneType.LevelSpecial:
                        break;
                }
                if (n > 0)
                {
                    atmosphericSource.Play();
                }
            }
            if (breezeSource != null && GameManager.LevelController.useAudioBreeze)
            {
                breezeSource.SetClipsList(breeze);
                if (breeze.Count > 0)
                {
                    breezeSource.Play();
                }
            }
            if (faunaSource != null && GameManager.LevelController.useAudioFauna)
            {
                faunaSource.SetClipsList(fauna);
                if (fauna.Count > 0)
                {
                    faunaSource.Play();
                }
            }
            if (foliageSource != null && GameManager.LevelController.useAudioFoliage)
            {
                foliageSource.SetClipsList(foliage);
                if (foliage.Count > 0)
                {
                    foliageSource.Play();
                }
            }
        }
    }

    #region [ SFX SOURCE MANAGEMENT ]

    public int CreateSFXSource(Vector3 pos)
    {
        int n = sfxSources.Count;

        SFXSource sfxSource = Instantiate(sfxSourcePrefab, pos, Quaternion.identity);
        sfxSources.Add(sfxSource);

        return n;
    }

    public int CreateSFXSource(Vector3 pos, AudioClip clip)
    {
        int n = sfxSources.Count;

        SFXSource sfxSource = Instantiate(sfxSourcePrefab, pos, Quaternion.identity);
        sfxSource.SetAudioClip(clip);
        sfxSources.Add(sfxSource);

        return n;
    }
    
    public bool DestroySFXSource(int index)
    {
        bool indexInBounds = InBounds(index, sfxSources);
        if (indexInBounds)
        {
            Destroy(sfxSources[index], 0.05f);
            sfxSources.RemoveAt(index);
        }
        return indexInBounds;
    }

    public int CreateSFXSource(Vector3 pos, string listName)
    {
        int n = sfxSources.Count;

        SFXSource sfxSource;
        switch (listName)
        {
            case "beamSFX":
                n = beamSFX.Count;
                sfxSource = Instantiate(sfxSourcePrefab, pos, Quaternion.identity);
                beamSFX.Add(sfxSource);
                break;

            case "sfxSources":
            default:
                n = sfxSources.Count;
                sfxSource = Instantiate(sfxSourcePrefab, pos, Quaternion.identity);
                sfxSources.Add(sfxSource);
                break;
        }

        return n;
    }

    public int CreateSFXSource(Vector3 pos, AudioClip clip, string listName)
    {
        int n = sfxSources.Count;

        SFXSource sfxSource;
        switch (listName)
        {
            case "beamSFX":
                n = beamSFX.Count;
                sfxSource = Instantiate(sfxSourcePrefab, pos, Quaternion.identity);
                sfxSource.SetAudioClip(clip);
                beamSFX.Add(sfxSource);
                break;

            case "sfxSources":
            default:
                n = sfxSources.Count;
                sfxSource = Instantiate(sfxSourcePrefab, pos, Quaternion.identity);
                sfxSource.SetAudioClip(clip);
                sfxSources.Add(sfxSource);
                break;
        }

        return n;
    }

    public bool DestroySFXSource(int index, string listName)
    {
        bool indexInBounds = false;
        switch (listName)
        {
            case "beamSFX":
                if (indexInBounds)
                {
                    Destroy(beamSFX[index], 0.05f);
                    beamSFX.RemoveAt(index);
                }
                break;

            case "sfxSources":
            default:
                if (indexInBounds)
                {
                    Destroy(sfxSources[index], 0.05f);
                    sfxSources.RemoveAt(index);
                }
                break;
        }
        return indexInBounds;
    }

    public int AddSFXSource(SFXSource source)
    {
        int n = sfxSources.Count;

        sfxSources.Add(source);

        return n;
    }

    public bool RemoveSFXSource(int index)
    {
        bool indexInBounds = InBounds(index, sfxSources);
        if (indexInBounds)
        {
            sfxSources.RemoveAt(index);
        }
        return indexInBounds;
    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ PLAYER MOVEMENT ]

    public void PlayerWalk(float time, int stepCount)
    {
        if (playerWalkCycle != null)
        {
            StopCoroutine(playerWalkCycle);
        }
        playerWalkCycle = StartCoroutine(IPlayerWalk(time, stepCount));
    }

    private IEnumerator IPlayerWalk(float time, int stepCount)
    {
        float stepTime = time / (float)stepCount;

        float targetTime = stepTime * 0.9f;
        for (int i = 0; i < stepCount; i++)
        {
            float timePassed = 0.0f;
            while (timePassed <= targetTime)
            {
                yield return null;
                timePassed += Time.deltaTime;
            }

            PlayerStep();

            targetTime = stepTime;
        }
    }

    public void PlayerJump(float time)
    {
        if (playerWalkCycle != null)
        {
            StopCoroutine(playerWalkCycle);
        }
        playerWalkCycle = StartCoroutine(IPlayerJump(time));
    }

    private IEnumerator IPlayerJump(float time)
    {
        playerSFX.PlayAudioClip(PickFromList(jump));

        yield return new WaitForSecondsRealtime(time);

        switch (GameManager.Player.GetFloorType())
        {
            case FloorTypes.Empty:
            case FloorTypes.Stone:
            default:
                playerSFX.PlayAudioClip(PickFromList(land));
                break;
        }

    }

    public void PlayerStep()
    {
        switch (GameManager.Player.GetFloorType())
        {
            case FloorTypes.Empty:
            case FloorTypes.Stone:
            default:
                playerSFX.PlayAudioClip(PickFromList(walkStone));
                break;

            case FloorTypes.Wood:
                playerSFX.PlayAudioClip(PickFromList(walkWood));
                break;

            case FloorTypes.Grass:
            case FloorTypes.Foliage:
                playerSFX.PlayAudioClip(PickFromList(walkFoliage));
                break;

            case FloorTypes.UnderWater:
                playerSFX.PlayAudioClip(PickFromList(walkWater));
                break;
        }
    }

    #endregion

    public void AudioFade(bool fadeOut, float fadeTime)
    {
        StartCoroutine(IAudioFade(fadeOut, fadeTime));
    }

    private IEnumerator IAudioFade(bool fadeOut, float fadeTime)
    {
        float volStart = 0.0f;
        float volTarget = 0.0f;
        if (fadeOut)
        {
            volStart = 1.0f;
        }
        else
        {
            volTarget = 1.0f;
        }

        float timePassed = 0.0f;
        while (timePassed <= fadeTime)
        {
            yield return null;
            timePassed += Time.unscaledDeltaTime;
            float delta = InterpDelta.CosCurve(timePassed / fadeTime);
            AudioListener.volume = Mathf.Lerp(volStart, volTarget, delta);
        }
        AudioListener.volume = volTarget;
    }

    #region [ MUSIC ]

    private IEnumerator MusicVolumeCheck()
    {
        yield return new WaitForSeconds(0.2f);
        SetupMusic();
        if (musicPlayer.volume == 0.0f && defaultMusicVolume > 0.0f)
        {
            MusicFade(false, 0.4f);
        }
    }

    public void MusicFade(bool fadeOut, float fadeTime)
    {
        StartCoroutine(IMusicFade(fadeOut, fadeTime));
    }

    private IEnumerator IMusicFade(bool fadeOut, float fadeTime)
    {
        float volStart = 0.0f;
        float volTarget = 0.0f;
        if (fadeOut)
        {
            volStart = defaultMusicVolume;
        }
        else
        {
            volTarget = defaultMusicVolume;
        }

        float timePassed = 0.0f;
        while (timePassed <= fadeTime)
        {
            yield return null;
            timePassed += Time.unscaledDeltaTime;
            float delta = InterpDelta.CosCurve(timePassed / fadeTime);
            musicPlayer.volume = Mathf.Lerp(volStart, volTarget, delta);
        }
        musicPlayer.volume = volTarget;
    }

    #endregion

    public void ButtonClick(AudioButton.ButtonType type)
    {
        AudioClip clip;
        switch (type)
        {
            default:
            case AudioButton.ButtonType.Standard:
                clip = PickFromList(buttonStandard);
                break;
                
            case AudioButton.ButtonType.Heavy:
                clip = PickFromList(buttonHeavy);
                break;
                
            case AudioButton.ButtonType.Slider:
                clip = PickFromList(slider);
                break;
        }
        uiSFX.PlayAudioClip(clip, 1.0f, buttonVolumeScale);
    }

    public void StartObjectMove(SFXSource source)
    {
        AudioClip clip = PickFromList(moveObject);
        source.PlayAudioLoop(clip);
    }
    
    public void StopObjectMove(SFXSource source)
    {
        source.Stop();
    }

    public void ChangeAudioLoop(SFXSource sfx, AudioClip loop)
    {
        if (!sfx.CompareClip(loop))
        {
            if (sfx.source.isPlaying)
            {
                sfx.Stop();
                sfx.PlayAudioLoop(loop);
            }
            else
            {
                sfx.PlayAudioLoop(loop);
            }
        }
    }

}
