using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Globalization;

public class UdpClientTwoClients : MonoBehaviour
{
    public static UdpClient client;
    Thread receiveThread;
    IPEndPoint serverEP;
    public static int myId = -1;

    // Controle de bastões
    public GameObject localCube;
    public GameObject remoteCube;
    Rigidbody2D rb;
    public float moveSpeed = 12f;
    Vector3 remotePos;
    bool recebeuPosicao = false;

    void Start()
    {
        client = new UdpClient();
        serverEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5001);
        client.Connect(serverEP);

        receiveThread = new Thread(ReceiveData);
        receiveThread.IsBackground = true;
        receiveThread.Start();

        client.Send(Encoding.UTF8.GetBytes("HELLO"), 5);

        rb = localCube.GetComponent<Rigidbody2D>();

        // Pos inicial do bastão remoto
        remotePos = remoteCube.transform.position;
    }

    void FixedUpdate()
    {
        // 1️⃣ Movimento do bastão local
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector2 movement = new Vector2(h, v) * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

        // 2️⃣ Envia posição do bastão local
        string msg = "POS:" + myId + ";" +
                     localCube.transform.position.x.ToString("F2", CultureInfo.InvariantCulture) + ";" +
                     localCube.transform.position.y.ToString("F2", CultureInfo.InvariantCulture);
        client.Send(Encoding.UTF8.GetBytes(msg), msg.Length);

        // 3️⃣ Atualiza posição do bastão remoto suavemente
        if (recebeuPosicao)
        {
            remoteCube.transform.position = Vector3.Lerp(remoteCube.transform.position, remotePos, Time.deltaTime * 10f);
        }
    }

    void ReceiveData()
    {
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
        while (true)
        {
            try
            {
                byte[] data = client.Receive(ref remoteEP);
                string msg = Encoding.UTF8.GetString(data);

                // Recebe ID do cliente
                if (msg.StartsWith("ASSIGN:"))
                {
                    myId = int.Parse(msg.Substring(7));
                    Debug.Log("[Cliente] Meu ID = " + myId);
                }
                // Recebe posição do bastão
                else if (msg.StartsWith("POS:"))
                {
                    string[] parts = msg.Substring(4).Split(';');
                    if (parts.Length == 3)
                    {
                        int id = int.Parse(parts[0]);
                        if (id != myId)
                        {
                            float x = float.Parse(parts[1], CultureInfo.InvariantCulture);
                            float y = float.Parse(parts[2], CultureInfo.InvariantCulture);
                            remotePos = new Vector3(x, y, 0);
                            recebeuPosicao = true;
                        }
                    }
                }
                // Recebe posição da bola
                else if (msg.StartsWith("BALL:"))
                {
                    string[] parts = msg.Substring(5).Split(';');
                    if (parts.Length == 2)
                    {
                        float x = float.Parse(parts[0], CultureInfo.InvariantCulture);
                        float y = float.Parse(parts[1], CultureInfo.InvariantCulture);

                        GameObject ball = GameObject.FindWithTag("Ball");
                        if (ball != null)
                        {
                            BallGoalHandler ballScript = ball.GetComponent<BallGoalHandler>();
                            if (ballScript != null)
                                ballScript.UpdateRemoteBallPos(x, y);
                        }
                    }
                }
            }
            catch (SocketException e)
            {
                Debug.LogWarning("Erro UDP: " + e.Message);
            }
        }
    }

    void OnApplicationQuit()
    {
        if (receiveThread != null && receiveThread.IsAlive)
            receiveThread.Abort();
        if (client != null)
            client.Close();
    }
}
