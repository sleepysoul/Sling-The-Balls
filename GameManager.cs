using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("===========[ Core ]")]
    public bool isOver;
    public int maxLevel;
    public int score;
    public int health;

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

    [Header("===========[ Audio System ]")]
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip;
    public int sfxCursor;
    public enum Sfx { LevelUp , Next , GameOver , Attach , Button }

    [Header("===========[ UI ]")]
    public Text scoreText;
    public Text highScoreText;
    public Text subScoreText;
    public GameObject endGroup;

    [Header("===========[ ETC ]")]
    public GameObject line;


    void Awake()
    {
        Application.targetFrameRate = 60;
        bgmPlayer.Play();

        donglePool = new List<Dongle>();
        effectPool = new List<ParticleSystem>();
        for (int index=0; index < poolSize; index++) {
            MakeDongle();
        }
    }

    void Start()
    {
        NextDongle();
    }

    // 동글 가져오기
    void NextDongle()
    {
        Dongle newDongle = GetDongle();
        lastDongle = newDongle;
        lastDongle.isMerge = true;  // 동글 발사 대기중 머지 잠금
        lastDongle.level = Random.Range(0, maxLevel);
        lastDongle.gameObject.SetActive(true);

        SfxPlay(Sfx.Next);
        StartCoroutine("WaitNext");
    }

    IEnumerator WaitNext()
    {
        while (lastDongle != null) {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        NextDongle();
    }

    // 동글 생성
    Dongle GetDongle()
    {
        for (int index = 0;index < donglePool.Count;index++) {
            // poolCursor++; => donglePool.count 를 넘어가면 Out of indexing 오류 발생
            poolCursor = (poolCursor + 1) % donglePool.Count;
            if (!donglePool[poolCursor].gameObject.activeSelf) {
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
        
    }

    public void Reset()
    {
        SfxPlay(Sfx.Button);

        StartCoroutine(ResetRoutine());
    }

    IEnumerator ResetRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene("main");
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
            case Sfx.Button:
                sfxPlayer[sfxCursor].clip = sfxClip[6];
                break;
        } // end of switch
        
        sfxPlayer[sfxCursor].Play();
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length;
    }

    public void GameOver()
    {
        Debug.Log("Over!");
        if (isOver) {
            Debug.Log("isOver return!"); return;
        }

        isOver = true;
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        // 1. 장면 안에 활성화 되어 있는 모든 동글 가져오기
        Dongle[] dongles = GameObject.FindObjectsOfType<Dongle>();

        // 2. 지우기 전에 모든 동글의 물리효과 비활성화
        for (int index=0; index < dongles.Length; index++) {
            dongles[index].rb.simulated = false;
        }

        // 3. 1번 목록을 하나씩 접근하여 지우기
        for (int index=0; index < dongles.Length; index++) {
            dongles[index].Hide(Vector3.up * 100);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.5f);
        SfxPlay(Sfx.GameOver);

        // 현재 게임 최종 스코어 출력
        subScoreText.text = "SCORE : " + scoreText.text;
        // 최종 스코어와 저장된 최고 스코어 비교하여 저장
        int highScore = Mathf.Max(score, PlayerPrefs.GetInt("HighScore"));
        PlayerPrefs.SetInt("HighScore", highScore);
        // 최고 스코어 출력
        highScoreText.text = "HIGHSCORE : " + Mathf.Max(score, PlayerPrefs.GetInt("HighScore")).ToString();
        // 게임 오버 UI 출력
        endGroup.gameObject.SetActive(true);
    }

    void LateUpdate()
    {
        scoreText.text = score.ToString();
    }
}
