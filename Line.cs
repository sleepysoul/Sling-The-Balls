using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    public ParticleSystem fire;
    public AudioSource sfxPlayer_Die;
    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        fire.gameObject.SetActive(true);
        sfxPlayer_Die.Play();

        if (collision.tag == "Dongle") {
            this.GetComponent<ParticleSystem>().Play();
        }
    }
}
