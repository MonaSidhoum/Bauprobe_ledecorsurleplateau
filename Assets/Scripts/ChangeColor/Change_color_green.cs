using UnityEngine.UI;
using UnityEngine;

public class Change_color_green : MonoBehaviour
{
    public void SwitchColor()
    {
        this.GetComponent<Image>().color = Color.green;
    }
}
