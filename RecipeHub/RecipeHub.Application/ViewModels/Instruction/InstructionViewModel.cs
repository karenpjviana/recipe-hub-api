namespace RecipeHub.Application.ViewModels.Instruction
{
    public class InstructionViewModel
    {
        public Guid Id { get; set; }
        public int StepNumber { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
