using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour
{
    public AudioSource bgmPlayer;
    public AudioSource sfxPlayer;

    private void Awake()
    {
        bgmPlayer.Play();
    }

    public void GameStart()
    {        
        sfxPlayer.Play();
        StartCoroutine("GameStartRoutine");
    }

    IEnumerator GameStartRoutine()
    {
        yield return new WaitForSeconds(0.3f);

        if (!PlayerPrefs.HasKey("Stage"))
        {
            // Stage 에 저장된 값이 없을 때 (처음 플레이)
            Debug.Log("저장된 Stage 값 없음. Stage 1 로드 완료");
            SceneManager.LoadScene("Stage 1");
        }

        else
        {
            // Stage 에 저장된 값을 불러온다. 
            Debug.Log("저장된 Stage : " + PlayerPrefs.GetString("Stage") + " 로드 완료");
            SceneManager.LoadScene(PlayerPrefs.GetString("Stage"));
        }
    }
}
