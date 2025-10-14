using UnityEngine;

public class BallGoalHandler : MonoBehaviour
{
    public Vector2 initialPosition = Vector2.zero; // posição central
    public float relaunchDelay = 1f;
    public float launchSpeed = 8f;

    private Rigidbody2D rb;

    // Placar estático simples
    public static int leftScore = 0;
    public static int rightScore = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ResetBall();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("LeftGoal"))
        {
            rightScore++;
            Debug.Log("✅ Ponto para a Direita | Placar: " + leftScore + " - " + rightScore);
            ResetBall();
        }
        else if (other.CompareTag("RightGoal"))
        {
            leftScore++;
            Debug.Log("✅ Ponto para a Esquerda | Placar: " + leftScore + " - " + rightScore);
            ResetBall();
        }
    }

    void ResetBall()
    {
        // Para a bola e reseta posição
        rb.linearVelocity = Vector2.zero;
        transform.position = initialPosition;

        // Lança novamente após um pequeno delay
        Invoke(nameof(LaunchBall), relaunchDelay);
    }

    void LaunchBall()
    {
        Vector2 dir = new Vector2(
            Random.Range(0, 2) == 0 ? 1 : -1,
            Random.Range(-0.5f, 0.5f)
        ).normalized;

        rb.linearVelocity = dir * launchSpeed;
    }
}
