using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;

    public PlayerInputManager playerInput;

    private int oldPlayerCount;

    public GameObject[] playerVisuals;

    [SerializeField] private LayerMask player1Mask;
    [SerializeField] private LayerMask player2Mask;

    [SerializeField] private int playerOneMask;
    [SerializeField] private int playerTwoMask;

    [SerializeField] private GameObject _rigidbodyBear;
    private GameObject _newRigidbodyBear;

    public bool m_menuOn;

    private void Start()
    {
        instance = this;
        playerInput = FindObjectOfType<PlayerInputManager>();
        //ChangePlayerMask();
    }

    private void ChangePlayerMask()
    {
        if (playerInput.playerCount == 1)
        {

            for (int i = 0; i < playerVisuals.Length; i++)
            {
                playerVisuals[i].layer = playerOneMask;
                GetComponentInChildren<Camera>().cullingMask = player1Mask;
            }
        }
        if (playerInput.playerCount == 2)
        {
            for (int i = 0; i < playerVisuals.Length; i++)
            {
                playerVisuals[i].layer = playerTwoMask;
                GetComponentInChildren<Camera>().cullingMask = player2Mask;
            }
        }
    }

    public void TurnOnRigidbodyTeddyBear()
    {
        _newRigidbodyBear = Instantiate(_rigidbodyBear, transform.position, transform.rotation);

        for (int i = 0; i < playerVisuals.Length; i++)
        {
            playerVisuals[i].SetActive(false);
        }

        foreach (GameObject child in playerVisuals)
        {
            if (child.TryGetComponent<Rigidbody>(out Rigidbody childRb))
            {
                childRb.AddExplosionForce(50f, childRb.transform.position, 4f);
            }
        }
    }

    public void TurnOffRigidbodyTeddyBear()
    {
        for (int i = 0; i < playerVisuals.Length; i++)
        {
            playerVisuals[i].SetActive(true);
        }
        Destroy(_newRigidbodyBear, 1f);
    }

    public void InMenu()
    {
        GetComponent<Movement>().enabled = false;
        GetComponent<WallRunning>().enabled = false;
        GetComponent<Slide>().enabled = false;
        GetComponent<Climbing>().enabled = false;
        GetComponent<PlayerCam>().enabled = false;
        m_menuOn = true;
    }

    public void OutMenu()
    {
        GetComponent<Movement>().enabled = true;
        GetComponent<WallRunning>().enabled = true;
        GetComponent<Slide>().enabled = true;
        GetComponent<Climbing>().enabled = true;
        GetComponent<PlayerCam>().enabled = true;
        m_menuOn = false;
    }
}
