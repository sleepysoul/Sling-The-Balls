using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

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
    public int tempLife;
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

    [Header("-----[ Item ]")]

    // 아이템 사용 버튼 텍스트
    public Text lifeRecoveryButtonText;
    public Text summonDonglesButtonText;
    public Text unbreakableButtonText;
    public Text levelUpAllButtonText;

    // 보유 아이템 카운트
    public int lifeRecoveryItemCount;
    public int summonDonglesItemCount;
    public int unbreakableItemCount;
    public int levelUpAllItemCount;    
    
    // 아이템 사용 잠금
    public bool recoveryLife_ItemisUsed;
    public bool summonDongles_ItemisUsed;
    public bool unbreakable_ItemisUsed;
    public bool levelUpAll_ItemisUsed;
    
    // 아이템 이미지    
    public Image lifeRecoveryImage;
    public Image summonDonglesImage;
    public Image unbreakableImage;
    public Image levelUpAllImage;

    // 아이템 가격
    public int lifeRecoveryItemPrice;
    public int summonDonglesItemPrice;
    public int unbreakableItemPrice;
    public int levelUpAllItemPrice;
    
    // 상점 UI 컴포넌트 설정
    public GameObject shopUI;
    public Image itemImagePosition;
    public Text itemInfo;
    public Button buyButton;
    public Text buyButtonPriceText;

    // 아이템 기타 설정
    private int expectedPrice; // 구매 예정 금액
    public ParticleSystem lifeRecoveryEffect;
    public ParticleSystem lifeRecovery_ItemButtonEffect;
    public ParticleSystem SummonDongles_ItemButtonEffect;
    public ParticleSystem Unbreakable_ItemButtonEffect;
    public ParticleSystem LevelUpAll_ItemButtonEffect;
    public enum Item { LifeRecovery , SummonDongles , Unbreakable , LevelUpAll }
        
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

        // 보유 아이템 체크 (키에도 저장되어 있는지 우선 체크)
        ItemCheck();

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

                // 게임 플레이 UI 내 Dongle 동작 멈춤
        Dongle[] dongles = GameObject.FindObjectsOfType<Dongle>();

        for (int index = 0; index < dongles.Length; index++)
        {
            dongles[index].rb.simulated = false;
        }

        // 게임 플레이 UI 내 Obstacle 동작 멈춤
        GameObject[] cubee = GameObject.FindGameObjectsWithTag("Cubee");

        for (int j = 0; j < cubee.Length; j++)
        {
            cubee[j].GetComponent<Rigidbody2D>().simulated = false;
        }

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

        // 터치 패드 비활성화
        touchPad.gameObject.SetActive(false);

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

        // 게임 플레이 UI 내 Dongle 동작 멈춤
        Dongle[] dongles = GameObject.FindObjectsOfType<Dongle>();

        for (int index = 0; index < dongles.Length; index++)
        {
            dongles[index].rb.simulated = false;
        }

        // 게임 플레이 UI 내 Obstacle 동작 멈춤
        GameObject[] cubee = GameObject.FindGameObjectsWithTag("Cubee");

        for (int j = 0; j < cubee.Length; j++)
        {
            cubee[j].GetComponent<Rigidbody2D>().simulated = false;
        }

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
        // CheckDollar() => 키 없으면 0 , 있으면 보유 달러 리턴(int)
        optionDollarText.text = "보유 금액 : $" + CheckDollar().ToString();

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

        if (resetButtonSelectionUI.activeSelf || 
                exitButtonSelectionUI.activeSelf || 
                stageResetButtonSelectionUI.activeSelf || 
                statsResetButtonSelectionUI.activeSelf )
        {
            return;
        }

        // 게임 플레이 UI 내 Dongle 동작 잠금 해제
        Dongle[] dongles = GameObject.FindObjectsOfType<Dongle>();

        for (int index = 0; index < dongles.Length; index++)
        {
            dongles[index].rb.simulated = true;
        }

        // 게임 플레이 UI 내 Obstacle 동작 잠금 해제
        GameObject[] cubee = GameObject.FindGameObjectsWithTag("Cubee");

        for (int j = 0; j < cubee.Length; j++)
        {
            cubee[j].GetComponent<Rigidbody2D>().simulated = true;
        }

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
        HoldingItemCheck();

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
        if (life < 3)
        {
            lifeText.color = Color.red;
        } 
        else
        {
            lifeText.color = Color.white;
        }

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
        OptionButtonPressed();
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

    /* ITEM BUTTON UI */

    private void ItemCheck()
    {       

        if (!PlayerPrefs.HasKey("LifeRecovery"))
        {
            Debug.Log("Life Recovery 아이템 정보가 없습니다.");
            Debug.Log("Life Recovery 초기 보너스를 설정합니다. +3");
            PlayerPrefs.SetInt("LifeRecovery", 3);
            lifeRecoveryItemCount = PlayerPrefs.GetInt("LifeRecovery");            
        }
        else
        {
            Debug.Log("Life Recovery 아이템 정보가 있습니다.");
            lifeRecoveryItemCount = PlayerPrefs.GetInt("LifeRecovery");
            Debug.Log("Life Recovery 아이템 보유 : " + lifeRecoveryItemCount);
        }

        if (!PlayerPrefs.HasKey("SummonDongles"))
        {
            Debug.Log("SummonDongles 아이템 정보가 없습니다.");
            Debug.Log("SummonDongles 초기 보너스를 설정합니다. +3");
            PlayerPrefs.SetInt("SummonDongles", 3);
            summonDonglesItemCount = PlayerPrefs.GetInt("SummonDongles");
        }
        else
        {
            Debug.Log("SummonDongles 아이템 정보가 있습니다.");
            summonDonglesItemCount = PlayerPrefs.GetInt("SummonDongles");
            Debug.Log("SummonDongles 아이템 보유 : " + summonDonglesItemCount);
        }

        if (!PlayerPrefs.HasKey("Unbreakable"))
        {
            Debug.Log("Unbreakable 아이템 정보가 없습니다.");
            Debug.Log("Unbreakable 초기 보너스를 설정합니다. +3");
            PlayerPrefs.SetInt("Unbreakable", 3);
            unbreakableItemCount = PlayerPrefs.GetInt("Unbreakable");
        }
        else
        {
            Debug.Log("Unbreakable 아이템 정보가 있습니다.");
            unbreakableItemCount = PlayerPrefs.GetInt("Unbreakable");
            Debug.Log("Unbreakable 아이템 보유 : " + unbreakableItemCount);
        }

        if (!PlayerPrefs.HasKey("LevelUpAll"))
        {
            Debug.Log("LevelUpAll 아이템 정보가 없습니다.");
            Debug.Log("LevelUpAll 초기 보너스를 설정합니다. +3");
            PlayerPrefs.SetInt("LevelUpAll", 3);
            levelUpAllItemCount = PlayerPrefs.GetInt("LevelUpAll");
        }
        else
        {
            Debug.Log("LevelUpAll 아이템 정보가 있습니다.");
            levelUpAllItemCount = PlayerPrefs.GetInt("LevelUpAll");
            Debug.Log("LevelUpAll 아이템 보유 : " + levelUpAllItemCount);
        }
    }

    private void HoldingItemCheck()
    {
        // 아이템 사용 버튼 
        if (PlayerPrefs.GetInt("LifeRecovery") < 1)
        {
            lifeRecovery_ItemButtonEffect.gameObject.SetActive(false);
            lifeRecoveryButtonText.text = "X0";
        }
        else if (PlayerPrefs.GetInt("LifeRecovery") > 0)
        {
            lifeRecoveryButtonText.text = "X" + PlayerPrefs.GetInt("LifeRecovery").ToString();
            lifeRecovery_ItemButtonEffect.gameObject.SetActive(true);
            lifeRecovery_ItemButtonEffect.Play();
        }
    }

    // Item1_LifeRecoveryPressed
    public void LifeRecoveryPressed()
    {
        SfxPlay(Sfx.Button);
        
        if (recoveryLife_ItemisUsed)
        {
            Debug.Log("리커버리 아이템을 이미 사용했습니다.");
            return;
        }
        else
        {
            if (PlayerPrefs.GetInt("LifeRecovery") > 0)
            {
                UseItem(Item.LifeRecovery);
                recoveryLife_ItemisUsed = true;
                Debug.Log("리커버리 아이템을 사용했습니다.");
            }
            else
            {
                CallShop(Item.LifeRecovery);
            }
        }
    }

    // Item2_SummonDonglesPressed
    public void SummonDonglesPressed()
    {
        SfxPlay(Sfx.Button);

        if (summonDongles_ItemisUsed)
        {
            Debug.Log("동글 소환 아이템을 이미 사용했습니다.");
            return;
        }
        else
        {
            if (PlayerPrefs.GetInt("SummonDongles") > 0)
            {
                UseItem(Item.SummonDongles);
                summonDongles_ItemisUsed = true;
                Debug.Log("동글 소환 아이템을 사용했습니다.");
            }
            else
            {
                CallShop(Item.SummonDongles);
            }
        }
    }

    // Item3_UnbreakablePressed
    public void UnbreakablePressed()
    {
        SfxPlay(Sfx.Button);

        if (unbreakable_ItemisUsed)
        {
            Debug.Log("언브레이커블 아이템을 이미 사용했습니다.");
            return;
        }
        else
        {
            if (PlayerPrefs.GetInt("Unbreakable") > 0)
            {
                UseItem(Item.Unbreakable);
                unbreakable_ItemisUsed = true;
                Debug.Log("언브레이커블 아이템을 사용했습니다.");
            }
            else
            {
                CallShop(Item.Unbreakable);
            }
        }
    }

    // Item4_LevelUpAllPressed
    public void LevelUpAllPressed()
    {
        SfxPlay(Sfx.Button);

        if (levelUpAll_ItemisUsed)
        {
            Debug.Log("레벨업 아이템을 이미 사용했습니다.");
            return;
        }
        else
        {
            if (PlayerPrefs.GetInt("LevelUpAll") > 0)
            {
                UseItem(Item.LevelUpAll);
                levelUpAll_ItemisUsed = true;
                Debug.Log("레벨업 아이템을 사용했습니다.");
            }
            else
            {
                CallShop(Item.LevelUpAll);
            }
        }
    }

    private void UseItem(Item name)
    {
        // 아이템 이름에 따라 로직 처리
        switch(name)
        {
            case Item.LifeRecovery:
                lifeRecoveryEffect.Play();
                life += 5;
                lifeRecoveryItemCount--;
                Debug.Log("LifeRecovery 아이템을 사용합니다. 보유 : " + lifeRecoveryItemCount);
                PlayerPrefs.SetInt("LifeRecovery", lifeRecoveryItemCount);                
                break;
            case Item.SummonDongles:
                StartCoroutine(SpawnDongle());
                Debug.Log("동글을 소환합니다.");
                summonDonglesItemCount--;
                Debug.Log("SummonDongles 아이템을 사용합니다. 보유 : " + summonDonglesItemCount);
                PlayerPrefs.SetInt("SummonDongles", summonDonglesItemCount);
                break;
            case Item.Unbreakable:
                LifeFix();
                unbreakableItemCount--;
                Debug.Log("Unbreakable 아이템을 사용합니다. 보유 : " + unbreakableItemCount);
                PlayerPrefs.SetInt("Unbreakable", unbreakableItemCount);
                break;
            case Item.LevelUpAll:
                LevelUpAll();
                levelUpAllItemCount--;
                Debug.Log("Level Up All 아이템을 사용합니다. 보유 : " + levelUpAllItemCount);
                PlayerPrefs.SetInt("LevelUpAll", levelUpAllItemCount);
                break;
        }
    }

    [System.Obsolete]
    private void LifeFix()
    {
        tempLife = life;
        life = 99;
        lifeRecoveryEffect.loop = true;
        lifeRecoveryEffect.Play();
        Invoke("LifeRestore", 30f);
    }

    [System.Obsolete]
    private void LifeRestore()
    {
        lifeRecoveryEffect.loop = false;
        life = tempLife;
    }

    private void LevelUpAll()
    {
        // 게임 플레이 UI 내 Dongle 레벨업
        Dongle[] dongles = GameObject.FindObjectsOfType<Dongle>();

        for (int index = 0; index < dongles.Length; index++)
        {
            dongles[index].LevelUp();
        }
    }

    private void CallShop(Item name)
    {       

        // Shop UI 호출
        Debug.Log("아이템이 없습니다. 상점 UI 를 호출합니다.");
        shopUI.gameObject.SetActive(true);
        touchPad.gameObject.SetActive(false);
        isPaused = true;

        /* 아이템 이름에 따라 Shop UI 에 표기 */
        // 1. 이미지
        // 2. 설명
        // 3. 가격        
        switch (name)
        {           
            case Item.LifeRecovery:
                itemImagePosition.sprite = lifeRecoveryImage.sprite;
                itemInfo.text = "던질 수 있는 동글의 라이프를 +5 회복합니다.";
                buyButtonPriceText.text = "$" + lifeRecoveryItemPrice.ToString();
                CheckHoldingDollar(lifeRecoveryItemPrice);
                expectedPrice = lifeRecoveryItemPrice;
                break;
            case Item.SummonDongles:
                itemImagePosition = summonDonglesImage;
                itemInfo.text = "동글 15마리를 즉시 소환합니다.";
                buyButtonPriceText.text = "$" + summonDonglesItemPrice.ToString();
                CheckHoldingDollar(summonDonglesItemPrice);
                expectedPrice = summonDonglesItemPrice;
                break;
            case Item.Unbreakable:
                itemImagePosition = unbreakableImage;
                itemInfo.text = "30초간 라이프가 99로 변경됩니다!";
                buyButtonPriceText.text = "$" + unbreakableItemPrice.ToString();
                CheckHoldingDollar(unbreakableItemPrice);
                expectedPrice = unbreakableItemPrice;
                break;
            case Item.LevelUpAll:
                itemImagePosition = levelUpAllImage;
                itemInfo.text = "모든 동글 +1 레벨업 !!!";
                buyButtonPriceText.text = "$" + levelUpAllItemPrice.ToString();
                CheckHoldingDollar(levelUpAllItemPrice);
                expectedPrice = levelUpAllItemPrice;
                break;           
        }

        itemImagePosition.gameObject.SetActive(true); 
       
    }

    private void CheckHoldingDollar(int price)
    {
        Debug.Log("넘어온 가격 : $ " + price);
        // 함수로 요청된 아이템 가격보다 보유 달러가 부족한 경우
        if (CheckDollar() < price)
        {
            Debug.Log("보유 달러가 부족합니다.");
            // 구매 버튼 컬러 = 회색
            buyButton.GetComponent<Image>().color = Color.gray;
            buyButton.interactable = false;
            // 버튼 텍스트
            buyButtonPriceText.color = Color.yellow;
            buyButtonPriceText.text = "달러 부족";
        }
        else
        {
            Debug.Log("보유 달러가 충분합니다.");
            // 구매 버튼 컬러 = 기본색
            buyButton.GetComponent<Image>().color = new Color(1f, 0.6914676f, 0f);
            buyButton.interactable = true;
            // 버튼 텍스트
            buyButtonPriceText.color = new Color(0.1960784f, 0.1960784f, 0.1960784f);
            buyButtonPriceText.text = "$ " + price.ToString();
        }
    }

    public void BuyButtonPressed()
    {
        // 구매 아이템 구분은 Shop UI 호출 시 switch 구분하여 기대 가격에 설정
        // 기대 가격 불러와서 보유 달러 차감하고 Dollar 키에 저장
        Debug.Log("보유 달러 : $ " + CheckDollar());
        PlayerPrefs.SetInt("Dollar", CheckDollar() - expectedPrice);
        Debug.Log(expectedPrice + "만큼 차감!");
        Debug.Log("차감 후 보유 달러 : " + CheckDollar());

        // 아이템 카운트 추가 후 키에 저장
        switch (expectedPrice)
        {
            case 250000:
                Debug.Log("보유 LifeRecovery 아이템 : " + PlayerPrefs.GetInt("LifeRecovery"));
                PlayerPrefs.SetInt("LifeRecovery", PlayerPrefs.GetInt("LifeRecovery") + 1);                
                Debug.Log("증가된 LifeRecovery 아이템 : " + PlayerPrefs.GetInt("LifeRecovery"));
                break;
            case 500000:
                Debug.Log("보유 SummonDongles 아이템 : " + PlayerPrefs.GetInt("SummonDongles"));
                PlayerPrefs.SetInt("SummonDongles", PlayerPrefs.GetInt("SummonDongles") + 1);
                Debug.Log("증가된 SummonDongles 아이템 : " + PlayerPrefs.GetInt("SummonDongles"));
                break;
            case 1000000:
                Debug.Log("보유 Unbreakable 아이템 : " + PlayerPrefs.GetInt("Unbreakable"));
                PlayerPrefs.SetInt("Unbreakable", PlayerPrefs.GetInt("Unbreakable") + 1);
                Debug.Log("증가된 Unbreakable 아이템 : " + PlayerPrefs.GetInt("Unbreakable"));
                break;
            case 5000000:
                Debug.Log("보유 LevelUpAll 아이템 : " + PlayerPrefs.GetInt("LevelUpAll"));
                PlayerPrefs.SetInt("LevelUpAll", PlayerPrefs.GetInt("LevelUpAll") + 1);
                Debug.Log("증가된 LevelUpAll 아이템 : " + PlayerPrefs.GetInt("LevelUpAll"));
                break;
        }

        PlayerPrefs.Save();

        // 상점 UI 비활성화
        CloseShopUI();
    }

    public void CloseShopUI()
    {
        shopUI.gameObject.SetActive(false);
        touchPad.gameObject.SetActive(true);
        isPaused = false;
    }

    // 보유 달러 체크 후 리턴
    private int CheckDollar()
    {
        
        if (!PlayerPrefs.HasKey("Dollar"))
        {
            PlayerPrefs.SetInt("Dollar", 0);
            return 0;
        }
        else
        {
            return PlayerPrefs.GetInt("Dollar");
        }
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

    public void TestDollarResetButton()
    {        
        PlayerPrefs.SetInt("Dollar", 0);
        Debug.Log("보유 달러를 리셋했습니다. 보유 달러 : " + CheckDollar());

        PlayerPrefs.Save();
    }

    public void TestDollarChargeButton()
    {        
        PlayerPrefs.SetInt("Dollar", CheckDollar() + 250000);
        Debug.Log("보유 달러를 충전했습니다. 보유 달러 : " + CheckDollar());
        
        PlayerPrefs.Save();
    }

    public void TestItemResetButton()
    {
        PlayerPrefs.SetInt("LifeRecovery", 0);
        Debug.Log("LifeRecovery 를 리셋했습니다. 보유 : " + PlayerPrefs.GetInt("LifeRecovery"));
        PlayerPrefs.SetInt("SummonDongles", 0);
        Debug.Log("SummonDongles 를 리셋했습니다. 보유 : " + PlayerPrefs.GetInt("SummonDongles"));
        PlayerPrefs.SetInt("Unbreakable", 0);
        Debug.Log("Unbreakable 를 리셋했습니다. 보유 : " + PlayerPrefs.GetInt("Unbreakable"));
        PlayerPrefs.SetInt("LevelUpAll", 0);
        Debug.Log("LevelUpAll 를 리셋했습니다. 보유 : " + PlayerPrefs.GetInt("LevelUpAll"));

        PlayerPrefs.Save();
    }
}