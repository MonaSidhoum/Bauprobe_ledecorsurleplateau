using UnityEngine;
using UnityEngine.UI;

public class Change_color_white : MonoBehaviour
{
    public void SwitchColor()
    {
        this.GetComponent<Image>().color = Color.white;
    }
}
