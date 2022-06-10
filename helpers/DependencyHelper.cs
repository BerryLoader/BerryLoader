using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;

namespace BerryLoaderNS
{
    public static class DependencyHelper
    {
        /*public static void Test()
        {
            //var a = new Version("0.0.1");
            //var b = new Version("0.0.2");
            //BerryLoader.L.LogInfo(SignToExpectedValues("<").Contains(a.CompareTo(b)));
            var a = JsonConvert.DeserializeObject<ModManifest>("{\"id\": \"A\", \"version\": \"1.0.0\", \"dependencies\": {}}");
            var b = JsonConvert.DeserializeObject<ModManifest>("{\"id\": \"B\", \"version\": \"1.0.0\", \"dependencies\": {\"C\": \">= 1.0.0\"}}");
            var c = JsonConvert.DeserializeObject<ModManifest>("{\"id\": \"C\", \"version\": \"1.0.0\", \"dependencies\": {}}");
            GetValidModLoadOrder(new List<ModManifest>() { a, b, c });
        }*/

        public static List<ModManifest> GetValidModLoadOrder(List<ModManifest> mods)
        {
            var resolved = new List<ModManifest>();
            // a root node is required because uhhhhhhh
            var root = new ModManifest()
            {
                id = "Root",
                dependencies = new Dictionary<string, string>()
            };
            foreach (var mod in mods)
            {
                root.dependencies.Add(mod.id, $"= {mod.version}");
            }
            Resolve(mods, root, resolved, new List<ModManifest>());
            return resolved.Take(resolved.Count - 1).ToList(); // remove the root node
        }

        // https://www.electricmonk.nl/docs/dependency_resolving_algorithm/dependency_resolving_algorithm.html
        private static void Resolve(List<ModManifest> mods, ModManifest node, List<ModManifest> resolved, List<ModManifest> unresolved)
        {
            unresolved.Add(node);
            foreach (var edge in node.dependencies)
            {
                if (resolved.Find(x => x.id == edge.Key) == null)
                {
                    if (unresolved.Find(x => x.id == edge.Key) != null)
                        throw new Exception($"CIRCULAR DEP {node.id}->{edge.Key}");
                    var lookingFor = mods.Find(x => x.id == edge.Key);
                    if (lookingFor == null)
                        throw new Exception($"COULD NOT FIND {edge.Key}");
                    BerryLoader.L.LogInfo($"Looking for {edge.Key} {edge.Value}, found {lookingFor.id} {lookingFor.version}");
                    if (!SignToExpectedValues(edge.Value.Split(' ')[0]).Contains(new Version(edge.Value.Split(' ')[1]).CompareTo(new Version(lookingFor.version))))
                        throw new Exception($"BAD VERSION! EXPECTED {edge.Key} {edge.Value}, FOUND {lookingFor.id} {lookingFor.version}");
                    Resolve(mods, lookingFor, resolved, unresolved);
                }
            }
            resolved.Add(node);
            unresolved.Remove(node);
        }

        public static List<int> SignToExpectedValues(string sign)
        {
            switch (sign)
            {
                // these nummies are so confusing :(
                case ">=": return new List<int>() { -1, 0 };
                case ">": return new List<int>() { -1 };
                case "<=": return new List<int>() { 0, 1 };
                case "<": return new List<int>() { 1 };
                default: return new List<int>() { 0 };
            }
        }
    }
}