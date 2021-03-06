using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public float MasterVoulme => masterVoulme;
    public float BGMVolume => bgmVolume * MasterVoulme;
    public float FxVoulme => fxVoulme * MasterVoulme;

    public float masterVoulme = 1.0f;
    public float bgmVolume = 1.0f;
    public float fxVoulme = 1.0f;
    public float pitch = 1.0f;

    private AudioSource bgmAudioSource;
    private List<AudioSource> fxAudioSourceList = new List<AudioSource>();

    private Dictionary<string, AudioClip> bgmSoundDic = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> fxSoundDic = new Dictionary<string, AudioClip>();

    public static bool SoundOff = false;


    private static SoundManager instance;
    public static SoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SoundManager>();
                if (instance == null)
                {
                    instance = new GameObject(typeof(SoundManager).ToString()).AddComponent<SoundManager>();
                }
            }

            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this as SoundManager;
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this);

        foreach (var audioClip in Resources.LoadAll<AudioClip>("Sound/BGM"))
        {
            bgmSoundDic.Add(audioClip.name, audioClip);
        }
    }


    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = default;
        }
    }

    private AudioClip GetBGMSound(string name)
    {
        AudioClip result;
        if (!bgmSoundDic.TryGetValue(name, out result))
        {
            Debug.LogWarning(name + "Not Found");
        }
        return result;
    }

    private AudioClip GetFxSound(string name)
    {
        AudioClip result;
        if (!fxSoundDic.TryGetValue(name, out result))
        {
            result = Resources.Load<AudioClip>("Sound/FX/" + name);
            if (result == null)
            {
                Debug.LogWarning(name + "Not Found");
                return null;
            }
            fxSoundDic.Add(name, result);
        }
        return result;
    }

    private AudioSource MakeAudioSourceObject(string name)
    {
        GameObject audioObject = new GameObject();
        audioObject.name = name;
        audioObject.transform.SetParent(gameObject.transform);

        return audioObject.AddComponent<AudioSource>();
    }

    private void SetAudioSource(AudioSource audioSource, AudioClip audioClip, bool isLoop, float volume, bool isMute = false)
    {
        audioSource.clip = audioClip;
        audioSource.loop = isLoop;
        audioSource.volume = volume;
        audioSource.mute = isMute;
    }

    public void AdjustMasterVolume(float newVolume)
    {
        masterVoulme = newVolume;
        AdjustBGMVolume(bgmVolume);
        AdjustFxVoulme(fxVoulme);
    }

    public void AdjustBGMVolume(float newVolume)
    {
        bgmVolume = newVolume;
        if (bgmAudioSource != null)
        {
            bgmAudioSource.volume = BGMVolume;
        }
    }

    public void AdjustFxVoulme(float newVolume)
    {
        fxVoulme = newVolume;
        foreach (var fxAudioSource in fxAudioSourceList)
        {
            if (fxAudioSource != null)
            {
                fxAudioSource.volume = FxVoulme;
            }
        }
    }

    public void AdjustSoundPitch(float pitch)
    {
        this.pitch = pitch;
        foreach (var item in fxAudioSourceList)
        {
            if (item)
            {
                item.DOPitch(pitch, 0.5f).SetUpdate(true);
            }
        }

        if (bgmAudioSource)
        {
            bgmAudioSource.DOPitch(pitch, 0.5f).SetUpdate(true);
        }
    }

    public void PlayBGMSound(string name, float duration = 2)
    {
        if (SoundOff) return;

        if (bgmAudioSource == null)
        {
            bgmAudioSource = MakeAudioSourceObject("BGMObject");
        }

        if (bgmAudioSource.isPlaying)
        {
            StartCoroutine(ChangeBGM(BGMVolume, name, duration));
        }
        else
        {
            SetAudioSource(bgmAudioSource, GetBGMSound(name), true, BGMVolume, false);
            bgmAudioSource.Play();
        }

    }


    public IEnumerator ChangeBGM(float volume, string name, float duration)
    {
        DOTween.To(() => bgmAudioSource.volume, x => bgmAudioSource.volume = x, 0, duration)
            .OnComplete(() =>
            {
                bgmAudioSource.volume = volume * MasterVoulme;
                SetAudioSource(bgmAudioSource, GetBGMSound(name), true, BGMVolume, false);
                bgmAudioSource.Play();
            });
        yield return null;
    }

    public void PlayFXSound(string name)
    {
        if (SoundOff) return;

        foreach (var fxAudioSource in fxAudioSourceList)
        {
            if (!fxAudioSource.isPlaying)
            {
                SetAudioSource(fxAudioSource, GetFxSound(name), false, FxVoulme, false);
                fxAudioSource.Play();
                return;
            }
        }

        fxAudioSourceList.Add(MakeAudioSourceObject("FxObject"));
        PlayFXSound(name);
    }

    public void StopBGMSound(float duration = 2)
    {
        DOTween.To(() => bgmAudioSource.volume, x => bgmAudioSource.volume = x, 0, duration)
            .OnComplete(() =>
            {
                bgmAudioSource.Stop();
            });
    }

    public void OffSound()
    {
        if (bgmAudioSource == null) return;

        bgmAudioSource.volume = 0;
        for (int i = 0; i < fxAudioSourceList.Count; i++)
        {
            fxAudioSourceList[i].volume = 0;
        }


    }






}