using UnityEngine;

public class GameUi : MonoBehaviour
{
    [SerializeField] private GameObject hitButton;
    [SerializeField] private GameObject standButton;
    [SerializeField] private GameObject waitingButton;

    private void Start()
    {
        GameManager gm = GameManager.Instance;
    }
}
