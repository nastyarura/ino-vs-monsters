using System.Collections.Generic;
using UnityEngine;

public class Barricade : MonoBehaviour
{
    public static readonly List<Barricade> All = new List<Barricade>();
    public float hp = 300f;      // міцність (аркуш "Параметри")
    public const float MaxHp = 300f;

    void OnEnable()  { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    public void Damage(float d)
    {
        hp -= d;
        // Колір показує стан: від коричневого до червоного
        float k = Mathf.Clamp01(hp / MaxHp);
        GetComponent<Renderer>().material.color = Color.Lerp(Color.red, new Color(0.6f, 0.4f, 0.2f), k);
        if (hp <= 0) Destroy(gameObject);
    }

    public void Repair(float amount)
    {
        hp = Mathf.Min(MaxHp, hp + amount);
        GetComponent<Renderer>().material.color = Color.Lerp(Color.red, new Color(0.6f, 0.4f, 0.2f), hp / MaxHp);
    }
}
