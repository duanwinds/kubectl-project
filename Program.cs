using Newtonsoft.Json;
using YamlDotNet.Serialization;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace kubectl_project;

class Program
{
    static void Main(string[] args)
    {
        K8sConfig config = K8sConfig.Load();
        // config.Contexts.ForEach(ct => Console.WriteLine(ct.Config.UserName));
        
        if ( args.Length > 0 )
        {
            string ns = $"{args.First()}";
            Console.WriteLine(K8sConfig.Save(ns));
        } else {
           foreach(Context ct in config.Contexts)
           {
               if(ct.Name == config.CurrentContextName)
               {
                    if ( ct.Config.Namespace != null )
                    {
                        Console.WriteLine("now is {0} namespace", $"{ct.Config.Namespace}");
                    } else {
                        Console.Write("now is default namespace");
                    }
               }
           }
        }
    }

    public class K8sConfig
    {
        [YamlMember(Alias = "apiVersion")]
        public string ApiVersion { get; set; } = "v1";

        [YamlMember(Alias = "kind")]
        public string Kind { get; set; } = "Configuration";

        [YamlMember(Alias = "current-context")]
        public string? CurrentContextName { get; set; }

        [YamlMember(Alias = "contexts")]
        public List<Context> Contexts { get; set; } = new List<Context>();

        [YamlMember(Alias = "clusters")]
        public List<Cluster> Clusters { get; set; } = new List<Cluster>();

        public static K8sConfig Load(string configFile) => Load(new FileInfo(configFile));

        public static K8sConfig Load()
        {
            // Console.Write(Locate());
            
            return Load(configFile: Locate());
        }

        public static K8sConfig Load(FileInfo configFile)
        {
            if (configFile == null)
                throw new ArgumentNullException(nameof(configFile));

            IDeserializer deserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();

            using (StreamReader configReader = configFile.OpenText())
            {
                return deserializer.Deserialize<K8sConfig>(configReader);
            }
        }

        public static string Save(string ns)
        {
            // var serializer = new SerializerBuilder()
            //     .Build();
            // string yaml = serializer.Serialize(config);
            Process process = new Process();
            process.StartInfo.FileName = "kubectl";
            process.StartInfo.Arguments = "config set-context --current --namespace " + ns;

            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output;
        }

        public static string Locate()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var home = Environment.GetEnvironmentVariable("HOME");
                if (!String.IsNullOrEmpty(home))
                    return Path.Combine(home, ".kube", "config");

                var homeDrive = Environment.GetEnvironmentVariable("HOMEDRIVE");
                var homePath = Environment.GetEnvironmentVariable("HOMEPATH");
                if (!String.IsNullOrEmpty(homeDrive) && !String.IsNullOrEmpty(homePath))
                    return Path.Combine(homeDrive + homePath, ".kube", "config");

                var userProfile = Environment.GetEnvironmentVariable("USERPROFILE");
                if (!String.IsNullOrEmpty(userProfile))
                    return Path.Combine(userProfile, ".kube", "config");
            }

            // return Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".kube", "config");
            var myhome = Environment.GetEnvironmentVariable("HOME");
            return Path.Combine(myhome is null ? "" : myhome, ".kube", "config");
        }
    }
    public class ContextConfig
    {
        [YamlMember(Alias = "namespace", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public string? Namespace { get; set; }

        [YamlMember(Alias = "cluster")]
        public required string ClusterName { get; set; }

        [YamlMember(Alias = "user")]
        public required string UserName { get; set; }
    }

    public class Context
    {
        [YamlMember(Alias = "name")]
        public required string Name { get; set; }

        [YamlMember(Alias = "context")]
        public ContextConfig Config { get; set; } = new ContextConfig{ ClusterName = "", UserName = "" };
    }

    public class ClusterConfig
    {
        [YamlMember(Alias = "server")]
        public required string Server { get; set; }
    }

    public class Cluster
    {
        [YamlMember(Alias = "name")]
        public required string Name { get; set; }

        [YamlMember(Alias = "cluster")]
        public required ClusterConfig Config { get; set; }
    }
}