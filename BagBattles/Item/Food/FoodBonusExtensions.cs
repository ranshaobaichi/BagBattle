using System.Collections.Generic;

public static class FoodBonusExtensions
{
    /// <summary>
    /// 减少列表中所有加成的回合数，并移除已过期的加成
    /// </summary>
    /// <param name="bonusList">要处理的加成列表</param>
    /// <returns>返回所有被移除加成的值总和</returns>
    public static float DecreaseRounds(this LinkedList<Food.Bonus> bonusList)
    {
        float sum = 0;
        LinkedListNode<Food.Bonus> node = bonusList.First;
        
        while (node != null)
        {
            node.Value.DecreaseRound();
            
            if (node.Value.roundLeft <= 0)
            {
                sum += node.Value.bonusValue; // 累加移除的加成值
                LinkedListNode<Food.Bonus> nextNode = node.Next;
                bonusList.Remove(node);
                node = nextNode;
            }
            else
            {
                node = node.Next;
            }
        }
        
        return sum;
    }
}