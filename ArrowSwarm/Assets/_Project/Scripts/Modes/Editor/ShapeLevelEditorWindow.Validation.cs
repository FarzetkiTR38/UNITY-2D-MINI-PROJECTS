namespace ArrowSwarm.Modes.Editor
{
    using UnityEditor;
    using UnityEngine;

    public sealed partial class ShapeLevelEditorWindow
    {
        private void EditLevel(PuzzleLevel selected)
        {
            if (selected == _level || (selected == _asset && _level != null)) return;
            ReleaseDraft(); _asset = selected;
            if (selected == null) return;
            _level = Instantiate(selected); _level.name = selected.name; _level.hideFlags = HideFlags.HideAndDontSave;
            CaptureBaseline(); _status = "Editing a temporary draft. Only validated Save changes the asset.";
        }

        private void ReleaseDraft()
        {
            if (_level != null && !AssetDatabase.Contains(_level)) DestroyImmediate(_level);
            _level = null; _baselineLevel = null; _selected = -1;
        }

        private void CaptureBaseline()
        {
            if (_level == _baselineLevel) return;
            _baselineLevel = _level; _selected = -1;
            // The draft gets its legacy baseline before the first editable control.
            // The source asset receives it only after the user saves a valid draft.
            if (_level != null) _level.CaptureCoverageIfMissing();
        }

        private bool IsValid()
        {
            if (_level == null) { _status = "Choose a level first."; return false; }
            string coverage = ShapeImageMask.CoverageError(_level);
            if (coverage != null) { _status = "INVALID · " + coverage; return false; }
            var result = PuzzleSolver.Solve(_level);
            _status = result.Solvable
                ? $"VALID · {_level.Arrows.Count} arrows · {result.Steps.Count} solution steps · complete original mask. Click an arrow to edit."
                : "INVALID · " + result.Error;
            return result.Solvable;
        }

        private void Validate() => IsValid();

        private void Save()
        {
            if (!IsValid()) { _status = "Save blocked. " + _status; return; }
            if (_asset != null && AssetDatabase.Contains(_asset))
            {
                EditorUtility.CopySerialized(_level, _asset); _asset.hideFlags = HideFlags.None;
                EditorUtility.SetDirty(_asset); AssetDatabase.SaveAssets(); _status = "Validated asset and original mask saved."; return;
            }
            string path = EditorUtility.SaveFilePanelInProject("Save Shapes Level", _level.Title, "asset",
                "Save a validated level", ModeAuthoringUtility.Data);
            if (string.IsNullOrEmpty(path)) return;
            _asset = Instantiate(_level); _asset.name = _level.name; _asset.hideFlags = HideFlags.None;
            AssetDatabase.CreateAsset(_asset, path); AssetDatabase.SaveAssets();
            _status = "Level and original mask saved. Add it to ShapesCatalog to publish in the campaign.";
        }
    }
}
