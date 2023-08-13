using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace piyoryyta.AssetPalette
{
    public class AssetPaletteScriptable : ScriptableObject, ISerializationCallbackReceiver
    {
        public Vector2 viewOffset;
        public float zoom = 1;
        [SerializeField]
        public AssetPaletteScriptable current;
        [SerializeField]
        private List<string> _guids = new List<string>();
        [SerializeField]
        private List<Object> _resources = new List<Object>();
        [SerializeField]
        private List<Vector2> _positions = new List<Vector2>();
        [SerializeField]
        private List<float> _scales = new List<float>();

        public Dictionary<string, Tuple<Object, Vector2, float>> assetPaletteElements = new Dictionary<string, Tuple<Object, Vector2, float>>();

        private void AddToScriptable(string guid, Tuple<Object, Vector2, float> assetPaletteElement)
        {
            if (!_guids.Contains(guid))
            {
                _guids.Add(guid);
                _resources.Add(assetPaletteElement.Item1);
                _positions.Add(assetPaletteElement.Item2);
                _scales.Add(assetPaletteElement.Item3);
            }
        }

        public void OnBeforeSerialize()
        {
            _guids.Clear();
            _resources.Clear();
            _positions.Clear();
            _scales.Clear();
            foreach (var assetPaletteElement in assetPaletteElements)
            {
                AddToScriptable(assetPaletteElement.Key, assetPaletteElement.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            int[] counts = new int[] { _guids.Count, _resources.Count, _positions.Count, _scales.Count };
            int minCount = Mathf.Min(counts);
            int maxCount = Mathf.Max(counts);
            assetPaletteElements.Clear();
            if (minCount != maxCount)
            {
                Debug.LogWarning("AssetPalette: Don't edit the AssetPaletteScriptable file manually!");
            }
            for (int i = 0; i < minCount; i++)
            {
                if (_resources[i])
                {
                    assetPaletteElements.Add(_guids[i], new Tuple<Object, Vector2, float>(_resources[i], _positions[i], _scales[i]));
                }
            }
        }
    }
}