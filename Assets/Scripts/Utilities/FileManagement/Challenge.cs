using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable, CreateAssetMenu]
public class Challenge : ScriptableObject
{
    public string challengeName;
    public string challengeTrack;
    public Course challengeCourse;
    public int challengeIndex;
    public enum TicketLevel { Gold, Silver, Bronze, Glass };
    public TicketLevel challengeLevel;
    [FormerlySerializedAs("specificBoard")]
    public bool boardRule;
    public Board requiredBoard;
    [FormerlySerializedAs("minimumCoin")]
    public bool coinRule;
    [FormerlySerializedAs("requiredCoinCoint")]
    public int requiredCoinCount;
    [FormerlySerializedAs("maximumTime")]
    public bool timeRule;
    public int timeLimitInSeconds;
    public TimeSpan timeLimit;
}