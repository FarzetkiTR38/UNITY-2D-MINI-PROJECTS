using UnityEngine;

public class PlayerSwordSkill : MonoBehaviour
{
    public Transform swordAnchor;
    public GameObject swordPrefab;

    private GameObject activeSword;

    public void ActivateSword()
    {
        if (activeSword != null) return;

        activeSword = Instantiate(swordPrefab, swordAnchor);
        activeSword.transform.localPosition = Vector3.right * 1.5f;
    }

    public void Upgrade(int level)
    {
        if (activeSword == null)
            ActivateSword();

        OrbitMovement orbit = activeSword.GetComponent<OrbitMovement>();
        orbit.rotationSpeed = 180f + level * 60f;
        orbit.radius = 1.5f + level * 0.2f;
    }
}
