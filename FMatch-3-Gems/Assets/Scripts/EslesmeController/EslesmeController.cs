using System.Collections.Generic;
using Unity.VisualScripting;
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
                YeniMucevher gecerliMucevher = board.tumMucevherler[x, y].GetComponent<YeniMucevher>();

                if (gecerliMucevher != null)
                {
                    // x ekseni eşleşme kontrolü
                    if (x > 0 && x < board.width - 1)
                    {
                        YeniMucevher solMucevher = board.tumMucevherler[x - 1, y].GetComponent<YeniMucevher>();
                        YeniMucevher sagMucevher = board.tumMucevherler[x + 1, y].GetComponent<YeniMucevher>();

                        if (solMucevher != null && sagMucevher != null)
                        {
                            if (solMucevher.tipi == gecerliMucevher.tipi && sagMucevher.tipi == gecerliMucevher.tipi)
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

                    // y ekseni eşleşme kontrolü
                    if (y > 0 && y < board.height - 1)
                    {
                        YeniMucevher altMucevher = board.tumMucevherler[x, y - 1].GetComponent<YeniMucevher>();
                        YeniMucevher ustMucevher = board.tumMucevherler[x, y + 1].GetComponent<YeniMucevher>();

                        if (altMucevher != null && ustMucevher != null)
                        {
                            if (altMucevher.tipi == gecerliMucevher.tipi && ustMucevher.tipi == gecerliMucevher.tipi)
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
        }
        // listede aynı elemandan 2 tane olmaması için gereken kod:
        if (BulunanMucevherlerListe.Count > 0)
        {
            BulunanMucevherlerListe = BulunanMucevherlerListe.Distinct().ToList();
        }

    }


}
