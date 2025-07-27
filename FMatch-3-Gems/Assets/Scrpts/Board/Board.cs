using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class Mucevher
{
    public string ad;
    public Sprite ikon;
    public GameObject prefab; // sahneye Instantiate edilecek obje
}

public class Board : MonoBehaviour
{

    public int height;
    public int width;

    public GameObject tilePrefab;

    public Mucevher[] mucevherler;

    public GameObject[,] tumMucevherler; // 2 boyutlu dizi olması için [,] yapıyoruz.

    void Start()
    {

        tumMucevherler = new GameObject[width, height];

        DuzenleFNC();

    }

    void DuzenleFNC()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 pos = new Vector2(x, y);

                GameObject bgTile = Instantiate(tilePrefab, pos, Quaternion.identity);
                // Quaternion.identity açısını değiştirmemeye yarıyor 0 0 0 yani
                bgTile.transform.parent = this.transform;

                bgTile.name="BG Tile -" + x + ", "+ y;
                
                int rastgeleMucevher = Random.Range(0, mucevherler.Length);

                MucevherOlustur(new Vector2Int(x, y), mucevherler[rastgeleMucevher]);

            }
        }
    }

    void MucevherOlustur(Vector2Int pos, Mucevher olusacakMucevher)
    {
        GameObject yeniMucevher = Instantiate(olusacakMucevher.prefab, new Vector3(pos.x, pos.y, 0f), Quaternion.identity);
        yeniMucevher.transform.parent = this.transform;
        yeniMucevher.name = "Mucevher - " + pos.x + ", " + pos.y;

        tumMucevherler[pos.x, pos.y] = yeniMucevher;
    }

}
