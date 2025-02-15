using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Rdr2CinematicCamera
{
  public class Main : Script
  {
    private bool firstTime = true;
    private const string ModName = "RDR2 Cinematic Camera";
    private const string Developer = "Hermes";
    private const string Version = "1.2";
    private bool _debug;
    private readonly Menu menu;
    private readonly CinematicBars cinematicBars;
    private readonly Stopwatch holdStopwatch = new Stopwatch();
    private int pressedCounter;
    private Vector3 currentDestination;

    public Main()
    {
      cinematicBars = new CinematicBars();
      Global.CinematicBars = cinematicBars;
      SettingsManager = new Config();
      menu = new Menu(SettingsManager);
      Tick += new EventHandler(OnTick);
      KeyDown += new KeyEventHandler(OnKeyDown);
      Interval = 1;
    }

    private void OnTick(object sender, EventArgs e)
    {
      if (_debug && Game.IsWaypointActive)
      {
        Vector3 waypointPosition = World.WaypointPosition;
        new TextElement(string.Format("X: {0}\nY: {1}\nZ: {2}\n", (object) waypointPosition.X, (object) waypointPosition.Y, (object) waypointPosition.Z), new PointF(0.0f, 300f), 0.35f, Color.Beige).Draw();
      }

      if (!SettingsManager.Enabled)
        return;

      menu.ProcessMenus();

      if (firstTime)
      {
        Notification.Show("RDR2 Cinematic Camera 1.2 by Hermes Loaded");
        firstTime = false;
      }

      HandleCinematicMode();

      if (!SettingsManager.CinematicBars || !(Game.Player.Character.CurrentVehicle != null))
        return;

      cinematicBars.Draw();
    }

    public static void HandleCinematicMode()
    {

      if (Global.ForceCinCam)
        Function.Call(Hash.SET_CINEMATIC_MODE_ACTIVE, true);
      if (Global.IsCinematicModeActive && Game.IsControlPressed(Control.NextCamera))
        Global.ForceCinCam = false;
      if (Game.IsControlJustPressed(Control.VehicleCinCam))
        ++pressedCounter;


      if (Game.IsControlPressed(Control.VehicleCinCam))
      {
        if (!Global.IsCinematicModeActive)
          Global.CinematicDriveActive = false;

        if (!holdStopwatch.IsRunning)
          holdStopwatch.Start();

        if (holdStopwatch.ElapsedMilliseconds > 1000L && pressedCounter == 1)
        {
          Global.CinematicDriveActive = true;
          if (Game.Player.Character.CurrentVehicle.Type != VehicleType.Helicopter && Game.Player.Character.CurrentVehicle.Type != VehicleType.Plane)
          {
            if (Game.IsWaypointActive)
              CinematicDriveToWaypoint();
            else
              Main.CinematicCruising();
          }
          else
            Global.IsCinematicModeActive = !Global.IsCinematicModeActive;

          holdStopwatch.Stop();
          holdStopwatch.Reset();
          pressedCounter = 0;
        }
        if (holdStopwatch.ElapsedMilliseconds < 1000L && Global.SameHold && pressedCounter == 1)
        {
          if (Global.IsCinematicModeActive)
            cinematicBars.DecreaseY(2);
          else
            cinematicBars.IncreaseY(2);
        }
        Global.SameHold = true;
      }

      if (Game.IsControlJustReleased(Control.VehicleCinCam))
      {
        if (holdStopwatch.ElapsedMilliseconds < 1000L)
        {
          if (Global.IsCinematicModeActive)
            cinematicBars.Setup(1);
          else
            cinematicBars.DecreaseY(2);
        }
        holdStopwatch.Stop();
        holdStopwatch.Reset();
        Global.ForceCinCam = Global.IsCinematicModeActive;
        Global.SameHold = false;
        pressedCounter = 0;
      }

      if (Game.IsControlJustReleased(Control.VehicleHandbrake) && Game.IsControlJustReleased(Control.VehicleDuck))
        menu.Toggle();




      if (Global.IsAutoDriving && (double) Game.Player.Character.Position.DistanceTo(currentDestination) < 30.0)
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
          cinematicBars.DecreaseY(2);

        if (Global.CinematicDriveActive)
          Function.Call(Hash.SET_CINEMATIC_MODE_ACTIVE, false);

        if (Function.Call<bool>(Hash.IS_RADAR_PREFERENCE_SWITCHED_ON, true))
          Function.Call(Hash.DISPLAY_RADAR, true);
      }
    }


    private static void CinematicCruising()
    {
      if (Game.Player.Character.CurrentVehicle == null)
        return;
      if (!Global.IsCinematicModeActive && !Game.IsWaypointActive)
        Game.Player.Character.Task.CruiseWithVehicle(Game.Player.Character.CurrentVehicle, SettingsManager.Speed, SettingsManager.DrivingStyle);
      else
        Game.Player.Character.Task.ClearAll();
      Global.IsCinematicModeActive = !Global.IsCinematicModeActive;
      Global.IsCruising = !Global.IsCruising;
    }

    public void CinematicDriveToWaypoint()
    {
      if (Game.Player.Character.CurrentVehicle == null)
        return;
      if (!Global.IsCinematicModeActive && Game.IsWaypointActive)
      {
        currentDestination = World.WaypointPosition;
        Game.Player.Character.Task.DriveTo(Game.Player.Character.CurrentVehicle, currentDestination, 25f, SettingsManager.Speed, SettingsManager.DrivingStyle);
      }
      else
        Game.Player.Character.Task.ClearAll();
      Global.IsCinematicModeActive = !Global.IsCinematicModeActive;
      Global.IsAutoDriving = !Global.IsAutoDriving;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode != Keys.F10)
        return;
      menu.Toggle();
    }
  }
}
