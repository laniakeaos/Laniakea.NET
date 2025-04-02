namespace Laniakea.Linux;

using System;
using System.IO;

public class Distribution
{
    public string Name { get; }
    public string PrettyName { get; }
    public string Id { get; }
    public string BuildId { get; }

    public Distribution()
    {
        Name = "";
        PrettyName = "";
        Id = "";
        BuildId = "";

        const string filePath = "/etc/os-release";

        StreamReader? reader = null;
        try {
            reader = new StreamReader(filePath);

            string data = reader.ReadToEnd();
            string[] lines = data.Split("\n");
            foreach (string line in lines) {
                if (line == "") {
                    continue;
                }
                string[] keyval = line.Split('=');
                string key = keyval[0];
                string val = keyval[1].Trim('"');

                if (key == "NAME") {
                    Name = val;
                } else if (key == "PRETTY_NAME") {
                    PrettyName = val;
                } else if (key == "ID") {
                    Id = val;
                } else if (key == "BUILD_ID") {
                    BuildId = val;
                }
            }
        } catch (Exception e) {
            var _ = e;
        } finally {
            if (reader != null) {
                reader.Close();
            }
        }
    }
}
