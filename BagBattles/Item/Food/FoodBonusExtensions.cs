using System.Collections.Generic;

public static class LinkedListExtensions
{
    /// <summary>
    /// 减少链表中所有加成的回合数，并移除过期的加成，返回移除的加成值总和
    /// </summary>
    /// <param name="list">要处理的加成链表</param>
    /// <returns>移除的加成值总和</returns>
    public static float DecreaseRounds(this LinkedList<Food.Bonus> list)
    {
        if (list == null || list.Count == 0)
            return 0;

        float removedBonusSum = 0;
        LinkedListNode<Food.Bonus> node = list.First;
        
        while (node != null)
        {
            LinkedListNode<Food.Bonus> nextNode = node.Next;
            Food.Bonus bonus = node.Value;
            bonus.DecreaseRound();
            
            if (bonus.roundLeft <= 0)
            {
                removedBonusSum += bonus.bonusValue;
                list.Remove(node);
            }
            node = nextNode;
        }
        
        return removedBonusSum;
    }
}