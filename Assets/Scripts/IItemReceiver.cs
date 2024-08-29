using Assets.Scripts.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public interface IItemReceiver
    {
        public void ReceiveItem(ItemObject item);
        public bool CanReceive(ItemObject item);
        public Item GetOutputData();
        public Item TakeOutItem();
    }
}
