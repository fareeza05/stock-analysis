# Stock Chart Wave Analysis

This Windows Forms Application in C# visualizes stock data using candlestick charts and calculates the "Beauty" of selected waves based on Fibonacci levels. Users can load stock data from CSV files, select valid waves, and analyze potential future price predictions.

## Features

### 1. Stock Data Visualization

Displays OHLC (Open, High, Low, Close) data in a candlestick chart.

Volume data is displayed as a column plot beneath the candlestick chart.

Supports daily, weekly, and monthly stock data.

### 2. Wave Selection

Users can select valid waves by clicking or dragging over two candlesticks.

Valid waves start with a peak or valley candlestick.

Invalid selections are notified, allowing users to reselect.

### 3. Fibonacci Levels Calculation

After selecting a wave, Fibonacci levels (0%, 23.6%, 38.2%, 50%, 61.8%, 76.4%, 100%) are calculated.

These levels are displayed inside a rectangle representing the wave on the chart.

### 4. Beauty Function

Calculates the "Beauty" of a wave by summing Fibonacci confirmations within each candlestick.

Displays the Beauty as a function of price on a separate line chart.

Allows users to adjust price and observe changes in Beauty.

## Installation

1. Clone the repository:

`git clone https://github.com/your-username/stock-chart-wave-analysis.git`

2. Open the solution file (.sln) in Visual Studio.

3. Build the solution to restore NuGet packages and compile the project.

4. Run the application.

## Usage

1. Load Stock Data:

- Click the "Load Stock Data" button.

- Select a CSV file containing stock data (Daily, Weekly, or Monthly).

2. Update Chart:

- Use the DateTimePickers to select the start and end dates.

- Click the "Update Chart" button to filter and display the data within the selected range.

3. Wave Selection:

- Select two candlesticks by clicking or dragging to define a wave.

- Ensure the first candlestick is a peak or valley for a valid wave.

4. Analyze Fibonacci Levels and Beauty:

- Once a valid wave is selected, Fibonacci levels are displayed.

- The Beauty function is calculated and plotted as a function of price.

- Adjust the price to see how the Beauty changes.

## Project Structure

`StockChartForm.cs`: Main form handling chart display and wave selection.

`Candlestick.cs`: Defines the Candlestick class for handling OHLC data.

`SmartCandlestick.cs`: Extends Candlestick with additional properties for wave analysis.

`FibonacciCalculator.cs`: Calculates Fibonacci levels for selected waves.

`BeautyCalculator.cs`: Computes the Beauty of a wave based on Fibonacci confirmations.

## Data Format

CSV files should follow this format:

```
Date,Open,High,Low,Close,Volume
yyyy-MM-dd,float,float,float,float,int
```
