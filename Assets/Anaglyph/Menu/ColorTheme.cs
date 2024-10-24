using Anaglyph;
using UnityEngine;
using UnityEngine.UI;

namespace Anaglyph.Menu
{
    [RequireComponent(typeof(Image))]
    public class ColorTheme : MonoBehaviour
    {
        [SerializeField] private ColorObject colorEntry;
        
        private void OnValidate()
        {
            Image image = GetComponent<Image>();

            image.color = colorEntry.Value;
        }
    }
}