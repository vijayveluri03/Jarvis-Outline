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
        // TODO - clean it up after its done. 
    }

    public static void PlayPomodoroWorkComplete()
    {
        /// @todo - hardcode
        System.Media.SoundPlayer player = new System.Media.SoundPlayer(Jarvis.JConstants.PATH_TO_DATA + @"Pomo_WorkComplete.wav");
        player.Play();
        // TODO - clean it up after its done. 
    }

    public static void PlayPomodoroRestComplete()
    {
        /// @todo - hardcode
        System.Media.SoundPlayer player = new System.Media.SoundPlayer(Jarvis.JConstants.PATH_TO_DATA + @"Pomo_RestComplete.wav");
        player.Play();
        // TODO - clean it up after its done. 
    }

    public static void PlayPomodoroRestStarted()
    {
        /// @todo - hardcode
        System.Media.SoundPlayer player = new System.Media.SoundPlayer(Jarvis.JConstants.PATH_TO_DATA + @"Pomo_start.wav");
        player.Play();
        // TODO - clean it up after its done. 

    }

    public static void PlayPomodoroWorkStarted()
    {
        /// @todo - hardcode
        System.Media.SoundPlayer player = new System.Media.SoundPlayer(Jarvis.JConstants.PATH_TO_DATA + @"Pomo_start3.wav");
        player.Play();
        // TODO - clean it up after its done. 
    }

}
