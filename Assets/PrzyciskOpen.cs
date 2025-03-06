using FishNet.Object;
using UnityEngine;

public class PrzyciskOpen : NetworkBehaviour 
{
    [SerializeField] private door DoorScript;
    [SerializeField] private GameObject przedmiot;
    [SerializeField] float TimeToCloseDoor = 1f;
    [SerializeField] private bool IsPermanentOpen = true;
    [SerializeField] private bool SerioPermanent = false;

    /*private void OnValidate() 
    {
        if (IsPermanentOpen) 
            TimeToCloseDoor = float.MaxValue;
        else if(TimeToCloseDoor == float.MaxValue) 
            TimeToCloseDoor = 1f;
    }*/

    private void OnTriggerExit(Collider other) 
    {
        if (SerioPermanent) return;
		if (!other.CompareTag("przycisk_open")) 
        {
            DoorScript.CmdRequestOpenDoor(false, 0f);
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
		if (!other.CompareTag("przycisk_open")) 
        {
            DoorScript.CmdRequestOpenDoor(true, -1f);
        }
    }

    public void open() 
    {
		if (IsPermanentOpen)
			TimeToCloseDoor = float.MaxValue;
		else if (TimeToCloseDoor == float.MaxValue)
			TimeToCloseDoor = 1f;

		if (DoorScript != null) 
        {
            DoorScript.CmdRequestOpenDoor(true, TimeToCloseDoor);
        }
        
        if (przedmiot != null) 
        {
            pojawItem itemScript = przedmiot.GetComponent<pojawItem>();
            if (itemScript != null) 
            {
                itemScript.ToggleItemServerRpc(true);
            }
        }
    }
}