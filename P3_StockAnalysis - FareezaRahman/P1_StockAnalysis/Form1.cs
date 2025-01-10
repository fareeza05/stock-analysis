using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace P1_StockAnalysis
{
    public partial class Form1 : Form
    {
        private List<CandleStick> candleSticks = new List<CandleStick>(); // List of raw candlestick data
        private List<SmartCandlestick> smartCandlesticks = new List<SmartCandlestick>(); // List of smart candlesticks
        private List<StockChartForm> openStockForms = new List<StockChartForm>(); // List to track open StockChartForm instances

        // Constructor
        public Form1()
        {
            InitializeComponent();
            button_loadStockData.Click += Button_loadStockData_Click; // Attach event handler for Load Stock Data button
            button_update.Click += Button_update_Click; // Attach event handler for Update button
            dateTimePicker_startDate.Value = new DateTime(2024, 01, 01); // Set default start date
            dateTimePicker_endDate.Value = DateTime.Now; // Set default end date
        }

        /// <summary>
        /// Event handler for Load Stock Data button
        /// </summary>
        private void Button_loadStockData_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "CSV files (*.csv)|*.csv";
                openFileDialog.InitialDirectory = @"C:\Stock Data";
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (var filePath in openFileDialog.FileNames)
                    {
                        CsvReader csvReader = new CsvReader();

                        // Read the SmartCandlestick objects from the file
                        var smartCandlestickData = csvReader.ReadCandleSticks(filePath);

                        if (smartCandlestickData == null || !smartCandlestickData.Any())
                        {
                            MessageBox.Show($"No data found in file: {filePath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            continue;
                        }

                        // Update the main form's SmartCandlestick list
                        smartCandlesticks = smartCandlestickData;

                        // Convert SmartCandlesticks to CandleSticks if needed
                        candleSticks = smartCandlesticks.Cast<CandleStick>().ToList();

                        // Create and show the StockChartForm
                        DateTime startDate = dateTimePicker_startDate.Value;
                        DateTime endDate = dateTimePicker_endDate.Value;

                        StockChartForm stockForm = new StockChartForm(smartCandlestickData, startDate, endDate);
                        stockForm.Text = $"Stock Chart - {Path.GetFileNameWithoutExtension(filePath)}";
                        stockForm.Show();

                        // Track the form in the open forms list
                        openStockForms.Add(stockForm);
                    }
                }
            }
        }


        /// <summary>
        /// Converts raw candlestick data to smart candlesticks
        /// </summary>
        private void LoadSmartCandlesticks(List<CandleStick> candleSticks)
        {
            smartCandlesticks = candleSticks
                .Select(cs => new SmartCandlestick(cs)) // Convert each CandleStick to SmartCandlestick
                .ToList();
        }

        /// <summary>
        /// Event handler for Update button
        /// </summary>
        private void Button_update_Click(object sender, EventArgs e)
        {
            DateTime startDate = dateTimePicker_startDate.Value; // Get start date
            DateTime endDate = dateTimePicker_endDate.Value; // Get end date

            foreach (var stockForm in openStockForms)
            {
                stockForm.FilterAndUpdateChart(startDate, endDate); // Update all open stock forms
            }
        }

        /// <summary>
        /// Filters the candlesticks based on the given date range
        /// </summary>
        private List<CandleStick> GetFilteredCandleSticks(DateTime startDate, DateTime endDate)
        {
            return candleSticks // Return candlesticks matching the date range
                .Where(candle =>
                {
                    if (DateTime.TryParseExact(candle.Date.Trim().Replace("\"", ""),
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
        }
    }
}
