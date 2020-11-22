using System;
using UnityEngine;
[Serializable]
public class ChallengeConditions {
    public string challengeName;
    public int challengeIndex;
    public enum TicketLevel {Gold, Silver, Bronze, Glass};
    public TicketLevel challengeLevel;
    public bool specificBoard, specificCourse, minimumCoin, maximumTime;
    public int requiredCourse, requiredBoardID, requiredCoinCoint, timeLimitMin, timeLimitSec, timeLimitMil;
    public TimeSpan timeLimit;
}