using UnityEngine;
using System.Collections.Generic;

public class CellManager : MonoBehaviour
{
    public GameObject cellPrefab;
    public float splitForce = 5f;
    public int maxCells = 16;

    public List<GameObject> cells = new List<GameObject>();

    void Update()
    {
        Debug.Log("Update çalışıyor!");
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TrySplitAll();
        }
    }

    public void RegisterInitialCell(GameObject playerCell)
    {
        cells.Add(playerCell);
    }

    void TrySplitAll()
    {
        Debug.Log("Split called! Cell count: " + cells.Count);
        if (cells.Count >= maxCells) return;

        List<GameObject> newCells = new List<GameObject>();

        foreach (GameObject cellObj in new List<GameObject>(cells))
        {
            Cell cell = cellObj.GetComponent<Cell>();
            if (cell == null) continue;

            float currentMass = cell.GetMass();
            Debug.Log("Current mass: " + currentMass);
            if (currentMass < 2f) continue;

            float newMass = currentMass / 2f;
            cell.SetMass(newMass);

            // Mouse yönü
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;
            Vector3 direction = (mouseWorld - cellObj.transform.position).normalized;
            float cellRadius = cellObj.transform.localScale.x * 0.5f;
            Vector3 spawnOffset = direction * (cellRadius * 2f + 0.05f); // Yan yana, çakışmasın
            Vector3 spawnPos = cellObj.transform.position + spawnOffset;

            // Oyuncu prefab'ını kullan (cellPrefab değil)
            GameObject newCell = Instantiate(GameManager.instance.player, spawnPos, Quaternion.identity);

            // Componentler aktif ediliyor
            Cell newCellScript = newCell.GetComponent<Cell>();
            newCellScript.SetMass(newMass);

            SizeManager sm = newCell.GetComponent<SizeManager>();
            if (sm != null) sm.enabled = true;

            Movement mv = newCell.GetComponent<Movement>();
            if (mv != null) mv.enabled = true;

            Rigidbody2D rb = newCell.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(direction * splitForce, ForceMode2D.Impulse);
            }

            newCells.Add(newCell);
        }

        cells.AddRange(newCells);
    }

}
