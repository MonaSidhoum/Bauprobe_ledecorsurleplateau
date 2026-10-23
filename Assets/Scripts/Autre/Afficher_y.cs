
using UnityEngine;
using UnityEngine.InputSystem;


public class Afficher_y : MonoBehaviour
{
    public GameObject menuUI;
    public InputActionReference ybutton;

    void Start()
    {
        // S'assure que le menu est d�sactiv� au d�part
        if (menuUI != null)
            menuUI.SetActive(false);
    }

    private void OnEnable()
    {
        if (ybutton != null)
        {
            ybutton.action.performed += ToggleMenu;
        }
    }

    private void OnDisable()
    {
        if (ybutton != null)
        {
            ybutton.action.performed -= ToggleMenu;
        }
    }

    public void ToggleMenu(InputAction.CallbackContext context)
    {
        if (menuUI != null)
        {
            bool isActive = menuUI.activeSelf;

            // Trouver les menus
            GameObject menuprin = menuUI.transform.Find("Menu principal").gameObject;
            GameObject menusec = menuUI.transform.Find("Menu secondaire").gameObject;
            menuUI.SetActive(!isActive);
            menuprin.SetActive(true);

            menusec.SetActive(false);

            Debug.Log("Menu " + (isActive ? "ferm�" : "ouvert"));
        }
    }
}