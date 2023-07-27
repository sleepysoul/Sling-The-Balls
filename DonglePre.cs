using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DonglePre : MonoBehaviour
{
    public GameManager manager;
    public ParticleSystem effect;

    public Rigidbody2D rb; 
    public CircleCollider2D circle;
    public Animator anim;

    public int level;
    public bool isMerge;
    public bool isAttach;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        circle = GetComponent<CircleCollider2D>();
        anim = GetComponent<Animator>();
    }

    void OnEnable()
    {
        anim.SetInteger("Level", level);
    }

    void OnDisable()
    {
        // ���� �Ӽ� �ʱ�ȭ
        level = 0;
        isMerge = false;
        isAttach = false;
        // ���� Ʈ������ �ʱ�ȭ
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.zero;
        // ���� ���� �ʱ�ȭ
        GetComponent<SpringJoint2D>().enabled = true;
        rb.simulated = true;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;
        circle.enabled = true;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // �浹 �����ϸ� ��� ���� Ŭ���� ��������
        if (collision.gameObject.tag == "Dongle") {
            Dongle other = collision.gameObject.GetComponent<Dongle>();

            // ��� ���� ��� ����
            if (level == other.level && !isMerge && !other.isMerge && level < 7) {
                // �� ����, ��� ���� ��ġ ��������
                float meX = transform.position.x;
                float meY = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;

                // ��ġ ���� �����ϱ�
                if (meY < otherY || (meY == otherY && meX > otherX)) {
                    // 1. ��� ���� �����
                    other.Hide(transform.position);
                    // 2. �� ���� ������
                    LevelUp();
                }
            }            
        }

        if (isAttach) {
            return;
        }

        isAttach = true;
        manager.SfxPlay(GameManager.Sfx.Attach);

        StartCoroutine(AttachRoutine());
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        // �浹 �����ϸ� ��� ���� Ŭ���� ��������
        if (collision.gameObject.tag == "Dongle") {
            Dongle other = collision.gameObject.GetComponent<Dongle>();

            // ��� ���� ��� ����
            if (level == other.level && !isMerge && !other.isMerge && level < 7) {
                // �� ����, ��� ���� ��ġ ��������
                float meX = transform.position.x;
                float meY = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;

                // ��ġ ���� �����ϱ�
                if (meY < otherY || (meY == otherY && meX > otherX)) {
                    // 1. ��� ���� �����
                    other.Hide(transform.position);
                    // 2. �� ���� ������
                    LevelUp();
                }
            }
        }
    }

    IEnumerator AttachRoutine()
    {
        yield return new WaitForSeconds(.5f);
        
        isAttach = false;
    }

    public void Hide(Vector3 targetPos)
    {
        isMerge = true;

        rb.simulated = false;
        circle.enabled = false;

        StartCoroutine(HideRoutine(targetPos));
    }

    IEnumerator HideRoutine(Vector3 targetPos)
    {
        int frameCount = 0;
        while (frameCount < 20) {
            frameCount++;

            if (targetPos != Vector3.up * 100) {
                transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);

            }
            if (targetPos == Vector3.up * 100) {
                EffectPlay();
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.5f);
            }

            yield return null;
        }
        
        manager.score += (int)(Mathf.Pow(2, level)) * 100;

        isMerge = false;
        this.gameObject.SetActive(false);
    }

    void LevelUp()
    {
        isMerge = true;

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;

        StartCoroutine(LevelUpRoutine());
    }

    IEnumerator LevelUpRoutine()
    {
        // ��� ���� ����� �ð� ���
        yield return new WaitForSeconds(0.2f);

        // ������ �ִϸ��̼� ����
        anim.SetInteger("Level", level + 1);
        EffectPlay();
        manager.SfxPlay(GameManager.Sfx.LevelUp);

        // ������ �ִϸ��̼� ���� �ð� ���
        yield return new WaitForSeconds(0.2f);
        level++;

        manager.maxLevel = Mathf.Max(level, manager.maxLevel);

        isMerge = false;
    }


    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Finish") {

            manager.life -= 1;

            if (manager.life == 0) {
                manager.GameOver();
            }
        }

        this.gameObject.SetActive(false);
    }

    void EffectPlay()
    {
        effect.transform.position = transform.position;
        effect.transform.localScale = transform.localScale;
        effect.Play();
    }
}
