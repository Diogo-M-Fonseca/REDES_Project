using UnityEngine;

public class CardView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    /// <summary>
    /// organização necessária para a conta dar certo:
    /// 1- Hearts A-K
    /// 2- Diamonds A-K
    /// 3- Clubs A-K
    /// 4- Spades A-K
    /// </summary>
    [SerializeField] private Sprite[] cardSprites;
    

    public void SetSprite(Card card)
    {
        //não esquecer calculos para associar carta certa ao seu sprite certo
        int suitIndexOffset = (int)card.Suit * 13; // a cada 13 cartas troca de naipe
        int valueIndex = card.Value - 1; //como array começa em 0 diminuir vlor por 1

        int spriteIndex = suitIndexOffset + valueIndex;

        spriteRenderer.sprite = cardSprites[spriteIndex];
    }
}
