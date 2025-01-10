using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace P1_StockAnalysis
{
    public class StockChartForm : Form
    {
        private Chart chart_StockData;
        private ComboBox comboBox_patterns;
        private List<CandleStick> candleSticks;
        private List<SmartCandlestick> smartCandlesticks;
        private DataPoint firstSelectedPoint = null;
        private DataPoint secondSelectedPoint = null;

        // Constructor
        public StockChartForm(List<SmartCandlestick> candlestickData, DateTime startDate, DateTime endDate)
        {
            smartCandlesticks = candlestickData; // Assign smart candlesticks
            candleSticks = candlestickData.Cast<CandleStick>().ToList(); // Keep compatibility with raw candlesticks
            InitializeComponent();
            FilterAndUpdateChart(startDate, endDate);
            chart_StockData.MouseClick += Chart_MouseClick;

        }

        private void InitializeComponent()
        {
            // Create two separate chart areas
            var chartAreaPrice = new ChartArea("chartArea_StockPrice");
            var chartAreaBeauty = new ChartArea("chartArea_Beauty");

            // Create the price series (Candlestick)
            var seriesPrice = new Series("Candlestick")
            {
                ChartType = SeriesChartType.Candlestick,
                YValuesPerPoint = 4,
                ChartArea = chartAreaPrice.Name // Assign chart area to price series
            };


            // Initialize the chart
            chart_StockData = new Chart
            {
                Dock = DockStyle.Fill,
                ChartAreas = { chartAreaPrice},
                Series = { seriesPrice }

            };

            comboBox_patterns = new ComboBox
            {
                Dock = DockStyle.Top,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            Controls.Add(chart_StockData);
            Controls.Add(comboBox_patterns);

            Text = "Stock Chart Viewer";
            Width = 1000;
            Height = 600;

  
    }

        /// <summary>
        /// Binds candlestick data to the chart.
        /// </summary>
        private void BindCandlestickDataToChart(List<CandleStick> candlesticks)
        {
            var candlestickSeries = chart_StockData.Series["Candlestick"];
            candlestickSeries.Points.Clear();

            foreach (var candle in candlesticks)
            {
                int index = candlestickSeries.Points.AddXY(candle.Date, candle.High);
                var point = candlestickSeries.Points[index];
                point.YValues[1] = (double)candle.Low;
                point.YValues[2] = (double)candle.Open;
                point.YValues[3] = (double)candle.Close;

                // Set color based on bullish/bearish
                if (candle.Close > candle.Open)
                {
                    point.Color = Color.Green;
                    point.SetCustomProperty("PriceUpColor", "Green");
                    point.BorderColor = Color.Black;
                }
                else
                {
                    point.Color = Color.Red;
                    point.SetCustomProperty("PriceDownColor", "Red");
                    point.BorderColor = Color.Black;
                }

                
            }
        }

        /// <summary>
        /// Filters the chart data based on date and updates it.
        /// </summary>
        public void FilterAndUpdateChart(DateTime startDate, DateTime endDate)
        {
            var filteredData = candleSticks
                .Where(c =>
                {
                    if (DateTime.TryParseExact(c.Date.Trim().Replace("\"", ""),
                                               "yyyy-MM-dd",
                                               CultureInfo.InvariantCulture,
                                               DateTimeStyles.None,
                                               out DateTime parsedDate))
                    {
                        return parsedDate >= startDate && parsedDate <= endDate;
                    }
                    return false;
                })
                .ToList();

            BindCandlestickDataToChart(filteredData);
            NormalizeChart();
            AnnotatePeaksAndValleys();
        }

        /// <summary>
        /// Normalizes the Y-axis for the price and volume charts.
        /// </summary>
        private void NormalizeChart()
        {
            var candlestickSeries = chart_StockData.Series["Candlestick"];

            if (candlestickSeries.Points.Count == 0) return;

            double maxHigh = candlestickSeries.Points.Max(p => p.YValues[1]);
            double minLow = candlestickSeries.Points.Min(p => p.YValues[2]);

            var priceArea = chart_StockData.ChartAreas["chartArea_StockPrice"];

            priceArea.AxisY.Maximum = maxHigh * 1.02;
            priceArea.AxisY.Minimum = minLow * 0.98;
            priceArea.AxisY.LabelStyle.Format = "F2";

  
        }

        /// <summary>
        /// Mouse click handler for wave selection.
        /// </summary>
        private void Chart_MouseClick(object sender, MouseEventArgs e)
        {
            HitTestResult hitTest = chart_StockData.HitTest(e.X, e.Y);

            if (hitTest.ChartElementType == ChartElementType.DataPoint)
            {
                DataPoint clickedPoint = chart_StockData.Series["Candlestick"].Points[hitTest.PointIndex];

                if (firstSelectedPoint == null)
                {
                    if (IsPeak(hitTest.PointIndex) || IsValley(hitTest.PointIndex))
                    {
                        firstSelectedPoint = clickedPoint;
                        MessageBox.Show($"First point selected: {firstSelectedPoint.AxisLabel}");
                    }
                    else
                    {
                        MessageBox.Show("First candlestick must be a Peak or Valley.");
                    }
                }
                else if (secondSelectedPoint == null)
                {
                    secondSelectedPoint = clickedPoint;
                    ValidateWave();
                }
            }
        }

        /// <summary>
        /// Validates the wave selection.
        /// </summary>
        private void ValidateWave()
        {
            if (firstSelectedPoint == null || secondSelectedPoint == null) return;

            int firstIndex = chart_StockData.Series["Candlestick"].Points.IndexOf(firstSelectedPoint);
            int secondIndex = chart_StockData.Series["Candlestick"].Points.IndexOf(secondSelectedPoint);

            if (secondIndex <= firstIndex)
            {
                MessageBox.Show("Second point must come after the first.");
                ResetSelection();
                return;
            }

            double highBoundary = Math.Max(firstSelectedPoint.YValues[1], secondSelectedPoint.YValues[1]);
            double lowBoundary = Math.Min(firstSelectedPoint.YValues[2], secondSelectedPoint.YValues[2]);

            for (int i = firstIndex + 1; i < secondIndex; i++)
            {
                var point = chart_StockData.Series["Candlestick"].Points[i];
                if (point.YValues[1] > highBoundary || point.YValues[2] < lowBoundary)
                {
                    MessageBox.Show("Wave selection invalid: data points breach the boundary.");
                    ResetSelection();
                    return;
                }
            }

            MessageBox.Show($"Wave selected between {firstSelectedPoint.AxisLabel} and {secondSelectedPoint.AxisLabel}.");
            DrawFibonacciLevels();
        }

        private bool IsPeak(int index) =>
            index > 0 && index < candleSticks.Count - 1 &&
            candleSticks[index].High > candleSticks[index - 1].High &&
            candleSticks[index].High > candleSticks[index + 1].High;

        private bool IsValley(int index) =>
            index > 0 && index < candleSticks.Count - 1 &&
            candleSticks[index].Low < candleSticks[index - 1].Low &&
            candleSticks[index].Low < candleSticks[index + 1].Low;

        private void ResetSelection()
        {
            firstSelectedPoint = null;
            secondSelectedPoint = null;
        }

        private void AnnotatePeaksAndValleys()
        {
            // Clear existing annotations
            chart_StockData.Annotations.Clear();

            // Threshold for significant peaks/valleys
            double significantThreshold = 0.05; // 5% difference

            for (int i = 1; i < candleSticks.Count - 1; i++) // Skip the first and last candlesticks
            {
                var candle = candleSticks[i];

                // Check for peaks and valleys
                if (IsPeak(i) && IsSignificant(i, significantThreshold))
                {
                    AddAnnotation(i, (double)candle.High, "Peak", Color.Green, -20); // Position above the high
                }
                else if (IsValley(i) && IsSignificant(i, significantThreshold))
                {
                    AddAnnotation(i, (double)candle.Low, "Valley", Color.Red, 20); // Position below the low
                }
            }
        }

        // Consolidated AddAnnotation method
        private void AddAnnotation(int index, double yValue, string label, Color color, int yOffset)
        {
            // Ensure the index is valid
            if (index < 0 || index >= chart_StockData.Series["Candlestick"].Points.Count)
            {
                Console.WriteLine($"Index {index} is out of bounds, skipping annotation.");
                return; // Don't add annotation if the index is invalid
            }

            // Get the X value from the Candlestick series
            double xValue = chart_StockData.Series["Candlestick"].Points[index].XValue;

            // Create the annotation with a dynamic Y offset
            var annotation = new TextAnnotation
            {
                Text = label,
                ForeColor = color,
                Font = new Font("Arial", 9, FontStyle.Bold),
                AnchorX = xValue, // Correct X anchor based on the point's X value
                AnchorY = yValue + yOffset,  // Adjust Y position with the offset
                AnchorAlignment = ContentAlignment.MiddleCenter,
                IsSizeAlwaysRelative = false,
            };

            // Add annotation to chart
            chart_StockData.Annotations.Add(annotation);
        }

        // Method to add a text annotation at a specific index
        private void AddAnnotation(int index, double yValue, string text, Color color)
        {
            var annotation = new TextAnnotation
            {
                Text = text,
                ForeColor = color,
                Font = new Font("Arial", 8, FontStyle.Bold),
                AnchorX = index,
                AnchorY = yValue,
                AnchorAlignment = ContentAlignment.MiddleCenter,
                X = 0, // Use anchor points instead of absolute X/Y
                Y = 0
            };

            // Attach the annotation to the chart
            chart_StockData.Annotations.Add(annotation);
        }

        /// <summary>
        /// Determines if a peak or valley is "significant" based on a threshold.
        /// </summary>
        /// <param name="index">The index of the current candlestick.</param>
        /// <param name="threshold">The significance threshold (e.g., 5%).</param>
        /// <returns>True if the peak or valley is significant, false otherwise.</returns>
        private bool IsSignificant(int index, double threshold)
        {
            double prevHigh = (double)candleSticks[index - 1].High;
            double currHigh = (double)candleSticks[index].High;
            double nextHigh = (double)candleSticks[index + 1].High;

            double prevLow = (double)candleSticks[index - 1].Low;
            double currLow = (double)candleSticks[index].Low;
            double nextLow = (double)candleSticks[index + 1].Low;

            // Check for significant peaks
            if (IsPeak(index))
            {
                double maxNeighborHigh = Math.Max(prevHigh, nextHigh);
                return currHigh > maxNeighborHigh * (1 + threshold);
            }

            // Check for significant valleys
            if (IsValley(index))
            {
                double minNeighborLow = Math.Min(prevLow, nextLow);
                return currLow < minNeighborLow * (1 - threshold);
            }

            return false;
        }

        private void DrawFibonacciLevels()
        {
            if (firstSelectedPoint == null || secondSelectedPoint == null)
                return;

            // Get the indexes of the two selected points
            int firstIndex = chart_StockData.Series["Candlestick"].Points.IndexOf(firstSelectedPoint);
            int secondIndex = chart_StockData.Series["Candlestick"].Points.IndexOf(secondSelectedPoint);

            // Ensure secondIndex is after firstIndex
            if (secondIndex <= firstIndex)
            {
                MessageBox.Show("The second point must come after the first.");
                return;
            }

            // Get the high and low Y-values from the two selected points (to define the range for Fibonacci)
            double highPrice = Math.Max(firstSelectedPoint.YValues[1], secondSelectedPoint.YValues[1]);
            double lowPrice = Math.Min(firstSelectedPoint.YValues[2], secondSelectedPoint.YValues[2]);

            // Calculate the difference in Y-values
            double diff = highPrice - lowPrice;

            // Fibonacci levels
            var fibLevels = new double[]
            {
        0.0, 0.236, 0.382, 0.5, 0.618, 0.764, 1.0
            };

            // Create a new chart area for the Fibonacci levels
            var fibChartArea = new ChartArea("FibonacciChartArea");
            chart_StockData.ChartAreas.Add(fibChartArea);

            // Create a new series for the relevant candlesticks
            var fibSeries = new Series("FibonacciCandlesticks")
            {
                ChartType = SeriesChartType.Candlestick,
                XValueType = ChartValueType.DateTime
            };

            // Add the relevant candlesticks to the new series
            for (int i = firstIndex; i <= secondIndex; i++)
            {
                fibSeries.Points.Add(chart_StockData.Series["Candlestick"].Points[i]);
            }

            // Add the new series to the new chart area
            fibSeries.ChartArea = "FibonacciChartArea";
            chart_StockData.Series.Add(fibSeries);

            // Set the Y-axis values to the Fibonacci levels
            fibChartArea.AxisY.CustomLabels.Clear();
            for (int i = 0; i < fibLevels.Length; i++)
            {
                double fibPrice = highPrice - diff * fibLevels[i];
                fibChartArea.AxisY.CustomLabels.Add(fibPrice - diff * 0.01, fibPrice + diff * 0.01, $"{fibLevels[i] * 100}%");
            }

            // Adjust the Y-axis minimum and maximum to fit the Fibonacci levels
            fibChartArea.AxisY.Minimum = lowPrice - diff * 0.1;
            fibChartArea.AxisY.Maximum = highPrice + diff * 0.1;

            // Add dotted line annotations for each Fibonacci level
            for (int i = 0; i < fibLevels.Length; i++)
            {
                double fibPrice = highPrice - diff * fibLevels[i];

                // Create a HorizontalLineAnnotation for each Fibonacci level
                var fibLine = new HorizontalLineAnnotation
                {
                    Y = fibPrice,  // Set the Y position of the line for the Fibonacci level
                    LineColor = Color.Blue, // Set the line color to Blue
                    LineWidth = 1,
                    LineDashStyle = ChartDashStyle.Dash,
                    IsSizeAlwaysRelative = false,
                    AxisX = chart_StockData.ChartAreas["FibonacciChartArea"].AxisX, // Align to the X axis of the new chart area
                    AxisY = chart_StockData.ChartAreas["FibonacciChartArea"].AxisY
                };

                // Add Fibonacci line to the new chart area
                chart_StockData.Annotations.Add(fibLine);
            }

            // Calculate and plot Beauty vs. Price
            PlotBeautyVsPrice(firstIndex, secondIndex, fibLevels, highPrice, lowPrice, diff);
        }

        private void PlotBeautyVsPrice(int firstIndex, int secondIndex, double[] fibLevels, double highPrice, double lowPrice, double diff)
        {
            // Create a new chart area for the Beauty vs. Price plot
            var beautyChartArea = new ChartArea("BeautyChartArea");
            chart_StockData.ChartAreas.Add(beautyChartArea);

            // Create a new series for the Beauty values (column plot)
            var beautySeries = new Series("BeautySeries")
            {
                ChartType = SeriesChartType.Column,
                XValueType = ChartValueType.DateTime
            };

            // Calculate Beauty (closeness to nearest Fibonacci level) for each candlestick in the selected wave
            for (int i = firstIndex; i <= secondIndex; i++)
            {
                var candlestick = chart_StockData.Series["Candlestick"].Points[i];

                double closePrice = candlestick.YValues[3]; // Close price
                double closestFibLevel = fibLevels[0]; // Initialize with the first Fibonacci level
                double minDifference = Math.Abs(closePrice - (highPrice - diff * fibLevels[0])); // Start with the first level

                // Debugging: Print close price and initial difference
                Console.WriteLine($"Candlestick {i}: Close Price = {closePrice}, Initial Difference = {minDifference}");

                // Find the closest Fibonacci level to the candlestick's close price
                for (int j = 1; j < fibLevels.Length; j++)
                {
                    double fibPrice = highPrice - diff * fibLevels[j];
                    double difference = Math.Abs(closePrice - fibPrice);

                    // Debugging: Print difference for each Fibonacci level
                    Console.WriteLine($"Checking Fibonacci Level {fibLevels[j] * 100}%: Fib Price = {fibPrice}, Difference = {difference}");

                    if (difference < minDifference)
                    {
                        minDifference = difference;
                        closestFibLevel = fibLevels[j];
                    }
                }

                // Calculate closeness as a percentage (scaled between 0 and 100)
                double closenessPercentage = 100 - (minDifference / diff * 100);

                // Debugging: Print the closeness percentage
                Console.WriteLine($"Closeness Percentage = {closenessPercentage}");

                // Add the Beauty value (closeness) to the series
                beautySeries.Points.AddXY(DateTime.FromOADate(candlestick.XValue), closenessPercentage); // Convert XValue to DateTime for better display
            }

            // Debugging: Ensure beauty series points are being added
            Console.WriteLine($"Beauty Series Points Count: {beautySeries.Points.Count}");

            // Add the Beauty series to the new chart area
            beautySeries.ChartArea = "BeautyChartArea";
            chart_StockData.Series.Add(beautySeries);

            // Configure the Beauty chart area
            beautyChartArea.AxisX.Title = "Time (Candlestick)";
            beautyChartArea.AxisY.Title = "Beauty (%)";
            beautyChartArea.AxisX.Minimum = chart_StockData.Series["Candlestick"].Points[firstIndex].XValue;
            beautyChartArea.AxisX.Maximum = chart_StockData.Series["Candlestick"].Points[secondIndex].XValue;
            beautyChartArea.AxisY.Minimum = 0;
            beautyChartArea.AxisY.Maximum = 100;

            // Add color for better visibility
            beautySeries.Color = Color.CornflowerBlue;

            // Refresh the chart
            chart_StockData.Invalidate();

            // Debugging: Check if the chart is updated
            Console.WriteLine("Beauty vs Price Chart should now be populated.");
        }



        private void LogFibonacciMatches(int firstIndex, int secondIndex, double[] fibLevels, double highPrice, double lowPrice, double diff)
        {
            // Calculate Fibonacci levels based on high and low prices
            var fibPrices = fibLevels.Select(level => highPrice - diff * level).ToArray();

            // Loop through candlesticks in the range and check matches
            int countMatches = 0;
            double threshold = 0.015; // 1.5%

            for (int i = firstIndex; i <= secondIndex; i++)
            {
                var candlestick = candleSticks[i];

                double openPrice = (double)candlestick.Open;
                double closePrice = (double)candlestick.Close;

                foreach (var fibPrice in fibPrices)
                {
                    // Check if open or close price is within 1.5% threshold of the Fibonacci level
                    if (Math.Abs(openPrice - fibPrice) / fibPrice <= threshold || Math.Abs(closePrice - fibPrice) / fibPrice <= threshold)
                    {
                        countMatches++;
                    }
                }
            }

            // Log the Fibonacci levels
            Console.WriteLine("Fibonacci Levels:");
            foreach (var fibPrice in fibPrices)
            {
                Console.WriteLine($"{fibPrice:F2}");
            }

            Console.WriteLine($"Number of candlestick prices within 1.5% of Fibonacci levels: {countMatches}");
        }

    }
}
