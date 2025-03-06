using UnityEngine;
using TMPro;

public class PrzyciskLiczba : MonoBehaviour
{
    public string liczba = "0";
    public TextMeshPro[] textMeshPros;

    void Start()
    {
        textMeshPros = new TextMeshPro[6];
        for (int i = 0; i < 6; i++)
        {
            GameObject textObject = new GameObject("CubeText" + i);
            textObject.transform.SetParent(transform);
            textObject.transform.localPosition = GetTextPosition(i);
            textObject.transform.localRotation = GetTextRotation(i); 

            textMeshPros[i] = textObject.AddComponent<TextMeshPro>();
            textMeshPros[i].text = liczba.ToString();
            textMeshPros[i].fontSize = 5;
            textMeshPros[i].alignment = TextAlignmentOptions.Center;

            textObject.AddComponent<MeshRenderer>();
        }
    }

    private Vector3 GetTextPosition(int index)
    {
        switch (index)
        {
            case 0: return new Vector3(0, 0, 0.6f);  // Przód
            case 1: return new Vector3(0, 0, -0.6f); // Tył
            case 2: return new Vector3(0.6f, 0, 0);  // Prawo
            case 3: return new Vector3(-0.6f, 0, 0); // Lewo
            case 4: return new Vector3(0, 0.6f, 0);  // Góra
            case 5: return new Vector3(0, -0.6f, 0); // Dół
            default: return Vector3.zero;
        }
    }

    private Quaternion GetTextRotation(int index)
    {
        switch (index)
        {
            case 0: return Quaternion.Euler(0, 180, 0); // Przód 
            case 1: return Quaternion.Euler(0, 0, 0); // Tył
            case 2: return Quaternion.Euler(0, -90, 0);  // Prawo
            case 3: return Quaternion.Euler(0, 90, 0); // Lewo
            case 4: return Quaternion.Euler(90, 0, 0); // Góra
            case 5: return Quaternion.Euler(-90, 0, 0);  // Dół
            default: return Quaternion.identity;
        }
    }
}
