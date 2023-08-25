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
    public Transform spawnPoint_SummonDongles;
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
    public enum Sfx 
    { 
        LevelUp, 
        Next, 
        GameOver, 
        Attach, 
        DongleAttach, 
        Button, 
        StageClear,
        LifeRecovery
    }

    [Header("===========[ UI ]")]
    [Header("-----[ GamePlay UI ]")]
    public GameObject touchPad;
    public Text stageText;
    public Text lifeText;
    public Text playTimeText;
    public Text scoreText;
    public Image caution;

    public enum Item 
    { 
        LifeRecovery , 
        SummonDongles , 
        Unbreakable , 
        LevelUpAll , 
        BuyButton , 
        AdsButton 
    }
    
    [Header("===========[ Item UI ]")]

    // 아이템 사용 버튼 (아이템 사용 후 컬러 변경을 위함)
    [Header("-----[ Item Button ]")]
    public GameObject lifeRecoveryButton;
    public GameObject summonDonglesButton;
    public GameObject unbreakableButton;
    public GameObject levelUpAllButton;
    public Image lifeRecoveryRedDot;
    public Image summonDonglesRedDot;
    public Image unbreakableRedDot;
    public Image levelUpAllRedDot;

    // 아이템 사용 버튼 텍스트
    [Header("-----[ Item Button Text ]")]    
    public Text lifeRecoveryButtonText;
    public Text summonDonglesButtonText;
    public Text unbreakableButtonText;
    public Text levelUpAllButtonText;

    // 보유 아이템 카운트
    [Header("-----[ Item Count ]")]
    public int lifeRecoveryItemCount;
    public int summonDonglesItemCount;
    public int unbreakableItemCount;
    public int levelUpAllItemCount;

    // 아이템 사용 잠금
    [Header("-----[ Used Item Bool ]")]
    public bool recoveryLife_ItemisUsed;
    public bool summonDongles_ItemisUsed;
    public bool unbreakable_ItemisUsed;
    public bool levelUpAll_ItemisUsed;

    // Shop UI 내부 이미지
    [Header("-----[ Shop UI _ Item Image ]")]
    public Image lifeRecoveryImage;
    public Image summonDonglesImage;
    public Image unbreakableImage;
    public Image levelUpAllImage;

    // Shop UI 버튼 레드 닷
    [Header("-----[ Shop UI _ Item Image ]")]
    public Image buyButtonRedDot;
    public Image adsButtonRedDot;

    // 아이템 가격
    [Header("-----[ Item Price ]")]
    public int lifeRecoveryItemPrice;
    public int summonDonglesItemPrice;
    public int unbreakableItemPrice;
    public int levelUpAllItemPrice;

    // 상점 UI 컴포넌트 설정
    [Header("-----[ Shop UI _ Core ]")]
    private int expectedPrice; // 구매 예정 금액
    public GameObject shopUI;
    public Image itemImagePosition;
    public Text itemInfo;
    public Button buyButton;
    public Text buyButtonPriceText;

    // 아이템 이펙트 설정
    [Header("-----[ Item Effects ]")]
    public ParticleSystem lifeRecoveryEffect;
    public ParticleSystem lifeRecovery_ItemButtonEffect;
    public ParticleSystem summonDongles_ItemButtonEffect;
    public ParticleSystem unbreakable_ItemButtonEffect;
    public ParticleSystem levelUpAll_ItemButtonEffect;

    [Header("===========[ Option UI ]")]
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

    [Header("===========[ GameOver UI ]")]
    public GameObject endGroup;
    public Text gameOverSubScoreText; // 현재 스테이지에서 얻은 점수
    public Text overTotalScoreText;
    public Text overDollarText;

    [Header("===========[ GameClear UI ]")]    
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

        if (summonDongles_ItemisUsed)
        {
            Debug.Log("SummonDongles 아이템으로 소환을 시작합니다.");
            for (int index = 0; index < spawnNumber; index++)
            {
                for (int j = 0; j < donglePool.Count; j++)
                {
                    poolCursor = (poolCursor + 1) % donglePool.Count;
                    if (!donglePool[poolCursor].gameObject.activeSelf)
                    {
                        spawnDongle = donglePool[poolCursor];

                    }
                }
                spawnDongle.isMerge = true;
                spawnPoint.transform.position = new Vector3(spawnPoint_SummonDongles.position.x + Random.Range(-spawnXBoundary, spawnXBoundary), spawnPoint_SummonDongles.position.y);
                spawnDongle.transform.position = spawnPoint.transform.position;
                spawnDongle.level = Random.Range(minLevel, maxLevel);
                spawnDongle.GetComponent<SpringJoint2D>().enabled = false;
                spawnDongle.gameObject.SetActive(true);

                yield return new WaitForSeconds(0.3f);
                spawnDongle.isMerge = false;
            }
        }
        else
        {
            Debug.Log("게임 시작 동글 보너스 소환을 시작합니다.");
            for (int index = 0; index < spawnNumber; index++)
            {
                for (int j = 0; j < donglePool.Count; j++)
                {
                    poolCursor = (poolCursor + 1) % donglePool.Count;
                    if (!donglePool[poolCursor].gameObject.activeSelf)
                    {
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
        SfxPlay(Sfx.Button);
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
        optionDollarText.text = "보유 금액 : $" + GetDollar().ToString();

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
        SfxPlay(Sfx.Button);

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
        SfxPlay(Sfx.Button);

        Invoke("StageResetButtonSelectionUI", 0.2f);

    }

    private void StageResetButtonSelectionUI()
    {
        stageResetButtonSelectionUI.gameObject.SetActive(true);
    }

    public void StageResetButtonSelectionUIYesButton()
    {
        SfxPlay(Sfx.Button);
        SceneManager.LoadScene(1);
    }

    public void StageResetButtonSelectionUINoButton()
    {
        SfxPlay(Sfx.Button);
        stageResetButtonSelectionUI.gameObject.SetActive(false);
    }

    // Option UI > STATS RESET Button
    public void StatsResetButtonPressed()
    {
        SfxPlay(Sfx.Button);

        Invoke("StatsResetButtonSelectionUI", 0.2f);

    }

    private void StatsResetButtonSelectionUI()
    {
        statsResetButtonSelectionUI.gameObject.SetActive(true);
    }
    
    public void StatsResetButtonSelectionUIYesButton()
    {
        SfxPlay(Sfx.Button);

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
        SfxPlay(Sfx.Button);
        statsResetButtonSelectionUI.gameObject.SetActive(false);
    }

    // Option UI > RESET Button
    public void OptionResetButtonPressed()
    {
        SfxPlay(Sfx.Button);

        Invoke("ResetButtonSelectionUI", 0.3f);
        
    }
    
    private void ResetButtonSelectionUI()
    {
        resetButtonSelectionUI.gameObject.SetActive(true);
    }

    public void ResetButtonSelectionUIYesButton()
    {
        SfxPlay(Sfx.Button);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ResetButtonSelectionUINoButton()
    {
        SfxPlay(Sfx.Button);
        resetButtonSelectionUI.gameObject.SetActive(false);
    }


    // Option UI > EXIT Button
    public void OptionExitButtonPressed()
    {
        SfxPlay(Sfx.Button);

        Invoke("ExitButtonSelectionUI", 0.3f);

    }

    private void ExitButtonSelectionUI()
    {
        exitButtonSelectionUI.gameObject.SetActive(true);
    }

    public void ExitButtonSelectionUIYesButton()
    {
        SfxPlay(Sfx.Button);

        // 플레이 정보 저장 함수 호출

        // 게임 종료 → 메인으로 이동
        SceneManager.LoadScene(0);
    }

    public void ExitButtonSelectionUINoButton()
    {
        SfxPlay(Sfx.Button);
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

        HoldingItemCheck();

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
            case Sfx.LifeRecovery:
                sfxPlayer[sfxCursor].clip = sfxClip[9];
                break;
        } // end of switch

        sfxPlayer[sfxCursor].Play();
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length;
    }

    /* ITEM BUTTON UI */

    private void ItemCheck()
    {
        GetLifeRecovery();
        GetSummonDongles();
        GetUnbreakable();
        GetLevelUpAll();
        Debug.Log(" * * * LifeRecovery 보유 : " + GetLifeRecovery());
        Debug.Log(" * * * SummonDongle 보유 : " + GetSummonDongles());
        Debug.Log(" * * * Unbreakable 보유 : " + GetUnbreakable());
        Debug.Log(" * * * LevelUpAll 보유 : " + GetLevelUpAll());
    }

    private void HoldingItemCheck()
    {        
        if (GetLifeRecovery() < 1)
        {
            lifeRecovery_ItemButtonEffect.gameObject.SetActive(false);
            lifeRecoveryButtonText.text = "X0";
            if (recoveryLife_ItemisUsed)
            {
                OffRedDot(Item.LifeRecovery);
            }
            else
            {
                OnRedDot(Item.LifeRecovery);
            }            
        }
        else if (GetLifeRecovery() > 0)
        {
            lifeRecoveryButtonText.text = "X" + GetLifeRecovery().ToString();
            if (!recoveryLife_ItemisUsed)
            {
                lifeRecovery_ItemButtonEffect.gameObject.SetActive(true);
                lifeRecovery_ItemButtonEffect.Play();
            }
            OffRedDot(Item.LifeRecovery);
        }

        if (GetSummonDongles() < 1)
        {
            summonDongles_ItemButtonEffect.gameObject.SetActive(false);
            summonDonglesButtonText.text = "X0";
            if (summonDongles_ItemisUsed)
            {
                OffRedDot(Item.SummonDongles);
            }
            else
            {
                OnRedDot(Item.SummonDongles);
            }
        }
        else if (GetSummonDongles() > 0)
        {
            summonDonglesButtonText.text = "X" + GetSummonDongles().ToString();
            if (!summonDongles_ItemisUsed)
            {
                summonDongles_ItemButtonEffect.gameObject.SetActive(true);
                summonDongles_ItemButtonEffect.Play();
            }
            OffRedDot(Item.SummonDongles);
        }
        
        if (GetUnbreakable() < 1)
        {
            unbreakable_ItemButtonEffect.gameObject.SetActive(false);
            unbreakableButtonText.text = "X0";
            if (unbreakable_ItemisUsed)
            {
                OffRedDot(Item.Unbreakable);
            }
            else
            {
                OnRedDot(Item.Unbreakable);
            }
        }
        else if (GetUnbreakable() > 0)
        {
            unbreakableButtonText.text = "X" + GetUnbreakable().ToString();
            if (!unbreakable_ItemisUsed)
            {
                unbreakable_ItemButtonEffect.gameObject.SetActive(true);
                unbreakable_ItemButtonEffect.Play();
            }
            OffRedDot(Item.Unbreakable);
        }

        if (GetLevelUpAll() < 1)
        {
            levelUpAll_ItemButtonEffect.gameObject.SetActive(false);
            levelUpAllButtonText.text = "X0";
            if (levelUpAll_ItemisUsed)
            {
                OffRedDot(Item.LevelUpAll);
            }
            else
            {
                OnRedDot(Item.LevelUpAll);
            }
        }
        else if (GetLevelUpAll() > 0)
        {
            levelUpAllButtonText.text = "X" + GetLevelUpAll().ToString();
            if (!levelUpAll_ItemisUsed)
            {
                levelUpAll_ItemButtonEffect.gameObject.SetActive(true);
                levelUpAll_ItemButtonEffect.Play();
            }
            OffRedDot(Item.LevelUpAll);
        }
    }

    /* Item Button Pressed */ 

    // Item1_LifeRecoveryPressed
    public void LifeRecoveryPressed()
    {
        SfxPlay(Sfx.Button);
        lifeRecovery_ItemButtonEffect.gameObject.SetActive(false);

        if (recoveryLife_ItemisUsed)
        {
            Debug.Log("리커버리 아이템을 이미 사용했습니다.");
            return;
        }
        else
        {
            if (GetLifeRecovery() > 0)
            {
                recoveryLife_ItemisUsed = true;                
                UseItem(Item.LifeRecovery);
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
        summonDongles_ItemButtonEffect.gameObject.SetActive(false);

        if (summonDongles_ItemisUsed)
        {
            Debug.Log("동글 소환 아이템을 이미 사용했습니다.");
            return;
        }
        else
        {
            if (GetSummonDongles() > 0)
            {
                summonDongles_ItemisUsed = true;
                UseItem(Item.SummonDongles);
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
        unbreakable_ItemButtonEffect.gameObject.SetActive(false);

        if (unbreakable_ItemisUsed)
        {
            Debug.Log("언브레이커블 아이템을 이미 사용했습니다.");
            return;
        }
        else
        {
            if (GetUnbreakable() > 0)
            {
                unbreakable_ItemisUsed = true;
                UseItem(Item.Unbreakable);
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
        levelUpAll_ItemButtonEffect.gameObject.SetActive(false);

        if (levelUpAll_ItemisUsed)
        {
            Debug.Log("레벨업 아이템을 이미 사용했습니다.");
            return;
        }
        else
        {
            if (GetLevelUpAll() > 0)
            {
                levelUpAll_ItemisUsed = true;
                UseItem(Item.LevelUpAll);
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
                SfxPlay(Sfx.LifeRecovery);
                lifeRecoveryButton.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);
                PlayerPrefs.SetInt("LifeRecovery", GetLifeRecovery() - 1);
                lifeRecoveryEffect.Play();
                life += 5;                
                Debug.Log("LifeRecovery 아이템을 사용합니다. 보유 : " + GetLifeRecovery());
                break;
            case Item.SummonDongles:
                summonDonglesButton.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);
                PlayerPrefs.SetInt("SummonDongles", GetSummonDongles() - 1);
                Debug.Log("SummonDongles 아이템을 사용합니다. 보유 : " + GetSummonDongles());
                StartCoroutine(SpawnDongle());
                Debug.Log("동글을 소환합니다.");
                break;
            case Item.Unbreakable:
                unbreakableButton.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);
                PlayerPrefs.SetInt("Unbreakable", GetUnbreakable() - 1);
                Debug.Log("Unbreakable 아이템을 사용합니다. 보유 : " + GetUnbreakable());
                LifeFix();
                break;
            case Item.LevelUpAll:
                levelUpAllButton.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);
                PlayerPrefs.SetInt("LevelUpAll", GetLevelUpAll() - 1);
                Debug.Log("LevelUpAll 아이템을 사용합니다. 보유 : " + GetLevelUpAll());
                LevelUpAll();
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
        if (GetDollar() < price)
        {
            Debug.Log("보유 달러가 부족합니다.");
            // 구매 버튼 컬러 = 회색
            buyButton.GetComponent<Image>().color = Color.gray;
            buyButton.interactable = false;
            OffRedDot(Item.BuyButton);
            // 버튼 텍스트
            buyButtonPriceText.color = Color.yellow;
            buyButtonPriceText.fontSize = 21;
            buyButtonPriceText.text = "$" + (price - GetDollar()).ToString() + "\n 달러 부족";
        }
        else
        {
            Debug.Log("보유 달러가 충분합니다.");
            // 구매 버튼 컬러 = 기본색
            buyButton.GetComponent<Image>().color = new Color(1f, 0.6914676f, 0f);
            buyButton.interactable = true;
            OnRedDot(Item.BuyButton);
            // 버튼 텍스트
            buyButtonPriceText.color = new Color(0.1960784f, 0.1960784f, 0.1960784f);
            buyButtonPriceText.fontSize = 30;
            buyButtonPriceText.text = "$ " + price.ToString();
        }
    }

    public void BuyButtonPressed()
    {
        SfxPlay(Sfx.Button);
        // 구매 아이템 구분은 Shop UI 호출 시 switch 구분하여 기대 가격에 설정
        // 기대 가격 불러와서 보유 달러 차감하고 Dollar 키에 저장
        Debug.Log("보유 달러 : $ " + GetDollar());
        PlayerPrefs.SetInt("Dollar", GetDollar() - expectedPrice);
        Debug.Log(expectedPrice + "만큼 차감!");
        Debug.Log("차감 후 보유 달러 : " + GetDollar());

        // 아이템 카운트 추가 후 키에 저장
        switch (expectedPrice)
        {
            case 250000:
                Debug.Log("보유 LifeRecovery 아이템 : " + GetLifeRecovery());
                PlayerPrefs.SetInt("LifeRecovery", GetLifeRecovery() + 1);                
                Debug.Log("증가된 LifeRecovery 아이템 : " + GetLifeRecovery());
                break;
            case 500000:
                Debug.Log("보유 SummonDongles 아이템 : " + GetSummonDongles());
                PlayerPrefs.SetInt("SummonDongles", GetSummonDongles() + 1);
                Debug.Log("증가된 SummonDongles 아이템 : " + GetSummonDongles());
                break;
            case 1000000:
                Debug.Log("보유 Unbreakable 아이템 : " + GetUnbreakable());
                PlayerPrefs.SetInt("Unbreakable", GetUnbreakable() + 1);
                Debug.Log("증가된 Unbreakable 아이템 : " + GetUnbreakable());
                break;
            case 5000000:
                Debug.Log("보유 LevelUpAll 아이템 : " + GetLevelUpAll());
                PlayerPrefs.SetInt("LevelUpAll", GetLevelUpAll() + 1);
                Debug.Log("증가된 LevelUpAll 아이템 : " + GetLevelUpAll());
                break;
        }

        PlayerPrefs.Save();

        // 상점 UI 비활성화
        CloseShopUI();
    }

    public void CloseShopUI()
    {
        SfxPlay(Sfx.Button);
        shopUI.gameObject.SetActive(false);
        touchPad.gameObject.SetActive(true);
        isPaused = false;
    }


    /* PlayerPrefs GET 영역 */ 

    // 보유 달러 체크 후 리턴
    private int GetDollar()
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

    private int GetLifeRecovery()
    {

        if (!PlayerPrefs.HasKey("LifeRecovery"))
        {
            Debug.Log("Life Recovery 아이템 정보가 없습니다.");
            Debug.Log("Life Recovery 초기 보너스를 설정합니다. +3");
            PlayerPrefs.SetInt("LifeRecovery", 3);            
        }

        return PlayerPrefs.GetInt("LifeRecovery");
    }

    private int GetSummonDongles()
    {

        if (!PlayerPrefs.HasKey("SummonDongles"))
        {
            Debug.Log("SummonDongles 아이템 정보가 없습니다.");
            Debug.Log("SummonDongles 초기 보너스를 설정합니다. +3");
            PlayerPrefs.SetInt("SummonDongles", 3);
        }
        
        return PlayerPrefs.GetInt("SummonDongles");
    }

    private int GetUnbreakable()
    {

        if (!PlayerPrefs.HasKey("Unbreakable"))
        {
            Debug.Log("Unbreakable 아이템 정보가 없습니다.");
            Debug.Log("Unbreakable 초기 보너스를 설정합니다. +3");
            PlayerPrefs.SetInt("Unbreakable", 3);
        }

        return PlayerPrefs.GetInt("Unbreakable");
    }

    private int GetLevelUpAll()
    {

        if (!PlayerPrefs.HasKey("LevelUpAll"))
        {
            Debug.Log("LevelUpAll 아이템 정보가 없습니다.");
            Debug.Log("LevelUpAll 초기 보너스를 설정합니다. +3");
            PlayerPrefs.SetInt("LevelUpAll", 3);
        }

        return PlayerPrefs.GetInt("LevelUpAll");
    }



    /* 
     * Red Dot ON/OFF 
     * 
     * - Red Dot On -
     * Holding Item = False
     * Used Item = False
     * 
     */
    private void OnRedDot(Item type)
    {
        switch (type)
        {
            case Item.LifeRecovery:
                lifeRecoveryRedDot.gameObject.SetActive(true);
                break;
            case Item.SummonDongles:
                summonDonglesRedDot.gameObject.SetActive(true);
                break;
            case Item.Unbreakable:
                unbreakableRedDot.gameObject.SetActive(true);
                break;
            case Item.LevelUpAll:
                levelUpAllRedDot.gameObject.SetActive(true);
                break;
            case Item.BuyButton:
                buyButtonRedDot.gameObject.SetActive(true);
                break;
            case Item.AdsButton:
                adsButtonRedDot.gameObject.SetActive(true);
                break;
        }
    }

    private void OffRedDot(Item type)
    {        
        switch (type)
        {
            case Item.LifeRecovery:
                lifeRecoveryRedDot.gameObject.SetActive(false);
                break;
            case Item.SummonDongles:
                summonDonglesRedDot.gameObject.SetActive(false);
                break;
            case Item.Unbreakable:
                unbreakableRedDot.gameObject.SetActive(false);
                break;
            case Item.LevelUpAll:
                levelUpAllRedDot.gameObject.SetActive(false);
                break;
            case Item.BuyButton:
                buyButtonRedDot.gameObject.SetActive(false);
                break;
            case Item.AdsButton:
                adsButtonRedDot.gameObject.SetActive(false);
                break;
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




    /* 개발자 테스트 영역 */

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
        Debug.Log("보유 달러를 리셋했습니다. 보유 달러 : " + GetDollar());

        PlayerPrefs.Save();
    }

    public void TestDollarChargeButton()
    {        
        PlayerPrefs.SetInt("Dollar", GetDollar() + 250000);
        Debug.Log("보유 달러를 충전했습니다. 보유 달러 : " + GetDollar());
        
        PlayerPrefs.Save();
    }

    public void TestItemResetButton()
    {
        PlayerPrefs.SetInt("LifeRecovery", 0);
        Debug.Log("LifeRecovery 를 리셋했습니다. 보유 : " + GetLifeRecovery());
        PlayerPrefs.SetInt("SummonDongles", 0);
        Debug.Log("SummonDongles 를 리셋했습니다. 보유 : " + GetSummonDongles());
        PlayerPrefs.SetInt("Unbreakable", 0);
        Debug.Log("Unbreakable 를 리셋했습니다. 보유 : " + GetUnbreakable());
        PlayerPrefs.SetInt("LevelUpAll", 0);
        Debug.Log("LevelUpAll 를 리셋했습니다. 보유 : " + GetLevelUpAll());

        PlayerPrefs.Save();
    }

    public void TestItemDeleteKey()
    {
        PlayerPrefs.DeleteKey("LifeRecovery");
        Debug.Log("LifeRecovery Key 를 삭제했습니다.");
        PlayerPrefs.DeleteKey("SummonDongles");
        Debug.Log("SummonDongles Key 를 삭제했습니다.");
        PlayerPrefs.DeleteKey("Unbreakable");
        Debug.Log("Unbreakable 를 Key 를 삭제했습니다.");
        PlayerPrefs.DeleteKey("LevelUpAll");
        Debug.Log("LevelUpAll 를 Key 를 삭제했습니다.");
    }


}