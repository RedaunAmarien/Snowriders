using System;

[Serializable]
public class ChallengeConditions {
    public string challengeName, challengeTrack;
    public int challengeIndex;
    public enum TicketLevel {Gold, Silver, Bronze, Glass};
    public TicketLevel challengeLevel;
    public bool specificBoard, minimumCoin, maximumTime;
    public int requiredBoardID, requiredCoinCoint, timeLimitInSeconds;
    public TimeSpan timeLimit;
}