using FishNet.Object;
using UmnieDziala.Game.Items;
using UnityEngine;

public class pojawItem : NetworkBehaviour
{
    private ItemBase item;
	private void Awake()
	{
        TryGetComponent(out item);
	}
	public override void OnStartServer()
    {
        base.OnStartServer();
        //gameObject.SetActive(false); // Domyślnie ukryty
        /*if (item != null)
        {
            item.
        }*/
        SetActiveClientRpc(false);

	}

    [ServerRpc(RequireOwnership = false)]
    public void ToggleItemServerRpc(bool state)
    {
        gameObject.SetActive(state);
       
        SetActiveClientRpc(state); 
    }

    [ObserversRpc (BufferLast =true, RunLocally =true)] 
    private void SetActiveClientRpc(bool state)
    {
        gameObject.SetActive(state);
    }
}
