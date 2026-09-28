using MelonLoader;

namespace NocturneSkillEvolution
{
    // Global "Add New Skills" ON/OFF (settings "Enabled", default true).
    //
    // OFF means "behave as if this MOD were absent" for NEW work only:
    // - Chance/Repeat: GameplaySettingsService applies Native as the
    //   EFFECTIVE mode (native bytes restored to vanilla) while the stored
    //   Chance/Repeat values stay untouched, so ON restores them.
    // - Every patch that CHANGES native state/results (AddNew commit,
    //   full-capacity bridge arming, duplicate-target block, Core reentry/
    //   episode-latch suppression, handled-candidate reclassification,
    //   hidden-slot candidate injection) checks IsActive at its decision
    //   point and passes through to native when OFF.
    // - Read-only bookkeeping keeps running while OFF (latch self-resets on
    //   PUpSkillResult, levelUpCnt and result-lifecycle boundaries), so a
    //   later ON never sees a stale latch and an in-event OFF->ON can never
    //   re-commit the same event.
    // - A full-capacity bridge already in flight is allowed to finish (its
    //   continuation patches are driven by the bridge's own Active state),
    //   so OFF takes full effect from the next native episode instead of
    //   abandoning native mid-forget-UI.
    internal static class ModEnableGate
    {
        internal static bool IsActive => GameplaySettingsService.Enabled;

        // Presentation helpers that an in-flight bridge still depends on.
        internal static bool AllowsBridgePresentation =>
            IsActive || FullCapacityAddNewBridgeState.Active || MutationAddNewBridgeState.Active;

        // Called once per ON<->OFF transition (never per frame), on the
        // main thread between frames. Only the one-shot flag that would
        // otherwise leak into the first Core call after re-enabling is
        // cleared; LastOriginalCandidateSkillId is deliberately NOT zeroed
        // (the Always postfix writes it back into PUpSkillID, and 0 would be
        // the Reserve-skill regression).
        internal static void OnEnabledChanged(bool enabled)
        {
            if (!enabled)
            {
                SkillPowerUpChanceAlwaysPatch.SuppressNextMutationConversion = false;
            }
            MelonLogger.Msg(enabled
                ? "[NocturneSkillEvolution] Enabled = true."
                : "[NocturneSkillEvolution] Enabled = false; runtime extension state cleared " +
                  "(in-flight bridge=" +
                  (FullCapacityAddNewBridgeState.Active || MutationAddNewBridgeState.Active) +
                  ", allowed to finish).");
        }
    }
}
