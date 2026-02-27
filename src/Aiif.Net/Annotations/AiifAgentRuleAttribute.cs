namespace Aiif.Net.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Assembly, AllowMultiple = true, Inherited = true)]
public sealed class AiifAgentRuleAttribute : Attribute
{
    public AiifAgentRuleAttribute(string rule)
    {
        Rule = rule;
    }

    public string Rule { get; }
}
