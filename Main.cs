using System;
using System.Drawing;
using System.Windows.Forms;
using GTA;
using GTA.UI;
using GTA.Math;
using GTA.Native;
using Control = GTA.Control;
using System.Diagnostics;

namespace CinematicDrive
{
    public class Main : Script
    {
        private bool firstTime = true;
        public const string ModName = "Cinematic Drive";
        private const string Developer = "Shifuguru";
        private const string Version = "1.1";

        private CinematicBars cinematicBars;
        private Stopwatch holdStopwatch = new Stopwatch();
        private int pressedCounter;
        private Vector3 currentWaypoint;
        private bool waypointActive;

        public Main()
        {
            cinematicBars = new CinematicBars();
            Global.CinematicBars = cinematicBars;
            Tick += OnTick;
            KeyDown += OnKeyDown;
            Aborted += OnAborted;
            Interval = 1;

            SettingsManager.LoadSettings(); // Load Settings from ini file.
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!SettingsManager.ModEnabled || Game.Player.Character == null)
                return;

            #region WAYPOINT: 
            bool objectiveWaypointActive = false;
            waypointActive = Game.IsWaypointActive || objectiveWaypointActive;
            Blip[] ActiveBlips = World.GetAllBlips();

            if (Game.IsWaypointActive)
            {
                currentWaypoint = World.WaypointPosition;
            }
            else
            {
                // Get Array of All Blips on Map
                foreach (var Blip in ActiveBlips)
                {
                    // Stops foreach once it finds an existing Blip with a GPS route 
                    if (Function.Call<bool>(Hash.DOES_BLIP_EXIST, Blip) && Function.Call<bool>(Hash.DOES_BLIP_HAVE_GPS_ROUTE, Blip))
                    {
                        objectiveWaypointActive = true;
                        currentWaypoint = Blip.Position;
                        break;
                    }
                    else
                    {
                        objectiveWaypointActive = false;
                    }
                }
            }

            if (waypointActive)
            {
                if (SettingsManager.DebugEnabled)
                {
                    new TextElement(string.Format("X: {0}\nY: {1}\nZ: {2}\n", currentWaypoint.X, currentWaypoint.Y, currentWaypoint.Z), new PointF(0f, 300f), 0.35f, Color.Beige).Draw();
                }
            }
            #endregion

            if (firstTime)
            {
                Notification.Show($"{ModName} {Version} by {Developer} Loaded");
                firstTime = false;
            }

            HandleCinematicMode();
            cinematicBars.Draw(); // Draw black bars if active.
        }

        private void HandleCinematicMode()
        {
            if (Global.ForceCinCam)
                Function.Call(Hash.SET_CINEMATIC_MODE_ACTIVE, true);
            if (Global.IsCinematicModeActive && Game.IsControlPressed(Control.NextCamera))
                Global.ForceCinCam = false;
            if (Game.IsControlJustPressed(Control.VehicleCinCam))
                ++pressedCounter;

            // VEHICLE CIN CAM BUTTON PRESSED:
            if (Game.IsControlPressed(Control.VehicleCinCam))
            {
                if (!Global.IsCinematicModeActive)
                    Global.CinematicDriveActive = false;

                if (!holdStopwatch.IsRunning)
                    holdStopwatch.Start();

                if (holdStopwatch.ElapsedMilliseconds > SettingsManager.HoldDurationMs && pressedCounter == 1)
                {
                    Global.CinematicDriveActive = true;

                    if (Game.Player.Character.CurrentVehicle.Exists() && Game.Player.Character.CurrentVehicle != null)
                    {
                        if (Game.Player.Character.CurrentVehicle.Type != VehicleType.Helicopter && Game.Player.Character.CurrentVehicle.Type != VehicleType.Plane)
                        {
                            if (waypointActive)
                            {
                                DriveToWaypoint();
                            }
                            else
                            {
                                Cruise();
                            }
                        }
                    }
                    else
                        Global.IsCinematicModeActive = !Global.IsCinematicModeActive;

                    holdStopwatch.Stop();
                    holdStopwatch.Reset();
                    pressedCounter = 0;
                }

                if (holdStopwatch.ElapsedMilliseconds > 500L && holdStopwatch.ElapsedMilliseconds < SettingsManager.HoldDurationMs && Global.SameHold && pressedCounter == 1)
                {
                    if (Global.IsCinematicModeActive)
                        cinematicBars.DecreaseY(2);
                    else
                        cinematicBars.IncreaseY(2);
                }
                Global.SameHold = true;
            }

            // VEHICLE CIN CAM BUTTON RELEASED:
            if (Game.IsControlJustReleased(Control.VehicleCinCam))
            {
                if (holdStopwatch.ElapsedMilliseconds < 1000L)
                {
                    if (Global.IsCinematicModeActive)
                    {
                        cinematicBars.Setup(1);
                    }
                    else
                    {
                        cinematicBars.DecreaseY(2);
                        // Function.Call(Hash.SET_CINEMATIC_MODE_ACTIVE, false);
                    }
                }
                holdStopwatch.Stop();
                holdStopwatch.Reset();
                Global.ForceCinCam = Global.IsCinematicModeActive;
                Global.SameHold = false;
                pressedCounter = 0;
            }

            // MENU TOGGLE:
            if (Game.IsControlJustReleased(Control.VehicleHandbrake) && Game.IsControlJustReleased(Control.VehicleHorn))
                LemonMenu.ToggleMenu();



            // HAS REACHED DESTINATION:
            if (Global.IsAutoDriving && (double)Game.Player.Character.Position.DistanceTo(currentWaypoint) < 30.0)
            {
                Global.IsAutoDriving = false;
                Global.IsCinematicModeActive = false;
            }

            if (Global.IsCinematicModeActive)
            {
                Function.Call(Hash.DISPLAY_RADAR, false);
                Global.AlreadyCleared = false;
            }
            else
            {
                if (!Global.AlreadyCleared)
                {
                    Game.Player.Character.Task.ClearAll();
                    Global.AlreadyCleared = true;
                }

                if (!Global.SameHold)
                {
                    cinematicBars.DecreaseY(2);
                    // Function.Call(Hash.SET_CINEMATIC_MODE_ACTIVE, false);
                }

                // RESET CINEMATIC MODE:
                if (Global.CinematicDriveActive)
                    Function.Call(Hash.SET_CINEMATIC_MODE_ACTIVE, false);

                // RESET RADAR:
                if (Function.Call<bool>(Hash.IS_RADAR_PREFERENCE_SWITCHED_ON, true))
                    Function.Call(Hash.DISPLAY_RADAR, true);
            }
        }


        private void StartCinematicMode()
        {
            // START:
            Global.IsCinematicModeActive = true;
            Function.Call(Hash.SET_CINEMATIC_MODE_ACTIVE, true);
            //cinematicBars.Show();
        }


        private void StopCinematicMode()
        {
            // STOP:
            Global.IsCinematicModeActive = false;
            Global.IsCruising = false;
            Global.IsAutoDriving = false;
            Function.Call(Hash.SET_CINEMATIC_MODE_ACTIVE, false);
            //cinematicBars.Hide();
            Game.Player.Character.Task.ClearAll();
        }


        private void GoToWaypoint()
        {
            if (!Global.IsCinematicModeActive && waypointActive)
            {
                if (SettingsManager.OnFootEnabled && Game.Player.Character.IsOnFoot && !Game.Player.Character.IsInVehicle())
                {
                    Game.Player.Character.Task.GoTo(currentWaypoint, 15000);
                }
            }
            else
                Game.Player.Character.Task.ClearAll();

            Global.IsCinematicModeActive = !Global.IsCinematicModeActive;
            Global.IsOnFoot = !Global.IsOnFoot;
        }
        private void WalkAround()
        {
            if (!Global.IsCinematicModeActive && !waypointActive)
            {
                if (SettingsManager.OnFootEnabled && Game.Player.Character.IsOnFoot && !Game.Player.Character.IsInVehicle())
                    Game.Player.Character.Task.WanderAround();
            }
            else
                Game.Player.Character.Task.ClearAll();

            Global.IsCinematicModeActive = !Global.IsCinematicModeActive;
            Global.IsOnFoot = !Global.IsOnFoot;
        }

        private void DriveToWaypoint()
        {
            if (Game.IsControlJustReleased(Control.VehicleExit))
            {
                if (SettingsManager.EndOnExitEnabled)
                {
                    StopCinematicMode();
                    return;
                }
            }
            if (!Global.IsCinematicModeActive && waypointActive)
            {
                if (Game.Player.Character.IsInVehicle() && Game.Player.Character.CurrentVehicle != null && Game.Player.Character.CurrentVehicle.Exists())
                {
                    Game.Player.Character.Task.DriveTo(Game.Player.Character.CurrentVehicle, currentWaypoint, 30f, SettingsManager.Speed, SettingsManager.DrivingStyle);
                }
            }
            else
            {
                Game.Player.Character.Task.ClearAll();
            }

            Global.IsCinematicModeActive = !Global.IsCinematicModeActive;
            Global.IsAutoDriving = !Global.IsAutoDriving;
        }

        private void Cruise()
        {
            if (Game.IsControlJustReleased(Control.VehicleExit))
            {
                if (SettingsManager.EndOnExitEnabled)
                {
                    StopCinematicMode();
                    return;
                }
            }
            if (!Global.IsCinematicModeActive && !waypointActive)
            {
                if (Game.Player.Character.IsInVehicle() && Game.Player.Character.CurrentVehicle != null && Game.Player.Character.CurrentVehicle.Exists())
                {
                    Game.Player.Character.Task.CruiseWithVehicle(Game.Player.Character.CurrentVehicle, SettingsManager.Speed, SettingsManager.DrivingStyle);
                }
                if (SettingsManager.OnFootEnabled && Game.Player.Character.IsOnFoot && !Game.Player.Character.IsInVehicle())
                {
                    Game.Player.Character.Task.WanderAround();
                }
            }
            else
            {
                Game.Player.Character.Task.ClearAll();
            }

            Global.IsCinematicModeActive = !Global.IsCinematicModeActive;
            Global.IsCruising = !Global.IsCruising;

        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == SettingsManager.MenuKey)
                LemonMenu.ToggleMenu();
        }

        private void OnAborted(object sender, EventArgs e)
        {
            // StopCinematicMode();
        }
    }
}
