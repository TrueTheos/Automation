﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Items
{
    public abstract class Item : ScriptableObject
    {
        public string Name;
        public Sprite Icon;
        public int MaxStack;
    }
}
