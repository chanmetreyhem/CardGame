using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private Button startClientButton;
    [SerializeField] private Button startHostButton;


    private void Start()
    {
        startClientButton.onClick.AddListener(NetworkManager.singleton.StartClient);
        startHostButton.onClick.AddListener(NetworkManager.singleton.StartHost);
    }
}
