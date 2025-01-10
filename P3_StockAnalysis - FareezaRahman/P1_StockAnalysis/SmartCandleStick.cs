using P1_StockAnalysis;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using System.Drawing;
using System;
using System.Collections.Generic;

public class SmartCandlestick : CandleStick
{
    public decimal Range => High - Low; // Range of the candlestick
    public decimal BodyRange => Math.Abs(Close - Open); // Range of the candlestick body
    public decimal TopPrice => Math.Max(Open, Close); // Top price (max of Open and Close)
    public decimal BottomPrice => Math.Min(Open, Close); // Bottom price (min of Open and Close)
    public decimal UpperTail => High - TopPrice; // Upper tail height
    public decimal LowerTail => BottomPrice - Low; // Lower tail height

    // Dictionary to store boolean values for patterns
    public Dictionary<string, bool> Booleans { get; set; }

    // Constructor to initialize a SmartCandlestick from a normal CandleStick
    public SmartCandlestick(CandleStick baseCandlestick) : base(
        baseCandlestick.Date,
        baseCandlestick.Open,
        baseCandlestick.High,
        baseCandlestick.Low,
        baseCandlestick.Close,
        baseCandlestick.Volume)
    {
        Booleans = new Dictionary<string, bool>();
    }

    // Example pattern detection methods
    public bool IsBullish => Close > Open; // Bullish candlestick
    public bool IsBearish => Close < Open; // Bearish candlestick
    public bool IsDoji => BodyRange < (High - Low) * 0.1m; // Doji candlestick
}