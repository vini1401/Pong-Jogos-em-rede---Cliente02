using UnityEngine;
using System.Globalization;
using System.Text;

public class BallGoalHandler : MonoBehaviour
{
    public Vector2 initialPosition = Vector2.zero; // posição central
    public float relaunchDelay = 1f;
    public float launchSpeed = 8f;

    private Rigidbody2D rb;

    // Controle de rede
    public bool isBallOwner = false; // quem controla a bola
    private Vector3 remoteBallPos = Vector3.zero;

    // Placar simples
    public static int leftScore = 0;
    public static int rightScore = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 🧩 Define quem é o "dono" da bola:
        // cliente 1 controla a bola, cliente 2 apenas recebe
        isBallOwner = (UdpClientTwoClients.myId == 1);

        // se não for dono, desliga a física
        if (!isBallOwner)
        {
            rb.isKinematic = true;
        }

        ResetBall();
    }

    void FixedUpdate()
    {
        // Se for o dono da bola, envia posição para os outros clientes
        if (isBallOwner)
        {
            string msg = "BALL:" +
                transform.position.x.ToString("F2", CultureInfo.InvariantCulture) + ";" +
                transform.position.y.ToString("F2", CultureInfo.InvariantCulture);

            UdpClientTwoClients.client.Send(Encoding.UTF8.GetBytes(msg), msg.Length);
        }
        else
        {
            // Se não for o dono, atualiza a posição recebida da rede
            transform.position = Vector3.Lerp(transform.position, remoteBallPos, Time.deltaTime * 10f);
        }
    }

    // 🧱 Rebote nas paredes e bastões
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isBallOwner) return; // só o dono calcula física

        // Rebote nas paredes superior e inferior
        if (collision.gameObject.CompareTag("Wall"))
        {
            Vector2 newVelocity = rb.linearVelocity;
            newVelocity.y = -newVelocity.y;

            if (Mathf.Abs(newVelocity.y) < 0.1f)
                newVelocity.y = Mathf.Sign(newVelocity.y) * 0.2f;

            rb.linearVelocity = newVelocity.normalized * launchSpeed;
        }

        // Rebote nos bastões
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Enemy"))
        {
            float y = (transform.position.y - collision.transform.position.y) / collision.collider.bounds.size.y;
            Vector2 newDir = new Vector2(-rb.linearVelocity.x, y).normalized;
            rb.linearVelocity = newDir * launchSpeed;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isBallOwner) return; // só o dono reinicia e marca ponto

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
        rb.linearVelocity = Vector2.zero;
        transform.position = initialPosition;
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

    // 🚀 Função pública para receber posição via rede
    public void UpdateRemoteBallPos(float x, float y)
    {
        remoteBallPos = new Vector3(x, y, 0);
    }
}
