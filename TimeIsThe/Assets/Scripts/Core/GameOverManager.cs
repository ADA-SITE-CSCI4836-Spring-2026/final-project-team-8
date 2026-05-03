using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    public static bool HasLost { get; set; }

    public static GameOverManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
}
