using System;
using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace piyoryyta.AssetPalette
{
    public class AssetPaletteElement : VisualElement
    {
        public static float mouseDownTime = 0.3f;
        public static float mouseDownDistance = 5;
        public static Vector2 viewOffset = Vector2.zero;
        public static float zoom = 1;
        public readonly string guid;
        public float scale { get; private set; }
        public Vector2 clickedPosition { get; private set; }

        private Vector2 _position = new Vector2(0, 0);
        private Vector2 _rectScale = new Vector2(100, 100);
        public Vector2 position
        {
            get => _position;
            set
            {
                _position = value;
                SetPosition();
            }
        }
        public Rect rect { get { return new Rect(_position.x, _position.y, _rectScale.x * scale, _rectScale.y * scale); } }
        public UnityEngine.Object asset { get; private set; }
        private Texture2D _previewRaw;
        private float _lastClickedTime;

        private readonly Label _assetNameLabel = new Label();
        private readonly Box _box = new Box();
        private readonly Image _assetImage = new Image();
        public Texture assetImage { get => _assetImage.image; }

        public Action<AssetPaletteElement> onRemove;
        public Action<AssetPaletteElement> onClicked;

        public AssetPaletteElement(string GUID, UnityEngine.Object asset, Vector2 positon, float scale)
        {
            this.guid = GUID;
            this.asset = asset;
            this.position = positon;
            this.scale = scale;
            _AssetPaletteElement();
        }

        public AssetPaletteElement()
        {
            guid = Guid.NewGuid().ToString();
            this.scale = 1;
            _AssetPaletteElement();
        }

        public void _AssetPaletteElement()
        {
            style.position = Position.Absolute;
            _box.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Column);
            _box.style.bottom = 0;
            _box.style.overflow = new StyleEnum<Overflow>(Overflow.Hidden);
            style.width = _rectScale.x;
            style.height = _rectScale.y;

            _assetNameLabel.text = "None";
            _assetImage.scaleMode = ScaleMode.ScaleToFit;
            _assetImage.style.width = 100;
            _assetImage.style.height = 100;

            Add(_box);
            _box.Add(_assetImage);
            _box.Add(_assetNameLabel);

            SetPosition();
            SetAsset(asset);

            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 0)
            {
                BringToFront();
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new UnityEngine.Object[1] { asset };
                DragAndDrop.StartDrag("AssetPaletteDrag");
                DragAndDrop.SetGenericData("fromAssetPalette", true);
                DragAndDrop.SetGenericData("owner", this);
                Event.current.Use();
                if (Time.realtimeSinceStartup - _lastClickedTime < mouseDownTime && Vector2.Distance(evt.mousePosition, clickedPosition) < mouseDownDistance)
                {
                    AssetDatabase.OpenAsset(asset);
                }
                _lastClickedTime = Time.realtimeSinceStartup;
                clickedPosition = evt.mousePosition;
                onClicked?.Invoke(this);
            }
            else if (evt.button == 1)
            {
                BringToFront();
            }
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (evt.button == 0)
            {

                if (Vector2.Distance(evt.mousePosition, clickedPosition) < mouseDownDistance)
                {
                }
            }
            else if (evt.button == 1)
            {
                //Show Context Menu
                GenericMenu menu = new GenericMenu();

                menu.AddItem(
                    new GUIContent("Scale/Scale Up"), false, () =>
                    {
                        scale = Math.Min(scale + 0.2f, 2);
                        SetPosition();
                    });
                menu.AddItem
                    (
                    new GUIContent("Scale/Scale Down"), false, () =>
                    {
                        scale = Math.Max(0.4f, scale - 0.2f);
                        SetPosition();
                    });
                menu.AddItem
                    (
                    new GUIContent("Scale/Reset Scale"), false, () =>
                    {
                        scale = 1;
                        SetPosition();
                    });
                menu.AddSeparator("");
                menu.AddItem
                    (
                    new GUIContent("Show in Project"), false, () =>
                    {
                        EditorGUIUtility.PingObject(asset);
                    });
                menu.AddSeparator("");
                menu.AddItem
                    (
                    new GUIContent("Reload Asset"), false, () =>
                    {
                        SetAsset(asset);
                    });
                menu.AddItem(new GUIContent("Delete from Palette"), false, () =>
                {
                    Remove();
                });
                menu.ShowAsContext();
            }
        }

        public void Remove()
        {
            onRemove?.Invoke(this);
            parent.Remove(this);
        }

        public Tuple<UnityEngine.Object, Vector2, float> GetSaveData()
        {
            return new Tuple<UnityEngine.Object, Vector2, float>(asset, position, scale);
        }
        public void SetAsset(UnityEngine.Object asset)
        {
            this.asset = asset;
            if (this.asset != null)
            {
                _assetNameLabel.text = this.asset.name;
                _previewRaw = AssetPreview.GetMiniThumbnail(this.asset);
                _assetImage.image = new Texture2D((int)_assetImage.style.width.value.value, (int)_assetImage.style.height.value.value);
                //TODO: Image can be smaller than 100x100
                //EditorUtility.CopySerialized(_previewRaw, _assetImage.image);
                _assetImage.image = _previewRaw;
                EditorCoroutineUtility.StartCoroutine(LoadAssetPreview(this.asset), this);
            }
            else
            {
                _assetNameLabel.text = "Missing";
                _assetImage.image = null;
            }
        }

        private IEnumerator LoadAssetPreview(UnityEngine.Object asset)
        {
            const int maxTry = 2000;
            int i = 0;

            // TODO: This sometime fails to load the preview, and somehow calling it again fixes it. This is Unity.
            Texture2D preview = AssetPreview.GetAssetPreview(asset);
            yield return null;
            preview = AssetPreview.GetAssetPreview(asset);
            do
            {
                preview = AssetPreview.GetAssetPreview(asset);
                yield return null;
                if (i++ > maxTry)
                {
                    yield break;
                }
            } while (AssetPreview.IsLoadingAssetPreview(asset.GetInstanceID()) && preview == null);
            if (preview != null)
            {
                //EditorUtility.CopySerialized(preview, _assetImage.image);
                _assetImage.image = preview;
            }
            MarkDirtyRepaint();
        }


        public void SetPosition()
        {
            if (visible)
            {
                transform.scale = new Vector3(zoom * scale, zoom * scale, 1);
                transform.position = (rect.position * zoom) + viewOffset;
                _assetNameLabel.MarkDirtyRepaint();
            }
        }

        public void SetScale(float scale)
        {
            this.scale = scale;
            MarkDirtyRepaint();
        }
    }
}
