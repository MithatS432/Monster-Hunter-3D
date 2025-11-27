using UnityEngine;

public class FireBall : MonoBehaviour
{

    public float damage = 50f;
    public float speed = 40f;
    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.GetDamage(damage);
            }
            Destroy(gameObject);
        }

    }
}
