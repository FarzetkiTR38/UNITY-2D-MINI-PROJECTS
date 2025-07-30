using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EslesmeController : MonoBehaviour
{
    Board board;

    public List<YeniMucevher> BulunanMucevherlerListe = new List<YeniMucevher>();

    private void Awake()
    {
        board = Object.FindAnyObjectByType<Board>();
    }

    public void EslemeleriBulFNC()
    {
        BulunanMucevherlerListe.Clear();

        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                GameObject gecerliObj = board.tumMucevherler[x, y];
                if (gecerliObj == null) continue;

                YeniMucevher gecerliMucevher = gecerliObj.GetComponent<YeniMucevher>();
                if (gecerliMucevher == null) continue;

                // X ekseni eşleşme
                if (x > 0 && x < board.width - 1)
                {
                    GameObject solObj = board.tumMucevherler[x - 1, y];
                    GameObject sagObj = board.tumMucevherler[x + 1, y];

                    if (solObj != null && sagObj != null)
                    {
                        YeniMucevher solMucevher = solObj.GetComponent<YeniMucevher>();
                        YeniMucevher sagMucevher = sagObj.GetComponent<YeniMucevher>();

                        if (solMucevher != null && sagMucevher != null &&
                            solMucevher.tipi == gecerliMucevher.tipi &&
                            sagMucevher.tipi == gecerliMucevher.tipi)
                        {
                            gecerliMucevher.eslesdiMi = true;
                            solMucevher.eslesdiMi = true;
                            sagMucevher.eslesdiMi = true;

                            BulunanMucevherlerListe.Add(gecerliMucevher);
                            BulunanMucevherlerListe.Add(solMucevher);
                            BulunanMucevherlerListe.Add(sagMucevher);
                        }
                    }
                }

                // Y ekseni eşleşme
                if (y > 0 && y < board.height - 1)
                {
                    GameObject altObj = board.tumMucevherler[x, y - 1];
                    GameObject ustObj = board.tumMucevherler[x, y + 1];

                    if (altObj != null && ustObj != null)
                    {
                        YeniMucevher altMucevher = altObj.GetComponent<YeniMucevher>();
                        YeniMucevher ustMucevher = ustObj.GetComponent<YeniMucevher>();

                        if (altMucevher != null && ustMucevher != null &&
                            altMucevher.tipi == gecerliMucevher.tipi &&
                            ustMucevher.tipi == gecerliMucevher.tipi)
                        {
                            gecerliMucevher.eslesdiMi = true;
                            altMucevher.eslesdiMi = true;
                            ustMucevher.eslesdiMi = true;

                            BulunanMucevherlerListe.Add(gecerliMucevher);
                            BulunanMucevherlerListe.Add(altMucevher);
                            BulunanMucevherlerListe.Add(ustMucevher);
                        }
                    }
                }
            }
        }

        if (BulunanMucevherlerListe.Count > 0)
        {
            BulunanMucevherlerListe = BulunanMucevherlerListe.Distinct().ToList();
        }


        BombayiBulFNC();
    }

    public void BombayiBulFNC()
    {
        for (int i = 0; i < BulunanMucevherlerListe.Count; i++)
        {
            YeniMucevher mucevher = BulunanMucevherlerListe[i];
            int x = mucevher.posIndex.x;
            int y = mucevher.posIndex.y;

            // Sol
            if (x > 0)
            {
                GameObject sol = board.tumMucevherler[x - 1, y];
                if (sol != null && sol.GetComponent<YeniMucevher>().tipi == YeniMucevher.MucevherTipi.bomba)
                {
                    YeniMucevher bomba = board.tumMucevherler[x, y].GetComponent<YeniMucevher>();
                    if (bomba != null)
                    {
                        BombaBolgesiniIsaretle(new Vector2Int(x, y), bomba.bombaHacmi);
                    }
                }
            }

            // Sağ
            if (x < board.width - 1)
            {
                GameObject sag = board.tumMucevherler[x + 1, y];
                if (sag != null && sag.GetComponent<YeniMucevher>().tipi == YeniMucevher.MucevherTipi.bomba)
                {
                    YeniMucevher bomba = board.tumMucevherler[x, y].GetComponent<YeniMucevher>();
                    if (bomba != null)
                    {
                        BombaBolgesiniIsaretle(new Vector2Int(x, y), bomba.bombaHacmi);
                    }
                }
            }

            // Alt
            if (y > 0)
            {
                GameObject alt = board.tumMucevherler[x, y - 1];
                if (alt != null && alt.GetComponent<YeniMucevher>().tipi == YeniMucevher.MucevherTipi.bomba)
                {
                    YeniMucevher bomba = board.tumMucevherler[x, y].GetComponent<YeniMucevher>();
                    if (bomba != null)
                    {
                        BombaBolgesiniIsaretle(new Vector2Int(x, y), bomba.bombaHacmi);
                    }
                }
            }

            // Üst
            if (y < board.height - 1)
            {
                GameObject ust = board.tumMucevherler[x, y + 1];
                if (ust != null && ust.GetComponent<YeniMucevher>().tipi == YeniMucevher.MucevherTipi.bomba)
                {
                    YeniMucevher bomba = board.tumMucevherler[x, y].GetComponent<YeniMucevher>();
                    if (bomba != null)
                    {
                        BombaBolgesiniIsaretle(new Vector2Int(x, y), bomba.bombaHacmi);
                    }
                }
            }



            
        }
    }

    public void BombaBolgesiniIsaretle(Vector2Int bombaPos, int hacim)
    {

        for (int x = bombaPos.x - hacim; x <= bombaPos.x + hacim; x++)
        {
            for (int y = bombaPos.y - hacim; y <= bombaPos.y + hacim; y++)
            {
                if (x >= 0 && x < board.width && y >= 0 && y < board.height)
                {
                    GameObject obj = board.tumMucevherler[x, y];
                    if (obj != null)
                    {
                        YeniMucevher muc = obj.GetComponent<YeniMucevher>();
                        if (muc != null)
                        {
                            muc.eslesdiMi = true;
                            BulunanMucevherlerListe.Add(muc);
                        }
                    }
                }
            }
        }
        
        if (BulunanMucevherlerListe.Count > 0)
        {
            BulunanMucevherlerListe = BulunanMucevherlerListe.Distinct().ToList();
        }
    }

}
