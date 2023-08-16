using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("===========[ Core ]")]
    public bool isOver;
    public bool isClear;
    public bool isPaused;
    public int minLevel;
    public int maxLevel;
    public int score;
    public int life;
    public int dollar;
    public int mergeCount;
    public int gameOverCount;
    public float playTime; // 스테이지 타임아웃 체크
    public float totalPlayTime; // 총 플레이 타임

    [Header("===========[ Obejct Pooling ]")]
    public Rigidbody2D hookRb;
    public GameObject donglePrefab;
    public Transform dongleGroup;
    public List<Dongle> donglePool;
    public GameObject effectPrefab;
    public Transform effectGroup;
    public List<ParticleSystem> effectPool;
    [Range(1, 30)]
    public int poolSize;
    public int poolCursor;
    public Dongle lastDongle;

    [Header("===========[ Spawn ]")]
    public Transform spawnPoint;
    [Range(1, 30)]
    public int spawnNumber;
    public Dongle spawnDongle;
    [SerializeField]
    private float spawnXBoundary = 1.2f;

    [Header("===========[ Audio System ]")]
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip;
    public int sfxCursor;
    public enum Sfx { LevelUp, Next, GameOver, Attach, DongleAttach, Button, StageClear }

    [Header("===========[ UI ]")]
    [Header("-----[ GamePlay UI ]")]
    public GameObject touchPad;
    public Text stageText;
    public Text lifeText;
    public Text playTimeText;
    public Text scoreText;
    public Image caution;
    
    [Header("-----[ GameOption UI ]")]
    public GameObject optionUI;
    public GameObject resetButtonSelectionUI;
    public GameObject exitButtonSelectionUI;
    public GameObject stageResetButtonSelectionUI;
    public GameObject statsResetButtonSelectionUI;

    [Header("-----[ GameOption Stats UI ]")]
    public Text optionStageText;
    public Text optionTotalScoreText;
    public Text optionDollarText;
    public Text optionTotalPlayTimeText;
    public Text optionTotalMergeCountText;
    public Text optionTotalGameOverCountText;

    [Header("-----[ GameOver UI ]")]
    public GameObject endGroup;
    public Text gameOverSubScoreText; // 현재 스테이지에서 얻은 점수
    public Text overTotalScoreText;
    public Text overDollarText;

    [Header("-----[ GameClear UI ]")]
    public GameObject stageClearGroup;
    public Text stageClearSubScoreText; // 현재 스테이지에서 얻은 점수
    public Text remainingLifeText;
    public Text remainingPlayTimeText;
    public Text stageClearScoreText;
    public Text clearTotalScoreText;
    public Text clearDollarText;

    [Header("===========[ ETC ]")]
    public GameObject line;
    public GameObject deadEffectPrefab;
    public Transform daedEffectGroup;
    public GameObject point;
    public GameObject[] points;


    void Awake()
    {
        // Scene 이 처음 호출될 때,  PlayerPrefs.Stage 에 현재 Scene name 저장
        Debug.Log("새로운 Scene 호출 감지. 호출된 Scene 의 이름은 [ " + SceneManager.GetActiveScene().name + " ] 입니다. 저장 완료.");
        PlayerPrefs.SetString("Stage", SceneManager.GetActiveScene().name);
        stageText.text = SceneManager.GetActiveScene().name;

        Application.targetFrameRate = 60;
        bgmPlayer.Play();

        donglePool = new List<Dongle>();
        effectPool = new List<ParticleSystem>();

        for (int index = 0;index < poolSize;index++) {
            MakeDongle();
        }

        StartCoroutine(SpawnDongle());
        StartCoroutine(Caution());
    }

    private void Start()
    {
        NextDongle();
    }

    IEnumerator Caution()
    {
        caution.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        caution.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        caution.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        caution.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        caution.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        caution.gameObject.SetActive(false);
    }

    void NextDongle()
    {
        if (isOver) {
            return;
        }

        Dongle newDongle = GetDongle();
        lastDongle = newDongle;
        lastDongle.isMerge = true;  
        lastDongle.level = Random.Range(minLevel, maxLevel);
        lastDongle.gameObject.SetActive(true);
        StartCoroutine(TouchPadActive());

        SfxPlay(Sfx.Next);
        StartCoroutine("WaitNext");
    }

    IEnumerator TouchPadActive()
    {
        yield return new WaitForSeconds(0.2f);
        touchPad.gameObject.SetActive(true);
    }

    IEnumerator WaitNext()
    {
        while (lastDongle != null) {
            yield return null;
        }

        yield return new WaitForSeconds(.5f);

        NextDongle();
    }

    Dongle GetDongle()
    {
        for (int index = 0;index < donglePool.Count;index++) {
            poolCursor = (poolCursor + 1) % donglePool.Count;
            if (!donglePool[poolCursor].gameObject.activeSelf) {
                donglePool[poolCursor].transform.position = dongleGroup.position;
                donglePool[poolCursor].GetComponent<SpringJoint2D>().enabled = true;
                return donglePool[poolCursor];
            }
        }
        return MakeDongle();
    }

    public Dongle MakeDongle()
    {
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect " + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        GameObject instantDongleObj = Instantiate(donglePrefab, dongleGroup);
        instantDongleObj.name = "Dongle " + donglePool.Count;
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
        instantDongle.manager = this;
        instantDongle.hook = hookRb;
        instantDongle.GetComponent<SpringJoint2D>().connectedBody = hookRb;
        instantDongle.effect = instantEffect;
        donglePool.Add(instantDongle);

        return instantDongle;
    }

    IEnumerator SpawnDongle()
    {        
        for (int index = 0; index < spawnNumber; index++) {
            for (int j = 0; j < donglePool.Count; j++) {
                poolCursor = (poolCursor + 1) % donglePool.Count;
                if (!donglePool[poolCursor].gameObject.activeSelf) {
                    spawnDongle = donglePool[poolCursor];

                }
            }
            spawnDongle.isMerge = true;
            spawnPoint.transform.position = new Vector3(spawnPoint.position.x + Random.Range(-spawnXBoundary, spawnXBoundary), spawnPoint.position.y);
            spawnDongle.transform.position = spawnPoint.transform.position;
            spawnDongle.level = Random.Range(minLevel, maxLevel);            
            spawnDongle.GetComponent<SpringJoint2D>().enabled = false;
            spawnDongle.gameObject.SetActive(true);

            yield return new WaitForSeconds(0.3f);
            spawnDongle.isMerge = false;
        }      
    }

    public void TouchDown()
    {
        if (lastDongle == null) {
            return;
        }

        lastDongle.Drag();
    }

    public void TouchUp()
    {
        if (lastDongle == null) {
            return;
        }

        lastDongle.Drop();
        touchPad.gameObject.SetActive(false);
    }

    public void StageClear()
    {
        StartCoroutine(StageClearRoutine());
    }

    IEnumerator StageClearRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        SfxPlay(Sfx.StageClear);

        /* 현재 게임 최종 스코어 출력 */
        // 남은 시간대 점수 환산
        int playTimeToScore;
        if (playTime < 60f) {
            playTimeToScore = 1;
        }
        else {
            playTimeToScore = (int)(playTime / 60f);
        }

        // 현재 스테이지 점수 , 남은 라이프 , 남은 시간 , 계산된 점수 출력
        Debug.Log("현재 스테이지 클리어 스코어 : " + score);
        stageClearSubScoreText.text = "점수 : " + score.ToString();
        Debug.Log("남은 라이프 : " + life);
        remainingLifeText.text = "남은 라이프 보너스 : X " + life.ToString();
        Debug.Log("남은 시간 : " + playTimeToScore);
        remainingPlayTimeText.text = "남은 시간 보너스 : X " + playTimeToScore.ToString();
        
        int stageClearScore = score * life * playTimeToScore;
        Debug.Log("합산 : " + stageClearScore);
        stageClearScoreText.text = "합산 점수 : " + stageClearScore.ToString();

        /* 토탈 스코어에 저장 */
        // 최초 득점 저장하기
        if (!PlayerPrefs.HasKey("TotalScore"))
        {
            PlayerPrefs.SetInt("TotalScore", stageClearScore);
            Debug.Log("최초 득점! Total Score 저장 완료 : " + stageClearScore);
        }
        else
        {
            // 저장 점수 + 클리어 시 점수 => 업데이트            
            int savedTotalScore = PlayerPrefs.GetInt("TotalScore");
            int upadteTotalScore = savedTotalScore + stageClearScore;
            Debug.Log("저장된 Total Score : " + savedTotalScore);
            Debug.Log("클리어 Score : " + stageClearScore);
            Debug.Log("Total Score 저장 완료 : " + upadteTotalScore);
            
            PlayerPrefs.SetInt("TotalScore", upadteTotalScore);            
        }

        Debug.Log("총 점수 : " + PlayerPrefs.GetInt("TotalScore"));
        clearTotalScoreText.text = "+" + stageClearScore.ToString() + " (토탈 점수 : " + PlayerPrefs.GetInt("TotalScore").ToString() + ")";

        // 스테이지 합산 점수 / 10 = 달러 , 저장 및 출력
        dollar = stageClearScore / 10;

        // 첫 달러 획득 처리
        if (!PlayerPrefs.HasKey("Dollar"))
        {
            PlayerPrefs.SetInt("Dollar", dollar);
        }
        // 저장된 Dollar 가 있다면 얻은 달러를 더해서 저장하기
        else
        {
            int savedDollar = PlayerPrefs.GetInt("Dollar");
            int updateDollar = savedDollar + dollar;
            PlayerPrefs.SetInt("Dollar", updateDollar);
        }
        // 스테이지에서 얻은 달러 표기
        clearDollarText.text = "+$" + dollar.ToString() + " (보유 달러 : $" + PlayerPrefs.GetInt("Dollar").ToString() + ")";

        // 플레이 타임 저장
        PlayTimeSave();

        // 머지 카운트 저장
        MergeCountSave();

        // 게임오버 카운트 저장
        GameOverCountSave();

        // 게임 클리어 UI 출력
        stageClearGroup.gameObject.SetActive(true);

        // BGM 종료
        bgmPlayer.Stop();

    }

    // 스테이지 클리어 UI → 다음 스테이지 진행 버튼
    public void NextStageButton()
    {
        SfxPlay(Sfx.Button);

        StartCoroutine(NextStageButtonRoutine());
    }

    IEnumerator NextStageButtonRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        int sceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (sceneIndex > SceneManager.sceneCount) {
            SceneManager.LoadScene(1);
        }

        SceneManager.LoadScene(sceneIndex + 1);

    }


    public void GameOver()
    {
        if (isOver) {
            return;
        }

        life = 0;
        isOver = true;
        gameOverCount += 1;
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        Dongle[] dongles = GameObject.FindObjectsOfType<Dongle>();

        for (int index = 0;index < dongles.Length;index++) {
            dongles[index].rb.simulated = false;
        }

        for (int index = 0;index < dongles.Length;index++) {
            dongles[index].Hide(Vector3.up * 100);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.5f);
        SfxPlay(Sfx.GameOver);
        bgmPlayer.Stop();

        // 게임오버 시 스테이지 점수 표기
        gameOverSubScoreText.text = "점수 : " + score.ToString();

        /* 토탈 스코어에 저장 */
        // 최초 득점 저장하기
        if (!PlayerPrefs.HasKey("TotalScore"))
        {
            PlayerPrefs.SetInt("TotalScore", score);
            Debug.Log("최초 득점! Total Score 저장 완료 : " + score);
        }
        else
        {
            // 저장 점수 + 게임오버 시 점수 => 업데이트            
            int savedTotalScore = PlayerPrefs.GetInt("TotalScore");
            int upadteTotalScore = savedTotalScore + score;
            Debug.Log("저장된 Total Score : " + savedTotalScore);
            Debug.Log("게임오버 Score : " + score);
            Debug.Log("Total Score 저장 완료 : " + upadteTotalScore);

            PlayerPrefs.SetInt("TotalScore", upadteTotalScore);
        }

        endGroup.gameObject.SetActive(true);

        Debug.Log("총 점수 : " + PlayerPrefs.GetInt("TotalScore"));
        overTotalScoreText.text = "총 점수 : +" + score.ToString() + "(" + PlayerPrefs.GetInt("TotalScore").ToString() + ")";

        // 스테이지 합산 점수 / 10 = 달러 , 저장 및 출력
        dollar = score / 10;

        // 첫 달러 획득 처리
        if (!PlayerPrefs.HasKey("Dollar"))
        {
            PlayerPrefs.SetInt("Dollar", dollar);
        }
        // 저장된 Dollar 가 있다면 얻은 달러를 더해서 저장하기
        else
        {
            int savedDollar = PlayerPrefs.GetInt("Dollar");
            int updateDollar = savedDollar + dollar;
            PlayerPrefs.SetInt("Dollar", updateDollar);
        }
        // 스테이지에서 얻은 달러 표기
        overDollarText.text = "+$" + dollar.ToString() + "($" + PlayerPrefs.GetInt("Dollar").ToString() + ")";

        // 플레이 타임 저장
        PlayTimeSave();

        // 머지 카운트 저장
        MergeCountSave();

        // 게임오버 카운트 저장
        GameOverCountSave();
    }

    // Option Button Pressed
    public void OptionButtonPressed()
    {
        SfxPlay(Sfx.Attach);
        isPaused = true;

        Invoke("OptionUI", 0.3f);
    }

    // Option UI
    private void OptionUI()
    {
        optionUI.gameObject.SetActive(true);

        /* Option UI > STATS TEXT 설정 */
        // 스테이지 표기
        optionStageText.text = "최종 스테이지 : " + stageText.text;

        // 총 점수 표기
        if (!PlayerPrefs.HasKey("TotalScore"))
        {
            PlayerPrefs.SetInt("TotalScore", 0);
            optionTotalScoreText.text = "총 점수 : 0 (+" + score.ToString() + ")";
        }
        else
        {
            optionTotalScoreText.text = "총 점수 : " + PlayerPrefs.GetInt("TotalScore").ToString() + "(+" + score.ToString() + ")";
        }        

        // 달러 표기
        if (!PlayerPrefs.HasKey("Dollar"))
        {
            PlayerPrefs.SetInt("Dollar", 0);
            optionDollarText.text = "보유 금액 : $ 0";
        }
        else
        {
            optionDollarText.text = "보유 금액 : $" + PlayerPrefs.GetInt("Dollar").ToString();
        }

        // 총 플레이타임 표기 => H:MM:SS
        if (!PlayerPrefs.HasKey("PlayTime"))
        {
            PlayerPrefs.SetFloat("PlayTime", totalPlayTime);
            optionTotalPlayTimeText.text = "총 플레이 타임 : 00:00:00";
        }
        else
        {
            float totalSeconds = PlayerPrefs.GetFloat("PlayTime");
            int hours = Mathf.FloorToInt(totalSeconds / 3600);
            int minutes = Mathf.FloorToInt((totalSeconds % 3600) / 60);
            int seconds = Mathf.FloorToInt(totalSeconds % 60);

            string formattedTime = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
            optionTotalPlayTimeText.text = "총 플레이 타임 : " + formattedTime;
        }

        // 총 머지 횟수 표기
        if (!PlayerPrefs.HasKey("MergeCount"))
        {
            PlayerPrefs.SetInt("MergeCount", 0);
            optionTotalMergeCountText.text = "총 머지 횟수 : 0";
        }
        else
        {
            optionTotalMergeCountText.text = "총 머지 횟수 : " + PlayerPrefs.GetInt("MergeCount").ToString();
        }

        // 총 게임오버 횟수
        if (!PlayerPrefs.HasKey("GameOverCount"))
        {
            PlayerPrefs.SetInt("GameOverCount", 0);
            optionTotalGameOverCountText.text = "총 게임오버 횟수 : 0";
        }
        else
        {
            optionTotalGameOverCountText.text = "총 게임오버 횟수 : " + PlayerPrefs.GetInt("GameOverCount").ToString();
        }
    }


    // Option UI > PLAY Button
    public void OptionPlayButtonPressed()
    {
        SfxPlay(Sfx.Attach);
        isPaused = false;
        optionUI.gameObject.SetActive(false);
    }

    // Option UI > STAGE RESET Button
    public void StageResetButtonPressed()
    {
        SfxPlay(Sfx.Attach);

        Invoke("StageResetButtonSelectionUI", 0.2f);

    }

    private void StageResetButtonSelectionUI()
    {
        stageResetButtonSelectionUI.gameObject.SetActive(true);
    }

    public void StageResetButtonSelectionUIYesButton()
    {
        SfxPlay(Sfx.Attach);
        SceneManager.LoadScene(1);
    }

    public void StageResetButtonSelectionUINoButton()
    {
        SfxPlay(Sfx.Attach);
        stageResetButtonSelectionUI.gameObject.SetActive(false);
    }

    // Option UI > STATS RESET Button
    public void StatsResetButtonPressed()
    {
        SfxPlay(Sfx.Attach);

        Invoke("StatsResetButtonSelectionUI", 0.2f);

    }

    private void StatsResetButtonSelectionUI()
    {
        statsResetButtonSelectionUI.gameObject.SetActive(true);
    }

    public void StatsResetButtonSelectionUIYesButton()
    {
        SfxPlay(Sfx.Attach);

        /* 모든 정보 초기화 */
        // 총 점수 
        PlayerPrefs.SetInt("TotalScore", 0);
        // 총 플레이 타임
        PlayerPrefs.SetFloat("PlayTime", 0f);
        // 총 머지 횟수
        PlayerPrefs.SetInt("MergeCount", 0);
        // 총 게임오버 횟수
        PlayerPrefs.SetInt("GameOverCount", 0);
        // 초기화 완료 후 선택 UI 비활성화
        statsResetButtonSelectionUI.gameObject.SetActive(false);
        OptionPlayButtonPressed();
        OptionButtonPressed();
    }

    public void StatsResetButtonSelectionUINoButton()
    {
        SfxPlay(Sfx.Attach);
        statsResetButtonSelectionUI.gameObject.SetActive(false);
    }

    // Option UI > RESET Button
    public void OptionResetButtonPressed()
    {
        SfxPlay(Sfx.Attach);

        Invoke("ResetButtonSelectionUI", 0.3f);
        
    }
    
    private void ResetButtonSelectionUI()
    {
        resetButtonSelectionUI.gameObject.SetActive(true);
    }

    public void ResetButtonSelectionUIYesButton()
    {
        SfxPlay(Sfx.Attach);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ResetButtonSelectionUINoButton()
    {
        SfxPlay(Sfx.Attach);
        resetButtonSelectionUI.gameObject.SetActive(false);
    }


    // Option UI > EXIT Button
    public void OptionExitButtonPressed()
    {
        SfxPlay(Sfx.Attach);

        Invoke("ExitButtonSelectionUI", 0.3f);

    }

    private void ExitButtonSelectionUI()
    {
        exitButtonSelectionUI.gameObject.SetActive(true);
    }

    public void ExitButtonSelectionUIYesButton()
    {
        SfxPlay(Sfx.Attach);

        // 플레이 정보 저장 함수 호출

        // 게임 종료 → 메인으로 이동
        SceneManager.LoadScene(0);
    }

    public void ExitButtonSelectionUINoButton()
    {
        SfxPlay(Sfx.Attach);
        exitButtonSelectionUI.gameObject.SetActive(false);
    }



    // 개발자 모드 버튼
    public void Reset()
    {
        SfxPlay(Sfx.Button);

        StartCoroutine(ResetRoutine());
    }

    IEnumerator ResetRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void TestNextStageButton()
    {
        SfxPlay(Sfx.Button);

        StartCoroutine(TestNextStageButtonRoutine());
    }
    
    IEnumerator TestNextStageButtonRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        int sceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (sceneIndex > SceneManager.sceneCount)
        {
            SceneManager.LoadScene(1);
        }

        SceneManager.LoadScene(sceneIndex + 1);
    }

    private void PlayTimeSave()
    {
        if (!PlayerPrefs.HasKey("PlayTime"))
        {
            Debug.Log("저장된 플레이타임이 없습니다.");
            PlayerPrefs.SetFloat("PlayTime", totalPlayTime);
        }
        else
        {
            float savedTotalPlayTime = PlayerPrefs.GetFloat("PlayTime");
            float updatePlayTime = savedTotalPlayTime + totalPlayTime;
            PlayerPrefs.SetFloat("PlayTime", updatePlayTime);
        }
    }

    public void MergeCountSave()
    {
        if (!PlayerPrefs.HasKey("MergeCount"))
        {
            Debug.Log("저장된 머지 횟수 정보가 없습니다.");
            PlayerPrefs.SetInt("MergeCount", mergeCount);
        }
        else
        {
            Debug.Log("저장된 머지 횟수 : " + PlayerPrefs.GetInt("MergeCount"));
            Debug.Log("현재 스테이지 머지 횟수 : " + mergeCount);
            int savedMergeCount = PlayerPrefs.GetInt("MergeCount");
            int updateMergeCount = savedMergeCount + mergeCount;
            PlayerPrefs.SetInt("MergeCount", updateMergeCount);
            Debug.Log("머지 횟수 합산 : " + updateMergeCount + " 저장 완료.");
        }
    }

    public void GameOverCountSave()
    {
        if (!PlayerPrefs.HasKey("GameOverCount"))
        {
            Debug.Log("저장된 게임오버 횟수 정보가 없습니다.");
            PlayerPrefs.SetInt("GameOverCount", gameOverCount);
        }
        else
        {
            Debug.Log("저장된 게임오버 횟수 : " + PlayerPrefs.GetInt("GameOverCount"));
            Debug.Log("현재 스테이지 게임오버 횟수 : " + gameOverCount);
            int savedGameOverCount = PlayerPrefs.GetInt("GameOverCount");
            int updateGameOverCount = savedGameOverCount + gameOverCount;
            PlayerPrefs.SetInt("GameOverCount", updateGameOverCount);
            Debug.Log("게임오버 횟수 합산 : " + updateGameOverCount + " 저장 완료.");
        }
    }

    void Update()
    {
        PlayTimeCheck();

        // Debug.Log("머지 횟수 : " + mergeCount);        
    }

    public void PlayTimeCheck()
    {
        if (isOver || isClear || isPaused)
        {
            return;
        }

        int min = (int)playTime / 60;
        float sec = playTime % 60;
        playTime -= Time.deltaTime;
        totalPlayTime += Time.deltaTime;

        if (playTime >= 60f)
        {

            playTimeText.text = "0" + min + " : " + (int)sec;
            if ((int)sec < 10)
            {
                playTimeText.text = "0" + min + " : " + "0" + (int)sec;
            }
        }

        if (playTime < 60f)
        {
            playTimeText.text = "00 : " + (int)playTime;
            if ((int)sec < 10)
            {
                playTimeText.text = "0" + min + " : " + "0" + (int)sec;
            }
        }

        if (playTime <= 0)
        {
            playTimeText.text = "00 : 00";
        }
    }

    void LateUpdate()
    {
        scoreText.text = "점수 : " + score.ToString();
        lifeText.text = "X " + life.ToString();

        if (maxLevel == 6)
        {
            if (isClear)
            {
                return;
            }

            isClear = true;
            StageClear();
        }
    }

    // 갑작스러운 게임 종료 시, 진행 정보 저장
    private void OnApplicationQuit()
    {        
        // 옵션 UI 를 활성화 (pause)
        Debug.Log("종료 감지. 옵션 UI 를 활성화합니다.");
        optionUI.gameObject.SetActive(true);
        // 진행중이던 현재 스테이지 PlayerPrefs 저장
        Debug.Log("종료 감지. 현재 스테이지 [ " + SceneManager.GetActiveScene().name + " ] 정보를 저장합니다.");
        PlayerPrefs.SetString("Stage", SceneManager.GetActiveScene().name);
        // 플레이 타임 저장
        Debug.Log("종료 감지. 플레이타임 (" + totalPlayTime + ") 을 저장합니다.");
        if (!PlayerPrefs.HasKey("PlayTime"))
        {
            Debug.Log("저장된 플레이타임이 없습니다.");
        }
        else
        {
            PlayTimeSave();
        }
    }

    // SFX
    public void SfxPlay(Sfx type)
    {
        switch (type)
        {
            case Sfx.LevelUp:
                sfxPlayer[sfxCursor].clip = sfxClip[Random.Range(0, 3)];
                break;
            case Sfx.Next:
                sfxPlayer[sfxCursor].clip = sfxClip[3];
                break;
            case Sfx.GameOver:
                sfxPlayer[sfxCursor].clip = sfxClip[4];
                break;
            case Sfx.Attach:
                sfxPlayer[sfxCursor].clip = sfxClip[5];
                break;
            case Sfx.DongleAttach:
                sfxPlayer[sfxCursor].clip = sfxClip[6];
                break;
            case Sfx.Button:
                sfxPlayer[sfxCursor].clip = sfxClip[7];
                break;
            case Sfx.StageClear:
                sfxPlayer[sfxCursor].clip = sfxClip[8];
                break;
        } // end of switch

        sfxPlayer[sfxCursor].Play();
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length;
    }
}