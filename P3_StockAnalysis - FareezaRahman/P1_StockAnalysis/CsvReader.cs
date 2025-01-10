using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace P1_StockAnalysis
{
    public class CsvReader
    {
        /// <summary>
        /// Reads candlestick data from a CSV file and returns a list of SmartCandlestick objects.
        /// </summary>
        /// <param name="csvFilename">The file path of the CSV to read.</param>
        /// <returns>A list of SmartCandlestick objects.</returns>
        public List<SmartCandlestick> ReadCandleSticks(string csvFilename)
        {
            var smartCandlesticks = new List<SmartCandlestick>(); // List to store parsed candlestick data

            try
            {
                using (var reader = new StreamReader(csvFilename))
                {
                    // Skip the header line in the CSV file
                    reader.ReadLine();

                    string line;
                    int lineNumber = 0;

                    // Read each line of the CSV
                    while ((line = reader.ReadLine()) != null)
                    {
                        try
                        {
                            // Split the line into fields
                            var fields = line.Split(',');

                            if (fields.Length < 6)
                            {
                                Console.WriteLine($"Skipping line {lineNumber + 1}: Not enough fields.");
                                continue;
                            }

                            // Parse each field
                            string date = fields[0].Trim(); // Date remains as string
                            decimal open = decimal.Parse(fields[1].Trim(), CultureInfo.InvariantCulture);
                            decimal high = decimal.Parse(fields[2].Trim(), CultureInfo.InvariantCulture);
                            decimal low = decimal.Parse(fields[3].Trim(), CultureInfo.InvariantCulture);
                            decimal close = decimal.Parse(fields[4].Trim(), CultureInfo.InvariantCulture);
                            long volume = long.Parse(fields[5].Trim(), CultureInfo.InvariantCulture);

                            // Create a SmartCandlestick instance
                            var candlestick = new SmartCandlestick(new CandleStick(date, open, high, low, close, volume));

                            // Add to the list
                            smartCandlesticks.Add(candlestick);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing line {lineNumber + 1}: {ex.Message}");
                        }

                        lineNumber++;
                    }
                }

                // Ensure the candlesticks are sorted by date
                smartCandlesticks.Sort((x, y) => string.Compare(x.Date, y.Date, StringComparison.Ordinal));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
            }

            return smartCandlesticks;
        }
    }
}
