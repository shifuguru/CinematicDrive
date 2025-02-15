// Decompiled with JetBrains decompiler
// Type: Rdr2CinematicCamera.Menu
// Assembly: Rdr2CinematicCamera, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45454D1B-190E-4E08-B650-4D990AAB0CE8
// Assembly location: C:\Users\Admin\Documents\Visual Studio 2022\Templates\Rdr2-Cinematic-Camera-1.2.2\Rdr2CinematicCamera.dll

using GTA;
using NativeUI;
using System.Collections.Generic;

#nullable disable
namespace Rdr2CinematicCamera
{
  public class LemonMenu
  {
    private readonly NativeMenu menu;
    private readonly ObjectPool pool;

    public LemonMenu()
    {
      pool = new ObjectPool();
      menu = new NativeMenu("Cinematic Camera", "Like in Red Dead Redemption 2");
      pool.Add(menu);

      List<object> items = new List<object>()
      {
        "Avoid Traffic",
        "Avoid Traffic Extremely",
        "Ignore Lights",
        "Normal",
        "Rushed",
        "Sometimes Overtake Traffic"
      };

      menu.AddItem((NativeMenuItem) new NativeMenuCheckboxItem("Enabled: ", CinematicBars));
      NativeMenuCheckboxItem cinematicBarsCheckBox = new NativeMenuCheckboxItem("Cinematic bars: ", CinematicBars);
      menu.AddItem((NativeMenuItem) cinematicBarsCheckBox);
      
      NativeMenuListItem menuDrivingStyles = new NativeMenuListItem("Driving style: ", items, 0);
      menu.AddItem((NativeMenuItem) menuDrivingStyles);
      menuDrivingStyles.Index = GetIndexFromEnum(DrivingStyle);

      NativeMenuListItem NativeMenuListItem = new NativeMenuListItem("Land plane on: ", new List<object>()
      {
        "Los Santos Airport",
        "Trevor's Airport",
        "McKenzie Airport"
      }, 0);

      NativeMenuSliderItem menuSpeed = new NativeMenuSliderItem("Speed: ")
      {
        Maximum = 250,
        Value = Speed
      };

      menu.AddItem((NativeMenuItem) menuSpeed);
      NativeMenuItem saveButton = new NativeMenuItem("Save changes");
      menu.AddItem(saveButton);
      NativeMenuItem restartButton = new NativeMenuItem("Restart");
      menu.AddItem(restartButton);

      menu.OnItemSelect += (ItemSelectEvent) ((sender, item, index) =>
      {
        if (item == saveButton)
        {
          DrivingStyle = DrivingStyles[menuDrivingStyles.Index];
          Speed = menuSpeed.Value;
          CinematicBars = cinematicBarsCheckBox.Checked;
          Save();
          if (!Global.IsDriving)
            return;
          Game.Player.Character.Task.DriveTo(Game.Player.Character.CurrentVehicle, World.WaypointPosition, 25f, (float) Speed, DrivingStyle);
        }
        else
        {
          if (item != restartButton)
            return;
          menu.Reset();
        }
      });
    }

    private void Reset()
    {
      Global.IsActive = false;
      Global.IsCruising = false;
      Global.IsDriving = false;
      Global.ForceCinCam = false;
      Global.ForceCinCam2 = false;
      Global.SameHold = false;
      Global.AlreadyCleared = false;
      Global.CinematicBars.Setup(0);
    }

    public void ProcessMenus() => pool?.ProcessMenus();

    public void Toggle() => menu.Visible = !menu.Visible;

    private int GetIndexFromEnum(DrivingStyle drivingStyle)
    {
      for (int index = 0; index < DrivingStyles.Count; ++index)
      {
        if (DrivingStyles[index] == drivingStyle)
          return index;
      }
      return 3;
    }
  }
}
