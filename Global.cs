
namespace CinematicDrive
{
    public static class Global
    {
        public static SettingsManager Settings { get; set; }

        public static CinematicBars CinematicBars { get; set; }

        public static bool IsCinematicModeActive { get; set; }

        public static bool IsCruising { get; set; }

        public static bool IsOnFoot { get; set; }

        public static bool IsAutoDriving { get; set; }

        public static bool ForceCinCam { get; set; }

        public static bool CinematicDriveActive { get; set; }

        public static bool SameHold { get; set; }

        public static bool AlreadyCleared { get; set; }
    }
}
