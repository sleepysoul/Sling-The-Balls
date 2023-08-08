using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{

    private int currentStage = 1; // ���� �������� �������� ��ȣ

    public void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) {
            Debug.Log("OnApplicationPause");
            SaveGame();
        }
    }

    public void SaveGame()
    {
        PlayerPrefs.SetInt("CurrentStage", currentStage);
        PlayerPrefs.Save();
        Debug.Log("���� ���� �Ϸ� - ���� ��������: " + currentStage);
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey("CurrentStage")) {
            currentStage = PlayerPrefs.GetInt("CurrentStage");
            Debug.Log("���� �ҷ����� �Ϸ� - ���� ��������: " + currentStage);
        }
        else {
            Debug.Log("����� ���� �����Ͱ� �����ϴ�. ó������ �����մϴ�.");
            currentStage = 1; // ����� �������� ������ ���� ��� �⺻������ 1�� ����
        }
    }

    // ���������� �ѱ� �� ȣ���ϴ� �޼���
    public void NextStage()
    {
        currentStage++;
        SaveGame();
        LoadNextScene();
    }

    public void LoadNextScene()
    {
        // ���� ���������� �� �̸��� �����ϰ� �ε��մϴ�.
        string nextSceneName = "Stage " + currentStage.ToString();
        SceneManager.LoadScene(nextSceneName);
    }
}