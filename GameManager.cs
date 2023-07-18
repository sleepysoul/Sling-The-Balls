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

    // ���� ��������
    void NextDongle()
    {
        Dongle newDongle = GetDongle();
        lastDongle = newDongle;
        lastDongle.isMerge = true;  // ���� �߻� ����� ���� ���
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

    // ���� ����
    Dongle GetDongle()
    {
        for (int index = 0;index < donglePool.Count;index++) {
            // poolCursor++; => donglePool.count �� �Ѿ�� Out of indexing ���� �߻�
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
        // 1. ��� �ȿ� Ȱ��ȭ �Ǿ� �ִ� ��� ���� ��������
        Dongle[] dongles = GameObject.FindObjectsOfType<Dongle>();

        // 2. ����� ���� ��� ������ ����ȿ�� ��Ȱ��ȭ
        for (int index=0; index < dongles.Length; index++) {
            dongles[index].rb.simulated = false;
        }

        // 3. 1�� ����� �ϳ��� �����Ͽ� �����
        for (int index=0; index < dongles.Length; index++) {
            dongles[index].Hide(Vector3.up * 100);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.5f);
        SfxPlay(Sfx.GameOver);

        // ���� ���� ���� ���ھ� ���
        subScoreText.text = "SCORE : " + scoreText.text;
        // ���� ���ھ�� ����� �ְ� ���ھ� ���Ͽ� ����
        int highScore = Mathf.Max(score, PlayerPrefs.GetInt("HighScore"));
        PlayerPrefs.SetInt("HighScore", highScore);
        // �ְ� ���ھ� ���
        highScoreText.text = "HIGHSCORE : " + Mathf.Max(score, PlayerPrefs.GetInt("HighScore")).ToString();
        // ���� ���� UI ���
        endGroup.gameObject.SetActive(true);
    }

    void LateUpdate()
    {
        scoreText.text = score.ToString();
    }
}
