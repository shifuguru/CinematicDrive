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
        private Vector3 currentDestination;
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
                currentDestination = World.WaypointPosition;
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
                        currentDestination = Blip.Position;
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
                    new TextElement(string.Format("X: {0}\nY: {1}\nZ: {2}\n", currentDestination.X, currentDestination.Y, currentDestination.Z), new PointF(0f, 300f), 0.35f, Color.Beige).Draw();
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
            {
                Function.Call(Hash.SET_CINEMATIC_MODE_ACTIVE, true);
            }
            if (Global.IsActive && Game.IsControlPressed(Control.NextCamera))
            {
                Global.ForceCinCam = false;
            }

            // PRESSED COUNTER:
            if (Game.IsControlJustPressed(Control.VehicleCinCam))
            {
                ++pressedCounter;
            }


            if (Game.IsControlPressed(Control.VehicleCinCam))
            {
                if (!Global.IsActive)
                {
                    Global.ForceCinCam2 = false;
                }

                if (!holdStopwatch.IsRunning)
                {
                    holdStopwatch.Start(); // Begin Stopwatch
                }

                if (holdStopwatch.ElapsedMilliseconds > SettingsManager.HoldDurationMs && pressedCounter == 1)
                {
                    Global.ForceCinCam2 = true;

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
                    {
                        Global.IsActive = !Global.IsActive;
                    }

                    holdStopwatch.Stop();
                    holdStopwatch.Reset();
                    pressedCounter = 0;
                }

                if (holdStopwatch.ElapsedMilliseconds > 500L && holdStopwatch.ElapsedMilliseconds < SettingsManager.HoldDurationMs && Global.SameHold && pressedCounter == 1)
                {
                    if (Global.IsActive)
                    {
                        cinematicBars.DecreaseY(2);
                    }
                    else
                    {
                        cinematicBars.IncreaseY(2);
                    }
                }

                Global.SameHold = true;
            }

            if (Game.IsControlJustReleased(Control.VehicleCinCam))
            {
                if (holdStopwatch.ElapsedMilliseconds < 1000L)
                {
                    if (Global.IsActive)
                    {
                        cinematicBars.Setup(1);
                    }
                    else
                    {
                        cinematicBars.DecreaseY(2);
                        Function.Call(Hash.SET_CINEMATIC_MODE_ACTIVE, false);
                    }
                }

                holdStopwatch.Stop();
                holdStopwatch.Reset();
                Global.ForceCinCam = Global.IsActive;
                Global.SameHold = false;
                pressedCounter = 0;
            }

            if (Game.IsControlJustReleased(Control.VehicleHandbrake) && Game.IsControlJustReleased(Control.VehicleHorn))
            {
                LemonMenu.ToggleMenu();
            }

            if (Global.IsDriving && (double)Game.Player.Character.Position.DistanceTo(currentDestination) < 30.0)
            {
                Global.IsDriving = false;
                Global.IsActive = false;
            }

            if (Global.IsActive)
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

                if (Global.ForceCinCam2)
                {
                    Function.Call(Hash.SET_CINEMATIC_MODE_ACTIVE, false);
                }

                Function.Call(Hash.DISPLAY_RADAR, true);
            }
        }


        private void StartCinematicMode()
        {
            // START:
            Global.IsActive = true;
            Function.Call(Hash.SET_CINEMATIC_MODE_ACTIVE, true);
            //cinematicBars.Show();
        }


        private void StopCinematicMode()
        {
            // STOP:
            Global.IsActive = false;
            Global.IsCruising = false;
            Global.IsDriving = false;
            Function.Call(Hash.SET_CINEMATIC_MODE_ACTIVE, false);
            //cinematicBars.Hide();
            Game.Player.Character.Task.ClearAll();
        }


        private void GoToWaypoint()
        {
            if (!Global.IsActive && waypointActive)
            {
                if (SettingsManager.OnFootEnabled && Game.Player.Character.IsOnFoot && !Game.Player.Character.IsInVehicle())
                {
                    Game.Player.Character.Task.GoTo(currentDestination, 15000);
                }
            }
            else
                Game.Player.Character.Task.ClearAll();

            Global.IsActive = !Global.IsActive;
            Global.IsOnFoot = !Global.IsOnFoot;
        }
        private void WalkAround()
        {
            if (!Global.IsActive && !waypointActive)
            {
                if (SettingsManager.OnFootEnabled && Game.Player.Character.IsOnFoot && !Game.Player.Character.IsInVehicle())
                    Game.Player.Character.Task.WanderAround();
            }
            else
                Game.Player.Character.Task.ClearAll();

            Global.IsActive = !Global.IsActive;
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
            if (!Global.IsActive && waypointActive)
            {
                if (Game.Player.Character.IsInVehicle() && Game.Player.Character.CurrentVehicle != null && Game.Player.Character.CurrentVehicle.Exists())
                {
                    Game.Player.Character.Task.DriveTo(Game.Player.Character.CurrentVehicle, currentDestination, 30f, SettingsManager.Speed, SettingsManager.DrivingStyle);
                }
            }
            else
            {
                Game.Player.Character.Task.ClearAll();
            }

            Global.IsActive = !Global.IsActive;
            Global.IsDriving = !Global.IsDriving;
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
            if (!Global.IsActive && !waypointActive)
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

            Global.IsActive = !Global.IsActive;
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
