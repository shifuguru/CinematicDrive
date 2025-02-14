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
        Function.Call(Hash.SET_CINEMATIC_MODE_ACTIVE, (InputArgument) true);

      if (Global.IsActive && Game.IsControlPressed(Control.NextCamera))
        Global.ForceCinCam = false;

      if (Game.IsControlJustPressed(Control.VehicleCinCam))
        ++pressedCounter;

      if (Game.IsControlPressed(Control.VehicleCinCam))
      {
        if (!Global.IsActive)
          Global.ForceCinCam2 = false;

        if (!holdStopwatch.IsRunning)
          holdStopwatch.Start();

        if (holdStopwatch.ElapsedMilliseconds > 1000L && pressedCounter == 1)
        {
          Global.ForceCinCam2 = true;
          if (Game.Player.Character.CurrentVehicle.Type != VehicleType.Helicopter && Game.Player.Character.CurrentVehicle.Type != VehicleType.Plane)
          {
            if (Game.IsWaypointActive)
              CinematicDriveToWaypoint();
            else
              Main.CinematicCruising();
          }
          else
            Global.IsActive = !Global.IsActive;

          holdStopwatch.Stop();
          holdStopwatch.Reset();
          pressedCounter = 0;
        }
        if (holdStopwatch.ElapsedMilliseconds < 1000L && Global.SameHold && pressedCounter == 1)
        {
          if (Global.IsActive)
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
          if (Global.IsActive)
            cinematicBars.Setup(1);
          else
            cinematicBars.DecreaseY(2);
        }
        holdStopwatch.Stop();
        holdStopwatch.Reset();
        Global.ForceCinCam = Global.IsActive;
        Global.SameHold = false;
        pressedCounter = 0;
      }

      if (Game.IsControlJustReleased(Control.VehicleHandbrake) && Game.IsControlJustReleased(Control.VehicleDuck))
        menu.Toggle();

      if (Global.IsDriving && (double) Game.Player.Character.Position.DistanceTo(currentDestination) < 30.0)
      {
        Global.IsDriving = false;
        Global.IsActive = false;
      }

      if (Global.IsActive)
      {
        Function.Call(Hash.DISPLAY_RADAR, (InputArgument) false);
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
        if (Global.ForceCinCam2)
          Function.Call(Hash.SET_CINEMATIC_MODE_ACTIVE, (InputArgument) false);
        Function.Call(Hash.DISPLAY_RADAR, (InputArgument) true);
      }
    }


    private static void CinematicCruising()
    {
      if (Game.Player.Character.CurrentVehicle == null)
        return;
      if (!Global.IsActive && !Game.IsWaypointActive)
        Game.Player.Character.Task.CruiseWithVehicle(Game.Player.Character.CurrentVehicle, SettingsManager.Speed, SettingsManager.DrivingStyle);
      else
        Game.Player.Character.Task.ClearAll();
      Global.IsActive = !Global.IsActive;
      Global.IsCruising = !Global.IsCruising;
    }

    public void CinematicDriveToWaypoint()
    {
      if (Game.Player.Character.CurrentVehicle == null)
        return;
      if (!Global.IsActive && Game.IsWaypointActive)
      {
        currentDestination = World.WaypointPosition;
        Game.Player.Character.Task.DriveTo(Game.Player.Character.CurrentVehicle, currentDestination, 25f, SettingsManager.Speed, SettingsManager.DrivingStyle);
      }
      else
        Game.Player.Character.Task.ClearAll();
      Global.IsActive = !Global.IsActive;
      Global.IsDriving = !Global.IsDriving;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode != Keys.F10)
        return;
      menu.Toggle();
    }
  }
}
