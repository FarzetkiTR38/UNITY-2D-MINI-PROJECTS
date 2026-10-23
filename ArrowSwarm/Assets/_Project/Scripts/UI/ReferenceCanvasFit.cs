namespace ArrowSwarm.UI
{
    using UnityEngine;

    /// <summary>Fits authored reference coordinates inside the safe area without distortion.</summary>
    [ExecuteAlways]
    public sealed class ReferenceCanvasFit : MonoBehaviour
    {
        [SerializeField] private Vector2 _referenceSize = new Vector2(941,1672);
        private Rect _previous;
        private Vector2Int _screen;
        private Canvas _canvas;
        private float _canvasScale;
        private void OnEnable() => Fit();
        private void Update()
        {
            var size=new Vector2Int(Screen.width,Screen.height);
            if (_canvas==null)_canvas=GetComponentInParent<Canvas>();
            if (_previous!=Screen.safeArea || _screen!=size || (_canvas!=null&&!Mathf.Approximately(_canvasScale,_canvas.scaleFactor))) Fit();
        }
        private void Fit()
        {
            var rect=transform as RectTransform;
            var parent=rect.parent as RectTransform;
            if (parent==null || Screen.width==0 || Screen.height==0) return;
            _previous=Screen.safeArea; _screen=new Vector2Int(Screen.width,Screen.height);
            if (_canvas==null)_canvas=GetComponentInParent<Canvas>();
            if (_canvas==null)return;
            float scale=Mathf.Max(.001f,_canvas.scaleFactor);_canvasScale=scale;
            Vector2 safeSize=_previous.size/scale;
            rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f); rect.pivot=new Vector2(.5f,.5f);
            rect.sizeDelta=_referenceSize;
            rect.localScale=Vector3.one*Mathf.Min(safeSize.x/_referenceSize.x,safeSize.y/_referenceSize.y);
            rect.anchoredPosition=(_previous.center-new Vector2(Screen.width*.5f,Screen.height*.5f))/scale;
        }
    }
}
