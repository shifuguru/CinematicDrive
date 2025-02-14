
namespace CinematicDrive
{
    public static class Global
    {
        public static SettingsManager Settings { get; set; }

        public static CinematicBars CinematicBars { get; set; }

        public static bool IsActive { get; set; }

        public static bool IsCruising { get; set; }

        public static bool IsDriving { get; set; }

        public static bool ForceCinCam { get; set; }

        public static bool ForceCinCam2 { get; set; }

        public static bool SameHold { get; set; }

        public static bool AlreadyCleared { get; set; }
    }
}
