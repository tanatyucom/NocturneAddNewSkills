using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NocturneAddNewSkills
{
    // Nocturne Modern Controller's current UI language, read the same way
    // its published gameplay mods (Smart Auto, Dash, ...) do: reflection on
    // ModernControllerApi.UseJapaneseUi, no hard reference, so this MOD
    // still loads without Controller. Controller is the only language
    // source: without it (or on a Controller without that property) no GUI
    // reads the snapshot, so the previous Japanese text is kept instead of
    // consulting the OS locale.
    internal static class ControllerUiLanguage
    {
        private const string ControllerAssemblyName = "NocturneModernController";
        private const string ApiTypeName = "NocturneModernController.ModernControllerApi";

        private static PropertyInfo? _useJapaneseUi;
        private static bool _lookupFinished;

        internal static bool UseJapanese
        {
            get
            {
                PropertyInfo? property = Resolve();
                if (property == null)
                {
                    return true;
                }
                try
                {
                    return (bool)property.GetValue(null)!;
                }
                catch
                {
                    return true;
                }
            }
        }

        // Called from OnLateInitializeMelon, when every MOD assembly is
        // loaded: a Controller that is still missing is not installed.
        internal static void FinishLookup()
        {
            Resolve();
            _lookupFinished = true;
        }

        // Controller may load after this MOD, so until FinishLookup a miss
        // is retried on the next call.
        private static PropertyInfo? Resolve()
        {
            if (_lookupFinished)
            {
                return _useJapaneseUi;
            }
            Assembly? controller = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly =>
                string.Equals(assembly.GetName().Name, ControllerAssemblyName, StringComparison.OrdinalIgnoreCase));
            if (controller == null)
            {
                return null;
            }
            _lookupFinished = true;
            try
            {
                _useJapaneseUi = controller.GetType(ApiTypeName)?.GetProperty(
                    "UseJapaneseUi", BindingFlags.Public | BindingFlags.Static);
                if (_useJapaneseUi?.PropertyType != typeof(bool))
                {
                    _useJapaneseUi = null;
                }
            }
            catch
            {
                _useJapaneseUi = null;
            }
            return _useJapaneseUi;
        }
    }

    // Display text for the Controller "MOD Features" cards. Display only:
    // feature ids, AllowedValues and the settings file keep their raw
    // values (Disabled/Native/Always, Native/Unlimited) in every language.
    internal static class FeatureLocalization
    {
        internal const string ProviderDisplayName = "Nocturne Skill Evolution";

        private sealed record LocalizedText(
            string NameJa, string NameEn, string DescriptionJa, string DescriptionEn);

        private static readonly Dictionary<string, LocalizedText> Texts =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["add_new_skills"] = new(
                    "変化後スキルを追加習得",
                    "Learn Transformed Skills",
                    "スキル強化・スキル変化時に、元のスキルを残したまま変化後のスキルを追加で習得します。",
                    "Keep the original skill and learn the transformed skill as an additional skill when Skill Power-Up or Skill Mutation occurs."),
                ["skill_mutation_chance"] = new(
                    "スキル変化：発生率",
                    "Skill Mutation: Chance",
                    "スキル変化の発生率です。100%では、変化できるスキルがあれば必ず変化を試みます。",
                    "Chance of Skill Mutation. At 100%, a mutation is always attempted when a skill can mutate."),
                ["skill_powerup_chance"] = new(
                    "スキル強化：発生率",
                    "Skill Power-Up: Chance",
                    "スキル強化の発生率です。100%では、レベルアップ時の判定1回につき強化（できない場合はスキル変化）を保証します。",
                    "Chance of Skill Power-Up. At 100%, each level-up check guarantees a Power-Up (or a Mutation if no Power-Up is possible)."),
                ["skill_powerup_repeat"] = new(
                    "スキル強化：繰り返し",
                    "Skill Power-Up: Repeat",
                    "無制限にすると、一度強化した後に続けて強化が起きにくくなる制限を解除します。",
                    "Unlimited removes the limit that makes further Power-Ups unlikely after a skill has been powered up."),
            };

        // Not localized: the published Controller gameplay mods show
        // "Gameplay Change" in both languages, so one category stays one label.
        internal const string Category = "Gameplay Change";

        internal static string Name(GameplayFeature feature, bool japanese) =>
            Texts.TryGetValue(feature.Id, out LocalizedText? text)
                ? (japanese ? text.NameJa : text.NameEn)
                : feature.Name;

        internal static string Description(GameplayFeature feature, bool japanese) =>
            Texts.TryGetValue(feature.Id, out LocalizedText? text)
                ? (japanese ? text.DescriptionJa : text.DescriptionEn)
                : feature.Description;

        // Labels keyed by raw value, for Controller's AllowedValueLabels.
        internal static Dictionary<string, string>? ValueLabels(GameplayFeature feature, bool japanese)
        {
            if (feature.AllowedValues == null || feature.AllowedValues.Length == 0)
            {
                return null;
            }
            var labels = new Dictionary<string, string>();
            foreach (string value in feature.AllowedValues)
            {
                labels[value] = value switch
                {
                    "Disabled" => "0%",
                    "Native" => japanese ? "通常" : "Native",
                    "Always" => "100%",
                    "Unlimited" => japanese ? "無制限" : "Unlimited",
                    _ => value
                };
            }
            return labels;
        }
    }
}
