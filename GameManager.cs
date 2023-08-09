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
    public int minLevel;
    public int maxLevel;
    public int score;
    public int life;
    public float playTime;

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
    public Text scoreText;
    public Text lifeText;
    public Text highScoreText;
    public Text subScoreText;
    public Text clearHighScoreText;
    public Text clearSubScoreText;
    public Text playTimeText;
    public Image caution;    
    public GameObject endGroup;
    public GameObject stageClearGroup;
    public GameObject touchPad;

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

        // 현재 게임 최종 스코어 출력
        int playTimeToScore;
        if (playTime < 60f) {
            playTimeToScore = 1;
        }
        else {
            playTimeToScore = (int)(playTime / 60f);
        }

        // 최종 스코어와 저장된 최고 스코어 비교하여 저장
        int stageClearScore = score * life * playTimeToScore;
        clearSubScoreText.text = "SCORE : " + stageClearScore.ToString();

        // 최고 스코어 출력
        int highScore = Mathf.Max(stageClearScore, PlayerPrefs.GetInt("HighScore"));
        PlayerPrefs.SetInt("HighScore", highScore);
        clearHighScoreText.text = "HIGHSCORE : " + highScore.ToString();

        // 게임 오버 UI 출력
        stageClearGroup.gameObject.SetActive(true);

        // BGM 종료
        bgmPlayer.Stop();

    }

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
            SceneManager.LoadScene(0);
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

        subScoreText.text = scoreText.text;
        int highScore = Mathf.Max(score, PlayerPrefs.GetInt("HighScore"));
        PlayerPrefs.SetInt("HighScore", highScore);
        highScoreText.text = "HIGHSCORE : " + highScore.ToString();

        endGroup.gameObject.SetActive(true);
    }

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

        if (sceneIndex > SceneManager.sceneCount) {
            SceneManager.LoadScene(0);
        }

        SceneManager.LoadScene(sceneIndex + 1);
    }

    public void SfxPlay(Sfx type)
    {
        switch (type) {
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

    void Update()
    {
        PlayTimeCheck();
    }

    public void PlayTimeCheck()
    {
        if (isOver || isClear) {
            return;
        }

        int min = (int)playTime / 60;
        float sec = playTime % 60;
        playTime -= Time.deltaTime;

        if (playTime >= 60f) {

            playTimeText.text = "0" + min + " : " + (int)sec;
            if ((int)sec < 10) {
                playTimeText.text = "0" + min + " : " + "0" + (int)sec;
            }
        }

        if (playTime < 60f) {
            playTimeText.text = "00 : " + (int)playTime;
            if ((int)sec < 10) {
                playTimeText.text = "0" + min + " : " + "0" + (int)sec;
            }
        }

        if (playTime <= 0) {
            playTimeText.text = "00 : 00";
        }
    }

    void LateUpdate()
    {
        scoreText.text = "SCORE : " + score.ToString();
        lifeText.text = "X " + life.ToString();

        if (maxLevel == 6) {
            if (isClear) {
                return;
            }

            isClear = true;
            StageClear();
        }
    }

    private void OnApplicationQuit()
    {
        // 게임 종료 시, 진행중이던 현재 스테이지 PlayerPrefs 저장
        Debug.Log("종료 감지. 현재 스테이지 [ " + SceneManager.GetActiveScene().name + " ] 정보를 저장합니다.");
        PlayerPrefs.SetString("Stage", SceneManager.GetActiveScene().name);
    }
}