using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Dongle : MonoBehaviour
{
    public GameManager manager;
    public ParticleSystem effect;

    public Rigidbody2D rb; 
    public Rigidbody2D hook;
    public CircleCollider2D circle;
    public Animator anim;

    public float releaseTime = .15f;
    public float maxDragDistance = 4f;
    public int level;

    public bool isPressed = false;
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
        isPressed = false;
        isMerge = false;
        isAttach = false;
        // ���� Ʈ������ �ʱ�ȭ
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.zero;
        // ���� ���� �ʱ�ȭ
        rb.simulated = false;
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

    IEnumerator AttachRoutine()
    {
        yield return new WaitForSeconds(0.2f);
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
            transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
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

    void Update()
    {
        if (isPressed) 
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (Vector3.Distance(mousePos, hook.position) > maxDragDistance) 
                rb.position = hook.position + (mousePos - hook.position).normalized * maxDragDistance;
            else 
                rb.position = mousePos;            
        }
    }

    public void OnMouseDown()
    {
        isPressed = true;
        rb.isKinematic = true;
    }

    public void OnMouseUp()
    {
        isPressed = false;
        rb.isKinematic = false;
        StartCoroutine("Release");
    }

    IEnumerator Release()
    {
        yield return new WaitForSeconds(releaseTime);

        GetComponent<SpringJoint2D>().enabled = false;
        manager.lastDongle = null;        
    }

    void EffectPlay()
    {
        effect.transform.position = transform.position;
        effect.transform.localScale = transform.localScale;
        effect.Play();
    }

}
