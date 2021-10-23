using UnityEngine;

public class VaultDoor : MonoBehaviour
{
    public Animator doorAnim;
    bool open = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            if (open)
            {
                open = false;
                doorAnim.SetTrigger("close");
            }
    }
}
