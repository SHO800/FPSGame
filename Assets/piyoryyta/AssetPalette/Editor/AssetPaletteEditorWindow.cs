using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace piyoryyta.AssetPalette
{
    public class AssetPaletteEditorWindow : EditorWindow
    {
        public const string version = "1.1.1";
        private AssetPaletteScriptable _assetPaletteScriptable;
        public AssetPaletteScriptable assetPaletteScriptable { get => _assetPaletteScriptable; }
        private AssetPaletteScriptable _defaultAssetPaletteScriptable;
        private Vector2 _viewOffset;
        private Vector2 _oldViewOffset = Vector2.zero;

        private AssetPaletteView _graphView;
        private Box _previewBox;
        private Box _pathBox;
        private Label _pathLabel;
        private AssetPaletteToolbar _toolbar;

        private bool _previewShown = false;

        private List<AssetPaletteElement> _assetPaletteElements = new List<AssetPaletteElement>();

        [MenuItem("piyoryyta/AssetPalette")]
        public static void ShowWindow()
        {
            EditorWindow window = GetWindow(typeof(AssetPaletteEditorWindow), false, "AssetPalette");
            window.minSize = new Vector2(400, 400);
        }

        private void OnEnable()
        {
            _previewBox = new Box();
            _previewBox.style.position = Position.Absolute;
            _previewBox.visible = false;

            _pathBox = new Box();
            _pathBox.style.position = Position.Absolute;
            _pathBox.style.bottom = 0;
            _pathBox.style.height = 20;
            _pathBox.style.right = 0;
            _pathBox.style.left = 0;

            _pathLabel = new Label();
            _pathBox.Add(_pathLabel);
            _pathLabel.StretchToParentSize();

            rootVisualElement.Add(_pathBox);

            string selfPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            selfPath = selfPath.Remove(selfPath.LastIndexOf("/Editor"));

            _defaultAssetPaletteScriptable = AssetDatabase.LoadAssetAtPath<AssetPaletteScriptable>(selfPath + "/DefaultAssetPalette.asset");
            if (!_defaultAssetPaletteScriptable)
            {
                _defaultAssetPaletteScriptable = CreateInstance<AssetPaletteScriptable>();
                _defaultAssetPaletteScriptable.current = _defaultAssetPaletteScriptable;
                AssetDatabase.CreateAsset(_defaultAssetPaletteScriptable, selfPath + "/DefaultAssetPalette.asset");
            }
            _assetPaletteScriptable = _defaultAssetPaletteScriptable.current;

            _toolbar = new AssetPaletteToolbar(this);
            _toolbar.onResetViewClicked += (assetPaletteToolbar) =>
            {
                _graphView.UpdateViewTransform(new Vector2(0, 0), Vector3.one);
            };
            _toolbar.onListViewClicked += (assetPaletteToolbar) =>
            {
                AssetPaletteListEditorWindow.SetAssetPaletteEditorWindow(this);
                AssetPaletteListEditorWindow.ShowWindow();
            };

            _graphView = new AssetPaletteView(null, _assetPaletteScriptable.zoom);
            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
            LoadScriptable(_assetPaletteScriptable);


            _graphView.SendToBack();
            _graphView.Add(_previewBox);
            _graphView.RegisterCallback<DragPerformEvent>(OnDragPerformEvent);

            _graphView.viewTransformChanged += OnViewMoved;

            _oldViewOffset -= Vector2.one;
            EditorCoroutineUtility.StartCoroutine(GetUpdate("https://pastebin.com/raw/VCDH6jkS"), this);
        }
        private IEnumerator GetUpdate(string URL)
        {
            System.Version version = new System.Version(AssetPaletteEditorWindow.version);
            string assetPalette_latestVersion = EditorUserSettings.GetConfigValue("piyoryyta.AssetPalette.latestVersion");
            if (assetPalette_latestVersion == null)
            {
                assetPalette_latestVersion = "0.0.0";
            }
            System.Version latestVersion = new System.Version(assetPalette_latestVersion);
            if (version > latestVersion)
            {
                string lastChecked = EditorUserSettings.GetConfigValue("piyoryyta.AssetPalette.lastChecked");

                if (lastChecked == null || lastChecked == "")
                {
                    lastChecked = System.DateTime.Now.ToString();
                }
                EditorUserSettings.SetConfigValue("piyoryyta.AssetPalette.lastChecked", System.DateTime.Now.ToString());
                if (System.DateTime.Now.Subtract(System.DateTime.Parse(lastChecked)).TotalHours > 12)
                {
                    UnityWebRequest request = UnityWebRequest.Get(URL);
                    yield return request.SendWebRequest();
                    if (request.isDone)
                    {
                        latestVersion = new System.Version(request.downloadHandler.text);
                    }
                }
            }

            if (version < latestVersion)
            {
                EditorUserSettings.SetConfigValue("piyoryyta.AssetPalette.latestVersion", latestVersion.ToString());
                _toolbar.hasUpdate = true;
                _toolbar.Refresh();
            }
        }

        private void OnGUI()
        {
            _viewOffset = _graphView.viewTransform.position;
            AssetPaletteElement.viewOffset = _viewOffset;
            AssetPaletteElement.zoom = 1;
            if (_oldViewOffset != _viewOffset)
            {
                Rect viewRect = new Rect(-_viewOffset / _graphView.scale, position.size / _graphView.scale);
                foreach (AssetPaletteElement assetPaletteElement in _assetPaletteElements)
                {
                    //check if assetPaletteElement is in view
                    if (viewRect.Overlaps(assetPaletteElement.rect))
                    {
                        assetPaletteElement.SetEnabled(true);
                        assetPaletteElement.visible = true;
                    }
                    else
                    {
                        assetPaletteElement.SetEnabled(false);
                        assetPaletteElement.visible = false;
                    }
                    assetPaletteElement.SetPosition();
                }
            }
            //D&D handler
            if (Event.current.type == EventType.DragUpdated && _graphView.contentRect.Contains(Event.current.mousePosition))
            {
                if ((DragAndDrop.GetGenericData("fromAssetPalette") as bool?) == true)
                {
                    AssetPaletteElement assetPaletteElement = DragAndDrop.GetGenericData("owner") as AssetPaletteElement;
                    if (!_previewShown)
                    {
                        _previewBox.visible = true;
                        _previewBox.PlaceBehind(assetPaletteElement);
                        _previewBox.transform.scale = assetPaletteElement.transform.scale;
                        _previewBox.style.width = assetPaletteElement.style.width.value.value;
                        _previewBox.style.height = assetPaletteElement.style.height.value.value; _previewBox.transform.position = assetPaletteElement.transform.position;
                        _previewBox.style.backgroundColor = new Color(0, 0, 0, 0.5f);
                    }
                    _previewBox.transform.position = assetPaletteElement.transform.position;
                    _previewBox.transform.position += (Vector3)(Event.current.mousePosition - assetPaletteElement.clickedPosition + new Vector2(0, 20));
                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                    DragAndDrop.AcceptDrag();
                }
                else if (DragAndDrop.objectReferences.Any(obj => AssetDatabase.Contains(obj)))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    DragAndDrop.AcceptDrag();
                }
            }
            else if (Event.current.type == EventType.DragExited)
            {
                _previewBox.visible = false;
                _previewShown = false;
            }
            _oldViewOffset = _viewOffset;
        }

        private void OnDisable()
        {
            SaveScriptable(_assetPaletteScriptable);
            rootVisualElement.Remove(_toolbar);
            rootVisualElement.Remove(_graphView);
            rootVisualElement.Remove(_pathBox);
            AssetPaletteListEditorWindow.HideWindow();
        }

        private void OnLostFocus()
        {
            SaveScriptable(_assetPaletteScriptable);
        }

        public void SaveScriptable(AssetPaletteScriptable assetPaletteScriptable)
        {
            _assetPaletteScriptable.zoom = _graphView.scale;
            _assetPaletteScriptable.viewOffset = _viewOffset;
            foreach (AssetPaletteElement assetPaletteElement in _assetPaletteElements)
            {
                _assetPaletteScriptable.assetPaletteElements[assetPaletteElement.guid] = assetPaletteElement.GetSaveData();
            }
            EditorUtility.SetDirty(_assetPaletteScriptable);
        }

        public List<AssetPaletteElement> GetAssetPaletteElement()
        {
            List<AssetPaletteElement> assetPaletteElements = new List<AssetPaletteElement>();
            foreach (AssetPaletteElement assetPaletteElement in _assetPaletteElements)
            {
                assetPaletteElements.Add(assetPaletteElement);
            }
            return assetPaletteElements;
        }

        public void LoadScriptable(AssetPaletteScriptable assetPaletteScriptable)
        {
            List<AssetPaletteElement> assetPaletteElements = new List<AssetPaletteElement>();
            foreach (AssetPaletteElement assetPaletteElement in _assetPaletteElements)
            {
                assetPaletteElement.parent.Remove(assetPaletteElement);
            }
            foreach (KeyValuePair<string, System.Tuple<Object, Vector2, float>> assetPaletteElement in assetPaletteScriptable.assetPaletteElements)
            {
                AssetPaletteElement element = new AssetPaletteElement(assetPaletteElement.Key, assetPaletteElement.Value.Item1, assetPaletteElement.Value.Item2, assetPaletteElement.Value.Item3);
                assetPaletteElements.Add(element);
                AddAssetPaletteElement(element);
            }
            _assetPaletteElements = assetPaletteElements;
            _assetPaletteScriptable = assetPaletteScriptable;

            _graphView.viewTransform.position = _assetPaletteScriptable.viewOffset;
        }

        public void FocusOnAssetPaletteElement(AssetPaletteElement assetPaletteElement)
        {
            //get window rect
            Rect windowRect = new Rect(position.x, position.y, position.width, position.height);
            _graphView.viewTransform.position -= assetPaletteElement.transform.position - (new Vector3(windowRect.size.x, windowRect.size.y) / 2);
        }

        private void _OnAssetPaletteElementRemoved(AssetPaletteElement assetPaletteElement)
        {
            _assetPaletteElements.Remove(assetPaletteElement);
            _assetPaletteScriptable.assetPaletteElements.Remove(assetPaletteElement.guid);
        }

        private void OnViewMoved(GraphView graphView)
        {

        }

        private void _OnAssetPaletteElementSelected(AssetPaletteElement assetPaletteElement)
        {
            _pathLabel.text = AssetDatabase.GetAssetPath(assetPaletteElement.asset);
            //move the elements to the last
            _assetPaletteElements.Remove(assetPaletteElement);
            _assetPaletteElements.Add(assetPaletteElement);
        }

        private void OnDragPerformEvent(DragPerformEvent evt)
        {
            if ((DragAndDrop.GetGenericData("fromAssetPalette") as bool?) == true)
            {
                _previewBox.visible = false;
                _previewShown = false;
                AssetPaletteElement assetPaletteElement = DragAndDrop.GetGenericData("owner") as AssetPaletteElement;
                assetPaletteElement.position += (evt.mousePosition - assetPaletteElement.clickedPosition) / AssetPaletteElement.zoom;
                assetPaletteElement.SetPosition();
                Event.current.Use();
            }
            else if (DragAndDrop.objectReferences.Any(obj => AssetDatabase.Contains(obj)))
            {
                List<Object> DDList = DragAndDrop.objectReferences.ToList();
                Vector2 positionSlide = new Vector2(0, -20);
                foreach (Object DDObject in DDList)
                {
                    if (AssetDatabase.Contains(DDObject))
                    {
                        AssetPaletteElement palettoElement = AddAssetPaletteElement(DDObject, Vector2.zero);
                        palettoElement.position = ((evt.mousePosition - _graphView.contentRect.position - _viewOffset + positionSlide) / AssetPaletteElement.zoom) - (palettoElement.rect.size * AssetPaletteElement.zoom / 2);
                        positionSlide += new Vector2(20, 20);
                    }
                }
                Event.current.Use();
            }
        }

        public void AddAssetPaletteElement(AssetPaletteElement assetPaletteElement)
        {
            _assetPaletteElements.Add(assetPaletteElement);
            if (!_assetPaletteScriptable.assetPaletteElements.ContainsKey(assetPaletteElement.guid))
            {
                _assetPaletteScriptable.assetPaletteElements.Add(assetPaletteElement.guid, assetPaletteElement.GetSaveData());
            }
            assetPaletteElement.onRemove += _OnAssetPaletteElementRemoved;
            assetPaletteElement.onClicked += _OnAssetPaletteElementSelected;
            _graphView.Add(assetPaletteElement);
        }

        public AssetPaletteElement AddAssetPaletteElement(string guid, Object asset, Vector2 position, float scale = 1)
        {
            AssetPaletteElement element = new AssetPaletteElement(guid, asset, position, scale);
            AddAssetPaletteElement(element);
            return element;
        }

        public AssetPaletteElement AddAssetPaletteElement(Object asset, Vector2 position, float scale = 1)
        {
            string guid = System.Guid.NewGuid().ToString();
            AssetPaletteElement element = AddAssetPaletteElement(guid, asset, position, scale);
            return element;
        }
    }

    class AssetPaletteToolbar : Toolbar
    {
        private AssetPaletteEditorWindow parent;
        public System.Action<AssetPaletteToolbar> onListViewClicked;
        public System.Action<AssetPaletteScriptable> onChangePaletteSelected;
        public System.Action<AssetPaletteScriptable> onNewPaletteSelected;
        public System.Action<AssetPaletteToolbar> onResetViewClicked;
        public bool hasUpdate;
        public string updateURL = "https://piyoryyta.booth.pm/items/4477987";
        public AssetPaletteToolbar(AssetPaletteEditorWindow assetPaletteEditorWindow)
        {
            parent = assetPaletteEditorWindow;
            Refresh();
        }
        public void Refresh()
        {
            //Get children
            List<VisualElement> childrenElements = new List<VisualElement>();
            foreach (VisualElement child in this.Children())
            {
                childrenElements.Add(child);
            }
            foreach (VisualElement visualElement in childrenElements)
            {
                Remove(visualElement);
            }
            if (parent.rootVisualElement.Contains(this))
            {
                parent.rootVisualElement.Remove(this);
            }
            ToolbarSpacer spacer = new ToolbarSpacer();
            ToolbarSpacer flexSpacer = new ToolbarSpacer() { flex = true };
            ToolbarMenu mainMenu = new ToolbarMenu() { text = "Menu" };
            mainMenu.menu.AppendAction("List View", a =>
            {
                onListViewClicked?.Invoke(this);
            }, a => DropdownMenuAction.Status.Normal);
            mainMenu.menu.AppendSeparator();
            mainMenu.menu.AppendAction("New Palette", a => { }, a => DropdownMenuAction.Status.Disabled);
            mainMenu.menu.AppendAction("Open Palette", a => { }, a => DropdownMenuAction.Status.Disabled);

            ToolbarButton update = new ToolbarButton() { text = "New Version!" };
            update.clicked += () =>
            {
                Application.OpenURL(updateURL);
            };
            ToolbarButton resetView = new ToolbarButton() { text = "Reset View" };
            resetView.clicked += () =>
            {
                onResetViewClicked?.Invoke(this);
            };

            Add(mainMenu);
            Add(spacer);
            if (hasUpdate) Add(update);
            Add(flexSpacer);
            Add(resetView);
            parent.rootVisualElement.Add(this);
        }
    }

    class AssetPaletteView : GraphView
    {
        public AssetPaletteView(Vector2? position = null, float zoom = 1) : base()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("AssetPaletteBackground"));
            GridBackground gridBackground = new GridBackground();
            Insert(0, gridBackground);
            gridBackground.StretchToParentSize();

            if (position == null)
            {
                position = Vector2.zero;
            }
            viewTransform.position = (Vector2)position;
            this.AddManipulator(new ContentDragger());
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        { }
    }
}