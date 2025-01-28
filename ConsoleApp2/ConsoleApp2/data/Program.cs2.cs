using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ConsoleApp2.data
{
    class Program
    {
        static void Main()
        {
            string logFilePath = @"C:\Users\ialaouik\AppData\Roaming\JetBrains\Rider2024.3\extensions\com.intellij.database\data\api_Carbon.Application.WinForm.Fux (1).log";
            string xmlFilePath = "ConsoleApp2/data/CrConfig (1).xml";
            string outputDir = @"../../../data";
            string outputFile = Path.Combine(outputDir, "Muenzbetrag.txt");

            Directory.CreateDirectory(outputDir);

            int? lastMuenzbetrag1 = null;
            int? lastMuenzbetrag2 = null;
            string modifiedCashboxId1 = null;
            string modifiedCashboxId2 = null;

            try
            {
                using (StreamWriter writer = new StreamWriter(outputFile, true))
                {
                    // Überprüfe, ob die Logdatei existiert
                    if (File.Exists(logFilePath))
                    {
                        writer.WriteLine("Das aktuelle Verzeichnis: " + Directory.GetCurrentDirectory());
                        writer.WriteLine("Der Pfad " + logFilePath + ": " + File.Exists(logFilePath));

                        string[] content = File.ReadAllLines(logFilePath);

                        // Verarbeite die Logdatei
                        foreach (string line in content)
                        {
                            if (line.Contains("TotalValueCoins"))
                            {
                                if (!ExtractWithRegularExpression(line, out var muenzbetrag)) continue;

                                // Verwende die modifizierten Cashbox-IDs
                                if (line.Contains(modifiedCashboxId1))
                                {
                                    lastMuenzbetrag1 = muenzbetrag;
                                }
                                else if (line.Contains(modifiedCashboxId2))
                                {
                                    lastMuenzbetrag2 = muenzbetrag; 
                                }
                            }
                        }

                        // Schreibe die Münzbeträge in die Ausgabedatei
                        WriteMuenzbetrag(writer, lastMuenzbetrag1, modifiedCashboxId1);
                        WriteMuenzbetrag(writer, lastMuenzbetrag2, modifiedCashboxId2);
                    }
                    else
                    {
                        writer.WriteLine("Die Logdatei wurde nicht gefunden.");
                    }

                    // Überprüfe, ob die XML-Datei existiert und lese die Cashboxen
                    if (File.Exists(xmlFilePath))
                    {
                        ReadCashboxesFromXml(writer, xmlFilePath, ref modifiedCashboxId1, ref modifiedCashboxId2);
                    }
                    else
                    {
                        writer.WriteLine("Die XML-Datei wurde nicht gefunden.");
                    }

                    writer.WriteLine("Der Durchlauf vom Script wurde erfolgreich durchgeführt und somit wurde der Münzbestand auf den neuesten Stand aktualisiert.");
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
            }
            else
            {
                writer.WriteLine($"Keine Zeilen mit 'TotalValueCoins' für {cashboxId} gefunden.");
            }
        }

        private static void ReadCashboxesFromXml(StreamWriter writer, string xmlFilePath, ref string modifiedCashboxId1, ref string modifiedCashboxId2)
        {
            XDocument xmlDoc = XDocument.Load(xmlFilePath);
            foreach (var cashbox in xmlDoc.Descendants("cashbox"))
            {
                string? name = cashbox.Attribute("name")?.Value;
                string? host = cashbox.Attribute("host")?.Value;

                // Überprüfe, ob name null ist und ob es mindestens 6 Zeichen hat
                if (!string.IsNullOrEmpty(name) && name.Length >= 6)
                {
                    string modifiedName = Insert01InCashboxId(name);
                    writer.WriteLine($"Cashbox Name: {modifiedName}, Host: {host}");

                    // Speichere die modifizierten Cashbox-IDs
                    if (name == "70019011")
                    {
                        modifiedCashboxId1 = modifiedName;
                    }
                    else if (name == "70019012")
                    {
                        modifiedCashboxId2 = modifiedName;
                    }
                }
                else
                {
                    writer.WriteLine("Warnung: Cashbox Name ist null oder zu kurz.");
                }
            }
        }

        private static string Insert01InCashboxId(string cashboxId)
        {
            // Füge "01" zwischen dem zweiten und dritten Zeichen ein
            return cashboxId.Substring(0, 2) + "01" + cashboxId.Substring(2);
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

