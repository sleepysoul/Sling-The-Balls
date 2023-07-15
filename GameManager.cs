using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int maxLevel;
    public int score;

    public Dongle lastDongle;
    public Rigidbody2D hookRb;

    public GameObject donglePrefab;
    public Transform dongleGroup;
    public GameObject effectPrefab;
    public Transform effectGroup;

    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip;
    public int sfxCursor;
    public enum Sfx { LevelUp , Next , GameOver , Attach , Button }

    void Awake()
    {
        Application.targetFrameRate = 60;
        bgmPlayer.Play();
    }

    void Start()
    {
        NextDongle();
    }

    // 동글 생성
    Dongle GetDongle()
    {
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();

        GameObject instantDongleObj = Instantiate(donglePrefab, dongleGroup);
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
        instantDongle.manager = this;
        instantDongle.hook = hookRb;        
        instantDongle.GetComponent<SpringJoint2D>().connectedBody = hookRb;
        instantDongle.effect = instantEffect;
        return instantDongle;
    }

    // 동글 가져오기
    void NextDongle()
    {
        Dongle newDongle = GetDongle();
        lastDongle = newDongle;
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

    void TouchDown()
    {
        if (lastDongle == null) {
            return;
        }

        lastDongle.OnMouseDown();
    }

    void TouchUp()
    {
        if (lastDongle == null) {
            return;
        }

        lastDongle.OnMouseUp();
        lastDongle = null;
    }

    public void Reset()
    {
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
}
