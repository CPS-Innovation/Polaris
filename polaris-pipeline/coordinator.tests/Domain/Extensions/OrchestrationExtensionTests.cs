using System;
using coordinator.Domain.Extensions;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Xunit;

namespace coordinator.tests.Domain.Extensions;

public class OrchestrationExtensionTests
{
    private readonly OrchestrationRuntimeStatus[] _candidateRuntimeStatuses = {OrchestrationRuntimeStatus.Completed, OrchestrationRuntimeStatus.Failed};
    
    [Fact]
    public void When_Runtime_Status_Is_Completed_Should_Return_True()
    {
        var proposedRuntimeStatus = Enum.GetName(OrchestrationRuntimeStatus.Completed);
        
        var testResult = proposedRuntimeStatus.IsClearDownCandidate(_candidateRuntimeStatuses);

        testResult.Should().BeTrue();
    }
    
    [Fact]
    public void When_Runtime_Status_Is_Failed_Should_Return_True()
    {
        var proposedRuntimeStatus = Enum.GetName(OrchestrationRuntimeStatus.Failed);
        
        var testResult = proposedRuntimeStatus.IsClearDownCandidate(_candidateRuntimeStatuses);

        testResult.Should().BeTrue();
    }
    
    [Fact]
    public void When_Runtime_Status_Is_Not_A_Qualifying_Value_Should_Return_False()
    {
        var proposedRuntimeStatus = Enum.GetName(OrchestrationRuntimeStatus.Running);
        
        var testResult = proposedRuntimeStatus.IsClearDownCandidate(_candidateRuntimeStatuses);

        testResult.Should().BeFalse();
    }
}