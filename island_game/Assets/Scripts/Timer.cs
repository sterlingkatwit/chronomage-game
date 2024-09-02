using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Timer
{
    private static float startTime;
    private static float endTime;
    private static bool timerActive = false;

    public static void StartTimer()
    {
        startTime = Time.time;
        timerActive = true;
    }

    public static void StopTimer()
    {
        endTime = Time.time - startTime;
        timerActive = false;
    }

    public static void ResumeTimer()
    {
        timerActive = true;
        startTime = Time.time - GetCurrentTime();
    }

    public static float GetEndTime(){
        return endTime;
    }

    public static float GetCurrentTime()
    {
        if (timerActive)
        {
            return Time.time - startTime;
        }
        else
        {
            return endTime - startTime;
        }
    }

    public static bool TimerStatus()
    {
        return timerActive;
    }
}
