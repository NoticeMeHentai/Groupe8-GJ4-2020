//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;

//[System.Serializable]
//public class MenuWindow
//{
//    public string m_Name;
//    public GameObject m_MenuObject;
//    public GameObject m_FirstSelection;
//}
//public class MenuManager : MonoBehaviour
//{
//    public GameObject mPauseMenu;
//    public GameObject mEndFailMenu;
//    public GameObject mEndWinMenu;
//    public GameObject mDownloadBar;

//    public GameObject mMainMenuFirstSelected;
//    public GameObject mOptionMenuFirstSelected;
//    public GameObject mPauseMenuFirstSelected;
//    public GameObject mEndFailMenuFirstSelected;
//    public GameObject mEndWinMenuFirstSelected;

//    public static bool sIsPaused = false;
//    public static bool sInMainMenu = true;
//    public bool mEndFail = false;
//    public bool mEndWin = false;
//    [Range(0.0f,1.0f)] public float mDownloadRatio = 0.0f;
//    public Text mScoreLose;
//    public Text mScoreWin;

//    private void Awake()
//    {
//        GameManager.OnGameOverNoLivesLeft += EndFail;
//        GameManager.OnGameOverTimeRanOut += EndWin;
        
//    }
//    private void Start()
//    {
//        EventSystem.current.SetSelectedGameObject(null);
//        EventSystem.current.SetSelectedGameObject(mMainMenuFirstSelected);
//        mDownloadText = mDownloadBar.transform.GetChild(0).GetComponent<Text>();
//        HUDManager.Enable(false);
        
//    }
//    private void Update()
//    {
//        if(mWifiLost && !mWifiLostLogoSetActive)
//        {
//            mWifiLostLogoSetActive = true;
//            mWifiLostLogo.SetActive(true);
//        }
//        if (!mWifiLost && mWifiLostLogoSetActive)
//        {
//            mWifiLostLogoSetActive = false;
//            mWifiLostLogo.SetActive(false);
//        }
//        mDownloadBar.GetComponent<Image>().material.SetFloat("_DownloadRatio", mDownloadRatio);
//        //if(mDownloading)
//        //{
//        //    mDownloadText.text = "Downloading" + new string('.', Mathf.FloorToInt((Time.time* mDownloadSpeed) % 3) + 1);
//        //}
//        if (Input.GetAxis("HorizontalMenu") > 0)
//        {
//            Debug.Log("a");

//        }    

//        if (!sInMainMenu)
//        {
//            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Joystick1Button7))
//            {
//                if (sIsPaused)
//                {
//                    sIsPaused = false;
//                    mPauseMenu.SetActive(false);
//                    Time.timeScale = 0.0f;
//                }
//                else
//                {
//                    sIsPaused = true;
//                    mPauseMenu.SetActive(true);
//                    Time.timeScale = 1.0f;
//                    EventSystem.current.SetSelectedGameObject(null);
//                    EventSystem.current.SetSelectedGameObject(mPauseMenuFirstSelected);

//                }
//            }
//        }
//        if (mEndFail)
//        {
//            EndFail();
//        }
//        if (mEndWin)
//        {
//            EndWin();
//        }
        
//    }
//    public void GoToOptions()
//    {
//        EventSystem.current.SetSelectedGameObject(null);
//        EventSystem.current.SetSelectedGameObject(mOptionMenuFirstSelected);
//    }

//    public void PlayGame()
//    {
//        if(!SceneManager.GetSceneByBuildIndex(1).isLoaded)
//        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1, LoadSceneMode.Additive);
//        sInMainMenu = false;
//        Time.timeScale = 1.0f;
//        mDownloadText = mDownloadBar.transform.GetChild(0).GetComponent<Text>();
//    }

//    public void Resume()
//    {
//        sIsPaused = false;
//        Time.timeScale = 1.0f;
//    }
//    public void BackToMainMenu()
//    {
//        sIsPaused = false;
//        sInMainMenu = true;
//        SceneManager.UnloadSceneAsync(1);
//        EventSystem.current.SetSelectedGameObject(null);
//        EventSystem.current.SetSelectedGameObject(mMainMenuFirstSelected);
//    }

//    public void EndFail()
//    {
//        mScoreLose.text = ScoreText();
//        mEndFail = false;
//        mEndFailMenu.SetActive(true);
//        Time.timeScale = 0.0f;
//        EventSystem.current.SetSelectedGameObject(null);
//        EventSystem.current.SetSelectedGameObject(mEndFailMenuFirstSelected);
//    }

//    public void EndWin()
//    {

//        mScoreWin.text = ScoreText();
//        mEndWin = false;
//        mEndWinMenu.SetActive(true);
//        Time.timeScale = 0.0f;
//        EventSystem.current.SetSelectedGameObject(null);
//        EventSystem.current.SetSelectedGameObject(mEndWinMenuFirstSelected);
//    }

//    public void Quit ()
//    {
//        Debug.Log("QUIT");
//        Application.Quit();
//    }

//    private string ScoreText()
//    {
//        string toReturn = "";
//        toReturn += "You managed to download: ";
//        for(int i = 0; i < GameManager.sDifferentFiles.Length;i++)
//        {
//            File currentFile = GameManager.sDifferentFiles[i];
//            toReturn += "\n " + currentFile.mAmountDownload + " " + currentFile.mName + " of " + currentFile.mFileSize + " units of space each one.";
//        }
//        toReturn += "\n A total of " + GameManager.sScore+" points.";
//        return toReturn;
//    }

//}
