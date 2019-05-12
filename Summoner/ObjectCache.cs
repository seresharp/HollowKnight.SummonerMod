using System;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using SeanprCore;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Summoner
{
    public static class ObjectCache
    {
        private static AudioClip _fireballSound;
        public static AudioClip FireballSound
        {
            get
            {
                if (_fireballSound != null)
                {
                    return Object.Instantiate(_fireballSound);
                }

                try
                {
                    FsmState fireVengefulSpirit = Ref.Hero.LocateFSM("Spell Control").GetState("Fireball 1");
                    GameObject vengefulSpiritContainer = fireVengefulSpirit.GetActionOfType<SpawnObjectFromGlobalPool>().gameObject.Value;
                    FsmState castState = vengefulSpiritContainer.LocateFSM("Fireball Cast").GetState("Cast Right");
                    AudioClip noise = (AudioClip)castState.GetActionOfType<AudioPlayerOneShotSingle>().audioClip.Value;

                    _fireballSound = noise;

                    Object.DontDestroyOnLoad(_fireballSound);
                    return Object.Instantiate(_fireballSound);
                }
                catch (Exception e)
                {
                    Summoner.Instance.LogError("Could not get fireball noise:\n" + e);
                    return null;
                }
            }
        }

        private static GameObject _fireball;
        public static GameObject Fireball
        {
            get
            {
                if (_fireball != null)
                {
                    return Object.Instantiate(_fireball);
                }

                try
                {
                    FsmState fireVengefulSpirit = Ref.Hero.LocateFSM("Spell Control").GetState("Fireball 1");
                    GameObject vengefulSpiritContainer = fireVengefulSpirit.GetActionOfType<SpawnObjectFromGlobalPool>().gameObject.Value;
                    FsmState castState = vengefulSpiritContainer.LocateFSM("Fireball Cast").GetState("Cast Right");
                    GameObject fireball = castState.GetActionOfType<SpawnObjectFromGlobalPool>().gameObject.Value;

                    Object.Destroy(fireball.LocateFSM("Fireball Control"));
                    Object.Destroy(fireball.LocateFSM("damages_enemy"));

                    _fireball = Object.Instantiate(fireball);
                    _fireball.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    _fireball.SetActive(false);

                    return Object.Instantiate(_fireball);
                }
                catch (Exception e)
                {
                    Summoner.Instance.LogError("Could not get fireball:\n" + e);
                    return null;
                }
            }
        }

        private static GameObject _shadeParticles;
        public static GameObject ShadeParticles
        {
            get
            {
                if (_shadeParticles != null)
                {
                    return Object.Instantiate(_shadeParticles);
                }

                try
                {
                    // Thank you Team Cherry for burying this particle system under 20 layers of playmaker
                    FsmState fireShadeSoul = Ref.Hero.LocateFSM("Spell Control").GetState("Fireball 2");
                    GameObject shadeSoulContainer = fireShadeSoul.GetActionOfType<SpawnObjectFromGlobalPool>().gameObject.Value;
                    FsmState castState = shadeSoulContainer.LocateFSM("Fireball Cast").GetState("Cast Right");
                    GameObject shadeSoul = castState.GetActionOfType<SpawnObjectFromGlobalPool>().gameObject.Value;

                    foreach (Transform t in shadeSoul.transform)
                    {
                        if (t.name == "Particle System B")
                        {
                            _shadeParticles = Object.Instantiate(t.gameObject);
                            _shadeParticles.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

                            _shadeParticles.GetComponent<ParticleSystemRenderer>().material.renderQueue = 4000;

                            ParticleSystem.MainModule settings = _shadeParticles.GetComponent<ParticleSystem>().main;
                            settings.startLifetime = 0.5f;

                            Object.DontDestroyOnLoad(_shadeParticles);
                            _shadeParticles.SetActive(false);

                            break;
                        }
                    }

                    return Object.Instantiate(_shadeParticles);
                }
                catch (Exception e)
                {
                    Summoner.Instance.LogError("Could not get shade particles:\n" + e);
                    return null;
                }
            }
        }
    }
}
