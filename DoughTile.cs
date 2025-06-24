using UnityEngine;

public class DoughTile : ObstacleTile
{
    public override void UpdateSprite()
    {
        if (layers > 0 && layers <= layerSprites.Length)
        {
            GetComponent<SpriteRenderer>().sprite = layerSprites[layers - 1];
        }
    }
}
