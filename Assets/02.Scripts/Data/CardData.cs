using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/CardData")]
public class CardData : ScriptableObject
{
    public string cardName;
    [TextArea(3, 10)]
    public string description;
    public Sprite cardIcon;
    public CardEffectType effectType;
    public float value;

    public string GetFormattedDescription()
    {
        return description.Replace("{value}", value.ToString());
    }
}
