using System.Collections;
using Il2CppTLD.AI;
using UnityEngine.AI;
using UnityEngine.Rendering;

[assembly: MelonInfo(typeof(Spirits___Halloween_Special.Core), "Spirits - Halloween Special", "1.0.0", "EtherSystem", null)]
[assembly: MelonGame("Hinterland", "TheLongDark")]

namespace Spirits___Halloween_Special
{
    public class Core : MelonMod
    {
        public static Core Instance { get; private set; }

        internal static void Log(string message, bool onlyWhenDebugEnabled = true)
        {
            if (onlyWhenDebugEnabled && !SpiritsSettingsManager.Options.IsLogging) return;

            Instance?.LoggerInstance.Msg(message);
        }

        internal static void Warn(string message, bool onlyWhenDebugEnabled = true)
        {
            if (onlyWhenDebugEnabled && !SpiritsSettingsManager.Options.IsLogging) return;

            Instance?.LoggerInstance.Warning(message);
        }

        internal static void Error(string message, bool onlyWhenDebugEnabled = false)
        {
            if (onlyWhenDebugEnabled && !SpiritsSettingsManager.Options.IsLogging) return;

            Instance?.LoggerInstance.Error(message);
        }

        public override void OnInitializeMelon()
        {
            Instance = this;
            SpiritsSettingsManager.Initialize();
            Log("Initialized.", false);
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            GhostAnimalEvent.Cleanup();
            GhostDeathTracker.Reset();
            GhostAppearanceManager.PruneDestroyed();
            GhostAmbushManager.Reset();
        }

        public override void OnUpdate()
        {
            GhostAnimalEvent.CheckAmbushSafety();
            GhostAmbushManager.Update();
        }

        public override void OnFixedUpdate()
        {
            GhostAnimalEvent.CheckAmbushSafety();
        }
    }

    internal enum GhostSpecies
    {
        Bear,
        Wolf,
        Moose,
        Cougar,
        Timberwolf,
        StarvingWolf,
        Stag,
        Doe,
        Rabbit,
        Ptarmigan
    }

    internal enum GhostBehavior
    {
        Undecided,
        Attack,
        Flee
    }

    internal sealed class GhostAnimalDefinition
    {
        internal readonly GhostSpecies Species;
        internal readonly string DisplayName;
        internal readonly string ObjectName;
        internal readonly bool Passive;

        internal GhostAnimalDefinition(GhostSpecies species, string displayName, string objectName, bool passive = false)
        {
            Species = species;
            DisplayName = displayName;
            ObjectName = objectName;
            Passive = passive;
        }
    }

    internal sealed class GhostDeathInfo
    {
        internal readonly GhostAnimalDefinition Definition;
        internal readonly Vector3 Position;

        internal GhostDeathInfo(GhostAnimalDefinition definition, Vector3 position)
        {
            Definition = definition;
            Position = position;
        }
    }

    internal sealed class GhostSpawnRequest
    {
        internal readonly GhostAnimalDefinition Definition;
        internal readonly Vector3? Position;
        internal readonly bool IsAmbush;

        internal GhostSpawnRequest(GhostAnimalDefinition definition, Vector3? position, bool isAmbush)
        {
            Definition = definition;
            Position = position;
            IsAmbush = isAmbush;
        }
    }

    internal sealed class GhostChargeInstance
    {
        internal readonly GhostAnimalDefinition Definition;
        internal readonly BaseAi Ai;
        internal readonly GameObject GameObject;
        internal readonly float StartedAt;
        internal readonly bool ForceAttack;
        internal float AttackStartedAt = -1f;
        internal bool Despawning;
        internal bool Spawning = true;
        internal GhostBehavior Behavior;
        internal float NextBehaviorCheckAt;
        internal float LastBehaviorRequestAt;
        internal bool BehaviorFailureLogged;
        internal float CurrentAlpha;
        internal readonly List<Material> Materials = new List<Material>();
        internal float ClosestApproachDistance = float.MaxValue;

        internal GhostChargeInstance(GhostAnimalDefinition definition, BaseAi ai, bool forceAttack)
        {
            Definition = definition;
            Ai = ai;
            GameObject = ai.gameObject;
            ForceAttack = forceAttack;
            StartedAt = Time.realtimeSinceStartup;
        }
    }

    internal static class GhostAnimalDefinitions
    {
        internal static readonly GhostAnimalDefinition Bear = new GhostAnimalDefinition(GhostSpecies.Bear, "Ghost Bear", "SPIRITS_GhostBear");
        internal static readonly GhostAnimalDefinition Wolf = new GhostAnimalDefinition(GhostSpecies.Wolf, "Ghost Wolf", "SPIRITS_GhostWolf");
        internal static readonly GhostAnimalDefinition Moose = new GhostAnimalDefinition(GhostSpecies.Moose, "Ghost Moose", "SPIRITS_GhostMoose");
        internal static readonly GhostAnimalDefinition Cougar = new GhostAnimalDefinition(GhostSpecies.Cougar, "Ghost Cougar", "SPIRITS_GhostCougar");
        internal static readonly GhostAnimalDefinition Timberwolf = new GhostAnimalDefinition(GhostSpecies.Timberwolf, "Ghost Timberwolf", "SPIRITS_GhostTimberwolf");
        internal static readonly GhostAnimalDefinition StarvingWolf = new GhostAnimalDefinition(GhostSpecies.StarvingWolf, "Ghost Starving Wolf", "SPIRITS_GhostStarvingWolf");
        internal static readonly GhostAnimalDefinition Stag = new GhostAnimalDefinition(GhostSpecies.Stag, "Ghost Stag", "SPIRITS_GhostStag", true);
        internal static readonly GhostAnimalDefinition Doe = new GhostAnimalDefinition(GhostSpecies.Doe, "Ghost Doe", "SPIRITS_GhostDoe", true);
        internal static readonly GhostAnimalDefinition Rabbit = new GhostAnimalDefinition(GhostSpecies.Rabbit, "Ghost Rabbit", "SPIRITS_GhostRabbit", true);
        internal static readonly GhostAnimalDefinition Ptarmigan = new GhostAnimalDefinition(GhostSpecies.Ptarmigan, "Ghost Ptarmigan", "SPIRITS_GhostPtarmigan", true);

        internal static bool TryGetForKilledAnimal(BaseAi ai, out GhostAnimalDefinition definition)
        {
            definition = null;
            if (ai == null) return false;
            if (ai.Ptarmigan != null) definition = Ptarmigan;
            else if (ai.Rabbit != null) definition = Rabbit;
            else if (ai.Doe != null) definition = Doe;
            else if (ai.Stag != null) definition = Stag;
            else if (ai.Cougar != null || ai.m_AiSubType == AiSubType.Cougar) definition = Cougar;
            else if (ai.Moose != null || ai.m_AiSubType == AiSubType.Moose) definition = Moose;
            else if (ai.Bear != null || ai.m_AiSubType == AiSubType.Bear) definition = Bear;
            else if (ai.Timberwolf != null) definition = Timberwolf;
            else if (ai.StarvingWolf != null) definition = StarvingWolf;
            else if (ai.m_AiSubType == AiSubType.Wolf) definition = Wolf;
            return definition != null;
        }

        internal static bool Matches(BaseAi ai, GhostAnimalDefinition definition)
        {
            if (ai == null || definition == null) return false;
            switch (definition.Species)
            {
                case GhostSpecies.Ptarmigan: return ai.Ptarmigan != null;
                case GhostSpecies.Rabbit: return ai.Rabbit != null;
                case GhostSpecies.Doe: return ai.Doe != null;
                case GhostSpecies.Stag: return ai.Stag != null;
                case GhostSpecies.Cougar: return ai.Cougar != null || ai.m_AiSubType == AiSubType.Cougar;
                case GhostSpecies.Moose: return ai.Moose != null || ai.m_AiSubType == AiSubType.Moose;
                case GhostSpecies.Bear: return ai.Bear != null || ai.m_AiSubType == AiSubType.Bear;
                case GhostSpecies.Timberwolf: return ai.Timberwolf != null;
                case GhostSpecies.StarvingWolf: return ai.StarvingWolf != null;
                default: return ai.m_AiSubType == AiSubType.Wolf && ai.Timberwolf == null && ai.StarvingWolf == null;
            }
        }
    }

    internal static class GhostDeathTracker
    {
        private static readonly HashSet<int> s_PlayerDamagedAnimals = new HashSet<int>();

        internal static void Reset()
        {
            s_PlayerDamagedAnimals.Clear();
        }

        internal static void TrackPlayerDamage(BaseAi ai)
        {
            if (!SpiritsSettingsManager.EnableGhostSpawns || ai == null || GhostAnimalEvent.IsGhostAnimal(ai)) return;
            if (!GhostAnimalDefinitions.TryGetForKilledAnimal(ai, out _)) return;
            s_PlayerDamagedAnimals.Add(ai.GetInstanceID());
        }

        internal static GhostDeathInfo CaptureDeath(BaseAi ai)
        {
            if (ai == null || GhostAnimalEvent.IsGhostAnimal(ai)) return null;
            int id = ai.GetInstanceID();
            if (!s_PlayerDamagedAnimals.Remove(id)) return null;
            if (!SpiritsSettingsManager.EnableGhostSpawns) return null;
            if (!GhostAnimalDefinitions.TryGetForKilledAnimal(ai, out GhostAnimalDefinition definition)) return null;
            return new GhostDeathInfo(definition, ai.transform.position);
        }
    }

    internal static class GhostAmbushManager
    {
        private const float MinimumSpawnDistance = 24f;
        private const float MaximumSpawnDistance = 32f;
        private const float MaximumBehindAngle = 35f;
        private const int PlacementAttempts = 20;
        private const int FailedPlacementRetryInGameSeconds = 300;

        private static readonly List<GhostAnimalDefinition> s_Eligible = new List<GhostAnimalDefinition>(4);
        private static double s_InGameSecondsRemaining = -1d;
        private static float s_LastHoursPlayedNotPaused = float.NaN;

        internal static void Reset()
        {
            s_InGameSecondsRemaining = -1d;
            s_LastHoursPlayedNotPaused = float.NaN;
        }

        internal static void OnSettingsConfirmed()
        {
            Reset();
        }

        internal static bool IsOutdoors()
        {
            Transform player = GameManager.GetPlayerTransform();
            Weather weather = GameManager.GetWeatherComponent();
            if (player == null || player.gameObject == null || !player.gameObject.activeInHierarchy || weather == null) return false;
            return !weather.IsIndoorScene() && !weather.IsIndoorEnvironment();
        }

        internal static void TriggerNow()
        {
            if (!IsOutdoors())
            {
                Core.Warn("Random ambush test requires an outdoor scene and outdoor environment.");
                return;
            }
            if (GhostAnimalEvent.HasActiveOrQueuedGhosts)
            {
                Core.Warn("Random ambush test: a ghost is already active or queued.");
                return;
            }
            GhostAnimalDefinition species = ChooseSpecies();
            if (species == null)
            {
                Core.Warn("Random ambush test: enable at least one predator species in settings.");
                return;
            }
            if (!TryFindSpawnPoint(out Vector3 position))
            {
                Core.Warn("Random ambush test: no safe navmesh spawn position found behind the player.");
                return;
            }
            GhostAnimalEvent.TriggerAmbush(species, position);
        }

        internal static void Update()
        {
            if (!SpiritsSettingsManager.Options.EnableRandomAmbushes)
            {
                Reset();
                return;
            }

            TimeOfDay timeOfDay = GameManager.GetTimeOfDayComponent();
            if (timeOfDay == null)
            {
                s_LastHoursPlayedNotPaused = float.NaN;
                return;
            }

            float hoursPlayed = timeOfDay.GetHoursPlayedNotPaused();
            if (float.IsNaN(hoursPlayed) || float.IsInfinity(hoursPlayed))
            {
                s_LastHoursPlayedNotPaused = float.NaN;
                return;
            }

            if (float.IsNaN(s_LastHoursPlayedNotPaused))
            {
                s_LastHoursPlayedNotPaused = hoursPlayed;
                return;
            }
            double elapsedInGameSeconds = ((double)hoursPlayed - s_LastHoursPlayedNotPaused) * 3600d;
            s_LastHoursPlayedNotPaused = hoursPlayed;
            if (elapsedInGameSeconds < 0d)
            {
                s_InGameSecondsRemaining = -1d;
                return;
            }
            if (!IsOutdoors() || Time.timeScale <= 0f) return;
            if (s_InGameSecondsRemaining < 0d) ScheduleNext();
            s_InGameSecondsRemaining -= elapsedInGameSeconds;
            if (s_InGameSecondsRemaining > 0d) return;

            if (GhostAnimalEvent.HasActiveOrQueuedGhosts) return;
            ScheduleNext();
            GhostAnimalDefinition species = ChooseSpecies();
            if (species == null) return;
            if (!TryFindSpawnPoint(out Vector3 position))
            {
                s_InGameSecondsRemaining = FailedPlacementRetryInGameSeconds;
                return;
            }
            GhostAnimalEvent.TriggerAmbush(species, position);
        }

        private static void ScheduleNext()
        {
            SpiritsSettings options = SpiritsSettingsManager.Options;
            int minimumSeconds = Mathf.Clamp(Mathf.RoundToInt(options.AmbushMinInGameMinutes), 30, 60) * 60;
            int maximumSeconds = Mathf.Clamp(options.AmbushMaxHours, 1, 24) * 3600;
            s_InGameSecondsRemaining = UnityEngine.Random.Range(minimumSeconds, maximumSeconds + 1);
        }

        private static GhostAnimalDefinition ChooseSpecies()
        {
            SpiritsSettings options = SpiritsSettingsManager.Options;
            s_Eligible.Clear();
            if (options.AmbushCougars) s_Eligible.Add(GhostAnimalDefinitions.Cougar);
            if (options.AmbushWolves) s_Eligible.Add(GhostAnimalDefinitions.Wolf);
            if (options.AmbushBears) s_Eligible.Add(GhostAnimalDefinitions.Bear);
            if (options.AmbushMoose) s_Eligible.Add(GhostAnimalDefinitions.Moose);
            return s_Eligible.Count == 0 ? null : s_Eligible[UnityEngine.Random.Range(0, s_Eligible.Count)];
        }

        private static bool TryFindSpawnPoint(out Vector3 location)
        {
            location = Vector3.zero;
            Transform player = GameManager.GetPlayerTransform();
            if (player == null) return false;
            Vector3 behind = -player.forward;
            behind.y = 0f;
            if (behind.sqrMagnitude < 0.01f) return false;
            behind.Normalize();

            if (!NavMesh.SamplePosition(player.position, out NavMeshHit playerNav, 3f, NavMesh.AllAreas)) return false;
            for (int i = 0; i < PlacementAttempts; i++)
            {
                float angle = UnityEngine.Random.Range(-MaximumBehindAngle, MaximumBehindAngle);
                float distance = UnityEngine.Random.Range(MinimumSpawnDistance, MaximumSpawnDistance);
                Vector3 direction = Quaternion.Euler(0f, angle, 0f) * behind;
                Vector3 candidate = player.position + direction * distance;
                if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2.5f, NavMesh.AllAreas)) continue;
                Vector3 navPosition = hit.position;
                Vector3 flatOffset = navPosition - player.position;
                flatOffset.y = 0f;
                float horizontalDistance = flatOffset.magnitude;
                if (horizontalDistance < 22f || horizontalDistance > 35f) continue;
                if (Mathf.Abs(navPosition.y - player.position.y) > 5f) continue;
                if (Vector3.Dot(flatOffset.normalized, behind) < 0.75f) continue;
                if (!Physics.Raycast(navPosition + Vector3.up * 2f, Vector3.down, out RaycastHit ground, 3f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) continue;
                if (Vector3.Dot(ground.normal, Vector3.up) < 0.60f || Mathf.Abs(ground.point.y - navPosition.y) > 0.7f) continue;
                if (Physics.CheckCapsule(navPosition + Vector3.up * 1.1f, navPosition + Vector3.up * 2.5f, 1.0f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) continue;
                NavMeshPath path = new NavMeshPath();
                if (!NavMesh.CalculatePath(navPosition, playerNav.position, NavMesh.AllAreas, path) || path.status != NavMeshPathStatus.PathComplete) continue;
                float pathLength = 0f;
                for (int p = 1; p < path.corners.Length; p++) pathLength += Vector3.Distance(path.corners[p - 1], path.corners[p]);
                if (pathLength > 55f) continue;
                location = navPosition;
                return true;
            }
            return false;
        }
    }

    internal static class GhostAnimalEvent
    {
        private const float FadeStartDistance = 8f;
        private const float DespawnDistance = 4f;
        private const float SpawnFadeSeconds = 2f;
        private const float DespawnFadeSeconds = 0.95f;
        private const float TimeoutFadeSeconds = 1.35f;
        private const float MaximumLifetimeSeconds = 20f;
        private const float ProximityGraceSeconds = 0.75f;
        private const int SpawnDetectionFrames = 60;

        private static readonly Dictionary<int, GhostChargeInstance> s_ActiveGhosts = new Dictionary<int, GhostChargeInstance>();
        private static readonly Queue<GhostSpawnRequest> s_SpawnQueue = new Queue<GhostSpawnRequest>();
        private static bool s_ProcessingSpawnQueue;
        private static int s_SpawnGeneration;

        internal static bool HasActiveOrQueuedGhosts => s_ActiveGhosts.Count > 0 || s_SpawnQueue.Count > 0 || s_ProcessingSpawnQueue;

        internal static void TriggerAmbush(GhostAnimalDefinition definition, Vector3 location)
        {
            if (definition == null || !GhostAmbushManager.IsOutdoors()) return;
            EnqueueSpawn(definition, location, true);
        }

        internal static void TriggerBear() => EnqueueSpawn(GhostAnimalDefinitions.Bear, null);
        internal static void TriggerWolf() => EnqueueSpawn(GhostAnimalDefinitions.Wolf, null);
        internal static void TriggerMoose() => EnqueueSpawn(GhostAnimalDefinitions.Moose, null);
        internal static void TriggerCougar() => EnqueueSpawn(GhostAnimalDefinitions.Cougar, null);
        internal static void TriggerTimberwolf() => EnqueueSpawn(GhostAnimalDefinitions.Timberwolf, null);
        internal static void TriggerStarvingWolf() => EnqueueSpawn(GhostAnimalDefinitions.StarvingWolf, null);
        internal static void TriggerStag() => EnqueueSpawn(GhostAnimalDefinitions.Stag, null);
        internal static void TriggerDoe() => EnqueueSpawn(GhostAnimalDefinitions.Doe, null);
        internal static void TriggerRabbit() => EnqueueSpawn(GhostAnimalDefinitions.Rabbit, null);
        internal static void TriggerPtarmigan() => EnqueueSpawn(GhostAnimalDefinitions.Ptarmigan, null);

        internal static void TriggerFromDeath(GhostDeathInfo deathInfo, Vector3 corpsePosition)
        {
            if (deathInfo == null) return;
            EnqueueSpawn(deathInfo.Definition, corpsePosition, false);
        }

        internal static bool IsGhostAnimal(BaseAi ai)
        {
            return ai != null && s_ActiveGhosts.ContainsKey(ai.GetInstanceID());
        }

        internal static void CheckAmbushSafety()
        {
            if (s_ActiveGhosts.Count == 0) return;
            Transform player = GameManager.GetPlayerTransform();
            if (player == null) return;
            Vector3 playerPosition = player.position;
            GhostChargeInstance closeAmbush = null;
            foreach (GhostChargeInstance instance in s_ActiveGhosts.Values)
            {
                if (!instance.ForceAttack || instance.Despawning || instance.Ai == null) continue;
                Vector3 difference = instance.Ai.transform.position - playerPosition;
                difference.y = 0f;
                if (difference.sqrMagnitude > DespawnDistance * DespawnDistance) continue;
                closeAmbush = instance;
                break;
            }
            if (closeAmbush == null) return;
            closeAmbush.CurrentAlpha = 0f;
            GhostVisuals.SetAlpha(closeAmbush.Materials, 0f);
            Despawn(closeAmbush.Ai, "within 4.0m of player");
        }

        internal static bool Despawn(BaseAi ai, string reason)
        {
            if (ai == null) return false;
            if (!s_ActiveGhosts.TryGetValue(ai.GetInstanceID(), out GhostChargeInstance instance)) return false;
            if (instance.Despawning) return true;
            instance.Despawning = true;
            Core.Log($"{instance.Definition.DisplayName} vanished ({reason}).");
            try
            {
                DisableGhost(instance);
                MelonCoroutines.Start(FadeGhost(instance, reason == "20 second timeout" ? TimeoutFadeSeconds : instance.CurrentAlpha <= 0.001f ? 0.05f : DespawnFadeSeconds, s_SpawnGeneration));
            }
            catch (Exception ex)
            {
                Core.Error($"{instance.Definition.DisplayName}: despawn effect failed: {ex}");
                Unregister(instance);
                if (instance.GameObject != null) UnityEngine.Object.Destroy(instance.GameObject);
            }
            return true;
        }

        private static void DisableGhost(GhostChargeInstance instance)
        {
            if (instance.Ai != null)
            {
                instance.Ai.m_CurrentTarget = null;
                instance.Ai.enabled = false;
            }
            if (instance.GameObject == null) return;
            foreach (Collider collider in instance.GameObject.GetComponentsInChildren<Collider>(true))
            {
                if (collider != null) collider.enabled = false;
            }
            foreach (Animator animator in instance.GameObject.GetComponentsInChildren<Animator>(true))
            {
                if (animator != null) animator.enabled = false;
            }
        }

        private static IEnumerator FadeGhost(GhostChargeInstance instance, float duration, int generation)
        {
            List<Material> materials = instance.Materials;
            float startAlpha = instance.CurrentAlpha;
            float elapsed = 0f;
            while (elapsed < duration && generation == s_SpawnGeneration && instance.GameObject != null)
            {
                instance.CurrentAlpha = startAlpha * (1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration)));
                GhostVisuals.SetAlpha(materials, instance.CurrentAlpha);
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (generation != s_SpawnGeneration) yield break;
            Unregister(instance);
            if (instance.GameObject != null) UnityEngine.Object.Destroy(instance.GameObject);
        }

        private static IEnumerator SpawnGhost(GhostChargeInstance instance, int generation)
        {
            List<Material> materials = instance.Materials;
            float targetAlpha = SpiritsSettingsManager.GetSpawnAlpha(instance.Definition.Species);
            instance.CurrentAlpha = 0f;
            GhostVisuals.SetAlpha(materials, 0f);
            float startedAt = Time.realtimeSinceStartup;
            float nextRefreshAt = startedAt;
            while (generation == s_SpawnGeneration && IsActive(instance))
            {
                float now = Time.realtimeSinceStartup;
                float t = Mathf.Clamp01((now - startedAt) / SpawnFadeSeconds);
                float proximityAlpha = instance.ForceAttack && instance.ClosestApproachDistance < FadeStartDistance
                    ? Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((instance.ClosestApproachDistance - DespawnDistance) / (FadeStartDistance - DespawnDistance)))
                    : 1f;
                instance.CurrentAlpha = targetAlpha * Mathf.SmoothStep(0f, 1f, t) * proximityAlpha;
                if (now >= nextRefreshAt)
                {
                    if (GhostVisuals.RefreshMaterials(instance.GameObject, materials)) GhostVisuals.Apply(instance.GameObject, instance.CurrentAlpha);
                    nextRefreshAt = now + 0.10f;
                }
                GhostVisuals.SetAlpha(materials, instance.CurrentAlpha);
                if (t >= 1f) break;
                yield return null;
            }
            if (generation != s_SpawnGeneration || !IsActive(instance)) yield break;
            instance.CurrentAlpha = targetAlpha * (instance.ForceAttack && instance.ClosestApproachDistance < FadeStartDistance
                ? Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((instance.ClosestApproachDistance - DespawnDistance) / (FadeStartDistance - DespawnDistance)))
                : 1f);
            GhostVisuals.SetAlpha(materials, instance.CurrentAlpha);
            instance.Spawning = false;
        }

        internal static bool AllowEnterFlee(BaseAi ai)
        {
            if (ai == null || !s_ActiveGhosts.TryGetValue(ai.GetInstanceID(), out GhostChargeInstance instance)) return true;
            if (instance.Despawning) return false;
            if (instance.Behavior == GhostBehavior.Attack) return false;
            if (instance.Behavior == GhostBehavior.Undecided)
            {
                instance.Behavior = GhostBehavior.Flee;
                Core.Log($"{instance.Definition.DisplayName} locked in flee mode.");
            }
            return true;
        }

        internal static bool AllowEnterAttack(BaseAi ai)
        {
            if (ai == null || !s_ActiveGhosts.TryGetValue(ai.GetInstanceID(), out GhostChargeInstance instance)) return true;
            if (instance.Despawning || instance.Definition.Passive || instance.Behavior == GhostBehavior.Flee) return false;
            if (instance.Behavior == GhostBehavior.Undecided)
            {
                instance.Behavior = GhostBehavior.Attack;
                Core.Log($"{instance.Definition.DisplayName} locked in attack mode.");
            }
            MarkAttackStarted(instance);
            return true;
        }

        private static void BeginFlee(GhostChargeInstance instance)
        {
            if (!IsActive(instance) || instance.Behavior == GhostBehavior.Attack) return;
            instance.Behavior = GhostBehavior.Flee;
            Transform player = GameManager.GetPlayerTransform();
            PlayerManager manager = GameManager.GetPlayerManagerComponent();
            if (manager != null && manager.m_AiTarget != null) instance.Ai.m_CurrentTarget = manager.m_AiTarget;
            try
            {
                if (player != null)
                {
                    instance.Ai.SetFleeReason(BaseAi.AiFleeReason.ThrownStone);
                    instance.Ai.FleeFrom(player);
                }
                instance.Ai.EnterFlee();
            }
            catch (Exception ex)
            {
                if (!instance.BehaviorFailureLogged) Core.Warn($"{instance.Definition.DisplayName}: flee request failed: {ex}");
                instance.BehaviorFailureLogged = true;
            }
            instance.LastBehaviorRequestAt = Time.realtimeSinceStartup;
        }

        private static void EnforceBehavior(GhostChargeInstance instance, float now)
        {
            if (now < instance.NextBehaviorCheckAt || instance.Behavior == GhostBehavior.Undecided) return;
            instance.NextBehaviorCheckAt = now + 0.35f;
            AiMode mode = instance.Ai.GetAiMode();
            if (instance.Behavior == GhostBehavior.Flee)
            {
                if (mode == AiMode.Flee && now - instance.LastBehaviorRequestAt < 3f) return;
                BeginFlee(instance);
                return;
            }
            if (mode == AiMode.Attack || mode == AiMode.PassingAttack)
            {
                MarkAttackStarted(instance);
                return;
            }
            if (now - instance.LastBehaviorRequestAt < 0.35f) return;
            instance.LastBehaviorRequestAt = now;
            try
            {
                PlayerManager manager = GameManager.GetPlayerManagerComponent();
                if (manager != null && manager.m_AiTarget != null) instance.Ai.m_CurrentTarget = manager.m_AiTarget;
                Transform player = GameManager.GetPlayerTransform();
                if (player != null && !instance.Ai.EnterAttackModeIfPossible(player.position, false)) instance.Ai.EnterAttack();
                if (instance.Ai.GetAiMode() == AiMode.Attack || instance.Ai.GetAiMode() == AiMode.PassingAttack) MarkAttackStarted(instance);
            }
            catch (Exception ex)
            {
                if (!instance.BehaviorFailureLogged) Core.Warn($"{instance.Definition.DisplayName}: attack request failed: {ex}");
                instance.BehaviorFailureLogged = true;
            }
        }

        internal static void Cleanup()
        {
            s_SpawnGeneration++;
            s_SpawnQueue.Clear();
            s_ProcessingSpawnQueue = false;
            if (s_ActiveGhosts.Count == 0) return;
            List<GhostChargeInstance> ghosts = new List<GhostChargeInstance>(s_ActiveGhosts.Values);
            s_ActiveGhosts.Clear();
            foreach (GhostChargeInstance ghost in ghosts)
            {
                if (ghost.GameObject != null) UnityEngine.Object.Destroy(ghost.GameObject);
            }
        }

        private static void EnqueueSpawn(GhostAnimalDefinition definition, Vector3? position, bool isAmbush = false)
        {
            if (definition == null) return;
            s_SpawnQueue.Enqueue(new GhostSpawnRequest(definition, position, isAmbush));
            if (s_ProcessingSpawnQueue) return;
            s_ProcessingSpawnQueue = true;
            int generation = s_SpawnGeneration;
            MelonCoroutines.Start(ProcessSpawnQueue(generation));
        }

        private static IEnumerator ProcessSpawnQueue(int generation)
        {
            while (generation == s_SpawnGeneration && s_SpawnQueue.Count > 0)
            {
                GhostSpawnRequest request = s_SpawnQueue.Dequeue();
                if (request.IsAmbush && !GhostAmbushManager.IsOutdoors()) continue;
                HashSet<int> existing = CaptureExistingSpecies(request.Definition);

                bool spawnFailed = false;
                try
                {
                    SpawnVanilla(request.Definition.Species);
                }
                catch (Exception ex)
                {
                    Core.Error($"{request.Definition.DisplayName}: vanilla console spawn failed: {ex}");
                    spawnFailed = true;
                }

                if (spawnFailed)
                {
                    yield return null;
                    continue;
                }

                BaseAi spawned = null;
                for (int frame = 0; frame < SpawnDetectionFrames && generation == s_SpawnGeneration; frame++)
                {
                    spawned = FindNewSpawn(request.Definition, existing);
                    if (spawned != null) break;
                    yield return null;
                }

                if (generation != s_SpawnGeneration) yield break;

                if (spawned == null)
                {
                    Core.Error($"{request.Definition.DisplayName}: vanilla console spawn produced no detectable animal.");
                    yield return null;
                    continue;
                }

                if (spawned != null && spawned.gameObject != null) GhostVisuals.Apply(spawned.gameObject, 0f);
                if (request.IsAmbush && !GhostAmbushManager.IsOutdoors())
                {
                    if (spawned != null && spawned.gameObject != null) UnityEngine.Object.Destroy(spawned.gameObject);
                    yield return null;
                    continue;
                }
                yield return null;
                if (generation != s_SpawnGeneration || spawned == null || spawned.gameObject == null) continue;

                if (request.IsAmbush && !GhostAmbushManager.IsOutdoors())
                {
                    UnityEngine.Object.Destroy(spawned.gameObject);
                    continue;
                }
                ConfigureSpawnedGhost(spawned, request);
                yield return null;
            }

            if (generation == s_SpawnGeneration) s_ProcessingSpawnQueue = false;
        }

        private static void SpawnVanilla(GhostSpecies species)
        {
            switch (species)
            {
                case GhostSpecies.Bear:
                    ConsoleManager.CONSOLE_spawn_bear();
                    break;
                case GhostSpecies.Wolf:
                    ConsoleManager.CONSOLE_spawn_wolf();
                    break;
                case GhostSpecies.Moose:
                    ConsoleManager.CONSOLE_spawn_moose();
                    break;
                case GhostSpecies.Cougar:
                    ConsoleManager.CONSOLE_spawn_cougar();
                    break;
                case GhostSpecies.Timberwolf:
                    ConsoleManager.CONSOLE_spawn_wolf_grey();
                    break;
                case GhostSpecies.StarvingWolf:
                    ConsoleManager.CONSOLE_spawn_wolf_starving();
                    break;
                case GhostSpecies.Stag:
                    ConsoleManager.CONSOLE_spawn_stag();
                    break;
                case GhostSpecies.Doe:
                    ConsoleManager.CONSOLE_spawn_doe();
                    break;
                case GhostSpecies.Rabbit:
                    ConsoleManager.CONSOLE_spawn_rabbit();
                    break;
                case GhostSpecies.Ptarmigan:
                    ConsoleManager.CONSOLE_spawn_ptarmigan();
                    break;
            }
        }

        private static HashSet<int> CaptureExistingSpecies(GhostAnimalDefinition definition)
        {
            HashSet<int> ids = new HashSet<int>();
            foreach (BaseAi ai in Resources.FindObjectsOfTypeAll<BaseAi>())
            {
                if (ai == null || ai.gameObject == null || !ai.gameObject.scene.IsValid()) continue;
                if (!GhostAnimalDefinitions.Matches(ai, definition)) continue;
                ids.Add(ai.GetInstanceID());
            }
            return ids;
        }

        private static BaseAi FindNewSpawn(GhostAnimalDefinition definition, HashSet<int> existing)
        {
            Transform player = GameManager.GetPlayerTransform();
            BaseAi best = null;
            float bestDistance = float.MaxValue;

            foreach (BaseAi ai in Resources.FindObjectsOfTypeAll<BaseAi>())
            {
                if (ai == null || ai.gameObject == null || !ai.gameObject.scene.IsValid()) continue;
                int id = ai.GetInstanceID();
                if (existing.Contains(id) || IsGhostAnimal(ai)) continue;
                if (!GhostAnimalDefinitions.Matches(ai, definition)) continue;

                float distance = player != null ? (ai.transform.position - player.position).sqrMagnitude : 0f;
                if (best != null && distance >= bestDistance) continue;
                best = ai;
                bestDistance = distance;
            }

            return best;
        }

        private static void ConfigureSpawnedGhost(BaseAi ai, GhostSpawnRequest request)
        {
            if (ai == null || ai.gameObject == null) return;

            if (request.IsAmbush)
            {
                if (!request.Position.HasValue || !PlaceAtAmbush(ai, request.Position.Value, request.Definition))
                {
                    Core.Warn($"{request.Definition.DisplayName}: ambush cancelled (no safe warp).");
                    UnityEngine.Object.Destroy(ai.gameObject);
                    return;
                }
            }
            else if (request.Position.HasValue) PlaceAtCorpse(ai, request.Position.Value, request.Definition);

            ai.gameObject.name = request.Definition.ObjectName;
            GhostChargeInstance instance = new GhostChargeInstance(request.Definition, ai, request.IsAmbush);
            s_ActiveGhosts[ai.GetInstanceID()] = instance;
            GhostVisuals.Apply(ai.gameObject, SpiritsSettingsManager.GetSpawnAlpha(request.Definition.Species));
            instance.Materials.AddRange(GhostVisuals.GetMaterials(ai.gameObject));
            GhostVisuals.SetAlpha(instance.Materials, 0f);
            if (instance.ForceAttack) instance.Behavior = GhostBehavior.Attack;
            else if (instance.Definition.Passive) instance.Behavior = GhostBehavior.Flee;
            else if (instance.Definition.Species == GhostSpecies.Wolf || instance.Definition.Species == GhostSpecies.StarvingWolf || instance.Definition.Species == GhostSpecies.Timberwolf) instance.Behavior = UnityEngine.Random.value < 0.30f ? GhostBehavior.Flee : GhostBehavior.Attack;
            MelonCoroutines.Start(SpawnGhost(instance, s_SpawnGeneration));
            if (instance.Behavior == GhostBehavior.Flee) BeginFlee(instance);
            else MelonCoroutines.Start(BeginAttack(instance));
            MelonCoroutines.Start(Monitor(instance));

            string origin = request.IsAmbush ? $"outdoor ambush={request.Position.Value}" : request.Position.HasValue ? $"corpse={request.Position.Value}" : "vanilla command position";
            Core.Log($"{request.Definition.DisplayName} acquired from vanilla console spawn | {origin} | final={ai.transform.position}.");
        }

        private static bool PlaceAtAmbush(BaseAi ai, Vector3 position, GhostAnimalDefinition definition)
        {
            if (!GhostAmbushManager.IsOutdoors()) return false;
            Transform player = GameManager.GetPlayerTransform();
            if (player == null) return false;
            Vector3 towardPlayer = player.position - position;
            towardPlayer.y = 0f;
            float horizontalDistance = towardPlayer.magnitude;
            if (horizontalDistance < 20f || horizontalDistance > 37f) return false;
            MoveAgent agent = ai.GetMoveAgent();
            if (agent == null) return false;
            try
            {
                if (!agent.Warp(position, 0f, false, NavMesh.AllAreas)) return false;
                return true;
            }
            catch (Exception ex)
            {
                Core.Warn($"{definition.DisplayName}: ambush warp failed: {ex.Message}");
                return false;
            }
        }

        private static void PlaceAtCorpse(BaseAi ai, Vector3 corpsePosition, GhostAnimalDefinition definition)
        {
            bool warped = false;
            MoveAgent moveAgent = ai.GetMoveAgent();

            if (moveAgent != null)
            {
                try
                {
                    warped = moveAgent.Warp(corpsePosition, 0f, false, NavMesh.AllAreas);
                }
                catch
                {
                    warped = false;
                }
            }

            ai.transform.position = corpsePosition;

            Transform player = GameManager.GetPlayerTransform();
            if (player != null)
            {
                Vector3 toPlayer = player.position - ai.transform.position;
                toPlayer.y = 0f;
                if (toPlayer.sqrMagnitude > 0.001f) ai.transform.rotation = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
            }

            float delta = Vector3.Distance(ai.transform.position, corpsePosition);
            Core.Log($"{definition.DisplayName} positioned on corpse | MoveAgentWarp={(warped ? "yes" : "no")} | exact transform applied | delta={delta:0.000}m.");
        }

        private static IEnumerator BeginAttack(GhostChargeInstance instance)
        {
            if (!IsActive(instance) || instance.Definition.Passive) yield break;
            PlayerManager playerManager = GameManager.GetPlayerManagerComponent();
            if (playerManager == null || playerManager.m_AiTarget == null) yield break;
            instance.Ai.m_CurrentTarget = playerManager.m_AiTarget;
            for (int attempt = 0; attempt < 10; attempt++)
            {
                if (!IsActive(instance) || instance.Behavior == GhostBehavior.Flee) yield break;
                Transform player = GameManager.GetPlayerTransform();
                if (player == null) yield break;
                AiMode currentMode = instance.Ai.GetAiMode();
                if (currentMode == AiMode.Flee && instance.Behavior != GhostBehavior.Attack)
                {
                    BeginFlee(instance);
                    yield break;
                }
                if (currentMode == AiMode.Attack || currentMode == AiMode.PassingAttack)
                {
                    instance.Behavior = GhostBehavior.Attack;
                    MarkAttackStarted(instance);
                    yield break;
                }
                bool accepted = false;
                try { accepted = instance.Ai.EnterAttackModeIfPossible(player.position, false); }
                catch (Exception ex) { Core.Warn($"{instance.Definition.DisplayName}: initial attack failed: {ex}"); }
                if (instance.Behavior == GhostBehavior.Flee) yield break;
                if (accepted)
                {
                    instance.Behavior = GhostBehavior.Attack;
                    MarkAttackStarted(instance);
                    yield break;
                }
                yield return new WaitForSeconds(0.10f);
            }
            if (!IsActive(instance)) yield break;
            if (instance.ForceAttack)
            {
                try { instance.Ai.EnterAttack(); }
                catch (Exception ex) { Core.Warn($"{instance.Definition.DisplayName}: forced attack request failed: {ex.Message}"); }
                yield break;
            }
            if (instance.Behavior == GhostBehavior.Undecided) BeginFlee(instance);
        }

        private static void MarkAttackStarted(GhostChargeInstance instance)
        {
            if (instance == null || instance.AttackStartedAt >= 0f) return;
            instance.AttackStartedAt = Time.realtimeSinceStartup;
        }

        private static IEnumerator Monitor(GhostChargeInstance instance)
        {
            while (HasRegisteredInstance(instance))
            {
                if (instance.Ai == null || instance.GameObject == null)
                {
                    Unregister(instance);
                    yield break;
                }

                if (!instance.GameObject.activeInHierarchy)
                {
                    Unregister(instance);
                    Core.Warn($"{instance.Definition.DisplayName} was disabled externally; ghost tracking stopped.");
                    yield break;
                }

                if (instance.ForceAttack && !GhostAmbushManager.IsOutdoors())
                {
                    Despawn(instance.Ai, "player entered an interior");
                    yield break;
                }

                float now = Time.realtimeSinceStartup;
                if (!instance.Despawning) EnforceBehavior(instance, now);
                if (now - instance.StartedAt >= MaximumLifetimeSeconds)
                {
                    Despawn(instance.Ai, "20 second timeout");
                    yield break;
                }

                bool checkProximity = !instance.Despawning &&
                    (instance.ForceAttack ||
                     (!instance.Spawning && instance.Behavior == GhostBehavior.Attack &&
                      instance.AttackStartedAt >= 0f && now - instance.AttackStartedAt >= ProximityGraceSeconds));
                if (checkProximity)
                {
                    Transform player = GameManager.GetPlayerTransform();
                    if (player == null) yield break;
                    Vector3 ghostPosition = instance.Ai.transform.position;
                    Vector3 playerPosition = player.position;
                    ghostPosition.y = 0f;
                    playerPosition.y = 0f;
                    float distance = Vector3.Distance(ghostPosition, playerPosition);
                    float fadeStart = FadeStartDistance;
                    float despawnAt = DespawnDistance;
                    if (distance <= fadeStart)
                    {
                        instance.ClosestApproachDistance = Mathf.Min(instance.ClosestApproachDistance, distance);
                        float visibility = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((instance.ClosestApproachDistance - despawnAt) / (fadeStart - despawnAt)));
                        instance.CurrentAlpha = SpiritsSettingsManager.GetSpawnAlpha(instance.Definition.Species) * visibility;
                        GhostVisuals.SetAlpha(instance.Materials, instance.CurrentAlpha);
                    }
                    if (distance <= despawnAt)
                    {
                        instance.CurrentAlpha = 0f;
                        GhostVisuals.SetAlpha(instance.Materials, 0f);
                        Despawn(instance.Ai, "within 4.0m of player");
                        yield break;
                    }
                }

                yield return null;
            }
        }

        private static bool IsActive(GhostChargeInstance instance)
        {
            return instance != null && !instance.Despawning && instance.Ai != null && instance.GameObject != null && instance.GameObject.activeInHierarchy && HasRegisteredInstance(instance);
        }

        private static bool HasRegisteredInstance(GhostChargeInstance instance)
        {
            if (instance == null || instance.Ai == null) return false;
            return s_ActiveGhosts.TryGetValue(instance.Ai.GetInstanceID(), out GhostChargeInstance active) && ReferenceEquals(active, instance);
        }

        private static void Unregister(GhostChargeInstance instance)
        {
            if (instance == null || instance.Ai == null) return;
            int id = instance.Ai.GetInstanceID();
            if (!s_ActiveGhosts.TryGetValue(id, out GhostChargeInstance active) || !ReferenceEquals(active, instance)) return;
            s_ActiveGhosts.Remove(id);
        }
    }

    internal static class GhostVisuals
    {
        internal static List<Material> GetMaterials(GameObject target)
        {
            List<Material> materials = new List<Material>();
            RefreshMaterials(target, materials);
            return materials;
        }

        internal static bool RefreshMaterials(GameObject target, List<Material> materials)
        {
            if (target == null) return false;
            bool changed = false;
            foreach (Renderer renderer in target.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null) continue;
                foreach (Material material in renderer.materials)
                {
                    if (material == null || (!material.HasProperty("_Color") && !material.HasProperty("_BaseColor")) || materials.Contains(material)) continue;
                    materials.Add(material);
                    changed = true;
                }
            }
            return changed;
        }

        internal static void SetAlpha(List<Material> materials, float alpha)
        {
            foreach (Material material in materials)
            {
                if (material == null) continue;
                if (material.HasProperty("_Color"))
                {
                    Color color = material.GetColor("_Color");
                    color.a = alpha;
                    material.SetColor("_Color", color);
                }
                if (material.HasProperty("_BaseColor"))
                {
                    Color color = material.GetColor("_BaseColor");
                    color.a = alpha;
                    material.SetColor("_BaseColor", color);
                }
            }
        }

        internal static void Apply(GameObject target, float alpha)
        {
            if (target == null) return;
            Shader ghostShader = Shader.Find("Unlit/Transparent");
            if (ghostShader == null) ghostShader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
            if (ghostShader == null) ghostShader = Shader.Find("Standard");
            Color ghostColor = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));

            foreach (Renderer renderer in target.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null) continue;
                renderer.enabled = true;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;

                foreach (Material material in renderer.materials)
                {
                    if (material == null) continue;
                    if (ghostShader != null) material.shader = ghostShader;
                    if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", null);
                    if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", null);
                    if (material.HasProperty("_Color")) material.SetColor("_Color", ghostColor);
                    if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", ghostColor);
                    if (material.HasProperty("_Mode")) material.SetFloat("_Mode", 2f);
                    if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
                    if (material.HasProperty("_SrcBlend")) material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                    if (material.HasProperty("_DstBlend")) material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                    if (material.HasProperty("_ZWrite")) material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                }
            }
        }
    }

    internal sealed class MaterialVisualState
    {
        internal readonly Material Material;
        internal readonly Shader Shader;
        internal readonly Texture MainTexture;
        internal readonly Texture BaseMap;
        internal readonly Color SavedColor;
        internal readonly Color SavedBaseColor;
        internal readonly bool HasMainTexture;
        internal readonly bool HasBaseMap;
        internal readonly bool HasColor;
        internal readonly bool HasBaseColor;
        internal readonly float Mode;
        internal readonly float Surface;
        internal readonly float SrcBlend;
        internal readonly float DstBlend;
        internal readonly float ZWrite;
        internal readonly bool HasMode;
        internal readonly bool HasSurface;
        internal readonly bool HasSrcBlend;
        internal readonly bool HasDstBlend;
        internal readonly bool HasZWrite;
        internal readonly bool AlphaTest;
        internal readonly bool AlphaBlend;
        internal readonly bool AlphaPremultiply;
        internal readonly string RenderType;
        internal readonly int RenderQueue;

        internal MaterialVisualState(Material material)
        {
            Material = material;
            Shader = material.shader;
            HasMainTexture = material.HasProperty("_MainTex");
            HasBaseMap = material.HasProperty("_BaseMap");
            HasColor = material.HasProperty("_Color");
            HasBaseColor = material.HasProperty("_BaseColor");
            HasMode = material.HasProperty("_Mode");
            HasSurface = material.HasProperty("_Surface");
            HasSrcBlend = material.HasProperty("_SrcBlend");
            HasDstBlend = material.HasProperty("_DstBlend");
            HasZWrite = material.HasProperty("_ZWrite");
            MainTexture = HasMainTexture ? material.GetTexture("_MainTex") : null;
            BaseMap = HasBaseMap ? material.GetTexture("_BaseMap") : null;
            SavedColor = HasColor ? material.GetColor("_Color") : UnityEngine.Color.white;
            SavedBaseColor = HasBaseColor ? material.GetColor("_BaseColor") : UnityEngine.Color.white;
            Mode = HasMode ? material.GetFloat("_Mode") : 0f;
            Surface = HasSurface ? material.GetFloat("_Surface") : 0f;
            SrcBlend = HasSrcBlend ? material.GetFloat("_SrcBlend") : 0f;
            DstBlend = HasDstBlend ? material.GetFloat("_DstBlend") : 0f;
            ZWrite = HasZWrite ? material.GetFloat("_ZWrite") : 0f;
            AlphaTest = material.IsKeywordEnabled("_ALPHATEST_ON");
            AlphaBlend = material.IsKeywordEnabled("_ALPHABLEND_ON");
            AlphaPremultiply = material.IsKeywordEnabled("_ALPHAPREMULTIPLY_ON");
            RenderType = material.GetTag("RenderType", false, string.Empty);
            RenderQueue = material.renderQueue;
        }

        internal void Restore()
        {
            if (Material == null) return;
            Material.shader = Shader;
            if (HasMainTexture) Material.SetTexture("_MainTex", MainTexture);
            if (HasBaseMap) Material.SetTexture("_BaseMap", BaseMap);
            if (HasColor) Material.SetColor("_Color", SavedColor);
            if (HasBaseColor) Material.SetColor("_BaseColor", SavedBaseColor);
            if (HasMode) Material.SetFloat("_Mode", Mode);
            if (HasSurface) Material.SetFloat("_Surface", Surface);
            if (HasSrcBlend) Material.SetFloat("_SrcBlend", SrcBlend);
            if (HasDstBlend) Material.SetFloat("_DstBlend", DstBlend);
            if (HasZWrite) Material.SetFloat("_ZWrite", ZWrite);
            SetKeyword(Material, "_ALPHATEST_ON", AlphaTest);
            SetKeyword(Material, "_ALPHABLEND_ON", AlphaBlend);
            SetKeyword(Material, "_ALPHAPREMULTIPLY_ON", AlphaPremultiply);
            Material.SetOverrideTag("RenderType", RenderType);
            Material.renderQueue = RenderQueue;
        }

        private static void SetKeyword(Material material, string keyword, bool enabled)
        {
            if (enabled) material.EnableKeyword(keyword);
            else material.DisableKeyword(keyword);
        }
    }

    internal sealed class RendererVisualState
    {
        internal readonly Renderer Renderer;
        internal readonly bool Enabled;
        internal readonly ShadowCastingMode ShadowCastingMode;
        internal readonly bool ReceiveShadows;
        internal readonly List<MaterialVisualState> Materials = new List<MaterialVisualState>();

        internal RendererVisualState(Renderer renderer)
        {
            Renderer = renderer;
            Enabled = renderer.enabled;
            ShadowCastingMode = renderer.shadowCastingMode;
            ReceiveShadows = renderer.receiveShadows;
            foreach (Material material in renderer.materials)
            {
                if (material != null) Materials.Add(new MaterialVisualState(material));
            }
        }

        internal void Restore()
        {
            if (Renderer == null) return;
            Renderer.enabled = Enabled;
            Renderer.shadowCastingMode = ShadowCastingMode;
            Renderer.receiveShadows = ReceiveShadows;
            foreach (MaterialVisualState material in Materials) material.Restore();
        }
    }

    internal sealed class AnimalVisualState
    {
        internal readonly BaseAi Ai;
        internal readonly List<RendererVisualState> Renderers = new List<RendererVisualState>();

        internal AnimalVisualState(BaseAi ai)
        {
            Ai = ai;
            foreach (Renderer renderer in ai.gameObject.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer != null) Renderers.Add(new RendererVisualState(renderer));
            }
        }

        internal void Restore()
        {
            foreach (RendererVisualState renderer in Renderers) renderer.Restore();
        }
    }

    internal static class GhostAppearanceManager
    {
        private static readonly Dictionary<int, AnimalVisualState> s_OriginalVisuals = new Dictionary<int, AnimalVisualState>();

        internal static void OnAnimalStarted(BaseAi ai)
        {
            if (ai == null || GhostAnimalEvent.IsGhostAnimal(ai)) return;
            ApplySetting(ai);
        }

        internal static void RefreshCurrentAnimals()
        {
            PruneDestroyed();
            foreach (BaseAi ai in Resources.FindObjectsOfTypeAll<BaseAi>())
            {
                if (ai == null || ai.gameObject == null || !ai.gameObject.scene.IsValid() || !ai.gameObject.activeInHierarchy) continue;
                if (GhostAnimalEvent.IsGhostAnimal(ai)) continue;
                ApplySetting(ai);
            }
        }

        internal static void PruneDestroyed()
        {
            if (s_OriginalVisuals.Count == 0) return;
            List<int> remove = new List<int>();
            foreach (KeyValuePair<int, AnimalVisualState> pair in s_OriginalVisuals)
            {
                if (pair.Value == null || pair.Value.Ai == null || pair.Value.Ai.gameObject == null) remove.Add(pair.Key);
            }
            foreach (int id in remove) s_OriginalVisuals.Remove(id);
        }

        private static void ApplySetting(BaseAi ai)
        {
            int id = ai.GetInstanceID();
            bool shouldGhost = SpiritsSettingsManager.ShouldUseGhostAppearance(ai);

            if (shouldGhost)
            {
                if (!s_OriginalVisuals.ContainsKey(id)) s_OriginalVisuals[id] = new AnimalVisualState(ai);
                GhostVisuals.Apply(ai.gameObject, SpiritsSettingsManager.GetLivingAlpha(ai));
                return;
            }

            if (!s_OriginalVisuals.TryGetValue(id, out AnimalVisualState original)) return;
            original.Restore();
            s_OriginalVisuals.Remove(id);
        }
    }

    [HarmonyPatch(typeof(ConsoleManager), nameof(ConsoleManager.RegisterCommands))]
    internal static class ConsoleManager_RegisterCommandsPatch
    {
        private static void Postfix()
        {
            uConsole.RegisterCommand("spirits_ghostbear", new Action(GhostAnimalEvent.TriggerBear));
            uConsole.RegisterCommand("spirits_ghostwolf", new Action(GhostAnimalEvent.TriggerWolf));
            uConsole.RegisterCommand("spirits_ghostmoose", new Action(GhostAnimalEvent.TriggerMoose));
            uConsole.RegisterCommand("spirits_ghostcougar", new Action(GhostAnimalEvent.TriggerCougar));
            uConsole.RegisterCommand("spirits_ghosttimberwolf", new Action(GhostAnimalEvent.TriggerTimberwolf));
            uConsole.RegisterCommand("spirits_ghoststarvingwolf", new Action(GhostAnimalEvent.TriggerStarvingWolf));
            uConsole.RegisterCommand("spirits_ghoststag", new Action(GhostAnimalEvent.TriggerStag));
            uConsole.RegisterCommand("spirits_ghostdoe", new Action(GhostAnimalEvent.TriggerDoe));
            uConsole.RegisterCommand("spirits_ghostrabbit", new Action(GhostAnimalEvent.TriggerRabbit));
            uConsole.RegisterCommand("spirits_ghostptarmigan", new Action(GhostAnimalEvent.TriggerPtarmigan));
            uConsole.RegisterCommand("spirits_ambush", new Action(GhostAmbushManager.TriggerNow));
        }
    }

    [HarmonyPatch(typeof(BaseAi), nameof(BaseAi.Start))]
    internal static class BaseAi_StartPatch
    {
        private static void Postfix(BaseAi __instance)
        {
            GhostAppearanceManager.OnAnimalStarted(__instance);
        }
    }

    [HarmonyPatch(typeof(BaseAi), nameof(BaseAi.EnterFlee))]
    internal static class BaseAi_EnterFleePatch
    {
        private static bool Prefix(BaseAi __instance)
        {
            return GhostAnimalEvent.AllowEnterFlee(__instance);
        }
    }

    [HarmonyPatch(typeof(BaseAi), nameof(BaseAi.EnterAttack), new Type[] { })]
    internal static class BaseAi_EnterAttackPatch
    {
        private static bool Prefix(BaseAi __instance)
        {
            return GhostAnimalEvent.AllowEnterAttack(__instance);
        }
    }

    [HarmonyPatch(typeof(AiCougar), nameof(AiCougar.SetWillDoPassingAttack))]
    internal static class AiCougar_SetWillDoPassingAttackPatch
    {
        private static bool Prefix(AiCougar __instance)
        {
            return !GhostAnimalEvent.IsGhostAnimal(__instance);
        }
    }

    [HarmonyPatch(typeof(BaseAi), nameof(BaseAi.EnterStruggle))]
    internal static class BaseAi_EnterStrugglePatch
    {
        private static bool Prefix(BaseAi __instance)
        {
            return !GhostAnimalEvent.IsGhostAnimal(__instance);
        }
    }

    [HarmonyPatch(typeof(BaseAi), nameof(BaseAi.ApplyDamage), new Type[] { typeof(float), typeof(DamageSource), typeof(string) })]
    internal static class BaseAi_ApplyDamageShortPatch
    {
        private static bool Prefix(BaseAi __instance, DamageSource damageSource)
        {
            if (GhostAnimalEvent.Despawn(__instance, "ghost hit")) return false;
            if (damageSource != DamageSource.Player) return true;
            GhostDeathTracker.TrackPlayerDamage(__instance);
            return true;
        }
    }

    [HarmonyPatch(typeof(BaseAi), nameof(BaseAi.ApplyDamage), new Type[] { typeof(float), typeof(float), typeof(DamageSource), typeof(string) })]
    internal static class BaseAi_ApplyDamageFullPatch
    {
        private static bool Prefix(BaseAi __instance, DamageSource damageSource)
        {
            if (GhostAnimalEvent.Despawn(__instance, "ghost hit")) return false;
            if (damageSource != DamageSource.Player) return true;
            GhostDeathTracker.TrackPlayerDamage(__instance);
            return true;
        }
    }

    [HarmonyPatch(typeof(BaseAi), nameof(BaseAi.EnterDead))]
    internal static class BaseAi_EnterDeadPatch
    {
        private static void Prefix(BaseAi __instance, out GhostDeathInfo __state)
        {
            __state = GhostDeathTracker.CaptureDeath(__instance);
        }

        private static void Postfix(BaseAi __instance, GhostDeathInfo __state)
        {
            if (__state == null) return;
            Vector3 corpsePosition = __instance != null ? __instance.transform.position : __state.Position;
            GhostAnimalEvent.TriggerFromDeath(__state, corpsePosition);
        }
    }

    [HarmonyPatch(typeof(AiCougar), nameof(AiCougar.EnterDead))]
    internal static class AiCougar_EnterDeadPatch
    {
        private static void Prefix(AiCougar __instance, out GhostDeathInfo __state)
        {
            __state = GhostDeathTracker.CaptureDeath(__instance);
        }

        private static void Postfix(AiCougar __instance, GhostDeathInfo __state)
        {
            if (__state == null) return;
            Vector3 corpsePosition = __instance != null ? __instance.transform.position : __state.Position;
            GhostAnimalEvent.TriggerFromDeath(__state, corpsePosition);
        }
    }
}