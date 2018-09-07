namespace DaySim.Framework.Core {
  public enum Bias {
    None,
    Low,
    High
  }

  public interface ITimeWindow {

    void SetBusyMinutes(int inclusiveStart, int exclusiveEnd);

    int TotalAvailableMinutes(int inclusiveStart, int inclusiveEnd);

    bool EntireSpanIsAvailable(int inclusiveStart, int inclusiveEnd);

    int AdjacentAvailableMinutesBefore(int minute);

    IMinuteSpan AdjacentAvailableWindowBefore(int minute);

    int AdjacentAvailableMinutesAfter(int minute);

    IMinuteSpan AdjacentAvailableWindowAfter(int minute);

    IMinuteSpan LongestAvailableFeasibleWindow(int apEnd, int dpStart, double timeTo, double timeFrom, int mad);

    int TotalAvailableMinutesBefore(int minute);

    int TotalAvailableMinutesAfter(int minute);

    int MaxAvailableMinutesBefore(int minute);

    int MaxAvailableMinutesAfter(int minute);

    int GetAvailableMinute(IRandomUtility randomUtility, int inclusiveStart, int exclusiveEnd,
                                  Bias bias = Bias.None);

    IMinuteSpan GetMinuteSpan(IRandomUtility randomUtility, int p1Start, int p1End, int p2Start, int p2End);

    int AvailableWindow(int minute, int direction);

    bool IsBusy(int minute);

    ITimeWindow DeepCloneToANewWindow();

    ITimeWindow IncorporateAnotherTimeWindow(ITimeWindow otherTimeWindow);
  }
}
