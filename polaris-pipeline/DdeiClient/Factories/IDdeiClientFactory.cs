using DdeiClient.Clients.Interfaces;
using DdeiClient.Enums;

namespace DdeiClient.Factories;

public interface IDdeiClientFactory
{
    IDdeiClient Create(string cmsAuthValues, DdeiClients client = DdeiClients.Ddei);
}