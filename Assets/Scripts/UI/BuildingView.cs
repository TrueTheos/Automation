using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public abstract class BuildingView : MonoBehaviour
    {
        protected bool _isOpen;
        public bool IsOpen => _isOpen;

        public void Update()
        {
            if (!_isOpen) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
        }

        public virtual void Close()
        {
            _isOpen = false;
            gameObject.SetActive(false);
        }

        public virtual void Open()
        {
            _isOpen = true;
            gameObject.SetActive(true);
        }
    }
}
