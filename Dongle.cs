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

    public int level;
    public float releaseTime = .1f;
    public float maxDragDistance = 0.01f;

    public bool isDrag;
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
        // 동글 속성 초기화
        level = 0;
        isDrag = false;
        isMerge = false;
        isAttach = false;
        // 동글 트랜스폼 초기화
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.zero;
        // 동글 물리 초기화
        GetComponent<SpringJoint2D>().enabled = true;
        rb.simulated = true;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;
        circle.enabled = true;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 충돌 감지하면 상대 동글 클래스 가져오기
        if (collision.gameObject.tag == "Dongle") {
            Dongle other = collision.gameObject.GetComponent<Dongle>();

            // 상대 동글 흡수 로직
            if (level == other.level && !isMerge && !other.isMerge && level < 7) {
                // 내 동글, 상대 동글 위치 가져오기
                float meX = transform.position.x;
                float meY = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;

                // 위치 조건 설정하기
                if (meY < otherY || (meY == otherY && meX > otherX)) {
                    // 1. 상대 동글 숨기기
                    other.Hide(transform.position);
                    // 2. 내 동글 레벨업
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
        yield return new WaitForSeconds(1f);
        
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
        // 상대 동글 숨기기 시간 대기
        yield return new WaitForSeconds(0.2f);

        // 레벨업 애니메이션 구동
        anim.SetInteger("Level", level + 1);
        EffectPlay();
        manager.SfxPlay(GameManager.Sfx.LevelUp);

        // 레벨업 애니메이션 구동 시간 대기
        yield return new WaitForSeconds(0.2f);
        level++;

        manager.maxLevel = Mathf.Max(level, manager.maxLevel);

        isMerge = false;
    }

    void Update()
    {
        if (isDrag) 
        {           
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (Vector3.Distance(mousePos, hook.position) > maxDragDistance) 
                rb.position = hook.position + (mousePos - hook.position).normalized * maxDragDistance;
            else 
                rb.position = mousePos;
        }

        
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

    public void Drag()
    {
        if (isDrag) {
            return;
        }

        isDrag = true;
        rb.isKinematic = true;
    }

    public void Drop()
    {        
        rb.isKinematic = false;
        StartCoroutine("Release");
    }

    IEnumerator Release()
    {
        yield return new WaitForSeconds(releaseTime);

        isDrag = false;
        rb.bodyType = RigidbodyType2D.Dynamic;

        GetComponent<SpringJoint2D>().enabled = false;
        manager.lastDongle = null;

        isMerge = false;  // 동글 발사 후 머지 잠금 해제        
    }

    void EffectPlay()
    {
        effect.transform.position = transform.position;
        effect.transform.localScale = transform.localScale;
        effect.Play();
    }
}
