using System;
using System.Diagnostics.CodeAnalysis;
using k8s;
using LWS_Gateway.Kube.ServiceDeployments;
using LWS_Gateway.Model.Deployment;

namespace LWS_Gateway.Kube;

[ExcludeFromCodeCoverage]
public class ServiceDeploymentProvider
{
    public IServiceDeployment SelectCorrectDeployment(DeploymentType deploymentType, IKubernetes kubernetes) =>
        deploymentType switch
        {
            DeploymentType.Ubuntu => CreateUbuntuDeployment(kubernetes),
            _ => throw new NullReferenceException("Deployment Type was somehow null!")
        };
    
    public IServiceDeployment CreateUbuntuDeployment(IKubernetes kubernetes) => new UbuntuDeployment(kubernetes);
}