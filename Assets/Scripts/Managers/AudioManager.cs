using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioClip PickupItemSound;
        [SerializeField] private AudioClip CraftSound;

        private static AudioSource _audioSource;

        public static AudioManager Instance;

        private void Awake()
        {
            Instance = this;
            _audioSource = GetComponent<AudioSource>();
        }

        public void PickupItem()
        {
            _audioSource.PlayOneShot(PickupItemSound);
        }

        public void Craft()
        {
            _audioSource.PlayOneShot(CraftSound);
        }
    }
}
