using iFruitAddon2;
using GTA;
using GTA.UI;
using Screen = GTA.UI.Screen;
using System;

namespace CinematicDrive
{
    public class iFruitAddon : Script
    {
        CustomiFruit _iFruit;

        public iFruitAddon()
        {
            LoadiFruitAddon();
            Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            _iFruit.Update();
        }

        #region LOAD IFRUITADDON2 
        private void LoadiFruitAddon()
        {
            // Custom phone creation
            _iFruit = new CustomiFruit();

            // Phone customization (optional)
            /*
            _iFruit.CenterButtonColor = System.Drawing.Color.Orange;
            _iFruit.LeftButtonColor = System.Drawing.Color.LimeGreen;
            _iFruit.RightButtonColor = System.Drawing.Color.Purple;
            _iFruit.CenterButtonIcon = SoftKeyIcon.Fire;
            _iFruit.LeftButtonIcon = SoftKeyIcon.Police;
            _iFruit.RightButtonIcon = SoftKeyIcon.Website;
            */

            // New contact (wait 3 seconds (3000ms) before picking up the phone)
            iFruitContact contact = new iFruitContact("Cinematic Drive");
            contact.Answered += ContactAnswered;   // Linking the Answered event with our function
            contact.DialTimeout = 3000;            // Delay before answering
            contact.Active = true;                 // true = the contact is available and will answer the phone
            contact.Icon = ContactIcon.Taxi;      // Contact's icon
            _iFruit.Contacts.Add(contact);         // Add the contact to the phone
        }

        private void ContactAnswered(iFruitContact contact)
        {
            // The contact has answered: 
            if (SettingsManager.DebugEnabled)
            {
                Notification.Show("Cinematic Drive Menu Opened");
            }

            if (!LemonMenu.menu.Visible)
            {
                LemonMenu.menu.Visible = true;
            }

            // We need to close the phone in a moment
            // We can close it as soon as the contact picks up by calling _iFruit.Close().
            // Here, we will close the phone in 5 seconds (5000ms). 
            _iFruit.Close();
        }
        #endregion

    }
}
