using System;
using UnityEngine;
public enum CardColor
{
    None,Red,Black
}
[Serializable]
public class Card
{
    [SerializeField] public string Name ;
    [SerializeField] public int value;
    [SerializeField] public CardColor color;
    [SerializeField] public Sprite sprite;

    public Card(Sprite sprite)
    {
        this.sprite = sprite;
        this.Name = sprite.name;
        this.value = GetCardValueFromName(this.Name);
        this.color = GetCardColorFromName(this.Name);
    }


    private CardColor GetCardColorFromName(string name)
    {

        if (name.Contains("S") || name.Contains("C"))
        {
            return CardColor.Black;
        }
        else if (name.Contains("D") || name.Contains("H"))
        {
            return CardColor.Red;
        }
        else
        {
            return CardColor.None;
        }
    }


    private int GetCardValueFromName(string name)
    {
        if (name.Contains("K") || name.Contains("Q") || name.Contains("J") || name.Contains("10"))
        {
            return 0;
        }
        else if (name.Contains("A"))
        {
            return 1;
        }
        else
        {
            string lastIndex = name[name.Length - 1].ToString();
            int toValue = int.Parse(lastIndex);
            try
            {
                return toValue;
            }
            catch (Exception e)
            {
                return 0;
            }
        }

    }
    public override string ToString()
    {
        return base.ToString();
    }
}

[Serializable]
public struct CardData
{
    [SerializeField] public string Name;
    [SerializeField] public int value;
    
}
