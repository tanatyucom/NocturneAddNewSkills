using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using MelonLoader;

namespace NocturneSkillEvolution
{
    // Persisted config shape. Kept as a plain POCO separate from the
    // runtime NativeChanceMode enum so a malformed/unknown string in the
    // file can never throw during deserialization - validation happens
    // explicitly in GameplaySettingsService.Load, field by field, with a
    // fail-safe fallback to "Native" and a warning log per field.
    internal sealed class GameplaySettingsData
    {
        // Global ON/OFF (see ModEnableGate). Nullable so a pre-toggle
        // settings file without this key loads as true, never silently OFF.
        public bool? Enabled { get; set; }
        public SkillMutationSettingsData SkillMutation { get; set; } = new();
        public SkillPowerUpSettingsData SkillPowerUp { get; set; } = new();
    }

    internal sealed class SkillMutationSettingsData
    {
        public string Chance { get; set; } = "Native";
    }

    internal sealed class SkillPowerUpSettingsData
    {
        public string Chance { get; set; } = "Native";
        public string Repeat { get; set; } = "Native";
    }

    // Single source of truth for [SkillMutation]/[SkillPowerUp] Chance (and,
    // once implemented, Repeat). Owns the config file
    // (NocturneSkillEvolution.settings.json), owns the current in-memory
    // mode values, and is the ONLY thing that calls
    // SkillMutationChanceControl.SetMode / SkillPowerUpChanceControl.SetMode
    // - both ModMain's startup (via Load) and GameplayFeatureRegistry's GUI
    // bridge (via SetSkillMutationChance/SetSkillPowerUpChance) go through
    // here, so runtime state and the saved file never diverge (see
    // NocturneModernGameplay / NocturneModernController integration spec,
    // section 2 - Single Source of Truth).
    //
    // NocturneSkillEvolution.settings.json is deliberately a NEW, MOD-name-
    // qualified file - the pre-existing root settings.json
    // ({"language":"ja"}) is an unrelated, currently-unread placeholder and
    // is left untouched.
    internal static class GameplaySettingsService
    {
        internal static bool Enabled { get; private set; } = true;
        internal static NativeChanceMode SkillMutationChance { get; private set; } = NativeChanceMode.Native;
        internal static NativeChanceMode SkillPowerUpChance { get; private set; } = NativeChanceMode.Native;

        // "Native" or "Unlimited". Unlimited's backend is
        // OptionFRepeatUnlimitedControl (Option F result conversion,
        // initial diagnostic-heavy candidate - see
        // investigations/REPEAT_UNLIMITED/PLAN.md). Any other value falls
        // back to Native with a warning, never silently no-ops as if it
        // worked.
        internal static string Repeat { get; private set; } = "Native";

        // What runtime code must read instead of Repeat: Native while the
        // MOD is globally OFF, without touching the stored value.
        internal static string EffectiveRepeat => Enabled ? Repeat : "Native";

        private static string SettingsPath =>
            Path.Combine(ModDirectory, "NocturneSkillEvolution.settings.json");

        // Settings file of the same MOD before its rename (published briefly
        // as "Nocturne Add New Skills" v0.1.0). Read once to seed the new
        // file when that one does not exist yet; never modified or deleted.
        private const string LegacySettingsFileName = "NocturneAddNewSkills.settings.json";

        private static string LegacySettingsPath =>
            Path.Combine(ModDirectory, LegacySettingsFileName);

        private static string ModDirectory =>
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

        // Loads the config file (writing a fresh default one if missing),
        // validates every field fail-safe, and APPLIES the result to
        // SkillMutationChanceControl/SkillPowerUpChanceControl immediately
        // (both must already be Initialize()'d - see ModMain.cs ordering).
        internal static void Load()
        {
            try
            {
                bool migrating = !File.Exists(SettingsPath) && File.Exists(LegacySettingsPath);
                if (!File.Exists(SettingsPath) && !migrating)
                {
                    Save();
                }
                else
                {
                    // Same schema and the same fail-safe validation as the
                    // current file; only the known keys carry over.
                    GameplaySettingsData? data = JsonSerializer.Deserialize<GameplaySettingsData>(
                        File.ReadAllText(migrating ? LegacySettingsPath : SettingsPath));
                    if (data != null)
                    {
                        Enabled = data.Enabled ?? true;
                        SkillMutationChance = ParseChanceMode(data.SkillMutation?.Chance, "SkillMutation.Chance");
                        SkillPowerUpChance = ParseChanceMode(data.SkillPowerUp?.Chance, "SkillPowerUp.Chance");
                        Repeat = ParseRepeat(data.SkillPowerUp?.Repeat);
                        if (migrating)
                        {
                            Save();
                            if (File.Exists(SettingsPath))
                            {
                                MelonLogger.Msg(
                                    $"[NocturneSkillEvolution] Migrated settings from {LegacySettingsFileName} " +
                                    "to NocturneSkillEvolution.settings.json (old file kept).");
                            }
                        }
                        else if (data.Enabled == null)
                        {
                            // Normalize an older file so the key is visible.
                            Save();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning(
                    "[NocturneSkillEvolution] Gameplay settings file unreadable/invalid; " +
                    $"falling back to all-Native defaults: {ex.Message}");
                Enabled = true;
                SkillMutationChance = NativeChanceMode.Native;
                SkillPowerUpChance = NativeChanceMode.Native;
                Repeat = "Native";
            }

            ApplyEffectiveChanceModes();
            // Repeat itself has no ApplyMode/SetMode step here - it is read
            // directly from this service's Repeat property by
            // OptionFRepeatUnlimitedControl on every Core invocation, so
            // there is nothing additional to "apply" at load time.

            MelonLogger.Msg(
                "[NocturneSkillEvolution] Gameplay settings loaded; " +
                $"Enabled={Enabled} SkillMutation.Chance={SkillMutationChance} SkillPowerUp.Chance={SkillPowerUpChance} " +
                $"SkillPowerUp.Repeat={Repeat}.");
        }

        internal static void Save()
        {
            try
            {
                var data = new GameplaySettingsData
                {
                    Enabled = Enabled,
                    SkillMutation = new SkillMutationSettingsData { Chance = SkillMutationChance.ToString() },
                    SkillPowerUp = new SkillPowerUpSettingsData
                    {
                        Chance = SkillPowerUpChance.ToString(),
                        Repeat = Repeat
                    }
                };
                Directory.CreateDirectory(ModDirectory);
                File.WriteAllText(
                    SettingsPath,
                    JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[NocturneSkillEvolution] Gameplay settings save failed: {ex.Message}");
            }
        }

        // Entry points for GUI-driven changes (GameplayFeatureRegistry).
        // Both apply to the backend immediately AND persist, per the
        // integration spec's sync rule (GUI change -> Settings Service ->
        // runtime -> config).
        internal static void SetSkillMutationChance(NativeChanceMode mode)
        {
            SkillMutationChance = mode;
            SkillMutationChanceControl.SetMode(EffectiveMode(mode));
            Save();
        }

        internal static void SetSkillPowerUpChance(NativeChanceMode mode)
        {
            SkillPowerUpChance = mode;
            SkillPowerUpChanceControl.SetMode(EffectiveMode(mode));
            Save();
        }

        // GUI entry point for SkillPowerUp.Repeat. Unlike Chance, there is
        // no SetMode step here - OptionFRepeatUnlimitedControl reads
        // GameplaySettingsService.Repeat directly on every Core invocation
        // (see its own header comment), so updating this property is the
        // entire "apply" step; Save() persists it immediately, matching the
        // Chance setters' apply-then-persist pattern. Reuses ParseRepeat so
        // an unexpected value can never be stored as anything other than
        // "Native" or "Unlimited" (fails safe to Native with a warning,
        // same as a malformed config file value).
        internal static void SetRepeat(string mode)
        {
            Repeat = ParseRepeat(mode);
            Save();
            MelonLogger.Msg($"[NocturneSkillEvolution] SkillPowerUp Repeat mode set; mode={Repeat}.");
        }

        // GUI entry point for the global "Add New Skills" toggle. Persists
        // only Enabled; Chance/Repeat stored values are never rewritten, so
        // turning it back ON resumes with the previous settings.
        internal static void SetEnabled(bool enabled)
        {
            if (Enabled == enabled) return;
            Enabled = enabled;
            ApplyEffectiveChanceModes();
            Save();
            ModEnableGate.OnEnabledChanged(enabled);
        }

        // Native bytes follow the stored Chance modes only while Enabled;
        // OFF restores vanilla bytes (Native) without changing the stored
        // values.
        private static void ApplyEffectiveChanceModes()
        {
            SkillMutationChanceControl.SetMode(EffectiveMode(SkillMutationChance));
            SkillPowerUpChanceControl.SetMode(EffectiveMode(SkillPowerUpChance));
        }

        private static NativeChanceMode EffectiveMode(NativeChanceMode stored) =>
            Enabled ? stored : NativeChanceMode.Native;

        private static NativeChanceMode ParseChanceMode(string? raw, string fieldName)
        {
            if (Enum.TryParse(raw, ignoreCase: true, out NativeChanceMode mode))
                return mode;
            MelonLogger.Warning(
                $"[NocturneSkillEvolution] config: unknown {fieldName} value '{raw}'; falling back to Native.");
            return NativeChanceMode.Native;
        }

        private static string ParseRepeat(string? raw)
        {
            if (string.Equals(raw, "Native", StringComparison.OrdinalIgnoreCase)) return "Native";
            if (string.Equals(raw, "Unlimited", StringComparison.OrdinalIgnoreCase)) return "Unlimited";
            if (!string.IsNullOrEmpty(raw))
                MelonLogger.Warning(
                    $"[NocturneSkillEvolution] config: unknown SkillPowerUp.Repeat value '{raw}'; falling back to Native.");
            return "Native";
        }
    }
}
