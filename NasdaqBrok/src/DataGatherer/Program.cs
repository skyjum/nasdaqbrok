using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

/*
 * NASDAQ Updated their site, so you will have to modify the URLS:

NASDAQ

    https://old.nasdaq.com/screening/companies-by-name.aspx?letter=0&exchange=nasdaq&render=download

AMEX

    https://old.nasdaq.com/screening/companies-by-name.aspx?letter=0&exchange=amex&render=download

NYSE

    https://old.nasdaq.com/screening/companies-by-name.aspx?letter=0&exchange=nyse&render=download
*/

namespace DataGatherer
{
    class Program
    {
        static List<StockItem> ParseCSV(string txt_data)
        {
            return null;
        }

        static DateTime GetLastDataTime(List<StockItem> data)
        {
            DateTime last = new DateTime(1, 1, 1);
            int y2 = DateTime.Today.Year - 2;
            int m2 = DateTime.Today.Month - 1;
            if (m2 < 1) {
                y2--;
                m2 = 12;
            }
            DateTime last_2_years = new DateTime(y2, m2, DateTime.Today.Day);

            if (data == null) return last_2_years;

            for (int i = 0; i < data.Count; i++) {
                StockItem slot = data[i];
                DateTime slot_time = new DateTime(slot.year, slot.month, slot.day);
                last = slot_time;
            }

            if (last.Year == 1) return last_2_years;

            return last;
        }

        static string UrlNasdaqHistorical(string stock_symbol, DateTime starting_day)
        {
            string s1 = "https://www.nasdaq.com/api/v1/historical/";

            DateTime thisDay = DateTime.Today;
            string this_year = thisDay.Year.ToString();
            string this_month = (thisDay.Month < 10) ? String.Concat("0", thisDay.Month.ToString()) : thisDay.Month.ToString();
            string this_day = (thisDay.Day < 10) ? String.Concat("0", thisDay.Day.ToString()) : thisDay.Day.ToString();
            string this_date = String.Concat(this_year, "-", this_month, "-", this_day);

            int start_year = starting_day.Year;
            string start_month = (starting_day.Month < 10) ? String.Concat("0", starting_day.Month.ToString()) : starting_day.Month.ToString();
            string start_day = (starting_day.Day < 10) ? String.Concat("0", starting_day.Day.ToString()) : starting_day.Day.ToString();
            string start_date = String.Concat(start_year.ToString(), "-", start_month, "-", start_day);

            string url = String.Concat(s1, stock_symbol, "/stocks/", start_date, "/", this_date);

            return url;
        }

        static string GetHistoricalFromNasdaq(string url)
        {
            // "https://www.nasdaq.com/api/v1/historical/OCFCP/stocks/2020-01-21/2021-01-21"
            // https://query1.finance.yahoo.com/v7/finance/download/%5EIXIC?period1=1579727041&period2=1611349441&interval=1d&events=history&includeAdjustedClose=true
            // https://query1.finance.yahoo.com/v7/finance/download/OPTT?period1=1579971294&period2=1611593694&interval=1d&events=history&includeAdjustedClose=true

            using (WebClient web = new WebClient()) {
                try {
                    web.Headers[HttpRequestHeader.Host] = "www.nasdaq.com";
                    web.Headers[HttpRequestHeader.Accept] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,/;q=0.8";
                    web.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
                    web.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.181 Mobile Safari/537.36";
                    string reply = web.DownloadString(url);

                    return reply;
                }
                catch (WebException ugly_msg) {
                    Console.WriteLine(ugly_msg.Message);

                    return string.Empty;
                }
            }
        }

        static string GetScreenerFromNasdaq(string url)
        {
            // https://www.nasdaq.com/market-activity/stocks/screener?exchange=NASDAQ&letter=aapl&render=download
            // https://old.nasdaq.com/screening/companies-by-name.aspx?letter=0&exchange=nasdaq&render=download
            using (WebClient web = new WebClient()) {
                web.Headers[HttpRequestHeader.Host] = "nasdaq.com";
                web.Headers[HttpRequestHeader.Accept] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,/;q=0.8";
                web.Headers[HttpRequestHeader.AcceptEncoding] = "";
                web.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.181 Mobile Safari/537.36";
                string reply = web.DownloadString(url);

                return reply;
            }
        }

        static string GetLine(string txtstream, int start, out int last_index)
        {
            char[] c_array = new char[1024];
            int count = 0;

            for (last_index = start; last_index < txtstream.Length; last_index++) {
                char c = txtstream[last_index];
                if (c == '\n') {
                    if (count == 0) {
                        continue;
                    }
                    else {
                        break;
                    }
                }
                c_array[count] = c;
                count++;
            }

            string char2string = new string(c_array, 0, count);
            return char2string;
        }

        static bool ParseNasdaqLine(string line, ref StockItem item, IFormatProvider format)
        {
            string[] tokens = line.Split(',');
            if (tokens.Length < 6) return false;

            string[] date_tokens = tokens[0].Split('/');
            if (date_tokens.Length != 3) return false;

            item = new StockItem();

            int.TryParse(date_tokens[1], out item.day);
            int.TryParse(date_tokens[0], out item.month);
            int.TryParse(date_tokens[2], out item.year);
            item.key = item.year*10000 + item.month * 100 + item.day;

            string last = tokens[1].Replace("$", "");
            item.last = Convert.ToDouble(last, format);

            string volumen = tokens[2];
            item.volumen = Convert.ToDouble(volumen, format);

            string open = tokens[3].Replace("$", "");
            item.open = Convert.ToDouble(open, format);

            string high = tokens[4].Replace("$", "");
            item.high = Convert.ToDouble(high, format);

            string low = tokens[5].Replace("$", "");
            item.low = Convert.ToDouble(low, format);

            return true;
        }

        static List<StockItem> ParseNasdaq(string txtstream)
        {
            // Date, Close/Last, Volume, Open, High, Low\n01/25/2021, $5.84, 44045480, $6.11, $7.3, $5.16\n...

            List<StockItem> data = new List<StockItem>();
            int start = 0;
            int last = 0;
            bool has_line = true;
            System.Globalization.NumberFormatInfo format = new System.Globalization.NumberFormatInfo() { NumberDecimalSeparator = "." };

            while (true) {
                string line = GetLine(txtstream, start, out last);
                if (line.Length == 0) break;

                start = last;

                StockItem slot = new StockItem();
                bool has_value = ParseNasdaqLine(line, ref slot, format);
                if (has_value == true) {
                    data.Add(slot);
                }
            }

            return data;
            
        }

        static List<StockItem> UrlNasdaq(string symbol, DateTime last_slot_day)
        {
            // Create the url
            string url = UrlNasdaqHistorical(symbol, last_slot_day);

            // Call the NASDAQ server and retrieve the historical data
            string historical = GetHistoricalFromNasdaq(url);

            // Parse the text
            List<StockItem> data = ParseNasdaq(historical);

            return data;
        }

        static void UpdateHistoricalData(List<StockItem> list_data, StockItem new_data_item)
        {
            StockItem item;
            if (list_data.Count == 0) {
                list_data.Add(new_data_item);
                return;
            }

            item = list_data[list_data.Count - 1];
            if (new_data_item.key == item.key) {
                list_data[list_data.Count - 1] = new_data_item;
                return;
            }

            if (new_data_item.key > item.key) {
                list_data.Add(new_data_item);
                return;
            }

            item = list_data[0];
            if (new_data_item.key < item.key) {
                list_data.Insert(0, new_data_item);
                return;
            }

            // This can be improved with a bisection algorithm or using a dictionary
            for (int i = 0; i < list_data.Count; i++) {
                if (list_data[i].key == new_data_item.key) {
                    list_data[i] = new_data_item;
                    break;
                }
                else if (list_data[i].key > new_data_item.key) {
                    list_data.Insert(i, new_data_item);
                }
            }
        }

        static List<StockItem> AppendHistoricalData(List<StockItem> downloaded_data, List<StockItem> historical_data)
        {
            if (historical_data == null) {
                historical_data = new List<StockItem>();
            }

            for (int i = downloaded_data.Count - 1; i >= 0; i--) {
                StockItem item = downloaded_data[i];
                UpdateHistoricalData(historical_data, item);
            }

            return historical_data;
        }

        static void SaveStockDataCSV(List<StockItem> data, string filename)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename)) {
                string header = string.Concat
                    ("Fecha, ", "Ultimo, ", "Volumen, ", "Apertura, ", "Maximo, ", "Minimo");
                file.WriteLine(header);

                System.Globalization.NumberFormatInfo format = new System.Globalization.NumberFormatInfo() { NumberDecimalSeparator = "." };
                for (int i = data.Count-1; i>=0; i--){
                    StockItem item = data[i];
                    DateTime date = new DateTime(item.year, item.month, item.day);
                    string datetxt = date.ToString("yyyy-MM-dd");
                    string last = Convert.ToString(item.last, format);
                    string volumen = Convert.ToString(item.volumen, format);
                    string open = Convert.ToString(item.open, format);
                    string high = Convert.ToString(item.high, format);
                    string low = Convert.ToString(item.low, format);

                    string line = string.Concat
                        (datetxt, ", ", last, ", ", volumen, ", ", open, ", ", high, ", ", low);

                    file.WriteLine(line);
                }
            }
        }

        static void UpdateStockHistoricalData(string symbol, string path)
        {
            // Check the symbols is in the history folder and read all the current data
            List<StockItem> historical_data = null;
            string file_historical_data = string.Concat(path, symbol, ".csv");

            if (System.IO.File.Exists(file_historical_data) == true) {
                string txt_stream;
                try {
                    txt_stream = File.ReadAllText(file_historical_data);
                }
                catch (Exception e) {
                    Console.WriteLine(e.Message);
                    return;
                }

                historical_data = ParseCSV(txt_stream);
            }

            // Check the last day int the current historical data. If there is no data, it takes the last two years
            DateTime last_data = GetLastDataTime(historical_data);

            // Download the historical data from NASDAQ
            List<StockItem> downloaded_data = UrlNasdaq(symbol, last_data);

            // Add downloaded data to the current historical data
            historical_data = AppendHistoricalData(downloaded_data, historical_data);

            // Store updated data in the data base
            SaveStockDataCSV(historical_data, file_historical_data);
        }

        static void Main(string[] args)
        {
            string working_path = "../../../";
            string historical_data_path = string.Concat(working_path, "data/");

            // Look for files in the data folder. If the file is empty, the last two years are downloaded
            string[] list_files;
            try {
                list_files = System.IO.Directory.GetFiles(historical_data_path, "*.csv");
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                return;
            }

            foreach (string filename in list_files) {
                string symbol = System.IO.Path.GetFileNameWithoutExtension(filename);
                symbol = symbol.Trim();
                // Ignore files that start with .
                if (symbol.Length > 0 && symbol[0] != '.') {
                    Console.WriteLine(symbol);
                    UpdateStockHistoricalData(symbol, historical_data_path);
                }
            }

            Console.WriteLine("...Hecho!");
            Console.ReadLine();
            return;
        }
    }
}
