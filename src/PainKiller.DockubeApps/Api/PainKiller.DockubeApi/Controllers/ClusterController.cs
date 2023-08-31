using k8s;
using Microsoft.AspNetCore.Mvc;
using PainKiller.Dockube.Shared.DomainObjects;

namespace PainKiller.DockubeApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClusterController : ControllerBase
    {
        private readonly IKubernetes _kubernetesClient;
        public ClusterController()
        {
            var config = KubernetesClientConfiguration.BuildDefaultConfig();
            _kubernetesClient = new Kubernetes(config);
        }
        public IActionResult Get(string? podName)
        {
            var pods = new List<Pod>();
            var namespaces = _kubernetesClient.CoreV1.ListNamespace();
            foreach (var ns in namespaces.Items) {
                var list = _kubernetesClient.CoreV1.ListNamespacedPod(ns.Metadata.Name);
                foreach (var item in list.Items)
                {
                    pods.Add(new Pod(Name: item.Metadata.Name, Phase: $"{item.Status.Phase}", Kind: item.Kind, Uid: item.Metadata.Uid));
                }
            }
            return Ok(pods);
        }
    }
}