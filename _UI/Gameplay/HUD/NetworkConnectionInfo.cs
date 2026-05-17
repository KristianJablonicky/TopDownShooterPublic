using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class NetworkConnectionInfo : PeriodicallyInvoked
{
    [Header("References")]
    [SerializeField] private GameObject pingContainer;
    [SerializeField] private TMP_Text pingValue;

    [Header("Ping settings")]
    [SerializeField] private int sampleCount = 5;
    [SerializeField] private float pingThreshold = 60f;


    [SerializeField] private Color
        goodColor = Color.green,
        mediumColor = Color.orange,
        badColor = Color.red;

    PeriodicalInvokeManager periodicalInvokeManager;
    private readonly Queue<float> samples = new();
    private float sum;

    private void Awake()
    {
        if (DataStorage.IsSinglePlayer) Destroy(gameObject);
        else pingValue.text = string.Empty;
    }

    public override void Invoke()
    {
        if (NetworkManager.Singleton == null
        || !NetworkManager.Singleton.IsConnectedClient) return;
        
        var diff = Mathf.Abs(
            NetworkManager.Singleton.LocalTime.TimeAsFloat -
            NetworkManager.Singleton.ServerTime.TimeAsFloat
        );

        var pingMs = diff * 1000f;

        samples.Enqueue(pingMs);
        sum += pingMs;

        if (samples.Count > sampleCount)
            sum -= samples.Dequeue();

        SetPing(sum / samples.Count);
    }

    private void SetPing(float ping)
    {
        if (ping >= pingThreshold)
        {
            if (!pingContainer.activeSelf)
            {
                pingContainer.SetActive(true);
            }
            pingValue.text = ((int)ping).ToString();
            pingValue.color = GetColor(ping);
        }
        else if (pingContainer.activeSelf)
        {
            pingContainer.SetActive(false);
        }
    }

    private Color GetColor(float ping)
    {
        return ping switch
        {
            _ when ping <= 40f => goodColor,
            _ when ping <= 90f => mediumColor,
            _ => badColor
        };
    }
}
