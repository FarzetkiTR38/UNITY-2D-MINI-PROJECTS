using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.ReorderableList.Element_Adder_Menu;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SistemKodu : MonoBehaviour
{
    [Header("Yazı Objeleri")]

    public TMP_Text paraText;
    public TMP_Text artisSaniyeText;

    public TMP_Text tiklamaBasinaParaText;

    public TMP_Text rebirthText;

    public TMP_Text elmasText;
    // // // // // // // // // // // 
    private bool iscibool = false;
    private bool ciftcibool = false;
    private bool madencibool = false;
    private bool robotbool = false;
    private bool dukkanbool = false;
    private bool fabrikabool = false;
    private bool tedarikcibool = false;
    private bool bankabool = false;
    private bool sirketbool = false;
    private bool teknolojimerkezibool = false;
    private bool uzaymadencisibool = false;






    private float para;

    private float Para
    {
        get
        {
            return para;
        }

        set
        {
            para = value;
            paraText.text = para.ToString("0");
            //paraText.text = "PARA: " + para.ToString("0.00") + " TL";
        }
    }

    [Header("Tıklama Bilgi")]

    [Tooltip("Oyuncunun her tıkladığında kazanacağı para miktarı")]

    public float tiklamaBasinaPara = 1;

    private float TiklamaBasinaPara
    {
        get
        {
            return tiklamaBasinaPara;
        }

        set
        {
            tiklamaBasinaPara = value;
            tiklamaBasinaParaText.text = tiklamaBasinaPara.ToString("0") + " TL / CLICK";
        }
    }

    public float saniyeBasinaPara;

    private float SaniyeBasinaPara
    {
        get
        {
            return saniyeBasinaPara;
        }

        set
        {
            saniyeBasinaPara = value;
            artisSaniyeText.text = saniyeBasinaPara.ToString("0") + " TL / S";
            //artisSaniyeText.text = "Saniyedeki Kazanç: " + saniyeBasinaPara.ToString("0.00") + " TL";
        }
    }


    private float elmas;

    private float Elmas
    {
        get
        {
            return elmas;
        }

        set
        {
            elmas = value;
            elmasText.text = elmas.ToString("0");
        }
    }


    float orijinalSaniyePara;
    float orijinalTiklamaPara;



    //elmas fnc için gereken sayaç:
    private float elmaszamanlayici = 1f; // 1 saniyelik sayaç
    private float parazamanlayici = 1f; // 1 saniyelik sayaç
    private float rebirthzamanlayici = 1f; // 1 saniyelik sayaç

    private bool kontrolYapildiMi = false;



    private float LuckyClickChange;

    private float totalClickAmount; // stats da belirtmek için total tıklama sayısını hesaplatcaz

    private float totalEarnedMoney; // stats da göstermek için toplam kazanılan parayı hesaplatcaz

    public void TusaBasinca()
    {

        Para += tiklamaBasinaPara;

        totalEarnedMoney += tiklamaBasinaPara;

        LuckyClickChange = Random.Range(0, 100);

        if (LuckyClickChange < 10)
        {
            Para += tiklamaBasinaPara * 4; // 4 ile çarpma sebebim 1x yukarıda veriliyor zaten eğer şanslıysa 4x daha verilecek ve 5x para almış olacak yoksa 1x 
            totalEarnedMoney += tiklamaBasinaPara * 4;
        }

        totalClickAmount += 1;

    }


    // UI referansları
    public TextMeshProUGUI isciGerekenPara, ciftciGerekenPara, madenciGerekenPara,
                          robotGerekenPara, dukkanGerekenPara, fabrikaGerekenPara,
                          tedarikciGerekenPara, bankaGerekenPara, sirketGerekenPara,
                          teknolojimerkeziGerekenPara, uzaymadencisiGerekenPara;

    // --------------------------------------------------------------------------------------------- //

    float isciPara = 10;
    float ciftciPara = 50;
    float madenciPara = 200;
    float robotPara = 1000;
    float dukkanPara = 5000;
    float fabrikaPara = 12000;
    float tedarikciPara = 50000;
    float bankaPara = 150000;
    float sirketPara = 500000;
    float teknolojimerkeziPara = 1500000;
    float uzaymadencisiPara = 5000000;

    private void IsciParalariFNC() // rebirth sonrası bu fnc çalışıyor ki işçi paraları resetlensin.
    {
        isciPara = 10;
        ciftciPara = 50;
        madenciPara = 200;
        robotPara = 1000;
        dukkanPara = 5000;
        fabrikaPara = 12000;
        tedarikciPara = 50000;
        bankaPara = 150000;
        sirketPara = 500000;
        teknolojimerkeziPara = 1500000;
        uzaymadencisiPara = 5000000;

        iscibool = false;
        ciftcibool = false;
        madencibool = false;
        robotbool = false;
        dukkanbool = false;
        fabrikabool = false;
        tedarikcibool = false;
        bankabool = false;
        sirketbool = false;
        teknolojimerkezibool = false;
        uzaymadencisibool = false;

        isciGerekenPara.text = isciPara.ToString("F0") + " TL";
        ciftciGerekenPara.text = ciftciPara.ToString("F0") + " TL";
        madenciGerekenPara.text = madenciPara.ToString("F0") + " TL";
        robotGerekenPara.text = robotPara.ToString("F0") + " TL";
        dukkanGerekenPara.text = dukkanPara.ToString("F0") + " TL";
        fabrikaGerekenPara.text = fabrikaPara.ToString("F0") + " TL";
        tedarikciGerekenPara.text = tedarikciPara.ToString("F0") + " TL";
        bankaGerekenPara.text = bankaPara.ToString("F0") + " TL";
        sirketGerekenPara.text = sirketPara.ToString("F0") + " TL";
        teknolojimerkeziGerekenPara.text = teknolojimerkeziPara.ToString("F0") + " TL";
        uzaymadencisiGerekenPara.text = uzaymadencisiPara.ToString("F0") + " TL";
    }

    // Güncellenmiş fonksiyonlar:
    public void isci()
    {

        if (!iscibool && Para >= 10)
        {
            iscibool = true;
            Para -= 10;

            if (!iksirEtkisiAktif)
            {
                saniyeBasinaPara += 1;
                orijinalSaniyePara = saniyeBasinaPara; // güncelle
            }
            else
            {
                orijinalSaniyePara += 1; // iksir aktifken sadece orijinal güncellenir
                saniyeBasinaPara = orijinalSaniyePara * 2;
            }

            isciGerekenPara.text = (isciPara * 2.5f).ToString("F0") + " TL";
        }
        else if (iscibool && Para >= isciPara * 2.5f)
        {
            if (!iksirEtkisiAktif)
            {
                saniyeBasinaPara += 1;
                orijinalSaniyePara = saniyeBasinaPara; // güncelle
            }
            else
            {
                orijinalSaniyePara += 1; // iksir aktifken sadece orijinal güncellenir
                saniyeBasinaPara = orijinalSaniyePara * 2;
            }

            Para -= isciPara * 2.5f;
            isciPara *= 2.5f;
            isciGerekenPara.text = (isciPara * 2.5f).ToString("F0") + " TL";
        }

    }

    public void ciftci()
    {
        if (!ciftcibool && Para >= 50)
        {
            ciftcibool = true;
            Para -= 50;

            if (!iksirEtkisiAktif)
            {
                saniyeBasinaPara += 5;
                orijinalSaniyePara = saniyeBasinaPara;
            }
            else
            {
                orijinalSaniyePara += 5;
                saniyeBasinaPara = orijinalSaniyePara * 2;
            }

            ciftciGerekenPara.text = (ciftciPara * 2.5f).ToString("F0") + " TL";
        }
        else if (ciftcibool && Para >= ciftciPara * 2.5f)
        {
            Para -= ciftciPara * 2.5f;

            if (!iksirEtkisiAktif)
            {
                saniyeBasinaPara += 5;
                orijinalSaniyePara = saniyeBasinaPara;
            }
            else
            {
                orijinalSaniyePara += 5;
                saniyeBasinaPara = orijinalSaniyePara * 2;
            }

            ciftciPara *= 2.5f;
            ciftciGerekenPara.text = (ciftciPara * 2.5f).ToString("F0") + " TL";
        }
    }

    public void madenci()
    {
        if (!madencibool && Para >= 200)
        {
            madencibool = true;
            Para -= 200;

            if (!iksirEtkisiAktif)
            {
                saniyeBasinaPara += 15;
                orijinalSaniyePara = saniyeBasinaPara;
            }
            else
            {
                orijinalSaniyePara += 15;
                saniyeBasinaPara = orijinalSaniyePara * 2;
            }

            madenciGerekenPara.text = (madenciPara * 2.5f).ToString("F0") + " TL";
        }
        else if (madencibool && Para >= madenciPara * 2.5f)
        {
            Para -= madenciPara * 2.5f;

            if (!iksirEtkisiAktif)
            {
                saniyeBasinaPara += 15;
                orijinalSaniyePara = saniyeBasinaPara;
            }
            else
            {
                orijinalSaniyePara += 15;
                saniyeBasinaPara = orijinalSaniyePara * 2;
            }

            madenciPara *= 2.5f;
            madenciGerekenPara.text = (madenciPara * 2.5f).ToString("F0") + " TL";
        }
    }

    public void robot()
    {
        if (!robotbool && Para >= 1000)
        {
            robotbool = true;
            Para -= 1000;

            if (!iksirEtkisiAktif)
            {
                saniyeBasinaPara += 80;
                orijinalSaniyePara = saniyeBasinaPara;
            }
            else
            {
                orijinalSaniyePara += 80;
                saniyeBasinaPara = orijinalSaniyePara * 2;
            }

            robotGerekenPara.text = (robotPara * 2.5f).ToString("F0") + " TL";
        }
        else if (robotbool && Para >= robotPara * 2.5f)
        {
            Para -= robotPara * 2.5f;

            if (!iksirEtkisiAktif)
            {
                saniyeBasinaPara += 80;
                orijinalSaniyePara = saniyeBasinaPara;
            }
            else
            {
                orijinalSaniyePara += 80;
                saniyeBasinaPara = orijinalSaniyePara * 2;
            }

            robotPara *= 2.5f;
            robotGerekenPara.text = (robotPara * 2.5f).ToString("F0") + " TL";
        }
    }

    public void dukkan()
    {
        if (!dukkanbool && Para >= 5000)
        {
            dukkanbool = true;
            Para -= 5000;

            if (!iksirEtkisiAktif)
            {
                saniyeBasinaPara += 400;
                orijinalSaniyePara = saniyeBasinaPara;
            }
            else
            {
                orijinalSaniyePara += 400;
                saniyeBasinaPara = orijinalSaniyePara * 2;
            }

            dukkanGerekenPara.text = (dukkanPara * 2.5f).ToString("F0") + " TL";
        }
        else if (dukkanbool && Para >= dukkanPara * 2.5f)
        {
            Para -= dukkanPara * 2.5f;

            if (!iksirEtkisiAktif)
            {
                saniyeBasinaPara += 400;
                orijinalSaniyePara = saniyeBasinaPara;
            }
            else
            {
                orijinalSaniyePara += 400;
                saniyeBasinaPara = orijinalSaniyePara * 2;
            }

            dukkanPara *= 2.5f;
            dukkanGerekenPara.text = (dukkanPara * 2.5f).ToString("F0") + " TL";
        }
    }
    public void fabrika()
    {
        if (!fabrikabool && Para >= 12000)
        {
            fabrikabool = true;
            Para -= 12000;

            if (!iksirEtkisiAktif)
            {
                saniyeBasinaPara += 1000;
                orijinalSaniyePara = saniyeBasinaPara;
            }
            else
            {
                orijinalSaniyePara += 1000;
                saniyeBasinaPara = orijinalSaniyePara * 2;
            }

            fabrikaGerekenPara.text = (fabrikaPara * 2.5f).ToString("F0") + " TL";
        }
        else if (fabrikabool && Para >= fabrikaPara * 2.5f)
        {
            Para -= fabrikaPara * 2.5f;

            if (!iksirEtkisiAktif)
            {
                saniyeBasinaPara += 1000;
                orijinalSaniyePara = saniyeBasinaPara;
            }
            else
            {
                orijinalSaniyePara += 1000;
                saniyeBasinaPara = orijinalSaniyePara * 2;
            }

            fabrikaPara *= 2.5f;
            fabrikaGerekenPara.text = (fabrikaPara * 2.5f).ToString("F0") + " TL";
        }
    }

    public void tedarikci()
    {
        if (!tedarikcibool && Para >= 50000)
        {
            tedarikcibool = true;
            Para -= 50000;

            if (!iksirEtkisiAktif)
            {
                saniyeBasinaPara += 4500;
                orijinalSaniyePara = saniyeBasinaPara;
            }
            else
            {
                orijinalSaniyePara += 4500;
                saniyeBasinaPara = orijinalSaniyePara * 2;
            }

            tedarikciGerekenPara.text = (tedarikciPara * 2.5f).ToString("F0") + " TL";
        }
        else if (tedarikcibool && Para >= tedarikciPara * 2.5f)
        {
            Para -= tedarikciPara * 2.5f;

            if (!iksirEtkisiAktif)
            {
                saniyeBasinaPara += 4500;
                orijinalSaniyePara = saniyeBasinaPara;
            }
            else
            {
                orijinalSaniyePara += 4500;
                saniyeBasinaPara = orijinalSaniyePara * 2;
            }

            tedarikciPara *= 2.5f;
            tedarikciGerekenPara.text = (tedarikciPara * 2.5f).ToString("F0") + " TL";
        }
    }

    public void banka()
    {
        if (!bankabool && Para >= 150000)
        {
            bankabool = true;
            Para -= 150000;

            if (!iksirEtkisiAktif)
            {
                saniyeBasinaPara += 10000;
                orijinalSaniyePara = saniyeBasinaPara;
            }
            else
            {
                orijinalSaniyePara += 10000;
                saniyeBasinaPara = orijinalSaniyePara * 2;
            }

            bankaGerekenPara.text = (bankaPara * 2.5f).ToString("F0") + " TL";
        }
        else if (bankabool && Para >= bankaPara * 2.5f)
        {
            Para -= bankaPara * 2.5f;

            if (!iksirEtkisiAktif)
            {
                saniyeBasinaPara += 10000;
                orijinalSaniyePara = saniyeBasinaPara;
            }
            else
            {
                orijinalSaniyePara += 10000;
                saniyeBasinaPara = orijinalSaniyePara * 2;
            }

            bankaPara *= 2.5f;
            bankaGerekenPara.text = (bankaPara * 2.5f).ToString("F0") + " TL";
        }
    }

    public void sirket()
    {
        if (!sirketbool && Para >= 500000)
        {
            sirketbool = true;
            Para -= 500000;

            if (!iksirEtkisiAktif)
            {
                saniyeBasinaPara += 40000;
                orijinalSaniyePara = saniyeBasinaPara;
            }
            else
            {
                orijinalSaniyePara += 40000;
                saniyeBasinaPara = orijinalSaniyePara * 2;
            }

            sirketGerekenPara.text = (sirketPara * 2.5f).ToString("F0") + " TL";
        }
        else if (sirketbool && Para >= sirketPara * 2.5f)
        {
            Para -= sirketPara * 2.5f;

            if (!iksirEtkisiAktif)
            {
                saniyeBasinaPara += 40000;
                orijinalSaniyePara = saniyeBasinaPara;
            }
            else
            {
                orijinalSaniyePara += 40000;
                saniyeBasinaPara = orijinalSaniyePara * 2;
            }

            sirketPara *= 2.5f;
            sirketGerekenPara.text = (sirketPara * 2.5f).ToString("F0") + " TL";
        }
    }

    public void teknolojimerkezi()
    {
        if (!teknolojimerkezibool && Para >= 1500000)
        {
            teknolojimerkezibool = true;
            Para -= 1500000;

            if (!iksirEtkisiAktif)
            {
                saniyeBasinaPara += 120000;
                orijinalSaniyePara = saniyeBasinaPara;
            }
            else
            {
                orijinalSaniyePara += 120000;
                saniyeBasinaPara = orijinalSaniyePara * 2;
            }

            teknolojimerkeziGerekenPara.text = (teknolojimerkeziPara * 2.5f).ToString("F0") + " TL";
        }
        else if (teknolojimerkezibool && Para >= teknolojimerkeziPara * 2.5f)
        {
            Para -= teknolojimerkeziPara * 2.5f;

            if (!iksirEtkisiAktif)
            {
                saniyeBasinaPara += 120000;
                orijinalSaniyePara = saniyeBasinaPara;
            }
            else
            {
                orijinalSaniyePara += 120000;
                saniyeBasinaPara = orijinalSaniyePara * 2;
            }

            teknolojimerkeziPara *= 2.5f;
            teknolojimerkeziGerekenPara.text = (teknolojimerkeziPara * 2.5f).ToString("F0") + " TL";
        }
    }

    public void uzaymadencisi()
    {
        if (!uzaymadencisibool && Para >= 5000000)
        {
            uzaymadencisibool = true;
            Para -= 5000000;

            if (!iksirEtkisiAktif)
            {
                saniyeBasinaPara += 350000;
                orijinalSaniyePara = saniyeBasinaPara;
            }
            else
            {
                orijinalSaniyePara += 350000;
                saniyeBasinaPara = orijinalSaniyePara * 2;
            }

            uzaymadencisiGerekenPara.text = (uzaymadencisiPara * 2.5f).ToString("F0") + " TL";
        }
        else if (uzaymadencisibool && Para >= uzaymadencisiPara * 2.5f)
        {
            Para -= uzaymadencisiPara * 2.5f;

            if (!iksirEtkisiAktif)
            {
                saniyeBasinaPara += 350000;
                orijinalSaniyePara = saniyeBasinaPara;
            }
            else
            {
                orijinalSaniyePara += 350000;
                saniyeBasinaPara = orijinalSaniyePara * 2;
            }

            uzaymadencisiPara *= 2.5f;
            uzaymadencisiGerekenPara.text = (uzaymadencisiPara * 2.5f).ToString("F0") + " TL";
        }
    }


    // ---------------------------------------------------------------------------------------------------- //
    [Header("Rebirth Objeleri")]
    public TextMeshProUGUI mevcutPara;
    public TextMeshProUGUI mevcutBoost;
    public TextMeshProUGUI sonrakiPara;
    public TextMeshProUGUI sonrakiBoost;
    public TextMeshProUGUI rebirtheKalanParaOrani;

    private float totalEarnedDiamond;

    private float rebirth;

    private float Rebirth
    {
        get
        {
            return rebirth;
        }

        set
        {
            rebirth = value;
            rebirthText.text = rebirth.ToString("0");
        }
    }

    private float rebirthgerekenparaorani;
    private float rebirthgerekenpara;
    public void RebirthFNC()
    {
        if (rebirth == 0)
        {
            mevcutPara.text = para.ToString("F0");
            mevcutBoost.text = "%0";
            rebirthgerekenpara = 100000;
            sonrakiPara.text = rebirthgerekenpara.ToString("F0");
            sonrakiBoost.text = "%100";
            rebirthgerekenparaorani = ((para / 100000) * 100);
            rebirtheKalanParaOrani.text = "%" + rebirthgerekenparaorani.ToString("F0");
            if (((para / 100000) * 100) > 100)
            {
                rebirtheKalanParaOrani.text = "%100";
            }
        }
        else if (rebirth >= 1)
        {
            mevcutPara.text = para.ToString("F0");
            mevcutBoost.text = "%" + (rebirth * 100);
            rebirthgerekenpara = (100000 * rebirth * rebirth / 2);
            sonrakiPara.text = rebirthgerekenpara.ToString("F0");
            sonrakiBoost.text = "%" + (rebirth * 100 + 100);
            rebirthgerekenparaorani = ((para / (100000 * rebirth * rebirth / 2)) * 100);
            rebirtheKalanParaOrani.text = "%" + rebirthgerekenparaorani.ToString("F0");
            if (((para / (100000 * rebirth * rebirth / 2)) * 100) > 100)
            {
                rebirtheKalanParaOrani.text = "%100";
            }
        }
        else
        {
            print("Beklenmedik bir durumla karşılaşıldı RebirthYap fonksiyonunu kontrol et.");
        }



    }

    // bool olan true false değerleri int olarak 1 0 değerlerine çevirmek için ufak bir fonksiyon:
    int BoolToInt(bool value) => value ? 1 : 0;

    public void RebirthYap()
    {
        if (rebirth == 0 && para >= rebirthgerekenpara && rebirthgerekenparaorani >= 1)
        {
            Para = 0;
            //tiklamaBasinaPara = orijinalTiklamaPara;
            TiklamaBasinaPara = orijinalTiklamaPara;
            SaniyeBasinaPara = orijinalSaniyePara = 0;
            Elmas += 1000 * (1 + BoolToInt(ElmasIksiriBool));
            totalEarnedDiamond += 1000 * (1 + BoolToInt(ElmasIksiriBool));
            Rebirth = 1;

            if (rebirth > 0)
            {
                orijinalTiklamaPara += 1;

                if (iksirEtkisiAktif)
                {
                    tiklamaBasinaPara = orijinalTiklamaPara * 2;
                    GuncelleParaDegerleri();
                }
                else
                {
                    tiklamaBasinaPara = orijinalTiklamaPara;
                    GuncelleParaDegerleri();
                }

            }

            IsciParalariFNC();
            RebirthSonrasiYukseltmeSifirla();

            kontrolYapildiMi = false;

        }
        else if (rebirth >= 1 && para >= rebirthgerekenpara && rebirthgerekenparaorani >= 1)
        {
            Para = 0;
            //tiklamaBasinaPara = orijinalTiklamaPara;
            TiklamaBasinaPara = orijinalTiklamaPara;
            SaniyeBasinaPara = orijinalSaniyePara = 0;
            Elmas += 1000 * (rebirth + 1) * (1 + BoolToInt(ElmasIksiriBool));
            totalEarnedDiamond += 1000 * (rebirth + 1) * (1 + BoolToInt(ElmasIksiriBool));
            Rebirth += 1 * (1 + BoolToInt(RebirthIksiriBool));

            // rebirth iksiri varsa 2 rebirth atlayacağı için bir sonraki rebirthin elmasını da verelim.
            if ((1 + BoolToInt(ElmasIksiriBool)) == 2)
            {
                Elmas += 1000 * (rebirth + 1) * (1 + BoolToInt(ElmasIksiriBool));
                totalEarnedDiamond += 1000 * (rebirth + 1) * (1 + BoolToInt(ElmasIksiriBool));
                orijinalTiklamaPara += 1;
            }

            if (rebirth > 1)
            {
                orijinalTiklamaPara += 1;

                if (iksirEtkisiAktif)
                {
                    tiklamaBasinaPara = orijinalTiklamaPara * 2;
                    GuncelleParaDegerleri();
                }
                else
                {
                    tiklamaBasinaPara = orijinalTiklamaPara;
                    GuncelleParaDegerleri();
                }
            }

            IsciParalariFNC();
            RebirthSonrasiYukseltmeSifirla();

            kontrolYapildiMi = false;

        }
    }

    // ---------------------------------------------------------------------------------------------------- //
    // POTION 

    private float elmasIksiri;
    public TMP_Text elmasIksiriText;

    private bool ParaIksiriBool;
    private bool ElmasIksiriBool;
    private bool RebirthIksiriBool;

    private bool iksirEtkisiAktif = false;

    private float saat;
    private float dk;
    private float saniye;

    // --------------------------------------------------------------------------------------------- //

    private float ElmasIksiri
    {
        get
        {
            return elmasIksiri;
        }

        set
        {
            elmasIksiri = value;
            elmasIksiriText.text = elmasIksiri.ToString("00");

            if (elmasIksiri > 60f && elmasIksiri < 3600f)
            {
                dk = elmasIksiri / 60f;
                saniye = elmasIksiri % 60f;
                elmasIksiriText.text = Mathf.Floor(dk).ToString("00") + ":" + saniye.ToString("00");
            }
            else if (elmasIksiri >= 3600f)
            {
                saat = elmasIksiri / 3600f;
                dk = ((elmasIksiri % 3600f) / 60f);
                saniye = elmasIksiri % 60f;
                elmasIksiriText.text = Mathf.Floor(saat).ToString("00") + ":" + Mathf.Floor(dk).ToString("00") + ":" + saniye.ToString("00");
            }

        }
    }

    public void ElmasIksiriAl()
    {
        if (Elmas >= 100)
        {
            ElmasIksiri += 600; // 600sn yani 10dk
            Elmas -= 100;
        }

    }

    private void ElmasIksiriFNC()
    {
        // elmas iksiri süreç geri sayımı
        if (elmasIksiri > 0)
        {
            ElmasIksiriBool = true;
            elmaszamanlayici -= Time.deltaTime;

            if (elmaszamanlayici <= 0f)
            {
                ElmasIksiri--;
                elmaszamanlayici = 1f;
            }
        }
        else
        {
            ElmasIksiriBool = false;
            elmasIksiriText.text = "00:00:00";
        }
    }

    // --------------------------------------------------------------------------------------------- //

    private float paraIksiri;
    public TMP_Text paraIksiriText;

    private float ParaIksiri
    {
        get
        {
            return paraIksiri;
        }

        set
        {
            paraIksiri = value;
            paraIksiriText.text = paraIksiri.ToString("00");

            if (paraIksiri > 60f && paraIksiri < 3600f)
            {
                dk = paraIksiri / 60f;
                saniye = paraIksiri % 60f;
                paraIksiriText.text = Mathf.Floor(dk).ToString("00") + ":" + saniye.ToString("00");


            }
            else if (paraIksiri >= 3600f)
            {
                saat = paraIksiri / 3600f;
                dk = ((paraIksiri % 3600f) / 60f);
                saniye = paraIksiri % 60f;
                paraIksiriText.text = Mathf.Floor(saat).ToString("00") + ":" + Mathf.Floor(dk).ToString("00") + ":" + saniye.ToString("00");
            }

        }
    }

    public void ParaIksiriAl()
    {
        if (Elmas >= 100)
        {
            ParaIksiri += 600; // 600sn yani 10dk
            Elmas -= 100;
        }

    }

    private void ParaIksiriFNC()
    {
        if (paraIksiri > 0)
        {
            ParaIksiriBool = true;
            parazamanlayici -= Time.deltaTime;

            if (parazamanlayici <= 0f)
            {
                ParaIksiri--;
                parazamanlayici = 1f;
            }
        }
        else
        {
            ParaIksiriBool = false;
            paraIksiriText.text = "00:00:00";
        }
    }

    // --------------------------------------------------------------------------------------------- //
    private float rebirthIksiri;

    public TMP_Text rebirthIksiriText;

    private float RebirthIksiri
    {
        get
        {
            return rebirthIksiri;
        }

        set
        {
            rebirthIksiri = value;
            rebirthIksiriText.text = rebirthIksiri.ToString("00");

            if (rebirthIksiri > 60f && rebirthIksiri < 3600f)
            {
                dk = rebirthIksiri / 60f;
                saniye = rebirthIksiri % 60f;
                rebirthIksiriText.text = Mathf.Floor(dk).ToString("00") + ":" + saniye.ToString("00");
            }
            else if (rebirthIksiri >= 3600f)
            {
                saat = rebirthIksiri / 3600f;
                dk = ((rebirthIksiri % 3600f) / 60f);
                saniye = rebirthIksiri % 60f;
                rebirthIksiriText.text = Mathf.Floor(saat).ToString("00") + ":" + Mathf.Floor(dk).ToString("00") + ":" + saniye.ToString("00");
            }

        }
    }

    public void RebirthIksiriAl()
    {
        if (Elmas >= 100)
        {
            RebirthIksiri += 600; // 600sn yani 10dk
            Elmas -= 100;
        }

    }

    private void RebirthIksiriFNC()
    {
        if (rebirthIksiri > 0)
        {
            RebirthIksiriBool = true;
            rebirthzamanlayici -= Time.deltaTime;

            if (rebirthzamanlayici <= 0f)
            {
                RebirthIksiri--;
                rebirthzamanlayici = 1f;
            }
        }
        else
        {
            RebirthIksiriBool = false;
            rebirthIksiriText.text = "00:00:00";
        }
    }

    // --------------------------------------------------------------------------------------------- //

    public void IksirlerFNC()
    {

        if (ParaIksiriBool && !iksirEtkisiAktif)
        {
            //orijinalSaniyePara = saniyeBasinaPara;
            //orijinalTiklamaPara = tiklamaBasinaPara;

            // orjinalTiklamaPara da bir sorun oluştu galiba yukarısı yanlıştı emin olunca silerim başka bir hatayla karşılaşırsam düzeltirim.

            saniyeBasinaPara = orijinalSaniyePara; 
            tiklamaBasinaPara = orijinalTiklamaPara;

            saniyeBasinaPara = orijinalSaniyePara * 2;
            tiklamaBasinaPara = orijinalTiklamaPara * 2;

            iksirEtkisiAktif = true;
        }

        if (!ParaIksiriBool && iksirEtkisiAktif)
        {
            saniyeBasinaPara = orijinalSaniyePara;
            tiklamaBasinaPara = orijinalTiklamaPara;

            iksirEtkisiAktif = false;
        }
    }

    private void GuncelleParaDegerleri()
    {
        if (iksirEtkisiAktif)
        {
            tiklamaBasinaPara = orijinalTiklamaPara * 2;
            saniyeBasinaPara = orijinalSaniyePara * 2;
        }
        else
        {
            tiklamaBasinaPara = orijinalTiklamaPara;
            saniyeBasinaPara = orijinalSaniyePara;
        }
    }


    private void FixUI()
    {
        if (iksirEtkisiAktif)
        {
            artisSaniyeText.text = (orijinalSaniyePara * 2).ToString("0") + " TL / S";
            tiklamaBasinaParaText.text = TiklamaBasinaPara.ToString("0") + " TL / CLICK";
        }
        else
        {
            artisSaniyeText.text = orijinalSaniyePara.ToString("0") + " TL / S";
            tiklamaBasinaParaText.text = TiklamaBasinaPara.ToString("0") + " TL / CLICK";
        }

        anlikTiklamaBasinaPara = orijinalTiklamaPara;

        MevcutTiklamaBasinaPara.text = TiklamaBasinaPara.ToString("0") + " TL / CLICK";

        SonrakiTiklamaBasinaPara.text = (TiklamaBasinaPara * 2).ToString("0") + " TL / CLICK";

        YukseltmeParasi.text = yukseltmeyeGerekenPara + " TL";
        

    }

    // --------------------------------------------------------------------------------------------- //
    [Header("Yukseltme Menusu")]
    public TextMeshProUGUI MevcutTiklamaBasinaPara;
    public TextMeshProUGUI SonrakiTiklamaBasinaPara;
    public TextMeshProUGUI YukseltmeParasi;

    private float anlikTiklamaBasinaPara;
    private float yukseltmeyeGerekenPara = 100f;

    private bool yukseltmeYapildiMi;
    public void TiklamaBasinaParaYukselt()
    {

        if (!yukseltmeYapildiMi && para >= yukseltmeyeGerekenPara)
        {
            anlikTiklamaBasinaPara = tiklamaBasinaPara;

            yukseltmeyeGerekenPara = 100f;

            MevcutTiklamaBasinaPara.text = anlikTiklamaBasinaPara.ToString("0") + " TL / CLICK";

            SonrakiTiklamaBasinaPara.text = (anlikTiklamaBasinaPara * 2).ToString("0") + " TL / CLICK";

            YukseltmeParasi.text = yukseltmeyeGerekenPara + " TL";

            tiklamaBasinaPara *= 2;

            yukseltmeYapildiMi = true;

            Para -= yukseltmeyeGerekenPara;

            yukseltmeyeGerekenPara *= 2.5f;
        }
        else if (yukseltmeYapildiMi && para >= yukseltmeyeGerekenPara)
        {
            anlikTiklamaBasinaPara = tiklamaBasinaPara;

            MevcutTiklamaBasinaPara.text = anlikTiklamaBasinaPara.ToString("0") + " TL / CLICK";

            SonrakiTiklamaBasinaPara.text = (anlikTiklamaBasinaPara * 2).ToString("0") + " TL / CLICK";

            YukseltmeParasi.text = yukseltmeyeGerekenPara + " TL";

            tiklamaBasinaPara *= 2;

            Para -= yukseltmeyeGerekenPara;
            
            yukseltmeyeGerekenPara *= 2.5f;
        }

    }

    public void RebirthSonrasiYukseltmeSifirla()
    {
        yukseltmeYapildiMi = false;
        tiklamaBasinaPara = orijinalTiklamaPara;
        yukseltmeyeGerekenPara = 100f;
    }

    // --------------------------------------------------------------------------------------------- //
    // STATS AND SETTINGS
    [Header("Stats and settings")]

    private float playTime;
    private float playTimeDAKIKA;
    private float playTimeSANIYE;
    private float playTimeSAAT;
    
    public TextMeshProUGUI ToplamTiklamaSayisi;
    public TextMeshProUGUI ToplamKazanilanPara;
    public TextMeshProUGUI ToplamKazanilanElmas;
    public TextMeshProUGUI ToplamYapilanRebirth;
    public TextMeshProUGUI ToplamOynamaSuresi;

    private void Stats()
    {

        ToplamTiklamaSayisi.text = "TotalClickAmount: " + totalClickAmount.ToString("0") + "";
        ToplamKazanilanPara.text = "TotalEarnedMoney: " + totalEarnedMoney.ToString("0") + " TL";
        ToplamKazanilanElmas.text = "TotalEarnedDiamond" + totalEarnedDiamond.ToString("0");
        ToplamYapilanRebirth.text = "TotalRebirthAmount: " + rebirth.ToString("0");

        playTime += Time.deltaTime;

        ToplamOynamaSuresi.text = "PlayTime: " + playTime.ToString("00") + " saniye";

        if (playTime > 60f && playTime < 3600f)
        {
            playTimeDAKIKA = playTime / 60f;
            playTimeSANIYE = playTime % 60f;
            ToplamOynamaSuresi.text = "PlayTime: " + Mathf.Floor(playTimeDAKIKA).ToString("00") + " dakika " + playTimeSANIYE.ToString("00") + " saniye";
        }
        else if (playTime >= 3600f)
        {
            playTimeSAAT = playTime / 3600f;
            playTimeDAKIKA = ((playTime % 3600f) / 60f);
            playTimeSANIYE = playTime % 60f;
            ToplamOynamaSuresi.text = "PlayTime: " + Mathf.Floor(playTimeSAAT).ToString("00") + " saat " + Mathf.Floor(playTimeDAKIKA).ToString("00") + " dakika" + playTimeSANIYE.ToString("00") + " saniye";
        }
        else if (playTime >= 86400f)
        {
            // uğraşmak istemedim zor değil yukarıdaki gibi extra olarak playTimeGUN değişkeni oluşturulup yapılacak.
        }
            
    }




    // --------------------------------------------------------------------------------------------- //



    void Start()
    {
        Para = 0;
        SaniyeBasinaPara = 0;
        TiklamaBasinaPara = 1;
        Elmas = 0;
        Rebirth = 0;

        elmasIksiri = 0;
        paraIksiri = 0;
        rebirthIksiri = 0;

        orijinalSaniyePara = saniyeBasinaPara;
        orijinalTiklamaPara = tiklamaBasinaPara;
    }

    void Update()
    {
        Para += SaniyeBasinaPara * Time.deltaTime; // *Time.deltaTime saniyeye çeviriyor. 

        totalEarnedMoney += SaniyeBasinaPara * Time.deltaTime;

        RebirthFNC();

        ElmasIksiriFNC();
        ParaIksiriFNC();
        RebirthIksiriFNC();

        IksirlerFNC();

        FixUI();

         Stats();

        // test süreci için eklendi
        /*        if (Input.GetButtonDown("Jump"))
                {
                    Para += 100000;
                }
        */


    }
    
}
