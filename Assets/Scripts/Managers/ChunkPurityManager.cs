using Assets.Scripts.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.VFX;

namespace Assets.Scripts
{
    public class ChunkPurityManager : MonoBehaviour
    {
        [SerializeField] private VisualEffect _fog;

        public bool hasScafander = false;
        public float visionRadius = 5f;//temp to sie pozniej bedzie sprawdzalo po itemach 

        private MapGenerator _mapGen => MapGenerator.Instance;
        private MapManager _mapManager;
        private Player _player;
        private Camera _mainCam;

        public static ChunkPurityManager Instance;

        private Vector3 _lastPlayerPos;

        private HashSet<VisualEffect> _enabledFogs = new HashSet<VisualEffect>();

        private void Awake()
        {
            Instance = this;
            _mainCam = Camera.main;
            _fog.SetFloat("VisionRadius", visionRadius);
        }

        private void Start()
        {
            _mapManager = MapManager.Instance;
            _player = GameManager.Instance.CurrentPlayer;
        }

        private void LateUpdate()
        {
            if (!_mapGen.MapGenerated) return;

            foreach(Chunk chunk in _mapGen.Chunks) 
            {
                VisualEffect fogObject = chunk.Fog;
                fogObject.gameObject.SetActive(true);
                fogObject.SetBool("hasScafander", hasScafander);
                fogObject.SetVector3("PlayerPos", _player.transform.position);
                fogObject.SetFloat("Time", DayNightCycleManager.Instance.PercentOfDay());
            }

            if(_lastPlayerPos != _player.transform.position)
            {
                /*Vector2 bottomLeft = _mainCam.ViewportToWorldPoint(new Vector2(0, 0));
                Vector2 topRight = _mainCam.ViewportToWorldPoint(new Vector2(1, 1));

                bottomLeft.x = Mathf.Clamp(bottomLeft.x, 0, _mapManager.Width - 1);
                bottomLeft.y = Mathf.Clamp(bottomLeft.y, 0, _mapManager.Height - 1);

                topRight.x = Mathf.Clamp(topRight.x, 0, _mapManager.Width - 1);
                topRight.y = Mathf.Clamp(topRight.y, 0, _mapManager.Height - 1);

                Chunk leftBottomChunk = _mapGen.GetChunk(Mathf.RoundToInt(bottomLeft.x), Mathf.RoundToInt(bottomLeft.y));
                Chunk topRightChunk = _mapGen.GetChunk(Mathf.RoundToInt(topRight.x), Mathf.RoundToInt(topRight.y));

                //HashSet<VisualEffect> currentlyVisibleFogs = new HashSet<VisualEffect>();

                for (int x = Mathf.Max(0, leftBottomChunk.X - 1); x <= Mathf.Min(_mapGen.Chunks.GetLength(0) - 1, topRightChunk.X + 1); x++)
                {
                    for (int y = Mathf.Max(0, leftBottomChunk.Y - 1); y <= Mathf.Min(_mapGen.Chunks.GetLength(1) - 1, topRightChunk.Y + 1); y++)
                    {
                        VisualEffect fogObject = _mapGen.Chunks[x, y].Fog;
                        fogObject.gameObject.SetActive(true);
                        //currentlyVisibleFogs.Add(fogObject);
                        fogObject.SetBool("hasScafander", hasScafander);
                        fogObject.SetVector3("PlayerPos", _player.transform.position);
                    }
                }

                foreach (VisualEffect fog in _enabledFogs)
                {
                    if (!currentlyVisibleFogs.Contains(fog))
                    {
                        fog.gameObject.SetActive(false);
                    }
                }

                _enabledFogs = currentlyVisibleFogs;
                _lastPlayerPos = _player.transform.position;*/
            }
        }

        public void SetupFog()
        {
            int chunkSize = MapGenerator.CHUNK_SIZE;

            foreach (var chunk in _mapGen.Chunks)
            {
                var newFog = Instantiate(_fog, new Vector3(chunk.WorldX + chunkSize / 2, chunk.WorldY + chunkSize / 2, 0), Quaternion.identity);
                chunk.Fog = newFog;
            }
        }
    }
}
