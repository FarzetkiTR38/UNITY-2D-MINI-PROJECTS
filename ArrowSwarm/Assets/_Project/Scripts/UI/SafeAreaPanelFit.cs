namespace ArrowSwarm.UI
{
    using UnityEngine;

    /// <summary>Shrinks a separate layout wrapper when an authored dialog would exceed the safe area, leaving popup animation scale untouched.</summary>
    [ExecuteAlways,DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaPanelFit:MonoBehaviour
    {
        [Tooltip("Twice the largest authored visual distance from the wrapper center, in Canvas units.")]
        [SerializeField] private Vector2 _referenceExtent=Vector2.one;
        [SerializeField] private Vector2 _padding=new Vector2(24,24);
        private Canvas _canvas;
        private Rect _safe;
        private Vector2Int _screen;
        private float _canvasScale;
        private void OnEnable(){Fit();}
        private void Update()
        {
            if(_canvas==null)_canvas=GetComponentInParent<Canvas>();
            if(_safe!=Screen.safeArea||_screen.x!=Screen.width||_screen.y!=Screen.height||
                (_canvas!=null&&!Mathf.Approximately(_canvasScale,_canvas.scaleFactor)))Fit();
        }
        private void Fit()
        {
            if(_canvas==null)_canvas=GetComponentInParent<Canvas>();
            if(_canvas==null||Screen.width==0||Screen.height==0)return;
            _safe=Screen.safeArea;_screen=new Vector2Int(Screen.width,Screen.height);
            _canvasScale=Mathf.Max(.001f,_canvas.scaleFactor);
            Vector2 available=_safe.size/_canvasScale-_padding*2;
            float fit=Mathf.Clamp01(Mathf.Min(available.x/Mathf.Max(1,_referenceExtent.x),available.y/Mathf.Max(1,_referenceExtent.y)));
            transform.localScale=Vector3.one*fit;
            ((RectTransform)transform).anchoredPosition=(_safe.center-new Vector2(Screen.width*.5f,Screen.height*.5f))/_canvasScale;
        }
    }
}
