using UnityEngine;

public class BallController : MonoBehaviour
{
    public float speed = 8f; // velocidade da bola
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Define uma direção inicial aleatória
        Vector2 initialDirection = new Vector2(
            Random.Range(0, 2) == 0 ? 1 : -1, // esquerda ou direita
            Random.Range(-0.5f, 0.5f)          // leve variação no eixo Y
        ).normalized;

        rb.linearVelocity = initialDirection * speed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Adiciona um pequeno ajuste de direção quando bate no bastão
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Enemy"))
        {
            // Pega a posição relativa do contato para variar o ângulo do quique
            float y = (transform.position.y - collision.transform.position.y) / collision.collider.bounds.size.y;
            Vector2 newDir = new Vector2(-rb.linearVelocity.x, y).normalized;
            rb.linearVelocity = newDir * speed;
        }
        else
        {
            // Normal bounce padrão nas paredes
            rb.linearVelocity = rb.linearVelocity.normalized * speed;
        }
    }
}
