using BoxEd;
using UnityEngine;

public class Localiser : MonoBehaviour
{
	private void Awake()
	{
		Localisation.LoadLanguage("en");
	}
}