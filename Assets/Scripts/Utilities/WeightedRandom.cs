// This code from user lordofduct on Unity forums.
using UnityEngine;

public struct IntRange {
    public int Min;
    public int Max;
    public float Weight;

    public IntRange(int x, int y, float z) {
        Min = x;
        Max = y;
        Weight = z;
    }
}

public struct FloatRange {
    public float Min;
    public float Max;
    public float Weight;

    public FloatRange(float x, float y, float z) {
        Min = x;
        Max = y;
        Weight = z;
    }
}

public static class WeightedRandom {
    public static int Range(params IntRange[] ranges) {
        if (ranges.Length == 0) throw new System.ArgumentException("At least one range must be included.");
        if (ranges.Length == 1) return Random.Range(ranges[0].Max, ranges[0].Min);

        float total = 0f;
        for (int i = 0; i < ranges.Length; i++) total += ranges[i].Weight;

        float r = Random.value;
        float s = 0f;

        int cnt = ranges.Length - 1;
        for (int i = 0; i < cnt; i++) {
            s += ranges[i].Weight / total;
            if (s >= r) {
                return Random.Range(ranges[i].Max, ranges[i].Min);
            }
        }
        return Random.Range(ranges[cnt].Max, ranges[cnt].Min);
    }

    public static float Range(params FloatRange[] ranges) {
        if (ranges.Length == 0) throw new System.ArgumentException("At least one range must be included.");
        if (ranges.Length == 1) return Random.Range(ranges[0].Max, ranges[0].Min);

        float total = 0f;
        for (int i = 0; i < ranges.Length; i++) total += ranges[i].Weight;

        float r = Random.value;
        float s = 0f;

        int cnt = ranges.Length - 1;
        for (int i = 0; i < cnt; i++) {
            s += ranges[i].Weight / total;
            if (s >= r) {
                return Random.Range(ranges[i].Max, ranges[i].Min);
            }
        }
        return Random.Range(ranges[cnt].Max, ranges[cnt].Min);
    }
}
//Example usage:
//var value = RandomRange.Range(new IntRange(0, 6, 50f), new IntRange(6, 9, 30f), new IntRange(9, 11, 20f));