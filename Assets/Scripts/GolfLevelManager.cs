using UnityEngine;

[System.Serializable]
public class GolfLevel
{
    public GameObject root;
    public GolfHole hole;
    public Transform ballStart;
}

public class GolfLevelManager : MonoBehaviour
{
    [SerializeField] private GolfBallController ball;
    [SerializeField] private GolfLevel[] levels;

    public int CurrentIndex { get; private set; }
    public int LevelCount => levels.Length;
    public bool IsLastLevel => CurrentIndex >= levels.Length - 1;

    private void Start()
    {
        LoadLevel(0);
    }

    public void LoadLevel(int index)
    {
        CurrentIndex = index;

        for (int i = 0; i < levels.Length; i++)
            levels[i].root.SetActive(i == index);

        levels[index].hole.ResetDetection();
        ball.SetStartPoint(levels[index].ballStart);
        ball.RestartHole();
    }

    public void NextLevel()
    {
        if (!IsLastLevel)
            LoadLevel(CurrentIndex + 1);
    }

    public void RestartLevel()
    {
        LoadLevel(CurrentIndex);
    }
}