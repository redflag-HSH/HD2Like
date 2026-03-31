using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] Button serverBtn;
    [SerializeField] Button hostBtn;
    [SerializeField] Button clientBtn;
    [SerializeField] GameObject blockingImage;

    private void Awake()
    {
        serverBtn.onClick.AddListener(() => { NetworkManager.Singleton.StartServer(); HideBlocking(); });
        hostBtn.onClick.AddListener(() => { NetworkManager.Singleton.StartHost(); HideBlocking(); });
        clientBtn.onClick.AddListener(() => { NetworkManager.Singleton.StartClient(); HideBlocking(); });
    }

    void HideBlocking()
    {
        if (blockingImage != null) blockingImage.SetActive(false);
    }
}
