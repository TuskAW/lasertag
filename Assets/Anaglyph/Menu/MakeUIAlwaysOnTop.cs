using UnityEngine;

namespace Anaglyph.Menu
{
	public class MakeUIAlwaysOnTop : MonoBehaviour
	{
		void Start()
		{
			Canvas.GetDefaultCanvasMaterial().SetInt("unity_GUIZTestMode", (int)UnityEngine.Rendering.CompareFunction.Always);
		}
	}
}