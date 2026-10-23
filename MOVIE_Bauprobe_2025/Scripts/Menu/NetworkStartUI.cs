using UnityEngine;
using Unity.Netcode;

public class NetworkStartUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnGUI()
    {
        float w = 200f, h = 40f;
        float x = 10f, y = 10f;

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUI.Button(new Rect(x,y,w,h), "Host")) NetworkManager.Singleton.StartHost();
            if (GUI.Button(new Rect(x,y+h+10,w,h), "Client")) NetworkManager.Singleton.StartClient();
            if (GUI.Button(new Rect(x,y+2*(h+10),w,h), "Server")) NetworkManager.Singleton.StartServer();

        }
    }
}
