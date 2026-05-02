using System;
using UnityEngine;

/// <summary>
/// Reusable countdown or elapsed timer. Drive it by calling Tick() in Update().
/// </summary>
public class Timer
{
    public float Duration { get; private set; }
    public float Elapsed { get; private set; }
    public float Remaining => Mathf.Max(0f, Duration - Elapsed);
    public bool IsFinished => Elapsed >= Duration;
    public bool IsRunning { get; private set; }

    public event Action OnCompleted;

    public Timer(float duration)
    {
        Duration = duration;
    }

    public void Start()
    {
        Elapsed = 0f;
        IsRunning = true;
    }

    public void Stop()
    {
        IsRunning = false;
    }

    public void Reset()
    {
        Elapsed = 0f;
        IsRunning = false;
    }

    /// <summary>Call this from MonoBehaviour.Update().</summary>
    public void Tick(float deltaTime)
    {
        if (!IsRunning || IsFinished) return;

        Elapsed += deltaTime;

        if (IsFinished)
        {
            IsRunning = false;
            OnCompleted?.Invoke();
        }
    }

    /// <summary>Normalised progress from 0 to 1.</summary>
    public float Progress() => Duration > 0f ? Mathf.Clamp01(Elapsed / Duration) : 1f;
}
