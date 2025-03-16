using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MenuAndPreferenses : MonoBehaviour
{
    private RaycastHit hit;                 // Столкновение луча с объектом
    private Ray ray;
    private GameManager manager;
    public bool RunMenu = true;

    [SerializeField]
    private Animator BoardAnimator;
    [SerializeField]
    private Animator CameraAnimator;

    private void Awake()
    {
        manager = CameraAnimator.gameObject.GetComponent<GameManager>();
    }
    private void Update()
    {
        if (RunMenu)
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition); // Запуск луча
            if (Physics.Raycast(ray, out hit)) //Если луч попал во что - то
            {
                if (Input.GetKeyDown(KeyCode.Mouse0) && !CameraAnimator.enabled)
                {
                    string Button = hit.collider.name;          
                    switch (Button)
                    {
                    case "Settings":
                        StartCoroutine(Flipper());
                        break;
                    case "Back":
                        StartCoroutine(Flipper());
                        break;
                    case "Start":
                        StartCoroutine(Starter());
                        break;
                    case "Exit":
                        Application.Quit();
                        break;
                    default:
                        break;
                    }
                }
            }
        }
    }
    public IEnumerator Starter()
    {
        manager.audioButton.Play();
        yield return new WaitForSeconds(0.2f);
        CameraAnimator.enabled = true;
        CameraAnimator.SetBool("Fly", true);
        RunMenu = false;
    }
    public IEnumerator Flipper()
    {
        manager.audioButton.Play();
        yield return new WaitForSeconds(0.2f);
        if (BoardAnimator.GetCurrentAnimatorStateInfo(0).IsName("Stop")) BoardAnimator.SetTrigger("FTriger");
        // Перезапускаем игру
    }
    public void PrefStart()
    {
        if (SceneManager.loadedSceneCount < 2 && !CameraAnimator.enabled)
        {
            SceneManager.LoadScene(1, LoadSceneMode.Additive);
            manager.CanvasAnim();
        }
    }
    public void Restart() // Кнопка Заново
    {
        if (SceneManager.loadedSceneCount < 2 && !CameraAnimator.enabled)
            StartCoroutine(manager.Reborn());
    }
    public void Exit() // Кнопка вывод
    {
        if(SceneManager.loadedSceneCount < 2 && !CameraAnimator.enabled)
            StartCoroutine(manager.Quit());
    }
}
