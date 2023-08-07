using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{

    private int currentStage = 1; // 현재 진행중인 스테이지 번호

    public void OnApplicationPause(bool pauseStatus)
    {
        Debug.Log("OnApplicationPause");
        if (pauseStatus) {
            SaveGame();
        }
    }

    public void SaveGame()
    {
        PlayerPrefs.SetInt("CurrentStage", currentStage);
        PlayerPrefs.Save();
        Debug.Log("게임 저장 완료 - 현재 스테이지: " + currentStage);
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey("CurrentStage")) {
            currentStage = PlayerPrefs.GetInt("CurrentStage");
            string thisSceneName = "Stage " + currentStage.ToString();
            SceneManager.LoadScene(thisSceneName);
            Debug.Log("게임 불러오기 완료 - 현재 스테이지: " + currentStage);
        }
        else {
            Debug.Log("저장된 게임 데이터가 없습니다. 처음부터 시작합니다.");
            currentStage = 1; // 저장된 스테이지 정보가 없을 경우 기본값으로 1로 설정
        }
    }

    // 스테이지를 넘길 때 호출하는 메서드
    public void NextStage()
    {
        currentStage++;
        SaveGame();
        LoadNextScene();
    }

    public void LoadNextScene()
    {
        // 다음 스테이지의 씬 이름을 정의하고 로드합니다.
        string nextSceneName = "Stage " + currentStage.ToString();
        SceneManager.LoadScene(nextSceneName);
    }
}