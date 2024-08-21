using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioClip PickupItemSound;

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
    }
}
