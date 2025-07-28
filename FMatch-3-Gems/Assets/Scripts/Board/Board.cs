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

    public float mucevherHiz;

    public EslesmeController eslesmeController;

    void Awake()
    {
        eslesmeController = Object.FindAnyObjectByType<EslesmeController>();
    }

    void Start()
    {

        tumMucevherler = new GameObject[width, height];

        DuzenleFNC();

    }

    void Update()
    {
        eslesmeController.EslemeleriBulFNC();
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

                bgTile.name = "BG Tile -" + x + ", " + y;

                int rastgeleMucevher = Random.Range(0, mucevherler.Length);

                int kontrolSayaci = 0;
                while (EslesmeVarMiFNC(new Vector2Int(x, y), mucevherler[rastgeleMucevher]) && kontrolSayaci < 100)
                {
                    rastgeleMucevher = Random.Range(0, mucevherler.Length);
                    kontrolSayaci++; 
                    print(kontrolSayaci);
                }

                MucevherOlustur(new Vector2Int(x, y), mucevherler[rastgeleMucevher]);

            }
        }
    }

    void MucevherOlustur(Vector2Int pos, Mucevher olusacakMucevher)
    {
        GameObject YeniMucevher = Instantiate(olusacakMucevher.prefab, new Vector3(pos.x, pos.y, 0f), Quaternion.identity);
        YeniMucevher.transform.parent = this.transform;
        YeniMucevher.name = "Mucevher - " + pos.x + ", " + pos.y;

        tumMucevherler[pos.x, pos.y] = YeniMucevher;

        // önce component'e eriş
        YeniMucevher mucevherScript = YeniMucevher.GetComponent<YeniMucevher>();

        // null kontrolü ile birlikte güvenli kullanım
        if (mucevherScript != null)
        {
            mucevherScript.MucevheriDuzenle(pos, this);
        }
    }

    bool EslesmeVarMiFNC(Vector2Int posKontrol, Mucevher kontrolEdilenMucevher)
    {
        // Yatay kontrol
        if (posKontrol.x > 1)
        {
            YeniMucevher m1 = tumMucevherler[posKontrol.x - 1, posKontrol.y].GetComponent<YeniMucevher>();
            YeniMucevher m2 = tumMucevherler[posKontrol.x - 2, posKontrol.y].GetComponent<YeniMucevher>();

            if (m1 != null && m2 != null)
            {
                if (m1.tipi.ToString() == kontrolEdilenMucevher.ad && m2.tipi.ToString() == kontrolEdilenMucevher.ad)
                {
                    return true;
                }
            }
        }

        // Dikey kontrol
        if (posKontrol.y > 1)
        {
            YeniMucevher m1 = tumMucevherler[posKontrol.x, posKontrol.y - 1].GetComponent<YeniMucevher>();
            YeniMucevher m2 = tumMucevherler[posKontrol.x, posKontrol.y - 2].GetComponent<YeniMucevher>();

            if (m1 != null && m2 != null)
            {
                if (m1.tipi.ToString() == kontrolEdilenMucevher.ad && m2.tipi.ToString() == kontrolEdilenMucevher.ad)
                {
                    return true;
                }
            }
        }

        return false;
    }





}