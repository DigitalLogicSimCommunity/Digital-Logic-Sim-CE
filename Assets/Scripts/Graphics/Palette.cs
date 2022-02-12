using UnityEngine;

[CreateAssetMenu()]
public class Palette : ScriptableObject {
  public Color onCol;
  public Color offCol;
  public Color highZCol;
  public Color busColor;
  public Color selectedColor;

  public Color nonInteractableCol;
}
