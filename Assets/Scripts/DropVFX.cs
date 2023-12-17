using UnityEngine;

/**
 * Attached to a gradient white sprite which can be tinted and 
 * used as a 'streak' effect for when a piece drops down instantaneously.
 * 
 * Initial position and scale is set by the BloxGameManager.
 */
public class DropVFX : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float fadeOutSpeed = 1f;
    [SerializeField] private float alpha = 0.1f;

    public void SetColor (Color pColor)
	{
        pColor.a = alpha;
        spriteRenderer.color = pColor;
	}

    void Update()
    {
        Vector3 scale = transform.localScale;
        scale.y -= fadeOutSpeed * Time.deltaTime;

        if (scale.y < 0)
		{
            Destroy(gameObject);
		}
		else
		{
            transform.localScale = scale;
		}
    }
}
