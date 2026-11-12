namespace ArrowSwarm.Puzzle
{
    using ArrowSwarm.Modes;

    /// <summary>Retains one paid arrow or rotation suggestion until it is consumed or its attempt becomes invalid.</summary>
    public sealed partial class PuzzleSession
    {
        private const int NoHintAction = int.MinValue;
        private int _hintAction = NoHintAction;
        private int _hintVersion = -1;
        private int _hintOrientation;

        /// <summary>Whether the current attempt already has an unused paid hint.</summary>
        public bool HasPendingHint
        {
            get
            {
                if (_hintAction == NoHintAction) return false;
                bool valid = _hintVersion == _version && Level != null && !Won && !Lost;
                if (valid && _hintAction < 0) valid = _mode == GameModeId.ColorShift && Turns > 0 && Orientation == _hintOrientation;
                else if (valid) valid = _removed != null && _hintAction < _removed.Length && !_removed[_hintAction] &&
                    !_inFlight[_hintAction] && _views[_hintAction] != null && _views[_hintAction].isActiveAndEnabled &&
                    PuzzleRules.CanRemove(Level, _removed, _hintAction, Orientation);
                if (!valid) ResetHintSuggestion();
                return valid;
            }
        }

        private bool RepeatHintSuggestion()
        {
            if (!HasPendingHint) return false;
            RocketArmed = false; RestoreHintHighlight();
            SetMessage(_hintAction < 0 ? "puzzle_hint_rotate" : "puzzle_hint_arrow");
            Notify(); return true;
        }

        private void RememberHintSuggestion(int action)
        {
            ResetHintSuggestion(); _hintAction = action; _hintVersion = _version; _hintOrientation = Orientation;
        }

        private void RestoreHintHighlight()
        {
            if (HasPendingHint && _hintAction >= 0) _views[_hintAction].Highlight(true);
        }

        private void ConsumeHintArrow(int index)
        { if (_hintAction == index) ResetHintSuggestion(); }

        private void ConsumeHintRotation()
        {
            // The rotation consumes a rotation suggestion and invalidates a Color Shift arrow's old wall match.
            if (_hintAction != NoHintAction) ResetHintSuggestion();
        }

        private void ResetHintSuggestion()
        {
            if (_hintAction >= 0 && _views != null && _hintAction < _views.Length &&
                _views[_hintAction] != null && _views[_hintAction].isActiveAndEnabled) _views[_hintAction].Highlight(false);
            _hintAction = NoHintAction; _hintVersion = -1;
        }
    }
}
