using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceCube : MonoBehaviour
{
    Rigidbody2D rigid;

    public GameObject iceCube;
    public SpriteRenderer iceColor;
    public ParticleSystem iceEffect;
    public AudioSource iceCubeSfxPlayer;
    

    public int iceCubeCount;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Move(0);
    }

    void Move(float velocity)
    {
        if (velocity == 0)
        {
            rigid.AddForce(Vector2.right * 32000 * Time.deltaTime, ForceMode2D.Impulse);
            Debug.Log("velocity 0 ! Let's Move. Speed : " + Vector2.right * 16000 * Time.deltaTime);
        }
        else
        {
            rigid.AddForce(rigid.velocity * 32000 * Time.deltaTime, ForceMode2D.Impulse);
        }
    }

    private void Update()
    {
        if (Mathf.Abs(rigid.velocity.x) < 1f)
        {
            Move(rigid.velocity.x);
        }

        iceCubeCheck();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Dongle")
        {
            // �ε��� ������Ʈ�� ������ ����
            Rigidbody2D otherRigidbody = collision.gameObject.GetComponent<Rigidbody2D>();

            if (otherRigidbody != null)
            {
                // �ε��� ������Ʈ�� �ӵ��� ����
                Vector2 collisionVelocity = collision.relativeVelocity;

                Debug.Log("collisionVelocity : " + collisionVelocity);
                if (Mathf.Abs(collisionVelocity.x) > 20f)
                {
                    iceCubeCount--;
                }

                // �ε��� ������Ʈ�� ���� ���� ���� ����
                // otherRigidbody.AddForce(new Vector2(10f, 0f), ForceMode2D.Impulse);
            }
        }
    }

    public void iceCubeCheck()
    {
        if (iceCubeCount == 3)
        {
            iceColor.color = Color.blue;
        }
        else if (iceCubeCount == 2)
        {
            iceColor.color = Color.yellow;
        }
        else if (iceCubeCount == 1)
        {
            iceColor.color = Color.red;
        }
        else if (iceCubeCount == 0)
        {
            if (iceCubeSfxPlayer.isPlaying == false)
            {
                iceCubeSfxPlayer.Play();
            }
            iceEffect.gameObject.SetActive(true);
            StartCoroutine("Bomb");
        }
    }

    IEnumerator Bomb()
    {
        yield return new WaitForSeconds(0.5f);
        iceCube.gameObject.SetActive(false);
    }
}
