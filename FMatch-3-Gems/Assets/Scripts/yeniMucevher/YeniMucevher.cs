using System.Collections;
using UnityEngine;

public class YeniMucevher : MonoBehaviour
{
    public Vector2Int posIndex;

    public Board board;

    public Vector2 birinciBasilanPos;
    public Vector2 sonBasilanPos;

    bool mouseBasildi;
    float suruklemeAngle;

    YeniMucevher digerMucevher;

    public enum MucevherTipi { mavi, pembe, sari, acikyesil, kapaliyesil };

    public MucevherTipi tipi;
    public bool eslesdiMi;

    Vector2Int ilkPos;

    public void MucevheriDuzenle(Vector2Int pos, Board theBoard)
    {
        posIndex = pos;
        board = theBoard;
    }

    private void Update()
    {

        transform.position = Vector2.Lerp(transform.position, posIndex, board.mucevherHiz * Time.deltaTime);

        if (mouseBasildi && Input.GetMouseButtonUp(0))
        {
            mouseBasildi = false;
            sonBasilanPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            HesaplaAngleFNC();
        }
    }

    private void OnMouseDown()
    {
        birinciBasilanPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseBasildi = true;
    }

    void HesaplaAngleFNC()
    {
        float dx = sonBasilanPos.x - birinciBasilanPos.x;
        float dy = sonBasilanPos.y - birinciBasilanPos.y;

        suruklemeAngle = Mathf.Atan2(dy, dx);
        suruklemeAngle = suruklemeAngle * 180 / Mathf.PI;

        if (Vector3.Distance(birinciBasilanPos, sonBasilanPos) >= 0.5f && Vector3.Distance(birinciBasilanPos, sonBasilanPos) <= 1.5f)
        {
            TileHareketFNC();
        }
    }

    void TileHareketFNC()
    {
        ilkPos = posIndex;

        if (suruklemeAngle < 45 && suruklemeAngle > -45 && posIndex.x < board.width - 1)
        {
            digerMucevher = board.tumMucevherler[posIndex.x + 1, posIndex.y].GetComponent<YeniMucevher>();
            digerMucevher.posIndex.x--;
            posIndex.x++;
        }
        else if (suruklemeAngle < 135 && suruklemeAngle > 45 && posIndex.y < board.height - 1)
        {
            digerMucevher = board.tumMucevherler[posIndex.x, posIndex.y + 1].GetComponent<YeniMucevher>();
            digerMucevher.posIndex.y--;
            posIndex.y++;
        }
        else if (suruklemeAngle < -45 && suruklemeAngle > -135 && posIndex.y > 0)
        {
            digerMucevher = board.tumMucevherler[posIndex.x, posIndex.y - 1].GetComponent<YeniMucevher>();
            digerMucevher.posIndex.y++;
            posIndex.y--;
        }
        else if ((suruklemeAngle > 135 || suruklemeAngle < -135) && posIndex.x > 0)
        {
            digerMucevher = board.tumMucevherler[posIndex.x - 1, posIndex.y].GetComponent<YeniMucevher>();
            digerMucevher.posIndex.x++;
            posIndex.x--;
        }

        board.tumMucevherler[posIndex.x, posIndex.y] = this.gameObject;
        board.tumMucevherler[digerMucevher.posIndex.x, digerMucevher.posIndex.y] = digerMucevher.gameObject;

        StartCoroutine(HareketiKontrolEtRouitne());
    }

    public IEnumerator HareketiKontrolEtRouitne()
    {
        yield return new WaitForSeconds(0.3f);

        board.eslesmeController.EslemeleriBulFNC();

        if (digerMucevher != null)
        {
            if (eslesdiMi == false && digerMucevher.eslesdiMi == false)
            {
                digerMucevher.posIndex = posIndex;
                posIndex = ilkPos;

                board.tumMucevherler[posIndex.x, posIndex.y] = this.gameObject;
                board.tumMucevherler[digerMucevher.posIndex.x, digerMucevher.posIndex.y] = digerMucevher.gameObject;

            }
        }
    }

}
