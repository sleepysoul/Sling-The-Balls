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
        level = 0;
        isDrag = false;
        isMerge = false;
        isAttach = false;

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.zero;

        GetComponent<SpringJoint2D>().enabled = true;
        rb.simulated = true;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;
        circle.enabled = true;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Dongle") {
            Dongle other = collision.gameObject.GetComponent<Dongle>();

            if (level == other.level && !isMerge && !other.isMerge && level < 7) {
                float meX = transform.position.x;
                float meY = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;

                if (meY < otherY || (meY == otherY && meX > otherX)) {
                    other.Hide(transform.position);
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
        yield return new WaitForSeconds(.5f);

        isAttach = false;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Dongle") {
            Dongle other = collision.gameObject.GetComponent<Dongle>();

            if (level == other.level && !isMerge && !other.isMerge && level < 7) {
                float meX = transform.position.x;
                float meY = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;

                if (meY < otherY || (meY == otherY && meX > otherX)) {
                    other.Hide(transform.position);
                    LevelUp();
                }
            }
        }
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
        yield return new WaitForSeconds(0.2f);

        anim.SetInteger("Level", level + 1);
        EffectPlay();
        manager.SfxPlay(GameManager.Sfx.LevelUp);

        yield return new WaitForSeconds(0.2f);
        level++;

        manager.maxLevel = Mathf.Max(level, manager.maxLevel);

        isMerge = false;
    }

    void Update()
    {
        if (isDrag) {
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
        isDrag = false;
        rb.isKinematic = false;
        StartCoroutine("Release");
    }

    IEnumerator Release()
    {
        yield return new WaitForSeconds(releaseTime);

        rb.bodyType = RigidbodyType2D.Dynamic;

        GetComponent<SpringJoint2D>().enabled = false;
        manager.lastDongle = null;

        isMerge = false;  
    }

    void EffectPlay()
    {
        effect.transform.position = transform.position;
        effect.transform.localScale = transform.localScale;
        effect.Play();
    }
}