using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin
{
    //[CreateAssetMenu(fileName = "Skin", menuName = "Griffin/Skin")]
    public class GEditorSkin : ScriptableObject
    {
        private static GEditorSkin instance;
        public static GEditorSkin Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = AssetDatabase.LoadAssetAtPath<GEditorSkin>("Assets/Polaris - Low Poly Ecosystem/Polaris V2 - Low Poly Terrain Engine/Editor/Data/Skin.asset");
                    if (instance == null)
                    {
                        instance = ScriptableObject.CreateInstance<GEditorSkin>();
                    }
                }
                return instance;
            }
        }

        [SerializeField]
        private List<Texture2D> textures;
        public List<Texture2D> Textures
        {
            get
            {
                if (textures == null)
                {
                    textures = new List<Texture2D>();
                }
                return textures;
            }
            set
            {
                textures = value;
            }
        }

        public Texture2D GetTexture(string name)
        {
            return Textures.Find(t => t.name.Equals(name));
        }
    }
}
