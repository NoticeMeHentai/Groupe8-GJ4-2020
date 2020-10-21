using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float m_TimeBeforeDestroy = 5f;

    private float speed ;
    private float currentTime = 0;
    private float damageToDeal = 0;
    private Vector3 startingPoint;
    private Enemy targetEnemy;
    public void Initialize(float damage, float speed)
    {
        damageToDeal = damage;
        this.speed = speed;
    }

    public void Initialize(float damage, float speed, Enemy enemy)
    {
        damageToDeal = damage;
        this.speed = speed;
        targetEnemy = enemy;
    }

    private void FixedUpdate()
    {
        currentTime += Time.fixedDeltaTime;
        Vector3 dir = transform.forward;
        if (targetEnemy != null) dir = ((targetEnemy.Position + Vector3.up) - transform.position).normalized;
        if (currentTime > m_TimeBeforeDestroy) Destroy(gameObject);
        else
        transform.position += dir * Time.fixedDeltaTime * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            enemy.TakeDamage(damageToDeal);
            GameObject.Destroy(gameObject);
        }
    }
}
