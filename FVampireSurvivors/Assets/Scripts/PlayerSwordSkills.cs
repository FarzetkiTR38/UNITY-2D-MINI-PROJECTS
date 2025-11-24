using Unity.VisualScripting;
using UnityEngine;

public class PlayerSwordSkill : MonoBehaviour
{
    public Transform swordAnchor;
    public GameObject swordPrefab;

    private GameObject activeSword;

    private bool swordBool = false;

    public void ActivateSword()
    {
        if (activeSword != null) return;

        activeSword = Instantiate(swordPrefab, swordAnchor);
        activeSword.transform.localPosition = Vector3.zero;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && !swordBool)
        {
            ActivateSword();
            swordBool = true;
        }
        else if(Input.GetKeyDown(KeyCode.Alpha1) && swordBool)
        {
            DeactivateSword();
            swordBool = false;
        }
    }

    public void DeactivateSword()
    {
        if (activeSword != null)
            Destroy(activeSword);
    }
}