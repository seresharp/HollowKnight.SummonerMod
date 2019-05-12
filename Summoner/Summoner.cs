using System;
using System.IO;
using System.Reflection;
using GlobalEnums;
using HutongGames.PlayMaker.Actions;
using Modding;
using SeanprCore;
using UnityEngine;

namespace Summoner
{
    public class Summoner : Mod
    {
        public static Summoner Instance { get; private set; }

        private GameObject shadeOrb;
        private Sprite turretSprite;

        public override void Initialize()
        {
            Instance = this;

            Assembly randoDLL = GetType().Assembly;
            foreach (string res in randoDLL.GetManifestResourceNames())
            {
                if (res.EndsWith("Turret.png"))
                {
                    // Read bytes of image
                    Stream imageStream = randoDLL.GetManifestResourceStream(res);
                    byte[] buffer = new byte[imageStream.Length];
                    imageStream.Read(buffer, 0, buffer.Length);
                    imageStream.Dispose();

                    // Create texture from bytes
                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(buffer, true);

                    // Create sprite from texture
                    turretSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }
            }

            ModHooks.Instance.AttackHook += KillDamage;
            ModHooks.Instance.BeforeSavegameSaveHook += RestoreDamage;
            ModHooks.Instance.HeroUpdateHook += SpawnShadeOrb;
            On.HutongGames.PlayMaker.Actions.SpawnObjectFromGlobalPool.OnEnter += SpawnTurret;

            Enemies.BeginTracking();
        }

        public override string GetVersion() => "0.0.1";

        private void KillDamage(AttackDirection dir)
        {
            PlayerData.instance.nailDamage = 1;
            PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
        }

        private void RestoreDamage(SaveGameData data)
        {
            PlayerData.instance.nailDamage = 5 + 4 * PlayerData.instance.nailSmithUpgrades;
        }

        private void SpawnTurret(On.HutongGames.PlayMaker.Actions.SpawnObjectFromGlobalPool.orig_OnEnter orig, SpawnObjectFromGlobalPool self)
        {
            if (self.Fsm.FsmComponent.FsmName == "Spell Control" && self.State.Name == "Fireball 1")
            {
                GameObject turret = new GameObject();
                turret.SetActive(false);

                turret.AddComponent<Turret>();
                turret.AddComponent<NonBouncer>().active = true;

                turret.transform.position = Ref.Hero.transform.position;
                BoxCollider2D col = turret.AddComponent<BoxCollider2D>();
                SpriteRenderer render = turret.AddComponent<SpriteRenderer>();
                render.sprite = turretSprite;
                col.size = turretSprite.bounds.size;

                turret.transform.localScale = new Vector3(Ref.Hero.cState.facingRight ? -4f : 4f, 4f, 4f);

                // Doesn't make a whole lot of sense intuitively but this layer has the proper collision
                turret.layer = (int)PhysLayers.PROJECTILES;

                turret.SetActive(true);

                self.Finish();
                return;
            }

            orig(self);
        }

        private void SpawnShadeOrb()
        {
            if (shadeOrb == null && Ref.Hero.transitionState == HeroTransitionState.WAITING_TO_TRANSITION)
            {
                shadeOrb = new GameObject();
                shadeOrb.SetActive(false);

                shadeOrb.AddComponent<ShadeOrb>();
                shadeOrb.AddComponent<NonBouncer>().active = true;

                EnemyDamager c = shadeOrb.AddComponent<EnemyDamager>();
                c.damage = 3;
                c.cooldown = 0.5f;
                c.type = AttackTypes.Spell;
                c.bypassIframes = true;
                c.bypassStun = true;

                // Required for collision events for some reason
                Rigidbody2D rb2d = shadeOrb.AddComponent<Rigidbody2D>();
                rb2d.isKinematic = true;

                CircleCollider2D col = shadeOrb.AddComponent<CircleCollider2D>();
                col.radius = 0.5f;
                col.isTrigger = true;

                GameObject particles = ObjectCache.ShadeParticles;
                particles.SetActive(true);
                particles.transform.SetParent(shadeOrb.transform);

                shadeOrb.layer = (int)PhysLayers.HERO_ATTACK;
                shadeOrb.name = "Shade Orb";
                shadeOrb.tag = "Hero Spell";
                shadeOrb.transform.position = Ref.Hero.transform.position;
                shadeOrb.SetActive(true);
            }
        }

        public static bool CheckEnemyValid(HealthManager hm)
        {
            if (hm == null || hm.isDead || hm.hp <= 0
                || (hm.IsInvincible && hm.InvincibleFromDirection == 0 && !hm.name.StartsWith("Moss Charger"))
                || (hm.name.StartsWith("Plant Trap") && hm.LocateFSM("Plant Trap Control").ActiveStateName != "Snap"))
            {
                return false;
            }

            return true;
        }
    }
}
