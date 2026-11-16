using System.Collections.Generic;
using UnityEngine;

public enum EnemyType { Normal, Runner, Armored, Boss }

// Ворог: йде до бази, зупиняється біля барикади і ламає її.
// Числа беруться з таблиці балансу (аркуш "Вороги").
public class Enemy : MonoBehaviour
{
    public static readonly List<Enemy> All = new List<Enemy>();

    public float hp, speed, damagePerSecond;
    Transform baseT;

    public static void Spawn(EnemyType type, Vector3 pos, Transform baseTransform)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position = pos;
        var e = go.AddComponent<Enemy>();
        e.baseT = baseTransform;
        Color c = Color.green;
        switch (type)
        {
            case EnemyType.Normal:  e.hp = 30;  e.speed = 1.0f; e.damagePerSecond = 3;  c = new Color(0.4f, 0.8f, 0.4f); break;
            case EnemyType.Runner:  e.hp = 15;  e.speed = 2.5f; e.damagePerSecond = 2;  c = Color.yellow; go.transform.localScale = new Vector3(0.7f, 1f, 0.7f); break;
            case EnemyType.Armored: e.hp = 90;  e.speed = 0.8f; e.damagePerSecond = 8;  c = Color.gray; go.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f); break;
            case EnemyType.Boss:    e.hp = 400; e.speed = 0.6f; e.damagePerSecond = 20; c = Color.red; go.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f); break;
        }
        go.GetComponent<Renderer>().material.color = c;
        go.name = "Enemy_" + type;
    }

    void OnEnable()  { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    void Update()
    {
        if (GameManager.I.phase != Phase.Night) return;

        Vector3 dir = baseT.position - transform.position; dir.y = 0;
        float dist = dir.magnitude;
        Vector3 d = dir.normalized;

        // Дійшов до бази: б'є базу
        if (dist < 2.5f) { GameManager.I.DamageBase(damagePerSecond * Time.deltaTime); return; }

        // Попереду барикада: ламає її
        Collider[] hits = Physics.OverlapSphere(transform.position + d * 1.2f, 0.8f);
        foreach (var h in hits)
        {
            var b = h.GetComponent<Barricade>();
            if (b != null) { b.Damage(damagePerSecond * Time.deltaTime); return; }
        }

        transform.position += d * speed * Time.deltaTime;
    }

    public void Hit(float dmg)
    {
        hp -= dmg;
        if (hp <= 0) Destroy(gameObject);
    }
}
