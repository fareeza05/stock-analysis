using System;

namespace P1_StockAnalysis
{
    // A class to represent a single candlestick data entry in the stock chart.
    public class CandleStick
    {
        // The date of the candlestick in "yyyy-MM-dd" format, stored as a string.
        public string Date { get; private set; }

        // Opening price of the stock on this date, represented as a decimal for precision.
        public decimal Open { get; private set; }

        // Highest price reached by the stock during the trading day, represented as a decimal.
        public decimal High { get; private set; }

        // Lowest price reached by the stock during the trading day, represented as a decimal.
        public decimal Low { get; private set; }

        // Closing price of the stock on this date, represented as a decimal.
        public decimal Close { get; private set; }

        // The total volume of shares traded on this date, represented as a long integer.
        public long Volume { get; private set; }

        // Constructor to initialize a CandleStick object with the provided values.
        // Parameters:
        //   - date: The date of the candlestick (as a string).
        //   - open: The opening price of the stock on this date.
        //   - high: The highest price reached by the stock during the day.
        //   - low: The lowest price reached by the stock during the day.
        //   - close: The closing price of the stock on this date.
        //   - volume: The total volume of shares traded on this date.
        public CandleStick(string date, decimal open, decimal high, decimal low, decimal close, long volume)
        {
            Date = date;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            Volume = volume;
        }
    }
}
