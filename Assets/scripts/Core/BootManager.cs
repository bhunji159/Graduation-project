using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class BootManager : MonoBehaviour
{
    public Slider loadingBar;

    void Start() => StartCoroutine(LoadNextScene());

    IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(1f); // 로딩 시뮬레이션
        AsyncOperation op = SceneManager.LoadSceneAsync("CodeInputScene");
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            if (loadingBar)
                loadingBar.value = Mathf.Clamp01(op.progress / 0.9f);
            if (op.progress >= 0.9f)
            {
                yield return new WaitForSeconds(0.5f);
                op.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}
