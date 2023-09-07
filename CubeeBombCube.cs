using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeeBombCube : MonoBehaviour
{
    public GameObject bombCube;
    public SpriteRenderer bombColor;
    public ParticleSystem bombEffect;        
    public int bombCubeCount;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // �ε��� ������Ʈ�� ������ ����
        Rigidbody2D otherRigidbody = collision.gameObject.GetComponent<Rigidbody2D>();

        if (otherRigidbody != null)
        {
            // �ε��� ������Ʈ�� �ӵ��� ����
            Vector2 collisionVelocity = collision.relativeVelocity;

            Debug.Log("collisionVelocity : " + collisionVelocity);
            if (Mathf.Abs(collisionVelocity.x) > 10f)
            {
                bombCubeCount--;
            }

            // �ε��� ������Ʈ�� ���� ���� ���� ����
            // otherRigidbody.AddForce(new Vector2(10f, 0f), ForceMode2D.Impulse);
        }
    }

    private void Update()
    {
        BombCubeCheck();
    }

    public void BombCubeCheck()
    {
        if (bombCubeCount == 3)
        {
            bombColor.color = Color.blue;
        }
        else if (bombCubeCount == 2)
        {
            bombColor.color = Color.yellow;
        }
        else if (bombCubeCount == 1)
        {
            bombColor.color = Color.red;
        }
        else if (bombCubeCount == 0)
        {
            bombEffect.gameObject.SetActive(true);
            StartCoroutine("Bomb");
        }
    }

    IEnumerator Bomb()
    {
        yield return new WaitForSeconds(1f);
        bombCube.gameObject.SetActive(false);
    }
}
