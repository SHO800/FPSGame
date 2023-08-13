using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace piyoryyta.AssetPalette
{
    public class AssetPaletteListEditorWindow : EditorWindow
    {
        private static EditorWindow _self;
        private static AssetPaletteEditorWindow _main;
        private static List<AssetPaletteElement> assetPaletteElements;
        private List<Texture> _miniThumbnails;

        private int _deleteAllPressedState = 0;

        private Vector2 _scrollPosition = new Vector2();
        public static void ShowWindow()
        {
            _self = GetWindow(typeof(AssetPaletteListEditorWindow), true, "List View - AssetPalette");
            _self.Show();
        }

        public static void HideWindow()
        {
            if (_self != null)
            {
                _self.Close();
            }
        }

        public static void SetAssetPaletteEditorWindow(AssetPaletteEditorWindow assetPaletteEditorWindow)
        {
            _main = assetPaletteEditorWindow;
            assetPaletteElements = new List<AssetPaletteElement>();
        }

        public void OnFocus()
        {
            assetPaletteElements = _main.GetAssetPaletteElement();
            _miniThumbnails = new List<Texture>();
            foreach (AssetPaletteElement assetPaletteElement in assetPaletteElements)
            {
                _miniThumbnails.Add(assetPaletteElement.assetImage);
            }
        }

        public void OnDisable()
        {
        }
        public void OnGUI()
        {
            using (EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scrollView.scrollPosition;
                foreach (AssetPaletteElement assetPaletteElement in assetPaletteElements)
                {
                    GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2));
                    GUILayout.Box(assetPaletteElement.assetImage, GUILayout.Width(EditorGUIUtility.singleLineHeight * 2), GUILayout.Height(EditorGUIUtility.singleLineHeight * 2));
                    GUILayout.Label(assetPaletteElement.asset.name);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Focus", GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
                    {
                        _main.FocusOnAssetPaletteElement(assetPaletteElement);
                    }
                    if (GUILayout.Button("Delete", GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
                    {
                        assetPaletteElement.Remove();
                        OnFocus();
                    }
                    GUILayout.EndHorizontal();
                }
                //Delete all
                GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2));
                GUILayout.FlexibleSpace();
                if (_deleteAllPressedState > 0)
                {
                    if (_deleteAllPressedState > 1 && GUILayout.Button("Press here " + (5 - _deleteAllPressedState).ToString() + " more time(s)", GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
                    {
                        _deleteAllPressedState++;
                    }
                    if (GUILayout.Button("Are you sure?", GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
                    {
                        _deleteAllPressedState = 2;
                    }
                    if (_deleteAllPressedState >= 5)
                    {
                        _deleteAllPressedState = -1;
                        foreach (AssetPaletteElement assetPaletteElement in assetPaletteElements)
                        {
                            assetPaletteElement.Remove();
                        }
                        OnFocus();
                    }
                }
                if (_deleteAllPressedState < 0)
                {
                    if (Random.value < 0.02f)
                    {
                        GUILayout.Label("︻デ═一 Bang bang!", GUILayout.Height(EditorGUIUtility.singleLineHeight * 2));
                    }
                    else
                    {
                        GUILayout.Label("Whoosh!", GUILayout.Height(EditorGUIUtility.singleLineHeight * 2));
                    }
                }
                if (GUILayout.Button("Delete All", GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
                {
                    _deleteAllPressedState = 1;
                }

            }
        }
    }
}
