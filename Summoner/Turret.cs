using System;
using GlobalEnums;
using SeanprCore;
using UnityEngine;

namespace Summoner
{
    public class Turret : MonoBehaviour
    {
        public float cooldownMax = 1.9f;
        public float lifetime = 4f;

        private bool falling = true;
        private Rigidbody2D rb2d;
        private AudioSource audio;
        private SpriteRenderer renderer;
        private float cooldown;

        private void Awake()
        {
            renderer = GetComponent<SpriteRenderer>();

            audio = gameObject.AddComponent<AudioSource>();
            audio.clip = ObjectCache.FireballSound;

            rb2d = gameObject.AddComponent<Rigidbody2D>();
            rb2d.velocity = (Ref.Hero.cState.facingRight ? Vector2.right * 10 : Vector2.left * 10) + new Vector2(0f, 0.1f);
            rb2d.freezeRotation = true;
        }

        private void Update()
        {
            if (falling)
            {
                if (rb2d.velocity.y < -20f)
                {
                    rb2d.velocity = new Vector2(rb2d.velocity.x, -20f);
                }
                else if (rb2d.velocity.y == 0f)
                {
                    falling = false;
                }

                return;
            }

            cooldown -= Time.deltaTime;
            lifetime -= Time.deltaTime;

            if (lifetime <= 0f)
            {
                if (renderer.color.a > 0f)
                {
                    renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, renderer.color.a - Time.deltaTime);
                }
                else
                {
                    Destroy(gameObject);
                }

                return;
            }

            if (cooldown <= 0)
            {
                HealthManager closest = Enemies.GetClosest(RaycastEnemy, 15f, transform.position);
                if (closest == null)
                {
                    return;
                }

                // Reset cooldown
                cooldown = cooldownMax;

                // Spawn fireball
                GameObject fireball = ObjectCache.Fireball;
                fireball.transform.position = transform.position + Vector3.up * 1.5f;

                fireball.GetComponent<Collider2D>().isTrigger = true;
                fireball.AddComponent<NonBouncer>().active = true;
                fireball.AddComponent<Fireball>().target = closest.GetComponent<Collider2D>().bounds.center;
                fireball.SetActive(true);

                // Play sound
                audio.Play();
            }
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (col.gameObject.layer == (int)PhysLayers.TERRAIN)
            {
                rb2d.constraints |= RigidbodyConstraints2D.FreezePositionX;
            }
        }

        private bool RaycastEnemy(HealthManager hm)
        {
            if (!Summoner.CheckEnemyValid(hm))
            {
                return false;
            }

            // Cursed
            if (hm.tag == "Hatchling Magnet")
            {
                return true;
            }

            RaycastHit2D raycast = Physics2D.Raycast(
                transform.position + Vector3.up * 1.5f,
                hm.transform.position - transform.position,
                Vector2.Distance(transform.position, hm.transform.position),
                1 << (int)PhysLayers.TERRAIN);

            return raycast.collider == null || raycast.collider.gameObject.GetComponentInSelfChildOrParent<HealthManager>();
        }
    }
}
