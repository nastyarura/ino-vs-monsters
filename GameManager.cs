using System.Collections;
using UnityEngine;

public enum Phase { Day, Night, Victory, Defeat }

// Головний цикл гри: День -> Ніч -> Підсумок -> ... -> Перемога/Поразка.
// Кількості ворогів у хвилях відповідають аркушу "Баланс хвиль".
public class GameManager : MonoBehaviour
{
    public static GameManager I;

    public Phase phase = Phase.Day;
    public float dayDuration = 60f;      // для тесту; у плані було 2-3 хв
    public int day = 1;
    public int maxDays = 3;
    public int wood = 0;
    public int maxBarricades = 3;
    public float baseHp = 500f;
    public Transform baseTransform;      // якщо порожньо, шукаємо об'єкт "Base"

    float dayTimer;
    string message = "";

    // [ніч][хвиля] = { Незграбний, Біжун, Броньований, Бос }
    readonly int[][][] waves = new int[][][]
    {
        new[] { new[]{5,0,0,0}, new[]{7,2,0,0} },
        new[] { new[]{6,3,0,0}, new[]{8,4,1,0} },
        new[] { new[]{10,5,2,0}, new[]{8,4,2,1} },
    };

    void Awake() { I = this; }

    void Start()
    {
        if (baseTransform == null) baseTransform = GameObject.Find("Base").transform;
        StartDay();
    }

    void StartDay()
    {
        phase = Phase.Day;
        dayTimer = dayDuration;
        message = "День " + day + ": збирайте дерево, будуйте барикади (B), лагодьте (R)";
        for (int i = 0; i < 8; i++)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Resource";
            go.transform.localScale = Vector3.one * 0.6f;
            go.transform.position = new Vector3(Random.Range(-15f, 15f), 0.3f, Random.Range(-14f, 14f));
            go.GetComponent<Renderer>().material.color = new Color(1f, 0.8f, 0.1f);
            go.AddComponent<Resource>();
        }
    }

    void Update()
    {
        if (phase == Phase.Day)
        {
            dayTimer -= Time.deltaTime;
            if (dayTimer <= 0) StartCoroutine(Night());
        }
    }

    IEnumerator Night()
    {
        phase = Phase.Night;
        message = "Ніч " + day + ": тримайте оборону! (Space / ЛКМ — стріляти)";
        foreach (var r in Resource.All.ToArray()) Destroy(r.gameObject);

        var nightWaves = waves[Mathf.Min(day - 1, waves.Length - 1)];
        foreach (var w in nightWaves)
        {
            SpawnWave(w);
            float t = 0;
            // наступна хвиля, коли всіх вбито або минуло 20 с
            while (t < 20f && Enemy.All.Count > 0 && phase == Phase.Night) { t += Time.deltaTime; yield return null; }
        }
        while (Enemy.All.Count > 0 && phase == Phase.Night) yield return null;

        if (phase != Phase.Night) yield break;   // програли

        if (day >= maxDays) { phase = Phase.Victory; message = "ПЕРЕМОГА! Вертоліт прибув."; yield break; }

        // Підсумок дня: нагорода
        wood += 15;
        day++;
        StartDay();
    }

    void SpawnWave(int[] w)
    {
        EnemyType[] types = { EnemyType.Normal, EnemyType.Runner, EnemyType.Armored, EnemyType.Boss };
        for (int t = 0; t < 4; t++)
            for (int i = 0; i < w[t]; i++)
            {
                Vector3 p = new Vector3(Random.Range(-15f, 15f), 0.5f, 20f + Random.Range(0f, 6f));
                Enemy.Spawn(types[t], p, baseTransform);
            }
    }

    public void DamageBase(float d)
    {
        if (phase != Phase.Night) return;
        baseHp -= d;
        if (baseHp <= 0) { baseHp = 0; phase = Phase.Defeat; message = "ПОРАЗКА. Базу знищено."; }
    }

    public void TryBuildBarricade(Vector3 pos)
    {
        if (Barricade.All.Count >= maxBarricades || wood < 10) return;
        wood -= 10;
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Barricade";
        go.transform.localScale = new Vector3(4f, 1.5f, 0.6f);
        pos.y = 0.75f; go.transform.position = pos;
        go.GetComponent<Renderer>().material.color = new Color(0.6f, 0.4f, 0.2f);
        go.AddComponent<Barricade>();
    }

    public void TryRepair(Vector3 from)
    {
        if (wood < 5) return;
        Barricade nearest = null; float best = 4f;
        foreach (var b in Barricade.All)
        {
            float d = Vector3.Distance(b.transform.position, from);
            if (d < best) { best = d; nearest = b; }
        }
        if (nearest != null) { wood -= 5; nearest.Repair(50f); }
    }

    // Простий інтерфейс без Canvas — щоб не налаштовувати UI
    void OnGUI()
    {
        GUI.skin.label.fontSize = 18;
        GUI.Label(new Rect(10, 10, 900, 30), message);
        GUI.Label(new Rect(10, 40, 900, 30),
            "День: " + day + "/" + maxDays + "   Дерево: " + wood +
            "   База HP: " + Mathf.CeilToInt(baseHp) +
            "   Барикади: " + Barricade.All.Count + "/" + maxBarricades +
            (phase == Phase.Day ? "   До ночі: " + Mathf.CeilToInt(dayTimer) + " с" : "   Ворогів: " + Enemy.All.Count));
    }
}
