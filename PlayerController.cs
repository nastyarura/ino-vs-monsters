using UnityEngine;

// Керування: WASD/стрілки — рух, B — поставити барикаду (вдень),
// R — полагодити найближчу барикаду (вдень), Space або ліва кнопка миші — стріляти (вночі).
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float shootRange = 8f;
    public float shootCooldown = 0.5f;
    public float shootDamage = 5f;        // 5 / 0.5 с = 10 DPS (аркуш "Параметри")
    public float accuracy = 0.7f;         // влучність (аркуш "Параметри")

    Vector3 facing = Vector3.back;
    float nextShot;

    void Update()
    {
        var gm = GameManager.I;
        if (gm.phase == Phase.Victory || gm.phase == Phase.Defeat) return;

        // Рух
        Vector3 move = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        if (move.sqrMagnitude > 0.01f) { facing = move.normalized; transform.position += facing * moveSpeed * Time.deltaTime; }

        // Збір ресурсів
        for (int i = Resource.All.Count - 1; i >= 0; i--)
        {
            var r = Resource.All[i];
            if ((r.transform.position - transform.position).sqrMagnitude < 1.5f * 1.5f)
            {
                gm.wood += 10;
                Destroy(r.gameObject);
            }
        }

        if (gm.phase == Phase.Day)
        {
            if (Input.GetKeyDown(KeyCode.B)) gm.TryBuildBarricade(transform.position + facing * 2.5f);
            if (Input.GetKeyDown(KeyCode.R)) gm.TryRepair(transform.position);
        }

        if (gm.phase == Phase.Night && (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)) && Time.time >= nextShot)
        {
            nextShot = Time.time + shootCooldown;
            Enemy target = null; float best = shootRange;
            foreach (var e in Enemy.All)
            {
                float d = Vector3.Distance(e.transform.position, transform.position);
                if (d < best) { best = d; target = e; }
            }
            if (target != null && Random.value < accuracy) target.Hit(shootDamage);
        }
    }
}
