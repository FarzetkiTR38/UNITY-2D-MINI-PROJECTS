using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class Mucevher
{
    public string ad;
    public Sprite ikon;
    public GameObject prefab; // sahneye Instantiate edilecek obje
    internal bool eslesdiMi;
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

    public enum BoardDurum {bekliyor, hareketEdiyor}

    public BoardDurum gecerliDurum = BoardDurum.hareketEdiyor;

    void Awake()
    {
        eslesmeController = Object.FindAnyObjectByType<EslesmeController>();
    }

    void Start()
    {

        tumMucevherler = new GameObject[width, height];

        DuzenleFNC();

    }
    /*
        void Update()
        {
            eslesmeController.EslemeleriBulFNC();
        }
    */
    // gerek yok zaten fonksiyonların içinde çalıştırıyoruz.

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

        // önce component'e erişiyoz
        YeniMucevher mucevherScript = YeniMucevher.GetComponent<YeniMucevher>();

        // null kontrolü daha güvenli yapıyormuş bla bla
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

    // tumMucevherler[pos.x, pos.y].GetComponent<YeniMucevher>().eslesdiMi  
    // bu şekilde erişiyoruz lazım olursa
    void EslesenMucevheriYokEt(Vector2Int pos)
    {

        if (tumMucevherler[pos.x, pos.y] != null)
        {
            if (tumMucevherler[pos.x, pos.y].GetComponent<YeniMucevher>().eslesdiMi)
            {

                Instantiate(tumMucevherler[pos.x, pos.y].GetComponent<YeniMucevher>().mucevherEffect, new Vector2(pos.x, pos.y), Quaternion.identity);
                Destroy(tumMucevherler[pos.x, pos.y]);
                tumMucevherler[pos.x, pos.y] = null;
            }
        }
    }

    public void TumEslesenMucevherleriYokEt()
    {
        for (int i = 0; i < eslesmeController.BulunanMucevherlerListe.Count; i++)
        {
            if (eslesmeController.BulunanMucevherlerListe[i] != null)
            {
                EslesenMucevheriYokEt(eslesmeController.BulunanMucevherlerListe[i].posIndex);
            }
        }

        StartCoroutine(AltaKaydirRouitine());
    }

    IEnumerator AltaKaydirRouitine()
    {
        yield return new WaitForSeconds(0.3f);

        int boslukSayac = 0;


        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {

                if (tumMucevherler[x, y] == null)
                {
                    boslukSayac++;
                }
                else if (boslukSayac > 0)
                {
                    tumMucevherler[x, y].GetComponent<YeniMucevher>().posIndex.y -= boslukSayac;
                    tumMucevherler[x, y - boslukSayac] = tumMucevherler[x, y];
                    tumMucevherler[x, y] = null;
                }

            }

            boslukSayac = 0;
        }

        StartCoroutine(BoardYenidenDoldurRouitine());



    }
    // tumMucevherler[x, y].GetComponent<YeniMucevher>().posIndex.y -= boslukSayac;

    IEnumerator BoardYenidenDoldurRouitine()
    {
        yield return new WaitForSeconds(0.5f);

        UstBosluklariDoldurFNC();

        yield return new WaitForSeconds(0.5f);

        eslesmeController.EslemeleriBulFNC();

        if (eslesmeController.BulunanMucevherlerListe.Count > 0)
        {
            yield return new WaitForSeconds(0.5f);

            TumEslesenMucevherleriYokEt();
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            gecerliDurum = BoardDurum.hareketEdiyor;
        }
    }

    void UstBosluklariDoldurFNC()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (tumMucevherler[x, y] == null)
                {
                    int rastgeleMucevher = Random.Range(0, mucevherler.Length);

                    MucevherOlustur(new Vector2Int(x, y), mucevherler[rastgeleMucevher]);
                }


            }
        }

        YanlisYerlestirmeleriKontrolEt();
    }

    void YanlisYerlestirmeleriKontrolEt()
    {
        List<YeniMucevher> bulunanMucevherlerList = new List<YeniMucevher>();

    bulunanMucevherlerList.AddRange(FindObjectsByType<YeniMucevher>(FindObjectsSortMode.None));

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (bulunanMucevherlerList.Contains(tumMucevherler[x, y].GetComponent<YeniMucevher>()))
                {
                    bulunanMucevherlerList.Remove(tumMucevherler[x, y].GetComponent<YeniMucevher>());
                }
            }
        }

        foreach (YeniMucevher mucevher in bulunanMucevherlerList)
        {
            Destroy(mucevher.gameObject);
        }
    }


}