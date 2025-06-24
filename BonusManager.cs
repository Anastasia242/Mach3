using System.Collections.Generic;
using UnityEngine;

public class BonusManager : MonoBehaviour
{
    [Header("Prefab бонусів")]
    public GameObject spoonBonusPrefab;
    public GameObject ladleBonusPrefab;
    public GameObject vinegarBonusPrefab;
    public GameObject mixerBonusPrefab;


    [Header("Куди спавнити бонуси")]
    public Transform bonusPanel; // UI панель або місце в грі

    public void SpawnBonuses(List<string> bonuses)
    {
        if (bonuses == null || bonuses.Count == 0)
            return;

        foreach (string bonus in bonuses)
        {
            GameObject prefab = GetBonusPrefab(bonus);
            if (prefab != null && bonusPanel != null)
            {
                Instantiate(prefab, bonusPanel);
            }
            else
            {
                Debug.LogWarning($"⚠️ Бонус '{bonus}' не знайдено або bonusPanel не призначено.");
            }
        }
    }

    private GameObject GetBonusPrefab(string bonusName)
    {
        switch (bonusName)
        {
            case "Spoon": return spoonBonusPrefab;
            case "Ladle": return ladleBonusPrefab;
            case "Vinegar": return vinegarBonusPrefab;
            case "Mixer": return mixerBonusPrefab;
            default: return null;
        }
    }

}
