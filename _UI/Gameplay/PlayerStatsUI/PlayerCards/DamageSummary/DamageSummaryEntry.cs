using TMPro;
using UnityEngine;

public class DamageSummaryEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text damageTMP;

    public void ShowDamage(int damage, int hitCount)
    {
        if (damage == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        damageTMP.text = $"{damage} in {hitCount}";
    }
}
