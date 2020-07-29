namespace InGame.Settings
{
    public class SettingsModel
    {

        [Option("Graphics")]
        public SettingsGraphicsModel Graphics { get; set; } = new SettingsGraphicsModel();

        [Option("Sound")]
        public SettingsSoundModel Sound { get; set; } = new SettingsSoundModel();

        [Option("Gameplay")]
        public SettingsGameplayModel Gameplay { get; set; } = new SettingsGameplayModel();

        [Option("Menu")]
        public SettingsMenuModel Menu { get; set; } = new SettingsMenuModel();

        [Option("Dev Tools")]
        public SettingsDevModel Dev { get; set; } = new SettingsDevModel();
    }



    public class SettingsGraphicsModel
    {
        [Option("Glow quality")]
        public GlowQuality GlowQuality { get; set; } = GlowQuality.High;

        public bool IsGlowEnabled => GlowQuality != GlowQuality.Disabled;

        [Option("Glow power")]
        [Enabled("IsGlowEnabled")]
        public GlowPower GlowPower { get; set; } = GlowPower.Middle;

        [Option("Trackname text position")]
        public TracknameTextPosition TracknameTextPosition { get; set; } = TracknameTextPosition.Top;
    }


    public class SettingsSoundModel
    {
        [Option("Music in menu")]
        public bool MenuMusicEnabled { get; set; } = true;
        
        [Option("Menu music volume")]
        [Range(0, 100)]
        [Enabled("MenuMusicEnabled")]
        public int MenuMusicVolume { get; set; } = 30;



        [Option("Cube slice effect")]
        public bool SliceEffectEnabled { get; set; } = true;

        [Option("Slice effect volume")]
        [Range(0, 100)]
        [Enabled("SliceEffectEnabled")]
        public int SliceEffectVolume { get; set; } = 70;
    }


    public class SettingsGameplayModel
    {
        [Option("Finger pause", "Finger pause description")]
        public bool FingerPauseEnabled { get; set; } = true;

        [Option("Show SS,S,A grades while playing")]
        public bool ShowGrade { get; set; } = true;

        [Option("FOV")]
        [Range(45, 90)]
        public int FOV { get; set; } = 60;

        [Option("Camera angle")]
        [Range(-4, 20)]
        public int CameraAngle { get; set; } = 0;

        [Option("Camera height")]
        [Range(-2, 4)]
        public float CameraHeight { get; set; } = 2;

        [Option("Camera offset")]
        [Range(-4, 4)]
        public float CameraOffset { get; set; } = 0;


        [Option("Top cubes height")]
        [Range(3.3f, 5.8f)]
        public float SecondCubeHeight { get; set; } = 4.6f;

        [Option("Distance between cube roads")]
        [Range(2.5f, 4)]
        public float RoadsDistance { get; set; } = 2.5f;

        [Option("NEW! Are texts (score, %, SS) in game always on top?")]
        [Media("OverlayOn", "OverlayOff")]
        public bool TextMeshOverlay { get; set; } = true;

        [Option("Hide missed text")]
        public bool HideMissedText { get; set; } = false;
    }

    public class SettingsMenuModel
    {
        [Option("Two column group list")]
        public bool TwoColumnList { get; set; } = true;
    }

    public class SettingsDevModel
    {
        [Option("Console")]
        public bool ConsoleEnabled { get; set; } = false;

        [Option("Fps")]
        public bool ShowFpsEnabled { get; set; } = false;
    }






    public enum GlowQuality
    {
        High, Low, Disabled
    }
    public enum GlowPower
    {
        High, Middle, Low
    }
    public enum TracknameTextPosition
    {
        Top, Bottom, BottomReverse
    }
}