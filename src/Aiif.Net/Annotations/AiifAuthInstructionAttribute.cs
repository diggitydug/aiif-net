namespace Aiif.Net.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Assembly, AllowMultiple = true, Inherited = true)]
public sealed class AiifAuthInstructionAttribute : Attribute
{
    public AiifAuthInstructionAttribute(string instruction)
    {
        Instruction = instruction;
    }

    public string Instruction { get; }
}
