using System.Text.RegularExpressions;
using System.Xml.Linq;
namespace ConsoleApp2.data
{
    class Program
    {
        static void Main()
        {
            string logFilePath = @"C:\Users\ialaouik\AppData\Roaming\JetBrains\Rider2024.3\extensions\com.intellij.database\data\api_Carbon.Application.WinForm.Fux (1).log";
            string xmlFilePath = @"C:\Users\ialaouik\RiderProjects\ConsoleApp2\ConsoleApp2\data\CrConfig (1).xml";
            string outputDir = @"../../../data";
            string outputFile = Path.Combine(outputDir, "Muenzbetrag.txt");
            Directory.CreateDirectory(outputDir);
            int? lastMuenzbetrag70019011 = null;
            int? lastMuenzbetrag70019012 = null;
            try
            {
                using (StreamWriter writer = new StreamWriter(outputFile, true))
                {
                    if (File.Exists(logFilePath))
                    {
                       Console.WriteLine("Das aktuelle Verzeichnis: " + Directory.GetCurrentDirectory());
                       Console.WriteLine("Der Pfad " + logFilePath + ": " + File.Exists(logFilePath));

                        string[] content = File.ReadAllLines(logFilePath);
                        foreach (string line in content)
                        {
                            if (line.Contains("TotalValueCoins"))
                            {
                                if (!ExtractWithRegularExpression(line, out var muenzbetrag)) continue;
                                if (line.Contains("70019011"))
                                {
                                    lastMuenzbetrag70019011 = muenzbetrag;
                                }
                                else if (line.Contains("70019012"))
                                {
                                    lastMuenzbetrag70019012 = muenzbetrag; 
                                }
                            }
                        }
                        // Ausgabe der Münzbeträge
                        WriteMuenzbetrag(writer, lastMuenzbetrag70019011, "70019011");
                        WriteMuenzbetrag(writer, lastMuenzbetrag70019012, "70019012");
                    }
                    else
                    {
                        writer.WriteLine("Die Datei wurde nicht gefunden.");
                    }
                    // XML-Datei auslesen
                    if (File.Exists(xmlFilePath))
                    {
                        ReadCashboxesFromXml(writer, xmlFilePath);
                    }
                    else
                    {
                        writer.WriteLine("Die XML-Datei wurde nicht gefunden.");
                    }
                    writer.WriteLine("Münzbetrag der beiden Cashrecycler aktualisiert. ");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler: " + ex.Message);
            }
            Console.WriteLine("Code wird ausgeführt: " + DateTime.Now);
            Console.ReadLine(); 
        }
        private static void WriteMuenzbetrag(StreamWriter writer, int? muenzbetrag, string cashboxId)
        {
            if (muenzbetrag.HasValue)
            {
                writer.WriteLine($"\n=== Block für {cashboxId} ===");
                writer.WriteLine($"Letzter Münzbetrag für {cashboxId}: {muenzbetrag.Value / 100.0:F2}€");
                writer.WriteLine($"\n========================");
            }
            else
            {
                writer.WriteLine($"Keine Zeilen mit 'TotalValueCoins' für {cashboxId} gefunden.");
            }
        }
        private static void ReadCashboxesFromXml(StreamWriter writer, string xmlFilePath)
        {
            XDocument xmlDoc = XDocument.Load(xmlFilePath);
            foreach (var cashbox in xmlDoc.Descendants("cashbox"))
            {
                string name = cashbox.Attribute("name")?.Value;
                string host = cashbox.Attribute("host")?.Value;
                writer.WriteLine($"Cashbox Name: {name}, Host: {host}");
            }
        }
        private static bool ExtractWithRegularExpression(string line, out int muenzbetrag)
        {
            muenzbetrag = 0;
            string pattern = "\"TotalValueCoins\":\\s*(\\d+)";
            Match match = Regex.Match(line, pattern);
            if (!match.Success)
            {
                return false;
            }
            var totalValueString = match.Groups[1].Value;
            var success = int.TryParse(totalValueString, out int result);
            if (!success)
            {
                Console.WriteLine($"Der Wert {totalValueString} konnte nicht geparst werden.");
                return false;
            }
            muenzbetrag = result;
            return true;

        }
    }
}