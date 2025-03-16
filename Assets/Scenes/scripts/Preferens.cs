using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class Preferens : MonoBehaviour
{
    [SerializeField]
    private GameObject AudioPack;
    [SerializeField]
    private GameObject VideoPack;
    [SerializeField]
    private GameObject RulesPack;
    [SerializeField]
    private Toggle WindToggle;
    [SerializeField]
    private Toggle KillToggle;
    [SerializeField]
    private Toggle SpeedToggle;

    private GameManager Manager;
    private sbyte CurMode = 1;
    private bool Restart = false;

    public void Awake()
    {
        Manager = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<GameManager>();
        KillToggle.isOn = Manager.GKill;
        SpeedToggle.isOn = Manager.GSpeed;
        WindToggle.isOn = Manager.GWindow;
    }
    public void Starting(sbyte mode)
    {
        WindToggle.isOn = Screen.fullScreen;
        switch (mode)
        {
            case 1: Audio(); break; 
            case 2: Video(); break; 
            case 3: Rules(); break;
        }
    }
    public void Audio()
    {
        if (CurMode != 1) 
        {
            VideoPack.SetActive(false);
            RulesPack.SetActive(false);
            AudioPack.SetActive(true);
            CurMode = 1;
        }
    }
    public void Video()
    {
        if (CurMode != 2)
        {
            VideoPack.SetActive(true);
            RulesPack.SetActive(false);
            AudioPack.SetActive(false);
            CurMode = 2;
        }
    }
    public void Rules()
    {
        if (CurMode != 3)
        {
            VideoPack.SetActive(false);
            RulesPack.SetActive(true);
            AudioPack.SetActive(false);
            CurMode = 3;
        }
    }
    public void KillT()
    {
        Manager.GKill = KillToggle.isOn;
        Manager.FilerRefriter();
        Restart = true;
    }
    public void SpeedT()
    {
        Manager.GSpeed = SpeedToggle.isOn;
        Manager.FilerRefriter();
        Restart = true;
    }
    public void WindowT()
    {
        Screen.fullScreen = WindToggle.isOn;
        Manager.GWindow = WindToggle.isOn;
        Manager.FilerRefriter();
    }
    public void QuitButton()
    {
        GetComponent<Animator>().SetBool("!", true);
    }
    public void Quit()
    {
        if (Restart)
        {
            Manager.LevelRemover();
            Manager.Level0();
        }
        Manager.CanvasAnim();
        SceneManager.UnloadSceneAsync(1);
    }

}
