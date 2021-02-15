﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nine_Testing
{
    public class VariableSetting : IDisposable
    {
        public static void LaunchSettingsFixture()
        {
            using var file = File.OpenText("Properties\\launchSettings.json");
            var reader = new JsonTextReader(file);
            var jObject = JObject.Load(reader);

            var variables = jObject
                .GetValue("profiles")
                .SelectMany(profiles => profiles.Children())
                .SelectMany(profile => profile.Children<JProperty>())
            .Where(prop => prop.Name == "environmentVariables")
            .SelectMany(prop => prop.Value.Children<JProperty>())
            .ToList();

            foreach (var variable in variables)
            {
                Environment.SetEnvironmentVariable(variable.Name, variable.Value.ToString());
            }
        }

        public void Dispose()
        {
            //clean up
        }
    }
}
