using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Canvas titleMenu;
    public Canvas gameplay;
    public Canvas winDisplay;
    public Canvas loseDisplay;
    public Canvas pause;
    public Canvas options;
    public Canvas credits;

    [SerializeField]
    private GUI gameUI;
    [SerializeField]
    private OUI optionUI;

    public GUI GameUI { get { return gameUI; } }
    public OUI OptionUI { get { return optionUI; } }

    private void Awake()
    {
        LoadTitleMenu();
    }

    private void Update()
    {
        //Debug.Log(credits.enabled);
    }

    public void DisableAll()
    {
        titleMenu.enabled = false;
        options.enabled = false;
        credits.enabled = false;
        gameplay.enabled = false;
        winDisplay.enabled = false;
        loseDisplay.enabled = false;
        pause.enabled = false;
    }

    public void LoadTitleMenu()
    {
        DisableAll();
        titleMenu.enabled = true;
    }

    public void LoadGameplay()
    {
        DisableAll();
        gameplay.enabled = true;
    }

    public void LoadWinScreen()
    {
        winDisplay.enabled = true;
    }

    public void LoadLoseScreen()
    {
        loseDisplay.enabled = true;
    }

    public void LoadPauseScreen()
    {
        credits.enabled = false;
        options.enabled = false;
        pause.enabled = true;
    }

    public void LoadOptions()
    {
        options.enabled = true;
    }

    public void LoadCredits()
    {
        DisableAll();
        credits.enabled = true;
    }
}

[System.Serializable]
public class GUI
{
    [Tooltip("any button that calls load")]
    public Button[] allLoadButtons;
    public Canvas notePlain;
}

[System.Serializable]
public class OUI // Option User Interface
{
    public Slider brightnessSlider;
    public Image brightnessImage;
}
