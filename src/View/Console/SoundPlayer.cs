using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Media;

public static class SoundPlayer
{

    public static void PlayNotification()
    {
        /// @todo - hardcode
        System.Media.SoundPlayer player = new System.Media.SoundPlayer(Jarvis.JConstants.PATH_TO_DATA + @"Notification.wav");
        player.Play();
    }
}
