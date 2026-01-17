using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MinimapOnlySprite : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Başlangıçta kapalı
        spriteRenderer.enabled = true;
    }

}
