public static class StarCalculator
{
    public static (double Value, string Unit)? Calculate(star selectedStar, string operation)
    {
        if (selectedStar.dist is not double parsecs ||
            !double.IsFinite(parsecs) || parsecs < 0 || parsecs >= 100_000)
        {
            return null;
        }

        return operation switch
        {
            "light-years" => (parsecs * 3.26156, "light-years"),
            "apparent-magnitude" when parsecs > 0 &&
                selectedStar.absmag is double absoluteMagnitude =>
                (absoluteMagnitude + 5 * Math.Log10(parsecs / 10), "mag"),
            _ => null
        };
    }
}