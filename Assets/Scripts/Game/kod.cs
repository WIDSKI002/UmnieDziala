using UnityEngine;
using FishNet.Object;
using TMPro;

public class kod : NetworkBehaviour
{
    [SerializeField] private bool RandomCode = true;
    public string obecnycode;
    public TextMeshPro textMeshPro;
	public string code;
	public override void OnStartServer()
	{
		base.OnStartServer();
        
	}

	[ServerRpc(RequireOwnership = false)] 
    public void ZmianaKodu(string liczba)
    {
        if(liczba == "#"){
             obecnycode = string.Empty;
              UpdateTextMeshPro(); 
             return;
        }
        obecnycode += liczba;
        if(obecnycode.Length > code.Length)
        {
            obecnycode = string.Empty; 
        }
        UpdateTextMeshPro(); 
        if (obecnycode == code)
        {
            Debug.Log(code);
        PrzyciskOpen przyciskOpenScript = GetComponent<PrzyciskOpen>();
        if (przyciskOpenScript != null)
        {
            przyciskOpenScript.open();
        }
        }
        
    }
    [ObserversRpc(BufferLast = true, RunLocally =true)] // observersrpc zeby wyslalo do kazdego
    private void UpdateTextMeshPro()
    {
        if (textMeshPro != null)
        {
            textMeshPro.text = obecnycode; 
        }
    }
}
