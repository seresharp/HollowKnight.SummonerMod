using System;
using System.Collections.Generic;
using SeanprCore;
using UnityEngine;

using Random = System.Random;

namespace Summoner
{
    public class ShadeOrb : MonoBehaviour
    {
        private const float AGGRO_RANGE = 7.5f;
        private const float DRIFT_RANGE = 5f;

        private const int LINE_COUNT = 20;
        private const float TIMER_MAX = 0.02f;
        private static Random rnd = new Random();

        private HealthManager target;
        private Vector2 velocity;
        private Vector2 goal;

        private float lineTimer;
        private Queue<Vector3> linePositions = new Queue<Vector3>();
        private LineRenderer line;

        private float MaxSpeed
        {
            get
            {
                if (Ref.Hero == null)
                {
                    return 5f;
                }

                float distHeroSelf = Vector2.Distance(transform.position, Ref.Hero.transform.position);

                return Math.Max(distHeroSelf * 2f, 5f);
            }
        }

        private float Acceleration => MaxSpeed * 0.75f;

        private void Awake()
        {
            line = gameObject.AddComponent<LineRenderer>();
            line.positionCount = LINE_COUNT;
            line.startWidth = .02f;
            line.endWidth = .15f;
            line.sharedMaterial = new Material(Shader.Find("Particles/Additive"));
            line.sharedMaterial.renderQueue = 4000;
            line.startColor = Color.white;
            line.endColor = Color.black;

            for (int i = 0; i < LINE_COUNT; i++)
            {
                linePositions.Enqueue(transform.position);
            }

            line.SetPositions(linePositions.ToArray());
        }

        private void Update()
        {
            if (Ref.Hero == null)
            {
                return;
            }

            lineTimer -= Time.deltaTime;

            if (lineTimer <= 0)
            {
                lineTimer = TIMER_MAX;
                linePositions.Enqueue(transform.position);
                linePositions.Dequeue();

                line.SetPositions(linePositions.ToArray());
            }

            if (target == null)
            {
                target = Enemies.GetClosest(Summoner.CheckEnemyValid, AGGRO_RANGE);
            }

            if (!Summoner.CheckEnemyValid(target))
            {
                target = null;
            }

            if (target != null)
            {
                float dist = Vector2.Distance(Ref.Hero.transform.position, target.transform.position);
                if (target.isDead || target.hp <= 0 || dist > AGGRO_RANGE)
                {
                    target = null;
                }
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, target.GetComponent<Collider2D>().bounds.center, 20f * Time.deltaTime);
                    return;
                }
            }

            if (Vector2.Distance(Ref.Hero.transform.position, goal) > DRIFT_RANGE || Vector2.Distance(transform.position, goal) < 0.25f)
            {
                float r = DRIFT_RANGE / 1.5f * (float)Math.Sqrt(rnd.NextDouble());
                float theta = (float)(rnd.NextDouble() * 2 * Math.PI);

                goal.x = Ref.Hero.transform.position.x + r * (float)Math.Cos(theta);
                goal.y = Ref.Hero.transform.position.y + r * (float)Math.Sin(theta);
            }

            if (transform.position.x < goal.x && velocity.x < MaxSpeed)
            {
                if (velocity.x > 1f && Math.Abs(transform.position.x - goal.x) < 0.5f)
                {
                    velocity.x -= Time.deltaTime * Acceleration * 2;
                }
                else
                {
                    velocity.x += Time.deltaTime * Acceleration;
                }
            }
            else if (transform.position.x > goal.x && velocity.x > -MaxSpeed)
            {
                if (velocity.x < -1f && Math.Abs(transform.position.x - goal.x) < 0.5f)
                {
                    velocity.x += Time.deltaTime * Acceleration * 2;
                }
                else
                {
                    velocity.x -= Time.deltaTime * Acceleration;
                }
            }

            if (transform.position.y < goal.y && velocity.y < MaxSpeed)
            {
                if (velocity.y > 1f && Math.Abs(transform.position.y - goal.y) < 0.5f)
                {
                    velocity.y -= Time.deltaTime * Acceleration * 2;
                }
                else
                {
                    velocity.y += Time.deltaTime * Acceleration;
                }
            }
            else if (transform.position.y > goal.y && velocity.y > -MaxSpeed)
            {
                if (velocity.y < -1f && Math.Abs(transform.position.y - goal.y) < 0.5f)
                {
                    velocity.y += Time.deltaTime * Acceleration * 2;
                }
                else
                {
                    velocity.y -= Time.deltaTime * Acceleration;
                }
            }

            velocity.x = Mathf.Clamp(velocity.x, -MaxSpeed, MaxSpeed);
            velocity.y = Mathf.Clamp(velocity.y, -MaxSpeed, MaxSpeed);

            transform.position += (Vector3)velocity * Time.deltaTime;

            if (Vector2.Distance(transform.position, Ref.Hero.transform.position) > 25f)
            {
                transform.position = Ref.Hero.transform.position;
            }
        }
    }
}
