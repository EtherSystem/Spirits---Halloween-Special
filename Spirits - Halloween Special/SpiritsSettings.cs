using System.Reflection;
using ModSettings;
using UnityEngine;

namespace Spirits___Halloween_Special
{
    internal sealed class SpiritsSettings : JsonModSettings
    {
        [Section("Ghost Events")]

        [Name("Enable ghost spawns")]
        [Description("When enabled, killing wildlife can summon its ghost. Predators attack or flee; deer, rabbits and ptarmigans always flee.")]
        public bool EnableGhostSpawns = true;

        [Name("Bears opacity")]
        [Description("Opacity of bears ghosts summoned after a kill (0 = invisible, 100 = opaque).")]
        [Slider(0f, 100f, 101, NumberFormat = "{0:0}")]
        public float SpawnBearOpacity = 10f;

        [Name("Wolves opacity")]
        [Description("Opacity of wolves ghosts summoned after a kill (0 = invisible, 100 = opaque).")]
        [Slider(0f, 100f, 101, NumberFormat = "{0:0}")]
        public float SpawnWolfOpacity = 10f;

        [Name("Timberwolves opacity")]
        [Description("Opacity of timberwolves ghosts summoned after a kill (0 = invisible, 100 = opaque).")]
        [Slider(0f, 100f, 101, NumberFormat = "{0:0}")]
        public float SpawnTimberwolfOpacity = 10f;

        [Name("Starving wolves opacity")]
        [Description("Opacity of starving wolves ghosts summoned after a kill (0 = invisible, 100 = opaque).")]
        [Slider(0f, 100f, 101, NumberFormat = "{0:0}")]
        public float SpawnStarvingWolfOpacity = 10f;

        [Name("Moose opacity")]
        [Description("Opacity of moose ghosts summoned after a kill (0 = invisible, 100 = opaque).")]
        [Slider(0f, 100f, 101, NumberFormat = "{0:0}")]
        public float SpawnMooseOpacity = 10f;

        [Name("Cougars opacity")]
        [Description("Opacity of cougars ghosts summoned after a kill (0 = invisible, 100 = opaque).")]
        [Slider(0f, 100f, 101, NumberFormat = "{0:0}")]
        public float SpawnCougarOpacity = 10f;

        [Name("Stags opacity")]
        [Description("Opacity of stags ghosts summoned after a kill (0 = invisible, 100 = opaque).")]
        [Slider(0f, 100f, 101, NumberFormat = "{0:0}")]
        public float SpawnStagOpacity = 10f;

        [Name("Does opacity")]
        [Description("Opacity of does ghosts summoned after a kill (0 = invisible, 100 = opaque).")]
        [Slider(0f, 100f, 101, NumberFormat = "{0:0}")]
        public float SpawnDoeOpacity = 10f;

        [Name("Rabbits opacity")]
        [Description("Opacity of rabbits ghosts summoned after a kill (0 = invisible, 100 = opaque).")]
        [Slider(0f, 100f, 101, NumberFormat = "{0:0}")]
        public float SpawnRabbitOpacity = 10f;

        [Name("Ptarmigans opacity")]
        [Description("Opacity of ptarmigans ghosts summoned after a kill (0 = invisible, 100 = opaque).")]
        [Slider(0f, 100f, 101, NumberFormat = "{0:0}")]
        public float SpawnPtarmiganOpacity = 10f;

        [Section("Random Predator Ambushes")]

        [Name("Enable random ghost ambushes")]
        [Description("Ghost predators sometimes appear behind you while outdoors. Independent of the kill-triggered ghost setting.")]
        public bool EnableRandomAmbushes = false;

        [Name("Minimum delay")]
        [Description("Minimum outdoor in-game time between ambush attempts: 30 to 60 minutes, in one-minute increments. Pauses indoors and while the game is paused.")]
        [Slider(30, 60, 31, NumberFormat = "{0:0}min")]
        public int AmbushMinInGameMinutes = 30;

        [Name("Maximum delay")]
        [Description("Maximum outdoor in-game time between ambush attempts: 1 to 24 hours, in one-hour increments.")]
        [Slider(1, 24, 24, NumberFormat = "{0:0}h")]
        public int AmbushMaxHours = 1;

        [Name("Ambush cougars")]
        [Description("Allow cougars to be selected for random ghost ambushes.")]
        public bool AmbushCougars = false;

        [Name("Ambush wolves")]
        [Description("Allow normal wolves to be selected for random ghost ambushes.")]
        public bool AmbushWolves = true;

        [Name("Ambush bears")]
        [Description("Allow bears to be selected for random ghost ambushes.")]
        public bool AmbushBears = false;

        [Name("Ambush moose")]
        [Description("Allow moose to be selected for random ghost ambushes.")]
        public bool AmbushMoose = false;

        [Section("Ghost Wildlife Appearance")]

        [Name("Ghost bears")]
        [Description("Give all normal bears the ghost appearance while keeping their normal behavior.")]
        public bool GhostBears = false;

        [Name("Bears opacity")]
        [Description("Opacity of living bears when ghost appearance is enabled (0 = invisible, 100 = opaque).")]
        [Slider(0f, 100f, 101, NumberFormat = "{0:0}")]
        public float LivingBearOpacity = 10f;

        [Name("Ghost wolves")]
        [Description("Give normal and starving wolves the ghost appearance while keeping their normal behavior.")]
        public bool GhostWolves = false;

        [Name("Wolves opacity")]
        [Description("Opacity of living wolves when ghost appearance is enabled (0 = invisible, 100 = opaque).")]
        [Slider(0f, 100f, 101, NumberFormat = "{0:0}")]
        public float LivingWolfOpacity = 10f;

        [Name("Starving wolves opacity")]
        [Description("Opacity of living starving wolves when ghost appearance is enabled (0 = invisible, 100 = opaque).")]
        [Slider(0f, 100f, 101, NumberFormat = "{0:0}")]
        public float LivingStarvingWolfOpacity = 10f;

        [Name("Ghost timberwolves")]
        [Description("Give timberwolves the ghost appearance while keeping their normal behavior.")]
        public bool GhostTimberwolves = false;

        [Name("Timberwolves opacity")]
        [Description("Opacity of living timberwolves when ghost appearance is enabled (0 = invisible, 100 = opaque).")]
        [Slider(0f, 100f, 101, NumberFormat = "{0:0}")]
        public float LivingTimberwolfOpacity = 10f;

        [Name("Ghost moose")]
        [Description("Give all moose the ghost appearance while keeping their normal behavior.")]
        public bool GhostMoose = false;

        [Name("Moose opacity")]
        [Description("Opacity of living moose when ghost appearance is enabled (0 = invisible, 100 = opaque).")]
        [Slider(0f, 100f, 101, NumberFormat = "{0:0}")]
        public float LivingMooseOpacity = 10f;

        [Name("Ghost cougars")]
        [Description("Give all cougars the ghost appearance while keeping their normal behavior.")]
        public bool GhostCougars = false;

        [Name("Cougars opacity")]
        [Description("Opacity of living cougars when ghost appearance is enabled (0 = invisible, 100 = opaque).")]
        [Slider(0f, 100f, 101, NumberFormat = "{0:0}")]
        public float LivingCougarOpacity = 10f;

        [Name("Ghost deer")]
        [Description("Give stags and does the ghost appearance while keeping their normal behavior.")]
        public bool GhostDeer = false;

        [Name("Stags opacity")]
        [Description("Opacity of living stags when ghost appearance is enabled (0 = invisible, 100 = opaque).")]
        [Slider(0f, 100f, 101, NumberFormat = "{0:0}")]
        public float LivingStagOpacity = 10f;

        [Name("Does opacity")]
        [Description("Opacity of living does when ghost appearance is enabled (0 = invisible, 100 = opaque).")]
        [Slider(0f, 100f, 101, NumberFormat = "{0:0}")]
        public float LivingDoeOpacity = 10f;

        [Name("Ghost rabbits")]
        [Description("Give rabbits the ghost appearance while keeping their normal behavior.")]
        public bool GhostRabbits = false;

        [Name("Rabbits opacity")]
        [Description("Opacity of living rabbits when ghost appearance is enabled (0 = invisible, 100 = opaque).")]
        [Slider(0f, 100f, 101, NumberFormat = "{0:0}")]
        public float LivingRabbitOpacity = 10f;

        [Name("Ghost ptarmigans")]
        [Description("Give ptarmigans the ghost appearance while keeping their normal behavior.")]
        public bool GhostPtarmigans = false;

        [Name("Ptarmigans opacity")]
        [Description("Opacity of living ptarmigans when ghost appearance is enabled (0 = invisible, 100 = opaque).")]
        [Slider(0f, 100f, 101, NumberFormat = "{0:0}")]
        public float LivingPtarmiganOpacity = 10f;

        [Section("Advanced")]

        [Name("ML Logging")]
        [Description("Add logs for debugging in the ML console.")]
        public bool IsLogging = false;

        internal void RefreshOpacityVisibility()
        {
            SetFieldVisible(nameof(SpawnBearOpacity), EnableGhostSpawns);
            SetFieldVisible(nameof(SpawnWolfOpacity), EnableGhostSpawns);
            SetFieldVisible(nameof(SpawnTimberwolfOpacity), EnableGhostSpawns);
            SetFieldVisible(nameof(SpawnStarvingWolfOpacity), EnableGhostSpawns);
            SetFieldVisible(nameof(SpawnMooseOpacity), EnableGhostSpawns);
            SetFieldVisible(nameof(SpawnCougarOpacity), EnableGhostSpawns);
            SetFieldVisible(nameof(SpawnStagOpacity), EnableGhostSpawns);
            SetFieldVisible(nameof(SpawnDoeOpacity), EnableGhostSpawns);
            SetFieldVisible(nameof(SpawnRabbitOpacity), EnableGhostSpawns);
            SetFieldVisible(nameof(SpawnPtarmiganOpacity), EnableGhostSpawns);
            SetFieldVisible(nameof(AmbushMinInGameMinutes), EnableRandomAmbushes);
            SetFieldVisible(nameof(AmbushMaxHours), EnableRandomAmbushes);
            SetFieldVisible(nameof(AmbushCougars), EnableRandomAmbushes);
            SetFieldVisible(nameof(AmbushWolves), EnableRandomAmbushes);
            SetFieldVisible(nameof(AmbushBears), EnableRandomAmbushes);
            SetFieldVisible(nameof(AmbushMoose), EnableRandomAmbushes);
            SetFieldVisible(nameof(LivingBearOpacity), GhostBears);
            SetFieldVisible(nameof(LivingWolfOpacity), GhostWolves);
            SetFieldVisible(nameof(LivingStarvingWolfOpacity), GhostWolves);
            SetFieldVisible(nameof(LivingTimberwolfOpacity), GhostTimberwolves);
            SetFieldVisible(nameof(LivingMooseOpacity), GhostMoose);
            SetFieldVisible(nameof(LivingCougarOpacity), GhostCougars);
            SetFieldVisible(nameof(LivingStagOpacity), GhostDeer);
            SetFieldVisible(nameof(LivingDoeOpacity), GhostDeer);
            SetFieldVisible(nameof(LivingRabbitOpacity), GhostRabbits);
            SetFieldVisible(nameof(LivingPtarmiganOpacity), GhostPtarmigans);
        }

        protected override void OnChange(FieldInfo field, object oldValue, object newValue)
        {
            RefreshOpacityVisibility();
        }

        protected override void OnConfirm()
        {
            base.OnConfirm();
            RefreshOpacityVisibility();
            GhostAppearanceManager.RefreshCurrentAnimals();
            GhostAmbushManager.OnSettingsConfirmed();
        }
    }

    internal static class SpiritsSettingsManager
    {
        internal static SpiritsSettings Options { get; } = new SpiritsSettings();
        internal static bool EnableGhostSpawns => Options.EnableGhostSpawns;

        internal static void Initialize()
        {
            Options.RefreshOpacityVisibility();
            Options.AddToModSettings("Spirits - Halloween Special");
        }

        internal static float GetSpawnAlpha(GhostSpecies species)
        {
            switch (species)
            {
                case GhostSpecies.Bear: return Mathf.Clamp01(Options.SpawnBearOpacity / 100f);
                case GhostSpecies.Wolf: return Mathf.Clamp01(Options.SpawnWolfOpacity / 100f);
                case GhostSpecies.Moose: return Mathf.Clamp01(Options.SpawnMooseOpacity / 100f);
                case GhostSpecies.Cougar: return Mathf.Clamp01(Options.SpawnCougarOpacity / 100f);
                case GhostSpecies.Timberwolf: return Mathf.Clamp01(Options.SpawnTimberwolfOpacity / 100f);
                case GhostSpecies.StarvingWolf: return Mathf.Clamp01(Options.SpawnStarvingWolfOpacity / 100f);
                case GhostSpecies.Stag: return Mathf.Clamp01(Options.SpawnStagOpacity / 100f);
                case GhostSpecies.Doe: return Mathf.Clamp01(Options.SpawnDoeOpacity / 100f);
                case GhostSpecies.Rabbit: return Mathf.Clamp01(Options.SpawnRabbitOpacity / 100f);
                case GhostSpecies.Ptarmigan: return Mathf.Clamp01(Options.SpawnPtarmiganOpacity / 100f);
                default: return 0.30f;
            }
        }

        internal static float GetLivingAlpha(BaseAi ai)
        {
            if (ai == null) return 0.30f;
            if (ai.Ptarmigan != null) return Mathf.Clamp01(Options.LivingPtarmiganOpacity / 100f);
            if (ai.Rabbit != null) return Mathf.Clamp01(Options.LivingRabbitOpacity / 100f);
            if (ai.Doe != null) return Mathf.Clamp01(Options.LivingDoeOpacity / 100f);
            if (ai.Stag != null) return Mathf.Clamp01(Options.LivingStagOpacity / 100f);
            if (ai.Cougar != null) return Mathf.Clamp01(Options.LivingCougarOpacity / 100f);
            if (ai.Moose != null) return Mathf.Clamp01(Options.LivingMooseOpacity / 100f);
            if (ai.Bear != null) return Mathf.Clamp01(Options.LivingBearOpacity / 100f);
            if (ai.Timberwolf != null) return Mathf.Clamp01(Options.LivingTimberwolfOpacity / 100f);
            if (ai.StarvingWolf != null) return Mathf.Clamp01(Options.LivingStarvingWolfOpacity / 100f);
            if (ai.m_AiSubType == AiSubType.Wolf) return Mathf.Clamp01(Options.LivingWolfOpacity / 100f);
            return 0.30f;
        }

        internal static bool ShouldUseGhostAppearance(BaseAi ai)
        {
            if (ai == null) return false;
            if (ai.Ptarmigan != null) return Options.GhostPtarmigans;
            if (ai.Rabbit != null) return Options.GhostRabbits;
            if (ai.Doe != null || ai.Stag != null) return Options.GhostDeer;
            if (ai.Cougar != null) return Options.GhostCougars;
            if (ai.Moose != null) return Options.GhostMoose;
            if (ai.Bear != null) return Options.GhostBears;
            if (ai.Timberwolf != null) return Options.GhostTimberwolves;
            if (ai.NormalWolf != null || ai.StarvingWolf != null || ai.m_AiSubType == AiSubType.Wolf) return Options.GhostWolves;
            return false;
        }
    }
}