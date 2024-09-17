using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    [Serializable]
    public class RandomDropTable
    {
        public List<RandomDropItem> Table = new();

        public ItemAmount GetRandomDrop()
        {
            if (Table.Count == 0)
            {
                return null;
            }

            float totalProbability = Table.Sum(item => item.Probability);
            float randomValue = UnityEngine.Random.Range(0f, totalProbability);
            float cumulativeProbability = 0f;

            foreach (var item in Table)
            {
                cumulativeProbability += item.Probability;
                if (randomValue <= cumulativeProbability)
                {
                    return item.ItemAmount;
                }
            }

            // If we somehow didn't select an item (shouldn't happen), return the last item
            return Table[^1].ItemAmount;
        }
    }

    [Serializable]
    public class RandomDropItem
    {
        public ItemAmount ItemAmount;
        public float Probability;
    }
}
