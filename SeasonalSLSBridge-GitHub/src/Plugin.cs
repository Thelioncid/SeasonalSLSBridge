using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace SeasonalSLSBridge
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "TheLionCid.SeasonalSLSBridge";
        public const string PluginName = "Seasonal SLS Bridge";
        public const string PluginVersion = "1.1.0";

        internal static Plugin Instance;
        internal static ManualLogSource Log;

        /*
         * ============================================================
         * SEASONAL CREATURE NAMES
         * ============================================================
         *
         * Stores only creatures that actually received a seasonal
         * modifier through this bridge.
         *
         * SLS itself is NOT modified.
         */

        private static readonly Dictionary<int, string> SeasonalCreatures =
            new Dictionary<int, string>();

        internal static void RegisterSeasonalCreature(
            Character creature,
            string season)
        {
            if (creature == null ||
                string.IsNullOrEmpty(season))
            {
                return;
            }

            lock (SeasonalCreatures)
            {
                SeasonalCreatures[
                    creature.GetInstanceID()
                ] = season;
            }
        }

        internal static void UnregisterSeasonalCreature(
            Character creature)
        {
            if (creature == null)
                return;

            lock (SeasonalCreatures)
            {
                SeasonalCreatures.Remove(
                    creature.GetInstanceID()
                );
            }
        }

        internal static string GetSeasonalTitle(
            Character creature)
        {
            if (creature == null)
                return null;

            string season;

            lock (SeasonalCreatures)
            {
                if (!SeasonalCreatures.TryGetValue(
                        creature.GetInstanceID(),
                        out season))
                {
                    return null;
                }
            }

            switch (season)
            {
                case "winter":
                    return "Invernal";

                case "spring":
                    return "Primaveral";

                case "summer":
                    return "Estival";

                case "autumn":
                    return "Otoñal";

                default:
                    return null;
            }
        }

        /*
         * ============================================================
         * SEASONAL MODIFIER CONFIGURATION
         * ============================================================
         *
         * Values are weights from 0 to 100.
         *
         * 0   = disabled
         * 100 = maximum weight
         *
         * The bridge chooses AT MOST ONE seasonal modifier per creature.
         *
         * The actual SLS modifier names are used internally.
         */

        private readonly Dictionary<string, ConfigEntry<float>> _winter =
            new Dictionary<string, ConfigEntry<float>>();

        private readonly Dictionary<string, ConfigEntry<float>> _spring =
            new Dictionary<string, ConfigEntry<float>>();

        private readonly Dictionary<string, ConfigEntry<float>> _summer =
            new Dictionary<string, ConfigEntry<float>>();

        private readonly Dictionary<string, ConfigEntry<float>> _autumn =
            new Dictionary<string, ConfigEntry<float>>();

        internal ConfigEntry<float> SeasonalRollChance;
        internal ConfigEntry<float> DelaySeconds;
        internal ConfigEntry<bool> IncludeTamed;
        internal ConfigEntry<bool> LogAppliedModifiers;
        internal ConfigEntry<bool> LogApiDiscovery;

        /*
         * All non-Boss modifiers found in the user's
         * StarLevelSystem Modifiers.yaml.
         *
         * IMPORTANT:
         * BossSummoner and LifeLink are intentionally excluded.
         */

        private static readonly string[] MajorModifiers =
        {
            "Brutal",
            "ElementalChaos",
            "Fire",
            "Frost",
            "Poison",
            "Lightning",
            "Splitter",
            "SoulEater",
            "ResistPierce",
            "ResistSlash",
            "ResistBlunt"
        };

        private static readonly string[] MinorModifiers =
        {
            "ResistFire",
            "ResistFrost",
            "ResistPoison",
            "ResistSpirit",
            "FireNova",
            "PoisonNova",
            "Lootbags",
            "Alert",
            "Big",
            "Fast",
            "StaminaDrain",
            "Evolving",
            "EitrDrain"
        };

        private static readonly string[] AllModifiers =
            MajorModifiers
                .Concat(MinorModifiers)
                .ToArray();

        private void Awake()
        {
            Instance = this;
            Log = base.Logger;

            /*
             * --------------------------------------------------------
             * GENERAL
             * --------------------------------------------------------
             */

            SeasonalRollChance = Config.Bind(
                "General",
                "SeasonalRollChance",
                100f,
                "Overall chance that the bridge attempts a seasonal modifier on a newly spawned creature. Value is a percentage from 0 to 100."
            );

            DelaySeconds = Config.Bind(
                "General",
                "DelaySeconds",
                0.75f,
                "Delay after Character.Awake before the seasonal roll. This gives StarLevelSystem time to finish its own spawn setup."
            );

            IncludeTamed = Config.Bind(
                "General",
                "IncludeTamed",
                false,
                "If enabled, tamed creatures can receive seasonal modifiers. Normally disabled."
            );

            LogAppliedModifiers = Config.Bind(
                "Diagnostics",
                "LogAppliedModifiers",
                true,
                "Log successful seasonal modifier applications."
            );

            LogApiDiscovery = Config.Bind(
                "Diagnostics",
                "LogApiDiscovery",
                true,
                "Log the StarLevelSystem API methods discovered at runtime."
            );

            /*
             * --------------------------------------------------------
             * SEASONAL CONFIG
             * --------------------------------------------------------
             */

            ConfigureWinter();
            ConfigureSpring();
            ConfigureSummer();
            ConfigureAutumn();

            /*
             * --------------------------------------------------------
             * HARMONY
             * --------------------------------------------------------
             */

            new Harmony(PluginGuid).PatchAll();

            Log.LogInfo(
                $"{PluginName} {PluginVersion} loaded."
            );

            Log.LogInfo(
                "Seasonal modifier selection is active."
            );

            Log.LogInfo(
                "Hostile-only filtering is disabled: any valid creature can receive a seasonal modifier."
            );

            Log.LogInfo(
                "Boss-only modifiers are excluded."
            );

            Log.LogInfo(
                $"Loaded {AllModifiers.Length} non-Boss StarLevelSystem modifiers."
            );

            /*
             * Discover SLS API after the plugin has loaded.
             */

            SLSReflection.Discover(
                LogApiDiscovery.Value
            );
        }

        /*
         * ============================================================
         * SEASON CONFIGURATION
         * ============================================================
         */

        private void ConfigureWinter()
        {
            BindSeasonModifier(
                _winter,
                "Winter",
                "Frost",
                95f,
                "Cold creatures of winter. Strongly associated with winter."
            );

            BindSeasonModifier(
                _winter,
                "Winter",
                "ResistFrost",
                25f,
                "Creatures adapted to winter cold."
            );

            BindSeasonModifier(
                _winter,
                "Winter",
                "Big",
                25f,
                "Allows unusually large creatures to appear during winter."
            );

            BindSeasonModifier(
                _winter,
                "Winter",
                "StaminaDrain",
                15f,
                "Winter conditions make physical exertion more demanding."
            );

            BindSeasonModifier(
                _winter,
                "Winter",
                "ResistPierce",
                10f,
                "Hardier winter creatures."
            );

            BindSeasonModifier(
                _winter,
                "Winter",
                "ResistSlash",
                10f,
                "Hardier winter creatures."
            );

            BindSeasonModifier(
                _winter,
                "Winter",
                "ResistBlunt",
                10f,
                "Hardier winter creatures."
            );

            BindSeasonModifier(
                _winter,
                "Winter",
                "EitrDrain",
                5f,
                "Rare magical effect associated with harsh magical environments."
            );

            BindRemainingModifiers(
                _winter,
                "Winter"
            );
        }

        private void ConfigureSpring()
        {
            BindSeasonModifier(
                _spring,
                "Spring",
                "Poison",
                65f,
                "Poisonous life and natural toxins become more common in spring."
            );

            BindSeasonModifier(
                _spring,
                "Spring",
                "ElementalChaos",
                25f,
                "Unstable elemental activity during the changing season."
            );

            BindSeasonModifier(
                _spring,
                "Spring",
                "PoisonNova",
                20f,
                "Rare creatures capable of releasing poisonous bursts."
            );

            BindSeasonModifier(
                _spring,
                "Spring",
                "ResistPoison",
                15f,
                "Creatures adapted to the increased presence of toxins."
            );

            BindSeasonModifier(
                _spring,
                "Spring",
                "Fast",
                20f,
                "Increased activity during the growing season."
            );

            BindSeasonModifier(
                _spring,
                "Spring",
                "Evolving",
                10f,
                "Rapid biological development."
            );

            BindSeasonModifier(
                _spring,
                "Spring",
                "Splitter",
                5f,
                "Rare unusual reproductive or splitting behaviour."
            );

            BindRemainingModifiers(
                _spring,
                "Spring"
            );
        }

        private void ConfigureSummer()
        {
            BindSeasonModifier(
                _summer,
                "Summer",
                "Fire",
                70f,
                "Heat and fire-associated creatures become more common."
            );

            BindSeasonModifier(
                _summer,
                "Summer",
                "FireNova",
                20f,
                "Rare creatures capable of producing bursts of fire."
            );

            BindSeasonModifier(
                _summer,
                "Summer",
                "ResistFire",
                20f,
                "Creatures adapted to extreme summer heat."
            );

            BindSeasonModifier(
                _summer,
                "Summer",
                "Lightning",
                15f,
                "Summer storms can produce unusual lightning-infused creatures."
            );

            BindSeasonModifier(
                _summer,
                "Summer",
                "Fast",
                15f,
                "Increased activity during the warm season."
            );

            BindSeasonModifier(
                _summer,
                "Summer",
                "Brutal",
                10f,
                "Some creatures become more aggressive and physically powerful."
            );

            BindSeasonModifier(
                _summer,
                "Summer",
                "Big",
                10f,
                "Warm conditions can produce unusually large creatures."
            );

            BindRemainingModifiers(
                _summer,
                "Summer"
            );
        }

        private void ConfigureAutumn()
        {
            BindSeasonModifier(
                _autumn,
                "Autumn",
                "Lightning",
                40f,
                "Autumn storms bring increased lightning activity."
            );

            BindSeasonModifier(
                _autumn,
                "Autumn",
                "Poison",
                30f,
                "Decay and toxins become more prevalent."
            );

            BindSeasonModifier(
                _autumn,
                "Autumn",
                "ElementalChaos",
                20f,
                "Unstable elemental conditions during seasonal transition."
            );

            BindSeasonModifier(
                _autumn,
                "Autumn",
                "SoulEater",
                15f,
                "A rare supernatural effect associated with death and decay."
            );

            BindSeasonModifier(
                _autumn,
                "Autumn",
                "ResistSpirit",
                10f,
                "Creatures becoming resistant to spiritual forces."
            );

            BindSeasonModifier(
                _autumn,
                "Autumn",
                "Alert",
                15f,
                "Creatures become more alert as conditions worsen."
            );

            BindSeasonModifier(
                _autumn,
                "Autumn",
                "ResistPoison",
                10f,
                "Creatures adapted to seasonal decay and toxins."
            );

            BindSeasonModifier(
                _autumn,
                "Autumn",
                "Big",
                5f,
                "Rare unusually large creatures."
            );

            BindRemainingModifiers(
                _autumn,
                "Autumn"
            );
        }

        private void BindRemainingModifiers(
            Dictionary<string, ConfigEntry<float>> dictionary,
            string section)
        {
            foreach (string modifier in AllModifiers)
            {
                if (dictionary.ContainsKey(modifier))
                    continue;

                dictionary[modifier] = BindSeasonModifier(
                    dictionary,
                    section,
                    modifier,
                    0f,
                    $"Enable {modifier} as a seasonal modifier for {section}."
                ).ConfigEntry;
            }
        }

        private SeasonBindingResult BindSeasonModifier(
            Dictionary<string, ConfigEntry<float>> dictionary,
            string section,
            string modifier,
            float defaultValue,
            string description)
        {
            if (dictionary.ContainsKey(modifier))
            {
                return new SeasonBindingResult(
                    dictionary[modifier]
                );
            }

            ConfigEntry<float> entry = Config.Bind(
                section,
                modifier + "Chance",
                defaultValue,
                description +
                " Value is a percentage weight from 0 to 100. Set to 0 to disable."
            );

            dictionary[modifier] = entry;

            return new SeasonBindingResult(entry);
        }

        private sealed class SeasonBindingResult
        {
            public ConfigEntry<float> ConfigEntry { get; }

            public SeasonBindingResult(
                ConfigEntry<float> configEntry)
            {
                ConfigEntry = configEntry;
            }
        }

        /*
         * ============================================================
         * SEASON DETECTION
         * ============================================================
         */

        internal static string GetSeason()
        {
            if (ZoneSystem.instance == null)
                return null;

            if (ZoneSystem.instance.GetGlobalKey("season_winter"))
                return "winter";

            if (ZoneSystem.instance.GetGlobalKey("season_spring"))
                return "spring";

            if (ZoneSystem.instance.GetGlobalKey("season_summer"))
                return "summer";

            if (ZoneSystem.instance.GetGlobalKey("season_fall"))
                return "autumn";

            return null;
        }

        /*
         * ============================================================
         * SEASONAL POOLS
         * ============================================================
         */

        internal static List<string> GetSeasonalPool(
            string season)
        {
            Dictionary<string, ConfigEntry<float>> dictionary =
                GetSeasonDictionary(season);

            if (dictionary == null)
                return new List<string>();

            return dictionary
                .Where(
                    x => Mathf.Clamp(
                        x.Value.Value,
                        0f,
                        100f
                    ) > 0f
                )
                .Select(
                    x => x.Key
                )
                .ToList();
        }

        /*
         * ============================================================
         * MODIFIER ROLL
         * ============================================================
         */

        internal static string RollModifier(
            string season)
        {
            if (Instance == null)
                return null;

            float globalChance =
                Mathf.Clamp(
                    Instance.SeasonalRollChance.Value,
                    0f,
                    100f
                );

            if (UnityEngine.Random.Range(
                    0f,
                    100f
                ) >= globalChance)
            {
                return null;
            }

            Dictionary<string, ConfigEntry<float>> dictionary =
                GetSeasonDictionary(season);

            if (dictionary == null ||
                dictionary.Count == 0)
            {
                return null;
            }

            var candidates =
                new List<Tuple<string, float>>();

            foreach (KeyValuePair<string, ConfigEntry<float>> pair
                     in dictionary)
            {
                float weight =
                    Mathf.Clamp(
                        pair.Value.Value,
                        0f,
                        100f
                    );

                if (weight <= 0f)
                    continue;

                candidates.Add(
                    Tuple.Create(
                        pair.Key,
                        weight
                    )
                );
            }

            if (candidates.Count == 0)
                return null;

            float total =
                candidates.Sum(
                    x => x.Item2
                );

            if (total <= 0f)
                return null;

            float roll =
                UnityEngine.Random.Range(
                    0f,
                    Mathf.Max(
                        100f,
                        total
                    )
                );

            if (roll >= total)
                return null;

            float cursor = 0f;

            foreach (Tuple<string, float> candidate
                     in candidates)
            {
                cursor += candidate.Item2;

                if (roll < cursor)
                    return candidate.Item1;
            }

            return null;
        }

        private static Dictionary<string, ConfigEntry<float>>
            GetSeasonDictionary(string season)
        {
            switch (season)
            {
                case "winter":
                    return Instance._winter;

                case "spring":
                    return Instance._spring;

                case "summer":
                    return Instance._summer;

                case "autumn":
                    return Instance._autumn;

                default:
                    return null;
            }
        }

        /*
         * ============================================================
         * MODIFIER CATEGORY
         * ============================================================
         */

        internal static bool IsMajorModifier(
            string modifierName)
        {
            return MajorModifiers.Contains(
                modifierName,
                StringComparer.OrdinalIgnoreCase
            );
        }

        internal static bool IsMinorModifier(
            string modifierName)
        {
            return MinorModifiers.Contains(
                modifierName,
                StringComparer.OrdinalIgnoreCase
            );
        }

        internal static string GetModifierCategory(
            string modifierName)
        {
            if (IsMajorModifier(modifierName))
                return "Major";

            if (IsMinorModifier(modifierName))
                return "Minor";

            return "Unknown";
        }
    }

    /*
     * ================================================================
     * CHARACTER SPAWN PATCH
     * ================================================================
     */

    [HarmonyPatch(typeof(Character), "Awake")]
    internal static class CharacterAwakePatch
    {
        private static readonly HashSet<int> Queued =
            new HashSet<int>();

        private static void Postfix(
            Character __instance)
        {
            if (__instance == null)
                return;

            if (__instance.IsPlayer())
                return;

            if (!Plugin.Instance.IncludeTamed.Value &&
                __instance.IsTamed())
            {
                return;
            }

            int id =
                __instance.GetInstanceID();

            lock (Queued)
            {
                if (!Queued.Add(id))
                    return;
            }

            Plugin.Instance.StartCoroutine(
                RollAfterDelay(
                    __instance,
                    id
                )
            );
        }

        private static IEnumerator RollAfterDelay(
            Character creature,
            int id)
        {
            yield return new WaitForSeconds(
                Mathf.Max(
                    0f,
                    Plugin.Instance.DelaySeconds.Value
                )
            );

            lock (Queued)
            {
                Queued.Remove(id);
            }

            if (creature == null)
                yield break;

            if (creature.IsPlayer())
                yield break;

            if (!Plugin.Instance.IncludeTamed.Value &&
                creature.IsTamed())
            {
                yield break;
            }

            string season =
                Plugin.GetSeason();

            if (string.IsNullOrEmpty(season))
                yield break;

            /*
             * Select exactly one seasonal modifier.
             */

            string modifier =
                Plugin.RollModifier(
                    season
                );

            if (string.IsNullOrEmpty(modifier))
                yield break;

            /*
             * Apply through the existing SLS reflection system.
             *
             * NOTHING inside SLS is modified here.
             */

            if (SLSReflection.TryAddModifier(
                    creature,
                    modifier,
                    out string detail))
            {
                /*
                 * SLS successfully accepted the seasonal modifier.
                 *
                 * Only now do we register the creature as seasonal.
                 */

                Plugin.RegisterSeasonalCreature(
                    creature,
                    season
                );

                if (Plugin.Instance.LogAppliedModifiers.Value)
                {
                    Plugin.Log.LogInfo(
                        $"Seasonal modifier: {modifier} " +
                        $"({Plugin.GetModifierCategory(modifier)}) " +
                        $"-> {creature.m_name} " +
                        $"[{season}] {detail}"
                    );
                }
            }
        }
    }

    /*
     * ================================================================
     * SEASONAL NAME PATCH
     * ================================================================
     *
     * Adds a seasonal title to creatures that actually received
     * a seasonal modifier.
     *
     * Examples:
     *
     * Invernal Esqueleto [Poison]
     * Primaveral Greydwarf [Poison]
     * Estival Draugr [Fire]
     * Otoñal Greydwarf [Lightning]
     *
     * SLS continues generating its own name normally.
     */

    [HarmonyPatch(typeof(Character), "GetHoverName")]
    internal static class CharacterHoverNamePatch
    {
        private static void Postfix(
            Character __instance,
            ref string __result)
        {
            if (__instance == null)
                return;

            if (string.IsNullOrEmpty(__result))
                return;

            string seasonalTitle =
                Plugin.GetSeasonalTitle(
                    __instance
                );

            if (string.IsNullOrEmpty(seasonalTitle))
                return;

            /*
             * Prevent duplication if Valheim calls GetHoverName
             * repeatedly on the same creature.
             */

            if (__result.StartsWith(
                    seasonalTitle + " ",
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            __result =
                seasonalTitle +
                " " +
                __result;
        }
    }

    /*
     * ================================================================
     * SEASONAL NAME REGISTRY CLEANUP
     * ================================================================
     *
     * Removes destroyed creatures from the seasonal registry.
     */

    [HarmonyPatch(typeof(Character), "OnDestroy")]
    internal static class CharacterDestroyPatch
    {
        private static void Prefix(
            Character __instance)
        {
            Plugin.UnregisterSeasonalCreature(
                __instance
            );
        }
    }

    /*
     * ================================================================
     * STAR LEVEL SYSTEM REFLECTION
     * ================================================================
     */

    internal static class SLSReflection
    {
        private static readonly List<MethodInfo> Candidates =
            new List<MethodInfo>();

        private static bool _loggedFailure;

        /*
         * Discover AddModifier methods exposed by SLS.
         */

        public static void Discover(
            bool log)
        {
            Candidates.Clear();

            foreach (Assembly asm
                     in AppDomain.CurrentDomain.GetAssemblies())
            {
                string name =
                    asm.GetName().Name ??
                    string.Empty;

                if (!name.Contains(
                        "StarLevel",
                        StringComparison.OrdinalIgnoreCase) &&
                    !name.Contains(
                        "SLS",
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                Type[] types;

                try
                {
                    types = asm.GetTypes();
                }
                catch
                {
                    continue;
                }

                foreach (Type type in types)
                {
                    foreach (MethodInfo method
                             in type.GetMethods(
                                 BindingFlags.Public |
                                 BindingFlags.NonPublic |
                                 BindingFlags.Static |
                                 BindingFlags.Instance))
                    {
                        if (!string.Equals(
                                method.Name,
                                "AddModifierToCreature",
                                StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(
                                method.Name,
                                "AddModifierToTargetCreature",
                                StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        Candidates.Add(
                            method
                        );

                        if (!log)
                            continue;

                        string parameters =
                            string.Join(
                                ", ",
                                method.GetParameters()
                                    .Select(
                                        p =>
                                            p.ParameterType.FullName +
                                            " " +
                                            p.Name
                                    )
                            );

                        Plugin.Log.LogInfo(
                            $"[SLS API] " +
                            $"{type.FullName}." +
                            $"{method.Name}(" +
                            $"{parameters}) -> " +
                            $"{method.ReturnType.FullName}"
                        );
                    }
                }
            }

            Plugin.Log.LogInfo(
                $"[SLS API] Found " +
                $"{Candidates.Count} " +
                $"modifier-add methods."
            );
        }

        /*
         * Try to add the selected modifier.
         */

        public static bool TryAddModifier(
            Character creature,
            string modifierName,
            out string detail)
        {
            detail = string.Empty;

            foreach (MethodInfo method
                     in Candidates)
            {
                if (!TryBuildArguments(
                        method,
                        creature,
                        modifierName,
                        out object target,
                        out object[] args))
                {
                    continue;
                }

                try
                {
                    object result =
                        method.Invoke(
                            target,
                            args
                        );

                    detail =
                        $"via " +
                        $"{method.DeclaringType?.FullName}." +
                        $"{method.Name}";

                    if (result is bool b &&
                        !b)
                    {
                        continue;
                    }

                    return true;
                }
                catch (TargetInvocationException ex)
                {
                    if (Plugin.Instance.LogApiDiscovery.Value)
                    {
                        Plugin.Log.LogDebug(
                            $"[SLS API] " +
                            $"{method.Name} rejected call: " +
                            $"{ex.InnerException?.Message ?? ex.Message}"
                        );
                    }
                }
                catch (Exception ex)
                {
                    if (Plugin.Instance.LogApiDiscovery.Value)
                    {
                        Plugin.Log.LogDebug(
                            $"[SLS API] " +
                            $"{method.Name} failed: " +
                            $"{ex.Message}"
                        );
                    }
                }
            }

            if (!_loggedFailure)
            {
                _loggedFailure = true;

                Plugin.Log.LogWarning(
                    "[SLS API] No compatible AddModifier method could be invoked. The bridge will remain inactive until a compatible API is available."
                );
            }

            return false;
        }

        /*
         * Build arguments for the discovered SLS method.
         */

        private static bool TryBuildArguments(
            MethodInfo method,
            Character creature,
            string modifierName,
            out object target,
            out object[] args)
        {
            target = null;

            ParameterInfo[] parameters =
                method.GetParameters();

            args =
                new object[
                    parameters.Length
                ];

            int stringCount = 0;
            bool hasCreature = false;

            for (int i = 0;
                 i < parameters.Length;
                 i++)
            {
                Type t =
                    parameters[i].ParameterType;

                /*
                 * Character argument.
                 */

                if (typeof(Character).IsAssignableFrom(t))
                {
                    args[i] =
                        creature;

                    hasCreature = true;
                }

                /*
                 * Modifier name.
                 */

                else if (t == typeof(string))
                {
                    args[i] =
                        modifierName;

                    stringCount++;
                }

                /*
                 * SLS update flag.
                 */

                else if (t == typeof(bool))
                {
                    args[i] =
                        false;
                }

                /*
                 * Enum compatibility.
                 */

                else if (t.IsEnum)
                {
                    string[] names =
                        Enum.GetNames(t);

                    string major =
                        names.FirstOrDefault(
                            n =>
                                string.Equals(
                                    n,
                                    "Major",
                                    StringComparison.OrdinalIgnoreCase
                                )
                        );

                    if (major == null)
                        return false;

                    args[i] =
                        Enum.Parse(
                            t,
                            major
                        );
                }

                /*
                 * Optional/default arguments.
                 */

                else if (parameters[i].HasDefaultValue)
                {
                    args[i] =
                        parameters[i].DefaultValue;
                }

                /*
                 * Reference types.
                 */

                else if (!t.IsValueType)
                {
                    args[i] =
                        null;
                }

                /*
                 * Current known SLS modifierType argument.
                 */

                else if (t == typeof(int))
                {
                    args[i] =
                        0;
                }

                /*
                 * Float compatibility.
                 */

                else if (t == typeof(float))
                {
                    args[i] =
                        0f;
                }

                else
                {
                    return false;
                }
            }

            /*
             * We need exactly one Character and exactly one string
             * modifier name.
             */

            if (!hasCreature ||
                stringCount != 1)
            {
                return false;
            }

            /*
             * The discovered methods must be static.
             */

            if (!method.IsStatic)
                return false;

            return true;
        }
    }
}