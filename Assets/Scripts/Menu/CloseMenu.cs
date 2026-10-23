using UnityEditor;
using UnityEngine;

public class CloseMenu : MonoBehaviour
{

    public void Closemenu()
    {

        // Trouver les menus
        GameObject menuprin = transform.Find("Menu principal").gameObject;
        GameObject menusec = transform.Find("Menu secondaire").gameObject;
        GameObject Tutomove = transform.Find("Tuto_Move").gameObject;
        GameObject Tutodelete = transform.Find("Tuto_Delete").gameObject;
        GameObject Tutotele = transform.Find("Tuto_Teleport").gameObject;
        GameObject Tutoscale = transform.Find("Tuto_Scale").gameObject;
        this.gameObject.SetActive(false);

        menuprin.SetActive(true);

        //D�sactiver ce qu'il ne faut pas activer
        Tutomove.SetActive(false);
        Tutodelete.SetActive(false);
        Tutotele.SetActive(false);
        Tutoscale.SetActive(false);
        menusec.SetActive(false);

        Debug.Log("Menu ferme");
    }
}
