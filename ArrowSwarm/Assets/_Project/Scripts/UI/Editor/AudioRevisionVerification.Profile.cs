namespace ArrowSwarm.UI.Editor
{
    using System.Collections;
    using System.Linq;
    using ArrowSwarm.Audio;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Exercises the actual Home profile button, draft controls and dismissal sounds.</summary>
    public static partial class AudioRevisionVerification
    {
        private static IEnumerator Profile()
        {
            var menu = Object.FindFirstObjectByType<MainMenuUI>();
            var profile = Object.FindFirstObjectByType<ProfileSetupModalUI>(FindObjectsInactive.Include);
            Check(menu != null && profile != null, "Real Home and Profile controllers exist");
            if (profile.gameObject.activeInHierarchy) { profile.Hide(); yield return Settled(); }
            var avatar = Object.FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .FirstOrDefault(b => Enumerable.Range(0, b.onClick.GetPersistentEventCount()).Any(i => b.onClick.GetPersistentMethodName(i) == "OpenProfileModal"));
            int before = Plays(); Click(avatar); ExpectPlays(before, 2, "Profile avatar produces one click and one popup opening");
            yield return Settled();
            before = Plays(); profile.Show(); ExpectPlays(before, 0, "Repeated Show does not duplicate opening SFX");
            yield return Settled();
            foreach (string field in new[] { "_diceButton", "_prevCountryButton", "_nextCountryButton", "_countryButton" })
            {
                before = Plays(); Click(Field<Button>(profile, field)); ExpectPlays(before, 1, "Profile control produces one feedback sound: " + field);
                yield return Settled(.2f);
            }
            before = Plays(); profile.Hide(); ExpectPlays(before, 1, "Profile dismissal produces one closing sound");
            before = Plays(); profile.Hide(); ExpectPlays(before, 0, "Repeated Hide does not duplicate closing SFX");
            yield return Settled();
            Check(!profile.gameObject.activeSelf, "Profile is hidden after animated close");
            foreach (string field in new[] { "_saveButton", "_skipButton", "_closeButton" })
            {
                var control = Field<Button>(profile, field);
                if (control == null) { Results.Add("NOT AUTHORED: optional Profile control " + field); continue; }
                menu.OpenProfileModal(); yield return Settled();
                before = Plays(); Click(control);
                ExpectPlays(before, 2, "Profile commit/dismiss control produces one click plus close: " + field);
                yield return Settled();
                Check(!profile.gameObject.activeSelf, "Profile closes after " + field);
            }
            AudioManager.Instance.PlayBGM(AudioManager.Instance.Library.MenuBGM);
        }
    }
}
