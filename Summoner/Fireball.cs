using System;
using System.Collections;
using GlobalEnums;
using SeanprCore;
using UnityEngine;

namespace Summoner
{
    public class Fireball : MonoBehaviour
    {
        public Vector3 target;

        private EnemyDamager damager;
        private Rigidbody2D rb2d;
        private float collisionCooldown = 0.075f;

        private void Awake()
        {
            // Setup damage
            damager = gameObject.AddComponent<EnemyDamager>();
            damager.damage = 6;
            damager.bypassIframes = true;
            damager.cooldown = 1f / 3f;
            damager.type = AttackTypes.Spell;

            // Shaman stone
            if (PlayerData.instance.GetBool(nameof(PlayerData.equippedCharm_19)))
            {
                damager.damage = (int)(damager.damage * 4f / 3f);
                transform.localScale *= 1.25f;
            }

            // Rotation
            Vector3 angle = Vector3.RotateTowards(Vector3.one, target - transform.position, float.PositiveInfinity, float.PositiveInfinity);
            angle.z = 0;
            angle = angle.normalized * 25f;

            transform.rotation = Quaternion.LookRotation(angle);
            transform.rotation = new Quaternion(0, 0, transform.rotation.z, transform.rotation.w);

            if (angle.x < 0)
            {
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }

            // Velocity
            rb2d = GetComponent<Rigidbody2D>();
            rb2d.isKinematic = true;
            rb2d.velocity = angle;
        }

        private void Update()
        {
            if (collisionCooldown > 0)
            {
                collisionCooldown -= Time.deltaTime;
            }
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (collisionCooldown > 0f)
            {
                return;
            }

            PhysLayers layer = (PhysLayers)col.gameObject.layer;
            if (layer == PhysLayers.TERRAIN && (col.transform.parent == null || !col.transform.parent.name.StartsWith("Blocker")))
            {
                foreach (Transform child in transform)
                {
                    if (child.name == "Wall Impact")
                    {
                        child.gameObject.SetActive(true);
                        break;
                    }
                }

                StartCoroutine(DestroySelf());
            }
        }

        private IEnumerator DestroySelf()
        {
            Destroy(damager);
            rb2d.velocity = Vector2.zero;
            GetComponent<MeshRenderer>().enabled = false;
            yield return new WaitForSeconds(0.5f);
            Destroy(gameObject);
        }
    }
}
