using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FristScene : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("LoadingScene"); // 로딩 씬으로 이동
    }
}
