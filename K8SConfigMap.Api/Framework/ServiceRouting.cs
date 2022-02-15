using System.Collections.Generic;

namespace K8SConfigMap.Api.Framework
{
    public class ServiceRouting
    {
        public List<ServiceResolution> Resolutions { get; set; }
    }

    public class ServiceResolution
    {
        public string Function { get; set; }
        public string Service { get; set; }
        public string Path { get; set; }
        public string Methods { get; set; }
    }
}
