using Assets.Scripts.MapObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class IronCrateView : BuildingView
    {
        public List<ItemSlot> Slots = new();
        private IronCrateObject _ironCrate;

        private void Awake()
        {
            for (int i = 0; i < Slots.Count; i++)
            {
                Slots[i].OnItemChangeEvent += () => UpdateIronCrate(i);
            }
        }

        private void UpdateIronCrate(int slotID)
        {
            _ironCrate.UpdateSlot(Slots[slotID], slotID);
        }

        public override void ResetUI()
        {
            foreach (var slot in Slots)
            {
                slot.ResetSlot();
            }
        }

        public void UpdateSlot(int index, ItemAmount itemAmount)
        {
            ItemSlot slot = Slots[index];
            slot.ResetSlot(); //to nie jest optymalne ale mam wyjebane elo
            if(itemAmount.Item != null) slot.Init(itemAmount);
        }

        public void OpenIronCrate(IronCrateObject ironCrate)
        {
            _ironCrate = ironCrate;
            ResetUI(); //to nie jest optymalne ale mam wyjebane elo
            for (int i = 0; i < _ironCrate.SlotsCount; i++)
            {
                UpdateSlot(i, _ironCrate.Items[i]);
            }
            Open();
        }
    }
}
