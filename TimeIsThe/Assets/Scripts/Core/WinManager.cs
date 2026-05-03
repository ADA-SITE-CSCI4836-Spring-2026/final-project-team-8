using UnityEngine;

public class WinManager : MonoBehaviour
{
    public static bool HasWon { get; set; }

    public static WinManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
}
