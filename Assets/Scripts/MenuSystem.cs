using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MenuSystem : MonoBehaviour
{
    [Header("Sound")]
    public Slider SoundVFXSlider;
    public AudioMixer MainMixer;
    public Slider SoundBGMSlider;
    public Sprite SoundButtonMuted;
    public Sprite SoundButtonUnmuted;
    public Button MuteSoundButton;
    public string VolumeParameter = "volMusic";
    [Header("CanvasElements")]
    public GameObject PauseMenuCanvas;
    public GameObject MainMenuCanvas;
    public GameObject OptionsMenuCanvas;
    public GameObject GameOverCanvas;
    public GameObject GameWinCanvas;
    public Image FrontFading;
    [Min(0f)]public float TimeFadeIn = 1f;
    [Min(0f)]public float TimeFadeBack = .5f;

    private bool isSoundMuted = false;

    public static bool sExists => sI != null;
    private static MenuSystem sI;
    private void Awake()
    {
        sI = this;
        GameManager.EventPauseEnter += delegate { PauseMenuCanvas.SetActive(true); };
        GameManager.EventPauseLeave += delegate { PauseMenuCanvas.SetActive(false); };
        GameManager.EventRestartGame += delegate
        {
            OptionsMenuCanvas.SetActive(false);
            PauseMenuCanvas.SetActive(false);
            MainMenuCanvas.SetActive(false);
            GameOverCanvas.SetActive(false);
            GameWinCanvas.SetActive(false);
        };
        PlayerManager.OnDeath += delegate { GameOverCanvas.SetActive(true); };
        GameManager.EventGameVictory += delegate { GameWinCanvas.SetActive(true); };

        MainMenuCanvas.SetActive(GameManager.sHasMenu);
        
        PauseMenuCanvas.SetActive(false);
        GameOverCanvas.SetActive(false);
        GameWinCanvas.SetActive(false);
        OptionsMenuCanvas.SetActive(false);

    }

    public void StartGame()
    {
        StartCoroutine(FadingCoroutine(delegate {
            GameManager.StartGame();
            MainMenuCanvas.SetActive(false);
        }));
        
    }

    public void BackToMainMenu()
    {
        StopAllCoroutines();
        StartCoroutine(FadingCoroutine(delegate {
            MainMenuCanvas.SetActive(true);
            PauseMenuCanvas.SetActive(false);
            OptionsMenuCanvas.SetActive(false);
            GameOverCanvas.SetActive(false);
            GameWinCanvas.SetActive(false);
        }));
        
    }

    public void UpdateSoundLevel()
    {
        if (!isSoundMuted)
        {
            MainMixer.SetFloat("VolumeSFX"  , SoundVFXSlider.value);
            MainMixer.SetFloat("VolumeBGM", SoundBGMSlider.value);
        }
    }

    public void RestartGame()
    {
        Debug.Log("Restart game!");
        GameManager.RestartGame();
    }
    public void RestartCheckpoint()
    {
        Debug.Log("Restart checkpoint!");
        GameManager.RestartCheckpoint();
    }
    public void MuteSound()
    {
        isSoundMuted = !isSoundMuted;
        MainMixer.SetFloat("VolumeSFX", isSoundMuted?0:SoundVFXSlider.value);
        MainMixer.SetFloat("VolumeBGM", isSoundMuted?0:SoundBGMSlider.value);
        MuteSoundButton.image.sprite = isSoundMuted ? SoundButtonMuted : SoundButtonUnmuted;
    }

    public void QuitGame()
    {
        StartCoroutine(FadingCoroutine(delegate {
            Application.Quit();
        }));
        
    }

    private IEnumerator FadingCoroutine( Action action)
    {
        Debug.Log("huh");
        float currentTime = 0;
        Color color = Color.black;
        while(currentTime< TimeFadeIn)
        {
        Debug.Log("huh!");
            float progress = currentTime / TimeFadeIn;
            color.a = (progress);
            FrontFading.color = color;
            currentTime += Time.unscaledDeltaTime;
            yield return null;
        }
        color.a = 1;
        FrontFading.color = color;
        action();
        currentTime = 0;
        while (currentTime < TimeFadeBack)
        {
        //Debug.Log("huh?");
            float progress = currentTime / TimeFadeBack;
            color.a = (1 - progress);
            FrontFading.color = color;
            currentTime += Time.unscaledDeltaTime;
            yield return null;
        }
        color.a = 0;
        FrontFading.color = color;
    }


}
