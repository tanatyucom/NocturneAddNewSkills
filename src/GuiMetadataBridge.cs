using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using MelonLoader;

namespace NocturneSkillEvolution
{
    internal sealed class FeatureMetadataSnapshot
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public string Category { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool RequiresRestart { get; set; }
        public bool ReadOnly { get; set; }
        public string Version { get; set; } = string.Empty;
        public string Warning { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        // Optional multi-value surface (e.g. Chance: Disabled/Native/
        // Always). Null/empty AllowedValues means this feature stays
        // boolean-only (Enabled/SetEnabled) for full backward compatibility
        // with a GUI that does not understand Value at all.
        public string[]? AllowedValues { get; set; }
        public string? Value { get; set; }

        // Display label per raw AllowedValues entry (Controller 3.0.0+;
        // older Controllers ignore it and show their own default labels).
        public Dictionary<string, string>? AllowedValueLabels { get; set; }
    }

    internal sealed class ProviderMetadataSnapshot
    {
        public string ProviderId { get; set; } = "nocturne_skill_evolution";
        public string ProviderName { get; set; } = "Nocturne Skill Evolution";
        public string Version { get; set; } = "0.1.0";
        public List<FeatureMetadataSnapshot> Features { get; set; } = new();
        public string Error { get; set; } = string.Empty;
    }

    internal sealed class FeatureToggleRequest
    {
        public string ProviderId { get; set; } = string.Empty;
        public string FeatureId { get; set; } = string.Empty;
        public bool Enabled { get; set; }

        // When non-null/non-empty, this is a multi-value selection request
        // (takes precedence over Enabled) - see GameplayFeatureRegistry.TrySetValue.
        public string? Value { get; set; }
    }

    internal static class GuiMetadataBridge
    {
        private const string ProviderId = "nocturne_skill_evolution";
        private const int LanguageCheckIntervalMs = 500;
        private static int _lastRequestWriteTick;
        private static bool _snapshotJapanese = true;
        private static int _lastLanguageCheckTick;

        internal static void WriteSnapshot()
        {
            bool japanese = ControllerUiLanguage.UseJapanese;
            var provider = new ProviderMetadataSnapshot
            {
                ProviderName = FeatureLocalization.ProviderDisplayName,
                Features = GameplayFeatureRegistry.GetFeatures().Select(feature =>
                    new FeatureMetadataSnapshot
                    {
                        Id = feature.Id,
                        Name = FeatureLocalization.Name(feature, japanese),
                        Description = FeatureLocalization.Description(feature, japanese),
                        Enabled = feature.Enabled,
                        Category = FeatureLocalization.Category,
                        SortOrder = feature.SortOrder,
                        RequiresRestart = feature.RequiresRestart,
                        Version = "0.1.0",
                        AllowedValues = feature.AllowedValues,
                        Value = feature.Value,
                        AllowedValueLabels = FeatureLocalization.ValueLabels(feature, japanese)
                    }).ToList()
            };
            Directory.CreateDirectory(ModDirectory);
            File.WriteAllText(
                SnapshotPath,
                JsonSerializer.Serialize(
                    new[] { provider },
                    new JsonSerializerOptions { WriteIndented = true }));
            _snapshotJapanese = japanese;
            DeleteLegacySnapshot();
        }

        // Snapshot written by this MOD before its rename ("Nocturne Add New
        // Skills", provider nocturne_add_new_skills). Controller discovers
        // every NocturneModern*.features.json, so a leftover copy would show
        // a second, dead card. Only this one exact file name is touched.
        private const string LegacySnapshotFileName = "NocturneModernAddNewSkills.features.json";
        private static bool _legacySnapshotChecked;

        private static void DeleteLegacySnapshot()
        {
            if (_legacySnapshotChecked)
            {
                return;
            }
            _legacySnapshotChecked = true;
            string legacyPath = Path.Combine(ModDirectory, LegacySnapshotFileName);
            try
            {
                if (File.Exists(legacyPath))
                {
                    File.Delete(legacyPath);
                    MelonLogger.Msg($"[NocturneSkillEvolution] Removed old GUI snapshot {LegacySnapshotFileName}.");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning(
                    $"[NocturneSkillEvolution] Could not remove old GUI snapshot {LegacySnapshotFileName}: {ex.Message}");
            }
        }

        // Controller applies a language change when its Settings GUI closes
        // and re-reads this snapshot right before the next open, so a
        // throttled poll here keeps the cards in Controller's language on
        // every reopen without restarting the game.
        internal static void SampleLanguage(bool force = false)
        {
            int now = Environment.TickCount;
            if (!force && unchecked(now - _lastLanguageCheckTick) < LanguageCheckIntervalMs)
            {
                return;
            }
            _lastLanguageCheckTick = now;
            try
            {
                if (ControllerUiLanguage.UseJapanese != _snapshotJapanese)
                {
                    WriteSnapshot();
                }
            }
            catch
            {
                // Retried on the next poll; the snapshot is display-only.
            }
        }

        internal static void SampleToggleRequests()
        {
            if (!File.Exists(RequestPath))
            {
                return;
            }
            int writeTick = unchecked((int)File.GetLastWriteTimeUtc(RequestPath).Ticks);
            if (writeTick == _lastRequestWriteTick)
            {
                return;
            }
            _lastRequestWriteTick = writeTick;
            try
            {
                List<FeatureToggleRequest>? requests =
                    JsonSerializer.Deserialize<List<FeatureToggleRequest>>(
                        File.ReadAllText(RequestPath));
                if (requests == null)
                {
                    return;
                }
                List<FeatureToggleRequest> ownRequests = requests
                    .Where(item => string.Equals(item.ProviderId, ProviderId, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                if (ownRequests.Count == 0)
                {
                    return;
                }

                // SETTINGS-RELOAD diagnostic (Settings GUI Chance reset bug
                // investigation, 2026-09-12): "source" is this provider's
                // persistent record (GameplaySettingsService, which is
                // also what NocturneSkillEvolution.settings.json holds -
                // see that class's own header comment) as it stood BEFORE
                // this request batch is applied. "applied" is the same
                // three values AFTER. A human comparing the two lines can
                // immediately tell whether a GUI round-trip silently
                // changed something the persistent record did not already
                // reflect. Logged only when at least one request in this
                // batch actually targets this provider - never per frame.
                string sourceMutationChance = GameplaySettingsService.SkillMutationChance.ToString();
                string sourcePowerUpChance = GameplaySettingsService.SkillPowerUpChance.ToString();
                string sourceRepeat = GameplaySettingsService.Repeat;
                bool sourceEnabled = GameplaySettingsService.Enabled;

                bool changed = false;
                foreach (FeatureToggleRequest request in ownRequests)
                {
                    changed |= !string.IsNullOrEmpty(request.Value)
                        ? GameplayFeatureRegistry.TrySetValue(request.FeatureId, request.Value)
                        : GameplayFeatureRegistry.TrySetEnabled(request.FeatureId, request.Enabled);
                }

                MelonLogger.Msg(
                    "[NocturneSkillEvolution] SETTINGS-RELOAD; " +
                    $"sourceEnabled={sourceEnabled}; appliedEnabled={GameplaySettingsService.Enabled}; " +
                    $"sourceMutationChance={sourceMutationChance}; sourcePowerUpChance={sourcePowerUpChance}; " +
                    $"sourceRepeat={sourceRepeat}; " +
                    $"appliedMutationChance={GameplaySettingsService.SkillMutationChance}; " +
                    $"appliedPowerUpChance={GameplaySettingsService.SkillPowerUpChance}; " +
                    $"appliedRepeat={GameplaySettingsService.Repeat}.");

                if (changed)
                {
                    WriteSnapshot();
                }
            }
            catch
            {
                // A malformed optional GUI request must never stop standalone play.
            }
        }

        private static string ModDirectory =>
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
        // "NocturneModern" prefix is required: published ModernController
        // discovers external providers via "NocturneModern*.features.json".
        private static string SnapshotPath =>
            Path.Combine(ModDirectory, "NocturneModernSkillEvolution.features.json");
        private static string RequestPath =>
            Path.Combine(ModDirectory, "NocturneModernController.feature-requests.json");
    }
}
