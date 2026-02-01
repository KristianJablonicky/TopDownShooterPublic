using UnityEngine;

public class PlayerCardAnimator : MonoBehaviour
{
    private struct CardAnimData
    {
        public GameObject card;
        public Transform transform;
        public float baseY, phaseOffset;
    }

    [SerializeField] private GameObject[] inputCards;

    [Header("Animation Settings")]
    [SerializeField] private float periodDuration = 3f;
    [SerializeField] private float maxY = 15f;
    [SerializeField] private float totalPiMultiplierOffset = 1f;

    private CardAnimData[] cards;
    private float phase;
    private int count;

    private void Start()
    {
        count = inputCards.Length;
        cards = new CardAnimData[count];

        for (int i = 0; i < count; i++)
        {
            var transform = inputCards[i].transform;
            var offset = (i / (float)count) * totalPiMultiplierOffset * Mathf.PI;

            cards[i] = new CardAnimData
            {
                card = inputCards[i],
                transform = transform,
                baseY = transform.localPosition.y - maxY * 0.5f,
                phaseOffset = offset
            };
        }
    }

    private void Update()
    {
        phase += Time.deltaTime / periodDuration;

        for (int i = 0; i < count; i++)
        {
            var c = cards[i];
            if (!c.card.activeSelf) continue;

            float y = c.baseY + Mathf.Sin(phase + c.phaseOffset) * maxY;

            var pos = c.transform.localPosition;
            pos.y = y;
            c.transform.localPosition = pos;
        }
    }
}
