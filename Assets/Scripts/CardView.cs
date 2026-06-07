using UnityEngine;

/// <summary>
/// Classe responsável por ligar a lógica das cartas ao seu sprite
/// </summary>
public class CardView : MonoBehaviour
{
    /// <summary>
    /// Referencia ao componente spriterenderer do prefab das cartas
    /// </summary>
    [SerializeField] private SpriteRenderer spriteRenderer;

    /// <summary>
    /// organização necessária para a conta dar certo:
    /// 1- Hearts A-K
    /// 2- Diamonds A-K
    /// 3- Clubs A-K
    /// 4- Spades A-K
    /// </summary>
    [SerializeField] private Sprite[] cardSprites;
    

    /// <summary>
    /// Define o sprite da carta com base no valor e no naipe
    /// </summary>
    /// <param name="card"></param>
    public void SetSprite(Card card)
    {
        //Multiplicar o naipe por 13 porque cada naipe tem 13 variações
        int suitIndexOffset = (int)card.Suit * 13; // a cada 13 cartas troca de naipe
        //O valor da carta é de 1 a 13 mas o array é de 0 a 12
        int valueIndex = card.Value - 1; //como array começa em 0 diminuir vlor por 1
        //Indice final
        int spriteIndex = suitIndexOffset + valueIndex;

        //Atribui o sprite correspondente
        spriteRenderer.sprite = cardSprites[spriteIndex];
    }
}
